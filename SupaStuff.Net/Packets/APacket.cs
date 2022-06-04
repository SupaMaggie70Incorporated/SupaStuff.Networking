using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace SupaStuff.Net.Packets
{
    /// <summary>
    /// The attribute applied to packets to be found in PacketTypesFinder
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class APacket : Attribute
    {
        /// <summary>
        /// The id of the packet for encoding and decoding.
        /// </summary>
        public int PacketID;
        /// <summary>
        /// Is it client to server or server to client?
        /// </summary>
        public bool isS2C;
        /// <summary>
        /// The attribute applied to packets to be found in PacketTypesFinder
        /// </summary>
        /// <param name="PacketID">The packet id for encoding/decoding</param>
        /// <param name="isS2C">Is it client to server or server to client?</param>
        public APacket(int PacketID, bool isS2C)
        {
            this.PacketID = PacketID;
            this.isS2C = isS2C;

        }
    }
}
