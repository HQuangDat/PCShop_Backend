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

            var response = new
            {
                message = exception.Message,
                statusCode = context.Response.StatusCode
            };

            switch (exception)
            {
                case UnauthorizedException:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    response = new
                    {
                        message = exception.Message,
                        statusCode = StatusCodes.Status403Forbidden
                    };
                    Log.Warning("Authorization failed: {Message}", exception.Message);
                    break;

                case InvalidCredentialsException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response = new
                    {
                        message = exception.Message,
                        statusCode = StatusCodes.Status401Unauthorized
                    };
                    Log.Warning("Authentication failed: {Message}", exception.Message);
                    break;

                case InvalidTokenException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response = new
                    {
                        message = exception.Message,
                        statusCode = StatusCodes.Status401Unauthorized
                    };
                    Log.Warning("Invalid token: {Message}", exception.Message);
                    break;

                case NotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response = new
                    {
                        message = exception.Message,
                        statusCode = StatusCodes.Status404NotFound
                    };
                    Log.Warning("Resource not found: {Message}", exception.Message);
                    break;

                case ConflictException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    response = new
                    {
                        message = exception.Message,
                        statusCode = StatusCodes.Status409Conflict
                    };
                    Log.Warning("Conflict occurred: {Message}", exception.Message);
                    break;

                case ArgumentException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new
                    {
                        message = "Invalid argument: " + exception.Message,
                        statusCode = StatusCodes.Status400BadRequest
                    };
                    Log.Warning("Invalid argument: {Message}", exception.Message);
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response = new
                    {
                        message = "An unexpected error occurred. Please try again later.",
                        statusCode = StatusCodes.Status500InternalServerError
                    };
                    Log.Error(exception, "Unhandled exception: {Message}", exception.Message);
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
