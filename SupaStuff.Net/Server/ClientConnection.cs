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
        protected TcpClient tcpClient;
        protected NetworkStream stream;
        public bool IsLocal;
        protected List<Packet> packetsToWrite = new List<Packet>();
        public bool IsActive = true;
        protected HandshakeStage handshakeStage = HandshakeStage.unstarted;
        public PacketStream packetStream;
        public IPAddress address;
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
            address = (tcpClient.Client.RemoteEndPoint as IPEndPoint).Address;
        }
        protected ClientConnection()
        {
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
        public virtual void SendPacket(Packet packet)
        {
            packetStream.SendPacket(packet);
        }
        public virtual void Update()
        {
            packetStream.Update();
        }
        /// <summary>
        /// Kick the client from the server with a message
        /// </summary>
        /// <param name="message">
        /// The message to be sent
        /// </param>
        public virtual void Dispose()
        {
            Main.ServerLogger.log("Connection to client " + address + " terminated");
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
