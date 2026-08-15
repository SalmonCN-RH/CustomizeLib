using Il2CppInterop.Runtime.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuLibHook.BepInEx
{
    public static class CppCore
    {
        public static Lazy<ProcessModule> GameAsmModule = new(() =>
        {
            using var currentProcess = Process.GetCurrentProcess();
            foreach (ProcessModule module in currentProcess.Modules)
                if (module!.ModuleName!.Equals("GameAssembly.dll"))
                    return module;
            throw new DllNotFoundException("Not found module GameAssembly.dll in process modules");
        });
        public static IntPtr GameAsmBaseAddress => GameAsmModule.Value.BaseAddress;

        public static unsafe IntPtr GetRuntimeVA(Il2CppMethodInfo* methodPtr) =>
            UnityVersionHandler.Wrap(methodPtr).MethodPointer;
        public static unsafe IntPtr GetRuntimeVA(IntPtr methodPtr) =>
            GetRuntimeVA((Il2CppMethodInfo*)methodPtr);
        public static unsafe IntPtr GetRuntimeRVA(Il2CppMethodInfo* methodPtr) =>
            (IntPtr)((ulong)GetRuntimeVA(methodPtr) - (ulong)GameAsmBaseAddress);
        public static unsafe IntPtr GetRuntimeRVA(IntPtr methodPtr) =>
            GetRuntimeRVA((Il2CppMethodInfo*)methodPtr);
    }
}
