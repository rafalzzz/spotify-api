using SpotifyApi.Extensions;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IErrorHandlingService
    {
        public void DisplayLogError(ErrorType errorType, string logErrorAction, string errorMessage);
        Error HandleError(
            Exception exception,
            ErrorType errorType,
            string logErrorAction,
            string? initialErrorMessage = ""
        );
        Error HandleDatabaseError(Exception exception, string logErrorAction);
        Error HandleConfigurationError();
    }

    public class ErrorHandlingService(NLog.ILogger logger) : IErrorHandlingService
    {
        private readonly NLog.ILogger _logger = logger;

        public void DisplayLogError(ErrorType errorType, string logErrorAction, string errorMessage)
        {
            var logErrorMessage = $"ErrorType: {errorType.GetDescription()}. Action: {logErrorAction} Time: {DateTime.Now}. Error message: {errorMessage}";
            _logger.Error(logErrorMessage);
        }

        public Error HandleError(
            Exception exception,
            ErrorType errorType,
            string logErrorAction,
            string? initialErrorMessage = ""
        )
        {
            DisplayLogError(errorType, logErrorAction, exception.Message);

            var error = new Error(errorType, $"{initialErrorMessage}: " + exception.Message);
            return error;
        }

        public Error HandleDatabaseError(Exception exception, string logErrorAction)
        {
            return HandleError(exception, ErrorType.Database, logErrorAction);
        }

        public Error HandleConfigurationError()
        {
            var initialErrorMessage = "Unexpected configuration error";
            DisplayLogError(ErrorType.ConfigurationError, initialErrorMessage, "Missing secretKey");

            return Error.ConfigurationError;
        }
    }
}