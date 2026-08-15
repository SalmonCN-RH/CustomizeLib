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
        // MyHook.InstallHook();
        // Console.WriteLine($"get enum {EnumValueReader.ReadEnumValue(typeof(AdvBuff), "EnumValue0")}");
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        //var dic = new Dictionary<string, object>();
        //int loop = 500;
        //for (int i = 0; i < loop; i++)
        //    dic.Add($"MyCustomValue{i}", 1500 + i);
        //EnumInjector.InjectEnumValues(typeof(UltiBuff), dic);
        //Console.WriteLine("inj");
        //for (int i = 0; i < loop; i++)
        //{
        //    var item = Il2CppSystem.Enum.Parse(Il2CppType.From(typeof(UltiBuff)), $"MyCustomValue{i}");
        //    Console.WriteLine($"{1500 + i} {(int)item.Unbox<UltiBuff>()}");
        //}
        //var cnt = Class_GetFieldDefaultValue_HookPatch.cnt;
        //Console.WriteLine($"{cnt}, {Il2CppSystem.Enum.GetValues(Il2CppType.From(typeof(UltiBuff))).Length}");
        // var builtin = Il2CppSystem.Enum.Parse(Il2CppType.From(typeof(AdvBuff)), "精兵强将");
        // Console.WriteLine($"builtin = {(int)builtin.Unbox<AdvBuff>()}");
        Console.WriteLine("--------------");
        // GetFieldDefaultValueRedirect.Install();
        EnumInjector.InjectEnumValues(typeof(AdvBuff), new Dictionary<string, object> { ["MyCustomValue"] = 1500 });

        //var enumPtr = Il2CppClassPointerStore.GetNativeClassPointer(typeof(AdvBuff));
        //var klass = UnityVersionHandler.Wrap((Il2CppClass*)enumPtr);
        //Console.WriteLine($"[check] wrapper FieldCount = {klass.FieldCount}");

        // 用 IL2CPP API 真正数一遍（这是循环实际用的计数）
        //IntPtr iter = IntPtr.Zero, f; int n = 0; IntPtr last = IntPtr.Zero;
        //while ((f = IL2CPP.il2cpp_class_get_fields(enumPtr, ref iter)) != IntPtr.Zero) { n++; last = f; }
        //Console.WriteLine($"[check] il2cpp_class_get_fields count = {n}, last = 0x{last.ToInt64():X}");

        // foreach (var k in GetFieldDefaultValueRedirect.DumpKeys())
            // Console.WriteLine($"[override key] 0x{k.ToInt64():X}");
        Console.WriteLine((int)Il2CppSystem.Enum.Parse(Il2CppType.From(typeof(AdvBuff)), "MyCustomValue").Unbox<AdvBuff>());

        // 在栈上分配 2 个指针大小的空间，用来存放调用参数
        IntPtr* ptr = stackalloc IntPtr[2];

        // 第 0 个参数：枚举类型对象指针
        ptr[0] = IL2CPP.Il2CppObjectBaseToPtr(Il2CppType.From(typeof(AdvBuff)));

        // 第 1 个参数：要解析的字符串值指针
        ptr[1] = IL2CPP.ManagedStringToIl2Cpp("MyCustomValue");

        // 用于接收原生代码抛出的异常信息
        IntPtr exception = IntPtr.Zero;

        // 调用 IL2CPP 原生方法（静态方法，实例参数传 0）
        var methodPtr = (IntPtr)typeof(Il2CppSystem.Enum).GetField("NativeMethodInfoPtr_Parse_Public_Static_Object_Type_String_0", BindingFlags.NonPublic | BindingFlags.Static)!.
            GetValue(null)!;
        Console.WriteLine($"{methodPtr == IntPtr.Zero}");
        IntPtr result = IL2CPP.il2cpp_runtime_invoke(
            methodPtr,
            IntPtr.Zero,              // 静态方法，无实例对象
            (void**)ptr,              // 参数数组指针
            ref exception
        );

        // 如果原生代码有异常，此处会抛出对应的托管异常
        // Il2CppInterop.Runtime.Il2CppException.RaiseExceptionIfNecessary(exception);

        // 如果返回结果不为空，从对象池中获取托管对象包装，否则返回 null
        Console.WriteLine($"{result}, {(int)Il2CppObjectPool.Get<Il2CppSystem.Object>(result).Unbox<AdvBuff>()}");
        // now read the integer at rawValuePtr  
        int value = Marshal.ReadInt32(result);
        Console.WriteLine($"{result}");

        unsafe
        {
            // 1. 验证字段
            IntPtr field = IL2CPP.GetIl2CppField(Il2CppClassPointerStore<AdvBuff>.NativeClassPtr, "全息制冷"); // 你的字段指针
            if (field == IntPtr.Zero) return;

            // 2. 获取父类
            IntPtr parentClass = IL2CPP.il2cpp_field_get_parent(field);
            if (parentClass == IntPtr.Zero) return;

            // 3. 强制初始化类
            IL2CPP.il2cpp_runtime_class_init(parentClass);

            // 4. 尝试读取现有值
            int existing = 0;
            IL2CPP.il2cpp_field_static_get_value(field, &existing);
            Console.WriteLine($"Read existing value: {existing}");
            // 检查字段是否为静态
            int flags = IL2CPP.il2cpp_field_get_flags(field);
            bool isStatic = (flags & 0x10) != 0; // 根据 IL2CPP FieldAttributes 定义
            Console.WriteLine($"{flags:X}");
            uint offset = IL2CPP.il2cpp_field_get_offset(field);
            Console.WriteLine($"Offset: 0x{offset:X}");
            IntPtr staticFieldsPtr = *(IntPtr*)(parentClass + 184);
            Console.WriteLine($"static_fields address: 0x{staticFieldsPtr:X}");
            // 5. 写入新值
            // int newValue = 1501;
            // IL2CPP.il2cpp_field_static_set_value(field, &newValue);
        }
        Console.WriteLine("----------------");
        ExecuteMachineCodeExample.Main();
        Console.WriteLine($"cor: {IL2CPP.il2cpp_get_corlib():X}");
        var clazz = Il2CppClassPointerStore<ActionCard>.NativeClassPtr;
        // 返回的是 Il2CppMethodInfo*
        var method = IL2CPP.il2cpp_class_get_method_from_name(clazz, "ClickedEvent", 0);
        var strc = UnityVersionHandler.Wrap((Il2CppMethodInfo*)(void*)method);
        Console.WriteLine($"va = 0x{strc.VirtualMethodPointer.ToInt64():X}, methodptr = 0x{strc.MethodPointer.ToInt64():X}," +
            $"token = {strc.Token:X}, il2cpp api: {IL2CPP.il2cpp_method_get_token(method):X}, method = {method:X}, " +
            $"name = {Marshal.PtrToStringUTF8(strc.Name)}");
        foreach (var @byte in MemTools.MemRead(strc.MethodPointer, 64))
        {
            Console.WriteLine($"command: 0x{@byte:X}");
        }
        using (Process currentProcess = Process.GetCurrentProcess())
        {
            // 1. 获取主模块（EXE）的基址
            IntPtr exeBase = currentProcess.MainModule.BaseAddress;
            Console.WriteLine($"主模块基址: 0x{exeBase.ToInt64():X}");

            // 2. 遍历所有模块，按名称查找特定 DLL（如 GameAssembly.dll）
            foreach (ProcessModule module in currentProcess.Modules)
            {
                if (module.ModuleName.Equals("GameAssembly.dll", StringComparison.OrdinalIgnoreCase))
                {
                    IntPtr dllBase = module.BaseAddress;
                    Console.WriteLine($"GameAssembly.dll 基址: 0x{dllBase.ToInt64():X}");
                    break;
                }
            }
        }
        var asm = Process.GetCurrentProcess().Modules
            .Cast<ProcessModule>()
            .FirstOrDefault(m => m.ModuleName == "GameAssembly.dll");

        if (asm == null)
            throw new Exception("IL2CPP module not found.");
        Console.WriteLine($"addr {asm.BaseAddress:X}");

        Console.WriteLine($"tool: {GetMethodRVA<AbyssSwordStar>("AbyssSwordStar", "PlantShootUpdate")}");
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
        Cpp2IL.Core.Cpp2IlApi.DetermineUnityVersion()
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