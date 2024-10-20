using FluentValidation;
using SpotifyApi.Extensions;
using SpotifyApi.Requests;

namespace SpotifyApi.Validators
{
    public class LoginUserValidator : AbstractValidator<LoginUser>
    {
        public LoginUserValidator()
        {
            RuleFor(requestBody => requestBody.Login)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Login is required");

            RuleFor(requestBody => requestBody.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password is required");

            RuleFor(requestBody => requestBody.RememberMe)
                .Cascade(CascadeMode.Stop)
                .IsBoolean();
        }
    }
}