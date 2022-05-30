using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupaStuff.Net.Packet;
using SupaStuff.Net.Server;

namespace SupaStuff.Net.Example
{
    [APacket(1 , false)]
    public class ExamplePacket : Packet.Packet
    {
        public override void Execute(ClientConnection sender)
        {
            return;
        }
        public override byte[] Bytify()
        {
            return new byte[2];
        }
        public ExamplePacket(byte[] bytes) : base(bytes)
        {

        }
        public ExamplePacket() : base(new byte[0])
        {

        }
    }
}
