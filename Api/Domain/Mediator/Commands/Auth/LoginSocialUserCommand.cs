using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Domain.ConfigurationModels;
using Domain.Dtos.Auth;
using Domain.Entities;
using Domain.Services;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Domain.Mediator.Commands.Auth;

public class LoginSocialUserCommand : IRequest<LoginResponseDto>
{
    public LoginSocialUserCommand(SocialLoginRequest request)
    {
        Request = request;
    }

    public SocialLoginRequest Request { get; set; }
    
    public class LoginSocialUserCommandHandler : IRequestHandler<LoginSocialUserCommand, LoginResponseDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly SocialLoginConfiguration _socialLoginConfiguration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthService _authService;

        public LoginSocialUserCommandHandler(
            UserManager<User> userManager,
            IOptions<SocialLoginConfiguration> socialLoginConfig,
            IHttpClientFactory httpClientFactory,
            IAuthService authService)
        {
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
            _authService = authService;
            _socialLoginConfiguration = socialLoginConfig.Value;
        }

        public async Task<LoginResponseDto> Handle(LoginSocialUserCommand command, CancellationToken cancellationToken)
        {
            await ValidateSocialToken(command.Request);

            var user = await _userManager.FindByEmailAsync(command.Request.Email!) ?? await RegisterSocialUser(command.Request);
        
            if (user.Provider != command.Request.Provider)
            {
                throw new Exception(
                    $"User was registered via {user.Provider} and cannot be logged via {command.Request.Provider}.");
            }

            var token = await _authService.CreateToken(user);

            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = _authService.GenerateRefreshToken()
            };
        }
        
        private async Task ValidateSocialToken(SocialLoginRequest request)
        {
            var _ = request.Provider switch
            {
                Consts.LoginProviders.Facebook => await ValidateFacebookToken(request),
                Consts.LoginProviders.Google => await ValidateGoogleToken(request),
                _ => throw new Exception($"{request.Provider} provider is not supported.")
            };
        }

        private async Task<bool> ValidateFacebookToken(SocialLoginRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var appAccessTokenResponse = await httpClient.GetFromJsonAsync<FacebookAppAccessTokenResponse>($"https://graph.facebook.com/oauth/access_token?client_id={_socialLoginConfiguration.Facebook!.ClientId!}&client_secret={_socialLoginConfiguration.Facebook!.ClientSecret!}&grant_type=client_credentials");
            var response =
                await httpClient.GetFromJsonAsync<FacebookTokenValidationResult>(
                    $"https://graph.facebook.com/debug_token?input_token={request.AccessToken}&access_token={appAccessTokenResponse!.AccessToken}");

            if (response is null || !response.Data.IsValid)
            {
                throw new Exception($"{request.Provider} access token is not valid.");
            }
        
            return true;
        }

        private async Task<bool> ValidateGoogleToken(SocialLoginRequest request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _socialLoginConfiguration.Google!.TokenAudience! }
                };
                await GoogleJsonWebSignature.ValidateAsync(request.AccessToken, settings);
                
            }
            catch (InvalidJwtException)
            {
                throw new Exception($"{request.Provider} access token is not valid.");
            }
        
            return true;
        }

        private async Task<User> RegisterSocialUser(SocialLoginRequest request)
        {
            var user = new User()
            {
                Email = request.Email,
                UserName = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                Provider = request.Provider!
            };
                
            var result = await _userManager.CreateAsync(user, $"Pass!1{Guid.NewGuid().ToString()}");
            
            if(!result.Succeeded)
            {
                throw new Exception($"Unable to register user {request.Email}, errors: {GetErrorsText(result.Errors)}");
            }

            await _userManager.AddToRoleAsync(user, Role.User);
        
            return user;
        }
        
        private static string GetErrorsText(IEnumerable<IdentityError> errors)
        {
            return string.Join(", ", errors.Select(error => error.Description).ToArray());
        }
    }
}