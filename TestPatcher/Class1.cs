using BepInEx.Preloader.Core.Patching;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;
using System.Runtime.InteropServices;

namespace TestPatcher
{
    public enum TestEnum
    {
        Test1,
        Test2,
        Test3
    }

    [PatcherPluginInfo("salmon.testpatcher", "TestPatcher", "1.0.0")]
    public class Core : BasePatcher
    {
        public unsafe override void Initialize()
        {
            base.Initialize();
            EnumInjector.RegisterEnumInIl2Cpp<TestEnum>();
            // 获取注入后的类型指针  
            var enumClassPtr = Il2CppClassPointerStore.GetNativeClassPointer(typeof(TestEnum));
            var il2CppType = Il2CppType.TypeFromPointer(enumClassPtr);

            // 使用il2CppType进行Enum.Parse  
            Console.WriteLine((int)Il2CppSystem.Enum.Parse(il2CppType, "Test3").Unbox<TestEnum>());
        }
    }
}
