using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable
namespace CustomizeLib.BepInEx.UnmanagedTools
{
    public static class Il2CppMethods
    {
        public static Dictionary<string, Delegate> CachedMethod = new();

        public static T GetIl2CppMethod<T>(string name) where T : Delegate
        {
            if (!CachedMethod.TryGetValue(name, out var func))
            {
                var result = Il2CppMethodExtensions.GetIl2CppMethodInfo(name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).ToDelegate<T>();
                if (!CachedMethod.ContainsKey(name)) CachedMethod.Add(name, result);
                return result;
            }
            return (T)func;
        }
    }

    public static class Il2CppMethodExtensions
    {
        public static MethodInfo GetIl2CppMethodInfo(string name, BindingFlags flags) =>
            typeof(IL2CPP).GetMethod(name, flags);
        public static MethodInfo GetIl2CppMethodInfo(string name, BindingFlags flags, Type[] args) =>
            typeof(IL2CPP).GetMethod(name, flags, args);
        public static T ToDelegate<T>(this MethodInfo info) where T : Delegate => (T)info.CreateDelegate(typeof(T));
    }
}
