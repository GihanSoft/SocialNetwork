using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System;
using System.Linq;

namespace Gihan.Identity;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AuthAttribute : Attribute, IActionFilter
{
    private readonly string[] _roles;

    public AuthAttribute(string role = null)
    {
        _roles = role?.Split(',').Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim()).ToArray();
    }

    private static void ThrowUnauthorizedRequest(ActionExecutingContext filterContext)
        => filterContext.Result = new UnauthorizedResult();
    private static void ThrowForbiddenRequest(ActionExecutingContext context)
        => context.Result = new ForbidResult();

    public void OnActionExecuted(ActionExecutedContext context) { } //do nothing

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            ThrowUnauthorizedRequest(context);
            return;
        }

        if (_roles?.All(r => !context.HttpContext.User.IsInRole(r)) ?? false)
        {
            ThrowForbiddenRequest(context);
            return;
        }
    }
}
