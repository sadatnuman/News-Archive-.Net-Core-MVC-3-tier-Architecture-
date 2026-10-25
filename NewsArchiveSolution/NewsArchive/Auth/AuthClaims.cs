using BLL.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using System.Security.Claims;

namespace NewsArchive.Auth
{
    public static class AuthClaims
    {
        public const string RoleIdType = "RoleId";
        public const string StampType = "SecurityStamp";

        public static ClaimsPrincipal CreatePrincipal(AuthUserDTO user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.RoleName),
                new(RoleIdType, user.RoleId.ToString(CultureInfo.InvariantCulture)),
                new(StampType, user.SecurityStamp)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        public static bool MatchesCurrent(this ClaimsPrincipal principal, AuthUserDTO? user)
        {
            return user != null
                && principal.UserId() == user.UserId
                && principal.RoleId() == user.RoleId
                && principal.FindFirstValue(StampType) == user.SecurityStamp;
        }

        public static bool IsSignedIn(this ClaimsPrincipal user) => user.Identity?.IsAuthenticated == true;

        public static int? UserId(this ClaimsPrincipal user) => ReadInt(user, ClaimTypes.NameIdentifier);

        public static int? RoleId(this ClaimsPrincipal user) => ReadInt(user, RoleIdType);

        public static string? Email(this ClaimsPrincipal user) => user.IsSignedIn() ? user.FindFirstValue(ClaimTypes.Email) : null;

        public static string? DisplayName(this ClaimsPrincipal user) => user.IsSignedIn() ? user.FindFirstValue(ClaimTypes.Name) : null;

        private static int? ReadInt(ClaimsPrincipal user, string type)
        {
            if (!user.IsSignedIn()) return null;
            return int.TryParse(user.FindFirstValue(type), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
        }
    }
}
