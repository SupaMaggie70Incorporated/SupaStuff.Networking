using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupaStuff.Net.Packet;
using SupaStuff.Net.Server;

namespace SupaStuff.Net.Example
{
    [APacket(0 , false)]
    public class ExamplePacket : Packet.Packet
    {
        public override void Execute(ClientConnection sender)
        {
            return;
        }
        public byte[] bytify()
        {
            return new byte[0];
        }
        public ExamplePacket(byte[] bytes) : base(bytes)
        {

        }
        public ExamplePacket() : base(new byte[0])
        {

        }
    }
}
