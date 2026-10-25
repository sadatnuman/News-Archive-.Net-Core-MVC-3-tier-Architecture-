using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace NewsArchive.Controllers
{
    public abstract class AppController : Controller
    {
        protected int? CurrentRoleId => HttpContext.Session.GetInt32("RoleId");

        protected string? CurrentEmail => HttpContext.Session.GetString("Email");

        protected int? CurrentUserId
        {
            get
            {
                var id = HttpContext.Session.GetInt32("UserId");
                if (id.HasValue || string.IsNullOrEmpty(CurrentEmail)) return id;

                var user = HttpContext.RequestServices.GetRequiredService<UserService>().GetByEmail(CurrentEmail);
                if (user == null) return null;
                HttpContext.Session.SetInt32("UserId", user.UserId);
                return user.UserId;
            }
        }

        protected void SetArticleLookups(IEnumerable<BLL.DTOs.ArticleDTO> articles)
        {
            var services = HttpContext.RequestServices;
            var categories = services.GetRequiredService<CategoryService>().Get();
            ViewBag.AuthorNames = services.GetRequiredService<UserService>().GetNames(articles.Select(a => a.AuthorId));
            ViewBag.CategoryNames = categories.ToDictionary(c => c.CategoryId, c => c.Name);
            ViewBag.CategorySlugs = categories.ToDictionary(c => c.CategoryId, c => c.Slug);
        }

        protected void Flash(string cssClass, string message)
        {
            TempData["Class"] = cssClass;
            TempData["Msg"] = message;
        }
    }
}
