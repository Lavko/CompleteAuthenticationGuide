using System.IdentityModel.Tokens.Jwt;
using Domain.Dtos.Auth;
using Domain.Entities;
using Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Domain.Mediator.Commands.Auth;

public class LoginUserCommand : IRequest<LoginResponseDto>
{
    public LoginUserCommand(LoginRequest request)
    {
        Request = request;
    }

    public LoginRequest Request { get; set; }

    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponseDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;

        public LoginUserCommandHandler(
            UserManager<User> userManager,
            IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<LoginResponseDto> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(command.Request.Username!) ?? await _userManager.FindByEmailAsync(command.Request.Username!);

            if (user is null || !await _userManager.CheckPasswordAsync(user, command.Request.Password!))
            {
                throw new Exception($"Unable to authenticate user {command.Request.Username}");
            }
        
            if (user.Provider != Consts.LoginProviders.Password)
            {
                throw new Exception($"User was registered via {user.Provider} and cannot be logged via {Consts.LoginProviders.Password}.");
            }

            var token = await _authService.CreateToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTimeOffset.Now.AddDays(14);

            await _userManager.UpdateAsync(user);

            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken
            };
        }
    }
}