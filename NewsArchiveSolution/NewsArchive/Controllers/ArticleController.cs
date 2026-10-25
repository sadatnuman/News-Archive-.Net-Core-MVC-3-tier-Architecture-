using BLL;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NewsArchive.Auth;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    public class ArticleController : AppController
    {
        ArticleService articleService;
        CategoryService categoryService;
        CommentService commentService;
        UserService userService;
        TagService tagService;

        public ArticleController(ArticleService articleService, CategoryService categoryService, CommentService commentService, UserService userService, TagService tagService)
        {
            this.tagService = tagService;
            this.articleService = articleService;
            this.categoryService = categoryService;
            this.commentService = commentService;
            this.userService = userService;
        }

        private void SetFormData()
        {
            ViewBag.Categories = categoryService.Get();
            ViewBag.Tags = tagService.Get();
            ViewBag.IsAdmin = CurrentRoleId == AppRoles.Admin;
        }

        [HttpGet("", Order = 0)]
        [HttpGet("search", Order = 1)]
        [HttpGet("Article", Order = 2)]
        [HttpGet("Article/Index", Order = 3)]
        public IActionResult Index(string? keyword = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 9)
        {
            var isSearch = !string.IsNullOrWhiteSpace(keyword) || from.HasValue || to.HasValue;
            if (isSearch && Request.Path == "/")
                return RedirectPermanent("/search" + Request.QueryString);

            int total;
            var data = articleService.Search(keyword, from, to, page, pageSize, out total, CurrentLanguage);
            var trending = isSearch ? new List<ArticleDTO>() : articleService.GetTrendingArticles(5, CurrentLanguage);
            SetArticleLookups(data.Concat(trending));

            ViewBag.IsSearch = isSearch;
            ViewBag.Trending = trending;
            ViewBag.PopularTags = isSearch ? new List<TagDTO>() : tagService.GetPopular(12, CurrentLanguage);
            ViewBag.Keyword = keyword;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Page = page < 1 ? 1 : page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return View(isSearch ? "Search" : "Index", data);
        }

        [HttpGet("breaking-news", Order = 0)]
        [HttpGet("Article/BreakingNews", Order = 1)]
        public IActionResult BreakingNews()
        {
            var data = articleService.GetBreakingNews(CurrentLanguage);
            SetArticleLookups(data);

            ViewBag.Heading = "Breaking News";
            ViewBag.Intro = "The most important developing stories, updated as they happen.";
            ViewBag.CanonicalPath = Seo.SeoUrls.Page("/breaking-news");
            return View("List", data);
        }

        [HttpGet("trending", Order = 0)]
        [HttpGet("Article/TrendingNews", Order = 1)]
        public IActionResult TrendingNews()
        {
            var trending = articleService.GetTrendingArticles(12, CurrentLanguage);
            SetArticleLookups(trending);

            ViewBag.Heading = "Trending News";
            ViewBag.Intro = "The stories readers are viewing and discussing the most right now.";
            ViewBag.CanonicalPath = Seo.SeoUrls.Page("/trending");
            return View("List", trending);
        }

        [HttpGet("news/{id:int}/{slug?}", Order = 0)]
        [HttpGet("Article/Details/{id:int}", Order = 1)]
        public IActionResult Details(int id, string? slug)
        {
            var article = articleService.GetWithDetails(id);
            var userId = CurrentUserId;
            var roleId = CurrentRoleId;
            if (article == null || !ArticleService.CanView(article, userId, roleId))
                return NotFound();

            if (article.IsPublished && slug != article.Slug)
                return RedirectPermanent(Seo.SeoUrls.Article(article));

            if (article.IsPublished && userId.HasValue)
                articleService.IncrementViewCount(id);

            var comments = article.IsPublished ? commentService.GetByArticleId(id) : new List<CommentDTO>();
            var users = userService.GetNames(comments.Select(c => c.UserId).Append(article.AuthorId));

            ViewBag.Comments = comments;
            ViewBag.AuthorName = users.TryGetValue(article.AuthorId, out var authorName) ? authorName : L["Staff reporter"].Value;
            ViewBag.Category = categoryService.Get(article.CategoryId);
            ViewBag.CommentUserNames = comments.ToDictionary(
                c => c.CommentId,
                c => users.TryGetValue(c.UserId, out var name) ? name : L["Reader"].Value);
            ViewBag.UserId = userId;
            ViewBag.RoleId = roleId;
            ViewBag.CanEdit = ArticleService.CanEdit(article.AuthorId, article.StatusId, userId ?? 0, roleId);
            var recommendations = article.IsPublished
                ? articleService.GetRecommendedArticles(id, 4)
                : new List<ArticleDTO>();
            var latest = articleService.GetLatest(6, article.LanguageCode).Where(a => a.ArticleId != id).Take(5).ToList();
            SetArticleLookups(recommendations.Concat(latest));
            ViewBag.Recommendations = recommendations;
            ViewBag.Latest = latest;

            return View(article);
        }

        [CustomAuthorize(AppRoles.Admin, AppRoles.Author)]
        [HttpGet]
        public IActionResult Create()
        {
            SetFormData();
            return View(new ArticleFormDTO { LanguageCode = CurrentLanguage });
        }

        [CustomAuthorize(AppRoles.Admin, AppRoles.Author)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ArticleFormDTO form, string? submitAction)
        {
            var userId = CurrentUserId;
            var roleId = CurrentRoleId;
            if (ModelState.IsValid && userId.HasValue && roleId.HasValue)
            {
                if (articleService.Create(form, userId.Value, roleId.Value, submitAction == "submit") > 0)
                {
                    if (roleId == AppRoles.Admin)
                    {
                        Flash("success", "Article published.");
                        return RedirectToAction("Index");
                    }
                    Flash("success", submitAction == "submit" ? "Article sent to the admin for approval." : "Draft saved.");
                    return RedirectToAction("MyArticles");
                }
                ModelState.AddModelError(string.Empty, L["The article could not be saved."]);
            }

            SetFormData();
            return View(form);
        }

        [CustomAuthorize(AppRoles.Admin, AppRoles.Author)]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var form = articleService.GetForm(id);
            var article = articleService.Get(id);
            if (form == null || article == null) return NotFound();
            if (!ArticleService.CanEdit(article.AuthorId, article.StatusId, CurrentUserId ?? 0, CurrentRoleId))
                return NotFound();

            SetFormData();
            return View(form);
        }

        [CustomAuthorize(AppRoles.Admin, AppRoles.Author)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ArticleFormDTO form, string? submitAction)
        {
            var userId = CurrentUserId;
            var roleId = CurrentRoleId;
            if (ModelState.IsValid && userId.HasValue && roleId.HasValue)
            {
                if (articleService.Update(id, form, userId.Value, roleId.Value, submitAction == "submit"))
                {
                    if (roleId == AppRoles.Admin)
                    {
                        Flash("success", "Article updated.");
                        return RedirectToAction("Details", new { id });
                    }
                    Flash("success", submitAction == "submit" ? "Changes sent to the admin for approval." : "Draft saved.");
                    return RedirectToAction("MyArticles");
                }
                ModelState.AddModelError(string.Empty, L["You cannot edit this article."]);
            }

            form.ArticleId = id;
            SetFormData();
            return View(form);
        }

        [CustomAuthorize(AppRoles.Admin, AppRoles.Author)]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var data = articleService.Get(id);
            if (data == null) return NotFound();
            if (!ArticleService.CanEdit(data.AuthorId, data.StatusId, CurrentUserId ?? 0, CurrentRoleId))
                return NotFound();
            return View(data);
        }

        [CustomAuthorize(AppRoles.Admin, AppRoles.Author)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string Decision)
        {
            if (Decision == "Yes")
            {
                if (articleService.Delete(id, CurrentUserId ?? 0, CurrentRoleId ?? 0))
                    Flash("success", "Article deleted.");
                else
                    Flash("danger", "The article could not be deleted.");
            }
            return CurrentRoleId == AppRoles.Author ? RedirectToAction("MyArticles") : RedirectToAction("Index");
        }

        [CustomAuthorize(AppRoles.Author)]
        [HttpGet]
        public IActionResult MyArticles()
        {
            var articles = articleService.GetByAuthor(CurrentUserId ?? 0);
            ViewBag.CategoryNames = categoryService.Get().ToDictionary(c => c.CategoryId, c => c.Name);
            return View(articles);
        }

        [CustomAuthorize(AppRoles.Author)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int id)
        {
            if (articleService.Submit(id, CurrentUserId ?? 0))
                Flash("success", "Article sent to the admin for approval.");
            else
                Flash("danger", "Only drafts and rejected articles can be submitted.");
            return RedirectToAction("MyArticles");
        }

        [CustomAuthorize]
        [HttpPost]
        [EnableRateLimiting(ApiOptions.CommentRateLimit)]
        public IActionResult AddComment(int articleId, string commentText)
        {
            var userId = CurrentUserId;
            var article = articleService.Get(articleId);
            if (userId == null)
                return RedirectToAction("Login", "Auth");
            if (article == null || !article.IsPublished)
                return NotFound();

            var created = commentService.Create(new CommentDTO
            {
                ArticleId = articleId,
                UserId = userId.Value,
                Text = commentText,
                CreatedDate = DateTime.Now
            });
            if (!created)
                Flash("danger", "Comments must be between 2 and 500 characters.");

            return RedirectToAction("Details", new { id = articleId });
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return RedirectPermanent("/privacy");
        }

    }
}
