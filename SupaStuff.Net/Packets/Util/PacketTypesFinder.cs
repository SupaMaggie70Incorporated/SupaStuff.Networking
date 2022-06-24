using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using SupaStuff.Core.Util;
namespace SupaStuff.Net.Packets.Util
{
    internal static class PacketTypesFinder
    {
        public static Dictionary<int, Type> c2stypes;
        public static Dictionary<int, Type> s2ctypes;
        public static Dictionary<int, Func<byte[], Packet>> c2sConstructors;
        public static Dictionary<int, Func<byte[],Packet>> s2cConstructors;

        public static Dictionary<int, PacketTypeInfo> c2sTypes;
        public static Dictionary<int, PacketTypeInfo> s2cTypes;
        /// <summary>
        /// Gets the classes with the APacket attribute to add to a list, for easier encoding and decoding
        /// </summary>
        public static void GetTypes()
        {
            c2stypes = new Dictionary<int, Type>();
            s2ctypes = new Dictionary<int, Type>();
            c2sConstructors = new Dictionary<int, Func<byte[], Packet>>();
            s2cConstructors = new Dictionary<int, Func<byte[], Packet>>();
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
                        if (s2ctypes.ContainsKey(property.PacketID))
                        {
                            throw new PacketException($"Multiple S2C packets with the same id found! Fix this problem please!\nPackets sharing id:\n    {s2ctypes[property.PacketID].FullName}\n    {type.FullName}");
                        }
                        s2ctypes.Add(property.PacketID, type);
                        ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(byte[]) });
                        Func<byte[], Packet> func = (byte[] bytes) => constructorInfo.Invoke(new object[] { bytes }) as Packet;
                        s2cConstructors.Add(property.PacketID, func);
                        PacketTypeInfo info = new PacketTypeInfo();
                        info.type = type;
                        info.constructor = func;
                        s2cTypes.Add(property.PacketID, info);
                    }
                    else
                    {
                        if (c2stypes.ContainsKey(property.PacketID))
                        {
                            throw new PacketException($"Multiple C2S packets with the same id found! Fix this problem please!\nPackets sharing id:\n    {s2ctypes[property.PacketID].FullName}\n    {type.FullName}");
                        }
                        c2stypes.Add(property.PacketID, type);
                        ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(byte[]) });
                        Func<byte[], Packet> func = (byte[] bytes) => constructorInfo.Invoke(new object[] { bytes }) as Packet;
                        Func<int, bool> lenFunc = GetLengthFunc(type);
                        c2sConstructors.Add(property.PacketID, func);
                        PacketTypeInfo info = new PacketTypeInfo();
                        info.type = type;
                        info.constructor = func;
                        c2sTypes.Add(property.PacketID, info);
                    }
                }
            }
        }
        private static readonly Type[] argTypes = new Type[] { typeof(byte) };
        public static Func<int,bool> GetLengthFunc(Type type)
        {
            MethodInfo method = type.GetMethod("",argTypes);
        }
        [Obsolete]
        public static Type GetS2CPacket(int id)
        {
            return s2ctypes[id];
        }
        [Obsolete]
        public static Type GetC2SPacket(int id)
        {
            return c2stypes[id];
        }
        [Obsolete]
        public static Type GetPacket(int id, bool isS2C)
        {
            if (isS2C) return GetS2CPacket(id);
            else return GetC2SPacket(id);
        }
        public static Func<byte[],Packet> GetConstructor(int id,bool isS2C)
        {
            if (isS2C) return s2cConstructors[id];
            else return c2sConstructors[id];
        }
    }
}
