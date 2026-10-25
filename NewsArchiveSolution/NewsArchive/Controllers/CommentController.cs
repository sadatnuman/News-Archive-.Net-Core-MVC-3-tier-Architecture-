using BLL;
using NewsArchive.Filters;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace App.Controllers
{
    public class CommentController : Controller
    {
        CommentService commentService;
        UserService userService;

        public CommentController(CommentService commentService, UserService userService)
        {
            this.commentService = commentService;
            this.userService = userService;
        }

        [Logged]
        [HttpPost]
        public IActionResult Delete(int id, int articleId)
        {
            var email = HttpContext.Session.GetString("Email");
            var roleId = HttpContext.Session.GetInt32("RoleId");

            if (string.IsNullOrEmpty(email) || !roleId.HasValue)
                return RedirectToAction("Login", "Auth");

            var user = userService.GetByEmail(email);
            if (user == null)
                return RedirectToAction("Details", "Article", new { id = articleId });

            var comment = commentService.Get(id);
            if (comment == null)
                return RedirectToAction("Details", "Article", new { id = articleId });

            if (comment.UserId != user.UserId && roleId != 1)
                return RedirectToAction("Details", "Article", new { id = articleId });

            commentService.Delete(id);
            return RedirectToAction("Details", "Article", new { id = articleId });
        }
    }
}
