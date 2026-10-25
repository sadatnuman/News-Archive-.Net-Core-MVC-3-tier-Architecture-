using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Seo;

namespace NewsArchive.Controllers
{
    public class AuthorController : AppController
    {
        private const int PageSize = 12;

        private readonly AuthorService authorService;

        public AuthorController(AuthorService authorService)
        {
            this.authorService = authorService;
        }

        [HttpGet("authors")]
        public IActionResult Index()
        {
            return View(authorService.GetAuthors(CurrentLanguage));
        }

        [HttpGet("author/{id:int}/{slug?}")]
        public IActionResult Profile(int id, string? slug, int page = 1)
        {
            var author = authorService.GetAuthor(id, CurrentLanguage);
            if (author == null) return NotFound();

            if (slug != author.Slug)
                return RedirectPermanent(SeoUrls.Author(author.UserId, author.Name, page: page));

            page = Math.Max(page, 1);
            var articles = authorService.GetArticles(id, CurrentLanguage, page, PageSize, out var total);
            if (page > 1 && articles.Count == 0) return NotFound();

            SetArticleLookups(articles);
            ViewBag.Author = author;
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Total = total;
            return View(articles);
        }
    }
}
