using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PCShop_Backend.Filters;

public class ValidIdFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var arg in context.ActionArguments)
        {
            bool isIdParam = arg.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                          || arg.Key.Equals("id", StringComparison.OrdinalIgnoreCase);

            if (isIdParam && arg.Value is int id && id <= 0)
            {
                context.Result = new BadRequestObjectResult(
                    new { message = $"{arg.Key} must be greater than 0." });
                return;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
