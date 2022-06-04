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
        public short num;
        public override void Execute(ClientConnection sender)
        {
            Console.WriteLine("Number: " + num.ToString());
            return;
        }
        public override byte[] Bytify()
        {
            byte[] data = new byte[2];
            byte[] shortbytes =  BitConverter.GetBytes(num);
            Buffer.BlockCopy(shortbytes, 0, data, 0, 2);
            return data;
        }
        public ExamplePacket(byte[] bytes) : base(bytes)
        {
            num = BitConverter.ToInt16(bytes, 0);
        }
        public ExamplePacket(short num) : base(new byte[0])
        {
            this.num = num;
        }
    }
}
