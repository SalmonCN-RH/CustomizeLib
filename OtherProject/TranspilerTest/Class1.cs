using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.UnmanagedTools;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.Runtime;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TranspilerTest;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum TestEnum
{
    Test1,
    Test2,
    Test3
}

[BepInPlugin("com.example.enumtest", "enumtest", "1.0")]
public class Plugin : BasePlugin
{
    private Harmony harmony;
    public static GCHandle tmp;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
        UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;      // 区域基址
        public IntPtr AllocationBase;   // 分配基址
        public uint AllocationProtect;  // 最初分配时的保护属性
        public IntPtr RegionSize;       // 区域大小 (在64位下是8字节)
        public uint State;              // 页面状态 (MEM_COMMIT, MEM_RESERVE, MEM_FREE)
        public uint Protect;            // 当前内存保护属性 (这就是你需要的)
        public uint Type;               // 页面类型 (MEM_IMAGE, MEM_MAPPED, MEM_PRIVATE)
    }

    // 声明 VirtualQueryEx 函数
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern int VirtualQueryEx(
        IntPtr hProcess,                // 目标进程句柄
        IntPtr lpAddress,               // 要查询的内存地址
        out MEMORY_BASIC_INFORMATION lpBuffer, // 接收信息的结构体
        uint dwLength                   // 结构体的大小
    );

    public override unsafe void Load()
    {
        var clsPtr = IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "", "UltimateSpring");
        IL2CPP.il2cpp_init(clsPtr);
        var cls = UnityVersionHandler.Wrap((Il2CppClass*)clsPtr);
        
        var data = ((VirtualInvokeData*)cls.VTable)[61];

        delegate*<IntPtr, IntPtr, IntPtr, bool> myClick = &MyOnClickedMethod;
        data.methodPtr = (IntPtr)myClick;
        var method = UnityVersionHandler.Wrap(data.method);
        method.MethodPointer = method.VirtualMethodPointer = (IntPtr)myClick;
        data.method = method.MethodInfoPointer;

        ((VirtualInvokeData*)cls.VTable)[61] = data;
    }

    public static bool MyOnClickedMethod(IntPtr @this, IntPtr mouse, IntPtr method)
    {
        Console.WriteLine("my click");
        return true;
    }

    public override bool Unload()
    {
        tmp.Free();
        harmony?.UnpatchSelf();
        return base.Unload();
    }

    public static GCHandle PinObjectVal(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        // 对于值类型，Alloc 会自动装箱，固定该装箱对象
        var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
        return handle;
    }

    /// <summary>
    /// 获取 IL2CPP 模块的基址
    /// </summary>
    private static IntPtr GetIl2CppModuleBase()
    {
        // Windows: GameAssembly.dll
        // Android: libil2cpp.so
        // iOS: UnityFramework
        string moduleName = "GameAssembly.dll"; // 根据平台修改

        using (Process process = Process.GetCurrentProcess())
        {
            ProcessModule module = process.Modules
                .Cast<ProcessModule>()
                .FirstOrDefault(m => m.ModuleName.Equals(moduleName,
                    StringComparison.OrdinalIgnoreCase));

            if (module == null)
                throw new Exception($"未找到模块: {moduleName}");

            return module.BaseAddress;
        }
    }

    /// <summary>
    /// 通过类名和方法名获取 MethodInfo 指针，并计算 RVA
    /// </summary>
    public static unsafe ulong GetMethodRVA<T>(string className, string methodName)
    {
        // 1. 获取 Il2Cpp 类指针
        IntPtr classPtr = Il2CppClassPointerStore<T>.NativeClassPtr;

        if (classPtr == IntPtr.Zero)
            throw new Exception($"未找到类: {className}");

        // 2. 获取 MethodInfo 指针 (参数数量为 0)
        IntPtr methodInfoPtr = IL2CPP.il2cpp_class_get_method_from_name(
            classPtr, methodName, 0
        );

        if (methodInfoPtr == IntPtr.Zero)
            throw new Exception($"未找到方法: {methodName}");

        // 3. 获取模块基址并计算 RVA
        IntPtr baseAddress = GetIl2CppModuleBase();
        var strc = UnityVersionHandler.Wrap((Il2CppMethodInfo*)(void*)methodInfoPtr);
        ulong rva = (ulong)strc.MethodPointer - (ulong)baseAddress;
        var klass = UnityVersionHandler.Wrap(strc.Class);
        for (int i = 0; i < klass.MethodCount; ++i)
        {
            var info = UnityVersionHandler.Wrap(*(klass.Methods + i));
            Console.WriteLine($"{info.MethodPointer:X}, {(IntPtr)(*(klass.Methods + i)):X}, {info.Token:X}, " +
                $"{IL2CPP.il2cpp_method_get_token((IntPtr)info.MethodInfoPointer):X}");
            Console.WriteLine($"{Marshal.PtrToStringUTF8(info.Name)}");
        }
        Cpp2IL.Core.Cpp2IlApi.InitializeLibCpp2Il("GameAssembly.dll", "metadata", new());
        Console.WriteLine($"{strc.MethodPointer:X}, {baseAddress:X}, {rva:X}");
        var filter = Il2CppSystem.Reflection.Module.FilterTypeName;
        var tmp = UnityVersionHandler.Wrap((Il2CppMethodInfo*)filter.method);
        Console.WriteLine($"{filter.method:X}, {filter.method_ptr:X}, {tmp.MethodPointer:X}, {tmp.GetType()}");
        return rva;
    }

    [HarmonyPatch]
    public static class Class_GetFieldDefaultValue_HookPatch
    {
        public static int cnt = 0;
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            var hookType = Type.GetType("Il2CppInterop.Runtime.Injection.Hooks.Class_GetFieldDefaultValue_Hook, Il2CppInterop.Runtime");

            return hookType?.GetMethod("Hook", BindingFlags.NonPublic | BindingFlags.Instance)!;
        }

        [HarmonyPostfix]
        public static unsafe void Postfix(ref Il2CppFieldInfo* field, ref Il2CppTypeStruct* type, ref byte* __result)
        {
            //var injector = Type.GetType("Il2CppInterop.Runtime.Injection.EnumInjector, Il2CppInterop.Runtime")!;
            //var get = injector.GetMethod("GetDefaultValueOverride", BindingFlags.Static | BindingFlags.NonPublic)!;
            //var args = new object[] { Pointer.Box(field, typeof(Il2CppFieldInfo*)), IntPtr.Zero };
            //var result = (bool)get!.Invoke(null, args)!;
            //cnt++;
            //if (!result) return;
            //var dic = (ConcurrentDictionary<IntPtr, IntPtr>)typeof(EnumInjector)!.GetField("s_DefaultValueOverrides", BindingFlags.NonPublic | BindingFlags.Static)!.
            //    GetValue(null)!;
            //Console.WriteLine((IntPtr)field);
            //var ptr = (IntPtr)args[1];
            //var bp = (byte*)ptr;
            //Console.WriteLine($"hook result {(IntPtr)__result}");
        }
    }

    public class ExecuteMachineCodeExample
    {
        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect
        );

        // 定义托管函数签名，用于调用机器码
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int AddFunction(int x, int y);

        const uint PAGE_EXECUTE_READWRITE = 0x40;

        public static void Main()
        {
            // 机器码：实现 add eax, edx 并返回
            byte[] machineCode = {
                0x55,                   // push ebp
                0x8B, 0x45, 0x08,       // mov eax, [ebp+8]
                0x8B, 0x55, 0x0C,       // mov edx, [ebp+12]
                0x01, 0xD0,             // add eax, edx
                0x5D,                   // pop ebp
                0xC3                    // ret
            };

            // 使用 unsafe 代码固定数组，获取其地址
            unsafe
            {
                fixed (byte* ptr = machineCode)
                {
                    IntPtr memoryAddress = (IntPtr)ptr;

                    // 将包含机器码的内存页标记为“可执行、可读、可写”
                    if (!VirtualProtectEx(
                        Process.GetCurrentProcess().Handle,
                        memoryAddress,
                        (UIntPtr)machineCode.Length,
                        PAGE_EXECUTE_READWRITE,
                        out uint _))
                    {
                        throw new System.ComponentModel.Win32Exception();
                    }

                    // 将内存地址转换为托管委托并调用
                    AddFunction add = Marshal.GetDelegateForFunctionPointer<AddFunction>(memoryAddress);
                    int result = add(10, -15);

                    Console.WriteLine($"计算结果: {result}"); // 输出: -5
                }
            }
        }
    }

    [HarmonyPatch(typeof(IL2CPP))]
    public static class IL2CPPPatch
    {
        [HarmonyPatch(nameof(IL2CPP.il2cpp_runtime_invoke))]
        [HarmonyPostfix]
        public static void PostRuntimeInvod(IntPtr __result)
        {
            //Console.WriteLine($"runtime invoke {__result}");
        }
    }
}