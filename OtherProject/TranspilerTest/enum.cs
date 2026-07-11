using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime.Startup;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TranspilerTest
{
    public static unsafe class EnumInjectorFix
    {
        // ---- 反射句柄（EnumInjector / InjectorHelpers 的 internal / private 成员）----
        private static readonly Type EnumInjectorType = typeof(EnumInjector);

        // private static IntPtr CreateOrUpdateFieldDefaultValue(Il2CppFieldInfo*, Il2CppTypeStruct*, object)
        private static readonly MethodInfo CreateOrUpdateFieldDefaultValueMI =
            EnumInjectorType.GetMethod("CreateOrUpdateFieldDefaultValue",
                BindingFlags.NonPublic | BindingFlags.Static);

        // private static ConcurrentDictionary<IntPtr, IntPtr> s_DefaultValueOverrides
        private static readonly ConcurrentDictionary<IntPtr, IntPtr> DefaultValueOverrides =
            (ConcurrentDictionary<IntPtr, IntPtr>)EnumInjectorType
                .GetField("s_DefaultValueOverrides", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);

        // internal static class InjectorHelpers
        private static readonly Type InjectorHelpersType =
            EnumInjectorType.Assembly.GetType("Il2CppInterop.Runtime.Injection.InjectorHelpers");

        // internal static void Setup()
        private static readonly MethodInfo SetupMI =
            InjectorHelpersType.GetMethod("Setup", BindingFlags.NonPublic | BindingFlags.Static);

        // internal static d_ClassInit ClassInit  (void(Il2CppClass*))
        private static readonly FieldInfo ClassInitFI =
            InjectorHelpersType.GetField("ClassInit", BindingFlags.NonPublic | BindingFlags.Static);

        // 指针参数类型（反射装箱用）
        private static readonly Type Il2CppFieldInfoPtr = typeof(Il2CppFieldInfo).MakePointerType();
        private static readonly Type Il2CppTypeStructPtr = typeof(Il2CppTypeStruct).MakePointerType();
        private static readonly Type Il2CppClassPtr = typeof(Il2CppClass).MakePointerType();

        public static void FixInjectEnumValues<TEnum>(Dictionary<string, object> valuesToAdd) where TEnum : Enum
            => FixInjectEnumValues(typeof(TEnum), valuesToAdd);

        public static void FixInjectEnumValues(Type type, Dictionary<string, object> valuesToAdd)
        {
            if (type == null) throw new ArgumentException("Type argument cannot be null");
            if (!type.IsEnum) throw new ArgumentException("Type argument needs to be an enum");

            var enumPtr = Il2CppClassPointerStore.GetNativeClassPointer(type);
            if (enumPtr == IntPtr.Zero) throw new ArgumentException("Type needs to be an Il2Cpp enum");

            // InjectorHelpers.Setup();  —— 确保 GetFieldDefaultValue hook 已挂上
            SetupMI.Invoke(null, null);

            // InjectorHelpers.ClassInit((Il2CppClass*)enumPtr);
            var classInit = (Delegate)ClassInitFI.GetValue(null);
            classInit.DynamicInvoke(Pointer.Box((void*)enumPtr, Il2CppClassPtr));

            var il2cppEnum = UnityVersionHandler.Wrap((Il2CppClass*)enumPtr);
            var newFieldCount = il2cppEnum.FieldCount + valuesToAdd.Count;
            var newFields = (Il2CppFieldInfo*)Marshal.AllocHGlobal(newFieldCount * UnityVersionHandler.FieldInfoSize());

            // 1) 拷贝原有字段，并迁移已登记的 default value override
            int fieldIdx;
            for (fieldIdx = 0; fieldIdx < il2cppEnum.FieldCount; ++fieldIdx)
            {
                var offset = fieldIdx * UnityVersionHandler.FieldInfoSize();
                var oldField = UnityVersionHandler.Wrap(il2cppEnum.Fields + offset);
                var newField = UnityVersionHandler.Wrap(newFields + offset);

                newField.Name = oldField.Name;
                newField.Type = oldField.Type;
                newField.Parent = oldField.Parent;
                newField.Offset = oldField.Offset;

                if (DefaultValueOverrides.TryRemove((IntPtr)oldField.FieldInfoPointer, out var blob))
                    DefaultValueOverrides[(IntPtr)newField.FieldInfoPointer] = blob;
            }

            // blob 宽度仍按底层元素类型
            var enumElementType = UnityVersionHandler.Wrap(il2cppEnum.ElementClass).ByValArg;

            // 复用一个已有【枚举常量成员】(literal) 的 Type 指针
            Il2CppTypeStruct* memberTypePtr = null;
            for (int i = 0; i < il2cppEnum.FieldCount; i++)   // 此时 FieldCount 还是原始值
            {
                var f = UnityVersionHandler.Wrap(newFields + i * UnityVersionHandler.FieldInfoSize());
                if ((UnityVersionHandler.Wrap(f.Type).Attrs & (ushort)System.Reflection.FieldAttributes.Literal) != 0)
                { memberTypePtr = f.Type; break; }
            }
            if (memberTypePtr == null) throw new InvalidOperationException("no existing literal member to clone");

            foreach (var newData in valuesToAdd)
            {
                var newField = UnityVersionHandler.Wrap(newFields + fieldIdx * UnityVersionHandler.FieldInfoSize());
                newField.Name = Marshal.StringToCoTaskMemUTF8(newData.Key);
                newField.Type = memberTypePtr;          // ← 带 LITERAL 的成员类型（不是裸 int）
                newField.Parent = il2cppEnum.ClassPointer;
                newField.Offset = 0;
                CreateOrUpdateFieldDefaultValueMI.Invoke(null, new object[] {
        Pointer.Box((void*)newField.FieldInfoPointer,  Il2CppFieldInfoPtr),
        Pointer.Box((void*)enumElementType.TypePointer, Il2CppTypeStructPtr),  // blob 仍按底层 int
        newData.Value });
                ++fieldIdx;
            }

            il2cppEnum.FieldCount = (ushort)newFieldCount;
            il2cppEnum.Fields = newFields;

            // 清掉 mono 对枚举名/值的缓存，否则新值不会出现在反射结果里
            var runtimeEnumType = Il2CppType.TypeFromPointer(enumPtr).TryCast<Il2CppSystem.RuntimeType>();
            if (runtimeEnumType != null)
                runtimeEnumType.GenericCache = null;
        }
    }

    public static unsafe class GetFieldDefaultValueRedirect
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate byte* MethodDelegate(Il2CppFieldInfo* field, out Il2CppTypeStruct* type);

        static MethodDelegate _detour, _original;
        static IDetour _idetour;

        static readonly ConcurrentDictionary<IntPtr, IntPtr> Overrides =
            (ConcurrentDictionary<IntPtr, IntPtr>)typeof(EnumInjector)
                .GetField("s_DefaultValueOverrides", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null)!;

        public static IEnumerable<IntPtr> DumpKeys() => Overrides.Keys;

        public static void Install()
        {
            typeof(EnumInjector).Assembly
                .GetType("Il2CppInterop.Runtime.Injection.InjectorHelpers")!
                .GetMethod("Setup", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, null);

            var il2CppModule = Process.GetCurrentProcess()
                .Modules.OfType<ProcessModule>()
                .Single(x => x.ModuleName is "GameAssembly.dll" or "GameAssembly.so" or "UserAssembly.dll");

            IntPtr target = IntPtr.Zero;

            // Skip signature scan and reflection - use known working offset  
            Console.WriteLine("[MyDetour] Using hardcoded offset for Unity 2022.3.62f1c1");
            target = il2CppModule.BaseAddress + 0x32C9E0;

            if (target == IntPtr.Zero)
            {
                throw new Exception("Failed to find Class::GetDefaultFieldValue function");
            }

            Console.WriteLine($"[MyDetour] target=0x{target.ToInt64():X}");

            _detour = Detour;
            _idetour = Il2CppInteropRuntime.Instance.DetourProvider.Create<MethodDelegate>(target, _detour);
            _original = _idetour.GenerateTrampoline<MethodDelegate>();
            _idetour.Apply();
            Console.WriteLine("[MyDetour] installed");
        }

        static IntPtr FindTargetViaReflection()
        {
            try
            {
                // 获取 Class_GetFieldDefaultValue_Hook 类型  
                var hookType = typeof(EnumInjector).Assembly
                    .GetType("Il2CppInterop.Runtime.Injection.Hooks.Class_GetFieldDefaultValue_Hook");

                if (hookType == null) return IntPtr.Zero;

                // 创建实例  
                var hookInstance = Activator.CreateInstance(hookType, nonPublic: true);

                // 调用 FindTargetMethod  
                var findTargetMethod = hookType.GetMethod("FindTargetMethod",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (findTargetMethod == null) return IntPtr.Zero;

                return (IntPtr)findTargetMethod.Invoke(hookInstance, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MyDetour] Reflection XRef scan error: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        static unsafe IntPtr FindSignature(IntPtr baseAddr, int size, string pattern, string mask)
        {
            var patternBytes = pattern.ToCharArray();
            var maskBytes = mask.ToCharArray();

            for (long address = 0; address < size; address++)
            {
                var found = true;
                for (uint offset = 0; offset < maskBytes.Length; offset++)
                    if (*(byte*)(address + (long)baseAddr + offset) != (byte)patternBytes[offset] && maskBytes[offset] != '?')
                    {
                        found = false;
                        break;
                    }

                if (found)
                    return (IntPtr)(address + (long)baseAddr);
            }

            return IntPtr.Zero;
        }

        static byte* Detour(Il2CppFieldInfo* field, out Il2CppTypeStruct* type)
        {
            if (Overrides.TryGetValue((IntPtr)field, out var blob))
            {
                var wf = UnityVersionHandler.Wrap(field);
                var parent = UnityVersionHandler.Wrap(wf.Parent);
                var elem = UnityVersionHandler.Wrap(parent.ElementClass);
                type = elem.ByValArg.TypePointer;
                Console.WriteLine($"[MyDetour] HIT field=0x{((IntPtr)field).ToInt64():X} *blob={*(int*)blob}");
                return (byte*)blob;
            }
            return _original(field, out type);
        }
    }
}
