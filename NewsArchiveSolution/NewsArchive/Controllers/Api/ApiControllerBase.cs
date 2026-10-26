using BLL;
using BLL.DTOs;
using BLL.Interfaces;
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
        protected Task<AuthUserDTO> RequireApiUserAsync()
        {
            return Task.FromResult(TokenAuthorizationFilter.CurrentUser(HttpContext)
                ?? throw new InvalidOperationException("ApiAuthorize must run before this action."));
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
