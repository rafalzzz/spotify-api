using SpotifyApi.Extensions;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IErrorHandlingService
    {
        Error HandleError(
            Exception exception,
            ErrorType errorType,
            string logErrorAction,
            string? initialErrorMessage = ""
        );
        Error HandleDatabaseError(Exception exception, string logErrorAction);
    }

    public class ErrorHandlingService(NLog.ILogger logger) : IErrorHandlingService
    {
        private readonly NLog.ILogger _logger = logger;

        public Error HandleError(
            Exception exception,
            ErrorType errorType,
            string logErrorAction,
            string? initialErrorMessage = ""
        )
        {
            var logErrorMessage = $"ErrorType: {errorType.GetDescription()}. Action: {logErrorAction} Time: {DateTime.Now}. Error message: {exception.Message}";
            _logger.Error(logErrorMessage);

            var error = new Error(errorType, $"{initialErrorMessage}: " + exception.Message);

            return error;
        }

        public Error HandleDatabaseError(Exception exception, string logErrorAction)
        {
            return HandleError(exception, ErrorType.Database, logErrorAction);
        }
    }
}