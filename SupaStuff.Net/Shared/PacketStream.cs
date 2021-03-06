using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SupaStuff.Core;
using SupaStuff.Core.Util;
using System.Net;

using System.Reflection;

using System.Net.Sockets;
using SupaStuff.Net.Packets;
using SupaStuff.Net.Packets.BuiltIn;

using SupaStuff.Net.Packets.Util;
namespace SupaStuff.Net.Shared
{
    public class PacketStream : IDisposable
    {
        internal bool isServer;
        internal bool isRunning = true;
        internal ServerSide.ClientConnection clientConnection = null;
        private NetworkStream stream;
        private Func<bool> customOnError;
        internal List<Packet> packetsToWrite = new List<Packet>(1024);
        private List<Packet> packetsToHandle = new List<Packet>(1024);
        private bool sendingPacket = false;
        public Packet currentSentPacket;
        public Logger logger = Main.NetLogger;
        //Server only
        internal DateTime lastCheckedIn = DateTime.UtcNow;
        public static readonly int MaxUncheckedTime = 10;

        #region Packet buffer

        bool packetHeaderComplete = false;
        byte[] packetHeader = new byte[8];
        int packetID = -1;
        int packetSize = -1;
        byte[] packetBody = null;
        int packetBodyIndex = 0;
        #endregion
        /// <summary>
        /// Called when an error occurs
        /// </summary>
        private void onError()
        {
            try
            {
                customOnError();
            }
            catch
            {

            }
        }
        /// <summary>
        /// Tries to recieve a packet, if it can't recieve the whole thing saves what it did get to variables to be continued later
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private bool TryGetPacket(out Packet packet)
        {
            packet = null;
            try
            {
                if (!stream.DataAvailable || !stream.CanRead)
                {
                    return false;
                }
                if (!packetHeaderComplete)
                {
                    int headerReqLength = 8 - packetBodyIndex;
                    int headerAmountRead = stream.Read(packetHeader, packetBodyIndex, headerReqLength);
                    if (headerAmountRead == headerReqLength)
                    {
                        packetID = BitConverter.ToInt32(packetHeader, 0);
                        packetSize = BitConverter.ToInt32(packetHeader, 4);
                        packetHeaderComplete = true;
                        packetBody = new byte[packetSize];
                        packetBodyIndex = 0;


                        if(isServer)
                        {
                            if (!PacketTypesFinder.c2sTypes[packetID].isRightLength(packetSize))
                            {
                                Dispose();
                                return false;
                            }
                        }
                        else
                        {
                            if (!PacketTypesFinder.s2cTypes[packetID].isRightLength(packetSize))
                            {
                                Dispose();
                                return false;
                            }
                        }
                    }
                    else
                    {
                        packetBodyIndex += headerAmountRead;
                        return false;
                    }
                }
                if (packetSize == 0)
                {
                    packet = FinishRecievePacket();
                    return true;
                }
                int reqLength = packetSize - packetBodyIndex;
                int amountRead = stream.Read(packetBody, packetBodyIndex, reqLength);
                if (reqLength == amountRead)
                {
                    packet = FinishRecievePacket();
                    return true;
                }
                else
                {
                    packetBodyIndex += amountRead;
                    return false;
                }
            }catch
            {
                logger.log("Error recieving packet, disconnecting");
                onError();
                Dispose();
                return false;
            }
        }
        /// <summary>
        /// Check to see if any packets can be fully read
        /// </summary>
        private void CheckForPackets()
        {
            while (true)
            {
                Packet packet = null;
                if (TryGetPacket(out packet))
                {
                    packetsToHandle.Add(packet);
                }
                else break;
            }
        }
        private static readonly Type[] builtinPackets = new Type[]
        {
            typeof(C2SDisconnectPacket),
            typeof(S2CKickPacket),
            typeof(YesImHerePacket),
            typeof(C2SWelcomePacket)
        };
        /// <summary>
        /// Called when a packet is recieved, to execute the packet's code and whatnot
        /// </summary>
        /// <param name="packet"></param>
        public void HandleIncomingPacket(Packet packet)
        {
            try 
            { 
                Type type = packet.GetType();
                if(isServer && !clientConnection.finishAuth && type != typeof(C2SWelcomePacket))
                {
                    logger.log("We recieved a packet other than the C2SWelcomePacket as our first packet, so fuck off hacker");
                    onError();
                    Dispose();
                    return;
                }
                packetsToHandle.Remove(packet);
                if (!builtinPackets.Contains(type))
                {
                    RecievePacketEvent(packet);
                }
                packet.Execute(clientConnection);
            }
            catch
            {
                if (this != null)
                {
                    logger.log("We had issues handling a packet, so we're gonna commit die");
                    onError();
                    Dispose();
                }
            }
        }
        /// <summary>
        /// Finish recieving a packet
        /// </summary>
        /// <returns></returns>
        private Packet FinishRecievePacket()
        {
            try
            {
                Packet packet = Packet.GetPacket(packetID, packetBody, !isServer);
                PacketCleanup();
                if(isServer) lastCheckedIn = DateTime.UtcNow;
                return packet;
            }
            catch
            {
                logger.log("Failed to complete the packet");
                onError();
                Dispose();
                return null;
            }

        }
        /// <summary>
        /// Cleans up variables after a packet is recieved
        /// </summary>
        private void PacketCleanup()
        {
            packetID = -1;
            packetBody = null;
            packetBodyIndex = 0;
            packetHeader = new byte[8];
            packetSize = -1;
            packetHeaderComplete = false;

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
        private void BeginSendPacket()
        {
            try
            {
                sendingPacket = true;
                Packet packet = packetsToWrite[0];
                packetsToWrite.RemoveAt(0);
                currentSentPacket = packet;
                byte[] bytes;
                try
                {
                    bytes = Packet.EncodePacket(packet);
                }
                catch (Exception ex)
                {
                    logger.log("Error encoding packet of type " + packet.GetType() + " : undisclosed error");
                    onError();
                    Dispose();
                    return;
                }
                stream.BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(EndSendPacket), null);
            }catch
            {
                logger.log("We had an error sending a packet(BeginSendPacket), potentially due to unsafe threading, if you encounter this and it does not happen consistently tell SupaMaggie70 because its kinda weird");
                onError();
                Dispose();
                return;
            }
        }
        /// <summary>
        ///  Finish sending queue of packets, or keep going, up to you.
        /// </summary>
        /// <param name="ar"></param>
        private void EndSendPacket(IAsyncResult ar)
        {
            lastCheckedIn = DateTime.UtcNow;
            try
            {
                stream.EndWrite(ar);
                if(currentSentPacket.GetType() == typeof(S2CKickPacket))
                {
                    logger.log("We disconnected because we sent a kick packet and theyre no longer welcome");
                    onError();
                    Dispose();
                    return;
                }
                else if(currentSentPacket.GetType() == typeof(C2SDisconnectPacket))
                {
                    logger.log("We are disconneting and just finished sending the packet so byeeeee");
                    onError();
                    Dispose();
                }
                if (packetsToWrite.Count > 0)
                {
                    BeginSendPacket();
                }
                else
                {
                    sendingPacket = false;
                    currentSentPacket = null;
                }
                
                if (!isServer)
                {
                    lastCheckedIn = DateTime.UtcNow;
                }
            }catch
            {
                logger.log("We had an error sending a packet(EndSendPacket), potentially due to unsafe threading, if you encounter this and it does not happen consistently tell SupaMaggie70 because its kinda weird");
                onError();
                Dispose();
            }

        }

