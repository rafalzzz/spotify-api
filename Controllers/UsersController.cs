using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SpotifyApi.Variables;
using SpotifyApi.Requests;
using SpotifyApi.Services;

namespace SpotifyApi.Controllers
{
    [ApiController]
    [Route(ControllerRoutes.User)]
    public class UserController(
        IRequestValidatorService requestValidatorService,
        IValidator<RegisterUser> registerUserValidator,
        IUserService userService
    ) : ControllerBase
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<RegisterUser> _registerUserValidator = registerUserValidator;
        private readonly IUserService _userService = userService;

        [HttpPost]
        public async Task<ActionResult> Register([FromBody] RegisterUser registerUserDto)
        {
            return await ValidateRegistration(registerUserDto)
                .BindAsync(async _ => await CheckIfUserExists(registerUserDto))
                .ThenBind(_ => CreateUser(registerUserDto))
                .MatchAsync(_ => Results.Ok(), e => Results.BadRequest("test"));
        }

        private Result<RegisterUser> ValidateRegistration(RegisterUser registerUserDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(registerUserDto, _registerUserValidator);

            return validationResult.IsSuccess ? Result<RegisterUser>.Success(registerUserDto) :
                                               Result<RegisterUser>.Failure(new Error(ErrorType.Validation, validationResult.Error.Description));
        }

        private async Task<Result<RegisterUser>> CheckIfUserExists(RegisterUser registerUserDto)
        {
            bool exists = await _userService.UserExists(registerUserDto.Email, registerUserDto.Nickname);
            return !exists ? Result<RegisterUser>.Success(registerUserDto) :
                             Result<RegisterUser>.Failure(new Error(ErrorType.Failure, "UserExists"));
        }

        private Result<int> CreateUser(RegisterUser registerUserDto)
        {
            var id = _userService.CreateUser(registerUserDto);
            return id != null ? Result<int>.Success(id.Value) :
                                Result<int>.Failure(new Error(ErrorType.Failure, "Failed to create user"));
        }
    }

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
            Result<TIn> result = await taskResult;
            return result.IsSuccess ? bind(result.Value) : Result<TOut>.Failure(result.Error);

        }

        public static TOut Match<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure)
        {
            return result.IsSuccess
                ? onSuccess(result.Value)
                : onFailure(result.Error);
        }

        public static async Task<TOut> MatchAsync<TIn, TOut>(this Task<Result<TIn>> taskResult, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure)
        {
            Result<TIn> result = await taskResult;
            return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
        }
    }

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

