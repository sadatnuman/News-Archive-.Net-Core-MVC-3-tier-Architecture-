using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize(AppRoles.Reader, AppRoles.Author)]
    public class MemberController : AppController
    {
        private readonly IUserService userService;
        private readonly IAuthService authService;

        public MemberController(IUserService userService, IAuthService authService)
        {
            this.userService = userService;
            this.authService = authService;
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
            return View(new ChangePasswordDTO { UserId = user.UserId, Email = user.Email });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO obj)
        {
            var userId = CurrentUserId ?? 0;
            obj.UserId = userId;
            obj.Email = CurrentEmail;
            if (!ModelState.IsValid) return View(obj);

            if (authService.ChangePassword(userId, obj.PasswordHash))
            {
                await authService.RevokeAllAsync(userId);
                var refreshed = authService.GetUser(userId);
                if (refreshed != null) await SignInUserAsync(refreshed);
                Flash("success", "Password updated");
                return RedirectToAction("Profile", "Member");
            }

            Flash("danger", "Update failed");
            return View(obj);
        }
    }
}
