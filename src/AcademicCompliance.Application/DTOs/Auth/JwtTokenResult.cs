namespace AcademicCompliance.Application.DTOs.Auth;

public sealed class JwtTokenResult
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime Expiration { get; init; }
}
