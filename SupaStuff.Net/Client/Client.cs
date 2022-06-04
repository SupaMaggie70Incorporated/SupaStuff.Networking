using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SupaStuff.Net.Server;
using SupaStuff.Net.Shared;
using SupaStuff.Net.Packets;

namespace SupaStuff.Net.Client
{
    public class Client : IDisposable
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        public static Client Instance;
        public bool IsLocal;
        public ClientConnection localConnection;
        public PacketStream packetStream;
        /// <summary>
        /// Create a client and attempt to connect to server
        /// </summary>
        /// <param name="ip"></param>
        public Client(IPAddress ip)
        {
            //External client
            IsLocal = false;
            Instance = this;
            //Server script will calculate the local IP stuff for us
            ServerHost.GetHost();
            //New client to connect with
            tcpClient = new TcpClient();
            //How long to wait
            TimeSpan timeSpan = TimeSpan.FromSeconds(1);
            //Try to connect to server
            var result = tcpClient.BeginConnect(ip, ServerHost.port, null, null);
            //Wait a second, if it hasnt worked it cant connect
            var success = result.AsyncWaitHandle.WaitOne(timeSpan);
            if (!success)
            {
                return;
            }
            //Get the stream
            stream = tcpClient.GetStream();
            packetStream = new PacketStream(stream, false, () => false);

        }
        /// <summary>
        /// Create a local client connection
        /// </summary>
        /// <param name="localconnection"></param>
        public Client(ClientConnection localconnection)
        {
            //Local client
            this.IsLocal = true;
            Instance = this;
            ServerHost.GetHost();
            localConnection = localconnection;
        }
        /// <summary>
        /// Send a packet over the stream
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet)
        {
            packetStream.SendPacket(packet);
        }
        /// <summary>
        /// Try to recieve and write packets
        /// </summary>
        public void Update()
        {
            if (!IsLocal)
            {
                packetStream.Update();
            }
            if(!tcpClient.Connected) Dispose();
        }
        public delegate void OnMessage(Packet packet);
        public event OnMessage onMessage;
        /// <summary>
        /// GC assister
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("Client closing");
            Instance = null;
            stream.Close();
            stream.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
        }
    }
}
