using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize]
    public class HistoryController : AppController
    {
        private const int PageSize = 12;

        private readonly IRecommendationService recommendationService;
        private readonly IArticleLookupService lookupService;

        public HistoryController(IRecommendationService recommendationService, IArticleLookupService lookupService)
        {
            this.recommendationService = recommendationService;
            this.lookupService = lookupService;
        }

        private bool WantsJson => Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

        [HttpGet("history")]
        public IActionResult Index(int page = 1)
        {
            page = Math.Max(page, 1);
            var entries = recommendationService.GetHistory(CurrentUserId ?? 0, page, PageSize, out var total);
            var forYou = page == 1
                ? recommendationService.ForReader(CurrentUserId, null, CurrentLanguage, 4)
                : new List<BLL.DTOs.RecommendationDTO>();
            SetArticleLookups(lookupService.Build(entries.Select(e => e.Article).Concat(forYou.Select(r => r.Article)), CurrentUserId, CurrentLanguage));
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Total = total;
            ViewBag.ForYou = forYou;
            return View(entries);
        }

        [HttpPost("history/remove")]
        public IActionResult Remove(int articleId)
        {
            var removed = recommendationService.RemoveFromHistory(CurrentUserId ?? 0, articleId);
            if (WantsJson) return removed ? Json(new { removed }) : NotFound(new { removed });
            if (removed)
                Flash("success", "Story removed from your reading history.");
            else
                Flash("danger", "This story is not in your reading history.");
            return RedirectToAction("Index");
        }

        [HttpPost("history/clear")]
        public IActionResult Clear()
        {
            var count = recommendationService.ClearHistory(CurrentUserId ?? 0);
            if (WantsJson) return Json(new { removed = count });
            Flash("success", "Your reading history has been cleared.");
            return RedirectToAction("Index");
        }
    }
}
