using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.Class;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HookTest
{
    [BepInPlugin("salmon.hooktest", "HookTest", "1.0.0")]
    public class Class1 : BasePlugin
    {
        public override unsafe void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool OnClicked(Plant instance, Mouse mouse)
        {
            Console.WriteLine($"{instance.thePlantType}");
            return true;
        }
    }

    [HarmonyPatch(typeof(GameAPP), nameof(GameAPP.Awake))]
    public static class AwakePatch
    {
        [HarmonyPostfix]
        public static void PostAwake()
        {
            unsafe
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                var clsStruct = UnityVersionHandler.Wrap((Il2CppClass*)Il2CppClassPointerStore<Plant>.NativeClassPtr);
                var vtable = (VirtualInvokeData*)clsStruct.VTable;
                var onclick = vtable[34];
                var strc = UnityVersionHandler.Wrap(onclick.method);
                Console.WriteLine($"{clsStruct.VtableCount}, 0x{(IntPtr)(&vtable[61]):X}, 0x{onclick.methodPtr:X}");
                Console.WriteLine($"{Marshal.PtrToStringUTF8(strc.Name)}");

                var method = typeof(ClassInjector).GetMethod("ConvertMethodInfo", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo methodInfo = typeof(Class1).GetMethod("OnClicked", BindingFlags.Static | BindingFlags.Public)!;
                INativeClassStruct ts = clsStruct;
                var result = method.Invoke(null, new object[] { method, ts });
            }
        }
    }
}
