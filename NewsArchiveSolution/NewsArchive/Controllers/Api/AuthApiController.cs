using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using NewsArchive.Auth;
using NewsArchive.Filters;

namespace NewsArchive.Controllers.Api
{
    [Route("api/auth")]
    public class AuthApiController : ApiControllerBase
    {
        private readonly AuthService authService;
        private readonly LoginThrottle throttle;
        private readonly ApiOptions options;

        public AuthApiController(AuthService authService, LoginThrottle throttle, IOptions<ApiOptions> options)
        {
            this.authService = authService;
            this.throttle = throttle;
            this.options = options.Value;
        }

        [HttpPost("login")]
        [EnableRateLimiting(ApiOptions.LoginRateLimit)]
        [ProducesResponseType(typeof(TokenDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        public IActionResult Login([FromBody] LoginDTO login, [FromQuery] bool webClient = false)
        {
            if (throttle.IsLocked(login.Email))
                return Problem(statusCode: StatusCodes.Status429TooManyRequests, title: "Too many failed attempts. Try again in 15 minutes.");

            var user = authService.Authenticate(login.Email, login.Password);
            if (user == null)
            {
                throttle.RecordFailure(login.Email);
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password");
            }

            throttle.Reset(login.Email);

            var token = authService.IssueToken(user, webClient ? AuthService.WebClient : AuthService.ApiClient, options.TokenLifetime);
            if (token == null)
                return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Could not create a token");

            if (!webClient) return Ok(token);

            Response.Cookies.Append(TokenAuthorizationFilter.CookieName, token.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/api",
                Expires = token.ExpiresAt,
                IsEssential = true
            });
            return Ok(new { token.ExpiresAt, token.User });
        }

        [HttpPost("register")]
        [EnableRateLimiting(ApiOptions.LoginRateLimit)]
        [ProducesResponseType(typeof(AuthUserDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult Register([FromBody] RegisterDTO register)
        {
            var user = authService.Register(register);
            if (user == null)
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Email already exists");

            return CreatedAtAction(nameof(Me), null, user);
        }

        [HttpGet("me")]
        [ApiAuthorize]
        [ProducesResponseType(typeof(AuthUserDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me()
        {
            return Ok(await RequireApiUserAsync());
        }

        [HttpPost("logout")]
        [ApiAuthorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            await authService.LogoutAsync(TokenAuthorizationFilter.ReadToken(Request));
            Response.Cookies.Delete(TokenAuthorizationFilter.CookieName, new CookieOptions { Path = "/api" });
            return NoContent();
        }

        [HttpPost("logout-all")]
        [ApiAuthorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LogoutAll()
        {
            var user = await RequireApiUserAsync();
            await authService.RevokeAllAsync(user.UserId);
            Response.Cookies.Delete(TokenAuthorizationFilter.CookieName, new CookieOptions { Path = "/api" });
            return NoContent();
        }
    }
}
