using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;
namespace SupaStuff.Net.Packets.Util
{

    internal static class Bytifier
    {
        private static Dictionary<Type, Func<object, byte[]>> bytifyFunctions = new Dictionary<Type, Func<object, byte[]>>();
        private static Dictionary<Type, Func<byte[], object>> debytifyFunctions = new Dictionary<Type, Func<byte[], object>>();
        /// <summary>
        /// Finds the packets out there, collects them and their functions, and adds them to lists
        /// </summary>
        public static void Init()
        {
            MethodInfo[] methods = TypeFinder.GetMethods();
            int numO2B = 0;
            int numB2O = 0;
            foreach (MethodInfo method in methods)
            {
                if (method == null) continue;
                AConvert convert = method.GetCustomAttribute<AConvert>();
                if (convert == null) continue;
                if (method.GetParameters().Length != 1)
                {
                    continue;
                }
                if (convert.b2o)
                {
                    numB2O++;
                    debytifyFunctions[method.ReturnType] = (byte[] bytes) => method.Invoke(null, new object[] { bytes });
                }
                else
                {
                    numO2B++;
                    ParameterInfo parameter = method.GetParameters()[0];
                    bytifyFunctions[parameter.ParameterType] = (object instance) => method.Invoke(instance, new object[] { }) as byte[];
                }
            }
            Type[] types = TypeFinder.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsAssignableFrom(typeof(IBytifiable)))
                {
                    continue;
                }
                if (!bytifyFunctions.ContainsKey(type))
                {
                    bytifyFunctions[type] = null;
                }
            }
        }
        /*
        private static Func<object,byte[]> GetBytifyFunction(Type t)
        {
            Func<object, byte[]> func = (object obj) => { };
            foreach(FieldInfo field in t.GetRuntimeFields())
            {

            }
        }
        private static Func<byte[],object> MakeDebytifyFunction(Type t)
        {

        }
        */
        /// <summary>
        /// Turns an object into bytes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Bytify<T>(T obj)
        {
            IBytifiable bytifiable = obj as IBytifiable;
            if (obj != null)
            {
                return bytifiable.Bytify();
            }
            else
            {
                return bytifyFunctions[typeof(T)](obj) as byte[];
            }
        }
        #region Object to Bytes Converters
        [AConvert(false)]
        public static byte[] stringConvert(string obj)
        {
            return Encoding.UTF8.GetBytes(obj);
        }
        [AConvert(false)]
        public static byte[] byteArrayConvert(byte[] obj)
        {
            return obj;
        }
        [AConvert(false)]
        public static byte[] intConvert(int obj)
        {
            return BitConverter.GetBytes(obj);
        }
        [AConvert(false)]
        public static byte[] shortConvert(short obj)
        {
            return BitConverter.GetBytes(obj);
        }
        [AConvert(false)]
        public static byte[] longConvert(long obj)
        {
            return BitConverter.GetBytes(obj);
        }
        [AConvert(false)]
        public static byte[] byteConvert(byte obj)
        {
            return new byte[]
            {
                    obj
            };
        }
        [AConvert(false)]
        public static byte[] uintConvert(uint obj)
        {
            return BitConverter.GetBytes(obj);
        }
        [AConvert(false)]
        public static byte[] ushortConvert(ushort obj)
        {
            return BitConverter.GetBytes(obj);
        }
        [AConvert(false)]
        public static byte[] ulongConvert(ulong obj)
        {
            return BitConverter.GetBytes(obj);
        }
        [AConvert(false)]
        public static byte[] sbyteConvert(sbyte obj)
        {
            return BitConverter.GetBytes(obj);
        }
        #endregion
        #region Bytes to Object Converters
        [AConvert(true)]
        public static string stringFrom(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        [AConvert(true)]
        public static byte[] byteArrayFrom(byte[] obj)
        {
            return obj;
        }
        [AConvert(true)]
        public static int intFrom(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }
        [AConvert(true)]
        public static short shortFrom(byte[] bytes)
        {
            return BitConverter.ToInt16(bytes, 0);
        }
        [AConvert(true)]
        public static long longConvert(byte[] bytes)
        {
            return BitConverter.ToInt64(bytes, 0);
        }
        [AConvert(true)]
        public static byte byteConvert(byte[] bytes)
        {
            return bytes[0];
        }
        [AConvert(true)]
        public static uint uintConvert(byte[] bytes)
        {
            return BitConverter.ToUInt32(bytes, 0);
        }
        [AConvert(true)]
        public static ushort ushortConvert(byte[] bytes)
        {
            return BitConverter.ToUInt16(bytes, 0);
        }
        [AConvert(true)]
        public static ulong ulongConvert(byte[] bytes)
        {
            return BitConverter.ToUInt64(bytes, 0);
        }
        [AConvert(true)]
        public static sbyte sbyteConvert(byte[] bytes)
        {
            return (sbyte)bytes[0];
        }
        #endregion

        /// <summary>
        /// This signifies that the function will convert a type to bytes, or the other way around, without requiring the function to be in the class. For example for strings
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
        public class AConvert : Attribute
        {
            public bool b2o;
            /// <summary>
            /// B2O means "bytes to object". Giving it the value true signifies it is converting the bytes given to an object, and not the object to bytes.
            /// </summary>
            /// <param name="b2o"></param>
            public AConvert(bool b2o)
            {
                this.b2o = b2o;
            }
        }
        /// <summary>
        /// This attribute signifies to the parser that it should be bytified with the object
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
        public class AConvertField : Attribute
        {
            public bool b2o;
            /// <summary>
            /// B2O means "bytes to object". Giving it the value true signifies it is converting the bytes given to an object, and not the object to bytes.
            /// </summary>
            /// <param name="b2o"></param>
            public AConvertField(bool b2o)
            {
                this.b2o = b2o;
            }
        }
    }
}