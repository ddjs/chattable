using RemoteShared;
using System;
using SharedData;
using SharedData.Extensions;

namespace ServerTest
{
    class Program
    {
        private static Client client;

        private static System.Guid id;

        private static string Name;

        public static void Main()
        {
            var chat = new ChattableServer(RemoteBase.ConnectionPort);

            client = new Client("localhost");
            client.Message+= HandleMessage;
            client.Start();

            while (true)
            {
                var message = Console.ReadLine();
                client.Send(new Datawrapper(new ChatMessage { Message = message, Name = Name }).ToJson());
            }
        }

        private static void HandleUser(UserStatus msg)
        {
        }

        private static void HandleLogin(LoginMessage msg)
        {
            if (msg.Status == true)
            {
                Console.WriteLine("Logged In");
                id = msg.Id;
                Name = msg.Name;
            }
            else if (msg.Status == false)
            {
                Console.WriteLine("Logged In Failed");
            }
            else
            {
                msg.Name = "Random";
                client.Send(new Datawrapper(msg).ToJson());
            }
        }

        private static void HandleChat(ChatMessage msg)
        {
            Console.Write($"{msg.Name}: {msg.Message} " + Environment.NewLine);
        }

        private static void HandleMessage(System.Net.Sockets.Socket sender, string e)
        {
            var message = e.FromJson<Datawrapper>();

            switch (message.Type)
            {
                case Datawrapper.MessageType.Chat:
                    HandleChat(message.Data.ChangeType<ChatMessage>());
                    break;

                case Datawrapper.MessageType.Login:
                    HandleLogin(message.Data.ChangeType<LoginMessage>());
                    break;

                case Datawrapper.MessageType.User:
                    HandleUser(message.Data.ChangeType<UserStatus>());
                    break;
            }
        }

        private static void Server_StringMessage(System.Net.Sockets.Socket sender, string e)
        {

        }
    }
}
