using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CuLibHook.BepInEx
{
    public static class AsmCore
    {
        public static unsafe object RunAssemblyCode<TDelegate>(byte* codes, uint size, params object?[]? args)
            where TDelegate : Delegate
        {
            if ((IntPtr)codes == IntPtr.Zero || size <= 0) throw new ArgumentException("Codes array cannot be null or empty");
            uint old = 0x00;
            var addr = (IntPtr)codes;
            try
            {
                if (!SystemAPIs.VirtualProtectEx(Process.GetCurrentProcess().Handle, addr, (UIntPtr)size,
                    SystemConstant.PAGE_EXECUTE_READWRITE, out old))
                {
                    throw new System.ComponentModel.Win32Exception();
                }
                var func = Marshal.GetDelegateForFunctionPointer<TDelegate>(addr);
                return func.DynamicInvoke(args)!;
            }
            finally
            {
                SystemAPIs.VirtualProtectEx(Process.GetCurrentProcess().Handle, addr, (UIntPtr)size,
                    old, out var _);
            }
        }
        public static unsafe byte[] GetAssemblyCode(byte* start, uint size)
        {
            var result = new byte[size];
            for (uint offset = 0; offset < size; ++offset)
                result[offset] = *(start + offset);
            return result;
        }
        public static unsafe bool SetAccess(byte* start, uint size, uint access, out uint oldAccess) =>
            SystemAPIs.VirtualProtectEx(Process.GetCurrentProcess().Handle, (IntPtr)start, (UIntPtr)size, access, out oldAccess);
        public static unsafe void SetAssemblyCode(byte* start, uint offset, byte[] newCodes)
        {
            SetAccess(start + offset, (uint)newCodes.Length, SystemConstant.PAGE_EXECUTE_READWRITE, out var old);
            for (uint idx = 0; idx < newCodes.Length; ++idx)
                *(start + offset + idx) = newCodes[idx];
            SystemAPIs.FlushInstructionCache(Process.GetCurrentProcess().Handle, (IntPtr)start, offset);
            SetAccess(start + offset, (uint)newCodes.Length, old, out var _);
        }
        public static GCHandle AllocMemmory(byte[] codes)
        {
            var alloc = new byte[codes.Length];
            Array.Copy(codes, alloc, codes.Length);
            return GCHandle.Alloc(alloc, GCHandleType.Pinned);
        }
        /// <summary>
        /// 找到在指定内存空间下所有符合<paramref name="func"/>的地址
        /// </summary>
        /// <param name="start">起始地址</param>
        /// <param name="size">单元长度</param>
        /// <param name="search">搜索长度(自起始字节(含)开始, 一直到要搜索的末尾字节(含))</param>
        /// <param name="func">搜索条件</param>
        /// <returns>所有满足搜索条件的地址</returns>
        public static unsafe IntPtr[] FindCodeOf(byte* start, uint size, uint search, Func<byte[], bool> func)
        {
            var result = new List<IntPtr>();
            for (uint offset = 0; offset <= search - size; ++offset)
            {
                byte[]? arr = GetAssemblyCode(start + offset, size);
                if (func.Invoke(arr))
                    result.Add((IntPtr)(start + offset));
            }
            return result.ToArray();
        }
    }
}
