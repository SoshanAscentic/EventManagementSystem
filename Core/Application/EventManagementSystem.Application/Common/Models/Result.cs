// <copyright file="Result.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Models
{
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException("Success result cannot have an error");
            }

            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException("Failure result must have an error");
            }

            this.IsSuccess = isSuccess;
            this.Error = error;
        }

        protected Result(bool isSuccess, Error[] errors)
        {
            if (isSuccess && errors.Any(e => e != Error.None))
            {
                throw new InvalidOperationException("Success result cannot have errors");
            }

            if (!isSuccess && !errors.Any())
            {
                throw new InvalidOperationException("Failure result must have at least one error");
            }

            this.IsSuccess = isSuccess;
            this.Errors = errors;
            this.Error = errors.FirstOrDefault() ?? Error.None;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !this.IsSuccess;

        public Error Error { get; } = Error.None;

        public Error[] Errors { get; } = Array.Empty<Error>();

        public static implicit operator Result(Error error) => Failure(error);

        public static Result Success() => new(true, Error.None);

        public static Result Failure(Error error) => new(false, error);

        public static Result Failure(Error[] errors) => new(false, errors);

        public static Result Failure(string code, string message) => new(false, Error.Failure(code, message));

        // Helper method for validation failures from string array
        public static Result Failure(IEnumerable<string> errors) =>
            new(false, errors.Select(e => Error.Validation("Validation.Failed", e)).ToArray());

        // Specific error type methods
        public static Result ValidationFailure(string code, string message) =>
            new(false, Error.Validation(code, message));

        public static Result ValidationFailure(IEnumerable<string> errors) =>
            new(false, errors.Select(e => Error.Validation("Validation.Failed", e)).ToArray());

        public static Result NotFound(string code, string message) =>
            new(false, Error.NotFound(code, message));

        public static Result Conflict(string code, string message) =>
            new(false, Error.Conflict(code, message));

        public static Result Unauthorized(string code, string message) =>
            new(false, Error.Unauthorized(code, message));

        public static Result Forbidden(string code, string message) =>
            new(false, Error.Forbidden(code, message));

        public static Result BadRequest(string code, string message) =>
            new(false, Error.BadRequest(code, message));

        // Backward compatibility methods
        public string GetErrorMessage() => this.Error.Message;

        public string[] GetErrorMessages() => this.Errors.Select(e => e.Message).ToArray();

        public string[] GetErrorCodes() => this.Errors.Select(e => e.Code).ToArray();
    }

    public class Result<T> : Result
    {
        private readonly T? value;

        protected Result(T? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            this.value = value;
        }

        protected Result(T? value, bool isSuccess, Error[] errors)
            : base(isSuccess, errors)
        {
            this.value = value;
        }

        public T Value => this.IsSuccess
            ? this.value!
            : throw new InvalidOperationException("Cannot access value of a failed result");

        public T? ValueOrDefault => this.value;

        public static implicit operator Result<T>(T value) =>
           value is not null ? Success(value) : Failure(Error.NullValue);

        public static implicit operator Result<T>(Error error) => Failure(error);

        public static Result<T> Success(T value) => new(value, true, Error.None);

        public static new Result<T> Failure(Error error) => new(default, false, error);

        public static new Result<T> Failure(Error[] errors) => new(default, false, errors);

        public static new Result<T> Failure(string code, string message) =>
            new(default, false, Error.Failure(code, message));

        // Helper method for validation failures from string array
        public static new Result<T> Failure(IEnumerable<string> errors) =>
            new(default, false, errors.Select(e => Error.Validation("Validation.Failed", e)).ToArray());

        public static new Result<T> ValidationFailure(string code, string message) =>
            new(default, false, Error.Validation(code, message));

        public static new Result<T> ValidationFailure(IEnumerable<string> errors) =>
            new(default, false, errors.Select(e => Error.Validation("Validation.Failed", e)).ToArray());

        public static new Result<T> NotFound(string code, string message) =>
            new(default, false, Error.NotFound(code, message));

        public static new Result<T> Conflict(string code, string message) =>
            new(default, false, Error.Conflict(code, message));

        public static new Result<T> Unauthorized(string code, string message) =>
            new(default, false, Error.Unauthorized(code, message));

        public static new Result<T> Forbidden(string code, string message) =>
            new(default, false, Error.Forbidden(code, message));

        public static new Result<T> BadRequest(string code, string message) =>
            new(default, false, Error.BadRequest(code, message));

        // Functional programming methods
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return this.IsSuccess ? Result<TNew>.Success(mapper(this.Value)) : Result<TNew>.Failure(this.Error);
        }

        public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
        {
            return this.IsSuccess ? Result<TNew>.Success(await mapper(this.Value)) : Result<TNew>.Failure(this.Error);
        }

        public Result<T> Tap(Action<T> action)
        {
            if (this.IsSuccess)
            {
                action(this.Value);
            }

            return this;
        }

        public async Task<Result<T>> TapAsync(Func<T, Task> action)
        {
            if (this.IsSuccess)
            {
                await action(this.Value);
            }

            return this;
        }
    }
}
