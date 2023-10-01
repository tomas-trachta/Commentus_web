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
using Commentus_web.Services;

namespace Commentus_web.Controllers
{
    public class RoomController : Controller
    {
        private IRoomService _roomService { get; set; }

        private TestContext _context { get; }

        public RoomController(IRoomService roomService, TestContext context)
        {
            _roomService = roomService;
            _context = context;
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
            var model = _roomService.GetRoom(RoomsName, HttpContext, _context);

            return View(model);
        }

        [SessionFilter]
        [HttpGet("Home/Room/SendMessage")]
        public void SendMessage(string message, string roomName)
        { 
            _roomService.SendMessage(message, roomName, HttpContext, _context);
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetNewMessages")]
        public async Task<string?> GetNewMessages(string roomName)
        {
            return await _roomService.GetNewMessages(HttpContext, roomName);
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetNewTasks")]
        public string? GetNewTasks(string roomName)
        {
            return _roomService.GetNewTasks(HttpContext, roomName, _context);
        }

        [AdminOnly]
        [SessionFilter]
        [HttpGet("Home/Room/AddMember")]
        public void AddMember(string username, string roomName)
        {
            _roomService.AddNember(username, roomName, _context);
        }
    }
}