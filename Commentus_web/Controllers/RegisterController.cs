using Commentus_web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;

namespace Commentus_web.Controllers
{
    using Commentus_web.Cryptography;
    using System.Text;

    public class RegisterController : Controller
    {
        private TestContext _context { get; }
        public RegisterController(TestContext context)
        {
            _context = context;
        }

        [Route("Home/Register")]
        public IActionResult Index()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"]?.ToString();

            return View();
        }

        [ValidateAntiForgeryToken]
        [Route("Home/Register/Action")]
        public IActionResult Register(UserModel user)
        {
            var hashedPassword = PasswordManager.HashPassword(user.Password);

            var newUser = new User();

            newUser.Name = user.Name;
            newUser.Password = hashedPassword.Item1;
            newUser.Salt = hashedPassword.Item2;

            if (this._context.Users.Any(x => x.Name == user.Name)) {
                    TempData["ErrorMessage"] = "User with this name already exists in database!";
                    return RedirectToAction("Index");
            }

                this._context.Users.Add(newUser);
                this._context.SaveChanges();

                HttpContext.Session.SetInt32("IsLoggedIn", 1);
                HttpContext.Session.SetString("Name", user.Name);
                HttpContext.Session.SetInt32("IsAdmin", 0);

            return RedirectToAction("Index","Profile");  
        }
    }
}
