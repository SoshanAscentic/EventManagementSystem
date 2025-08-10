// <copyright file="ApiResponse.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public T? Data { get; set; }

        public List<string> Errors { get; set; } = new ();

        public string? Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
            };
        }

        public static ApiResponse<T> ErrorResponse(string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<string> { error },
            };
        }

        public static ApiResponse<T> ErrorResponse(List<string> errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = errors,
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResponse(string? message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
            };
        }

        public static new ApiResponse ErrorResponse(string error)
        {
            return new ApiResponse
            {
                Success = false,
                Errors = new List<string> { error },
            };
        }

        public static new ApiResponse ErrorResponse(List<string> errors)
        {
            return new ApiResponse
            {
                Success = false,
                Errors = errors,
            };
        }
    }
}
