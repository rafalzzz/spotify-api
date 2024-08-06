using FluentValidation;
using SpotifyApi.Extensions;
using SpotifyApi.Requests;

namespace SpotifyApi.Validators
{
    public class PasswordResetCompleteValidator : AbstractValidator<PasswordResetComplete>
    {
        public PasswordResetCompleteValidator()
        {
            RuleFor(requestBody => requestBody.Password)
            .Cascade(CascadeMode.Stop)
            .Password();
        }

    }
}