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
using Commentus_web.Networking.Interfaces;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace Commentus_web.Services
{
    public class RoomService : IRoomService
    {
        private const int PAGE_SIZE = 15;

        private Dictionary<string, DateTime?> _taskTimeStamps { get; set; }

        private IClientsContainer _clientsContainer { get; set; }

        public RoomService(IClientsContainer clientsContainer)
        {
            _clientsContainer = clientsContainer;
        }

        public RoomModel GetRoom(string roomName, HttpContext httpContext, TestContext _context)
        {
            var userName = httpContext.Session.GetString("Name");

            if (!userName.IsClient(_clientsContainer.Clients))
            {
                _clientsContainer.AddClient(httpContext);
            }

            var roomId = _context.Rooms.First(x => x.Name == roomName).Id;
            _clientsContainer.ChangeLastMessageTimeStamp(httpContext, _context, roomId);

            var roomModel = new RoomModel();

            roomModel.Room = _context.Rooms.Where(r => r.Name == roomName).FirstOrDefault();
            roomModel.Members = _context.RoomsMembers.Include(m => m.Room).Where(m => m.Room.Name == roomName).Include(m => m.User);
            //take 15 newest messages
            roomModel.Messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room).Where(m => m.Room.Name == roomName).OrderByDescending(x => x.Id).Take(PAGE_SIZE);

            if (httpContext.Session.GetInt32("IsAdmin") == 1)
            {
                var tasksolvers = _context.TasksSolvers.GetTaskSolversAsAdmin(_context.Rooms, roomName);

                IQueryable<TasksSolver>? tasks = Enumerable.Empty<TasksSolver>().AsQueryable();

                if (tasksolvers.Any())
                    tasks = tasksolvers.ToList().DistinctBy(t => t.TaskId).AsQueryable();

                roomModel.Tasks = tasks;
            }
            else
            {
                roomModel.Tasks = _context.TasksSolvers.GetTaskSolvers(userName, _context.Rooms, roomName);
            }

            var message = $"{userName}:{roomName}";

            _clientsContainer.Send(httpContext, Encoding.UTF8.GetBytes(message));

            return roomModel;
        }

        public void SendMessage(string message, string roomName, HttpContext httpContext, TestContext _context)
        {
            var userName = httpContext.Session.GetString("Name");

            var newMessage = new RoomsMessage()
            {
                Message = Encoding.UTF8.GetBytes(message),
                RoomId = _context.Rooms.Where(x => x.Name == roomName).First().Id,
                UserId = _context.Users.Where(x => x.Name == userName).First().Id
            };

            _context.RoomsMessages.Add(newMessage);

            _context.SaveChanges();

            var roomNameBuffer = Encoding.UTF8.GetBytes(roomName);

            _clientsContainer.Send(httpContext, roomNameBuffer);
        }

        public async Task<string?> GetNewMessages(HttpContext httpContext, string roomName, TestContext context)
        {
            var userName = httpContext.Session.GetString("Name");
            var client = _clientsContainer.Clients.First(x => x.Name == userName);

            //long polling
            var received = await _clientsContainer.ReceiveMessage(httpContext);

            var messages = context.RoomsMessages.Include(m => m.User).Include(m => m.Room)
                                                .Where(m => m.Room.Name == roomName
                                                && m.Timestamp > client.LastMessageTimeStamp).ToList();

            if (messages.Any())
            {
                _clientsContainer.ChangeLastMessageTimeStamp(httpContext, context, context.Rooms.First(x => x.Name == roomName).Id, messages.LastOrDefault()?.Timestamp);

                StringBuilder rString = new();

                string URIPattern = @"^(?:\w+:)?\/\/([^\s\.]+\.\S{2}|localhost[\:?\d]*)\S*$";

                foreach (var message in messages)
                {
                    var imgBytes = message.User.ProfilePicture;
                    string img = imgBytes != null ? Convert.ToBase64String(imgBytes) : "";
                    string imgUrl = string.Format("data:image/png;base64,{0}", img);

                    byte[] messageBytes = message.Message;
                    string messageString = Encoding.UTF8.GetString(messageBytes);

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
                    if ((messageBytes.Length > 3 && messageBytes[0] == 0xFF && messageBytes[1] == 0xD8
                                && messageBytes[messageBytes.Length - 2] == 0xFF && messageBytes[messageBytes.Length - 1] == 0xD9)
                                || (messageBytes[0] == 137 && messageBytes[1] == 80 && messageBytes[2] == 78 && messageBytes[3] == 71
                                && messageBytes[4] == 13 && messageBytes[5] == 10 && messageBytes[6] == 26 && messageBytes[7] == 10))
                    {
                        string image = messageBytes != null ? Convert.ToBase64String(messageBytes) : "";
                        string imageUrl = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(messageBytes));

                        rString.Append(@"<div class=""col-auto p-0"">");
                        rString.Append($"<img src=\"{imageUrl}\" />");
                        rString.Append("</div>");
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(messageString, URIPattern))
                    {
                        rString.Append(@"<div class=""col-auto p-0"">");
                        rString.Append($"<img class=\"w-100\" src=\"{messageString}\" />");
                        rString.Append("</div>");
                    }
                    else
                    {
                        rString.Append("<span>");
                        rString.Append(messageString);
                        rString.Append("</span>");
                    }

                    rString.Append("</div></div>");
                }

                return rString.ToString();
            }
            return null;
        }

        public string? GetNewTasks(HttpContext httpContext, string roomName, TestContext _context)
        {
            var userName = httpContext.Session.GetString("Name");

            var timeStamp = DateTime.MinValue;

            var room = _context.Rooms.First(room => room.Name == roomName);
            var tasks = _context.TasksSolvers.GetAddedTaskSolvers(userName, room, timeStamp);

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

        public string? Paginator(int page, TestContext _context, string roomName)
        {
            var messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room).Where(m => m.Room.Name == roomName).OrderByDescending(x => x.Id).Skip(page * PAGE_SIZE).Take(PAGE_SIZE);

            StringBuilder? rString = messages == null ? null : new();

            string URIPattern = @"^(?:\w+:)?\/\/([^\s\.]+\.\S{2}|localhost[\:?\d]*)\S*$";

            foreach (var message in messages)
            {
                var imgBytes = message.User.ProfilePicture;
                string img = imgBytes != null ? Convert.ToBase64String(imgBytes) : "";
                string imgUrl = string.Format("data:image/png;base64,{0}", img);

                byte[] messageBytes = message.Message;
                string messageString = Encoding.UTF8.GetString(messageBytes);

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
                if ((messageBytes.Length > 3 && messageBytes[0] == 0xFF && messageBytes[1] == 0xD8
                            && messageBytes[messageBytes.Length - 2] == 0xFF && messageBytes[messageBytes.Length - 1] == 0xD9)
                            || (messageBytes[0] == 137 && messageBytes[1] == 80 && messageBytes[2] == 78 && messageBytes[3] == 71
                            && messageBytes[4] == 13 && messageBytes[5] == 10 && messageBytes[6] == 26 && messageBytes[7] == 10))
                {
                    string image = messageBytes != null ? Convert.ToBase64String(messageBytes) : "";
                    string imageUrl = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(messageBytes));

                    rString.Append(@"<div class=""col-auto p-0"">");
                    rString.Append($"<img src=\"{imageUrl}\" />");
                    rString.Append("</div>");
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(messageString, URIPattern))
                {
                    rString.Append(@"<div class=""col-auto p-0"">");
                    rString.Append($"<img class=\"w-100\" src=\"{messageString}\" />");
                    rString.Append("</div>");
                }
                else
                {
                    rString.Append("<span>");
                    rString.Append(messageString);
                    rString.Append("</span>");
                }
                
                rString.Append("</div></div>");
            }

            return rString?.ToString();
        }
    }
}
