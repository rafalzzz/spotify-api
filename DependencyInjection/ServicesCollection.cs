using SpotifyApi.Services;

namespace SpotifyApi.DependencyInjection
{
    public static class ServicesCollection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IPasswordHasher, PasswordHasher>();
            services.AddTransient<IRequestValidatorService, RequestValidatorService>();

            return services;
        }
    }
}
