using FluentValidation;
using SpotifyApi.Classes;

namespace SpotifyApi.Validators
{
    public class SearchTracksParamsValidator : AbstractValidator<SearchTracksParams>
    {
        public SearchTracksParamsValidator()
        {
            RuleFor(requestParams => requestParams.Term)
            .NotEmpty()
            .WithMessage("Term is required.");

            RuleFor(requestParams => requestParams.Entity)
            .IsInEnum()
            .WithMessage("Entity is required.");

            RuleFor(requestParams => requestParams.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100.");

            RuleFor(requestParams => requestParams.Offset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Offset must be greater than or equal to 0.");
        }
    }
};
