using DentalNUB.Entities;
using DentalNUB.Entities.Models;
using DentalNUB.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DentalNUB.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    


    public AuthController(IAuthService authService, ITokenService tokenService )
    {
        _authService = authService;
        _tokenService = tokenService;
       
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
        try
        {
            await _authService.SignUpAsync(request);
            return Ok(new { Message = "تم إرسال الكود إلى بريدك الإلكتروني." });
        }
        catch (Exception ex)
        {
            if (ex.Message == "فشل في إرسال البريد الإلكتروني.")
                return StatusCode(500, ex.Message);
            return BadRequest(ex.Message);
        }
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
