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
            services.AddScoped<IValidator<LoginUser>, LoginUserValidator>();
            services.AddScoped<IValidator<PasswordReset>, PasswordResetValidator>();
            services.AddScoped<IValidator<PasswordResetComplete>, PasswordResetCompleteValidator>();
            services.AddScoped<IValidator<SearchTracksParams>, SearchTracksParamsValidator>();
            services.AddScoped<IValidator<CreatePlaylist>, CreatePlaylistValidator>();
            services.AddScoped<IValidator<EditPlaylist>, EditPlaylistValidator>();

            return services;
        }
    }
}
