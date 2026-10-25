using BLL;
using NewsArchive.Filters;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace NewsArchive.Controllers
{
    public class CategoryController : AppController
    {
        CategoryService categoryService;
        ArticleService articleService;

        public CategoryController(CategoryService categoryService, ArticleService articleService)
        {
            this.categoryService = categoryService;
            this.articleService = articleService;
        }

        public IActionResult Index()
        {
            var data = categoryService.Get();
            return View(data);
        }

        [HttpGet("category/{id:int}/{slug?}", Order = 0)]
        [HttpGet("Category/ArticlesByCategory/{id:int}", Order = 1)]
        public IActionResult ArticlesByCategory(int id, string? slug)
        {
            var category = categoryService.Get(id);
            if (category == null)
                return NotFound();

            if (slug != category.Slug)
                return RedirectPermanent(Seo.SeoUrls.Category(category.CategoryId, category.Slug));

            var articles = articleService.GetByCategoryId(id);
            SetArticleLookups(articles);

            ViewBag.Category = category;
            return View(articles);
        }

        [RoleAccess(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Create()
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            return View(new CategoryDTO());
        }

        [RoleAccess(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Create(CategoryDTO c)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                var res = categoryService.Create(c);
                if (res == true)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(c);
        }

        [RoleAccess(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            var data = categoryService.Get(id);
            return View(data);
        }

        [RoleAccess(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Edit(CategoryDTO c)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                var res = categoryService.Update(c);
                if (res == true)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(c);
        }

        [RoleAccess(AppRoles.Admin)]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            var data = categoryService.Get(id);
            return View(data);
        }

        [RoleAccess(AppRoles.Admin)]
        [HttpPost]
        public IActionResult Delete(int id, string Decision)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            if (Decision == "Yes")
            {
                if (categoryService.Delete(id))
                    Flash("success", "Category deleted.");
                else
                    Flash("danger", "This category still has articles. Move or delete them first.");
            }
            return RedirectToAction("Index");
        }
    }
}