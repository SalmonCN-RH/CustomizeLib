using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.UnmanagedTools
{
    public static class ObjectPinner
    {
        // 保存所有已分配的 GCHandle，用于后续统一释放
        private static readonly List<GCHandle> handles = new();

        /// <summary>
        /// 固定一个值类型对象（装箱后）并返回其内部数据的地址。
        /// </summary>
        /// <param name="value">值类型实例（会自动装箱）</param>
        /// <returns>固定后的内存地址</returns>
        /// <exception cref="ArgumentNullException">当 value 为 null 时抛出</exception>
        public static IntPtr PinObjectVal(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // 对于值类型，Alloc 会自动装箱，固定该装箱对象
            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            handles.Add(handle);
            return handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// 固定一个引用类型对象并返回其数据区的地址。
        /// </summary>
        /// <param name="value">引用类型实例</param>
        /// <returns>固定后的内存地址</returns>
        /// <exception cref="ArgumentNullException">当 value 为 null 时抛出</exception>
        public static IntPtr PinObjectRef(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            handles.Add(handle);
            return handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// 释放所有由本类固定的对象，解除固定并回收句柄。
        /// </summary>
        public static void Release()
        {
            foreach (var handle in handles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
            handles.Clear();
        }
    }
}
