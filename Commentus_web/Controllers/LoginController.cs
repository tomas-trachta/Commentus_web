using Commentus_web.Cryptography;
using Commentus_web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Commentus_web.Controllers
{
    public class LoginController : Controller
    {
        private TestContext Context { get; }
        public LoginController()
        {
            this.Context = new TestContext();
        }

        [Route("Home/Login")]
        public IActionResult Index()
        {
            //null-coalescing operator doesnt work here for some reason 🤷‍
            ViewBag.ErrorMessage = TempData["ErrorMessage"] != null ? TempData["ErrorMessage"].ToString() : null;

            return View();
        }

        [ValidateAntiForgeryToken]
        [Route("Home/Login/Action")]
        public IActionResult Login(UserModel user)
        {
                var dbEntity = this.Context.Users.Any(x => x.Name == user.Name) ? this.Context.Users.Where(x => x.Name == user.Name).First() : null;
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
