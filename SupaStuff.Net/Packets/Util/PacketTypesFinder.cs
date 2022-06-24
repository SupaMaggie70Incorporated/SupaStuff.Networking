using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using SupaStuff.Core.Util;
namespace SupaStuff.Net.Packets.Util
{
    internal static class PacketTypesFinder
    {

        public static Dictionary<int, PacketTypeInfo> c2sTypes;
        public static Dictionary<int, PacketTypeInfo> s2cTypes;
        /// <summary>
        /// Gets the classes with the APacket attribute to add to a list, for easier encoding and decoding
        /// </summary>
        public static void GetTypes()
        {
            c2sTypes = new Dictionary<int, PacketTypeInfo>();
            s2cTypes = new Dictionary<int, PacketTypeInfo>();
            
            Type[] types =  TypeFinder.ReGetTypes();
            foreach (Type type in types)
            {
                APacket property = type.GetCustomAttribute<APacket>();
                if (property != null)
                {
                    if (property.isS2C)
                    {
                        if (s2cTypes.ContainsKey(property.PacketID))
                        {
                            throw new PacketException($"Multiple S2C packets with the same id found! Fix this problem please!\nPackets sharing id:\n    {s2cTypes[property.PacketID].type.FullName}\n    {type.FullName}");
                        }
                        ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(byte[]) });
                        Func<byte[], Packet> func = (byte[] bytes) => constructorInfo.Invoke(new object[] { bytes }) as Packet;
                        PacketTypeInfo info = new PacketTypeInfo();
                        info.type = type;
                        info.constructor = func;
                        info.isRightLength = GetLengthFunc(type);
                        s2cTypes.Add(property.PacketID, info);
                    }
                    else
                    {
                        if (c2sTypes.ContainsKey(property.PacketID))
                        {
                            throw new PacketException($"Multiple C2S packets with the same id found! Fix this problem please!\nPackets sharing id:\n    {s2cTypes[property.PacketID].type.FullName}\n    {type.FullName}");
                        }
                        ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(byte[]) });
                        Func<byte[], Packet> func = (byte[] bytes) => constructorInfo.Invoke(new object[] { bytes }) as Packet;
                        Func<int, bool> lenFunc = GetLengthFunc(type);
                        PacketTypeInfo info = new PacketTypeInfo();
                        info.type = type;
                        info.constructor = func;
                        info.isRightLength = GetLengthFunc(type);
                        c2sTypes.Add(property.PacketID, info);
                    }
                }
            }
        }
        private static readonly Type[] argTypes = new Type[] { typeof(byte) };
        private static readonly BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        public static Func<int,bool> GetLengthFunc(Type type)
        {
            MethodInfo method = type.GetMethod("IsAllowedSize",flags,null,argTypes,null);
            if(method == null)
            {
                return Packet.IsAllowedSize;
            }
            return (int i) => (bool)(method.Invoke(null, new object[] { i }) as bool?);
        }
        [Obsolete]
        public static Type GetS2CPacket(int id)
        {
            return s2cTypes[id].type;
        }
        [Obsolete]
        public static Type GetC2SPacket(int id)
        {
            return c2sTypes[id].type;
        }
        [Obsolete]
        public static Type GetPacket(int id, bool isS2C)
        {
            if (isS2C) return GetS2CPacket(id);
            else return GetC2SPacket(id);
        }
        public static Func<byte[],Packet> GetConstructor(int id,bool isS2C)
        {
            if (isS2C) return s2cTypes[id].constructor;
            else return c2sTypes[id].constructor;
        }
    }
}
