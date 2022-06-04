using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SupaStuff.Net
{
    public static class Main
    {
        //public static Logger NetLogger;
        public static void Init()
        {
            //NetLogger = Logger.GetLogger("SupaStuff.Net");
            Packet.Util.Bytifier.Init();
            Packet.Util.PacketTypesFinder.GetTypes();
            Server.ServerHost.GetHost();
        }
    }
}
