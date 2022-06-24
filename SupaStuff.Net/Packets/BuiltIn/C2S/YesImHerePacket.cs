using SupaStuff.Net.ServerSide;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net.Packets.BuiltIn
{
    [APacket(-194752,false,false)]
    internal class YesImHerePacket : Packet
    {
        public override byte[] Bytify()
        {
            return new byte[0];
        }
        public override void Execute(ClientConnection sender)
        {
        }
        public YesImHerePacket() : base(null)
        {

        }
        public YesImHerePacket(byte[] bytes) : base(null)
        {
        }

        public static bool IsAllowedSize(int size)
        {
            return size == 0;
        }
    }
}
