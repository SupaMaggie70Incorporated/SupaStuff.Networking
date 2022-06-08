using SupaStuff.Net.ServerSide;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net.Packets.BuiltIn
{
    [APacket(-538927,false,false)]
    internal class C2SWelcomePacket : Packet
    {
        public override byte[] Bytify()
        {
            return new byte[0];
        }
        public override void Execute(ClientConnection sender)
        {
            sender.finishAuth = true;
        }
        public C2SWelcomePacket() : base(null)
        {

        }
        public C2SWelcomePacket(byte[] bytes) : base(null)
        {

        }
    }
}
