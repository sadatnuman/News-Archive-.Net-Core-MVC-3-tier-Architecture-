using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NewsArchive.Filters
{
    public class Logged : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Session.GetString("Email") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
