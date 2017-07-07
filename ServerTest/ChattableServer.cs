using RemoteShared;
using System.Net.Sockets;
using SharedData;
using SharedData.Extensions;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ServerTest
{
    public class ChattableServer
    {
        private readonly Server server;

        public ChattableServer(int port)
        {
            server = Server.StartNew(port);
            server.Message += this.Server_Message;
            server.ClientAdded += this.Server_ClientAdded;
            server.ClientRemoved += this.Server_ClientRemoved;

            Task.Run(() => this.ProcessClientsAsync());
        }

        public List<LoginMessage> Members = new List<LoginMessage>();

        internal Queue<TempMember> temporary = new Queue<TempMember>();

        private void Server_ClientRemoved(Socket client)
        {
            lock (this.Members)
            {
                // this.Members.Remove(client);
            }
        }

        private void Server_ClientAdded(Socket client)
        {
            temporary.Enqueue(new TempMember { id = System.Guid.NewGuid(), socket = client });
        }

        private void Server_Message(Socket sender, string e)
        {
            var message = e.FromJson<Datawrapper>();

            switch (message.Type)
            {
                case Datawrapper.MessageType.Chat:
                    HandleChat(sender, message.Data.ChangeType<ChatMessage>());
                    break;

                case Datawrapper.MessageType.Login:
                    HandleLogin(sender, message.Data.ChangeType<LoginMessage>());
                    break;

                case Datawrapper.MessageType.User:
                    HandleUser(sender, message.Data.ChangeType<UserStatus>());
                    break;
            }
        }

        private void HandleUser(Socket sender, UserStatus userStatus)
        {
        }

        private void HandleLogin(Socket sender, LoginMessage msg)
        {
            msg.Status = (this.Members.Count(x => x.Name.ToLower() == msg.Name.ToLower())) < 1;

            if (msg.Status == true)
            {
                this.Members.Add(msg);
                server.UpdateClientList(new Server.ClientObject() { Id = msg.Id, Socket = sender }, true);
            }

            server.NotifyClient(sender, new Datawrapper(msg).ToJson());
        }

        private void HandleChat(Socket sender, ChatMessage chatMessage)
        {
            var msg = new Datawrapper(chatMessage).ToJson();
            server.NotifyClients(msg);
        }

        private async Task ProcessClientsAsync()
        {
            while (true)
            {
                if (this.temporary.Count < 1)
                {
                    await Task.Delay(5);
                    continue;
                }

                var client = this.temporary.Dequeue();
                try
                {
                    server.NotifyClient(client.socket, new Datawrapper(new LoginMessage() { Id = client.id }).ToJson());
                }
                catch
                {
                }
            }
        }
        internal struct TempMember
        {
            public Guid id;
            public Socket socket;
        }
    }
}
