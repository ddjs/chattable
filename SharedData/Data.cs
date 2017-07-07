namespace SharedData
{
    public class ChatMessage
    {
        public string Name { get; set; }

        public System.Guid ToId { get; set; }

        public string Message { get; set; }
    }

    public class LoginMessage
    {
        public string Name { get; set; }

        public bool? Status { get; set; }

        public System.Guid Id { get; set; }
    }

    public class UserStatus
    {
        public string Name { get; set; }

        public bool Connected { get; set; }

        public System.Guid Id { get; set; }
    }

    public class Datawrapper
    {
        public enum MessageType : byte
        {
            Chat = 1,
            Login,
            User
        }

        public Datawrapper(object data)
        {
            if (data is UserStatus)
            {
                this.Type = MessageType.User;
            }
            else if (data is LoginMessage)
            {
                this.Type = MessageType.Login;
            }
            else if (data is ChatMessage)
            {
                this.Type = MessageType.Chat;
            }
            else
            {
                return;
            }

            this.Data = data;
        }

        public Datawrapper()
        {

        }
        public MessageType Type { get; set; }

        public object Data { get; set; }

    }
}
