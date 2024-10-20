using FluentValidation;
using SpotifyApi.Requests;

namespace SpotifyApi.Validators
{
    public class CreatePlaylistValidator : AbstractValidator<CreatePlaylist>
    {
        public CreatePlaylistValidator()
        {
            RuleFor(requestBody => requestBody.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(50)
            .WithMessage("Name cannot be longer than 50 characters");

            RuleFor(requestBody => requestBody.Description)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(300)
            .WithMessage("Description cannot be longer than 300 characters");

            RuleFor(requestBody => requestBody.IsPublic)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("IsPublic is required");
        }
    }
}