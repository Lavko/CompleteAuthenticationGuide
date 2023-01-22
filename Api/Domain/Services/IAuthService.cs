using System.IdentityModel.Tokens.Jwt;
using Domain.Entities;

namespace Domain.Services;

public interface IAuthService
{
    Task<JwtSecurityToken> CreateToken(User user);
    string GenerateRefreshToken();
}