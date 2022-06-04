using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using SupaStuff.Net.Packets;
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
        public ServerHost()
        {
            connections = new List<ClientConnection>(1024);
            localConnection = ClientConnection.LocalClient();
            GetHost();
            listener = new TcpListener(host, port);
            listener.Start();
            listener.BeginAcceptTcpClient(new System.AsyncCallback(ClientConnected), null);
        }

        public void Update()
        {
            for(int i = 0;i < connections.Count;i++)
            {
                ClientConnection connection = connections[i];
                if(connection == null)
                {
                    i--;
                    connections.RemoveAt(i);
                }
                connection.Update();
            }
        }
        private void ClientConnected(System.IAsyncResult ar)
        {
            try
            {
                ClientConnection connection = new ClientConnection(listener.EndAcceptTcpClient(ar));
                connections.Add(connection);
                Console.WriteLine("Someone connected!");
                ClientConnectedEvent(connection);
                listener.BeginAcceptTcpClient(new System.AsyncCallback(ClientConnected), null);
            }catch
            {
                if(this != null)
                {
                    Dispose();
                }
            }
        }
        public void Dispose()
        {
            foreach (var connection in connections)
            {
                try
                {
                    connection.Dispose();
                }
                catch
                {

                }
            }
            listener.Stop();
            Instance = null;
            connections.Clear();
        }
        public event Action<ClientConnection> OnClientConnected;
        private void ClientConnectedEvent(ClientConnection connection)
        {
            if (OnClientConnected == null) return;
            OnClientConnected.Invoke(connection);
        }

    }
}
