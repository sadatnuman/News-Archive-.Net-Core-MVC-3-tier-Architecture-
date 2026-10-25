using BLL;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize]
    public class AvatarController : AppController
    {
        private readonly UserService userService;
        private readonly AuthService authService;
        private readonly MediaService mediaService;

        public AvatarController(UserService userService, AuthService authService, MediaService mediaService)
        {
            this.userService = userService;
            this.authService = authService;
            this.mediaService = mediaService;
        }

        [HttpPost]
        [RequestSizeLimit(MediaService.MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MediaService.MaxRequestBytes)]
        public async Task<IActionResult> Upload(IFormFile? photo)
        {
            var userId = CurrentUserId ?? 0;
            var user = userService.Get(userId);
            if (user == null) return NotFound();

            if (photo == null || photo.Length == 0)
            {
                Flash("danger", "Choose an image to upload.");
                return BackToProfile();
            }

            using var stream = photo.OpenReadStream();
            var result = mediaService.SaveAvatar(stream, photo.Length);
            if (!result.Success)
            {
                Flash("danger", result.Error!);
                return BackToProfile();
            }

            if (!userService.SetAvatar(userId, result.Path))
            {
                mediaService.DeleteIfUploaded(result.Path);
                Flash("danger", "Update failed");
                return BackToProfile();
            }

            mediaService.DeleteIfUploaded(user.AvatarPath);
            await RefreshSignInAsync(userId);
            Flash("success", "Profile picture updated.");
            return BackToProfile();
        }

        [HttpPost]
        public async Task<IActionResult> Remove()
        {
            var userId = CurrentUserId ?? 0;
            var user = userService.Get(userId);
            if (user == null) return NotFound();

            if (userService.SetAvatar(userId, null))
            {
                mediaService.DeleteIfUploaded(user.AvatarPath);
                await RefreshSignInAsync(userId);
                Flash("success", "Profile picture removed.");
            }
            return BackToProfile();
        }

        private async Task RefreshSignInAsync(int userId)
        {
            var refreshed = authService.GetUser(userId);
            if (refreshed != null) await SignInUserAsync(refreshed);
        }

        private IActionResult BackToProfile()
        {
            return CurrentRoleId == AppRoles.Admin
                ? RedirectToAction("Profile", "Admin")
                : RedirectToAction("Profile", "Member");
        }
    }
}
