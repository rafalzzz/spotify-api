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
            services.AddTransient<IValidator<RegisterUser>, RegisterUserValidator>();
            services.AddTransient<IValidator<LoginUser>, LoginUserValidator>();
            services.AddTransient<IValidator<PasswordReset>, PasswordResetValidator>();
            services.AddTransient<IValidator<PasswordResetComplete>, PasswordResetCompleteValidator>();
            services.AddTransient<IValidator<SearchTracksParams>, SearchTracksParamsValidator>();
            services.AddTransient<IValidator<CreatePlaylist>, CreatePlaylistValidator>();
            services.AddTransient<IValidator<EditPlaylist>, EditPlaylistValidator>();

            return services;
        }
    }
}
