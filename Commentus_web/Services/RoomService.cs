using Commentus_web.Models;
using Commentus_web.Services.Interfaces;
using System.Net.Sockets;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System;
using Commentus_web.Networking;

namespace Commentus_web.Services
{
    public class RoomService : IRoomService
    {
        public static DateTime MessageTimestamp { get; set; }
        public static DateTime TasksTimestamp { get; set; }
        public static Room? Room { get; set; }
        public static new User? User { get; set; }

        private TestContext _context { get; }
        private RoomModel _roomModel { get; set; }
        private IPEndPoint _serverIpEndpoint { get; set; }

        private static Socket _client { get; set; }

        public RoomService()
        {
            _context = new TestContext();

            _roomModel = new RoomModel();

            _serverIpEndpoint = CommunicationProtocol.GetServerIpEdpoint();

            _client = new(_serverIpEndpoint.AddressFamily, CommunicationProtocol.SOCKET_TYPE, CommunicationProtocol.PROTOCOL_TYPE);
            _client.Connect(_serverIpEndpoint);
        }

        public RoomModel GetRoom(string RoomsName, HttpContext httpContext)
        {
            _roomModel.Room = _context.Rooms.Where(r => r.Name == RoomsName).FirstOrDefault();
            _roomModel.Members = _context.RoomsMembers.Include(m => m.Room).Where(m => m.Room.Name == RoomsName).Include(m => m.User);
            _roomModel.Messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room).Where(m => m.Room.Name == RoomsName);

            if (httpContext.Session.GetInt32("IsAdmin") == 1)
            {
                var tasksolvers = _context.TasksSolvers.Include(t => t.Task).Where(t => t.Task.RoomsId ==
                                                         _context.Rooms.Where(r => r.Name == RoomsName).First().Id)
                                                    .OrderBy(t => t.TaskId);

                List<TasksSolver> taskslist = new();

                if (tasksolvers.Any())
                {
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
                _roomModel.Tasks = taskslist.AsQueryable();
            }
            else
            {
                _roomModel.Tasks = _context.TasksSolvers.Include(t => t.User).Include(t => t.Task)
                                                   .Where(t => t.User.Name == httpContext.Session.GetString("Name"))
                                                   .Where(t => t.Task.RoomsId ==
                                                         (_context.Rooms.Where(r => r.Name == RoomsName).First()).Id);
            }

            if (_roomModel.Messages.Any())
                MessageTimestamp = _roomModel.Messages.OrderBy(t => t.Id).Last().Timestamp;

            if (_roomModel.Tasks.Any())
                TasksTimestamp = _roomModel.Tasks.Include(t => t.Task).OrderBy(t => t.Task.Timestamp).Last().Task.Timestamp;

            if (_roomModel.Members.Any())
                User = _context.Users.Where(m => m.Name == httpContext.Session.GetString("Name")).FirstOrDefault();

            Room = _roomModel.Room;

            var message = $"{httpContext.Session.GetString("Name")}:{Room.Name}";
            var sent = _client.Send(Encoding.UTF8.GetBytes(message));

            return _roomModel;
        }

        public void SendMessage(string message)
        {
            var mess = new RoomsMessage()
            {
                Message = Encoding.UTF8.GetBytes(message),
                RoomId = Room.Id,
                UserId = User.Id
            };

            _context.RoomsMessages.Add(mess);

            _context.SaveChanges();

            var roomNameBuffer = Encoding.UTF8.GetBytes(Room.Name);

            _client.Send(roomNameBuffer);
        }

        public async Task<string?> GetNewMessages()
        {
            var buffer = new byte[CommunicationProtocol.BUFFER_SIZE];
            //long polling
            var received = await _client.ReceiveAsync(buffer, SocketFlags.None);

            if (Encoding.UTF8.GetString(buffer, 0, received) == Room.Name)
            {
                var messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room)
                                                    .Where(m => m.Room.Name == Room.Name
                                                    && m.Timestamp > MessageTimestamp).ToList();

                if (messages.Any())
                {
                    MessageTimestamp = messages.Last().Timestamp;
                    string rString = "";

                    foreach (var message in messages)
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
            }
            return null;
        }

        public string? GetNewTasks(HttpContext httpContext)
        {
            var tasks = _context.TasksSolvers.Include(t => t.User).Include(t => t.Task)
                                               .Where(t => t.User.Name == httpContext.Session.GetString("Name"))
                                               .Where(t => t.Task.RoomsId == Room.Id && t.Task.Timestamp > TasksTimestamp).ToList();

            if (tasks.Any())
            {
                TasksTimestamp = tasks.OrderBy(t => t.Id).Last().Task.Timestamp;
                string rString = "";

                foreach (var task in tasks)
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

        public void AddNember(string username)
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
