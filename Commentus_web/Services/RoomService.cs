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

namespace Commentus_web.Services
{
    public class RoomService : IRoomService
    {
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
            roomModel.Messages = _context.RoomsMessages.Include(m => m.User).Include(m => m.Room).Where(m => m.Room.Name == roomName);

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
                    rString.Append("<span>");
                    rString.Append(messageString);
                    rString.Append("</span></div></div>");
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
    }
}
