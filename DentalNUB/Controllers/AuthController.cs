using DentalNUB.Entities;
using DentalNUB.Entities.Models;
using DentalNUB.Interface;
using DentalNUB.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DentalNUB.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DBContext _context;
    private readonly IConfiguration _config;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        ITokenService tokenService,
        DBContext context,
        IConfiguration config,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _config = config;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return Unauthorized("الإيميل أو الباسورد غلط");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            return Unauthorized("الإيميل أو الباسورد غلط");

        var token = await _tokenService.CreateToken(user); // تأكد إن CreateToken بيرجع String فقط

        var response = new
        {
            Token = token,
            UserId = user.UserId,
            Role = user.Role,
            FullName = user.FullName
        };

        return Ok(response);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] RegisterRequest request)
    {
        // 1. Check if passwords match
        if (request.Password != request.ConfirmPassword)
            return BadRequest("كلمة المرور وتأكيدها غير متطابقتين.");

        // 2. Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("الإيميل مستخدم بالفعل.");

        // 3. Generate random verification code (6 digits)
        var verificationCode = new Random().Next(100000, 999999).ToString();

        // 4. Save verification code and user data to PasswordResetCodes table
        var resetCode = new PasswordResetCode
        {
            Email = request.Email,
            Code = verificationCode,
            Expiration = DateTime.UtcNow.AddMinutes(10),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // حفظ الباسورد مشفر
            FullName = request.FullName,
            Role = request.Role
        };
        _context.PasswordResetCodes.Add(resetCode);
        await _context.SaveChangesAsync();

        // 5. Send verification code via email
        try
        {
            await SendVerificationEmail(request.Email, verificationCode);
        }
        catch (Exception)
        {
            return StatusCode(500, "فشل في إرسال البريد الإلكتروني.");
        }

        // 6. Return response
        return Ok(new { Message = "تم إرسال الكود إلى بريدك الإلكتروني." });
    }
    private async Task SendVerificationEmail(string email, string code)
    {
        var mailSettings = _config.GetSection("MailSettings").Get<MailSettings>();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Your Verification Code";
        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code is: {code}. It expires in 10 minutes."
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(mailSettings.Mail, mailSettings.Password); // الـ App Password هنا
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        // 1. Find the reset code using the provided code
        var resetCode = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(v => v.Code == request.Code && v.Expiration > DateTime.UtcNow);

        if (resetCode == null)
            return BadRequest("الكود غير صحيح أو منتهي الصلاحية.");

        // 2. Re-validate email (just in case)
        if (await _context.Users.AnyAsync(u => u.Email == resetCode.Email))
            return BadRequest("الإيميل مستخدم بالفعل.");

        // 3. Create user entity from stored data
        var user = new User
        {
            Email = resetCode.Email,
            PasswordHash = resetCode.PasswordHash,
            FullName = resetCode.FullName,
            Role = resetCode.Role
        };

        // 4. Add user to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync(); // لازم أحفظ هنا علشان ياخد UserId

        // 5. Create specific role entity (بس لـ Consultant و Patient، مش لـ Doctor)
        switch (user.Role)
        {
            case "Consultant":
                if (await _context.Consultants.AnyAsync(c => c.UserId == user.UserId))
                    return BadRequest("المستشار موجود بالفعل لهذا المستخدم.");

                var consultant = new Consultant
                {
                    UserId = user.UserId,
                    ConsName = user.FullName,
                    ConsEmail = user.Email,
                    ConsPassword = user.PasswordHash
                };
                _context.Consultants.Add(consultant);
                await _context.SaveChangesAsync();
                break;

            case "Patient":
                if (await _context.Patients.AnyAsync(p => p.UserId == user.UserId))
                    return BadRequest("المريض موجود بالفعل لهذا المستخدم.");

                var patient = new Patient
                {
                    UserId = user.UserId,
                    PatientName = user.FullName
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                break;

            case "Doctor":
                // ما نضيفش الدكتور هنا، هنستنى لما يكمل بياناته في complete-profile
                break;
        }

        // 6. Generate JWT Token
        var token = await _tokenService.CreateToken(user);

        // 7. Delete used verification code
        _context.PasswordResetCodes.Remove(resetCode);
        await _context.SaveChangesAsync();

        // 8. Return response
        return Ok(new
        {
            Message = "تم إنشاء الحساب بنجاح.",
            Token = token,
            Name = user.FullName,
            Role = user.Role,
            RequiresAdditionalInfo = user.Role == "Doctor", 
            UserId = user.UserId
        });
    }


    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        // 1. Check if email exists
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound("البريد الإلكتروني غير مسجل.");

        // 2. Generate random verification code (6 digits)
        var verificationCode = new Random().Next(100000, 999999).ToString();

        // 3. Save verification code to PasswordResetCodes table
        var resetCode = new PasswordResetCode
        {
            Email = request.Email,
            Code = verificationCode,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };
        _context.PasswordResetCodes.Add(resetCode);
        await _context.SaveChangesAsync();

        // 4. Send verification code via email
        try
        {
            await SendVerificationEmail(request.Email, verificationCode);
        }
        catch (Exception)
        {
            return StatusCode(500, "فشل في إرسال البريد الإلكتروني.");
        }

        return Ok("تم إرسال الكود إلى بريدك الإلكتروني.");
    }


    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        // 1. Check if code is valid
        var codeEntry = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(c => c.Email == request.Email && c.Code == request.Code);

        if (codeEntry == null)
            return NotFound("الكود غير صحيح أو الإيميل غير صحيح.");

        if (codeEntry.Expiration < DateTime.UtcNow)
            return BadRequest("انتهت صلاحية الكود. برجاء طلب كود جديد.");

        return Ok("الكود صحيح، يمكنك الآن إعادة تعيين كلمة المرور.");
    }


    [HttpPost("resend-verification-code")]
    public async Task<IActionResult> ResendVerificationCode([FromBody] ForgetPasswordRequest request)
    {
        // 1. Check if email exists
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound("البريد الإلكتروني غير مسجل.");

        // 2. Delete any existing code for this email
        var existingCode = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(c => c.Email == request.Email);
        if (existingCode != null)
        {
            _context.PasswordResetCodes.Remove(existingCode);
            await _context.SaveChangesAsync();
        }

        // 3. Generate new verification code (6 digits)
        var verificationCode = new Random().Next(100000, 999999).ToString();

        // 4. Save new verification code
        var resetCode = new PasswordResetCode
        {
            Email = request.Email,
            Code = verificationCode,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };
        _context.PasswordResetCodes.Add(resetCode);
        await _context.SaveChangesAsync();

        // 5. Send verification code via email
        try
        {
            await SendVerificationEmail(request.Email, verificationCode);
        }
        catch (Exception)
        {
            return StatusCode(500, "فشل في إرسال البريد الإلكتروني.");
        }

        return Ok("تم إرسال كود جديد إلى بريدك الإلكتروني.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        // 1. Check if passwords match
        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest("كلمة المرور الجديدة وتأكيدها غير متطابقتين.");

        // 2. Check if code is valid
        var codeEntry = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(c => c.Email == request.Email && c.Code == request.Code);

        if (codeEntry == null)
            return NotFound("الكود غير صحيح أو الإيميل غير صحيح.");

        if (codeEntry.Expiration < DateTime.UtcNow)
            return BadRequest("انتهت صلاحية الكود. برجاء طلب كود جديد.");

        // 3. Find the user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound("المستخدم غير موجود.");

        // 4. Update the password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // 5. Delete the used code
        _context.PasswordResetCodes.Remove(codeEntry);
        await _context.SaveChangesAsync();

        return Ok("تم تغيير كلمة المرور بنجاح.");
    }
}
