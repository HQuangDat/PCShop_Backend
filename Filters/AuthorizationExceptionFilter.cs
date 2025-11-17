using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PCShop_Backend.Exceptions;

namespace PCShop_Backend.Filters
{
    public class AuthorizationExceptionFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.Result is ForbidResult or ChallengeResult)
            {
                var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated ?? false;

                if (!isAuthenticated)
                {
                    throw new InvalidCredentialsException("Authentication required. Please provide a valid JWT token.");
                }
                else
                {
                    throw new UnauthorizedException("You do not have the required role to access this resource.");
                }
            }

            await Task.CompletedTask;
        }
    }
}
