using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.Runtime;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TranspilerTest;
using UnityEngine;

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
        var builtin = Il2CppSystem.Enum.Parse(Il2CppType.From(typeof(AdvBuff)), "精兵强将");
        Console.WriteLine($"builtin = {(int)builtin.Unbox<AdvBuff>()}");
        Console.WriteLine("--------------");
        GetFieldDefaultValueRedirect.Install();
        EnumInjector.InjectEnumValues(typeof(AdvBuff), new Dictionary<string, object> { ["MyCustomValue"] = 1500 });

        var enumPtr = Il2CppClassPointerStore.GetNativeClassPointer(typeof(AdvBuff));
        var klass = UnityVersionHandler.Wrap((Il2CppClass*)enumPtr);
        Console.WriteLine($"[check] wrapper FieldCount = {klass.FieldCount}");

        // 用 IL2CPP API 真正数一遍（这是循环实际用的计数）
        IntPtr iter = IntPtr.Zero, f; int n = 0; IntPtr last = IntPtr.Zero;
        while ((f = IL2CPP.il2cpp_class_get_fields(enumPtr, ref iter)) != IntPtr.Zero) { n++; last = f; }
        Console.WriteLine($"[check] il2cpp_class_get_fields count = {n}, last = 0x{last.ToInt64():X}");

        foreach (var k in GetFieldDefaultValueRedirect.DumpKeys())
            Console.WriteLine($"[override key] 0x{k.ToInt64():X}");
        Console.WriteLine((int)Il2CppSystem.Enum.Parse(Il2CppType.From(typeof(AdvBuff)), "MyCustomValue").Unbox<AdvBuff>());
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

    //[HarmonyPatch]
    //public static class Class_GetFieldDefaultValue_HookPatch
    //{
    //    public static int cnt = 0;
    //    [HarmonyTargetMethod]
    //    public static MethodBase TargetMethod()
    //    {
    //        var hookType = Type.GetType("Il2CppInterop.Runtime.Injection.Hooks.Class_GetFieldDefaultValue_Hook, Il2CppInterop.Runtime");

    //        return hookType?.GetMethod("Hook", BindingFlags.NonPublic | BindingFlags.Instance)!;
    //    }

    //    [HarmonyPostfix]
    //    public static unsafe void Postfix(ref Il2CppFieldInfo* field, ref Il2CppTypeStruct* type, ref byte* __result)
    //    {
    //        var injector = Type.GetType("Il2CppInterop.Runtime.Injection.EnumInjector, Il2CppInterop.Runtime")!;
    //        var get = injector.GetMethod("GetDefaultValueOverride", BindingFlags.Static | BindingFlags.NonPublic)!;
    //        var args = new object[] { Pointer.Box(field, typeof(Il2CppFieldInfo*)), IntPtr.Zero };
    //        var result = (bool)get!.Invoke(null, args)!;
    //        cnt++;
    //        if (!result) return;
    //        var dic = (ConcurrentDictionary<IntPtr, IntPtr>)typeof(EnumInjector)!.GetField("s_DefaultValueOverrides", BindingFlags.NonPublic | BindingFlags.Static)!.
    //            GetValue(null)!;
    //        Console.WriteLine((IntPtr)field);
    //        var ptr = (IntPtr)args[1];
    //        var bp = (byte*)ptr;
    //        Console.WriteLine($"value = {ptr}, {*(int*)__result}");
    //    }
    //}
}