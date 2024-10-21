using FluentValidation;
using SpotifyApi.Requests;

namespace SpotifyApi.Validators
{
    public class AddCollaboratorValidator : AbstractValidator<AddCollaborator>
    {
        public AddCollaboratorValidator()
        {
            RuleFor(requestBody => requestBody.CollaboratorId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("CollaboratorId is required");
        }
    }
}