namespace Domain.Dtos.Auth;

public class RefreshTokenDto
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}