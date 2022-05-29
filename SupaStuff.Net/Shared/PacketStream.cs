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
        public List<Packet.Packet> packetsToWrite = new List<Packet.Packet>(1024);
        public List<Packet.Packet> packetsToHandle = new List<Packet.Packet>(1024);
        public Task currentTask = null;
        public bool isRunning = true;
        public bool sendingPacket = false;
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
        public int packetID = -1;
        public int packetSize = -1;
        public byte[] packetBody = null;
        public int packetBodyIndex = 0;
        #endregion
        /// <summary>
        /// Called when an error occurs
        /// </summary>
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
        /// <summary>
        /// Tries to recieve a packet, if it can't recieve the whole thing saves what it did get to variables to be continued later
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
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
                    int headerReqLength = 12 - packetBodyIndex;
                    int headerAmountRead = stream.Read(packetHeader, packetBodyIndex, headerReqLength);
                    if (headerAmountRead == headerReqLength)
                    {
                        packetID = BitConverter.ToInt32(packetHeader, 0);
                        packetSize = BitConverter.ToInt32(packetHeader, 4);
                        packetHeaderComplete = true;
                    }
                    else
                    {
                        return false;
                    }
                }
                int reqLength = packetSize - packetBodyIndex + 12;
                int amountRead = stream.Read(packetBody, packetBodyIndex - 12, reqLength);
                if (reqLength == amountRead)
                {
                    packet = FinishRecievePacket();
                    if (OnRecievePacket != null)
                    {
                        bool stop = RecievePacketEvent(packet);
                        if (!stop) HandleIncomingPacket(packet);
                    }
                    else
                    {
                        HandleIncomingPacket(packet);
                    }
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
        /// <summary>
        /// Called when a packet is recieved, to execute the packet's code and whatnot
        /// </summary>
        /// <param name="packet"></param>
        public void HandleIncomingPacket(Packet.Packet packet)
        {
        }
        /// <summary>
        /// Finish recieving a packet
        /// </summary>
        /// <returns></returns>
        public Packet.Packet FinishRecievePacket()
        {
            Packet.Packet packet = Packet.Packet.GetPacket(packetID, packetBody, !isServer);
            PacketCleanup();
            return packet;

        }
        /// <summary>
        /// Cleans up variables after a packet is recieved
        /// </summary>
        public void PacketCleanup()
        {
            packetID = -1;
            packetBody = null;
            packetBodyIndex = -1;
            packetHeader = new byte[8];
            packetSize = -1;
            packetHeaderComplete = false;

        }
        /// <summary>
        /// Whether or not a packet is ready to be recieved
        /// </summary>
        /// <returns></returns>
        public bool PacketAvailable()
        {
            return stream.DataAvailable;
        }
        /// <summary>
        /// The main constructor
        /// </summary>
        /// <param name="stream">The stream to send and recieve from</param>
        /// <param name="isServer">Whether or not it is a server, used for packet decoding</param>
        /// <param name="onError">The function to be called on errors</param>
        public PacketStream(NetworkStream stream, bool isServer, Func<bool> onError)
        {
            this.stream = stream;
            this.isServer = isServer;
            customOnError = onError;
        }
        /// <summary>
        /// Begin asynchronous sending of packet queue
        /// </summary>
        public void StartSendPacket()
        {
            sendingPacket = true;
            byte[] bytes = Packet.Packet.EncodePacket(packetsToWrite[0]);
            stream.BeginWrite(bytes,0,bytes.Length,new AsyncCallback(EndSendPacket),null);
            packetsToWrite.RemoveAt(0);
        }

        /// <summary>
        ///  Finish sending queue of packets, or keep going, up to you.
        /// </summary>
        /// <param name="ar"></param>
        public void EndSendPacket(IAsyncResult ar)
        {
            stream.EndWrite(ar);
            if (packetsToWrite.Count > 0)
            {
                StartSendPacket();
            }
            else
            {
                sendingPacket = false;
            }
        }

        /// <summary>
        /// Takes the packet queue, iterates through them, removes them if stale, otherwise processes them
        /// </summary>
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
            if(!sendingPacket && packetsToWrite.Count > 0)
            {
                StartSendPacket();
            }
        }
        /// <summary>
        /// Called to ease up the Garbage collection by disposing manually
        /// </summary>
        public void Dispose()
        {

        }
        /// <summary>
        /// Add the packet to the queue, to be sent when its ready
        /// </summary>
        /// <param name="packet"></param>
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
        /// <summary>
        /// Delegate function for when you recieve a packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public delegate bool _OnRecievePacket(Packet.Packet packet);
        /// <summary>
        /// Called when a packet is recieved
        /// </summary>
        public event _OnRecievePacket OnRecievePacket;
        public bool RecievePacketEvent(Packet.Packet packet)
        {
            _OnRecievePacket[] methods = (_OnRecievePacket[])OnRecievePacket.GetInvocationList();
            bool shouldReturn = false;
            foreach(_OnRecievePacket method in methods)
            {
                if(method.Invoke(packet))
                {
                    shouldReturn = true;
                }
            }
            return shouldReturn;
        }
    }
}