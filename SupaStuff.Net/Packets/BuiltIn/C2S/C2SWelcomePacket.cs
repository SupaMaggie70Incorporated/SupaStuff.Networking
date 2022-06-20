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
            return bytes;
        }
        public override void Execute(ClientConnection sender)
        {
            if(Math.ByteArraysEqual(bytes,Server.password))
            {
                sender.finishAuth = true;
                Main.ServerLogger.log(sender.address + " finished authorizing!");
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
            this.bytes = bytes;
        }
    }
}
