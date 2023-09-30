using Commentus_web.Attributes;
using Commentus_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Commentus_web.Controllers
{
    public class ProfileController : Controller
    {
        private TestContext _context;

        public ProfileController(TestContext context)
        {
            _context = context;
        }

        [SessionFilter]
        [Route("Home/{controller}")]
        [Route("{controller}")]
        public IActionResult Index()
        {
            return View();
        }

        [SessionFilter]
        [HttpGet("Home/{controller}/{id}")]
        [HttpGet("{controller}/{id}")]
        public IActionResult GetProfileById(int id)
        {
            var model = _context.Users.Where(x => x.Id == id).FirstOrDefault();

            if(model != null)
            {
                return View(model);
            }

            return BadRequest();
        }
    }
}
