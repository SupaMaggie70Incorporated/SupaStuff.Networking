﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using SupaStuff.Net.Packets;
namespace SupaStuff.Net.ServerSide
{
    public class Server : IDisposable
    {
        public static Server Instance;
        public TcpListener listener;
        public static IPAddress host;
        public static int port = 12345;
        public bool IsActive = true;
        public LocalClientConnection localConnection;
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
        public Server()
        {
            connections = new List<ClientConnection>(1024);
            GetHost();
            listener = new TcpListener(host, port);
            StartListening();
            Console.WriteLine("Server started");
            listener.BeginAcceptTcpClient(new System.AsyncCallback(ClientConnected), null);
            Console.WriteLine("Accepting tcp client");
            localConnection = ClientConnection.LocalClient();
            connections.Add(localConnection);
            Main.NetLogger.log("Server started!");
        }
        public void StartListening()
        {
            listener.Start();
        }
        public void StopListening()
        {
            listener.Stop();
        }

        public void Update()
        {
            for (int i = 0; i < connections.Count; i++)
            {
                ClientConnection connection = connections[i];
                if (connection == null)
                {
                    i--;
                    connections.RemoveAt(i);
                }
                try { 
                    connection.Update();
                }
                catch
                {
                    connection.Dispose();
                }
            }
        }
        private void ClientConnected(System.IAsyncResult ar)
        {
            try
            {
                ClientConnection connection = new ClientConnection(listener.EndAcceptTcpClient(ar));
                connections.Add(connection);
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
            Main.NetLogger.log("Closing server");
        }
        public event Action<ClientConnection> OnClientConnected;
        private void ClientConnectedEvent(ClientConnection connection)
        {
            if (OnClientConnected == null) return;
            OnClientConnected.Invoke(connection);
        }
        public void SendToAll(Packet packet)
        { 
            foreach(ClientConnection connection in connections) 
            {
                connection.SendPacket(packet);
            }
        }
        public void Kick(ClientConnection connection,string message)
        {
            connection.Kick(message);
        }
        public void Kick(ClientConnection connection)
        {
            connection.Dispose();
        }
        public LocalClientConnection MakeLocalConnection()
        {
            LocalClientConnection connection = LocalClientConnection.LocalClient();
            connections.Add(connection);
            return connection;
        }

    }
}
