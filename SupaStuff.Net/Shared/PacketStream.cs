﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SupaStuff.Core;
using SupaStuff.Core.Util;
using System.Net;
using System.Net.Sockets;
using SupaStuff.Net.Packets;
namespace SupaStuff.Net.Shared
{
    public class PacketStream : IDisposable
    {
        public NetworkStream stream;
        public bool isServer;
        public Func<bool> customOnError;
        public List<Packet> packetsToWrite = new List<Packet>(1024);
        public List<Packet> packetsToHandle = new List<Packet>(1024);
        public Task currentTask = null;
        public bool isRunning = true;
        public bool sendingPacket = false;
        public Packet currentPacketToSend;
        public Server.ClientConnection clientConnection = null;
        #region Packet buffer

        public bool packetHeaderComplete = false;
        public byte[] packetHeader = new byte[8];
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
        public bool TryGetPacket(out Packet packet)
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
                Dispose();
                return false;
            }
        }
        /// <summary>
        /// Check to see if any packets can be fully read
        /// </summary>
        public void CheckForPackets()
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
        /// <summary>
        /// Called when a packet is recieved, to execute the packet's code and whatnot
        /// </summary>
        /// <param name="packet"></param>
        public void HandleIncomingPacket(Packet packet)
        {
            packetsToHandle.Remove(packet);
            try
            {
                if(!RecievePacketEvent(packet))
                {
                    packet.Execute(clientConnection);
                }
            }
            catch
            {
                if (this != null) Dispose();
            }
        }
        /// <summary>
        /// Finish recieving a packet
        /// </summary>
        /// <returns></returns>
        public Packet FinishRecievePacket()
        {
            Packet packet = Packet.GetPacket(packetID, packetBody, !isServer);
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
            packetBodyIndex = 0;
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
            try
            {
                lock (packetsToWrite)
                {
                    sendingPacket = true;
                    Packet packet = packetsToWrite[0];
                    packetsToWrite.RemoveAt(0);
                    byte[] bytes = Packet.EncodePacket(packet);
                    stream.BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(EndSendPacket), null);
                }
            }catch
            {
                Console.WriteLine("Error :(");
                onError();
                Dispose();
            }
        }
        /// <summary>
        ///  Finish sending queue of packets, or keep going, up to you.
        /// </summary>
        /// <param name="ar"></param>
        public void EndSendPacket(IAsyncResult ar)
        {
            try
            {
                lock (packetsToWrite)
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
            }catch
            {
                Console.WriteLine("Error :(");
                onError();
                Dispose();
            }

        }

        /// <summary>
        /// Takes the packet queue, iterates through them, removes them if stale, otherwise processes them
        /// </summary>
        public void Update()
        {
            long time = DateTime.UtcNow.Ticks;
            //Remove old packets
            for (int i = 0; i < packetsToWrite.Count; i++)
            {
                Packet packet = packetsToWrite[i];
                long ticks = packet.startTime.Ticks;
                if (time - ticks > packet.getMaxTime())
                {
                    packetsToWrite.RemoveAt(i);
                    i--;
                }
            }
            
            //Check for new packets to recieve
            CheckForPackets();

            //Start sending a packet if you can
            if(!sendingPacket && packetsToWrite.Count > 0)
            {
                StartSendPacket();
            }

            //Handle incoming packets
            Packet[] packets = packetsToHandle.ToArray();
            foreach(Packet packet in packets)
            {
                HandleIncomingPacket(packet);
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
        public void SendPacket(Packet packet)
        {
            if (packetsToWrite.Count + 1 == packetsToWrite.Capacity)
            {
                //To many packets in queue!
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
                packet.startTime = DateTime.UtcNow;
                packetsToWrite.Add(packet);
            }catch
            {

            }
        }
        /// <summary>
        /// Delegate function for when you recieve a packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public delegate bool _OnRecievePacket(Packet packet);
        /// <summary>
        /// Called when a packet is recieved
        /// </summary>
        public event _OnRecievePacket OnRecievePacket;
        public bool RecievePacketEvent(Packet packet)
        {
            if (OnRecievePacket == null) return false;
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