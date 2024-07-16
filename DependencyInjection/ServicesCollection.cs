using SpotifyApi.Services;

namespace SpotifyApi.DependencyInjection
{
    public static class ServicesCollection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IRequestValidatorService, RequestValidatorService>();

            return services;
        }
    }
}
