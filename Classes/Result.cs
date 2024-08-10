namespace SpotifyApi.Classes
{
    public sealed class Result<T>
    {
        private Result(T value)
        {
            Value = value;
            IsSuccess = true;
        }

        private Result(Error error)
        {
            Error = error;
            IsSuccess = false;
        }

        public T Value { get; private set; }
        public Error Error { get; private set; }
        public bool IsSuccess { get; private set; }

        public static Result<T> Success(T value) => new Result<T>(value);
        public static Result<T> Failure(Error error) => new Result<T>(error);
    }

    public enum ErrorType
    {
        Failure = 0,
        Validation = 1
    }



    public record Error(ErrorType Type, object Description)
    {
        public static Error NoLineItems = new(ErrorType.Validation, "Line items are empty");
        public static Error NotEnoughStock = new(ErrorType.Validation, "Not enough stock for order");
        public static Error PaymentFailed = new(ErrorType.Failure, "Failed to process payment");
    }
}
