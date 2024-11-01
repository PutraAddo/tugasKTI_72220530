using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using System.Linq;
using System;

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _protector;

        // Constructor yang diperbaiki dengan parameter IDataProtectionProvider
        public AccountController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = new PasswordHasher<User>();
            _protector = dataProtectionProvider.CreateProtector("SampleSecureWeb.SecretProtector"); // Perbaikan di sini
        }

        // POST: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.Clear();
            }

            return RedirectToAction("Login", "Account");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = _passwordHasher.HashPassword(user, user.Password);
                string recoverySecret = Guid.NewGuid().ToString();
                string encryptedSecret = _protector.Protect(recoverySecret);
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);

            if (existingUser != null &&
                _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, password) == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetString("Username", username);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        // GET: Account/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword)
        {
            var username = HttpContext.Session.GetString("Username");

            if (username != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username);

                if (user != null && _passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword) == PasswordVerificationResult.Success)
                {
                    user.Password = _passwordHasher.HashPassword(user, newPassword);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Old password is incorrect.");
            }
            return View();
        }
    }
}
