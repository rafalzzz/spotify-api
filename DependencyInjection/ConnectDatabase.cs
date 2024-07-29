
using Microsoft.EntityFrameworkCore;
using SpotifyApi.Entities;
using SpotifyApi.Variables;

namespace SpotifyApi.DependencyInjection
{
    public static class Database
    {
        public static IServiceCollection ConnectDatabase(this IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariables.ConnectionString);
            services.AddDbContext<SpotifyDbContext>(options => options.UseNpgsql(connectionString));

            return services;
        }
    }
}
