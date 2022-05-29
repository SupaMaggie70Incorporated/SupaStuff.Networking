using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SupaStuff.Net.Server;

namespace SupaStuff.Net.Client
{
    public class Client : IDisposable
    {
        public TcpClient tcpClient;
        public NetworkStream stream;
        public static Client Instance;
        public bool IsLocal;
        public ClientConnection localConnection;
        // Start is called before the first frame update
        public Client(IPAddress ip)
        {
            //External client

            IsLocal = false;
            Instance = this;
            Main.NetLogger.log("Client started");
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
            if (IsLocal)
            {
                localConnection.RecievePacket(packet);
            }
            else
            {
                if (stream.CanWrite && tcpClient.Connected)
                {
                    byte[] buffer = packet.Bytify();
                    stream.Write(buffer, 0, buffer.Length);
                }
                else throw new Exception("Unable to send packet: server terminated connection");
            }
        }
        /// <summary>
        /// Try to recieve and write packets
        /// </summary>
        public void OnUpdate()
        {
            if (!IsLocal)
            {
                TryRecievePacket();
            }
        }
        /// <summary>
        /// Execute the packet given
        /// </summary>
        /// <param name="packet"></param>
        public void RecievePacket(Packet.Packet packet)
        {
            packet.Execute(null);
        }
        /// <summary>
        /// Finish recieving a packet
        /// </summary>
        /// <returns></returns>
        public Packet.Packet RecievePacket()
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, buffer.Length);
            int packetid = BitConverter.ToInt32(buffer, 0);
            int size = BitConverter.ToInt32(buffer, 4);
            if (size > 1024)
                buffer = new byte[size];
            int recievesize = stream.Read(buffer, 0, size);
            byte[] packetbytes;
            if (recievesize != size)
            {
                Dispose();
                return null;
            }
            else
            {
                packetbytes = buffer;
                Packet.Packet packet = Packet.Packet.GetPacket(packetid, packetbytes, false);
                return packet;
            }

        }
        /// <summary>
        /// See if there are any packets to be recieved
        /// </summary>
        public void TryRecievePacket()
        {
            if (!tcpClient.Connected)
            {
                Dispose();
            }
            if (stream.DataAvailable)
            {
                RecievePacket(RecievePacket());
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
