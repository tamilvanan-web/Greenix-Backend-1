using System.Threading.Tasks;
using Greenix.Shared.Models;
using Greenix.Application.DTOs.Auth;

namespace Greenix.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<BaseResponse<AuthResponse>> RegisterCitizenAsync(RegisterRequest request);
        Task<BaseResponse<AuthResponse>> LoginAsync(LoginRequest request);
        Task<BaseResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<BaseResponse<string>> VerifyEmailAsync(VerifyEmailRequest request);
        Task<BaseResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
