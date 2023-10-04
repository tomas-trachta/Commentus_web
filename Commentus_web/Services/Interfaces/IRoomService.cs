using Commentus_web.Models;

namespace Commentus_web.Services.Interfaces
{
    public interface IRoomService
    {
        RoomModel GetRoom(string RoomsName, HttpContext httpContext, TestContext _context);
        void SendMessage(string message, string roomName, HttpContext httpContext, TestContext _context);
        Task<string?> GetNewMessages(HttpContext httpContext, string roomName, TestContext context);
        string? GetNewTasks(HttpContext httpContext, string roomName, TestContext _context);
        void AddNember(string username, string roomName, TestContext _context);
    }
}
