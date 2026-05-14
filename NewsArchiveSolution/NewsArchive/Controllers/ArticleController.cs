using Auth.AuthFilters;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace App.Controllers
{
    public class ArticleController : Controller
    {
        ArticleService articleService;
        CategoryService categoryService;
        CommentService commentService;
        UserService userService;

        public ArticleController(ArticleService articleService, CategoryService categoryService, CommentService commentService, UserService userService)
        {
            this.articleService = articleService;
            this.categoryService = categoryService;
            this.commentService = commentService;
            this.userService = userService;
        }

        public IActionResult Index(string keyword = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 9)
        {
            int total;
            var data = articleService.Search(keyword, from, to, page, pageSize, out total);

            var users = userService.Get().ToDictionary(u => u.UserId, u => u.Name);
            var categories = categoryService.Get().ToDictionary(c => c.CategoryId, c => c.Name);

            var authorNames = new Dictionary<int, string>();
            var categoryNames = new Dictionary<int, string>();

            foreach (var article in data)
            {
                if (users.TryGetValue(article.AuthorId, out var authorName))
                    authorNames[article.ArticleId] = authorName;

                if (categories.TryGetValue(article.CategoryId, out var categoryName))
                    categoryNames[article.CategoryId] = categoryName;
            }

            ViewBag.AuthorNames = authorNames;
            ViewBag.CategoryNames = categoryNames;
            ViewBag.Categories = categories;

            ViewBag.Keyword = keyword;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return View(data);
        }

        public IActionResult BreakingNews()
        {
            var data = articleService.Get().Where(a => a.IsBreakingNews).ToList();

            var users = userService.Get().ToDictionary(u => u.UserId, u => u.Name);
            var categories = categoryService.Get().ToDictionary(c => c.CategoryId, c => c.Name);

            var authorNames = new Dictionary<int, string>();
            var categoryNames = new Dictionary<int, string>();

            foreach (var article in data)
            {
                if (users.TryGetValue(article.AuthorId, out var authorName))
                    authorNames[article.ArticleId] = authorName;

                if (categories.TryGetValue(article.CategoryId, out var categoryName))
                    categoryNames[article.CategoryId] = categoryName;
            }

            ViewBag.AuthorNames = authorNames;
            ViewBag.CategoryNames = categoryNames;
            ViewBag.Categories = categories;

            ViewData["Title"] = "Breaking News";
            return View("Index", data);
        }

        public IActionResult Details(int id)
        {
            var email = HttpContext.Session.GetString("Email");
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (!string.IsNullOrEmpty(email) && (roleId == 1 || roleId == 2))
            {
                articleService.IncrementViewCount(id);
            }
            var article = articleService.GetWithDetails(id);
            if (article == null)
                return NotFound();

            var comments = commentService.GetByArticleId(id);

            ViewBag.Article = article;
            ViewBag.Comments = comments;

            var authorName = userService.Get().FirstOrDefault(u => u.UserId == article.AuthorId)?.Name;
            var categoryName = categoryService.Get().FirstOrDefault(c => c.CategoryId == article.CategoryId)?.Name;
            ViewBag.AuthorName = authorName ?? "Unknown Author";
            ViewBag.CategoryName = categoryName ?? "Unknown";

            var commentUserIds = comments?.Select(c => c.UserId).Distinct().ToList() ?? new List<int>();
            var users = userService.Get().Where(u => commentUserIds.Contains(u.UserId)).ToDictionary(u => u.UserId, u => u.Name);
            var commentUserNames = new Dictionary<int, string>();
            if (comments != null)
            {
                foreach (var comm in comments)
                {
                    if (users.TryGetValue(comm.UserId, out var name))
                        commentUserNames[comm.CommentId] = name;
                    else
                        commentUserNames[comm.CommentId] = "Unknown User";
                }
            }
            ViewBag.CommentUserNames = commentUserNames;

            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(email) && roleId.HasValue;
            var userInfo = userService.Get()
                .Where(u => u.Email == email)
                .Select(u => new { u.UserId, u.Name })
                .FirstOrDefault();

            ViewBag.UserId = userInfo?.UserId;
            ViewBag.UserName = userInfo?.Name;
            ViewBag.RoleId = roleId;

            var recommendations = articleService.GetRecommendedArticles(id, 4);
            ViewBag.Recommendations = recommendations;

            return View(article);
        }

        [AdminAccess]
        [HttpGet]
        public IActionResult Create()
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            var email = HttpContext.Session.GetString("Email");
            ViewBag.Categories = categoryService.Get();

            var userInfo = userService.Get()
                .Where(u => u.Email == email)
                .Select(u => new { u.UserId, u.Name })
                .FirstOrDefault();

            ViewBag.UserId = userInfo?.UserId;
            ViewBag.UserName = userInfo?.Name;
            ViewBag.UserEmail = email;

            return View(new ArticleDTO());
        }

        [AdminAccess]
        [HttpPost]
        public IActionResult Create(ArticleDTO a)
        {
            var email = HttpContext.Session.GetString("Email");
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                var user = userService.Get().FirstOrDefault(u => u.Email == email);
                a.AuthorId = user?.UserId ?? 0;
                a.PublishDate = DateTime.Now;

                var res = articleService.Create(a);
                if (res == true)
                {
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Categories = categoryService.Get();

            var userInfo = userService.Get()
                .Where(u => u.Email == email)
                .Select(u => new { u.UserId, u.Name })
                .FirstOrDefault();

            ViewBag.UserEmail = email;
            ViewBag.UserId = userInfo?.UserId;
            ViewBag.UserName = userInfo?.Name;
            ViewBag.RoleId = roleId;

            return View(a);
        }

        [AdminAccess]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            var email = HttpContext.Session.GetString("Email");
            ViewBag.Categories = categoryService.Get();
            var data = articleService.Get(id);

            var userInfo = userService.Get()
                .Where(u => u.Email == email)
                .Select(u => new { u.UserId, u.Name })
                .FirstOrDefault();

            ViewBag.UserId = userInfo?.UserId;
            ViewBag.UserName = userInfo?.Name;
            ViewBag.UserEmail = email;

            return View(data);
        }

        [AdminAccess]
        [HttpPost]
        public IActionResult Edit(ArticleDTO a)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId != 1)
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                var res = articleService.Update(a);
                if (res == true)
                {
                    return RedirectToAction("Index");
                }
            }

            var email = HttpContext.Session.GetString("Email");
            ViewBag.Categories = categoryService.Get();

            var userInfo = userService.Get()
                .Where(u => u.Email == email)
                .Select(u => new { u.UserId, u.Name })
                .FirstOrDefault();

            ViewBag.UserEmail = email;
            ViewBag.UserId = userInfo?.UserId;
            ViewBag.UserName = userInfo?.Name;
            ViewBag.RoleId = roleId;

            return View(a);
        }

        [AdminAccess]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");

            if (roleId != 1)
                return RedirectToAction("Index");

            var data = articleService.Get(id);
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
                var res = articleService.Delete(id);
                if (res == true)
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        [Logged]
        [HttpPost]
        public IActionResult AddComment(int articleId, string commentText)
        {
            var email = HttpContext.Session.GetString("Email");
            var roleId = HttpContext.Session.GetInt32("RoleId");

            if (string.IsNullOrEmpty(email) || !roleId.HasValue)
                return RedirectToAction("Login", "Auth");

            if (roleId != 1 && roleId != 2)
                return RedirectToAction("Details", new { id = articleId });

            var user = userService.Get().FirstOrDefault(u => u.Email == email);
            if (user == null)
                return RedirectToAction("Details", new { id = articleId });

            var comment = new CommentDTO
            {
                ArticleId = articleId,
                UserId = user.UserId,
                Text = commentText,
                CreatedDate = DateTime.Now
            };

            commentService.Create(comment);
            return RedirectToAction("Details", new { id = articleId });
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            Response.StatusCode = 500;
            return View();
        }

        public IActionResult TrendingNews()
        {
            var trending = articleService.GetTrendingArticles(10);
            var users = userService.Get().ToDictionary(u => u.UserId, u => u.Name);
            var categories = categoryService.Get().ToDictionary(c => c.CategoryId, c => c.Name);
            var authorNames = new Dictionary<int, string>();
            var categoryNames = new Dictionary<int, string>();
            foreach (var article in trending)
            {
                if (users.TryGetValue(article.AuthorId, out var authorName))
                    authorNames[article.ArticleId] = authorName;
                if (categories.TryGetValue(article.CategoryId, out var categoryName))
                    categoryNames[article.CategoryId] = categoryName;
            }
            ViewBag.AuthorNames = authorNames;
            ViewBag.CategoryNames = categoryNames;
            ViewBag.Categories = categories;
            ViewData["Title"] = "Trending News";
            return View("Index", trending);
        }
    }
}
