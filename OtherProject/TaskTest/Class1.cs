using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.OldVersion.MethodInvoke;
using CustomizeLib.BepInEx.UnmanagedTools;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace TaskTest
{
    [BepInPlugin("salmon.test.tasktest", "APlantTest", "2.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<MyFirstPlant>();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

    public class MyFirstPlant : Plant
    {
        public MyFirstPlant(IntPtr ptr) : base(ptr) { }

        public MyFirstPlant() : base(ClassInjector.DerivedConstructorPointer<MyFirstPlant>()) => ClassInjector.DerivedConstructorBody(this);

        public override void Awake()
        {
            gameObject.GetComponents<PeaShooter>().First(shooter => shooter != this).CopyFieldAndPropTo(this, null, new List<string> { "m_CachedPtr" });
            // ReverseMethod.Plant_Awake(Cast<Plant>()); // base调用
            Debug.Log("try delete");
            MethodInvoke.InvokeBase(MethodInvoke.GetBaseMethodPtr(typeof(Plant), nameof(Plant.Awake), typeof(void), new Type[] { }), this);
            gameObject.GetComponents<PeaShooter>().First(shooter => shooter != this).enabled = false; // 禁用原组件
            Debug.Log("delete done");
        }

        //public unsafe void CallAwake()
        //{
        //    var ptr = // IL2CPP.il2cpp_class_get_method_from_name(Il2CppClassPointerStore.GetNativeClassPointer(typeof(Plant)), "Awake", 0);
        //              IL2CPP.GetIl2CppMethod(Il2CppClassPointerStore.GetNativeClassPointer(typeof(Plant)), false, "Awake", 
        //              IL2CPP.RenderTypeName(typeof(void)), Array.Empty<string>());
        //    Debug.Log($"ptr = {ptr}");
        //    void** paramsArray = null;

        //    // 异常指针  
        //    var exc = IntPtr.Zero;

        //    // 调用基类方法  
        //    IntPtr result = IL2CPP.il2cpp_runtime_invoke(
        //        ptr,  // 方法指针  
        //        Pointer,              // this 指针  
        //        paramsArray,               // 参数数组  
        //        ref exc                    // 异常指针  
        //    );
        //    Debug.Log($"excptr = {exc}");
        //    // 检查异常  
        //    if (exc != IntPtr.Zero)
        //    {
        //        Il2CppException.RaiseExceptionIfNecessary(exc);
        //    }
        //}
    }

    [HarmonyPatch]
    public static class ReverseMethod
    {
        [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
        [HarmonyReversePatch]
        public static void Plant_Awake(object instance) => throw new NotImplementedException();
    }
}
