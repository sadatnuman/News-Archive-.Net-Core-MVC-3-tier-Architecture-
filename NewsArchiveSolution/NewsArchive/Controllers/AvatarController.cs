using BLL;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize]
    public class AvatarController : AppController
    {
        private readonly IUserService userService;
        private readonly IAuthService authService;
        private readonly IMediaService mediaService;

        public AvatarController(IUserService userService, IAuthService authService, IMediaService mediaService)
        {
            this.userService = userService;
            this.authService = authService;
            this.mediaService = mediaService;
        }

        [HttpPost]
        [RequestSizeLimit(MediaLimits.MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MediaLimits.MaxRequestBytes)]
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

        private RedirectToActionResult BackToProfile()
        {
            return CurrentRoleId == AppRoles.Admin
                ? RedirectToAction("Profile", "Admin")
                : RedirectToAction("Profile", "Member");
        }
    }
}
