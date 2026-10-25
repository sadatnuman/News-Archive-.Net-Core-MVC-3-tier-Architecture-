using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NewsArchive.Filters
{
    public class TokenAuthorizationFilter : IAsyncAuthorizationFilter
    {
        public const string CookieName = "AuthToken";
        private const string UserItemKey = "ApiUser";

        private readonly AuthService authService;
        private readonly int[] roles;

        public TokenAuthorizationFilter(AuthService authService, int[] roles)
        {
            this.authService = authService;
            this.roles = roles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = await ResolveUserAsync(context.HttpContext, authService);
            if (user == null)
            {
                context.Result = Problem(StatusCodes.Status401Unauthorized, "Authentication required",
                    "Send a valid token in the Authorization header as Bearer {token} or in the AuthToken cookie.");
                return;
            }

            if (roles.Length > 0 && !roles.Contains(user.RoleId))
            {
                context.Result = Problem(StatusCodes.Status403Forbidden, "Access denied", "Your role cannot use this endpoint.");
            }
        }

        public static string? ReadToken(HttpRequest request)
        {
            var header = request.Headers.Authorization.ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return header["Bearer ".Length..].Trim();

            return request.Cookies.TryGetValue(CookieName, out var cookie) ? cookie : null;
        }

        public static async Task<AuthUserDTO?> ResolveUserAsync(HttpContext http, AuthService authService)
        {
            if (http.Items.TryGetValue(UserItemKey, out var cached)) return cached as AuthUserDTO;

            var user = await authService.ValidateTokenAsync(ReadToken(http.Request));
            http.Items[UserItemKey] = user;
            return user;
        }

        private static ObjectResult Problem(int status, string title, string detail)
        {
            return new ObjectResult(new ProblemDetails { Status = status, Title = title, Detail = detail })
            {
                StatusCode = status,
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}
