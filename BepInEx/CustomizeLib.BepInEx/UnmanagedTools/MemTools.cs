using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.UnmanagedTools
{
    /// <summary>
    /// 处理内存
    /// </summary>
    public static class MemTools
    {
        /// <summary>
        /// 写入字节(按字节)
        /// </summary>
        /// <param name="ptr">内存起始指针</param>
        /// <param name="val">值</param>
        /// <param name="length">长度</param>
        public static unsafe void MemSetByte(IntPtr ptr, byte val, uint length)
        {
            var p = (byte*)ptr;
            for (uint i = 0; i < length; i++)
                p[i] = val;
        }

        /// <summary>
        /// 写入字节(按类型大小)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="ptr">内存起始指针</param>
        /// <param name="val">值</param>
        /// <param name="times">次数</param>
        public static unsafe void MemSet<T>(IntPtr ptr, T val, uint times) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            var p = (byte*)ptr;
            for (uint i = 0; i < times; i++)
                Unsafe.Write(p + i * size, val);
        }

        /// <summary>
        /// 复制字节
        /// </summary>
        /// <param name="ptr">内存起始指针</param>
        /// <param name="target">目标内存指针</param>
        /// <param name="length">复制的字节数</param>
        public static unsafe void MemCpy(IntPtr ptr, IntPtr target, uint length)
        {
            // 调用库复制字节
            Buffer.MemoryCopy((byte*)ptr, (byte*)target, length, length);
        }

        /// <summary>
        /// 读取字节(按字节)
        /// </summary>
        /// <param name="ptr">内存起始指针</param>
        /// <param name="length">长度</param>
        /// <returns>值，索引代表偏移量</returns>
        public static unsafe byte[] MemRead(IntPtr ptr, uint length)
        {
            var p = (byte*)ptr;
            byte[] result = new byte[length];
            for (uint i = 0; i < length; i++)
                result[i] = p[i];
            return result;
        }

        /// <summary>
        /// 读取字节(按类型大小)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="ptr">内存起始指针</param>
        /// <param name="size">长度</param>
        /// <returns>值</returns>
        public static unsafe T[] MemRead<T>(IntPtr ptr, uint size) where T : unmanaged
        {
            var arr = new T[size];
            fixed (T* p = arr)
                Unsafe.CopyBlock(p, (void*)ptr, (uint)(sizeof(T) * size));
            return arr;
        }

        public static unsafe byte[] ToBytes<T>(this T value) where T : unmanaged
        {
            byte[] bytes = new byte[Unsafe.SizeOf<T>()];
            fixed (byte* p = bytes) Unsafe.Write(p, value);
            return bytes;
        }
    }
}
