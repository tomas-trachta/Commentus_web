using Commentus_web.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Commentus_web.Controllers
{
    public class ProfileController : Controller
    {
        [SessionFilter]
        [Route("Home/Profile")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
