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
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly DBContext _context;
    private readonly IEmailService _emailService;

    public AuthController(DBContext context,IAuthService authService, ITokenService tokenService, IEmailService emailService)
    {
        _authService = authService;
        _tokenService = tokenService;
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var user = await _authService.LoginAsync(request);
            var token = await _tokenService.CreateToken(user);

            var response = new
            {
                Token = token,
                UserId = user.Id,
                Role = user.Role,
                FullName = user.FullName
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] RegisterRequest request)
    {
        // 1. Check if passwords match
        if (request.Password != request.ConfirmPassword)
            return BadRequest("كلمة المرور وتأكيدها غير متطابقتين.");

        // 2. Validate input
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Password))
            return BadRequest("الإيميل والاسم الكامل وكلمة المرور مطلوبة.");

        // 3. Check if email already exists
        if (await _authService.LoginAsync(new LoginRequest { Email = request.Email, Password = request.Password }) != null)
            return BadRequest("الإيميل مستخدم بالفعل.");

        // 4. Validate Role
        var validRoles = new[] { "Doctor", "Consultant", "Patient" };
        if (string.IsNullOrEmpty(request.Role) || !validRoles.Contains(request.Role))
            return BadRequest("الدور غير صحيح. يرجى اختيار Doctor أو Consultant أو Patient.");

        // 5. Generate random verification code (6 digits)
        var verificationCode = new Random().Next(100000, 999999).ToString();

        // 6. Save verification code
        var resetCode = new PasswordResetCode
        {
            Email = request.Email,
            Code = verificationCode,
            Expiration = DateTime.UtcNow.AddMinutes(10),
            FullName = request.FullName,
            Role = request.Role,
            Password = request.Password
        };
        _context.PasswordResetCodes.Add(resetCode);
        await _context.SaveChangesAsync();

        // 7. Send verification code via email
        try
        {
            await _emailService.SendVerificationEmail(request.Email, verificationCode);
        }
        catch (Exception)
        {
            return StatusCode(500, "فشل في إرسال البريد الإلكتروني.");
        }

        // 8. Return response
        return Ok(new { Message = "تم إرسال الكود إلى بريدك الإلكتروني." });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            var user = await _authService.VerifyEmailAsync(request);
            var token = await _tokenService.CreateToken(user);

            return Ok(new
            {
                Message = "تم إنشاء الحساب بنجاح.",
                Token = token,
                Name = user.FullName,
                Role = user.Role,
                RequiresAdditionalInfo = user.Role == "Doctor",
                UserId = user.Id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        try
        {
            await _authService.ForgetPasswordAsync(request);
            return Ok("تم إرسال الكود إلى بريدك الإلكتروني.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        try
        {
            await _authService.VerifyCodeAsync(request);
            return Ok("الكود صحيح، يمكنك الآن إعادة تعيين كلمة المرور.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("resend-verification-code")]
    public async Task<IActionResult> ResendVerificationCode([FromBody] ForgetPasswordRequest request)
    {
        try
        {
            await _authService.ResendVerificationCodeAsync(request);
            return Ok("تم إرسال كود جديد إلى بريدك الإلكتروني.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);
            return Ok("تم تغيير كلمة المرور بنجاح.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
