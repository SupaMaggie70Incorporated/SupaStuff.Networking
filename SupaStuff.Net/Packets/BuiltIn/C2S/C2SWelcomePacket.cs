using SupaStuff.Net.ServerSide;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net.Packets.BuiltIn
{
    [APacket(-538927,false,false)]
    internal class C2SWelcomePacket : Packet
    {
        public byte[] bytes;
        public override byte[] Bytify()
        {
            return new byte[0];
        }
        public override void Execute(ClientConnection sender)
        {
            if(Math.ByteArraysEqual(bytes,Server.password))
            {
                sender.finishAuth = true;
            }
            else
            {
                sender.Dispose();
            }
        }
        public C2SWelcomePacket() : base(null)
        {
            bytes = Server.password;
        }
        public C2SWelcomePacket(byte[] bytes) : base(null)
        {
            bytes = bytes;
        }
    }
}
