using System.Security.Claims;
using Greenix.Domain.Entities;

namespace Greenix.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, string role);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
