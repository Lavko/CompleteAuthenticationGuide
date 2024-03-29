using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos.Auth;

public class SocialLoginRequest
{
    [MinLength(Consts.UsernameMinLength, ErrorMessage = Consts.UsernameLengthValidationError)]
    public string? Email { get; set; }

    [Required] public string? Provider { get; set; }

    [Required] public string? AccessToken { get; set; }
}