        /// <summary>
        /// Takes the packet queue, iterates through them, removes them if stale, otherwise processes them
        /// </summary>
        public void Update()
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                //Remove old packets

                for (int i = 0; i < packetsToWrite.Count; i++)
                {
                    Packet packet = packetsToWrite[i];
                    DateTime startTime = packet.startTime;
                    if (Math.TimeBetween(packet.startTime,now) > 1)
                    {
                        packetsToWrite.RemoveAt(i);
                        i--;
                    }
                }



                //Check for new packets to recieve
                CheckForPackets();

                //Start sending a packet if you can
                if (!sendingPacket && packetsToWrite.Count > 0)
                {
                    BeginSendPacket();
                }

                //Handle incoming packets
                Packet[] packets = packetsToHandle.ToArray();
                foreach (Packet packet in packets)
                {
                    HandleIncomingPacket(packet);
                }

                if (!isServer && Math.TimeBetween(lastCheckedIn,now) > 5)
                {
                    packetsToWrite.Insert(0, new YesImHerePacket());
                    lastCheckedIn = DateTime.UtcNow;
                }
                if (isServer && Math.TimeBetween(lastCheckedIn,now) > MaxUncheckedTime)
                {
                    logger.log("We kicked a client because they waited too long to check in");
                    onError();
                    Dispose();
                }
            }
            catch
            {
                logger.log("We had an undisclosed error updating, so now we're gonna leave");
                Dispose();
            }
        }
        /// <summary>
        /// Called to ease up the Garbage collection by disposing manually
        /// </summary>
        public void Dispose()
        {
            DisconnectedEvent();
        }
        /// <summary>
        /// Add the packet to the queue, to be sent when its ready
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet)
        {
            Type packetType = packet.GetType();
            APacket attribute = packetType.GetCustomAttribute<APacket>();
            if(!attribute.allowDuplicates)
            {
                foreach(Packet _packet in packetsToWrite)
                {
                    if(_packet.GetType() == packetType)
                    {
                        _packet.startTime = DateTime.UtcNow;
                        return;
                    }
                }
            }
            if (packetsToWrite.Count + 1 == packetsToWrite.Capacity)
            {
                //Too many packets in queue!
                logger.log("Too many packets in queue, we're out!");
                onError();
                try
                {
                    Dispose();
                } catch
                {

                }
            }
            try
            {
                if (builtinPackets.Contains(packetType))
                {
                    packet.startTime = DateTime.UtcNow;
                    packetsToWrite.Insert(0, packet);
                    return;
                }
                else
                {
                    packet.startTime = DateTime.UtcNow;
                    packetsToWrite.Add(packet);
                }
            }catch
            {

            }
        }
        /// <summary>
        /// Delegate function for when you recieve a packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        /// <summary>
        /// Called when a packet is recieved
        /// </summary>
        public event Action<Packet> OnRecievePacket;
        private void RecievePacketEvent(Packet packet)
        {
            if (OnRecievePacket == null) return;
            OnRecievePacket.Invoke(packet);
        }
        public event Action OnDisconnected;

        private void DisconnectedEvent()
        {
            if (!isRunning) return;
            isRunning = false;
            if (OnDisconnected == null) return;
            OnDisconnected.Invoke();
            if(isServer)
            {
                Main.ServerLogger.log("Client decided to disconnect!");
            }
            else
            {
                Main.ClientLogger.log("Server aborted connection!");
            }
        }

    }
}