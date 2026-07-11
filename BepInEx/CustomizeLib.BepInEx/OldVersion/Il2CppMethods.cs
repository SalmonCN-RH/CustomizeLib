using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CustomizeLib.BepInEx.OldVersion.Il2CppMethodExtensions;

namespace CustomizeLib.BepInEx.OldVersion
{
    #pragma warning disable
    public static class Il2CppMethods
    {
        public static class Il2CppMethodCallingTools
        {
            public static Lazy<Func<string, IntPtr>> GetIl2CppImage = new(() =>
                GetIl2CppMethodInfo("GetIl2CppImage", BindingFlags.NonPublic | BindingFlags.Static).ToDelegate<Func<string, IntPtr>>());
            public static Lazy<Func<IntPtr[]>> GetIl2CppImages = new(() =>
                GetIl2CppMethodInfo("GetIl2CppImages", BindingFlags.NonPublic | BindingFlags.Static).ToDelegate<Func<IntPtr[]>>());
        }

        /// <summary>
        /// 获取Image
        /// </summary>
        /// <param name="name">dll名(带.dll后缀)</param>
        public static IntPtr GetIl2CppImage(string name) => Il2CppMethodCallingTools.GetIl2CppImage.Value.Invoke(name);

        /// <summary>
        /// 获取Images
        /// </summary>
        public static IntPtr[] GetIl2CppImages() => Il2CppMethodCallingTools.GetIl2CppImages.Value.Invoke();
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
