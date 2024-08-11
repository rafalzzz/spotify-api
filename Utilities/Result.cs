namespace SpotifyApi.Utilities
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

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(Error error) => new(error);
    }
}