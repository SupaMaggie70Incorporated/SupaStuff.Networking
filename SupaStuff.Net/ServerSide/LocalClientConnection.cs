
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SupaStuff.Net.Packets;

namespace SupaStuff.Net.ServerSide
{
    public class LocalClientConnection : ClientConnection
    {
        public ClientSide.Client client { get; internal set; }
        private LocalClientConnection()
        {
            this.IsLocal = true;
            client = new ClientSide.Client(this);
            Main.ClientLogger.log("Local client initialized");
        }
        internal static LocalClientConnection LocalClient()
        {
            return new LocalClientConnection();
        }
        public override void SendPacket(Packet packet)
        {
            client.RecievePacket(packet);
        }
        public void RecievePacket(Packet packet)
        {
            packet.Execute(this);
        }
        public override void Update()
        {
            return;
        }
        public override void Kick()
        {
        }
        public override void Kick(string message)
        {
        }
    }
}
