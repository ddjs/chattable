using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteShared
{
    public delegate void RemoteMessage(Socket sender, string e);

    public abstract class RemoteBase
    {
        /// <summary>
        /// The Max packet size we can send. 
        /// </summary>
        public const int MaxPacket = 1024;

        /// <summary>
        /// The Port we use for Connection.
        /// </summary>
        public const int ConnectionPort = 786;

        /// <summary>
        /// Our Header Length.
        /// </summary>
        public const int HeaderLength = 4;

        /// <summary>
        /// The Exit Packet. 
        /// </summary>
        protected static readonly byte[] exitPacket = new byte[] { 0xff, 0xff, 0xff, 0xE, 0x1 };

        /// <summary>
        /// Flag to store Disposed or not.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Flag to store our running State.
        /// </summary>
        protected bool Running { get; set; }

        /// <summary>
        /// The string message Event.
        /// </summary>
        public event RemoteMessage Message;

        /// <summary>
        /// Gets the current Address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The Port we are connecting with
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The Name of the instance.
        /// </summary>
        public string Name { get => this.GetType().Name; }
        public abstract bool Start();

        public abstract void Stop();

        protected virtual bool Send(Socket client, byte[] buffer)
        {
            if (!client.Connected)
            {
                return false;
            }

            client.Send(buffer);
            return true;
        }

        protected virtual async Task ReaderAsync(Socket client, CancellationToken cancel = default(CancellationToken))
        {
            // get the read stream. 
            this.Running = true;
            await Task.Delay(1);

            while (this.Running)
            {
                // create a buffer to store the data from the client.
                var buffer = new byte[MaxPacket + 1];

                try
                {
                    // Receive the data into the buffer
                    // and store the count of data we have.
                    var count = client.Receive(buffer);
                    if (count == 0)
                    {
                        break;
                    }

                    // send the actual bytes to the process method. 
                    this.ProcessPacket(client, buffer.Take(count).ToArray());
                }
                catch
                {
                    // ignore
                    break;
                }
            }
        }

        protected virtual void ProcessPacket(Socket client, byte[] packet)
        {
            if (packet.SequenceEqual(exitPacket))
            {
                // we should exit our reading.
                this.Running = false;
                return;
            }

            this.Message?.Invoke(client, Encoding.UTF8.GetString(packet, 0, packet.Length));
        }

        #region IDisposable Support


        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }
            if (disposing)
            {
                this.Stop();
            }

            isDisposed = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

    public static class RemoteExtensions
    {
        public static byte[] ToByteArray(this string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }
    }
}
