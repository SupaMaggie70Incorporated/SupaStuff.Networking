using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;



namespace SupaStuff.Net.Packets.Util
{
    internal static class TypeFinder
    {
        private static bool hasGottenTypes = false;
        private static Type[] types = null;
        private static bool hasGottenMethods = false;
        private static MethodInfo[] methods = null;
        public static Type[] GetTypes()
        {
            if (hasGottenTypes) return TypeFinder.types;
            hasGottenTypes = true;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type[][] TypesPerAssembly = new Type[assemblies.Length][];
            int length = 0;
            for (int i = 0; i < assemblies.Length; i++)
            {
                TypesPerAssembly[i] = assemblies[i].GetTypes();
                length += TypesPerAssembly[i].Length;
            }
            Type[] types = new Type[length];
            int index = 0;
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] _types = TypesPerAssembly[i];
                for (int j = 0; j < _types.Length; j++)
                {
                    try
                    {
                        types[index++] = _types[j];
                    }catch
                    {
                        break;
                    }
                }
            }
            TypeFinder.types = types;
            return types;
        }
        public static Type[] ReGetTypes()
        {
            hasGottenTypes = true;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type[][] TypesPerAssembly = new Type[assemblies.Length][];
            int length = 0;
            for (int i = 0; i < assemblies.Length; i++)
            {
                TypesPerAssembly[i] = assemblies[i].GetTypes();
                length += TypesPerAssembly[i].Length;
            }
            Type[] types = new Type[length];
            int index = 0;
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] _types = TypesPerAssembly[i];
                for (int j = 0; j < _types.Length; j++)
                {
                    try
                    {
                        types[index++] = _types[j];
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            TypeFinder.types = types;
            return types;
        }
        public static MethodInfo[] GetMethods()
        {
            if (hasGottenMethods) return methods;
            Type[] types = GetTypes();
            hasGottenMethods = true;
            MethodInfo[][] methodsPerClass = new MethodInfo[types.Length][];
            int length = 0;
            for (int i = 0; i < types.Length; i++)
            {
                methodsPerClass[i] = types[i].GetMethods();
                length += methodsPerClass[i].Length;
            }
            MethodInfo[] _methods = new MethodInfo[length];
            int index = 0;
            for (int i = 0; i < types.Length; i++)
            {
                MethodInfo[] methods = methodsPerClass[i];
                for (int j = 0; j < methods.Length; j++)
                {
                    _methods[index] = _methods[j];
                    index++;
                }
            }
            methods = _methods;
            return _methods;
        }
        public static MethodInfo[] ReGetMethods()
        {
            Type[] types = ReGetTypes();
            hasGottenMethods = true;
            MethodInfo[][] methodsPerClass = new MethodInfo[types.Length][];
            int length = 0;
            for (int i = 0; i < types.Length; i++)
            {
                methodsPerClass[i] = types[i].GetMethods();
                length += methodsPerClass[i].Length;
            }
            MethodInfo[] _methods = new MethodInfo[length];
            int index = 0;
            for (int i = 0; i < types.Length; i++)
            {
                MethodInfo[] methods = methodsPerClass[i];
                for (int j = 0; j < methods.Length; j++)
                {
                    _methods[index] = _methods[j];
                    index++;
                }
            }
            methods = _methods;
            return _methods;
        }
    }
}