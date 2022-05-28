using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Squirrelgame.ClientLogger.log("Client started");
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
                Squirrelgame.ClientLogger.log("Unable to connect");
                return;
            }
            //Get the stream
            stream = tcpClient.GetStream();
            //Handshake packet
            SendPacket(new HandshakeClient());
            //hi
        }
        public Client(ClientConnection localconnection)
        {
            //Local client
            this.IsLocal = true;
            Instance = this;
            Squirrelgame.ClientLogger.log("Client started");
            ServerHost.GetHost();
            SendPacket(new HandshakeClient());
            localConnection = localconnection;
        }
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
        public void OnUpdate()
        {
            if (!IsLocal)
            {
                TryRecievePacket();
            }
        }
        public void RecievePacket(Packet.Packet packet)
        {
            packet.Execute(null);
        }
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
