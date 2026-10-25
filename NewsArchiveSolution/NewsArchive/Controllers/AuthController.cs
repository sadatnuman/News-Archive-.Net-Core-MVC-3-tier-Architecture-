using BLL;
using NewsArchive.Filters;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NewsArchive.Controllers
{
    public class AuthController : AppController
    {
        private readonly UserService _userService;
        private readonly RoleService _role_service;

        public AuthController(UserService userService, RoleService roleService)
        {
            _userService = userService;
            _role_service = roleService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Uname, string Pass)
        {
            var users = _userService.Get() ?? new List<UserDTO>();
            var hashed = GetMd5(Pass ?? string.Empty);

            var u = users.SingleOrDefault(user =>
                (string.Equals(user.Email, Uname, StringComparison.OrdinalIgnoreCase) 
                && string.Equals(user.PasswordHash, hashed, StringComparison.OrdinalIgnoreCase))
            );

            if (u != null) {    
                var roleId = _userService.Get()
                    .Where(user => string.Equals(user.Email, u.Email, StringComparison.OrdinalIgnoreCase))
                    .Select(user => user.RoleId)
                    .FirstOrDefault();

                HttpContext.Session.SetString("Email", u.Email);
                HttpContext.Session.SetInt32("UserId", u.UserId);
                HttpContext.Session.SetInt32("RoleId", roleId);

                return RedirectToAction("Index", "Article");
            }

            Flash("danger", "Invalid username/email or password");
            return View();
        }


        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _role_service.Get();
            return View(new UserDTO());
        }

        [HttpPost]
        public IActionResult Create(UserDTO obj)
        {
            ViewBag.Roles = _role_service.Get();

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
                RoleId = AppRoles.Reader,
                CreatedAt = DateTime.UtcNow
            };

            var created = _userService.Create(userDto);
            if (created)
            {
                Flash("success", "User created successfully");
                return RedirectToAction("Login", "Auth");
            }

            Flash("danger", "Failed to create user");
            return View(obj);
        }

        static string GetMd5(string input)
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

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Flash("success", "Logged out successfully");
            return RedirectToAction("Index", "Article");
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                Flash("danger", "Email is required");
                return View();
            }

            var user = _userService.Get().FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                Flash("danger", "No user found with that email");
                return View();
            }

            var otp = new Random().Next(100000, 999999);

            var otpString = otp.ToString();
            var expiry = DateTime.UtcNow.AddMinutes(1);
            HttpContext.Session.SetString($"PwdReset:Otp:{email}", otpString);
            HttpContext.Session.SetString($"PwdReset:Expiry:{email}", expiry.ToString("o"));
            HttpContext.Session.SetInt32($"PwdReset:Used:{email}", 0);
            HttpContext.Session.SetInt32($"PwdReset:UserId:{email}", user.UserId);

            Console.WriteLine("OTP: " + otpString);

            Flash("success", "OTP sent (check console for dev).");

            return RedirectToAction("VerifyOtp", new { email = email });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                Flash("danger", "Email and OTP are required");
                return View();
            }

            var sessionOtp = HttpContext.Session.GetString($"PwdReset:Otp:{email}");
            var expiryString = HttpContext.Session.GetString($"PwdReset:Expiry:{email}");
            var usedFlag = HttpContext.Session.GetInt32($"PwdReset:Used:{email}");
            var userId = HttpContext.Session.GetInt32($"PwdReset:UserId:{email}");

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(expiryString) || userId == null)
            {
                Flash("danger", "No OTP request found for this email");
                return View();
            }

            if (usedFlag != null && usedFlag.Value == 1)
            {
                Flash("danger", "OTP already used");
                return View();
            }

            if (!DateTime.TryParse(expiryString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiry))
            {
                Flash("danger", "Invalid OTP data");
                return View();
            }

            if (DateTime.UtcNow > expiry)
            {
                Flash("danger", "OTP expired");
                return View();
            }

            if (!string.Equals(sessionOtp, otp, StringComparison.Ordinal))
            {
                Flash("danger", "Invalid OTP");
                return View();
            }

            HttpContext.Session.SetInt32($"PwdReset:Used:{email}", 1);

            var user = _userService.Get(userId.Value);
            if (user == null)
            {
                Flash("danger", "User not found");
                return View();
            }

            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);

            if (user.RoleId == AppRoles.Admin)
                return RedirectToAction("ChangePassword", "Admin");
            else
                return RedirectToAction("ChangePassword", "Member");
        }
    }
}