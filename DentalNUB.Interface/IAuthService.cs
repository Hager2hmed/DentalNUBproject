using DentalNUB.Entities.Models;
using DentalNUB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalNUB.Interface;
public interface IAuthService
{
    Task SignUpAsync(RegisterRequest request);
    Task<User> LoginAsync(LoginRequest request);
    Task VerifyCodeAsync(VerifyCodeRequest request);
    Task<User> VerifyEmailAsync(VerifyEmailRequest request);
    Task<User> ForgetPasswordAsync(ForgetPasswordRequest request);
    Task<User> ResendVerificationCodeAsync(ForgetPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
