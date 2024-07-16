using FluentValidation;
using SpotifyApi.Classes;

namespace SpotifyApi.Validators
{
    public class SearchTracksParamsValidator : AbstractValidator<SearchTracksParams>
    {
        public SearchTracksParamsValidator()
        {
            RuleFor(x => x.Term)
            .NotEmpty()
            .WithMessage("Term is required.");

            RuleFor(x => x.Entity)
            .IsInEnum()
            .WithMessage("Entity is required.");

            RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100.");

            RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Offset must be greater than or equal to 0.");
        }
    }
};
