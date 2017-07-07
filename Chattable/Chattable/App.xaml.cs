using Xamarin.Forms;
using RemoteShared;

namespace Chattable
{
	public partial class App : Application
	{
         public static readonly Client client = new Client("localhost");

        public static LoginPage loginPage = new LoginPage();

        public static ChatPage chatPage = new ChatPage();
		public App ()
		{
            Instance = this;
			
            InitializeComponent();
          
			MainPage = loginPage;
		}

        public static App Instance
            {get; private set;}

        public static string Name
            {get;set;}


        public static System.Guid Id
            {get;set;}
		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
