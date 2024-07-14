using System.ComponentModel.DataAnnotations;

namespace SpotifyApi.Validators
{
    public class ValidEntityAttribute : ValidationAttribute
    {
        private readonly string[] _allowedValues;

        public ValidEntityAttribute(params string[] allowedValues)
        {
            _allowedValues = allowedValues;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || _allowedValues.Contains(value.ToString()))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult($"Invalid value for entity. Allowed values are: {string.Join(", ", _allowedValues)}");
        }
    }
}