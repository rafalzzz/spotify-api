using FluentValidation;
using SpotifyApi.Extensions;
using SpotifyApi.Requests;

namespace SpotifyApi.Validators
{
    public class PasswordResetValidator : AbstractValidator<PasswordReset>
    {
        public PasswordResetValidator()
        {
            RuleFor(requestBody => requestBody.Login)
            .Cascade(CascadeMode.Stop)
            .Login();
        }
    }
}