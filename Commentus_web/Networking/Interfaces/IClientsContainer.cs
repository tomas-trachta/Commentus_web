using Commentus_web.Models;

namespace Commentus_web.Networking.Interfaces
{
    public interface IClientsContainer
    {
        void AddClient(HttpContext httpContext);
        void Send(HttpContext httpContext, byte[] dataBuffer);
        void ChangeLastMessageTimeStamp(HttpContext httpContext, TestContext context, int roomId, DateTime? timeStamp = default);
        Task<int> ReceiveMessage(HttpContext httpContext);
        List<Client> Clients { get; }
    }
}
