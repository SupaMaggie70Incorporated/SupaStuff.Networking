
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;


namespace SupaStuff.Net.Server
{
    public class LocalClientConnection : ClientConnection
    {
        public LocalClientConnection(IAsyncResult ar)
        {
            tcpClient = ServerHost.Instance.listener.EndAcceptTcpClient(ar);
            tcpClient.NoDelay = false;
            stream = tcpClient.GetStream();
        }
        private LocalClientConnection()
        {
            this.IsLocal = true;
            localClient = new Client.Client(this);
        }
        public static LocalClientConnection LocalClient()
        {
            return new LocalClientConnection();
        }
        public void SendPacket()
        {

        }
    }
}
