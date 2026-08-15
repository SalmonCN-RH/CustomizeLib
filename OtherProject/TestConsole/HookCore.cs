using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;

namespace TestConsole
{
    public static class HookCore
    {
        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect
        );

        public static unsafe object RunAssemblyCode<TDelegate>(byte* codes, uint size, params object?[]? args) 
            where TDelegate : Delegate
        {
            if ((IntPtr)codes == IntPtr.Zero || size <= 0) throw new ArgumentException("Codes array cannot be null or empty");
            uint old = 0x00;
            var addr = (IntPtr)codes;
            try
            {
                if (!VirtualProtectEx(Process.GetCurrentProcess().Handle, addr, (UIntPtr)size,
                    PageAccess.PAGE_EXECUTE_READWRITE, out old))
                {
                    throw new System.ComponentModel.Win32Exception();
                }
                var func = Marshal.GetDelegateForFunctionPointer<TDelegate>(addr);
                return func.DynamicInvoke(args)!;
            }
            finally
            {
                VirtualProtectEx(Process.GetCurrentProcess().Handle, addr, (UIntPtr)size,
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
            VirtualProtectEx(Process.GetCurrentProcess().Handle, (IntPtr)start, (UIntPtr)size, access, out oldAccess);
        public static unsafe void SetAssemblyCode(byte* start, uint offset, byte[] newCodes)
        {
            SetAccess(start + offset, (uint)newCodes.Length, PageAccess.PAGE_EXECUTE_READWRITE, out var old);
            for (uint idx = 0; idx < newCodes.Length; ++idx)
                *(start + offset + idx) = newCodes[idx];
            SetAccess(start + offset, (uint)newCodes.Length, old, out var _);
        }
        public static GCHandle AllocMemmory(byte[] codes)
        {
            var alloc = new byte[codes.Length];
            Array.Copy(codes, alloc, codes.Length);
            return GCHandle.Alloc(alloc, GCHandleType.Pinned);
        }
    }

    public static class PageAccess
    {
        public const uint PAGE_EXECUTE = 0x10;
        public const uint PAGE_EXECUTE_READ = 0x20;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;
        public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
        public const uint PAGE_NOACCESS = 0x01;
        public const uint PAGE_READONLY = 0x02;
        public const uint PAGE_READWRITE = 0x04;
        public const uint PAGE_WRITECOPY = 0x08;
        public const uint PAGE_TARGETS_INVALID = 0x40000000;
        public const uint PAGE_TARGETS_NO_UPDATE = 0x4000000;
    }
}
