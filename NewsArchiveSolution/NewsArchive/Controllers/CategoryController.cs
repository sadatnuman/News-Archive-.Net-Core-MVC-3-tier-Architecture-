using Auth.AuthFilters;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class CategoryController : Controller
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

        public IActionResult ArticlesByCategory(int id)
        {
            var category = categoryService.Get(id);
            if (category == null)
                return NotFound();

            var articles = articleService.GetByCategoryId(id);

            ViewBag.Category = category;
            return View(articles);
        }

        [AdminAccess]
        [HttpGet]
        public IActionResult Create()
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            return View(new CategoryDTO());
        }

        [AdminAccess]
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

        [AdminAccess]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            var data = categoryService.Get(id);
            return View(data);
        }

        [AdminAccess]
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

        [AdminAccess]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            var data = categoryService.Get(id);
            return View(data);
        }

        [AdminAccess]
        [HttpPost]
        public IActionResult Delete(int id, string Decision)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            if (Decision.Equals("Yes"))
            {
                var res = categoryService.Delete(id);
                if (res == true)
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }
    }
}