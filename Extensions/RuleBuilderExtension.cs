using FluentValidation;
using SpotifyApi.Enums;
using SpotifyApi.Helpers;

namespace SpotifyApi.Extensions
{
    public static class RuleBuilderExtensions
    {
        public static void Email<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email address");
        }

        public static void Password<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 8, int maxLength = 150)
        {
            ruleBuilder
            .MinimumLength(minimumLength)
            .WithMessage($"Password must contain at least {minimumLength} characters")
            .MaximumLength(maxLength)
            .WithMessage($"Password can contain up to {maxLength} characters")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character");
        }

        public static void Nickname<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 2, int maxLength = 150)
        {
            ruleBuilder
            .NotEmpty()
            .WithMessage("Nickname is required")
            .MinimumLength(minimumLength)
            .WithMessage($"Nickname must contain at least {minimumLength} characters")
            .MaximumLength(maxLength)
            .WithMessage($"Nickname can contain up to {maxLength} characters")
            .Matches("^[a-zA-Z0-9]+$")
            .WithMessage("Nickname must only contain letters and digits");
        }

        public static void DateOfBirth<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder
            .NotEmpty().WithMessage("DateOfBirth is required")
            .Must(CheckDateOfBirth.BeAValidDate).WithMessage("DateOfBirth must be in the format YYYY-MM-DD")
            .Must(CheckDateOfBirth.BeLessThan100YearsOld).WithMessage("DateOfBirth cannot be more than 100 years old")
            .Must(CheckDateOfBirth.BeLessThanOrEqualToToday).WithMessage("DateOfBirth cannot be later than today's date");
        }

        public static void Gender<T>(this IRuleBuilder<T, UserGender> ruleBuilder)
        {
            ruleBuilder
            .NotNull()
            .WithMessage("Gender is required")
            .IsInEnum()
            .WithMessage("Invalid gender value. It can be only 'Male', 'Female' or 'Other'");
        }

        public static void IsBoolean<T>(this IRuleBuilder<T, bool> ruleBuilder)
        {
            ruleBuilder
            .NotNull()
            .WithMessage("{PropertyName} is required");
        }
    }
}