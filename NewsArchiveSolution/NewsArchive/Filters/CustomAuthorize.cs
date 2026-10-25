using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NewsArchive.Auth;

namespace NewsArchive.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class CustomAuthorize : Attribute, IAuthorizationFilter
    {
        private readonly int[] roles;

        public CustomAuthorize(params int[] roles)
        {
            this.roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;
            if (!http.User.IsSignedIn())
            {
                var returnUrl = $"{http.Request.PathBase}{http.Request.Path}{http.Request.QueryString}";
                context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
                return;
            }

            if (roles.Length > 0 && !roles.Contains(http.User.RoleId() ?? 0))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }
    }
}
