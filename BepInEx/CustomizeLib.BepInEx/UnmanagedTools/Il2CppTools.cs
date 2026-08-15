using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.UnmanagedTools
{
    public static class Il2CppTupleHelper
    {
        private static Lazy<MethodInfo> ReadGeneric = new(() =>
            typeof(Unsafe).GetMethod(nameof(Unsafe.Read), BindingFlags.Public | BindingFlags.Static)!);
        private static Lazy<MethodInfo> PointerToValueGeneric = new(() =>
            typeof(IL2CPP).GetMethod(nameof(IL2CPP.PointerToValueGeneric), BindingFlags.Public | BindingFlags.Static)!);

        public static unsafe IntPtr CreateTuple(Type tupleType, params object[] items)
        {
            var clazz = Il2CppClassPointerStore.GetNativeClassPointer(tupleType);
            uint align = 0;
            int size = IL2CPP.il2cpp_class_value_size(clazz, ref align);
            var result = IL2CPP.il2cpp_alloc((uint)size);
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var field = IL2CPP.il2cpp_class_get_field_from_name(clazz, $"Item{i + 1}"); // 获取第i + 1个元素的字段指针
                SetFieldValue(result, field, item, item.GetType());
            }
            return result;
        }
        public static unsafe void SetFieldValue(IntPtr tuple, IntPtr field, object value, Type type)
        {
            var ptr = IntPtr.Zero;
            IntPtr? mem = null;
            if (type == typeof(string)) ptr = IL2CPP.il2cpp_string_new((string)value);
            else if (type.IsEnum)
            {
                var v = Convert.ToInt64(value);
                ptr = (IntPtr)(void*)&v;
            }
            else if (type.IsValueType)
            {
                mem = Marshal.AllocHGlobal(Marshal.SizeOf(type));
                Marshal.StructureToPtr(value, mem.Value, false);
                ptr = mem.Value;
            }
            else if (value is Il2CppObjectBase il2cppObj) ptr = il2cppObj.Pointer;
            IL2CPP.il2cpp_field_set_value(tuple, field, (void*)ptr);
            if (mem != null) Marshal.FreeHGlobal(mem.Value);
        }
        public static unsafe object[] GetTupleValues(IntPtr tuple, Type tupleType, params Type[] itemTypes)
        {
            var clazz = Il2CppClassPointerStore.GetNativeClassPointer(tupleType);
            var result = new object[itemTypes.Length];
            for (int i = 0; i < itemTypes.Length; i++)
            {
                var field = IL2CPP.il2cpp_class_get_field_from_name(clazz, $"Item{i + 1}"); // 获取第i + 1个元素的字段指针
                var itemType = itemTypes[i];
                var offset = IL2CPP.il2cpp_field_get_offset(field);
                result[i] = GetFieldValue(tuple, offset, itemType);
            }
            return result;
        }
        public static unsafe object GetFieldValue(IntPtr tuple, uint offset, Type type)
        {
            var fieldAddress = tuple + (int)offset;

            if (type == typeof(string))
            {
                var strPtr = *(IntPtr*)fieldAddress;
                return IL2CPP.Il2CppStringToManaged(strPtr)!;
            }
            else if (type.IsValueType)
                return ReadGeneric.Value.MakeGenericMethod(type).Invoke(null, [fieldAddress])!;
            else
                return PointerToValueGeneric.Value.MakeGenericMethod(type).Invoke(null, [*(IntPtr*)fieldAddress, false, false])!;
        }
    }
    public static class Il2CppDictionaryHelper
    {
        public static unsafe void DictionarySetItem<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> dic, TKey key, TValue value)
            where TKey : unmanaged where TValue : Il2CppObjectBase
        {
            if (dic == null) throw new ArgumentNullException(nameof(dic));
            var clazz = IL2CPP.il2cpp_object_get_class(dic.Pointer); // 获取字典类的类型指针
            var methodPtr = IL2CPP.GetIl2CppMethod(clazz, false, "set_Item", "System.Void", IL2CPP.RenderTypeName<TKey>(), IL2CPP.RenderTypeName<TValue>());
            if (methodPtr == IntPtr.Zero) throw new MissingMethodException(nameof(methodPtr));
            var rawValuePtr = IL2CPP.il2cpp_object_unbox(value.Pointer); // 字典的Item要求是unbox的
            var args = stackalloc IntPtr[2];
            args[0] = (IntPtr)(void*)&key;
            args[1] = rawValuePtr;
            var exec = IntPtr.Zero;
            IL2CPP.il2cpp_runtime_invoke(methodPtr, dic.Pointer, (void**)args, ref exec);

            if (exec != IntPtr.Zero)
                Il2CppException.RaiseExceptionIfNecessary(exec);
        }

        public static unsafe TValue DictionaryGetItem<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> dic, TKey key)
            where TKey : unmanaged where TValue : Il2CppObjectBase
        {
            if (dic == null) throw new ArgumentNullException(nameof(dic));
            var clazz = IL2CPP.il2cpp_object_get_class(dic.Pointer); // 获取字典类的类型指针
            var methodPtr = IL2CPP.GetIl2CppMethod(clazz, false, "get_Item", IL2CPP.RenderTypeName<TValue>(), IL2CPP.RenderTypeName<TKey>());
            if (methodPtr == IntPtr.Zero) throw new MissingMethodException(nameof(methodPtr));
            var args = stackalloc IntPtr[1];
            args[0] = (IntPtr)(void*)&key;
            var exec = IntPtr.Zero;
            var result = IL2CPP.il2cpp_runtime_invoke(methodPtr, dic.Pointer, (void**)args, ref exec);

            if (exec != IntPtr.Zero)
                Il2CppException.RaiseExceptionIfNecessary(exec);
            return Il2CppInterop.Runtime.Runtime.Il2CppObjectPool.Get<TValue>(result);
        }

        public static bool DictionaryTryGetValue<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> dic, TKey key, out TValue value)
            where TKey : unmanaged where TValue : Il2CppObjectBase
        {
            try
            {
                value = dic.DictionaryGetItem(key);
                return true;
            }
            catch (Exception)
            {
                value = null!;
                return false;
            }
        }

        public static bool DictionaryContainsKey<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> dic, TKey key)
            where TKey : unmanaged where TValue : Il2CppObjectBase
        {
            try
            {
                _ = dic.DictionaryGetItem(key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    public static class Il2CppHelper
    {
        public static List<IntPtr> GetAllMethods(Type type)
        {
            var result = new List<IntPtr>();
            var iter = IntPtr.Zero;
            var cur = IntPtr.Zero;
            var clazz = Il2CppClassPointerStore.GetNativeClassPointer(type);
            while ((cur = IL2CPP.il2cpp_class_get_methods(clazz, ref iter)) != IntPtr.Zero)
                result.Add(cur);
            return result;
        }
    }
}
