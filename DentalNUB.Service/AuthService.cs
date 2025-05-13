using DentalNUB.Entities.Models;
using DentalNUB.Entities;
using DentalNUB.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalNUB.Service;
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly DBContext _context;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<User> userManager, DBContext context, IEmailService emailService, ITokenService tokenService)
    {
        _userManager = userManager;
        _context = context;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task SignUpAsync(RegisterRequest request)
    {
        // 1. Check if passwords match
        if (request.Password != request.ConfirmPassword)
            throw new Exception("كلمة المرور وتأكيدها غير متطابقتين.");

        // 2. Validate input
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Password))
            throw new Exception("الإيميل والاسم الكامل وكلمة المرور مطلوبة.");

        // 3. Check if email already exists
        try
        {
            var existingUser = await LoginAsync(new LoginRequest { Email = request.Email, Password = request.Password });
            if (existingUser != null)
                throw new Exception("الإيميل مستخدم بالفعل.");
        }
        catch (Exception)
        {
            // Ignore exception, means user doesn't exist, which is what we want
        }

        // 4. Validate Role
        var validRoles = new[] { "Doctor", "Consultant", "Patient" };
        if (string.IsNullOrEmpty(request.Role) || !validRoles.Contains(request.Role))
            throw new Exception("الدور غير صحيح. يرجى اختيار Doctor أو Consultant أو Patient.");

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
        await _emailService.SendVerificationEmail(request.Email, verificationCode);
    }
    public async Task<User> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new Exception("الإيميل أو الباسورد غلط");

        return user;
    }

    public async Task VerifyCodeAsync(VerifyCodeRequest request)
    {
        var codeEntry = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(c => c.Email == request.Email && c.Code == request.Code);

        if (codeEntry == null)
            throw new Exception("الكود غير صحيح أو الإيميل غير صحيح.");

        if (codeEntry.Expiration < DateTime.UtcNow)
            throw new Exception("انتهت صلاحية الكود. برجاء طلب كود جديد.");
    }

    public async Task<User> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var resetCode = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(v => v.Code == request.Code && v.Expiration > DateTime.UtcNow);

        if (resetCode == null)
            throw new Exception("الكود غير صحيح أو منتهي الصلاحية.");

        if (await _userManager.FindByEmailAsync(resetCode.Email) != null)
            throw new Exception("الإيميل مستخدم بالفعل.");

        var user = new User
        {
            Email = resetCode.Email,
            UserName = resetCode.Email,
            FullName = resetCode.FullName,
            Role = resetCode.Role
        };

        var result = await _userManager.CreateAsync(user, resetCode.Password);
        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        switch (user.Role)
        {
            case "Consultant":
                if (await _context.Consultants.AnyAsync(c => c.UserId == user.Id))
                    throw new Exception("المستشار موجود بالفعل لهذا المستخدم.");

                var consultant = new Consultant
                {
                    UserId = user.Id,
                    ConsName = user.FullName,
                    ConsEmail = user.Email
                };
                _context.Consultants.Add(consultant);
                await _context.SaveChangesAsync();
                break;

            case "Patient":
                if (await _context.Patients.AnyAsync(p => p.UserId == user.Id))
                    throw new Exception("المريض موجود بالفعل لهذا المستخدم.");

                var patient = new Patient
                {
                    UserId = user.Id,
                    PatientName = user.FullName
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                break;
        }

        _context.PasswordResetCodes.Remove(resetCode);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> ForgetPasswordAsync(ForgetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new Exception("البريد الإلكتروني غير مسجل.");

        var verificationCode = new Random().Next(100000, 999999).ToString();
        var resetCode = new PasswordResetCode
        {
            Email = request.Email,
            Code = verificationCode,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };
        _context.PasswordResetCodes.Add(resetCode);
        await _context.SaveChangesAsync();

        await _emailService.SendVerificationEmail(request.Email, verificationCode);
        return user;
    }

    public async Task<User> ResendVerificationCodeAsync(ForgetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new Exception("البريد الإلكتروني غير مسجل.");

        var existingCode = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(c => c.Email == request.Email);
        if (existingCode != null)
        {
            _context.PasswordResetCodes.Remove(existingCode);
            await _context.SaveChangesAsync();
        }

        var verificationCode = new Random().Next(100000, 999999).ToString();
        var resetCode = new PasswordResetCode
        {
            Email = request.Email,
            Code = verificationCode,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };
        _context.PasswordResetCodes.Add(resetCode);
        await _context.SaveChangesAsync();

        await _emailService.SendVerificationEmail(request.Email, verificationCode);
        return user;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var codeEntry = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(c => c.Email == request.Email && c.Code == request.Code);

        if (codeEntry == null)
            throw new Exception("الكود غير صحيح أو الإيميل غير صحيح.");

        if (codeEntry.Expiration < DateTime.UtcNow)
            throw new Exception("انتهت صلاحية الكود. برجاء طلب كود جديد.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new Exception("المستخدم غير موجود.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        _context.PasswordResetCodes.Remove(codeEntry);
        await _context.SaveChangesAsync();
    }
}

