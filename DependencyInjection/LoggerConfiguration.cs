using NLog;
using NLog.Web;

namespace SpotifyApi.DependencyInjection
{
    public static class LoggerConfiguration
    {
        public static IServiceCollection UseCustomLogging(this IServiceCollection services)
        {
            LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            services.AddSingleton<NLog.ILogger>(LogManager.GetCurrentClassLogger());

            return services;
        }
    }
}