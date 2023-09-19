using Commentus_web.Attributes;
using Commentus_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Commentus_web.Controllers
{
    public class RoomController : Controller
    {
        public static DateTime MessageTimestamp { get; set; }
        public static DateTime TasksTimestamp { get; set; }
        public static Room? Room { get; set; }
        public static User? User { get; set; }

        private TestContext _context { get; }

        public RoomController(TestContext context)
        {
            _context = context;
        }

        [Route("Home/Room")]
        public IActionResult Index()
        {
            return View();
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetRoom/{RoomsName}")]
        public IActionResult GetRoom(string RoomsName)
        {
            var model = new RoomModel();
            model.Room = _context.Rooms.Where(r => r.Name == RoomsName).FirstOrDefault();
            model.Members = _context.RoomsMembers.Include(m => m.Room).Where(m => m.Room.Name == RoomsName).Include(m => m.User);
            model.Messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room).Where(m => m.Room.Name == RoomsName);

            if (HttpContext.Session.GetInt32("IsAdmin") == 1)
            {
                var tasksolvers = _context.TasksSolvers.Include(t => t.Task).Where(t => t.Task.RoomsId ==
                                                         _context.Rooms.Where(r => r.Name == RoomsName).First().Id)
                                                    .OrderBy(t => t.TaskId);

                List<TasksSolver> taskslist = new List<TasksSolver>();

                if (tasksolvers.Any()) {
                    int taskid = tasksolvers.First().TaskId;
                    taskslist.Add(tasksolvers.First());

                    foreach (var task in tasksolvers)
                    {
                        if (task.TaskId == taskid)
                        {
                            continue;
                        }

                        taskid = task.TaskId;
                        taskslist.Add(task);
                    }
                }
                model.Tasks = taskslist.AsQueryable();
            }
            else
            {
                model.Tasks = _context.TasksSolvers.Include(t => t.User).Include(t => t.Task)
                                                   .Where(t => t.User.Name == HttpContext.Session.GetString("Name"))
                                                   .Where(t => t.Task.RoomsId ==
                                                         (_context.Rooms.Where(r => r.Name == RoomsName).First()).Id);
            }

            if(model.Messages.Any())
                RoomController.MessageTimestamp = model.Messages.OrderBy(t => t.Id).Last().Timestamp;

            if (model.Tasks.Any())
                RoomController.TasksTimestamp = model.Tasks.Include(t => t.Task).OrderBy(t => t.Task.Timestamp).Last().Task.Timestamp;

            if (model.Members.Any())
                RoomController.User = _context.Users.Where(m => m.Name == HttpContext.Session.GetString("Name")).FirstOrDefault();

            RoomController.Room = model.Room;

            return View(model);
        }

        [SessionFilter]
        [HttpGet("Home/Room/SendMessage")]
        public void SendMessage(string message)
        {
            _context.RoomsMessages.Add(new RoomsMessage()
            {
                Message = Encoding.UTF8.GetBytes(message),
                Room = Room!,
                User = User!
            });

            _context.SaveChanges();
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetNewMessages")]
        public string GetNewMessages()
        {
            var messages = new List<RoomsMessage>();
            messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room)
                                                .Where(m => m.Room.Name == RoomController.Room.Name 
                                                && m.Timestamp > RoomController.MessageTimestamp).ToList();

            if (messages.Any())
            {
                RoomController.MessageTimestamp = messages.Last().Timestamp;
                string rString = "";

                foreach(var message in messages)
                {
                    var imgBytes = message.User.ProfilePicture;
                    string img = imgBytes != null ? Convert.ToBase64String(imgBytes) : "";
                    string imgUrl = string.Format("data:image/png;base64,{0}", img);

                    rString += @"<div class=""row m-0 align-items-center justify-content-start mt-4"">";
                    rString += @"<div class=""col-auto m-0 p-0 me-4 ms-1"">";
                    rString += @"<div class=""row bg-secondary p-2 rounded text-white align-items-center justify-content-center"">";
                    rString += $"<div class=\"col-auto\"><img src=\"{imgUrl}\" style=\"width:25px;\" /></div>";
                    rString += $"<div class=\"col-auto\">{message.User.Name}</div>";
                    rString += "</div></div>";
                    rString += @"<div class=""col-auto p-0"">";
                    rString += "<span>";
                    byte[] messageBytes = message.Message;
                    string messageString = Encoding.UTF8.GetString(messageBytes);
                    rString += messageString;
                    rString += "</span></div></div>";
                }

                return rString;
            }
            return null;
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetNewTasks")]
        public string GetNewTasks()
        {
            var tasks = new List<TasksSolver>();
            tasks = _context.TasksSolvers.Include(t => t.User).Include(t => t.Task)
                                               .Where(t => t.User.Name == HttpContext.Session.GetString("Name"))
                                               .Where(t => t.Task.RoomsId ==
                                                     (_context.Rooms.Where(r => r.Name == RoomController.Room.Name).FirstOrDefault()).Id 
                                                     && t.Task.Timestamp > RoomController.TasksTimestamp).ToList();

            if (tasks.Any())
            {
                RoomController.TasksTimestamp = tasks.OrderBy(t => t.Id).Last().Task.Timestamp;
                string rString = "";

                foreach(var task in tasks)
                {
                    rString += @"<div class=""row m-0 mt-2 rounded bg-secondary text-white align-items-center justify-content-center"">";
                    rString += $"<div class=\"row\"><h4><a asp-action=\"GetRoom\" class=\"link-light\">{task.Task.Name}</a></h4></div>";
                    rString += $"<div class=\"row\"><h5>Due date: {task.Task.DueDate:dd.MM.yyyy}</h5></div>";
                    rString += "</div>";
                }

                return rString;
            }

            return null;
        }

        [SessionFilter]
        [HttpGet("Home/Room/AddMember")]
        public void AddMember(string username)
        {
            var user = _context.Users.Where(u => u.Name == username).FirstOrDefault();

            if (user != null)
            {
                _context.RoomsMembers.Add(new RoomsMember()
                {
                    User = user,
                    Room = Room!
                });

                _context.SaveChanges();
            }
        }
    }
}