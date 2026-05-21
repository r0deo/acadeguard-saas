namespace AcademicCompliance.Application.Interfaces.Auth;

public interface IAuthRequestValidator
{
    void Validate<TRequest>(TRequest request);
}
