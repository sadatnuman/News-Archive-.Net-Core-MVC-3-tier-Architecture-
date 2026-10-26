using BLL;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NewsArchive.Auth;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize]
    public class CommentController : AppController
    {
        private readonly ICommentService commentService;

        public CommentController(ICommentService commentService)
        {
            this.commentService = commentService;
        }

        [HttpPost]
        public IActionResult Delete(int id, int articleId)
        {
            var comment = commentService.Get(id);
            if (comment == null)
                Flash("danger", "This comment no longer exists.");
            else if (comment.UserId != CurrentUserId && CurrentRoleId != AppRoles.Admin)
                Flash("danger", "You can only delete your own comments.");
            else if (commentService.Delete(id))
                Flash("success", "Comment deleted.");
            else
                Flash("danger", "The comment could not be deleted.");

            return RedirectToAction("Details", "Article", new { id = comment?.ArticleId ?? articleId });
        }

        [HttpPost]
        [EnableRateLimiting(ApiOptions.ReportRateLimit)]
        public IActionResult Report(int id, int reasonId, string? note)
        {
            var comment = commentService.Get(id);
            var result = commentService.Report(id, CurrentUserId ?? 0, reasonId, note);
            switch (result)
            {
                case ReportResult.Reported:
                    Flash("success", "Thank you. A moderator will review this comment.");
                    break;
                case ReportResult.AutoHidden:
                    Flash("success", "Thank you. The comment has been hidden until a moderator reviews it.");
                    break;
                case ReportResult.AlreadyReported:
                    Flash("warning", "You have already reported this comment.");
                    break;
                case ReportResult.OwnComment:
                    Flash("danger", "You cannot report your own comment.");
                    break;
                case ReportResult.InvalidReason:
                    Flash("danger", "Please choose a reason for the report.");
                    break;
                default:
                    Flash("danger", "This comment no longer exists.");
                    break;
            }

            if (comment == null) return RedirectToAction("Index", "Article");
            return Redirect(Url.Action("Details", "Article", new { id = comment.ArticleId }) + "#comments-heading");
        }
    }
}
