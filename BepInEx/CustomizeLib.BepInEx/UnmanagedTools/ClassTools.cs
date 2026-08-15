using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.UnmanagedTools
{
    public static class ClassTools
    {
        public const string Void = "System.Void";

        public static unsafe IntPtr Call(IntPtr method, IntPtr obj, params Il2CppMethodArg[] args)
        {
            void** param = null;
            var handles = new GCHandle[args.Length];
            try
            {
                var references = new List<int>();
                if (args != null && args.Length > 0)
                {
                    param = (void**)Marshal.AllocHGlobal(args.Length * IntPtr.Size);
                    for (int i = 0; i < args.Length; ++i)
                    {
                        param[i] = (void*)PinObject(args[i].value, out var handle);
                        if (handle != null) handles[i] = handle.Value;
                        if (args[i].reference) references.Add(i);
                    }
                }

                var exec = IntPtr.Zero;
                var result = IL2CPP.il2cpp_runtime_invoke(method, obj, param, ref exec);

                if (args != null && args.Length > 0)
                {
                    foreach (var idx in references)
                    {
                        if (idx >= args.Length) continue;
                        var type = args[idx].GetType();
                        if (type.IsValueType && handles.Length < idx && handles[idx].IsAllocated)
                            args[idx].value = Marshal.PtrToStructure(handles[idx].AddrOfPinnedObject(), type);
                        else if (type == typeof(string))
                            args[idx].value = IL2CPP.Il2CppStringToManaged((IntPtr)param[idx]);
                        else if (typeof(Il2CppObjectBase).IsAssignableFrom(type))
                            args[idx].value = IL2CPP.PointerToValueGeneric<object>((IntPtr)param[idx], false, false);
                    }
                }

                if (exec != IntPtr.Zero) Il2CppException.RaiseExceptionIfNecessary(exec);
                return result;
            }
            finally
            {
                if (param != null)
                    Marshal.FreeHGlobal((IntPtr)param);
                foreach (var handle in handles)
                    if (handle.IsAllocated)
                        handle.Free();
            }
        }

        private static IntPtr PinObject(object? obj, out GCHandle? handle)
        {
            handle = null;
            if (obj == null) 
                return IntPtr.Zero;

            var type = obj.GetType();
            if (type.IsValueType)
            {
                handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
                return handle.Value.AddrOfPinnedObject();
            }
            else if (type == typeof(string))
                return IL2CPP.ManagedStringToIl2Cpp((string)obj);
            else if (typeof(Il2CppObjectBase).IsAssignableFrom(type))
                return IL2CPP.Il2CppObjectBaseToPtr((Il2CppObjectBase)obj);

            return IntPtr.Zero;
        }

        private static object UnboxPtrToObject(IntPtr ptr)
        {

            return null;
        }
    }

    public struct Il2CppMethodArg
    {
        public Il2CppMethodArg(object? value, bool reference = false)
        {
            this.value = value;
            this.reference = reference;
        }

        public object? value = default;
        public bool reference = false;
    }

    public static class Il2CppMethodArgExt
    {
        public static Il2CppMethodArg GetIl2CppArg(this object? obj, bool reference = false) => new(obj, reference);
    }
}
