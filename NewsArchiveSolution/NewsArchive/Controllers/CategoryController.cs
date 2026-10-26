using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NewsArchive.Filters;
using NewsArchive.Seo;

namespace NewsArchive.Controllers
{
    public class CategoryController : AppController
    {
        private const int SectionPageSize = 13;

        private readonly IStringLocalizer<SharedResource> L;
        private readonly IArticleLookupService lookupService;
        private readonly ICategoryService categoryService;
        private readonly IArticleService articleService;

        public CategoryController(ICategoryService categoryService, IArticleService articleService, IArticleLookupService lookupService, IStringLocalizer<SharedResource> localizer)
        {
            L = localizer;
            this.lookupService = lookupService;
            this.categoryService = categoryService;
            this.articleService = articleService;
        }

        public IActionResult Index()
        {
            var data = categoryService.Get();
            ViewBag.ArticleCounts = categoryService.CountPublishedPerSection(CurrentLanguage);
            return View(data);
        }

        [HttpGet("category/{id:int}/{slug?}", Order = 0)]
        [HttpGet("Category/ArticlesByCategory/{id:int}", Order = 1)]
        public IActionResult ArticlesByCategory(int id, string? slug, int page = 1)
        {
            var category = categoryService.Get(id);
            if (category == null)
                return NotFound();

            if (slug != category.Slug)
                return RedirectPermanent(SeoUrls.Category(category.CategoryId, category.Slug, page: page));

            page = Math.Max(page, 1);
            var articles = articleService.GetByCategoryId(id, CurrentLanguage, page, SectionPageSize, out var total);
            if (page > 1 && articles.Count == 0) return NotFound();
            SetArticleLookups(lookupService.Build(articles, CurrentUserId, CurrentLanguage));

            ViewBag.Category = category;
            ViewBag.Page = page;
            ViewBag.PageSize = SectionPageSize;
            ViewBag.Total = total;
            return View(articles);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryDTO());
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Create(CategoryDTO c)
        {
            if (!string.IsNullOrWhiteSpace(c.Name) && categoryService.NameExists(c.Name, 0))
                ModelState.AddModelError(nameof(c.Name), L["A section with this name already exists."]);

            if (ModelState.IsValid && categoryService.Create(c))
            {
                Flash("success", "Section created.");
                return RedirectToAction("Index");
            }
            return View(c);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var data = categoryService.Get(id);
            if (data == null) return NotFound();
            return View(data);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Edit(CategoryDTO c)
        {
            if (!string.IsNullOrWhiteSpace(c.Name) && categoryService.NameExists(c.Name, c.CategoryId))
                ModelState.AddModelError(nameof(c.Name), L["A section with this name already exists."]);

            if (ModelState.IsValid && categoryService.Update(c))
            {
                Flash("success", "Section updated.");
                return RedirectToAction("Index");
            }
            return View(c);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var data = categoryService.Get(id);
            if (data == null) return NotFound();

            ViewBag.ArticleCount = categoryService.CountArticles(id);
            ViewBag.OtherCategories = categoryService.Get().Where(c => c.CategoryId != id).ToList();
            return View(data);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Delete(int id, string Decision, int? moveArticlesTo)
        {
            if (Decision == "Yes")
            {
                if (categoryService.Delete(id, moveArticlesTo))
                    Flash("success", "Section deleted.");
                else
                    Flash("danger", "Choose a section to move this section's articles to.");
            }
            return RedirectToAction("Index");
        }
    }
}
