
namespace Chattable
{
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;
    using System;
    using System.Linq;
    using RemoteShared;
    using System.Text;
    using SharedData;
    using SharedData.Extensions;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        private string message = string.Empty;

        public string Message
        {

            get
            {
                return this.message;
            }
            set
            {
                this.message = value;
                this.OnPropertyChanged();
            }

        }

        public System.Collections.ObjectModel.ObservableCollection<string> Messages
        {
            get;
            set;
        }
        public ChatPage()
        {
            this.Messages = new System.Collections.ObjectModel.ObservableCollection<string>();
            this.BindingContext = this;
            
            this.InitializeComponent();

            this.ListView.ItemAppearing += (s, e) =>
            {
                if (e.Item == null )
                    return;

                this.ListView.ScrollTo(e.Item, Xamarin.Forms.ScrollToPosition.MakeVisible, true);
            };
            App.client.Message += HandleMessage;
        }

        private void HandleMessage(System.Net.Sockets.Socket sender, string e)
        {
            if (!this.IsVisible)
            {
                return;
            }

            var message = e.FromJson<Datawrapper>();

            switch (message.Type)
            {
                case Datawrapper.MessageType.Chat:
                    HandleChat(message.Data.ChangeType<ChatMessage>());
                    break;


                case Datawrapper.MessageType.User:
                    HandleUser(message.Data.ChangeType<UserStatus>());
                    break;
            }
        }


        private void HandleUser(UserStatus msg)
        {
        }

        private void HandleChat(ChatMessage msg)
        {
            Console.WriteLine($"{msg.Name}: {msg.Message} ");
            Device.BeginInvokeOnMainThread(() =>
            {
                this.Messages.Add($"{msg.Name}: {msg.Message} ");
            });
        }

        public void OnClicked(object sender, EventArgs e)
        {
            App.client.Send(new Datawrapper(new ChatMessage { Message = this.message, Name = App.Name }).ToJson());
            this.Message = string.Empty;
        }
    }
}
