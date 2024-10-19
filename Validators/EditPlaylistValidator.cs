using FluentValidation;
using SpotifyApi.Requests;

namespace SpotifyApi.Validators
{
    public class EditPlaylistValidator : AbstractValidator<EditPlaylist>
    {
        public EditPlaylistValidator()
        {
            RuleFor(requestBody => requestBody.Name)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(50)
                .When(requestBody => requestBody.Name != null)
                .WithMessage("Name cannot be longer than 50 characters");

            RuleFor(requestBody => requestBody.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(300)
                .When(requestBody => requestBody.Description != null)
                .WithMessage("Description cannot be longer than 300 characters");

            RuleFor(requestBody => requestBody.IsPublic)
                   .Must(value => value != null)
                   .WithMessage("IsPublic must be a boolean value.");
        }
    }
}