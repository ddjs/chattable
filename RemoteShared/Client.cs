using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteShared
{
    public class Client : RemoteBase
    {
        /// <summary>
        /// The socket we use to connect to the server.
        /// </summary>
        private TcpClient client = new TcpClient();

        /// <summary>
        /// The cancellation token for reading from the server.
        /// </summary>
        private readonly CancellationTokenSource cancel = new CancellationTokenSource();

        /// <summary>
        /// The reading Task. 
        /// </summary>
        private Task readingTask;

        /// <summary>
        /// Create an Instance of a Client
        /// </summary>
        /// <param name="ipAddress"> The Address of the server we wish to connect to</param>
        /// <param name="port">The Port of the server. </param>
        public Client(string ipAddress, int port = RemoteBase.ConnectionPort)
        {
            this.Address = ipAddress;
            this.Port = port;
        }

        /// <summary>
        /// Lets us know if we are connected. 
        /// </summary>
        public bool Connected
        {
            get
            {
                return this.client != null && this.client.Connected;
            }
        }

        public EndPoint RemoteEndPoint
        {
            get
            {
                return this.client.Client.RemoteEndPoint;
            }
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                return this.client.Client.LocalEndPoint;
            }
        }

        /// <summary>
        /// Starts the Connection to the server. 
        /// </summary>
        /// <returns>
        /// Successful connection returns true;
        /// </returns>
        public override bool Start()
        {
            if (this.Connected)
            {
                return true;
            }

            if (this.client == null)
            {
                this.client = new TcpClient();
            }

            // await the connection.
            this.client.ConnectAsync(this.Address, this.Port).Wait(5000);

            if (this.Connected)
            {
                // Start the reading Thread.
                this.readingTask = Task.Run(() => this.ReaderAsync(this.client.Client, cancel.Token));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the connection to the server.
        /// </summary>
        public override void Stop()
        {
            // notify server we are exiting. 
            this.Send(this.client.Client, exitPacket);

            // cancel our reading Task by use of the cancellation token.
            cancel.Cancel(false);

            // wait for the Task to end. 
            while (this.readingTask.Status == TaskStatus.Running)
            {
            }

            // dispose of the client. 
            this.client.Dispose();
            this.client = null;
        }

        /// <summary>
        /// Sends a string message to the server
        /// </summary>
        /// <param name="message">
        /// the string to send
        /// </param>
        public void Send(string message)
        {
            base.Send(this.client.Client, message.ToByteArray());
        }
    }
}
