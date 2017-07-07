using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SharedData;
using SharedData.Extensions;

namespace Chattable
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private string name = string.Empty;
        public LoginPage()
        {
            this.BindingContext = this;
            InitializeComponent();
            App.client.Message += this.Client_Message;
        }

        private void Client_Message(System.Net.Sockets.Socket sender, string e)
        {
            if (!this.IsVisible)
            {
                return;
            }

            var message = e.FromJson<Datawrapper>();

            switch (message.Type)
            {
                case Datawrapper.MessageType.Login:
                    HandleLoginAsync(message.Data.ChangeType<LoginMessage>());
                    break;
            }
        }

        private async Task HandleLoginAsync(LoginMessage msg)
        {
            if (msg.Status == true)
            {
                App.Id = msg.Id;
                App.Name = msg.Name;
                Device.BeginInvokeOnMainThread(() =>
                {
                    App.Instance.MainPage = App.chatPage;
                });
            }
            else if (msg.Status == false)
            {
                await DisplayAlert("Error", $"User-name {this.name} is already Taken", "OK");
                App.client.Stop();
            }
            else
            {
                msg.Name = this.name;
                App.client.Send(new Datawrapper(msg).ToJson());
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public void OnClicked(object sender, EventArgs e)
        {
            App.client.Start();
        }

    }
}
