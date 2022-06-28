using SupaStuff.Net.ServerSide;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net.Packets.BuiltIn
{
    [APacket(-538927,false,false)]
    internal class C2SWelcomePacket : Packet
    {
        public readonly byte[] bytes;
        public override byte[] Bytify()
        {
            Main.ClientLogger.log("Bytifying password packet with password " + Encoding.ASCII.GetString(bytes));
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
                Main.ServerLogger.log(sender.address + " entered the wrong password! It was " + Encoding.ASCII.GetString(Server.password) + " but they thought it was " + Encoding.ASCII.GetString(bytes));
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
        public static bool IsAllowedSize(int size)
        {
            return size == Server.password.Length;
        }
    }
}
