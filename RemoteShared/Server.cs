using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteShared
{
    public class Server : RemoteBase
    {
        public delegate void ClientChangedDelegate(Socket client);

        private readonly TcpListener server;

        private readonly List<ClientObject> clients = new List<ClientObject>();

        public Server(int port)
        {
            this.server = new TcpListener(IPAddress.Any, port);
            this.Address = "Server";
            this.Port = port;
        }

        public event ClientChangedDelegate ClientAdded;

        public event ClientChangedDelegate ClientRemoved;

        public bool IsRunning
        {
            get
            {
                return this.Running;
            }
        }

        public static Server StartNew(int port)
        {
            var server = new Server(port);
            server.Start();
            return server;
        }

        public override bool Start()
        {
            if (this.Running)
            {
                return true;
            }

            // set our running flag to true
            this.Running = true;

            // start the server.
            this.server.Start();

            Task.Run(() => this.WaitForClients());

            return true;
        }

        public override void Stop()
        {
            this.server.Stop();
        }

        public void NotifyClients(string message)
        {
            var buffer = message.ToByteArray();

            foreach (var client in this.clients.Where(x => x.Connected))
            {
                this.Send(client.Socket, buffer);
            }
        }

        public void NotifyClient(Socket client, string message)
        {
            this.Send(client, message.ToByteArray());
        }

        private async void WaitForClients()
        {
            while (this.IsRunning)
            {
                // listen for clients. 
                var client = await this.server.AcceptSocketAsync();

                // run a thread to read this client async. 
                Task.Run(() => this.ReaderAsync(client, CancellationToken.None));
            }
        }

        protected override async Task ReaderAsync(Socket client, CancellationToken cancel = default(CancellationToken))
        {
            this.ClientAdded?.Invoke(client);
            await base.ReaderAsync(client, cancel);
            this.ClientRemoved?.Invoke(client);
        }

        public void UpdateClientList(ClientObject client, bool add)
        {
            lock(this.clients)
            {
                if (add)
                {
                    if (this.clients.Any(x=> x.Id.Equals(client.Id)))
                    {
                        return;
                    }

                    this.clients.Add(client);
                }
                else
                {
                    this.clients.Remove(client);
                }
            }
        }

        public struct ClientObject
        {
            public Socket Socket;

            public object Id;

            public bool Connected
            {
                get
                {
                    return this.Socket?.Connected == true;
                }
            }
        }
    }
}
