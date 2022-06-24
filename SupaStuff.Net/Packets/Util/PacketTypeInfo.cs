using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net.Packets.Util
{
    internal struct PacketTypeInfo
    {
        public Type type;
        public Func<byte[], Packet> constructor;
        public Func<int, bool> isRightLength;

    }
}
