using BLL;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using NewsArchive.Filters;

namespace NewsArchive.Controllers
{
    [CustomAuthorize(AppRoles.Admin)]
    public class AdminController : AppController
    {
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly AnalyticsService analyticsService;
        private readonly ArticleService articleService;
        private readonly CategoryService categoryService;
        private readonly AuthService authService;

        public AdminController(UserService userService, RoleService roleService, AnalyticsService analyticsService, ArticleService articleService, CategoryService categoryService, AuthService authService)
        {
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
            ViewBag.MostViewed = analyticsService.GetMostViewedArticles(5);
            ViewBag.ArticlesPerCategory = analyticsService.GetArticlesPerCategory();
            return View();
        }

        [HttpGet]
        public IActionResult PendingArticles()
        {
            var pending = articleService.GetPending();
            ViewBag.AuthorNames = _userService.GetNames(pending.Select(a => a.AuthorId));
            ViewBag.CategoryNames = categoryService.Get().ToDictionary(c => c.CategoryId, c => c.Name);
            return View(pending);
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            if (articleService.Approve(id, CurrentUserId ?? 0))
                Flash("success", "Article approved and published.");
            else
                Flash("danger", "Only pending articles can be approved.");
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
