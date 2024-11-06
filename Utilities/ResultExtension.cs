namespace SpotifyApi.Utilities
{
    public static class ResultExtensions
    {
        public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> bind)
        {
            return result.IsSuccess ? bind(result.Value) : Result<TOut>.Failure(result.Error);
        }

        public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> bindAsync)
        {
            return result.IsSuccess ? await bindAsync(result.Value) : Result<TOut>.Failure(result.Error);
        }

        public static async Task<Result<TOut>> ThenBind<TIn, TOut>(this Task<Result<TIn>> taskResult, Func<TIn, Result<TOut>> bind)
        {
            var result = await taskResult;
            return result.IsSuccess ? bind(result.Value) : Result<TOut>.Failure(result.Error);
        }

        public static async Task<Result<TOut>> ThenBindAsync<TIn, TOut>(this Task<Result<TIn>> taskResult, Func<TIn, Task<Result<TOut>>> bindAsync)
        {
            var result = await taskResult;
            return result.IsSuccess ? await bindAsync(result.Value) : Result<TOut>.Failure(result.Error);
        }

        public static TOut Match<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure)
        {
            return result.IsSuccess
                ? onSuccess(result.Value)
                : onFailure(result.Error);
        }

        public static async Task<TOut> MatchAsync<TIn, TOut>(this Task<Result<TIn>> taskResult, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure)
        {
            var result = await taskResult;
            return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
        }
    }
}