using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NewsArchive.Filters;
using NewsArchive.Seo;

namespace NewsArchive.Controllers
{
    public class TagController : AppController
    {
        private const int PageSize = 12;

        private readonly IStringLocalizer<SharedResource> L;
        private readonly IArticleLookupService lookupService;
        private readonly ITagService tagService;

        public TagController(ITagService tagService, IArticleLookupService lookupService, IStringLocalizer<SharedResource> localizer)
        {
            L = localizer;
            this.lookupService = lookupService;
            this.tagService = tagService;
        }

        [HttpGet("tags", Order = 0)]
        [HttpGet("Tag", Order = 1)]
        [HttpGet("Tag/Index", Order = 2)]
        public IActionResult Index()
        {
            var tags = tagService.GetWithCounts(CurrentLanguage);
            if (CurrentRoleId != AppRoles.Admin)
                tags = tags.Where(t => t.ArticleCount > 0).ToList();
            return View(tags.OrderByDescending(t => t.ArticleCount).ThenBy(t => t.LocalName(CurrentLanguage)).ToList());
        }

        [HttpGet("tag/{id:int}/{slug?}")]
        public IActionResult Articles(int id, string? slug, int page = 1)
        {
            var tag = tagService.Get(id);
            if (tag == null) return NotFound();

            if (slug != tag.Slug)
                return RedirectPermanent(SeoUrls.Tag(tag.TagId, tag.Slug, page: page));

            page = Math.Max(page, 1);
            var articles = tagService.GetArticles(id, CurrentLanguage, page, PageSize, out var total);
            if (page > 1 && articles.Count == 0) return NotFound();

            SetArticleLookups(lookupService.Build(articles, CurrentUserId, CurrentLanguage));
            ViewBag.Tag = tag;
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Total = total;
            return View(articles);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TagDTO());
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Create(TagDTO tag)
        {
            ValidateNames(tag, 0);
            if (ModelState.IsValid && tagService.Create(tag))
            {
                Flash("success", "Tag created.");
                return RedirectToAction("Index");
            }
            return View(tag);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var tag = tagService.Get(id);
            return tag == null ? NotFound() : View(tag);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Edit(TagDTO tag)
        {
            if (tagService.Get(tag.TagId) == null) return NotFound();

            ValidateNames(tag, tag.TagId);
            if (ModelState.IsValid && tagService.Update(tag))
            {
                Flash("success", "Tag updated.");
                return RedirectToAction("Index");
            }
            return View(tag);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var tag = tagService.Get(id);
            if (tag == null) return NotFound();
            ViewBag.ArticleCount = tagService.GetWithCounts().FirstOrDefault(t => t.TagId == id)?.ArticleCount ?? 0;
            return View(tag);
        }

        [CustomAuthorize(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Delete(int id, string Decision)
        {
            if (Decision == "Yes")
            {
                if (tagService.Delete(id))
                    Flash("success", "Tag deleted.");
                else
                    Flash("danger", "The tag could not be deleted.");
            }
            return RedirectToAction("Index");
        }

        private void ValidateNames(TagDTO tag, int tagId)
        {
            if (tagService.NameExists(tag.Name, tagId))
                ModelState.AddModelError(nameof(tag.Name), L["A tag with this name already exists."]);
            if (tagService.NameExists(tag.NameBn, tagId))
                ModelState.AddModelError(nameof(tag.NameBn), L["A tag with this name already exists."]);
        }
    }
}
