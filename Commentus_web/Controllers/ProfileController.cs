using Microsoft.AspNetCore.Mvc;

namespace Commentus_web.Controllers
{
    public class ProfileController : Controller
    {
        [Route("Home/Profile")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
