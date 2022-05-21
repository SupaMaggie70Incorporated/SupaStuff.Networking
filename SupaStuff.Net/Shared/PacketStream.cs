using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupaStuff.Core;
using SupaStuff.Core.Util;
using System.Net;
using System.Net.Sockets;
using SupaStuff.Net.Packet;
namespace SupaStuff.Net.Shared
{
    public class PacketStream : IDisposable
    {
        public NetworkStream stream;
        public bool isServer;
        public Func<bool> customOnError;
        public List<Packet.Packet> packetsToWrite = new List<Packet.Packet>(16);
        public Task currentTask = null;
        public bool isRunning = true;
        public Packet.Packet currentPacketToSend;
        #region Packet buffer
        /*
        public bool packetHeaderComplete;
        public byte[] packetHeader;
        public int packetHeaderIndex;
        public int packetSize;
        public int packetID;
        public byte[] packetBody;
        public int packetBodyIndex;
        */

        public bool packetHeaderComplete = false;
        public byte[] packetHeader = new byte[12];
        public int packetHeaderIndex = 0;
        public int packetID = -1;
        public int packetSize = -1;
        public byte[] packetBody = null;
        public int packetBodyIndex = 0;
        #endregion
        public void onError()
        {
            isRunning = customOnError();
            if (!isRunning)
            {
                if (currentTask != null)
                {
                }
            }
        }
        [Obsolete]
        public bool _TryGetPacket(out Packet.Packet packet)
        {
            if (!stream.DataAvailable || !stream.CanRead)
            {
                packet = null;
                return false;
            }
            try
            {
                byte[] buffer = new byte[8];
                stream.Read(buffer, 0, buffer.Length);
                int packetid = BitConverter.ToInt32(buffer, 0);
                int size = BitConverter.ToInt32(buffer, 4);
                if (size > 1024)
                    buffer = new byte[size];
                int recievesize = stream.Read(buffer, 0, size);
                byte[] packetbytes;
                if (recievesize != size)
                {
                    onError();
                    packet = null;
                    return false;
                }
                else
                {
                    packetbytes = buffer;
                    packet = Packet.Packet.GetPacket(packetid, packetbytes, !isServer);
                    return true;
                }
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
            Squirrelgame.MainLogger.log(e.StackTrace);
#endif
                onError();
                packet = null;
                return false;
            }
        }
        public bool TryGetPacket(out Packet.Packet packet)
        {
            packet = null;
            if (!stream.DataAvailable || !stream.CanRead)
            {
                return false;
            }
            try
            {
                if (!packetHeaderComplete)
                {
                    int headerReqLength = packetSize - packetBodyIndex - 1;
                    int headerAmountRead = stream.Read(packetBody, packetBodyIndex, headerReqLength);
                    if (headerAmountRead == headerReqLength)
                    {
                        packetID = BitConverter.ToInt32(packetHeader, 0);
                        packetSize = BitConverter.ToInt32(packetHeader, 4);
                    }
                    else
                    {
                        packetHeaderIndex += headerAmountRead;
                        return false;
                    }
                }
                int reqLength = packetHeader.Length - packetHeaderIndex - 1;
                int amountRead = stream.Read(packetHeader, packetHeaderIndex, reqLength);
                if (reqLength == amountRead)
                {
                    packet = FinishRecievePacket();
                    bool stop = OnRecievePacket(packet);
                    if (!stop) HandleIncomingPacket(packet);
                    return true;
                }
                else
                {
                    packetBodyIndex += amountRead;
                    return false;
                }
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
            Squirrelgame.MainLogger.error(e.StackTrace);
#endif
                PacketCleanup();
                onError();
                return false;
            }
        }
        public void HandleIncomingPacket(Packet.Packet packet)
        {
        }
        public Packet.Packet FinishRecievePacket()
        {
            Packet.Packet packet = Packet.Packet.GetPacket(packetID, packetBody, !isServer);
            PacketCleanup();
            return packet;

        }
        public void PacketCleanup()
        {
            packetID = -1;
            packetBody = null;
            packetBodyIndex = -1;
            packetHeader = new byte[8];
            packetHeaderIndex = 0;
            packetSize = -1;
            packetHeaderComplete = false;

        }
        public bool PacketAvailable()
        {
            return stream.DataAvailable;
        }
        public PacketStream(NetworkStream stream, bool isServer, Func<bool> onError)
        {
            this.stream = stream;
            this.isServer = isServer;
            customOnError = onError;
        }
        private async Task SendPacketTask()
        {
            while (isRunning)
            {
                while (packetsToWrite.Count > 0 && stream.CanWrite)
                {
                    Packet.Packet packet = packetsToWrite[0];
                    byte[] bytes = Packet.Packet.EncodePacket(packet);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    packetsToWrite.RemoveAt(1);
                }
            }
            currentTask = null;
        }
        public void StartSendLoop()
        {
            if (currentTask != null)
            {
                return;
            }
            currentTask = SendPacketTask();
            currentTask.Start();
        }
        public void Update()
        {
            long time = DateTime.UtcNow.Ticks;
            for (int i = 0; i < packetsToWrite.Count; i++)
            {
                Packet.Packet packet = packetsToWrite[i];
                long ticks = packet.startTime.Ticks;
                if (time - ticks > packet.getMaxTime())
                {
                    i--;
                    packetsToWrite.RemoveAt(i);
                }
            }
        }
        public void Dispose()
        {

        }
        public void SendPacket(Packet.Packet packet)
        {
            if (packetsToWrite.Count + 1 == packetsToWrite.Capacity)
            {
                Main.NetLogger.error("Nonfatal error: too many packets queued. Connection was too slow.");
                onError();
                Dispose();
            }
            packet.startTime = DateTime.UtcNow;
            packetsToWrite.Add(packet);
        }
        public delegate bool _OnRecievePacket(Packet.Packet packet);
        public event _OnRecievePacket OnRecievePacket;
    }
}