using FluentValidation.Results;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SpotifyApi.Classes;

namespace SpotifyApi.Services
{
    public interface IRequestValidatorService
    {
        ActionResult ValidateRequest<T>(T request, IValidator<T> validator);
    }

    public class RequestValidatorService : IRequestValidatorService
    {
        private IEnumerable<ValidationError>? GetValidationErrorsResult(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                var errorList = validationResult.Errors
                .Select(error => new ValidationError
                {
                    Property = error.PropertyName,
                    ErrorMessage = error.ErrorMessage
                });

                return errorList;
            }

            return null;
        }

        public ActionResult ValidateRequest<T>(T request, IValidator<T> validator)
        {
            var validationResults = validator.Validate(request);
            IEnumerable<ValidationError>? validationResultErrors = GetValidationErrorsResult(validationResults);

            if (validationResultErrors != null)
            {
                return new BadRequestObjectResult(validationResultErrors);
            }

            return new OkResult();
        }
    }
}