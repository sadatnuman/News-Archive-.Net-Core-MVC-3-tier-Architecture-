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
    [RoleAccess(AppRoles.Reader, AppRoles.Author)]
    [Logged]
    public class MemberController : AppController
    {
        private readonly UserService _userService;
        private readonly RoleService _roleService;

        public MemberController(UserService userService, RoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
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
                Flash("success", "User updated");
                return RedirectToAction("Profile","Member");
            }

            Flash("danger", "Update failed");
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
                Flash("success", "Password updated");
                return RedirectToAction("Profile", "Member");
            }

            Flash("danger", "Update failed");
            return View(obj);
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