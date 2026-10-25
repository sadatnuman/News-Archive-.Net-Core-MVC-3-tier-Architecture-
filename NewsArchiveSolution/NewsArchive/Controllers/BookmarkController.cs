using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize]
    public class BookmarkController : AppController
    {
        private const int PageSize = 12;

        private readonly BookmarkService bookmarkService;

        public BookmarkController(BookmarkService bookmarkService)
        {
            this.bookmarkService = bookmarkService;
        }

        [HttpGet("bookmarks")]
        public IActionResult Index(int page = 1)
        {
            page = Math.Max(page, 1);
            var articles = bookmarkService.GetSaved(CurrentUserId ?? 0, page, PageSize, out var total);
            SetArticleLookups(articles);
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Total = total;
            return View(articles);
        }

        [HttpPost("bookmarks/toggle")]
        public IActionResult Toggle(int articleId, string? returnUrl)
        {
            var ok = bookmarkService.Toggle(CurrentUserId ?? 0, articleId, out var saved);
            var wantsJson = Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

            if (wantsJson)
                return ok ? Json(new { saved }) : BadRequest(new { saved = false });

            if (!ok)
                Flash("danger", "This story could not be saved.");
            else
                Flash("success", saved ? "Story saved." : "Story removed from your saved list.");

            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index");
        }
    }
}
