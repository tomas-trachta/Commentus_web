using Commentus_web.Models;
using Commentus_web.Services.Interfaces;
using System.Net.Sockets;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System;
using Commentus_web.Networking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Commentus_web.Extensions;

namespace Commentus_web.Services
{
    public class RoomService : IRoomService
    {
        private IPEndPoint _serverIpEndpoint { get; set; }

        private Dictionary<string, Socket> _clients { get; }
        private Dictionary<string, DateTime?> _messageTimeStamps { get; set; }
        private Dictionary<string, DateTime?> _taskTimeStamps { get; set; }

        public RoomService()
        {
            _messageTimeStamps = new();

            _serverIpEndpoint = CommunicationProtocol.GetServerIpEdpoint();

            _clients = new();
        }

        public RoomModel GetRoom(string roomName, HttpContext httpContext, TestContext _context)
        {
            Socket? client = default;
            var userName = httpContext.Session.GetString("Name");
            if (!userName.IsClient(_clients))
            {
                client = new Socket(_serverIpEndpoint.AddressFamily, CommunicationProtocol.SOCKET_TYPE, CommunicationProtocol.PROTOCOL_TYPE);
                client.Connect(_serverIpEndpoint);

                _clients.Add(userName, client);
            }

            var roomModel = new RoomModel();

            roomModel.Room = _context.Rooms.Where(r => r.Name == roomName).FirstOrDefault();
            roomModel.Members = _context.RoomsMembers.Include(m => m.Room).Where(m => m.Room.Name == roomName).Include(m => m.User);
            roomModel.Messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room).Where(m => m.Room.Name == roomName);

            if (httpContext.Session.GetInt32("IsAdmin") == 1)
            {
                var tasksolvers = _context.TasksSolvers.Include(t => t.Task).Where(t => t.Task.RoomsId ==
                                                         _context.Rooms.Where(r => r.Name == roomName).First().Id)
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
                roomModel.Tasks = taskslist.AsQueryable();
            }
            else
            {
                roomModel.Tasks = _context.TasksSolvers.Include(t => t.User).Include(t => t.Task)
                                                   .Where(t => t.User.Name == httpContext.Session.GetString("Name"))
                                                   .Where(t => t.Task.RoomsId ==
                                                         (_context.Rooms.Where(r => r.Name == roomName).First()).Id);
            }

            var message = $"{httpContext.Session.GetString("Name")}:{roomName}";

            if(client == default)
                client = _clients.First(x => x.Key == userName).Value;

            var sent = client.Send(Encoding.UTF8.GetBytes(message));

            return roomModel;
        }

        public void SendMessage(string message, string roomName, HttpContext httpContext, TestContext _context)
        {
            var userName = httpContext.Session.GetString("Name");

            var mess = new RoomsMessage()
            {
                Message = Encoding.UTF8.GetBytes(message),
                RoomId = _context.Rooms.Where(x => x.Name == roomName).First().Id,
                UserId = _context.Users.Where(x => x.Name == userName).First().Id
            };

            _context.RoomsMessages.Add(mess);

            _context.SaveChanges();

            var roomNameBuffer = Encoding.UTF8.GetBytes(roomName);

            var client = _clients.First(x => x.Key == userName).Value;
            client.Send(roomNameBuffer);
        }

        public async Task<string?> GetNewMessages(HttpContext httpContext, string roomName)
        {
            using (var _context = new TestContext())
            {
                var userName = httpContext.Session.GetString("Name");

                var buffer = new byte[CommunicationProtocol.BUFFER_SIZE];
                var client = _clients.First(x => x.Key == userName).Value;
                //long polling
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);

                var timeStamp = _messageTimeStamps.GetTimeStamp(userName) ?? DateTime.MinValue;

                var messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room)
                                                    .Where(m => m.Room.Name == roomName
                                                    && m.Timestamp > timeStamp).ToList();

                if (messages.Any())
                {
                    if(_messageTimeStamps.ContainsKey(userName))
                        _messageTimeStamps[userName] = messages.LastOrDefault()?.Timestamp;
                    else
                        _messageTimeStamps.Add(userName, messages.LastOrDefault()?.Timestamp);

                    StringBuilder rString = new();

                    foreach (var message in messages)
                    {
                        var imgBytes = message.User.ProfilePicture;
                        string img = imgBytes != null ? Convert.ToBase64String(imgBytes) : "";
                        string imgUrl = string.Format("data:image/png;base64,{0}", img);

                        rString.Append(@"<div class=""row mt-4"">");
                        rString.Append(@"<div class=""col-1""></div>");
                        rString.Append($"<small class=\"col-auto text-muted\">{message.User.Name}</small>");
                        rString.Append($"<small class=\"col-auto text-muted ps-2\">{message.Timestamp}</small>");
                        rString.Append("</div>");
                        rString.Append(@"<div class=""row m-0 align-items-start justify-content-start mt-1"">");
                        rString.Append(@"<div class=""col-auto m-0 p-0 me-4 ms-1"">");
                        rString.Append(@"<div class=""row p-2 text-white align-items-center justify-content-center"">");
                        rString.Append($"<div class=\"col-auto\"><img src=\"{imgUrl}\" style=\"width:25px;\" /></div>");
                        rString.Append("</div></div>");
                        rString.Append(@"<div class=""col-9 d-flex h-100 align-items-center p-0 ps-2 rounded"" style=""background: #e8e8e8;"">");
                        rString.Append("<span>");
                        byte[] messageBytes = message.Message;
                        string messageString = Encoding.UTF8.GetString(messageBytes);
                        rString.Append(messageString);
                        rString.Append("</span></div></div>");
                    }

                    return rString.ToString();
                }
                return null;
            }
        }

        public string? GetNewTasks(HttpContext httpContext, string roomName, TestContext _context)
        {
            var userName = httpContext.Session.GetString("Name");

            var timeStamp = _taskTimeStamps.GetTimeStamp(userName) ?? DateTime.MinValue;

            var room = _context.Rooms.First(room => room.Name == roomName);
            var tasks = _context.TasksSolvers.GetTaskSolvers(userName, room, timeStamp);

            if (tasks.Any())
            {
                if (_taskTimeStamps.ContainsKey(userName))
                    _taskTimeStamps[userName] = tasks.LastOrDefault()?.Task.Timestamp;
                else
                    _taskTimeStamps.Add(userName, tasks.LastOrDefault()?.Task.Timestamp);

                StringBuilder rString = new();

                foreach (var task in tasks)
                {
                    rString.Append(@"<div class=""row m-0 mt-2 rounded bg-secondary text-white align-items-center justify-content-center"">");
                    rString.Append($"<div class=\"row\"><h4><a asp-action=\"GetRoom\" class=\"link-light\">{task.Task.Name}</a></h4></div>");
                    rString.Append($"<div class=\"row\"><h5>Due date: {task.Task.DueDate:dd.MM.yyyy}</h5></div>");
                    rString.Append("</div>");
                }

                return rString.ToString();
            }

            return null;
        }

        public void AddNember(string username, string roomName, TestContext _context)
        {
            var user = _context.Users.Where(u => u.Name == username).FirstOrDefault();
            var room = _context.Rooms.First(room => room.Name == roomName);

            if (user != null)
            {
                _context.RoomsMembers.Add(new RoomsMember()
                {
                    User = user,
                    Room = room
                });

                _context.SaveChanges();
            }
        }
    }
}
