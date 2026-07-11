using BepInEx.Logging;
using BepInEx.Preloader.Core.Patching;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MapValue.BepInEx
{
    [PatcherPluginInfo("salmon.mapvalue", "MapValue", "1.0.0")]
    public class Core : BasePatcher
    {
        public static Dictionary<IntPtr, (IntPtr, int)> ResetValues = new ();
        public static ManualLogSource Logger = null!;

        public override void Initialize()
        {
            Logger = Log;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogMessage("MapValue has loaded");
            base.Initialize();
        }

        public static void SetMap(IntPtr key, IntPtr value, int size)
        {
            if (ResetValues.ContainsKey(key)) ResetValues[key] = (value, size);
            else ResetValues.Add(key, (value, size));
        }

        public static (IntPtr, int) GetMap(IntPtr key)
        {
            if (!ResetValues.ContainsKey(key)) return (IntPtr.Zero, -1);
            return ResetValues[key];
        }

        public static void RemoveMap(IntPtr key)
        {
            if (ResetValues.ContainsKey(key))
                ResetValues.Remove(key);
        }
    }

    [HarmonyPatch(typeof(IL2CPP))]
    public static class IL2CPPPatch
    {
        [HarmonyPatch(nameof(IL2CPP.il2cpp_field_static_get_value))]
        [HarmonyPrefix]
        public unsafe static bool Pre_il2cpp_field_static_get_value(ref IntPtr field, ref void* value)
        {
            var data = Core.GetMap(field);
            if (data.Item1 != IntPtr.Zero && data.Item2 != -1)
            {
                Buffer.MemoryCopy((void*)data.Item1, value, data.Item2, data.Item2);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(IL2CPP.il2cpp_field_get_value))]
        [HarmonyPrefix]
        public unsafe static bool Pre_il2cpp_field_get_value(ref IntPtr field, ref void* value)
        {
            var data = Core.GetMap(field);
            if (data.Item1 != IntPtr.Zero && data.Item2 != -1)
            {
                Buffer.MemoryCopy((void*)data.Item1, value, data.Item2, data.Item2);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(IL2CPP.il2cpp_class_get_field_from_name))]
        [HarmonyPrefix]
        public static unsafe void Pre_il2cpp_class_get_field_from_name(ref IntPtr __result)
        {
            var data = Core.GetMap(__result);
            if (data.Item1 != IntPtr.Zero && data.Item2 != -1)
            {
                var fieldFlags = IL2CPP.il2cpp_field_get_flags(__result);
                if ((fieldFlags & 0x10) != 0) // 如果是静态字段 FIELD_ATTRIBUTE_STATIC = 0x10
                {
                    IL2CPP.il2cpp_field_static_set_value(__result, (void*)data.Item1);
                }
            }
        }
    }
}
