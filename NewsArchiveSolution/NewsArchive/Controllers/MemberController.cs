using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NewsArchive.Auth;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize(AppRoles.Reader, AppRoles.Author)]
    public class MemberController : AppController
    {
        private readonly IUserService userService;
        private readonly IAuthService authService;
        private readonly PasswordChanger passwordChanger;
        private readonly IStringLocalizer<SharedResource> L;

        public MemberController(IUserService userService, IAuthService authService, PasswordChanger passwordChanger, IStringLocalizer<SharedResource> localizer)
        {
            L = localizer;
            this.userService = userService;
            this.authService = authService;
            this.passwordChanger = passwordChanger;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var user = userService.GetByEmail(CurrentEmail);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(UserDTO obj)
        {
            var existing = userService.Get(CurrentUserId ?? 0);
            if (existing == null) return NotFound();

            if (string.IsNullOrWhiteSpace(obj.Name) || obj.Name.Trim().Length is < 2 or > 100)
            {
                Flash("danger", "Name must be between 2 and 100 characters");
                return View(existing);
            }

            existing.Name = obj.Name;
            existing.Bio = obj.Bio;
            if (userService.Update(existing))
            {
                Flash("success", "User updated");
                return RedirectToAction("Profile", "Member");
            }

            Flash("danger", "Update failed");
            return View(existing);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var user = userService.GetByEmail(CurrentEmail);
            if (user == null) return NotFound();
            ViewBag.ResetVerified = PasswordChanger.IsResetVerified(HttpContext.Session);
            return View(new ChangePasswordDTO { UserId = user.UserId, Email = user.Email });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO obj)
        {
            var userId = CurrentUserId ?? 0;
            obj.UserId = userId;
            obj.Email = CurrentEmail;
            ViewBag.ResetVerified = PasswordChanger.IsResetVerified(HttpContext.Session);
            if (!ModelState.IsValid) return View(obj);

            switch (await passwordChanger.ChangeAsync(HttpContext.Session, userId, CurrentEmail, obj))
            {
                case PasswordChanger.Outcome.Changed:
                    var refreshed = authService.GetUser(userId);
                    if (refreshed != null) await SignInUserAsync(refreshed);
                    Flash("success", "Password updated");
                    return RedirectToAction("Profile", "Member");
                case PasswordChanger.Outcome.WrongCurrentPassword:
                    ModelState.AddModelError(nameof(obj.CurrentPassword), L["Your current password is not correct."]);
                    return View(obj);
                case PasswordChanger.Outcome.Locked:
                    Flash("danger", "Too many failed attempts. Try again in 15 minutes.");
                    return View(obj);
                default:
                    Flash("danger", "Update failed");
                    return View(obj);
            }
        }
    }
}
