using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupaStuff.Net.Server
{
    public class ClientConnection : IDisposable
    {
        public TcpClient tcpClient;
        public NetworkStream stream;
        public Client.Client localClient;
        public bool IsLocal;
        public readonly ServerClientData clientData;
        public List<Packet.Packet> packetsToWrite = new List<Packet.Packet>();
        public bool IsActive = true;
        Client.Client client;
        public HandshakeStage handshakeStage = HandshakeStage.unstarted;
        public ClientConnection(IAsyncResult ar)
        {
            clientData = new ServerClientData(this);
            tcpClient = ServerHost.Instance.listener.EndAcceptTcpClient(ar);
            tcpClient.NoDelay = false;
            stream = tcpClient.GetStream();
            Squirrelgame.ServerLogger.log("Recieved connection from " + tcpClient);
        }
        private ClientConnection()
        {
            clientData = new ServerClientData(this);
            this.IsLocal = true;
            localClient = new Client.Client(this);
        }
        public static ClientConnection LocalClient()
        {
            return new ClientConnection();
        }
        public delegate void OnMessage(Packet.Packet packet);
        public event OnMessage onMessage;
        public void SendPacket(Packet.Packet packet)
        {
            if (!IsLocal)
            {
                if (tcpClient != null && stream != null)
                {
                    if (!tcpClient.Connected)
                    {
                        Dispose();
                        return;
                    }
                    if (!IsActive || !tcpClient.Connected) throw new NullReferenceException("Null reference: This stream has already been closed!");
                }
                else
                {
                    throw new NullReferenceException("Null reference: The external connection has already been disposed!");
                }
            }
            else
            {
                client.RecievePacket(packet);
            }
        }
        public void WritePacket(Packet.Packet packet)
        {
            SendPacketTask(packet).Start();
        }
        public void TryRecievePacket()
        {
            while (stream.DataAvailable)
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
                    Kick("Invalid packet!");
                }
                else
                {
                    packetbytes = buffer;
                    Packet.Packet packet = Packet.Packet.GetPacket(packetid, packetbytes, false);
                    RecievePacket(packet);
                }
            }
        }
        public void TryWritePacket()
        {
            if (packetsToWrite.Count <= 0)
            {
                return;
            }
            WritePacket(packetsToWrite[0]);
            packetsToWrite.RemoveAt(0);
        }
        public void Update()
        {
            TryWritePacket();
            TryRecievePacket();
        }
        public void RecievePacket(Packet.Packet packet)
        {
            ServerHost.Instance.RecievePacket(this, packet);
        }
        /// <summary>
        /// Kick the client from the server with a message
        /// </summary>
        /// <param name="message">
        /// The message to be sent
        /// </param>
        public void Kick(string message)
        {
            //SendPacket(new KickFromServerPacket(message));
            Dispose();
        }
        public void Kick()
        {
            Kick("Kicked from server!");
        }
        public void Dispose()
        {
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
            clientData.Dispose();
        }
        public async Task SendPacketTask(Packet.Packet packet)
        {
            byte[] bytes = Packet.Packet.EncodePacket(packet);
            await stream.WriteAsync(bytes, 0, bytes.Length);

        }
        public void OnSendPacketComplete()
        {
            TryWritePacket();
        }
    }
    public enum HandshakeStage
    {
        unstarted,
        complete
    }
}
