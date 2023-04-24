using Commentus_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Commentus_web.Controllers
{
    public class TaskController : Controller
    {
        [Route("Home/Task/{Name}")]
        public IActionResult Index(Models.Task task)
        {
            var Context = new TestContext();

            task.Description = Context.Tasks.Where(t => t.Name == task.Name).FirstOrDefault().Description;

            return View(task);
        }
    }
}
