﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
namespace SupaStuff.Net.Client
{
    public static class LanScan
    {
        /// <summary>
        /// Gets the games on the current local network
        /// </summary>
        /// <returns></returns>
        public static List<IPEndPoint> GetLanGames()
        {
            List<IPEndPoint> list = new List<IPEndPoint>();
            return list;
        }
    }
}
