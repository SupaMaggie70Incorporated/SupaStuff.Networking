
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using SupaStuff.Net.Packets;

namespace SupaStuff.Net.Server
{
    public class LocalClientConnection : ClientConnection
    {
        private Client.Client client;
        private LocalClientConnection()
        {
            this.IsLocal = true;
            client = new Client.Client(this);
            Main.ClientLogger.log("Local client initialized");
        }
        public static LocalClientConnection LocalClient()
        {
            return new LocalClientConnection();
        }
        public override void SendPacket(Packet packet)
        {
            client.packetStream.HandleIncomingPacket(packet);
        }
    }
}
