using BepInEx.Logging;
using CustomizeLib.BepInEx.UnmanagedTools;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Internal
{
    internal static class HookCall
    {
        internal static bool load = false;

        internal static void SetBuffArr()
        {
            // advancedBuffsText
            var newAdvancedBuffsText = new Il2CppSystem.Collections.Generic.Dictionary<AdvBuff, string>();
            // 复制原来的值
            foreach (var item in TravelDictionary.advancedBuffsText)
                newAdvancedBuffsText[item.Key] = item.Value;
            // 复制新的值
            foreach (var item in CustomCore.CustomAdvancedBuffs)
                newAdvancedBuffsText[(AdvBuff)item.Key] = item.Value.Item2;
            // 复制引用
            TravelDictionary.advancedBuffsText = newAdvancedBuffsText;

            // AdvBuffPlantPairs
            var newAdvBuffPlantPairs = new Il2CppSystem.Collections.Generic.Dictionary<AdvBuff, PlantType>();
            foreach (var item in TravelDictionary.AdvBuffPlantPairs)
                newAdvBuffPlantPairs[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomAdvancedBuffs)
                newAdvBuffPlantPairs[(AdvBuff)item.Key] = item.Value.Item1;
            TravelDictionary.AdvBuffPlantPairs = newAdvBuffPlantPairs;

            // ultimateBuffsText
            var newUltimateBuffsText = new Il2CppSystem.Collections.Generic.Dictionary<UltiBuff, string>();
            foreach (var item in TravelDictionary.ultimateBuffsText)
                newUltimateBuffsText[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomUltimateBuffs)
                newUltimateBuffsText[(UltiBuff)item.Key] = item.Value.Item2;
            TravelDictionary.ultimateBuffsText = newUltimateBuffsText;

            // unlocksText
            var newUnlocksText = new Il2CppSystem.Collections.Generic.Dictionary<TravelUnlocks, string>();
            foreach (var item in TravelDictionary.unlocksText)
                newUnlocksText[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomUnlockBuffs)
                newUnlocksText[(TravelUnlocks)item.Key] = item.Value.Item2;
            TravelDictionary.unlocksText = newUnlocksText;

            // PlantToUnlock
            var newPlantToUnlock = new Il2CppSystem.Collections.Generic.Dictionary<PlantType, TravelUnlocks>();
            foreach (var item in TravelDictionary.PlantToUnlock)
                newPlantToUnlock[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomUnlockBuffs)
                newPlantToUnlock[item.Value.Item1] = (TravelUnlocks)item.Key;
            TravelDictionary.PlantToUnlock = newPlantToUnlock;

            // UnlockToPlant
            var newUnlockToPlant = new Il2CppSystem.Collections.Generic.Dictionary<TravelUnlocks, PlantType>();
            foreach (var item in TravelDictionary.UnlockToPlant)
                newUnlockToPlant[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomUnlockBuffs)
                newUnlockToPlant[(TravelUnlocks)item.Key] = item.Value.Item1;
            TravelDictionary.UnlockToPlant = newUnlockToPlant;

            //unsafe
            //{
            //    var newDebuffData = new Il2CppSystem.Collections.Generic.Dictionary<TravelDebuff, Il2CppSystem.ValueTuple<string, ZombieType>>();
            //    var clazz = Il2CppClassPointerStore.GetNativeClassPointer(typeof(Il2CppSystem.ValueTuple<string, ZombieType>));
            //    var tupleItem1 = IL2CPP.GetIl2CppField(clazz, "Item1");
            //    var tupleItem2 = IL2CPP.GetIl2CppField(clazz, "Item2");
            //    Console.WriteLine($"clazz ptr = {clazz}, item1 ptr = {tupleItem1}, item2 ptr = {tupleItem2}");
            //    var strPtr = IL2CPP.ManagedStringToIl2Cpp("text");
            //    Console.WriteLine($"il2cpp unbox {IL2CPP.Il2CppStringToManaged(strPtr)}");
            //    var type = ZombieType.NormalZombie;
            //    var typePtr = Marshal.AllocHGlobal(sizeof(int));
            //    var value = Marshal.AllocHGlobal(4); // int大小
            //    *(int*)typePtr = (int)type;
            //    //foreach (var item in TravelDictionary.debuffData)
            //    //{
            //    //    var tuple = IL2CPP.il2cpp_object_new(clazz);
            //    //    IL2CPP.il2cpp_field_set_value(tuple, tupleItem1, (void*)strPtr);
            //    //    IL2CPP.il2cpp_field_set_value(tuple, tupleItem2, (void*)typePtr);
            //    //    Console.WriteLine("before get item2");
            //    //    IL2CPP.il2cpp_field_get_value(TravelDictionary.debuffData[item.Key].Pointer, tupleItem2, (void*)value);
            //    //    Console.WriteLine($"before {*(int*)value}, {*(ZombieType*)value}");
            //    //    var cutuple = Il2CppObjectPool.Get<Il2CppSystem.ValueTuple<string, ZombieType>>(tuple);
            //    //    Console.WriteLine($"mytuple = {cutuple.Pointer}, ori = {TravelDictionary.debuffData[item.Key].Pointer}");
            //    //    IL2CPP.il2cpp_field_get_value(cutuple.Pointer, tupleItem2, (void*)value);
            //    //    var addr = (IntPtr)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull(cutuple) + IL2CPP.il2cpp_field_get_offset(tupleItem2));
            //    //    // [vtable] [Item1] [Item2]
            //    //    var myOffset = IntPtr.Size * 2;
            //    //    var myAddr = (IntPtr)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull(cutuple) + myOffset);
            //    //    Console.WriteLine($"update {cutuple.Item2}, actu {*(ZombieType*)value}, get by tuple {IL2CPP.PointerToValueGeneric<int>(addr, true, false)}," +
            //    //        $"addr {addr}, offset = {IL2CPP.il2cpp_field_get_offset(tupleItem2)}, myoffset = {myOffset}, get by myoff {IL2CPP.PointerToValueGeneric<int>(myAddr, true, false)}, " +
            //    //        $"myaddr = {myAddr}");
            //    //    Console.WriteLine($"update {cutuple.Item1}, {cutuple}");
            //    //    newDebuffData[item.Key] = cutuple;
            //    //}
            //    foreach (var item in CustomCore.CustomDebuffs)
            //    {
            //        var mytuple = Il2CppTupleHelper.CreateTuple(item.Value.Item1, item.Value.Item2);
            //        newDebuffData[(TravelDebuff)item.Key] = Il2CppObjectPool.Get<Il2CppSystem.ValueTuple<string, ZombieType>>(mytuple);
            //    }
            //    TravelDictionary.debuffData = newDebuffData;

            //    // 创建 ValueTuple<string, int>  
            //    var t = Il2CppTupleHelper.CreateTuple("hello", ZombieType.NormalZombie);
            //    var it1 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore.GetNativeClassPointer(typeof(Il2CppSystem.ValueTuple<string, ZombieType>)), "Item1");
            //    var it2 = IL2CPP.GetIl2CppField(Il2CppClassPointerStore.GetNativeClassPointer(typeof(Il2CppSystem.ValueTuple<string, ZombieType>)), "Item2");
            //    var mem = Marshal.AllocHGlobal(4);
            //    var strP = IntPtr.Zero;
            //    IL2CPP.il2cpp_field_get_value(t, it1, &strP);
            //    Console.WriteLine($"strP = {strP}");
            //    if (strP != IntPtr.Zero)
            //        Console.WriteLine($"get it1 {IL2CPP.Il2CppStringToManaged(strP)}, {strP}");
            //    IL2CPP.il2cpp_field_get_value(t, it2, ((void*)mem));
            //    Console.WriteLine($"get it2 {*(ZombieType*)mem}");
            //    var obj = Il2CppObjectPool.Get<Il2CppSystem.ValueTuple<string, ZombieType>>(t);
            //    var (t1, t2) = Il2CppTupleHelper.GetTupleValue<string, ZombieType>(t);
            //    Console.WriteLine($"{t1}, {t2}");
            //    Console.WriteLine($"convert {obj.Item1}, {obj.Item2}");
            //    var offset = (int)IL2CPP.il2cpp_field_get_offset(it2);
            //    var instance = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull(obj);
            //    var addr = instance + offset;
            //    Console.WriteLine($"{offset}, {instance}, {addr}, {IL2CPP.PointerToValueGeneric<int>(addr, true, false)}, {IL2CPP.PointerToValueGeneric<int>(addr, false, false)}, {*(int*)addr}");
            //}
        }

        internal static void RegisterTypes()
        {
            // 以备后用
        }
    }

    //public static class Il2CppTupleHelper
    //{
    //    /// <summary>  
    //    /// 创建 Il2CppSystem.ValueTuple<T1, T2> 实例  
    //    /// </summary>  
    //    public static unsafe IntPtr CreateTuple<T1, T2>(T1 item1, T2 item2)
    //    {
    //        IntPtr tupleClass = Il2CppClassPointerStore<Il2CppSystem.ValueTuple<T1, T2>>.NativeClassPtr;

    //        // 获取字段信息  
    //        IntPtr item1Field = IL2CPP.il2cpp_class_get_field_from_name(tupleClass, "Item1");
    //        IntPtr item2Field = IL2CPP.il2cpp_class_get_field_from_name(tupleClass, "Item2");

    //        // 获取字段偏移量  
    //        uint item1Offset = IL2CPP.il2cpp_field_get_offset(item1Field);
    //        uint item2Offset = IL2CPP.il2cpp_field_get_offset(item2Field);

    //        // 分配内存  
    //        uint align = 0;
    //        int size = IL2CPP.il2cpp_class_value_size(tupleClass, ref align);
    //        IntPtr tupleMemory = IL2CPP.il2cpp_alloc((uint)size);

    //        // 设置 Item1  
    //        SetFieldValue<T1>(tupleMemory, item1Offset, item1);

    //        // 设置 Item2  
    //        SetFieldValue<T2>(tupleMemory, item2Offset, item2);

    //        return tupleMemory;
    //    }

    //    /// <summary>  
    //    /// 从 Il2CppSystem.ValueTuple<T1, T2> 实例获取 Item1 和 Item2  
    //    /// </summary>  
    //    public static unsafe (T1, T2) GetTupleValue<T1, T2>(IntPtr tuplePtr)
    //    {
    //        // 获取 ValueTuple 类指针  
    //        IntPtr tupleClass = Il2CppClassPointerStore<Il2CppSystem.ValueTuple<T1, T2>>.NativeClassPtr;

    //        // 获取字段信息  
    //        IntPtr item1Field = IL2CPP.il2cpp_class_get_field_from_name(tupleClass, "Item1");
    //        IntPtr item2Field = IL2CPP.il2cpp_class_get_field_from_name(tupleClass, "Item2");

    //        // 获取字段偏移量  
    //        uint item1Offset = IL2CPP.il2cpp_field_get_offset(item1Field);
    //        uint item2Offset = IL2CPP.il2cpp_field_get_offset(item2Field);

    //        // 读取字段值  
    //        T1 item1 = GetFieldValue<T1>(tuplePtr, item1Offset);
    //        T2 item2 = GetFieldValue<T2>(tuplePtr, item2Offset);

    //        return (item1, item2);
    //    }

    //    private static unsafe void SetFieldValue<T>(IntPtr tupleMemory, uint offset, T value)
    //    {
    //        IntPtr fieldAddress = tupleMemory + (int)offset;

    //        if (typeof(T) == typeof(string))
    //        {
    //            // 字符串类型特殊处理  
    //            IntPtr stringPtr = IL2CPP.il2cpp_string_new((string)(object)value);
    //            *(IntPtr*)fieldAddress = stringPtr;
    //        }
    //        else if (typeof(T).IsValueType)
    //        {
    //            // 值类型直接拷贝  
    //            Unsafe.Write(fieldAddress.ToPointer(), value);
    //        }
    //        else
    //        {
    //            // 引用类型存储指针  
    //            if (value is Il2CppObjectBase obj)
    //            {
    //                *(IntPtr*)fieldAddress = obj.Pointer;
    //            }
    //            else
    //            {
    //                *(IntPtr*)fieldAddress = IntPtr.Zero;
    //            }
    //        }
    //    }

    //    private static unsafe T GetFieldValue<T>(IntPtr tuplePtr, uint offset)
    //    {
    //        IntPtr fieldAddress = tuplePtr + (int)offset;

    //        if (typeof(T) == typeof(string))
    //        {
    //            // 字符串类型特殊处理  
    //            IntPtr stringPtr = *(IntPtr*)fieldAddress;
    //            return (T)(object)IL2CPP.Il2CppStringToManaged(stringPtr);
    //        }
    //        else if (typeof(T).IsValueType)
    //        {
    //            // 值类型直接读取  
    //            return Unsafe.Read<T>(fieldAddress.ToPointer());
    //        }
    //        else
    //        {
    //            // 引用类型使用 PointerToValueGeneric  
    //            IntPtr objectPtr = *(IntPtr*)fieldAddress;
    //            return IL2CPP.PointerToValueGeneric<T>(objectPtr, false, false);
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(PauseMenu_Btn))]
    public static class PauseMenu_BtnPatch
    {
        [HarmonyPatch(nameof(PauseMenu_Btn.OnMouseUp))]
        [HarmonyPostfix]
        public static void PostOnMouseUp()
        {
            if (HookCall.load) return;
            HookCall.SetBuffArr();
            HookCall.load = true;
        }
    }

    //[HarmonyPatch(typeof(Il2CppSystem.ValueTuple<string, ZombieType>))]
    //public static class ValueTupleWithStringZombieTypePatch
    //{
    //    [HarmonyPatch(nameof(Il2CppSystem.ValueTuple<string, ZombieType>.Item1), MethodType.Getter)]
    //    [HarmonyPrefix]
    //    public static unsafe bool PreGetItem1(Il2CppSystem.ValueTuple<string, ZombieType> __instance, ref string __result)
    //    {
    //        Console.WriteLine("try get item1");
    //        var clazz = Il2CppClassPointerStore.GetNativeClassPointer(typeof(Il2CppSystem.ValueTuple<string, ZombieType>));
    //        var tupleItem2 = IL2CPP.GetIl2CppField(clazz, "Item1");
    //        var res = IntPtr.Zero;
    //        IL2CPP.il2cpp_field_get_value(__instance.Pointer, tupleItem2, &res);
    //        __result = IL2CPP.Il2CppStringToManaged(res)!;
    //        return false;
    //    }

    //    [HarmonyPatch(nameof(Il2CppSystem.ValueTuple<string, ZombieType>.Item2), MethodType.Getter)]
    //    [HarmonyPrefix]
    //    public static unsafe bool PreGetItem2(Il2CppSystem.ValueTuple<string, ZombieType> __instance, ref ZombieType __result)
    //    {
    //        Console.WriteLine("try get item2");
    //        var clazz = Il2CppClassPointerStore.GetNativeClassPointer(typeof(Il2CppSystem.ValueTuple<string, ZombieType>));
    //        var tupleItem2 = IL2CPP.GetIl2CppField(clazz, "Item2");
    //        var res = Marshal.AllocHGlobal(sizeof(int));
    //        IL2CPP.il2cpp_field_get_value(__instance.Pointer, tupleItem2, (void*)res);
    //        __result = *(ZombieType*)res;
    //        Marshal.FreeHGlobal(res);
    //        return false;
    //    }
    //}

    //public static class TestUtils
    //{
    //    /// <summary>
    //    /// 强制将 Il2CppSystem.ValueTuple 等装箱类型的值插入到 Il2CppSystem.Collections.Generic.Dictionary 中。
    //    /// <para>
    //    /// <b>问题背景 (Context):</b><br/>
    //    /// 在 Il2CppInterop 中，`Il2CppSystem.ValueTuple` 被映射为一个类 (Class)，它在托管堆上持有一个指向 IL2CPP 对象的指针。
    //    /// 这个 IL2CPP 对象是已装箱 (Boxed) 的，包含对象头 (Header, 16字节) 和实际数据。<br/>
    //    /// 然而，底层的 C++ `Dictionary` (泛型参数为结构体时) 存储的是未装箱 (Unboxed) 的纯数据。<br/>
    //    /// 当直接调用 `dictionary.Add(key, value)` 时，Interop 层错误地将装箱对象的指针 (指向 Header) 传递给了底层方法，
    //    /// 导致底层方法将 Header 当作数据拷贝进字典，造成数据偏移 (Item3 变成 Item1) 和内存破坏 (读取 Item1 时访问非法指针)。
    //    /// </para>
    //    /// <para>
    //    /// <b>解决方案 (Solution):</b><br/>
    //    /// 此方法通过 `IL2CPP.il2cpp_object_unbox` 获取跳过 Header 后的纯数据指针，
    //    /// 并通过反射手动调用底层的 `set_Item` 方法，确保传入正确的数据指针。
    //    /// </para>
    //    /// </summary>
    //    /// <typeparam name="TKey">键类型 (必须是值类型，如 int, long, float 等)</typeparam>
    //    /// <typeparam name="TValue">值类型 (必须是 Il2CppSystem.Object 的子类，通常是 ValueTuple)</typeparam>
    //    /// <param name="dictionary">目标字典</param>
    //    /// <param name="key">键</param>
    //    /// <param name="value">值</param>
    //    public static unsafe void ForceAddOrUpdateValueTuple<TKey, TValue>(
    //        this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> dictionary,
    //        TKey key,
    //        TValue value)
    //        where TKey : struct
    //        where TValue : Il2CppSystem.Object
    //    {
    //        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
    //        if (value == null) throw new ArgumentNullException(nameof(value));

    //        // 1. 获取原始数据指针 (Get pointer to raw struct data, skipping the object header)
    //        System.IntPtr rawDataPtr = IL2CPP.il2cpp_object_unbox(value.Pointer);

    //        // 2. 查找 set_Item 方法 (Find set_Item method)
    //        // 我们需要缓存这个 MethodInfo 吗？为了性能可以考虑，但为了稳定性先实时查找。
    //        System.IntPtr methodPtr = System.IntPtr.Zero;
    //        System.IntPtr iter = System.IntPtr.Zero;
    //        System.IntPtr curMethod = System.IntPtr.Zero;
    //        System.IntPtr classPtr = IL2CPP.il2cpp_object_get_class(dictionary.Pointer);

    //        while ((curMethod = IL2CPP.il2cpp_class_get_methods(classPtr, ref iter)) != System.IntPtr.Zero)
    //        {
    //            string name = Marshal.PtrToStringAnsi(IL2CPP.il2cpp_method_get_name(curMethod));
    //            // 目标方法签名: set_Item(TKey key, TValue value)
    //            // 参数数量应为 2
    //            if (name == "set_Item" && IL2CPP.il2cpp_method_get_param_count(curMethod) == 2)
    //            {
    //                methodPtr = curMethod;
    //                break;
    //            }
    //        }

    //        if (methodPtr == System.IntPtr.Zero)
    //        {
    //            return;
    //        }

    //        // 3. 调用方法 (Invoke)
    //        // 构造参数数组。注意：对于泛型字典，Key 和 Value 通常都是通过指针传递的。
    //        // TKey 是 struct，我们将其装箱并固定，获取其数据指针。
    //        // TValue 是 struct (在 C++ 侧)，我们传 unboxed data pointer。

    //        GCHandle keyHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
    //        try
    //        {
    //            System.IntPtr* args = stackalloc System.IntPtr[2];
    //            args[0] = keyHandle.AddrOfPinnedObject();
    //            args[1] = rawDataPtr;

    //            System.IntPtr exc = System.IntPtr.Zero;
    //            IL2CPP.il2cpp_runtime_invoke(methodPtr, dictionary.Pointer, (void**)args, ref exc);
    //        }
    //        finally
    //        {
    //            keyHandle.Free();
    //        }
    //    }
    //}
}
