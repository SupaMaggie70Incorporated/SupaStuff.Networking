using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SupaStuff.Net.Server;
using SupaStuff.Net.Shared;
namespace SupaStuff.Net.Client
{
    public class Client : IDisposable
    {
        public TcpClient tcpClient;
        public NetworkStream stream;
        public static Client Instance;
        public bool IsLocal;
        public ClientConnection localConnection;
        public PacketStream packetStream;
        // Start is called before the first frame update
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
        public void SendPacket(Packet.Packet packet)
        {
            packetStream.SendPacket(packet);
        }
        /// <summary>
        /// Try to recieve and write packets
        /// </summary>
        public void OnUpdate()
        {
            if (!IsLocal)
            {
                packetStream.Update();
            }
        }
        public delegate void OnMessage(Packet.Packet packet);
        public event OnMessage onMessage;
        /// <summary>
        /// GC assister
        /// </summary>
        public void Dispose()
        {
            Instance = null;
            stream.Close();
            stream.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
        }
    }
}
