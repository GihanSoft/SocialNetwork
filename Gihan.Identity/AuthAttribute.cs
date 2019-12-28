using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace SocialNetwork.Identity
{
    public sealed class AuthAttribute : Attribute, IActionFilter
    {
        private readonly string[] roles;

        public AuthAttribute(string role = null)
        {
            roles = role?.Split(',').Where(r => !string.IsNullOrWhiteSpace(r))
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
            if (roles?.All(r => !context.HttpContext.User.IsInRole(r)) ?? false)
            {
                ThrowForbiddenRequest(context);
                return;
            }
        }
    }
}
