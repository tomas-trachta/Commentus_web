using Commentus_web.Attributes;
using Commentus_web.Models;
using Commentus_web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Commentus_web.Controllers
{
    public class TaskController : Controller
    {
        private TestContext _context { get; }

        public TaskController(TestContext context)
        {
            _context = context;
        }

        [SessionFilter]
        [Route("Home/Task/{Name}")]
        public IActionResult Index(Models.TaskModel taskModel)
        {
            taskModel.Description = _context.Tasks.Where(t => t.Name == taskModel.Name).First().Description;

            return View(taskModel);
        }

        [SessionFilter]
        [Route("Home/Task/NewTask")]
        public IActionResult NewTask() 
        {
            return View(new TaskModel());
        }

        [SessionFilter]
        [Route("Home/Task/AddTask")]
        public IActionResult AddTask(TaskModel taskModel) 
        {
            var task = new Models.Task();
            task.Name = taskModel.Name;
            task.DueDate = taskModel.DueDate;
            task.Description = taskModel.Description;
            task.RoomsId = RoomService.Room!.Id;
            
            foreach(var solver in taskModel.Users!)
            {
                task.TasksSolvers.Add(new TasksSolver { Task = task, TaskId = task.Id, User = _context.Users.Where(u => u.Name == solver).First()});
            }

            _context.Tasks.Add(task);
            _context.SaveChanges();

            return RedirectToAction("Index", "Room");
        }
    }
}
