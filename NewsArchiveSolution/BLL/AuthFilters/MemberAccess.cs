using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auth.AuthFilters
{
    public class MemberAccess : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var data = context.HttpContext.Session.GetString("Email");
            var type = context.HttpContext.Session.GetInt32("RoleId");
            if (data == null || type != 2)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
