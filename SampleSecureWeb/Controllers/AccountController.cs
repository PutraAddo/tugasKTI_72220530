using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // Tambahkan ini
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher; // Untuk hashing password

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
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
                user.Password = _passwordHasher.HashPassword(user, user.Password); // Hash password
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
        public IActionResult Login(User user)
        {
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Username == user.Username);

            if (existingUser != null && 
                _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, user.Password) == PasswordVerificationResult.Success) // Verifikasi hashed password
            {
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(user);
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

                if (user != null && _passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword) == PasswordVerificationResult.Success) // Verifikasi hashed password
                {
                    user.Password = _passwordHasher.HashPassword(user, newPassword); // Hash password baru
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Old password is incorrect.");
            }

            return View();
        }
    }
}
