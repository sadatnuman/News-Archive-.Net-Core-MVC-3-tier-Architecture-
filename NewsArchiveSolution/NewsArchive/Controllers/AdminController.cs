using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize(AppRoles.Admin)]
    public class AdminController : AppController
    {
        private readonly IStringLocalizer<SharedResource> L;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IAnalyticsService analyticsService;
        private readonly IArticleService articleService;
        private readonly ICategoryService categoryService;
        private readonly IAuthService authService;
        private readonly ICommentService commentService;
        private readonly IAuditService auditService;

        private const int ModerationPageSize = 20;
        private const int AuditPageSize = 30;

        public AdminController(IUserService userService, IRoleService roleService, IAnalyticsService analyticsService, IArticleService articleService, ICategoryService categoryService, IAuthService authService, ICommentService commentService, IAuditService auditService, IStringLocalizer<SharedResource> localizer)
        {
            L = localizer;
            this.commentService = commentService;
            this.auditService = auditService;
            this.authService = authService;
            _userService = userService;
            _roleService = roleService;
            this.analyticsService = analyticsService;
            this.articleService = articleService;
            this.categoryService = categoryService;
        }

        public IActionResult Index()
        {
            ViewBag.TotalArticles = analyticsService.GetTotalArticles();
            ViewBag.TotalUsers = analyticsService.GetTotalUsers();
            ViewBag.PendingCount = articleService.CountPending();
            ViewBag.ReportedCount = commentService.CountReported();
            ViewBag.MostViewed = analyticsService.GetMostViewedArticles(5);
            ViewBag.ArticlesPerCategory = analyticsService.GetArticlesPerCategory();
            return View();
        }

        [HttpGet]
        public IActionResult PendingArticles()
        {
            var pending = articleService.GetPending();
            var scheduled = articleService.GetScheduled();
            ViewBag.AuthorNames = _userService.GetNames(pending.Concat(scheduled).Select(a => a.AuthorId));
            ViewBag.CategoryNames = categoryService.Get().ToDictionary(c => c.CategoryId, c => c.Name);
            ViewBag.Scheduled = scheduled;
            return View(pending);
        }

        [HttpPost]
        public IActionResult Approve(int id, DateTime? publishAt)
        {
            var scheduled = ArticleRules.IsFutureSchedule(publishAt, DateTime.Now);
            if (articleService.Approve(id, CurrentUserId ?? 0, publishAt))
                Flash("success", scheduled ? "Article approved and scheduled." : "Article approved and published.");
            else
                Flash("danger", "Only pending articles can be approved.");
            return RedirectToAction("PendingArticles");
        }

        [HttpPost]
        public IActionResult PublishNow(int id)
        {
            if (articleService.PublishNow(id))
                Flash("success", "Article published.");
            else
                Flash("danger", "Only scheduled articles can be published early.");
            return RedirectToAction("PendingArticles");
        }

        [HttpPost]
        public IActionResult Unschedule(int id)
        {
            if (articleService.Unschedule(id))
                Flash("warning", "Schedule cancelled. The article is back in the review queue.");
            else
                Flash("danger", "Only scheduled articles can be changed.");
            return RedirectToAction("PendingArticles");
        }

        [HttpPost]
        public IActionResult Reject(int id, string? note)
        {
            if (articleService.Reject(id, CurrentUserId ?? 0, note))
                Flash("warning", "Article rejected and returned to the author.");
            else
                Flash("danger", "Only pending articles can be rejected.");
            return RedirectToAction("PendingArticles");
        }

        [HttpGet]
        public IActionResult Comments(string? tab, int page = 1)
        {
            var showHidden = tab == "hidden";
            page = Math.Max(page, 1);
            int total;
            var items = showHidden
                ? commentService.GetHidden(page, ModerationPageSize, out total)
                : commentService.GetReported(page, ModerationPageSize, out total);
            ViewBag.Tab = showHidden ? "hidden" : "reported";
            ViewBag.Page = page;
            ViewBag.PageSize = ModerationPageSize;
            ViewBag.Total = total;
            ViewBag.ReportedCount = showHidden ? commentService.CountReported() : total;
            return View(items);
        }

        [HttpGet]
        public IActionResult Audit(string? type, int? user, int page = 1)
        {
            page = Math.Max(page, 1);
            var entries = auditService.Get(type, user, page, AuditPageSize, out var total);
            ViewBag.Type = BLL.Audit.AuditEntities.All.FirstOrDefault(t => string.Equals(t, type, StringComparison.OrdinalIgnoreCase));
            ViewBag.UserFilter = user;
            ViewBag.UserFilterName = user.HasValue ? _userService.Get(user.Value)?.Name : null;
            ViewBag.Page = page;
            ViewBag.PageSize = AuditPageSize;
            ViewBag.Total = total;
            return View(entries);
        }

        private IActionResult BackTo(string? returnUrl)
        {
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Comments");
        }

        [HttpPost]
        public IActionResult HideComment(int id, string? returnUrl)
        {
            if (commentService.Hide(id, CurrentUserId ?? 0))
                Flash("success", "Comment hidden from readers.");
            else
                Flash("danger", "This comment no longer exists.");
            return BackTo(returnUrl);
        }

        [HttpPost]
        public IActionResult KeepComment(int id, string? returnUrl)
        {
            if (commentService.Keep(id, CurrentUserId ?? 0))
                Flash("success", "Comment kept and reports cleared.");
            else
                Flash("danger", "This comment no longer exists.");
            return BackTo(returnUrl);
        }

        [HttpPost]
        public IActionResult DeleteComment(int id, string? returnUrl)
        {
            if (commentService.Delete(id))
                Flash("success", "Comment deleted.");
            else
                Flash("danger", "This comment no longer exists.");
            return BackTo(returnUrl);
        }

        [HttpGet]
        public IActionResult AllUser()
        {
            ViewBag.CurrentAdminEmail = CurrentEmail;
            return View(_userService.Get());
        }

        [HttpPost]
        public IActionResult AllUser(string searchTerm)
        {
            ViewBag.CurrentAdminEmail = CurrentEmail;
            return View(_userService.Search(searchTerm));
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var user = _userService.GetByEmail(CurrentEmail);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(UserDTO obj)
        {
            var existing = _userService.Get(CurrentUserId ?? 0);
            if (existing == null) return NotFound();

            if (string.IsNullOrWhiteSpace(obj.Name) || obj.Name.Trim().Length is < 2 or > 100)
            {
                Flash("danger", "Name must be between 2 and 100 characters");
                return View(existing);
            }

            existing.Name = obj.Name;
            existing.Bio = obj.Bio;
            if (_userService.Update(existing))
            {
                Flash("success", "User updated");
                return RedirectToAction("Profile", "Admin");
            }

            Flash("danger", "Update failed");
            return View(existing);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var user = _userService.GetByEmail(CurrentEmail);
            if (user == null) return NotFound();
            return View(new ChangePasswordDTO { UserId = user.UserId, Email = user.Email });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO obj)
        {
            var userId = CurrentUserId ?? 0;
            obj.UserId = userId;
            obj.Email = CurrentEmail;
            if (!ModelState.IsValid) return View(obj);

            if (authService.ChangePassword(userId, obj.PasswordHash))
            {
                await authService.RevokeAllAsync(userId);
                var refreshed = authService.GetUser(userId);
                if (refreshed != null) await SignInUserAsync(refreshed);
                Flash("success", "Password updated");
                return RedirectToAction("Profile", "Admin");
            }

            Flash("danger", "Update failed");
            return View(obj);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _roleService.Get();
            return View(new UserDTO());
        }

        [HttpPost]
        public IActionResult Create(UserDTO obj)
        {
            ViewBag.Roles = _roleService.Get();

            if (!ModelState.IsValid)
                return View(obj);

            if (_roleService.Get(obj.RoleId) == null)
            {
                ModelState.AddModelError(nameof(obj.RoleId), L["Please choose a role"]);
                return View(obj);
            }

            if (_userService.Create(obj))
            {
                Flash("success", "User created successfully");
                return RedirectToAction("AllUser", "Admin");
            }

            Flash("danger", "Failed to create user");
            return View(obj);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.Roles = _roleService.Get();
            var user = _userService.Get(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(UserDTO obj)
        {
            ViewBag.Roles = _roleService.Get();

            var existing = _userService.Get(obj.UserId);
            if (existing == null) return NotFound();
            if (existing.UserId == CurrentUserId || _roleService.Get(obj.RoleId) == null)
            {
                Flash("danger", "Update failed");
                return RedirectToAction("AllUser");
            }

            existing.Name = obj.Name;
            existing.RoleId = obj.RoleId;

            if (_userService.Update(existing))
            {
                Flash("success", "User updated");
                return RedirectToAction("AllUser");
            }

            Flash("danger", "Update failed");
            return View(existing);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _userService.Get(id);
            if (user == null) return NotFound();

            var role = _roleService.Get(user.RoleId);
            ViewBag.RoleName = role?.RoleName ?? "Unknown";

            return View(user);
        }

        [HttpPost]
        public IActionResult Delete(int id, string Decision)
        {
            if (Decision != "Yes")
                return RedirectToAction("AllUser");

            if (id == CurrentUserId)
                Flash("danger", "You cannot delete your own account.");
            else if (_userService.Delete(id))
                Flash("success", "User deleted.");
            else
                Flash("danger", "This user has written articles. Reassign or delete them first.");

            return RedirectToAction("AllUser");
        }
    }
}
