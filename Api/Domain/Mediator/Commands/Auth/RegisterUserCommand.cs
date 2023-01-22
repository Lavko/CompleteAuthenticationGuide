using Domain.Dtos.Auth;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Domain.Mediator.Commands.Auth;

public class RegisterUserCommand : IRequest<RegisterResponseDto>
{
    public RegisterUserCommand(RegisterRequest request)
    {
        Request = request;
    }

    public RegisterRequest Request { get; set; }

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponseDto>
    {
        private readonly UserManager<User> _userManager;

        public RegisterUserCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<RegisterResponseDto> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var userByEmail = await _userManager.FindByEmailAsync(command.Request.Email!);
            var userByUsername = await _userManager.FindByNameAsync(command.Request.Username!);
        
            if (userByEmail is not null || userByUsername is not null)
            {
                throw new Exception($"User with email {command.Request.Email} or username {command.Request.Username} already exists.");
            }

            User user = new()
            {
                Email = command.Request.Email,
                UserName = command.Request.Username,
                Provider = Consts.LoginProviders.Password,
            };

            var result = await _userManager.CreateAsync(user, command.Request.Password!);
        
            await _userManager.AddToRoleAsync(user, Role.User);

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Unable to register user {command.Request.Username}, errors: {GetErrorsText(result.Errors)}");
            }

            return new RegisterResponseDto();
        }
        
        private static string GetErrorsText(IEnumerable<IdentityError> errors)
        {
            return string.Join(", ", errors.Select(error => error.Description).ToArray());
        }
    }
}