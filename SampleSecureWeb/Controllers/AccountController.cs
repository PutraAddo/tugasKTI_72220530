using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // Tambahkan ini
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using Microsoft.AspNetCore.Http; // Tambahkan untuk IHttpContextAccessor
using System.Linq; // Tambahkan untuk LINQ

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher; // Untuk hashing password
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor yang digabungkan
        public AccountController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = new PasswordHasher<User>(); // Inisialisasi password hasher
        }

        [HttpPost] // Use POST instead of GET for logout
public IActionResult Logout()
{
    // Clear session
    _httpContextAccessor.HttpContext.Session.Clear();
    
    // Redirect to login page or home after logout
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
                // Hash password sebelum menyimpan
                user.Password = _passwordHasher.HashPassword(user, user.Password); 
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
            // Cari user berdasarkan username
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Username == user.Username);

            // Verifikasi password
            if (existingUser != null && 
                _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, user.Password) == PasswordVerificationResult.Success)
            {
                // Set session jika login sukses
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Home");
            }

            // Jika login gagal
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
            // Ambil username dari session
            var username = HttpContext.Session.GetString("Username");

            if (username != null)
            {
                // Cari user berdasarkan username
                var user = _context.Users.FirstOrDefault(u => u.Username == username);

                if (user != null && _passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword) == PasswordVerificationResult.Success)
                {
                    // Hash password baru
                    user.Password = _passwordHasher.HashPassword(user, newPassword); 
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }

                // Jika password lama salah
                ModelState.AddModelError(string.Empty, "Old password is incorrect.");
            }
            return View();
        }
    }
}
