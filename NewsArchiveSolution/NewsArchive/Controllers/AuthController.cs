using BLL;
using BLL.Audit;
using BLL.DTOs;
using BLL.Email;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NewsArchive.Auth;
using NewsArchive.Email;

namespace NewsArchive.Controllers
{
    public class AuthController : AppController
    {
        private readonly IAuthService authService;
        private readonly IUserService userService;
        private readonly LoginThrottle throttle;
        private readonly PasswordResetStore resetStore;
        private readonly IEmailSender emailSender;
        private readonly MailComposer mail;
        private readonly IAuditService audit;

        public AuthController(IAuthService authService, IUserService userService, LoginThrottle throttle, PasswordResetStore resetStore,
            IEmailSender emailSender, MailComposer mail, IAuditService audit)
        {
            this.authService = authService;
            this.userService = userService;
            this.throttle = throttle;
            this.resetStore = resetStore;
            this.emailSender = emailSender;
            this.mail = mail;
            this.audit = audit;
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
        public IActionResult Logout()
        {
            if (!User.IsSignedIn()) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ActionName("Logout")]
        public async Task<IActionResult> LogoutConfirmed(string? next)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            Flash("success", "Logged out successfully");
            return next == "login" ? RedirectToAction("Login") : RedirectToAction("Index", "Article");
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
        public async Task<IActionResult> ForgetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 150)
            {
                Flash("danger", "Email is required");
                return View();
            }

            email = email.Trim();
            var user = userService.GetByEmail(email);
            if (user != null)
            {
                var code = resetStore.Issue(user.UserId, email);
                if (code != null)
                {
                    await emailSender.SendAsync(mail.PasswordResetCode(user.Email, code, CurrentLanguage, PasswordResetStore.CodeLifetime));
                    audit.Record(AuditActions.PasswordResetRequested, AuditEntities.Security, user.UserId, null, user.UserId);
                }
            }

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
            switch (resetStore.Verify(email, otp, out var userId))
            {
                case PasswordResetStore.VerifyResult.NotRequested:
                    Flash("danger", "No OTP request found for this email");
                    return View();
                case PasswordResetStore.VerifyResult.Expired:
                    Flash("danger", "OTP expired");
                    return View();
                case PasswordResetStore.VerifyResult.TooManyAttempts:
                    Flash("danger", "Too many failed attempts. Request a new code.");
                    return View();
                case PasswordResetStore.VerifyResult.Invalid:
                    Flash("danger", "Invalid OTP");
                    return View();
            }

            var user = authService.GetUser(userId);
            if (user == null)
            {
                Flash("danger", "User not found");
                return View();
            }

            await SignInUserAsync(user);
            PasswordChanger.MarkResetVerified(HttpContext.Session);

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
