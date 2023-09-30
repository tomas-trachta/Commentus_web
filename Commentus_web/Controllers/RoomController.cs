using Commentus_web.Attributes;
using Commentus_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Text;
using System.Reflection;
using System.Net.Sockets;
using System;
using System.Net;
using Commentus_web.Services.Interfaces;

namespace Commentus_web.Controllers
{
    public class RoomController : Controller
    {
        private IRoomService _roomService { get; set; }

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [Route("Home/Room")]
        public IActionResult Index()
        {
            return View();
        }

        [SessionFilter]
        [HttpGet("Home/Room/{RoomsName}")]
        public IActionResult GetRoom(string RoomsName)
        {
            var model = _roomService.GetRoom(RoomsName, HttpContext);

            return View(model);
        }

        [SessionFilter]
        [HttpGet("Home/Room/SendMessage")]
        public void SendMessage(string message)
        {
            _roomService.SendMessage(message);
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetNewMessages")]
        public async Task<string?> GetNewMessages()
        {
            return await _roomService.GetNewMessages();
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetNewTasks")]
        public string? GetNewTasks()
        {
            return _roomService.GetNewTasks(HttpContext);
        }

        [AdminOnly]
        [SessionFilter]
        [HttpGet("Home/Room/AddMember")]
        public void AddMember(string username)
        {
            _roomService.AddNember(username);
        }
    }
}