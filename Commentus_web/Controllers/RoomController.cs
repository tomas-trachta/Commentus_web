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
        [Route("Room")]
        public IActionResult Index()
        {
            return View();
        }

        [SessionFilter]
        [HttpGet("Home/Room/{RoomsName}")]
        [HttpGet("Room/{RoomsName}")]
        public IActionResult GetRoom(string RoomsName)
        {
            var model = _roomService.GetRoom(RoomsName, HttpContext, _context);

            return View(model);
        }

        [SessionFilter]
        [HttpGet("Home/Room/SendMessage")]
        [HttpGet("Room/SendMessage")]
        public void SendMessage(string message, string roomName)
        { 
            _roomService.SendMessage(message, roomName, HttpContext, _context);
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetNewMessages")]
        [HttpGet("Room/GetNewMessages")]
        public async Task<string?> GetNewMessages(string roomName)
        {
            try
            {
                return await _roomService.GetNewMessages(HttpContext, roomName, _context);
            }
            catch (Exception ex)
            {
                return $"{ex.Message}\n{ex.InnerException}";
            }
        }

        [SessionFilter]
        [HttpGet("Home/Room/GetNewTasks")]
        [HttpGet("Room/GetNewTasks")]
        public string? GetNewTasks(string roomName)
        {
            return _roomService.GetNewTasks(HttpContext, roomName, _context);
        }

        [AdminOnly]
        [SessionFilter]
        [HttpGet("Home/Room/AddMember")]
        [HttpGet("Room/AddMember")]
        public void AddMember(string username, string roomName)
        {
            _roomService.AddNember(username, roomName, _context);
        }

        [SessionFilter]
        [HttpGet("Home/Room/Paginator/GetNext")]
        [HttpGet("Room/Paginator/GetNext")]
        public string? GetNextPage(int page, string roomName)
        {
            return _roomService.Paginator(page, _context, roomName);
        }
    }
}