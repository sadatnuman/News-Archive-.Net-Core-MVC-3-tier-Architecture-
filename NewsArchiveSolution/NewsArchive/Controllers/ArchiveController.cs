using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Seo;

namespace NewsArchive.Controllers
{
    public class ArchiveController : AppController
    {
        private const int PageSize = 12;

        private readonly ArticleService articleService;

        public ArchiveController(ArticleService articleService)
        {
            this.articleService = articleService;
        }

        [HttpGet("archive")]
        public IActionResult Index()
        {
            return View(articleService.GetArchiveMonths(CurrentLanguage));
        }

        [HttpGet("archive/{year:int}")]
        public IActionResult Year(int year)
        {
            if (!ArticleService.IsValidArchiveDate(year)) return NotFound();

            var months = articleService.GetArchiveMonths(CurrentLanguage).Where(m => m.Year == year).ToList();
            ViewBag.Year = year;
            ViewBag.Years = articleService.GetArchiveMonths(CurrentLanguage).Select(m => m.Year).Distinct().ToList();
            return View(months);
        }

        [HttpGet("archive/{year:int}/{month:int}/{day:int?}")]
        public IActionResult Period(int year, int month, int? day, int page = 1)
        {
            if (!ArticleService.IsValidArchiveDate(year, month, day)) return NotFound();

            var canonical = SeoUrls.ArchivePath(year, month, day);
            if (!string.Equals(Request.Path.Value, canonical, StringComparison.Ordinal))
                return RedirectPermanent(SeoUrls.Archive(year, month, day, page: page));

            page = Math.Max(page, 1);
            var articles = articleService.GetArchive(year, month, day, CurrentLanguage, page, PageSize, out var total);
            if (page > 1 && articles.Count == 0) return NotFound();

            var months = articleService.GetArchiveMonths(CurrentLanguage);
            var key = year * 12 + month;
            var newer = months.Where(m => m.Year * 12 + m.Month > key).OrderBy(m => m.Year * 12 + m.Month).FirstOrDefault();
            var older = months.Where(m => m.Year * 12 + m.Month < key).OrderByDescending(m => m.Year * 12 + m.Month).FirstOrDefault();

            SetArticleLookups(articles);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.Day = day;
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Total = total;
            ViewBag.DayCounts = articleService.GetArchiveDayCounts(year, month, CurrentLanguage);
            ViewBag.Newer = newer;
            ViewBag.Older = older;
            return View(articles);
        }
    }
}
