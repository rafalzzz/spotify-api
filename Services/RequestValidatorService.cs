using FluentValidation;
using FluentValidation.Results;
using SpotifyApi.Classes;
using System.Linq;

namespace SpotifyApi.Services
{
    public interface IRequestValidatorService
    {
        Result<T> ValidateRequest<T>(T request, IValidator<T> validator);
    }

    public class RequestValidatorService : IRequestValidatorService
    {
        public Result<T> ValidateRequest<T>(T request, IValidator<T> validator)
        {
            ValidationResult validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    }).ToArray();

                return Result<T>.Failure(new Error(ErrorType.Validation, errors));
            }

            return Result<T>.Success(request);
        }
    }
}