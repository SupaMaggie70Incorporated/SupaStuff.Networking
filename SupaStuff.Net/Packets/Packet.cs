using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupaStuff.Net.Packets.Util;
using System.Reflection;
using SupaStuff.Net.ClientSide;
using SupaStuff.Net.ServerSide;
namespace SupaStuff.Net.Packets
{//this is where we make errors
    public abstract class Packet : IBytifiable, IDisposable
    {
        public static int id;
        internal DateTime startTime = DateTime.UtcNow;

        public Packet(byte[] bytes) : base(bytes)
        {
        }

        public int getMaxTime()
        {
            return 1000;
        }
        public static bool IsAllowedSize(int size)
        {
            return true;
        }
        public static Packet GetPacket(byte[] contents, bool isS2C)
        {
            if (contents.Length < 8)
            {
                if (isS2C)
                {
                    throw new PacketException("Incomplete packet recieved!");
                }
                else
                {
                    throw new PacketException("Incomplete packet recieved!");
                }
                throw new PacketException($"Incomplete packet recieved from {(isS2C ? "server" : "client")}!");
            }
            int packetid = BitConverter.ToInt32(contents, 0);
            int size = BitConverter.ToInt32(contents, 4);
            byte[] bytes = new byte[contents.Length - 8];
            Buffer.BlockCopy(contents, 8, bytes, 0, contents.Length - 8);
            return GetPacket(packetid, bytes, isS2C);
        }
        public static Packet GetPacket(int packetid, byte[] packetbytes, bool isS2C)
        {
            Packet packet = null;
            try
            {
                Func<byte[], Packet> func = PacketTypesFinder.GetConstructor(packetid, isS2C);
                if(func == null)
                {
                    throw new PacketException($"Invalid {(isS2C ? "S2C" : "C2S")} packet id: {packetid.ToString()} does not match any {(isS2C ? "S2C" : "C2S")} ids!");
                }
                packet = func(packetbytes);
            }
            catch
            {

            }
            if (packet == null)
            {
                throw new PacketException("Constructor failure somehow idk");
            }
            return packet;
        }
        public static byte[] EncodePacket(Packet packet)
        {
            byte[] packettype = BitConverter.GetBytes(packet.GetID());
            byte[] packetbytes = packet.Bytify();
            byte[] packetsize = BitConverter.GetBytes(packetbytes.Length);
            //First comes type, then size, then the packet
            byte[] final = new byte[8 + packetbytes.Length];
            Buffer.BlockCopy(packettype, 0, final, 0, packettype.Length);
            Buffer.BlockCopy(packetsize, 0, final, packettype.Length, packetsize.Length);
            Buffer.BlockCopy(packetbytes, 0, final, packettype.Length + packetsize.Length, packetbytes.Length);
            return final;
        }
        public abstract void Execute(ClientConnection sender);
        public int GetID()
        {
            return GetType().GetCustomAttribute<APacket>().PacketID;
        }
        public void Dispose()
        {
        }
    }
}
