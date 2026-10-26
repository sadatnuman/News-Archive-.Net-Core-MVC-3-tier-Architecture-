using BLL;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    public class CommentController : AppController
    {
        private readonly ICommentService commentService;

        public CommentController(ICommentService commentService)
        {
            this.commentService = commentService;
        }

        [CustomAuthorize]
        [HttpPost]
        public IActionResult Delete(int id, int articleId)
        {
            var comment = commentService.Get(id);
            if (comment != null && (comment.UserId == CurrentUserId || CurrentRoleId == AppRoles.Admin))
                commentService.Delete(id);

            return RedirectToAction("Details", "Article", new { id = articleId });
        }
    }
}
