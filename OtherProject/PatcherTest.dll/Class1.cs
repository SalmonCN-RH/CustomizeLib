using BepInEx.Logging;
using BepInEx.Preloader.Core.Patching;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PatcherTest.dll
{
    [PatcherPluginInfo("salmon.patchertest", "PatcherTest", "1.0.0")]
    public class Core : BasePatcher
    {
        public static Dictionary<IntPtr, IntPtr> OverrideDic = new(); // 字段指针 新值指针

        public override void Initialize()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            base.Initialize();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(IL2CPP))]
    public static class IL2CPPPatch
    {
        private static ManualLogSource logger = Logger.CreateLogSource("patchertest");

        [HarmonyPatch(nameof(IL2CPP.il2cpp_field_static_get_value))]
        [HarmonyPostfix]
        public unsafe static void Post_il2cpp_field_static_get_value(ref IntPtr field, ref void* value)
        {
            logger.LogMessage($"field ptr = {field}");
            if (Core.OverrideDic.TryGetValue(field, out var val))
                value = (void*)val;
        }
    }
}
