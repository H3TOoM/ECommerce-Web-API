using FluentValidation;
using ShopAPI.Common.Responses;
using System.Text.Json;

namespace ShopAPI.Middleware
{
    /// <summary>
    /// Global exception handling middleware for consistent error responses
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                // Handle validation exceptions
                ValidationException => CreateValidationErrorResponse(context, (ValidationException)exception),
                
                // Handle not found exceptions
                KeyNotFoundException => CreateErrorResponse(context, 404, exception.Message),
                
                // Handle argument exceptions
                ArgumentException => CreateErrorResponse(context, 400, exception.Message),
                ArgumentNullException => CreateErrorResponse(context, 400, "Required field is null"),
                
                // Handle unauthorized exceptions
                UnauthorizedAccessException => CreateErrorResponse(context, 401, "Unauthorized access"),
                
                // Default: internal server error
                _ => CreateErrorResponse(context, 500, "An unexpected error occurred")
            };

            return context.Response.WriteAsJsonAsync(response);
        }

        private static ApiResponse CreateErrorResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            return ApiResponse.ErrorResponse(message, statusCode);
        }

        private static ApiResponse CreateValidationErrorResponse(HttpContext context, ValidationException exception)
        {
            context.Response.StatusCode = 400;
            var errors = exception.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            return ApiResponse.ValidationErrorResponse(errors);
        }
    }

    public static class GlobalExceptionHandlerExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
