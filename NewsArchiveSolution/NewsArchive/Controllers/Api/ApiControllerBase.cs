using BLL;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Auth;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [ApiController]
    [IgnoreAntiforgeryToken]
    [EnableCors(ApiOptions.CorsPolicy)]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected Task<AuthUserDTO?> GetApiUserAsync()
        {
            return TokenAuthorizationFilter.ResolveUserAsync(HttpContext, HttpContext.RequestServices.GetRequiredService<AuthService>());
        }

        protected async Task<AuthUserDTO> RequireApiUserAsync()
        {
            return await GetApiUserAsync() ?? throw new InvalidOperationException("ApiAuthorize must run before this action.");
        }

        protected static string? ParseLanguage(string? lang)
        {
            return string.IsNullOrWhiteSpace(lang) ? null : AppLanguages.Normalize(lang);
        }

        protected ObjectResult Forbidden(string detail)
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, title: "Access denied", detail: detail);
        }
    }
}
