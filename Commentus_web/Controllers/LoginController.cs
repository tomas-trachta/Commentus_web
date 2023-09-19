using Commentus_web.Cryptography;
using Commentus_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Commentus_web.Controllers
{
    public class LoginController : Controller
    {
        private TestContext _context { get; }
        public LoginController(TestContext context)
        {
            _context = context;
        }

        [Route("Home/Login")]
        public IActionResult Index()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"]?.ToString();

            return View();
        }

        [ValidateAntiForgeryToken]
        [Route("Home/Login/Action")]
        public async Task<IActionResult> Login(UserModel user)
        {
            var dbEntity = await this._context.Users.Where(x => x.Name == user.Name).FirstOrDefaultAsync();
                if(dbEntity != null)
                {
                    var hash = PasswordManager.ComputeHash(user.Password, dbEntity.Salt);

                    if (hash.SequenceEqual(dbEntity.Password))
                    {
                        HttpContext.Session.SetInt32("IsLoggedIn", 1);
                        HttpContext.Session.SetString("Name", user.Name);
                        HttpContext.Session.SetInt32("IsAdmin", Convert.ToInt32(dbEntity.IsAdmin));

                        return RedirectToAction("Index", "Profile");
                    }
                }
                TempData["ErrorMessage"] = "Wrong credentials!";
                return RedirectToAction("Index");
        }
    }
}
