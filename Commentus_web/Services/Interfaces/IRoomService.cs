using Commentus_web.Models;

namespace Commentus_web.Services.Interfaces
{
    public interface IRoomService
    {
        RoomModel GetRoom(string RoomsName, HttpContext httpContext);
        void SendMessage(string message);
        Task<string?> GetNewMessages();
        string? GetNewTasks(HttpContext httpContext);
        void AddNember(string username);
    }
}
