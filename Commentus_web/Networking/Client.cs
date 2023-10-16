using System.Net;
using System.Net.Sockets;

namespace Commentus_web.Networking
{
    public class Client
    {
        public string? Name { get; set; }

        public Socket Socket { get; set; }

        private IPEndPoint _serverIpEndpoint { get; set; }

        public DateTime? LastMessageTimeStamp { get; set; }

        public DateTime? LastTaskTimeStamp { get; set; }

        public Client(HttpContext httpContext)
        {
            _serverIpEndpoint = CommunicationProtocol.GetServerIpEdpoint();

            Name = httpContext.Session.GetString("Name");

            Socket = new(_serverIpEndpoint.AddressFamily, CommunicationProtocol.SOCKET_TYPE, CommunicationProtocol.PROTOCOL_TYPE);

            Socket.Connect(_serverIpEndpoint);

            LastMessageTimeStamp = default;

            LastTaskTimeStamp = default;
        }
    }
}
