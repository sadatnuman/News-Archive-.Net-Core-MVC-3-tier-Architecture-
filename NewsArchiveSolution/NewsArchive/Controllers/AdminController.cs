using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NewsArchive.Auth;
using NewsArchive.Email;
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
        private readonly INewsletterService newsletterService;
        private readonly DigestSender digestSender;
        private readonly MailComposer mail;
        private readonly PasswordChanger passwordChanger;

        private const int ModerationPageSize = 20;
        private const int AuditPageSize = 30;
        private const int SubscriberPageSize = 30;

        public AdminController(IUserService userService, IRoleService roleService, IAnalyticsService analyticsService, IArticleService articleService,
            ICategoryService categoryService, IAuthService authService, ICommentService commentService, IAuditService auditService,
            INewsletterService newsletterService, DigestSender digestSender, MailComposer mail, PasswordChanger passwordChanger, IStringLocalizer<SharedResource> localizer)
        {
            L = localizer;
            this.passwordChanger = passwordChanger;
            this.newsletterService = newsletterService;
            this.digestSender = digestSender;
            this.mail = mail;
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
            var scheduled = ArticleRules.IsFutureSchedule(publishAt, BLL.SiteClock.Now);
            Decided(articleService.Approve(id, CurrentUserId ?? 0, publishAt),
                "success", scheduled ? "Article approved and scheduled." : "Article approved and published.",
                "Only pending articles can be approved.");
            return RedirectToAction("PendingArticles");
        }

        [HttpPost]
        public IActionResult PublishNow(int id)
        {
            Decided(articleService.PublishNow(id), "success", "Article published.", "Only scheduled articles can be published early.");
            return RedirectToAction("PendingArticles");
        }

        [HttpPost]
        public IActionResult Unschedule(int id)
        {
            Decided(articleService.Unschedule(id), "warning", "Schedule cancelled. The article is back in the review queue.", "Only scheduled articles can be changed.");
            return RedirectToAction("PendingArticles");
        }

        [HttpPost]
        public IActionResult Reject(int id, string? note)
        {
            Decided(articleService.Reject(id, CurrentUserId ?? 0, note), "warning", "Article rejected and returned to the author.", "Only pending articles can be rejected.");
            return RedirectToAction("PendingArticles");
        }

        private void Decided(ChangeResult result, string doneClass, string done, string notAllowed)
        {
            if (result == ChangeResult.Done)
                Flash(doneClass, done);
            else
                Flash("danger", result == ChangeResult.Conflict ? ArticleMessages.DecidedElsewhere : notAllowed);
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

        [HttpGet]
        public IActionResult Newsletter(int page = 1)
        {
            page = Math.Max(page, 1);
            var subscribers = newsletterService.Get(page, SubscriberPageSize, out var total);
            ViewBag.Stats = newsletterService.GetStats();
            ViewBag.DigestEnglish = newsletterService.BuildDigest(AppLanguages.English);
            ViewBag.DigestBangla = newsletterService.BuildDigest(AppLanguages.Bangla);
            ViewBag.Page = page;
            ViewBag.PageSize = SubscriberPageSize;
            ViewBag.Total = total;
            return View(subscribers);
        }

        [HttpPost]
        public IActionResult SendDigest(string language)
        {
            if (!digestSender.CanSend)
            {
                Flash("warning", "Email delivery is turned off on this server, so no digest was sent.");
                return RedirectToAction("Newsletter");
            }

            var digest = newsletterService.BuildDigest(language);
            if (digest.Stories.Count == 0)
            {
                Flash("warning", "There are no stories from the past week to send.");
                return RedirectToAction("Newsletter");
            }
            if (digest.Recipients.Count == 0)
            {
                Flash("warning", "No confirmed subscribers for this edition yet.");
                return RedirectToAction("Newsletter");
            }

            var sections = categoryService.Get().ToDictionary(c => c.CategoryId, c => c.LocalName(digest.LanguageCode));
            var mails = digest.Recipients.Select(r => (r.SubscriberId, mail.Digest(digest, r, sections))).ToList();
            if (digestSender.Enqueue(new DigestJob(digest.LanguageCode, digest.Stories.Count, mails)))
                Flash("success", "The digest is being sent in the background. The audit log shows the result.");
            else
                Flash("warning", "This edition is already being sent.");
            return RedirectToAction("Newsletter");
        }

        [HttpPost]
        public IActionResult RemoveSubscriber(int id)
        {
            if (newsletterService.Remove(id))
                Flash("success", "Subscriber removed.");
            else
                Flash("danger", "This subscriber no longer exists.");
            return RedirectToAction("Newsletter");
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
            ViewBag.ResetVerified = PasswordChanger.IsResetVerified(HttpContext.Session);
            return View(new ChangePasswordDTO { UserId = user.UserId, Email = user.Email });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO obj)
        {
            var userId = CurrentUserId ?? 0;
            obj.UserId = userId;
            obj.Email = CurrentEmail;
            ViewBag.ResetVerified = PasswordChanger.IsResetVerified(HttpContext.Session);
            if (!ModelState.IsValid) return View(obj);

            switch (await passwordChanger.ChangeAsync(HttpContext.Session, userId, CurrentEmail, obj))
            {
                case PasswordChanger.Outcome.Changed:
                    var refreshed = authService.GetUser(userId);
                    if (refreshed != null) await SignInUserAsync(refreshed);
                    Flash("success", "Password updated");
                    return RedirectToAction("Profile", "Admin");
                case PasswordChanger.Outcome.WrongCurrentPassword:
                    ModelState.AddModelError(nameof(obj.CurrentPassword), L["Your current password is not correct."]);
                    return View(obj);
                case PasswordChanger.Outcome.Locked:
                    Flash("danger", "Too many failed attempts. Try again in 15 minutes.");
                    return View(obj);
                default:
                    Flash("danger", "Update failed");
                    return View(obj);
            }
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
            if (existing.UserId == CurrentUserId)
            {
                Flash("danger", "You cannot change your own role. Use your profile page instead.");
                return RedirectToAction("AllUser");
            }
            if (_roleService.Get(obj.RoleId) == null)
            {
                ModelState.AddModelError(nameof(obj.RoleId), L["Please choose a role"]);
                return View(existing);
            }
            if (string.IsNullOrWhiteSpace(obj.Name) || obj.Name.Trim().Length is < 2 or > 100)
            {
                ModelState.AddModelError(nameof(obj.Name), L["Name must be between 2 and 100 characters"]);
                return View(existing);
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
