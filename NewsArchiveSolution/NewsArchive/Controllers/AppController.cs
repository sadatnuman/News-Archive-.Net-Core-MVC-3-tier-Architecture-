using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NewsArchive.Auth;
using NewsArchive.Seo;

namespace NewsArchive.Controllers
{
    public abstract class AppController : Controller
    {
        protected int? CurrentRoleId => User.RoleId();

        protected string? CurrentEmail => User.Email();

        protected int? CurrentUserId => User.UserId();

        protected string CurrentLanguage => UiLanguage.Current;

        protected IStringLocalizer L => HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

        protected async Task SignInUserAsync(AuthUserDTO user)
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                AuthClaims.CreatePrincipal(user),
                new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Name", user.Name);
        }

        protected void SetArticleLookups(IEnumerable<ArticleDTO> articles)
        {
            var services = HttpContext.RequestServices;
            var categories = services.GetRequiredService<CategoryService>().Get();
            ViewBag.AuthorNames = services.GetRequiredService<UserService>().GetNames(articles.Select(a => a.AuthorId));
            ViewBag.CategoryNames = categories.ToDictionary(c => c.CategoryId, c => c.LocalName(CurrentLanguage));
            ViewBag.CategorySlugs = categories.ToDictionary(c => c.CategoryId, c => c.Slug);
        }

        protected void Flash(string cssClass, string message)
        {
            TempData["Class"] = cssClass;
            TempData["Msg"] = L[message].Value;
        }
    }
}
