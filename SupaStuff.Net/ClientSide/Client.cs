using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SupaStuff.Net.ServerSide;
using SupaStuff.Net.Shared;
using SupaStuff.Net.Packets;
using SupaStuff.Net.Packets.BuiltIn;

namespace SupaStuff.Net.ClientSide
{
    public class Client : IDisposable
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        public static Client Instance = null;
        public bool IsLocal;
        public bool IsActive;
        public LocalClientConnection localConnection;
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
            Server.GetHost();
            //New client to connect with
            tcpClient = new TcpClient();
            /*
            //How long to wait
            TimeSpan timeSpan = TimeSpan.FromSeconds(1);
            //Try to connect to server
            var result = tcpClient.BeginConnect(ip, Server.port, null, null);
            //Wait a second, if it hasnt worked it cant connect
            var success = result.AsyncWaitHandle.WaitOne(timeSpan);
            if (!success)
            {
                return;
            }
            */
            tcpClient.Connect(new IPEndPoint(ip, Server.port));
            //Get the stream
            stream = tcpClient.GetStream();
            packetStream = new PacketStream(stream, false, () => { Main.ClientLogger.log("Disconnecting because we ran into an undisclosed network error"); Dispose();return false; });
            packetStream.OnDisconnected += () => { Main.ClientLogger.log("Disconnected because of network error/server kicking us"); Dispose(); };
            packetStream.logger = Main.ClientLogger;
            Main.ClientLogger.log("Client started!");
            System.Threading.Thread.Sleep(1000);
            SendPacket(new C2SWelcomePacket());
        }
        /// <summary>
        /// Create a local client connection
        /// </summary>
        /// <param name="localconnection"></param>
        internal Client(LocalClientConnection localconnection)
        {
            //Local client
            this.IsLocal = true;
            Instance = this;
            Server.GetHost();
            localConnection = localconnection;

        }
        /// <summary>
        /// Send a packet over the stream
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet)
        {
            if (IsLocal) localConnection.RecievePacket(packet);
            else packetStream.SendPacket(packet);
        }
        public void RecievePacket(Packet packet)
        {
            packet.Execute(null);
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
            if (!tcpClient.Connected)
            {
                Main.ClientLogger.log("Disonnected because the tcpclient was not connected?");
                Dispose();
            }
        }
        public delegate void OnMessage(Packet packet);
        public event OnMessage onMessage;
        /// <summary>
        /// GC assister
        /// </summary>
        public void Dispose()
        {
            if (!IsActive) return;
            DisconnectEvent();
            IsActive = false;
            Main.ClientLogger.log("Client closed!");
            Instance = null;
            stream.Close();
            stream.Dispose();
            tcpClient.Close();
            tcpClient.Dispose();
            packetStream.Dispose();
            if (IsLocal)
            {
                Main.ClientLogger.log("Closing client and its corresponding local connection!");
                localConnection.Dispose();
            }
        }
        public void Disconnect()
        {
            packetStream.packetsToWrite.Clear();
            packetStream.packetsToWrite.Add(new C2SDisconnectPacket());

        }
        public void Disconnect(string message)
        {
            packetStream.packetsToWrite.Clear();
            packetStream.packetsToWrite.Add(new C2SDisconnectPacket(message));
        }
        public void HardDisconnect()
        {
            Main.ClientLogger.log("Hard disconnecting");
            Dispose();
        }
        private void DisconnectEvent()
        {
            if(OnDisconnect != null) OnDisconnect();
        }
        public event Action OnDisconnect;
    }
}
