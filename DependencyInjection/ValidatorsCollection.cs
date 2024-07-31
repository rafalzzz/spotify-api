using FluentValidation;
using SpotifyApi.Classes;
using SpotifyApi.Requests;
using SpotifyApi.Validators;

namespace SpotifyApi.DependencyInjection
{
    public static class ValidatorsCollection
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<RegisterUser>, RegisterUserValidator>();
            services.AddScoped<IValidator<SearchTracksParams>, SearchTracksParamsValidator>();

            return services;
        }
    }
}
