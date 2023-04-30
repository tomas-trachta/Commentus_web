using Commentus_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Commentus_web.Controllers
{
    public class TaskController : Controller
    {
        [Route("Home/Task/{Name}")]
        public IActionResult Index(Models.TaskModel taskModel)
        {
            var Context = new TestContext();

            taskModel.Description = Context.Tasks.Where(t => t.Name == taskModel.Name).FirstOrDefault().Description;

            return View(taskModel);
        }

        [Route("Home/Task/NewTask")]
        public IActionResult NewTask() 
        {
            var model = new Commentus_web.Models.TaskModel();

            return View(model);
        }

        [Route("Home/Task/AddTask")]
        public IActionResult AddTask(TaskModel taskModel) 
        {
            var context = new TestContext();

            var task = new Models.Task();
            task.Name = taskModel.Name;
            task.DueDate = taskModel.DueDate;
            task.Description = taskModel.Description;
            task.RoomsId = RoomController.Room.Id;
            
            foreach(var solver in taskModel.Users)
            {
                task.TasksSolvers.Add(new TasksSolver { Task = task, TaskId = task.Id, User = context.Users.Where(u => u.Name == solver).First()});
            }

            context.Tasks.Add(task);
            context.SaveChanges();

            return RedirectToAction("Index", "Room");
        }
    }
}
