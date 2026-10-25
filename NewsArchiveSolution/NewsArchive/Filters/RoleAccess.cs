using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NewsArchive.Filters
{
    public class RoleAccess : Attribute, IAuthorizationFilter
    {
        private readonly int[] roles;

        public RoleAccess(params int[] roles)
        {
            this.roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var email = context.HttpContext.Session.GetString("Email");
            var roleId = context.HttpContext.Session.GetInt32("RoleId");
            if (email == null || roleId == null || !roles.Contains(roleId.Value))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
