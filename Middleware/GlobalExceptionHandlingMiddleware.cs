using PCShop_Backend.Exceptions;
using Serilog;
using System.Net;
using System.Text.Json;

namespace PCShop_Backend.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
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

            // Map exception to HTTP status and response
            var (statusCode, message) = GetStatusAndMessage(exception);
            context.Response.StatusCode = statusCode;

            var response = new { message, statusCode };

            // Log appropriately based on exception type
            if (statusCode >= 500)
            {
                Log.Error(exception, "Unhandled exception: {Message}", exception.Message);
            }
            else
            {
                Log.Warning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
            }

            return context.Response.WriteAsJsonAsync(response);
        }

        private static (int StatusCode, string Message) GetStatusAndMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication required. Please provide a valid JWT token."),
                UnauthorizedException => (StatusCodes.Status403Forbidden, exception.Message),
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
            };
        }
    }
}
