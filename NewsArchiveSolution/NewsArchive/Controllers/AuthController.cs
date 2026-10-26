using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NewsArchive.Auth;
using System.Security.Cryptography;

namespace NewsArchive.Controllers
{
    public class AuthController : AppController
    {
        private const int MaxOtpAttempts = 5;

        private readonly IAuthService authService;
        private readonly IUserService userService;
        private readonly LoginThrottle throttle;
        private readonly ILogger<AuthController> logger;
        private readonly IWebHostEnvironment environment;

        public AuthController(IAuthService authService, IUserService userService, LoginThrottle throttle, ILogger<AuthController> logger, IWebHostEnvironment environment)
        {
            this.authService = authService;
            this.userService = userService;
            this.throttle = throttle;
            this.logger = logger;
            this.environment = environment;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.IsSignedIn()) return RedirectToLocal(returnUrl);
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginDTO());
        }

        [HttpPost]
        [EnableRateLimiting(ApiOptions.LoginRateLimit)]
        public async Task<IActionResult> Login(LoginDTO model, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            if (throttle.IsLocked(model.Email))
            {
                Flash("danger", "Too many failed attempts. Try again in 15 minutes.");
                return View(model);
            }

            var user = authService.Authenticate(model.Email, model.Password);
            if (user == null)
            {
                throttle.RecordFailure(model.Email);
                Flash("danger", "Invalid username/email or password");
                return View(model);
            }

            throttle.Reset(model.Email);
            await SignInUserAsync(user);
            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserDTO());
        }

        [HttpPost]
        [EnableRateLimiting(ApiOptions.LoginRateLimit)]
        public IActionResult Create(UserDTO obj)
        {
            if (!ModelState.IsValid)
                return View(obj);

            obj.RoleId = AppRoles.Reader;
            if (userService.Create(obj))
            {
                Flash("success", "User created successfully");
                return RedirectToAction("Login", "Auth");
            }

            Flash("danger", "Failed to create user");
            return View(obj);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            Flash("success", "Logged out successfully");
            return RedirectToAction("Index", "Article");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        [EnableRateLimiting(ApiOptions.LoginRateLimit)]
        public IActionResult ForgetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Flash("danger", "Email is required");
                return View();
            }

            email = email.Trim();
            var user = userService.GetByEmail(email);
            if (user == null)
            {
                Flash("success", "If an account exists for that email, a verification code has been sent.");
                return RedirectToAction("VerifyOtp", new { email });
            }

            var otpString = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);
            HttpContext.Session.SetString($"PwdReset:Otp:{email}", otpString);
            HttpContext.Session.SetString($"PwdReset:Expiry:{email}", expiry.ToString("o"));
            HttpContext.Session.SetInt32($"PwdReset:Used:{email}", 0);
            HttpContext.Session.SetInt32($"PwdReset:UserId:{email}", user.UserId);
            HttpContext.Session.SetInt32($"PwdReset:Attempts:{email}", 0);

            if (environment.IsDevelopment())
                logger.LogInformation("Password reset code for {Email}: {Otp}", email, otpString);

            Flash("success", "If an account exists for that email, a verification code has been sent.");

            return RedirectToAction("VerifyOtp", new { email });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [EnableRateLimiting(ApiOptions.LoginRateLimit)]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            ViewBag.Email = email;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            {
                Flash("danger", "Email and OTP are required");
                return View();
            }

            email = email.Trim();
            var sessionOtp = HttpContext.Session.GetString($"PwdReset:Otp:{email}");
            var expiryString = HttpContext.Session.GetString($"PwdReset:Expiry:{email}");
            var usedFlag = HttpContext.Session.GetInt32($"PwdReset:Used:{email}");
            var userId = HttpContext.Session.GetInt32($"PwdReset:UserId:{email}");

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(expiryString) || userId == null)
            {
                Flash("danger", "No OTP request found for this email");
                return View();
            }

            if (usedFlag == 1)
            {
                Flash("danger", "OTP already used");
                return View();
            }

            var attempts = HttpContext.Session.GetInt32($"PwdReset:Attempts:{email}") ?? 0;
            if (attempts >= MaxOtpAttempts)
            {
                HttpContext.Session.SetInt32($"PwdReset:Used:{email}", 1);
                Flash("danger", "Too many failed attempts. Request a new code.");
                return View();
            }

            if (!DateTime.TryParse(expiryString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiry))
            {
                Flash("danger", "Invalid OTP data");
                return View();
            }

            if (DateTime.UtcNow > expiry)
            {
                Flash("danger", "OTP expired");
                return View();
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(sessionOtp),
                    System.Text.Encoding.UTF8.GetBytes(otp.Trim())))
            {
                HttpContext.Session.SetInt32($"PwdReset:Attempts:{email}", attempts + 1);
                Flash("danger", "Invalid OTP");
                return View();
            }

            HttpContext.Session.SetInt32($"PwdReset:Used:{email}", 1);

            var user = authService.GetUser(userId.Value);
            if (user == null)
            {
                Flash("danger", "User not found");
                return View();
            }

            await SignInUserAsync(user);

            return user.RoleId == AppRoles.Admin
                ? RedirectToAction("ChangePassword", "Admin")
                : RedirectToAction("ChangePassword", "Member");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction("Index", "Article");
        }
    }
}
