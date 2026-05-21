using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.Interfaces.Auth;

namespace AcademicCompliance.Application.Validators.Auth;

public sealed class DataAnnotationsAuthRequestValidator : IAuthRequestValidator
{
    public void Validate<TRequest>(TRequest request)
    {
        if (request is null)
        {
            throw new ValidationException("Request body is required.");
        }

        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        if (Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
        {
            return;
        }

        var message = string.Join(" ", validationResults.Select(result => result.ErrorMessage));
        throw new ValidationException(message);
    }
}
