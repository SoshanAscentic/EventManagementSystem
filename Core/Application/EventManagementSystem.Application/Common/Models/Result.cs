// <copyright file="Result.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Models
{
    public class Result
    {
        private static readonly Result SuccessValue = new (true, Array.Empty<string>());
        private static readonly Dictionary<string, Result> FailureCache = new ();

        protected Result(bool isSuccess, IEnumerable<string> errors)
        {
            this.IsSuccess = isSuccess;
            this.Errors = errors?.ToArray() ?? Array.Empty<string>();
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !this.IsSuccess;

        public string[] Errors { get; }

        public string Error => this.Errors.FirstOrDefault() ?? string.Empty;

        public static Result Success() => SuccessValue;

        public static Result Failure(string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                return SuccessValue;
            }

            // Cache common failures for performance
            if (FailureCache.TryGetValue(error, out var cachedResult))
            {
                return cachedResult;
            }

            var result = new Result(false, new[] { error });
            if (FailureCache.Count < 100) // Limit cache size
            {
                FailureCache[error] = result;
            }

            return result;
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            var errorArray = errors?.ToArray() ?? Array.Empty<string>();
            return errorArray.Length == 0 ? SuccessValue : new Result(false, errorArray);
        }

        public static Result<T> Success<T>(T value) => new (value, true, Array.Empty<string>());

        public static Result<T> Failure<T>(string error) => new (default, false, new[] { error });

        public static Result<T> Failure<T>(IEnumerable<string> errors) => new (default, false, errors);

        public override string ToString()
        {
            return this.IsSuccess ? "Success" : $"Failure: {string.Join(", ", this.Errors)}";
        }
    }

    public class Result<T> : Result
    {
        private readonly T? value;

        internal Result(T? value, bool isSuccess, IEnumerable<string> errors)
            : base(isSuccess, errors)
        {
            this.value = value;
        }

        public T Value => this.IsSuccess ? this.value! : throw new InvalidOperationException("Cannot access value of failed result");

        public T? ValueOrDefault => this.value;

        public static implicit operator Result<T>(T value) => Success(value);

        public static implicit operator Result<T>(Result result)
        {
            return result.IsSuccess ? Success(default(T) !) : Failure<T>(result.Errors);
        }

        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return this.IsSuccess ? Success(mapper(this.Value)) : Failure<TNew>(this.Errors);
        }

        public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
        {
            return this.IsSuccess ? Success(await mapper(this.Value)) : Failure<TNew>(this.Errors);
        }

        public Result<T> OnSuccess(Action<T> action)
        {
            if (this.IsSuccess)
            {
                action(this.Value);
            }

            return this;
        }

        public Result<T> OnFailure(Action<string[]> action)
        {
            if (this.IsFailure)
            {
                action(this.Errors);
            }

            return this;
        }
    }
}
