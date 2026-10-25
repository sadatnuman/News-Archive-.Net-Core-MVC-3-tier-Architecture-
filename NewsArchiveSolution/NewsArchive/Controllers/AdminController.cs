using BLL;
using NewsArchive.Filters;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NewsArchive.Controllers
{
    [RoleAccess(AppRoles.Admin)]
    [Logged]
    public class AdminController : Controller
    {
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly AnalyticsService analyticsService;


        public AdminController(UserService userService, RoleService roleService, AnalyticsService analyticsService)
        {
            _userService = userService;
            _roleService = roleService;
            this.analyticsService = analyticsService;
        }
        public IActionResult Index()
        {
            ViewBag.TotalArticles = analyticsService.GetTotalArticles();
            ViewBag.TotalUsers = analyticsService.GetTotalUsers();
            ViewBag.MostViewed = analyticsService.GetMostViewedArticles(5);
            ViewBag.ArticlesPerCategory = analyticsService.GetArticlesPerCategory();
            return View();
        }
        [HttpGet]
        public IActionResult AllUser()
        {
            var users = _userService.Get() ?? new List<UserDTO>();
            ViewBag.Roles = _roleService.Get();
            ViewBag.CurrentAdminEmail = HttpContext.Session.GetString("Email");
            return View(users);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AllUser(string searchTerm)
        {
            var users = _userService.Get() ?? new List<UserDTO>();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = users
                    .Where(u => u.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            ViewBag.Roles = _roleService.Get();
            ViewBag.CurrentAdminEmail = HttpContext.Session.GetString("Email");
            return View(users);
        }
        [HttpGet]
        public IActionResult Profile()
        {
            ViewBag.Roles = _roleService.Get();
            var email = HttpContext.Session.GetString("Email");
            var user = _userService.GetByEmail(email);
            if (user == null) return NotFound();
            ViewBag.RoleName = _roleService.Get().FirstOrDefault(r => r.Id == user.RoleId)?.RoleName;
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(UserDTO obj)
        {
            ViewBag.Roles = _roleService.Get();

            var existing = _userService.Get(obj.UserId);
            if (existing == null) return NotFound();

            existing.Name = obj.Name;

            var updated = _userService.Update(existing);
            if (updated)
            {
                TempData["Class"] = "success";
                TempData["Msg"] = "User updated";
                return RedirectToAction("Profile", "Admin");
            }

            TempData["Class"] = "danger";
            TempData["Msg"] = "Update failed";
            return View(existing);
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewBag.Roles = _roleService.Get();
            var email = HttpContext.Session.GetString("Email");
            var user = _userService.GetByEmail(email);
            if (user == null) return NotFound();
            ViewBag.RoleName = _roleService.Get().FirstOrDefault(r => r.Id == user.RoleId)?.RoleName;

            var model = new BLL.DTOs.ChangePasswordDTO
            {
                UserId = user.UserId,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(BLL.DTOs.ChangePasswordDTO obj)
        {
            ViewBag.Roles = _roleService.Get();

            if (!ModelState.IsValid)
            {
                return View(obj);
            }

            var existing = _userService.Get(obj.UserId);
            if (existing == null) return NotFound();

            existing.PasswordHash = GetMd5(obj.PasswordHash);
            existing.ConfPassword = GetMd5(obj.ConfPassword);

            var updated = _userService.Update(existing);
            if (updated)
            {
                TempData["Class"] = "success";
                TempData["Msg"] = "Password updated";
                return RedirectToAction("Profile", "Admin");
            }

            TempData["Class"] = "danger";
            TempData["Msg"] = "Update failed";
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

            var hashed1 = GetMd5(obj.PasswordHash ?? string.Empty);
            var hashed2 = GetMd5(obj.ConfPassword ?? string.Empty);

            var userDto = new UserDTO
            {
                Name = obj.Name,
                Email = obj.Email,
                PasswordHash = hashed1,
                ConfPassword = hashed2,
                RoleId = obj.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            var created = _userService.Create(userDto);
            if (created)
            {
                TempData["Class"] = "success";
                TempData["Msg"] = "User created successfully";
                return RedirectToAction("AllUser", "Admin");
            }

            TempData["Class"] = "danger";
            TempData["Msg"] = "Failed to create user";
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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserDTO obj)
        {
            ViewBag.Roles = _roleService.Get();

            var existing = _userService.Get(obj.UserId);
            if (existing == null) return NotFound();

            existing.Name = obj.Name;
            existing.RoleId = obj.RoleId;

            var updated = _userService.Update(existing);
            if (updated)
            {
                TempData["Class"] = "success";
                TempData["Msg"] = "User updated";
                return RedirectToAction("AllUser");
            }

            TempData["Class"] = "danger";
            TempData["Msg"] = "Update failed";
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
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string Decision)
        {
            var deleted = _userService.Delete(id);
            if (deleted)
            {
                TempData["Class"] = "success";
                TempData["Msg"] = "User deleted";
                return RedirectToAction("AllUser");
               
            }
            TempData["Class"] = "danger";
            TempData["Msg"] = "Delete failed";

            return RedirectToAction("AllUser");
        }

        private static string GetMd5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
    }
}