using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CustomizeLib.BepInEx.UnmanagedTools
{
#pragma warning disable
    public static class MethodInvoke
    {
        #region Type 版本的方法指针获取  

        /// <summary>  
        /// 获取基类方法指针（精确类型匹配）  
        /// </summary>  
        public static IntPtr GetBaseMethodPtr(Type baseType, string methodName, Type returnType, params Type[] paramTypes)
        {
            IntPtr baseClassPtr = Il2CppClassPointerStore.GetNativeClassPointer(baseType);
            if (baseClassPtr == IntPtr.Zero)
                throw new ArgumentException($"Base type {baseType} is not registered in Il2Cpp");

            string returnTypeName = IL2CPP.RenderTypeName(returnType);
            string[] argTypeNames = new string[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                argTypeNames[i] = IL2CPP.RenderTypeName(paramTypes[i]);
            }

            return IL2CPP.GetIl2CppMethod(baseClassPtr, false, methodName, returnTypeName, argTypeNames);
        }

        /// <summary>  
        /// 获取基类泛型方法指针  
        /// </summary>  
        public static IntPtr GetBaseGenericMethodPtr(Type baseType, string methodName, Type returnType, params Type[] paramTypes)
        {
            IntPtr baseClassPtr = Il2CppClassPointerStore.GetNativeClassPointer(baseType);
            if (baseClassPtr == IntPtr.Zero)
                throw new ArgumentException($"Base type {baseType} is not registered in Il2Cpp");

            string returnTypeName = IL2CPP.RenderTypeName(returnType);
            string[] argTypeNames = new string[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                argTypeNames[i] = IL2CPP.RenderTypeName(paramTypes[i]);
            }

            return IL2CPP.GetIl2CppMethod(baseClassPtr, true, methodName, returnTypeName, argTypeNames);
        }

        #endregion

        #region String 版本的方法指针获取  

        /// <summary>  
        /// 获取基类方法指针（精确类型匹配 - string 版本）  
        /// </summary>  
        public static IntPtr GetBaseMethodPtr(Type baseType, string methodName, string returnTypeName, params string[] paramTypeNames)
        {
            IntPtr baseClassPtr = Il2CppClassPointerStore.GetNativeClassPointer(baseType);
            if (baseClassPtr == IntPtr.Zero)
                throw new ArgumentException($"Base type {baseType} is not registered in Il2Cpp");

            return IL2CPP.GetIl2CppMethod(baseClassPtr, false, methodName, returnTypeName, paramTypeNames);
        }

        /// <summary>  
        /// 获取基类泛型方法指针（string 版本）  
        /// </summary>  
        public static IntPtr GetBaseGenericMethodPtr(Type baseType, string methodName, string returnTypeName, params string[] paramTypeNames)
        {
            IntPtr baseClassPtr = Il2CppClassPointerStore.GetNativeClassPointer(baseType);
            if (baseClassPtr == IntPtr.Zero)
                throw new ArgumentException($"Base type {baseType} is not registered in Il2Cpp");

            return IL2CPP.GetIl2CppMethod(baseClassPtr, true, methodName, returnTypeName, paramTypeNames);
        }

        #endregion

        #region 方法调用  

        /// <summary>
        /// 调用基类方法
        /// 有参无ref/out
        /// </summary>
        public static T InvokeBase<T>(IntPtr methodPtr, Il2CppObjectBase instance, Type returnType, object[] args) =>
            InvokeBase<T>(methodPtr, instance, returnType, args, new bool[args.Length]);

        /// <summary>
        /// 调用基类方法
        /// 无参
        /// </summary>
        public static T InvokeBase<T>(IntPtr methodPtr, Il2CppObjectBase instance, Type returnType) =>
            InvokeBase<T>(methodPtr, instance, returnType, new object[0], new bool[0]);

        /// <summary>
        /// 调用基类方法
        /// 有参无ref/out，void
        /// </summary>
        public static void InvokeBase(IntPtr methodPtr, Il2CppObjectBase instance, object[] args) =>
            InvokeBase<object>(methodPtr, instance, typeof(void), args, new bool[args.Length]);

        /// <summary>
        /// 调用基类方法
        /// 无参，void
        /// </summary>
        public static void InvokeBase(IntPtr methodPtr, Il2CppObjectBase instance) =>
            InvokeBase<object>(methodPtr, instance, typeof(void), new object[0], new bool[0]);

        /// <summary>
        /// 调用基类方法
        /// 有参有ref/out，void
        /// </summary>
        public static void InvokeBase(IntPtr methodPtr, Il2CppObjectBase instance, object[] args, bool[] isRefOrOut) =>
            InvokeBase<object>(methodPtr, instance, typeof(void), args, isRefOrOut);

        /// <summary>  
        /// 调用基类方法
        /// </summary>  
        public unsafe static T InvokeBase<T>(IntPtr methodPtr, Il2CppObjectBase instance, Type returnType, object[] args, bool[] isRefOrOut)
        {
            void** paramsArray = null;
            IntPtr[] paramBuffers = null;
            GCHandle[] handles = null;

            try
            {
                // 处理参数  
                if (args != null && args.Length > 0)
                {
                    paramsArray = (void**)Marshal.AllocHGlobal(args.Length * IntPtr.Size);
                    paramBuffers = new IntPtr[args.Length];
                    handles = new GCHandle[args.Length];

                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == null)
                        {
                            paramsArray[i] = (void*)IntPtr.Zero;
                            continue;
                        }

                        Type argType = args[i].GetType();

                        // 处理 ref/out 参数  
                        if (isRefOrOut != null && i < isRefOrOut.Length && isRefOrOut[i])
                        {
                            if (argType.IsValueType)
                            {
                                // 值类型 ref/out：固定内存并传递地址  
                                handles[i] = GCHandle.Alloc(args[i], GCHandleType.Pinned);
                                paramsArray[i] = (void*)handles[i].AddrOfPinnedObject();
                            }
                            else
                            {
                                // 引用类型 ref/out：分配内存并存储指针  
                                paramBuffers[i] = Marshal.AllocHGlobal(IntPtr.Size);

                                IntPtr objPtr = IntPtr.Zero;
                                if (argType == typeof(string))
                                {
                                    objPtr = IL2CPP.ManagedStringToIl2Cpp((string)args[i]);
                                }
                                else if (typeof(Il2CppObjectBase).IsAssignableFrom(argType))
                                {
                                    objPtr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)args[i]);
                                }

                                Marshal.WriteIntPtr(paramBuffers[i], objPtr);
                                paramsArray[i] = (void*)paramBuffers[i];
                            }
                        }
                        else
                        {
                            // 普通参数  
                            if (argType == typeof(string))
                            {
                                IntPtr il2cppString = IL2CPP.ManagedStringToIl2Cpp((string)args[i]);
                                paramBuffers[i] = il2cppString;
                                paramsArray[i] = (void*)il2cppString;
                            }
                            else if (typeof(Il2CppObjectBase).IsAssignableFrom(argType))
                            {
                                IntPtr objPtr = IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)args[i]);
                                paramBuffers[i] = objPtr;
                                paramsArray[i] = (void*)objPtr;
                            }
                            else if (argType.IsValueType)
                            {
                                handles[i] = GCHandle.Alloc(args[i], GCHandleType.Pinned);
                                IntPtr ptr = handles[i].AddrOfPinnedObject();
                                paramsArray[i] = (void*)ptr;
                            }
                            else
                            {
                                paramsArray[i] = (void*)IntPtr.Zero;
                            }
                        }
                    }
                }

                // 调用方法  
                IntPtr exc = IntPtr.Zero;
                IntPtr result = IL2CPP.il2cpp_runtime_invoke(
                    methodPtr,
                    instance?.Pointer ?? IntPtr.Zero,
                    paramsArray,
                    ref exc
                );

                // 处理异常  
                if (exc != IntPtr.Zero)
                {
                    Il2CppException.RaiseExceptionIfNecessary(exc);
                }

                // 处理 ref/out 参数的返回值  
                if (isRefOrOut != null && args != null)
                {
                    for (int i = 0; i < args.Length && i < isRefOrOut.Length; i++)
                    {
                        if (isRefOrOut[i] && paramBuffers[i] != IntPtr.Zero)
                        {
                            Type argType = args[i].GetType();
                            if (!argType.IsValueType)
                            {
                                // 从缓冲区读取更新后的指针  
                                IntPtr updatedPtr = Marshal.ReadIntPtr(paramBuffers[i]);

                                if (argType == typeof(string))
                                {
                                    args[i] = IL2CPP.Il2CppStringToManaged(updatedPtr);
                                }
                                else if (typeof(Il2CppObjectBase).IsAssignableFrom(argType))
                                {
                                    // 需要根据具体类型创建实例  
                                    args[i] = IL2CPP.PointerToValueGeneric<object>(updatedPtr, false, false);
                                }
                            }
                            else if (handles[i].IsAllocated)
                            {
                                // 从固定内存读取更新后的值  
                                args[i] = Marshal.PtrToStructure(handles[i].AddrOfPinnedObject(), argType);
                            }
                        }
                    }
                }

                // 处理返回值  
                if (returnType != null && returnType != typeof(void))
                {
                    return IL2CPP.PointerToValueGeneric<T>(result, false, false);
                }

                return default;
            }
            finally
            {
                // 清理内存  
                if (paramBuffers != null)
                {
                    foreach (var buffer in paramBuffers)
                    {
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(buffer);
                    }
                }

                if (handles != null)
                {
                    foreach (var handle in handles)
                    {
                        if (handle.IsAllocated)
                            handle.Free();
                    }
                }

                if (paramsArray != null)
                {
                    Marshal.FreeHGlobal((IntPtr)paramsArray);
                }
            }
        }

        #endregion

        #region 扩展
        public static string MakeRef(this string str) => str + '&'; // 添加ref（地址）标记
        public static string MakeOut(this string str) => str + '&'; // 添加out（地址）标记
        #endregion
    }
}
