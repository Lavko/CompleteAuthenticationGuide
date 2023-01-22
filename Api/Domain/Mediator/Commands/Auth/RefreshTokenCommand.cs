using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.ConfigurationModels;
using Domain.Dtos.Auth;
using Domain.Entities;
using Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Domain.Mediator.Commands.Auth;

public class RefreshTokenCommand : IRequest<RefreshTokenDto>
{
    public RefreshTokenCommand(RefreshTokenDto command)
    {
        Command = command;
    }

    public RefreshTokenDto Command { get; set; }
    
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtConfiguration _jwtConfiguration;
        private readonly IAuthService _authService;

        public RefreshTokenCommandHandler(
            UserManager<User> userManager,
            IOptions<JwtConfiguration> jwtConfiguration,
            IAuthService authService)
        {
            _userManager = userManager;
            _jwtConfiguration = jwtConfiguration.Value;
            _authService = authService;
        }

        public async Task<RefreshTokenDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var accessToken = request.Command.AccessToken;
            var refreshToken = request.Command.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                throw new Exception("Invalid access token or refresh token");
            }

            var username = principal.Identity!.Name!;

            var user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                throw new Exception("Invalid access token or refresh token");
            }

            var newAccessToken = await _authService.CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new RefreshTokenDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken
            };
        }
        
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Secret!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}