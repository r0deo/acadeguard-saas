namespace AcademicCompliance.Application.DTOs.Auth;

public sealed class LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime Expiration { get; init; }

    public string Role { get; init; } = string.Empty;

    public AuthenticatedUserDto User { get; init; } = new();
}
