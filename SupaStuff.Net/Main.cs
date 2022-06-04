using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupaStuff.Core.Util;

namespace SupaStuff.Net
{
    public static class Main
    {
        public static Logger NetLogger;
        public static Logger ServerLogger;
        public static Logger ClientLogger;
        public static void Init()
        {
            NetLogger = Logger.GetLogger("Net/Main");
            ServerLogger = Logger.GetLogger("Net/Server");
            ClientLogger = Logger.GetLogger("Net/Client");
            Packets.Util.Bytifier.Init();
            Packets.Util.PacketTypesFinder.GetTypes();
            ServerSide.Server.GetHost();
        }
    }
}
