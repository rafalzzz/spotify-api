using SpotifyApi.Extensions;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IErrorHandlingService
    {
        Error HandleError(
            ErrorType errorType,
            string logErrorAction,
            string initialErrorMessage,
            Exception exception
        );
        Error HandleDatabaseError(string logErrorAction, Exception exception);
    }

    public class ErrorHandlingService(NLog.ILogger logger) : IErrorHandlingService
    {
        private readonly NLog.ILogger _logger = logger;

        public Error HandleError(
            ErrorType errorType,
            string logErrorAction,
            string initialErrorMessage,
            Exception exception
        )
        {
            var logErrorMessage = $"ErrorType: {errorType.GetDescription()}. Action: {logErrorAction} Time: {DateTime.Now}. Error message: {exception.Message}";
            _logger.Error(logErrorMessage);

            var error = new Error(errorType, $"{initialErrorMessage}: " + exception.Message);

            return error;
        }

        public Error HandleDatabaseError(string logErrorAction, Exception exception)
        {
            var initialErrorMessage = "Unexpected database error";

            return HandleError(ErrorType.Database, logErrorAction, initialErrorMessage, exception);
        }
    }
}