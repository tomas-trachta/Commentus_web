using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using Commentus_web.Models;
using Task = System.Threading.Tasks.Task;
using MySqlX.XDevAPI;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Commentus_web.Networking
{
    public class Server
    {
        private static Dictionary<string, Socket> _clients { get; set; }

        private static Socket _listener { get; set; }
        private static byte[] _buffer { get; set; }

        private IPEndPoint _serverIpEndpoint { get; set; }

        private Task _deleteClosedConnections { get; set; }

        public Server()
        {
            _serverIpEndpoint = CommunicationProtocol.GetServerIpEdpoint();

            _listener = new(_serverIpEndpoint.AddressFamily, CommunicationProtocol.SOCKET_TYPE, CommunicationProtocol.PROTOCOL_TYPE);
            _clients = new();

            _listener.Bind(_serverIpEndpoint);
            _listener.Listen();

            _buffer = new byte[CommunicationProtocol.BUFFER_SIZE];

            StartReccuringTask();
        }

        public void Start()
        {
            _listener.BeginAccept(new AsyncCallback(AcceptCallback), _listener);
        }

        public void Stop()
        {
            _listener.Shutdown(SocketShutdown.Receive);
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            Socket socket = _listener.EndAccept(result);

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

            _listener.BeginAccept(new AsyncCallback(AcceptCallback), _listener);
        }

        private static void ReceiveCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState!;
            int received = socket.EndReceive(result);

            byte[] dataBuffer = new byte[received];
            Array.Copy(_buffer, dataBuffer, received);

            _buffer = new byte[CommunicationProtocol.BUFFER_SIZE];

            string message = Encoding.UTF8.GetString(dataBuffer);

            if (message.AddEndpoint())
            {
                var existingClient = _clients.Where(client => client.HasOpenedConnection(message)).FirstOrDefault();

                if (!existingClient.Equals(default(KeyValuePair<string, Socket>)))
                    _clients.Remove(existingClient.Key);

                _clients.Add(message, socket);
            }

            else
            {
                foreach (var client in _clients)
                {
                    if (client.IsMemberOfRoom(message))
                        socket.BeginSendTo(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, client.Value.LocalEndPoint, new AsyncCallback(SendCallback), socket);
                }
            }

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        private static void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState!;
            socket.EndSend(result);
        }

        private void SetReccuringTask()
        {
            _deleteClosedConnections = new Task(() => {
                foreach (var client in _clients)
                {
                    if (!client.Value.Connected)
                        _clients.Remove(client.Key);
                }
            });
        }

        private void StartReccuringTask()
        {
            new Thread(() =>
            {
                while(true)
                {
                    SetReccuringTask();
                    var task = _deleteClosedConnections;
                    task.Start();
                    task.Wait();
                    Thread.Sleep(1000);
                }
            }).Start();
        }
    }
}
