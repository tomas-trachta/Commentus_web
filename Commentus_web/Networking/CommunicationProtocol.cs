using Commentus_web.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Commentus_web.Networking
{
    public static class CommunicationProtocol
    {
        public const int PORT = 49160;
        public const SocketType SOCKET_TYPE = SocketType.Stream;
        public const ProtocolType PROTOCOL_TYPE = ProtocolType.Tcp;
        public const int BUFFER_SIZE = 64;

        public static IPEndPoint GetServerIpEdpoint() => new(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], PORT);


        #region Extensions
        public static bool AddEndpoint(this string message)
        {
            return message.Contains(':');
        }

        public static bool IsMemberOfRoom(this KeyValuePair<string,Socket> client, string message) 
        {
            return client.Key.Split(':')[1] == message;
        }

        public static bool HasOpenedConnection(this KeyValuePair<string, Socket> client, string message)
        {
            return client.Key.Contains(message.Split(':')[0]);
        }

        public static bool IsClient(this string name, List<Client> clients)
        {
            return clients.Where(x => x.Name == name).Any();
        }
        #endregion
    }
}
