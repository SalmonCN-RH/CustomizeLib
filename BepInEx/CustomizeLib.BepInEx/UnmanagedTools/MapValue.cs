using BepInEx.Logging;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CustomizeLib.BepInEx.LibTools;

namespace CustomizeLib.BepInEx.UnmanagedTools
{
    public static class MapValue
    {
        private static bool Enable = false; // 是否启用(有没有找到Patcher的dll)
        private static Assembly PatcherAssembly = null!;
        private static Action<IntPtr, IntPtr, int> SetMapDelegate = null!;
        private static Func<IntPtr, (IntPtr, int)> GetMapDelegate = null!;
        private static Action<IntPtr> RemoveMapDelegate = null!;
        private static ManualLogSource Logger = new("LibResetValueLoader");

        internal static void InitDatas()
        {
            // Environment.CurrentDirectory 返回游戏本体exe的目录
            if (Enable) return;
            var patcherPath = Path.Combine(Environment.CurrentDirectory, Strings.PatcherPath);
            if (!File.Exists(patcherPath)) { Console.WriteLine($"Not found patcher in {patcherPath}"); return; }
            PatcherAssembly = Assembly.LoadFrom(patcherPath);
            var type = PatcherAssembly.GetType(Strings.PatcherCoreName);
            if (type == null) { Logger.LogWarning($"Can't find type {Strings.PatcherPath}"); return; }
            // 如果你替换了Patcher的dll，你应该保证存在这几个方法，lib不检查是否存在
            var setMap = type.GetMethod("SetMap", BindingFlags.Public | BindingFlags.Static)!;
            SetMapDelegate = (Action<IntPtr, IntPtr, int>)Delegate.CreateDelegate(typeof(Action<IntPtr, IntPtr, int>), null, setMap);
            var getMap = type.GetMethod("GetMap", BindingFlags.Public | BindingFlags.Static)!;
            GetMapDelegate = (Func<IntPtr, (IntPtr, int)>)Delegate.CreateDelegate(typeof(Func<IntPtr, (IntPtr, int)>), null, getMap);
            var removeMap = type.GetMethod("RemoveMap", BindingFlags.Public | BindingFlags.Static)!;
            RemoveMapDelegate = (Action<IntPtr>)Delegate.CreateDelegate(typeof(Action<IntPtr>), null, removeMap);
            Enable = true;
        }

        /// <summary>
        /// 创建值映射
        /// </summary>
        /// <param name="key">字段地址</param>
        /// <param name="value">字段值</param>
        /// <param name="size">长度(按字节)</param>
        public static void SetMap(IntPtr key, IntPtr value, int size)
        {
            InitDatas();
            SetMapDelegate!.Invoke(key, value, size);
        }

        /// <summary>
        /// 获取值映射
        /// </summary>
        /// <param name="key">字段地址</param>
        /// <returns>(字段值, 长度(按字节))</returns>
        public static (IntPtr, int) GetMap(IntPtr key)
        {
            InitDatas();
            return GetMapDelegate!.Invoke(key);
        }

        /// <summary>
        /// 删除值映射
        /// </summary>
        /// <param name="key">字段地址</param>
        public static void RemoveMap(IntPtr key)
        {
            InitDatas();
            RemoveMapDelegate!.Invoke(key);
        }
    }
}
