using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security;
namespace SupaStuff.Net.Server
{
    public class ServerHost : IDisposable
    {
        public static ServerHost Instance;
        public TcpListener listener;
        public static IPAddress host;
        public static int port = 12345;
        public bool IsActive = true;
        public ClientConnection localConnection;
        public List<ClientConnection> connections;
        public static void GetHost()
        {
            var hosts = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hosts.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    host = ip;
                    return;
                }
            }
        }
        // Start is called before the first frame update
        public ServerHost()
        {
            localConnection = ClientConnection.LocalClient();
            GetHost();
            listener = new TcpListener(host, port);
            listener.Start();
            while (IsActive)
            {
                listener.BeginAcceptTcpClient(new System.AsyncCallback(ClientConnected), null);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void ClientConnected(System.IAsyncResult ar)
        {
            new ClientConnection(ar);
        }
        public void RecievePacket(ClientConnection player, Packet.Packet packet)
        {
            packet.Execute(player);
        }
        public void Dispose()
        {
            foreach (var connection in connections)
            {
                connection.Dispose();
            }
            listener.Stop();
            Instance = null;
            connections.Clear();
        }

    }
}
