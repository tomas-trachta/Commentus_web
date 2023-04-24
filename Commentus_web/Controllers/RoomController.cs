using Commentus_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Text;

namespace Commentus_web.Controllers
{
    public class RoomController : Controller
    {
        public static DateTime messageTimestamp { get; set; }
        public static DateTime tasksTimestamp { get; set; }
        public static Room? Room { get; set; }
        public static User? User { get; set; }

        [Route("Home/Room")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Home/Room/GetRoom/{RoomsName}")]
        public IActionResult GetRoom(string RoomsName)
        {
            var Context = new TestContext();

            var model = new RoomModel();
            model.Room = Context.Rooms.Where(r => r.Name == RoomsName).FirstOrDefault();
            model.Members = Context.RoomsMembers.Include(m => m.Room).Where(m => m.Room.Name == RoomsName).Include(m => m.User);
            model.Messages = Context.RoomsMessages.Include(m => m.User).Include(m => m.Room).Where(m => m.Room.Name == RoomsName);
            model.Tasks = Context.TasksSolvers.Include(t => t.User).Include(t => t.Task)
                                               .Where(t => t.User.Name == HttpContext.Session.GetString("Name"))
                                               .Where(t => t.Task.RoomsId ==
                                                     (Context.Rooms.Where(r => r.Name == RoomsName).FirstOrDefault()).Id);

            if(model.Messages.Any())
                RoomController.messageTimestamp = model.Messages.OrderBy(t => t.Id).Last().Timestamp;

            if (model.Tasks.Any())
                RoomController.tasksTimestamp = model.Tasks.Include(t => t.Task).OrderBy(t => t.Task.Timestamp).Last().Task.Timestamp;

            if (model.Members.Any())
                RoomController.User = Context.Users.Where(m => m.Name == HttpContext.Session.GetString("Name")).FirstOrDefault();

            RoomController.Room = model.Room;

            return View(model);
        }

        [HttpGet("Home/Room/SendMessage")]
        public void SendMessage(string message)
        {
            using (var Context = new TestContext()) {
                Context.Database.ExecuteSqlInterpolated($"INSERT INTO rooms_messages (User_id, Room_id, Message) VALUES ({RoomController.User.Id},{RoomController.Room.Id},{Encoding.UTF8.GetBytes(message)});");
            }
        }

        [HttpGet("Home/Room/GetNewMessages")]
        public string GetNewMessages()
        {
            var Context = new TestContext();

            var messages = new List<RoomsMessage>();
            messages = Context.RoomsMessages.Include(m => m.User).Include(m => m.Room)
                                                .Where(m => m.Room.Name == RoomController.Room.Name 
                                                && m.Timestamp > RoomController.messageTimestamp).ToList();

            if (messages.Any())
            {
                RoomController.messageTimestamp = messages.Last().Timestamp;
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

        [HttpGet("Home/Room/GetNewTasks")]
        public string GetNewTasks()
        {
            var Context = new TestContext();

            var tasks = new List<TasksSolver>();
            tasks = Context.TasksSolvers.Include(t => t.User).Include(t => t.Task)
                                               .Where(t => t.User.Name == HttpContext.Session.GetString("Name"))
                                               .Where(t => t.Task.RoomsId ==
                                                     (Context.Rooms.Where(r => r.Name == RoomController.Room.Name).FirstOrDefault()).Id 
                                                     && t.Task.Timestamp > RoomController.tasksTimestamp).ToList();

            if (tasks.Any())
            {
                RoomController.tasksTimestamp = tasks.OrderBy(t => t.Id).Last().Task.Timestamp;
                string rString = "";

                foreach(var task in tasks)
                {
                    rString += @"<div class=""row m-0 mt-2 rounded bg-secondary text-white align-items-center justify-content-center"">";
                    rString += $"<div class=\"row\"><h4><a asp-action=\"GetRoom\" class=\"link-light\">{task.Task.Name}</a></h4></div>";
                    rString += $"<div class=\"row\"><h5>Due date: {task.Task.DueDate.ToString("dd.MM.yyyy")}</h5></div>";
                    rString += "</div>";
                }

                return rString;
            }

            return null;
        }

        [HttpGet("Home/Room/AddMember")]
        public void AddMember(string username)
        {
            using (var Context = new TestContext())
            {
                if (Context.Users.Where(u => u.Name == username).Any())
                {
                    var userid = Context.Users.Where(u => u.Name == username).FirstOrDefault().Id;
                    Context.Database.ExecuteSqlInterpolated($"INSERT INTO rooms_members (User_id, Room_id) VALUES ({userid},{RoomController.Room.Id});");
                }
            }
        }
    }
}