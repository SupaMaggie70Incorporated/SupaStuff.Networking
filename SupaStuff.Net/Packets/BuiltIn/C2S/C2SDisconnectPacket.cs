using SupaStuff.Net.ServerSide;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupaStuff.Net.Packets.BuiltIn
{
    [APacket(-1, false)]
    internal sealed class C2SDisconnectPacket : Packet
    {
        public string message;
        public static readonly string defaultMessage = "We are leaving now, goodbye!";
        public override byte[] Bytify()
        {
            return Encoding.ASCII.GetBytes(message);
        }
        public override void Execute(ClientConnection sender)
        {
            Main.ServerLogger.log("Client " + sender.address.ToString() + " disconnected from server for:\n    " + message);
            sender.Dispose();
        }
        public C2SDisconnectPacket() : base(null)
        {
            message = defaultMessage;
        }
        public C2SDisconnectPacket(byte[] bytes) : base(bytes)
        {
            message = Encoding.ASCII.GetString(bytes);
        }
        public C2SDisconnectPacket(string message) : base(null)
        {
            this.message = message;
        }
    }
}
