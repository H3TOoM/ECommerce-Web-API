namespace ShopAPI.Common.Responses
{
    /// <summary>
    /// Standard API response wrapper for all endpoints
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Response data (null if failed)
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Detailed errors (for validation failures)
        /// </summary>
        public Dictionary<string, string[]> Errors { get; set; } = new();

        /// <summary>
        /// Timestamp when the response was generated
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string message = "Request successful", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 500, Dictionary<string, string[]>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default,
                Errors = errors ?? new(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, string[]> errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation failed",
                Data = default,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Non-generic version for endpoints that don't return data
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse SuccessResponse(string message = "Request successful", int statusCode = 200)
        {
            return new ApiResponse
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse ErrorResponse(string message, int statusCode = 500, Dictionary<string, string[]>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = errors ?? new(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse ValidationErrorResponse(Dictionary<string, string[]> errors)
        {
            return new ApiResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation failed",
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
