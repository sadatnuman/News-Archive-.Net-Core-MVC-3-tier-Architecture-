using BLL.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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

        protected void SetArticleLookups(ArticleLookupsDTO lookups)
        {
            ViewBag.AuthorNames = lookups.AuthorNames;
            ViewBag.CategoryNames = lookups.CategoryNames;
            ViewBag.CategorySlugs = lookups.CategorySlugs;
            if (lookups.SavedIds != null)
                ViewBag.SavedIds = lookups.SavedIds;
        }

        protected void Flash(string cssClass, string message)
        {
            TempData["Class"] = cssClass;
            TempData["Msg"] = message;
        }
    }
}
