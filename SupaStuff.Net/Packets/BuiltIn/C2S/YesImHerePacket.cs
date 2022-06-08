using SupaStuff.Net.ServerSide;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net.Packets.BuiltIn
{
    [APacket(-194752,false)]
    internal class YesImHerePacket : Packet
    {
        public override byte[] Bytify()
        {
            return base.Bytify();
        }
        public override void Execute(ClientConnection sender)
        {
            sender.lastCheckedIn = DateTime.Now;
        }
        public YesImHerePacket() : base(null)
        {

        }
        public YesImHerePacket(byte[] bytes) : base(null)
        {
        }
    }
}
