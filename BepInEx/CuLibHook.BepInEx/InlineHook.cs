using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CuLibHook.BepInEx
{
    public static class InlineHook
    {
        public static CulibHook Install(IntPtr target, uint overWritten, byte[] codes) =>
            InstallInternal(target, overWritten, codes, IntPtr.Zero);
        public static CulibHook Install(IntPtr target, uint overWritten, byte[] codes, IntPtr endAddr) =>
            InstallInternal(target, overWritten, codes, endAddr);

        private static CulibHook InstallInternal(IntPtr target, uint overWritten, byte[] codes, IntPtr endAddr)
        {
            if (target == IntPtr.Zero || overWritten < 5 || codes == null || codes.Length <= 0)
                throw new ArgumentException("target, overWritten or codes is invalid");

            var process = Process.GetCurrentProcess().Handle;
            var origin = new byte[overWritten];
            Marshal.Copy(target, origin, 0, (int)overWritten);

            // 分配hook代码的内存
            var hook = SystemAPIs.VirtualAlloc(IntPtr.Zero, (uint)codes.Length, 
                SystemConstant.MEM_COMMIT | SystemConstant.MEM_RESERVE, SystemConstant.PAGE_EXECUTE_READWRITE);
            if (hook == IntPtr.Zero)
                throw new Exception("Failed to allocate memory for hook code.");
            Marshal.Copy(codes, 0, hook, codes.Length);

            // 分配调用hook代码的内存(Inline Hook)
            var next = IntPtr.Add(target, (int)overWritten);
            var stubCode = endAddr == IntPtr.Zero ? GetStub(next, hook) : GetStub(next, hook, endAddr);
            var stub = SystemAPIs.VirtualAlloc(IntPtr.Zero, (uint)stubCode.Length,
                SystemConstant.MEM_COMMIT | SystemConstant.MEM_RESERVE, SystemConstant.PAGE_EXECUTE_READWRITE);
            if (stub == IntPtr.Zero)
            {
                SystemAPIs.VirtualFree(hook, 0, SystemConstant.MEM_RELEASE);
                throw new Exception("Failed to allocate stub memory.");
            }
            Marshal.Copy(stubCode, 0, stub, stubCode.Length);
            var fullPatch = GetFullPatch(target, stub, overWritten);

            if (!SystemAPIs.VirtualProtectEx(process, target, (UIntPtr)fullPatch.Length,
                SystemConstant.PAGE_EXECUTE_READWRITE, out uint oldProtect))
            {
                SystemAPIs.VirtualFree(hook, 0, SystemConstant.MEM_RELEASE);
                SystemAPIs.VirtualFree(stub, 0, SystemConstant.MEM_RELEASE);
                throw new Exception("Failed to modify memory protection.");
            }

            if (!SystemAPIs.WriteProcessMemory(process, target, fullPatch, (uint)fullPatch.Length, out uint bytesWritten) ||
                bytesWritten != fullPatch.Length)
            {
                SystemAPIs.VirtualProtectEx(process, target, (UIntPtr)fullPatch.Length, oldProtect, out _);
                SystemAPIs.VirtualFree(hook, 0, SystemConstant.MEM_RELEASE);
                SystemAPIs.VirtualFree(stub, 0, SystemConstant.MEM_RELEASE);
                throw new Exception("Failed to write the patch.");
            }
            SystemAPIs.VirtualProtectEx(process, target, (UIntPtr)fullPatch.Length, oldProtect, out _);
            SystemAPIs.FlushInstructionCache(process, target, (uint)fullPatch.Length);

            return new CulibHook(target, hook, stub, overWritten, origin);
        }

        public static byte[] GetFullPatch(IntPtr source, IntPtr destination, uint patchLen)
        {
            long diff = destination.ToInt64() - source.ToInt64() - 5;
            if (diff >= int.MinValue && diff <= int.MaxValue)
            {
                // 使用 E9 近跳转（5 字节）
                byte[] jmp = new byte[5];
                jmp[0] = 0xE9;
                BitConverter.GetBytes((int)diff).CopyTo(jmp, 1);
                byte[] fullPatch = new byte[patchLen];
                Array.Copy(jmp, fullPatch, jmp.Length);
                for (int i = jmp.Length; i < fullPatch.Length; i++) fullPatch[i] = 0x90;
                return fullPatch;
            }
            else
            {
                // 使用 jmp [rip] + 8字节地址（14 字节）
                byte[] jmp = new byte[14];
                // FF 25 00 00 00 00
                jmp[0] = 0xFF;
                jmp[1] = 0x25;
                jmp[2] = 0x00;
                jmp[3] = 0x00;
                jmp[4] = 0x00;
                jmp[5] = 0x00;
                // 8 字节目标地址
                BitConverter.GetBytes(destination.ToInt64()).CopyTo(jmp, 6);

                // 如果 patchLen > 14，填充 NOP，否则只写入前 patchLen 字节
                if (patchLen >= 14)
                {
                    byte[] fullPatch = new byte[patchLen];
                    Array.Copy(jmp, fullPatch, jmp.Length);
                    for (int i = jmp.Length; i < fullPatch.Length; i++) fullPatch[i] = 0x90;
                    return fullPatch;
                }
                else
                {
                    // 若 patchLen < 14，则无法容纳此跳转，应报错
                    throw new InvalidOperationException($"The patch length {patchLen} isn't enough to fit a 14-byte indirect jump.");
                }
            }
        }

        /// <summary>
        /// 获取hook函数通用代码
        /// 汇编:
        /// push rax
        /// mov rax, next
        /// jmp hook
        /// </summary>
        /// <param name="next">下一个要执行的命令的地址</param>
        /// <param name="hook">hook函数内存的起始位置</param>
        /// <returns>构造好的函数操作码数组</returns>
        public static byte[] GetStub(IntPtr next, IntPtr hook)
        {
            // 固定指令部分（地址占位符用 0 填充）
            byte[] fixedPart = new byte[]
            {
                0x50,                   // push rax
                0x48, 0xB8,             // mov rax, 
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // next (占位)
                0xFF, 0x25,             // jmp [rip+0]
                0x00, 0x00, 0x00, 0x00, // 偏移量 0（固定）
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00  // hook (占位)
            };

            // 复制一份，然后填入实际地址
            var stub = (byte[])fixedPart.Clone();
            // 填入 next (从索引 3(1 + 2) 开始，8 字节)
            BitConverter.GetBytes(next.ToInt64()).CopyTo(stub, 3);
            // 填入 hook (从索引 17(1 + 2 + 8 + 2 + 4) 开始，8 字节)
            BitConverter.GetBytes(hook.ToInt64()).CopyTo(stub, 17);

            return stub;
        }

        private static byte[] GetStub(IntPtr continueAddress, IntPtr hookAddress, IntPtr endAddress)
        {
            // 固定指令模板（地址占位符均为 0）
            byte[] fixedStub = new byte[]
            {
        0x50,                                     // push rax
        0x48, 0xB8,                               // mov rax, 
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // continueAddress (占位，索引 3~10)
        0xFF, 0x15,                               // call [rip+0]
        0x00, 0x00, 0x00, 0x00,                   // 偏移量 0（固定）
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // hookAddress (占位，索引 17~24)
        0x58,                                     // pop rax
        0xFF, 0x25,                               // jmp [rip+0]
        0x00, 0x00, 0x00, 0x00,                   // 偏移量 0（固定）
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00  // endAddress (占位，索引 32~39)
            };

            // 克隆模板，填入实际地址
            byte[] stub = (byte[])fixedStub.Clone();
            BitConverter.GetBytes(continueAddress.ToInt64()).CopyTo(stub, 3);
            BitConverter.GetBytes(hookAddress.ToInt64()).CopyTo(stub, 17);
            BitConverter.GetBytes(endAddress.ToInt64()).CopyTo(stub, 32);

            return stub;
        }
    }

    public class CulibHook : IDisposable
    {
        private bool Disposed = false;
        private IntPtr Target = IntPtr.Zero;
        private IntPtr Hook = IntPtr.Zero;
        private IntPtr JmpToHook = IntPtr.Zero;
        private uint OverWritten = 0;
        private byte[] OriginBytes = Array.Empty<byte>();
        private GCHandle Handle = new();

        internal CulibHook(IntPtr target, IntPtr hook, IntPtr toHook, uint overWritten, byte[] originBytes)
        {
            Target = target;
            Hook = hook;
            JmpToHook = toHook;
            OverWritten = overWritten;
            OriginBytes = originBytes;
            Handle = GCHandle.Alloc(this, GCHandleType.Normal);
        }

        public void UnInstall()
        {
            if (Disposed) return;

            var process = Process.GetCurrentProcess().Handle;
            if (SystemAPIs.VirtualProtectEx(process, Target, (UIntPtr)OverWritten,
                SystemConstant.PAGE_EXECUTE_READWRITE, out uint oldProtect))
            {
                SystemAPIs.WriteProcessMemory(process, Target, OriginBytes, (uint)OriginBytes.Length,
                    out uint _);
                SystemAPIs.VirtualProtectEx(process, Target, (UIntPtr)OverWritten, oldProtect, out _);
                SystemAPIs.FlushInstructionCache(process, Target, OverWritten);
            }

            // 释放分配的内存
            if (Hook != IntPtr.Zero)
                SystemAPIs.VirtualFree(Hook, 0, SystemConstant.MEM_RELEASE);
            if (JmpToHook != IntPtr.Zero)
                SystemAPIs.VirtualFree(JmpToHook, 0, SystemConstant.MEM_RELEASE);
            if (Handle.IsAllocated) // 释放引用
                Handle.Free();

            Disposed = true;
        }

        public void Dispose()
        {
            UnInstall();
            GC.SuppressFinalize(this);
        }
    }
}
