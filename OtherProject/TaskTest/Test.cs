using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Il2CppRuntimeInvokeWithRef
{
    private class ByRefParameter
    {
        public int Index;
        public IntPtr Store;
        public Type ElementType;
        public GCHandle? PinnedHandle;
    }

    public unsafe static IntPtr CallIl2CppMethod(IntPtr methodPointer, IntPtr objectPointer, object[] parameters)
    {
        IntPtr exception = IntPtr.Zero;
        IntPtr paramArray = IntPtr.Zero;
        var byRefParams = new List<ByRefParameter>();

        try
        {
            // 分配参数数组内存  
            if (parameters != null && parameters.Length > 0)
            {
                paramArray = Marshal.AllocHGlobal(parameters.Length * IntPtr.Size);

                for (int i = 0; i < parameters.Length; i++)
                {
                    IntPtr paramPtr = PrepareParameter(parameters[i], i, byRefParams);
                    Marshal.WriteIntPtr(paramArray, i * IntPtr.Size, paramPtr);
                }
            }

            // 调用 il2cpp_runtime_invoke  
            IntPtr result = IL2CPP.il2cpp_runtime_invoke(
                methodPointer,
                objectPointer,
                (void**)paramArray,
                ref exception);

            // 检查异常  
            if (exception != IntPtr.Zero)
                Il2CppException.RaiseExceptionIfNecessary(exception);

            // 更新 ref/out 参数  
            UpdateByRefParameters(byRefParams, parameters);

            return result;
        }
        finally
        {
            // 清理内存  
            foreach (var byRefParam in byRefParams)
            {
                if (byRefParam.Store != IntPtr.Zero)
                    Marshal.FreeHGlobal(byRefParam.Store);
                if (byRefParam.PinnedHandle.HasValue)
                    byRefParam.PinnedHandle.Value.Free();
            }

            if (paramArray != IntPtr.Zero)
                Marshal.FreeHGlobal(paramArray);
        }
    }

    private static IntPtr PrepareParameter(object param, int index, List<ByRefParameter> byRefParams)
    {
        if (param == null)
            return IntPtr.Zero;

        var paramType = param.GetType();

        // 检查是否是 ref/out 包装类型  
        if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Ref<>))
        {
            var refType = paramType.GetGenericArguments()[0];
            var refValue = paramType.GetProperty("Value").GetValue(param);

            if (refType.IsValueType)
            {
                // 值类型 ref/out：固定内存并传递地址  
                var pinned = GCHandle.Alloc(refValue, GCHandleType.Pinned);
                var byRefParam = new ByRefParameter
                {
                    Index = index,
                    Store = IntPtr.Zero,
                    ElementType = refType,
                    PinnedHandle = pinned
                };
                byRefParams.Add(byRefParam);
                return pinned.AddrOfPinnedObject();
            }
            else
            {
                // 引用类型 ref/out：创建临时存储  
                IntPtr store = Marshal.AllocHGlobal(IntPtr.Size);
                IntPtr il2cppPtr = ConvertToIl2CppPointer(refValue);
                Marshal.WriteIntPtr(store, il2cppPtr);

                var byRefParam = new ByRefParameter
                {
                    Index = index,
                    Store = store,
                    ElementType = refType,
                    PinnedHandle = null
                };
                byRefParams.Add(byRefParam);
                return store;
            }
        }

        // 普通参数  
        return ConvertToIl2CppPointer(param);
    }

    private static IntPtr ConvertToIl2CppPointer(object obj)
    {
        if (obj == null)
            return IntPtr.Zero;

        if (obj is string str)
            return IL2CPP.ManagedStringToIl2Cpp(str);

        if (obj is Il2CppObjectBase objBase)
            return IL2CPP.Il2CppObjectBaseToPtr(objBase);

        throw new NotSupportedException($"Parameter type {obj.GetType()} is not supported");
    }

    private static void UpdateByRefParameters(List<ByRefParameter> byRefParams, object[] parameters)
    {
        foreach (var byRefParam in byRefParams)
        {
            var param = parameters[byRefParam.Index];
            var refType = param.GetType().GetGenericArguments()[0];

            if (refType.IsValueType)
            {
                // 值类型：从固定内存中读取更新后的值  
                if (byRefParam.PinnedHandle.HasValue)
                {
                    var pinned = byRefParam.PinnedHandle.Value;
                    var updatedValue = Marshal.PtrToStructure(pinned.AddrOfPinnedObject(), refType);
                    param.GetType().GetProperty("Value").SetValue(param, updatedValue);
                }
            }
            else
            {
                // 引用类型：从临时存储中读取 Il2Cpp 指针并转换  
                IntPtr il2cppValue = Marshal.ReadIntPtr(byRefParam.Store);
                object managedValue = ConvertFromIl2CppPointer(il2cppValue, refType);
                param.GetType().GetProperty("Value").SetValue(param, managedValue);
            }
        }
    }

    private static object ConvertFromIl2CppPointer(IntPtr il2cppPtr, Type targetType)
    {
        if (il2cppPtr == IntPtr.Zero)
            return null;

        if (targetType == typeof(string))
            return IL2CPP.Il2CppStringToManaged(il2cppPtr);

        if (typeof(Il2CppObjectBase).IsAssignableFrom(targetType))
        {
            var ctor = targetType.GetConstructor(new[] { typeof(IntPtr) });
            if (ctor != null)
                return ctor.Invoke(new object[] { il2cppPtr });
        }

        throw new NotSupportedException($"Cannot convert Il2Cpp pointer to type {targetType}");
    }

    // ref/out 参数包装类型  
    public class Ref<T>
    {
        public T Value { get; set; }

        public Ref(T value)
        {
            Value = value;
        }

        public static implicit operator Ref<T>(T value) => new Ref<T>(value);
    }
}