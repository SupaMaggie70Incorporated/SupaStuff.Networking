using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupaStuff.Net.Packets
{
    public class PacketException : Exception
    {
        public readonly string message;
        public PacketException()
        {
            message = "Packet exception occured!";
        }
        public PacketException(string message)
        {
            message = "Packet exception: " + message;
        }
        public override string ToString()
        {
            return message;
        }
    }
}
