using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SupaStuff.Net.Shared;
using SupaStuff.Net.Packets;
using SupaStuff.Net.Packets.BuiltIn;
namespace SupaStuff.Net.ServerSide
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

        internal DateTime connectionStarted;
        internal bool finishAuth = false;
        public ClientConnection(IAsyncResult ar)
        {
            tcpClient = Server.Instance.listener.EndAcceptTcpClient(ar);
        }
        public ClientConnection(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            tcpClient.NoDelay = false;
            stream = tcpClient.GetStream();
            packetStream = new PacketStream(stream, true, () => false);
            packetStream.clientConnection = this;
            packetStream.OnDisconnected += Dispose;
            address = (tcpClient.Client.RemoteEndPoint as IPEndPoint).Address;
            connectionStarted = DateTime.UtcNow;
        }
        protected ClientConnection()
        {
        }
        internal static LocalClientConnection LocalClient()
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
            if (!IsActive) Console.WriteLine("Wait, why am I still being updated?");
            packetStream.Update();
            if (!finishAuth)
            {
                if (Math.TimeSince(connectionStarted) > 10) {
                    Main.ServerLogger.log("Shutting down connection to " + address + " because they were unable to authorize themselves");
                    Dispose();
                }
            }
        }
        /// <summary>
        /// Kick the client from the server with a message
        /// </summary>
        /// <param name="message">
        /// The message to be sent
        /// </param>
        public virtual void Dispose()
        {
            if (!IsActive) return;
            IsActive = false;
            Main.ServerLogger.log("Connection to client " + address + " terminated");
            try
            {
                Server.Instance.connections.Remove(this);
            }
            catch
            {

            }
            if (IsLocal)
            {
            }
            else
            {
                tcpClient.Close();
                tcpClient.Dispose();
                stream.Close();
                stream.Dispose();
                packetStream.Dispose();
            }
            IsActive = false;
        }
        public virtual void Kick(string message)
        {
            lock (packetStream.packetsToWrite)
            {
                packetStream.packetsToWrite.Clear();
                packetStream.packetsToWrite.Add(new S2CKickPacket(message));
            }
        }
        public virtual void Kick()
        {
            lock (packetStream.packetsToWrite)
            {
                packetStream.packetsToWrite.Clear();
                packetStream.packetsToWrite.Add(new S2CKickPacket());
            }
        }
    }
    public enum HandshakeStage
    {
        unstarted,
        complete
    }
}
