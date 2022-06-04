using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SupaStuff.Net.Shared;
using SupaStuff.Net.Packets;
namespace SupaStuff.Net.Server
{
    public class ClientConnection : IDisposable
    {
        public TcpClient tcpClient;
        public NetworkStream stream;
        public Client.Client localClient;
        public bool IsLocal;
        public List<Packet> packetsToWrite = new List<Packet>();
        public bool IsActive = true;
        Client.Client client;
        public HandshakeStage handshakeStage = HandshakeStage.unstarted;
        public PacketStream packetStream;
        public ClientConnection(IAsyncResult ar)
        {
            tcpClient = ServerHost.Instance.listener.EndAcceptTcpClient(ar);
        }
        public ClientConnection(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            tcpClient.NoDelay = false;
            stream = tcpClient.GetStream();
            packetStream = new PacketStream(stream, true, () => false);
            packetStream.clientConnection = this;
        }
        protected ClientConnection()
        {
            this.IsLocal = true;
            localClient = new Client.Client(this);
        }
        public static ClientConnection LocalClient()
        {
            return LocalClientConnection.LocalClient();
        }
        public delegate void OnMessage(Packet packet);
        public event OnMessage onMessage;
        /// <summary>
        /// Send a packet only if it's remote 
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet)
        {
            packetStream.SendPacket(packet);
        }
        public void Update()
        {
            packetStream.Update();
        }
        /// <summary>
        /// Kick the client from the server with a message
        /// </summary>
        /// <param name="message">
        /// The message to be sent
        /// </param>
        public void Dispose()
        {
            Console.WriteLine("Closing Server to Client connection");
            if (IsLocal)
            {
            }
            else
            {
                tcpClient.Close();
                tcpClient.Dispose();
                stream.Close();
                stream.Dispose();
            }
            IsActive = false;
        }
    }
    public enum HandshakeStage
    {
        unstarted,
        complete
    }
}
