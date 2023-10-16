using Commentus_web.Extensions;
using Commentus_web.Models;
using Commentus_web.Networking.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Net.Sockets;

namespace Commentus_web.Networking
{
    public class ClientsContainer : IClientsContainer
    {
        private List<Client> _clients { get; set; }

        public List<Client> Clients { get { return _clients; } }

        public ClientsContainer()
        {
            _clients = new();

            StartReccuringTask();
        }

        public void AddClient(HttpContext httpContext)
        {
            _clients.Add(new(httpContext));
        }

        public void Send(HttpContext httpContext, byte[] dataBuffer)
        {
            var name = httpContext.Session.GetString("Name");

            var client = _clients.Where(x => x.Name == name).FirstOrDefault();

            if (client is not null)
                client.Socket.Send(dataBuffer);
        }

        public void ChangeLastMessageTimeStamp(HttpContext httpContext, TestContext context, int roomId, DateTime? timeStamp = default)
        {
            var name = httpContext.Session.GetString("Name");

            var client = _clients.Where(x => x.Name == name).FirstOrDefault();

            if (client is not null)
            {
                client.LastMessageTimeStamp = !timeStamp.Equals(default) ? timeStamp : context.RoomsMessages.GetLastMessageTimeStamp(roomId);
            }
        }

        public async Task<int> ReceiveMessage(HttpContext httpContext)
        {
            var name = httpContext.Session.GetString("Name");

            var client = _clients.Where(x => x.Name == name).FirstOrDefault();

            var buffer = new byte[CommunicationProtocol.BUFFER_SIZE];

            if(client is not null)
                return await client.Socket.ReceiveAsync(buffer, SocketFlags.None);
            return 0;
        }

        private void StartReccuringTask()
        {
            new Thread(() =>
            {
                foreach(var client in _clients)
                {
                    if (!client.Socket.Connected)
                        _clients.Remove(client);
                }

                Thread.Sleep(1000);
            }).Start();
        }
    }
}
