using BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace NewsArchive.Auth
{
    public class CookieSessionValidator : CookieAuthenticationEvents
    {
        private readonly IAuthService authService;

        public CookieSessionValidator(IAuthService authService)
        {
            this.authService = authService;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userId = context.Principal?.UserId();
            var current = userId.HasValue ? authService.GetUser(userId.Value) : null;
            if (context.Principal == null || !context.Principal.MatchesCurrent(current))
            {
                context.RejectPrincipal();
                context.HttpContext.Session.Clear();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            if (context.Principal.NeedsRefresh(current!))
            {
                context.ReplacePrincipal(AuthClaims.CreatePrincipal(current!));
                context.ShouldRenew = true;
            }
        }
    }
}
