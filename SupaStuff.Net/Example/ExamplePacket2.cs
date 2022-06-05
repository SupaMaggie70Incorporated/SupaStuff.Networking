using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupaStuff.Net.Packets;
using SupaStuff.Net.ServerSide;

namespace SupaStuff.Net.Example
{
    [APacket(2,true)]
    public class ExamplePacket2 : Packet
    {
        public override byte[] Bytify()
        {
            return new byte[0];
        }
        public override void Execute(ClientConnection sender)
        {
            Console.WriteLine("S2C packet recieved!");
        }
        public ExamplePacket2(byte[] bytes) : base(bytes)
        {
        }
        public ExamplePacket2() : base(null)
        {
        }
    }
}
