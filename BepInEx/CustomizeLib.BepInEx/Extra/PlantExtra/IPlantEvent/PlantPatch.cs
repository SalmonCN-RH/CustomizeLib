using CustomizeLib.BepInEx.Hook;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;
using UnityEngine;

namespace CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent
{
    #region HarmonyPatch
    public static class PlantPatches
    {
        private const string UPDATE = "Plant_Update";
        private const string FIXEDUPDATE = "Plant_FixedUpdate";

        // 神秘il2cpp，只能有一个调用的async方法，多了就崩
        // 万能方法
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static async Task LocalMethod(Plant plant, TriggerType trigger, string callData)
        {
            switch (callData)
            {
                case UPDATE:
                    {
                        if (plant != null && PlantEvent.HasEventComp(plant))
                            PlantEvent.OnUpdate(plant, trigger);
                    }
                    break;
                case FIXEDUPDATE:
                    {
                        if (plant != null && PlantEvent.HasEventComp(plant))
                            PlantEvent.OnFixedUpdate(plant, plant, trigger);
                    }
                    break;
            }
        }

        [HarmonyPatch]
        [HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
        public static class PlantDiePatch
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetTargetMethods()
            {
                return SystemTools.GetAllMethods(SystemTools.GetAllDerivedTypes(typeof(Plant)), nameof(Plant.Die),
                    BindingFlags.Default.AddAllAccess().AddInstance().AddDeclaredOnly());
            }

            [HarmonyPrefix]
            public static void PreDie(Plant __instance, Plant.DieReason __0)
            {
                if (__instance != null && PlantEvent.HasEventComp(__instance))
                    PlantEvent.DieEvent(__instance, __0, TriggerType.Pre);
            }

            [HarmonyPostfix]
            public static void PostDie(Plant __instance, Plant.DieReason __0)
            {
                if (__instance != null && PlantEvent.HasEventComp(__instance))
                    PlantEvent.DieEvent(__instance, __0, TriggerType.Post);
            }
        }

        [HarmonyPatch]
        [HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
        public static class Plant_PlantUpdatePatch
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetTargetMethods()
            {
                return SystemTools.GetAllMethods(SystemTools.GetAllDerivedTypes(typeof(Plant)), nameof(Plant.PlantUpdate),
                    BindingFlags.Default.AddAllAccess().AddInstance().AddDeclaredOnly());
            }

            [HarmonyPrefix]
            public static void PrePlantUpdate(Plant __instance, ref bool __state)
            {
                if (__instance != null && PlantEvent.HasEventComp(__instance))
                {
                    // OnUpdate
                    // _ = PlantEvent.Resolvers.PlantResolver.PreUpdate.Update(__instance);
                    //_ = PlantEvent.Resolvers.Run(() =>
                    //{
                    //    if (__instance != null && PlantEvent.HasEventComp(__instance))
                    //        PlantEvent.OnUpdate(__instance, TriggerType.Pre);
                    //});
                    // AttributeEvent
                    if (__instance.attributeCountdown > 0f && __instance.attributeCountdown - Time.deltaTime * __instance.attributeSpeed <= 0f)
                    {
                        __state = true;
                        PlantEvent.AttributeEvent(__instance, TriggerType.Pre);
                    }
                }
            }

            [HarmonyPostfix]
            public static void PostPlantUpdate(Plant __instance, bool __state)
            {
                if (__instance != null && PlantEvent.HasEventComp(__instance))
                {
                    // OnUpdate
                    // _ = PlantEvent.Resolvers.PlantResolver.PostUpdate.Update(__instance);
                    //_ = PlantEvent.Resolvers.Run(() =>
                    //{
                    //    if (__instance != null && PlantEvent.HasEventComp(__instance))
                    //        PlantEvent.OnUpdate(__instance, TriggerType.Post);
                    //});
                    // AttributeEvent
                    if (__state)
                    {
                        PlantEvent.AttributeEvent(__instance, TriggerType.Post);
                    }
                }
            }
        }

        [HarmonyPatch]
        [HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
        public static class Plant_UpdatePatch
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetTargetMethods()
            {
                return SystemTools.GetAllMethods(SystemTools.GetAllDerivedTypes<Plant>(), nameof(Plant.Update),
                    BindingFlags.Default.AddAllAccess().AddInstance().AddDeclaredOnly());
            }

            [HarmonyPrefix]
            public static void PreUpdate(Plant __instance)
            {
                _ = LocalMethod(__instance, TriggerType.Pre, UPDATE);
            }

            [HarmonyPostfix]
            public static void PostUpdate(Plant __instance)
            {
                _ = LocalMethod(__instance, TriggerType.Post, UPDATE);
            }
        }

        [HarmonyPatch]
        [HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
        public static class Plant_FixedUpdatePatch
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetTargetMethods()
            {
                return SystemTools.GetAllMethods(SystemTools.GetAllDerivedTypes<Plant>(), nameof(Plant.FixedUpdate),
                    BindingFlags.Default.AddAllAccess().AddInstance().AddDeclaredOnly());
            }

            [HarmonyPrefix]
            public static void PreFixedUpdate(Plant __instance)
            {
                _ = LocalMethod(__instance, TriggerType.Pre, FIXEDUPDATE);
            }

            [HarmonyPostfix]
            public static void PostFixedUpdate(Plant __instance)
            {
                _ = LocalMethod(__instance, TriggerType.Post, FIXEDUPDATE);
            }
        }
    }
    #endregion

    #region NativeHook
    //[ApplyNativeHook]
    //public class PlantUpdateHook
    //{
    //    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //    private delegate void PlantUpdate(IntPtr @this, IntPtr method);

    //    private static PlantUpdate Original = null!;

    //    public static void ApplyHook()
    //    {
    //        LibNativeHook.CreateAndApply(LibNativeHook.GetAndInitMethodAddr(typeof(Plant), "Update"), OnPlantUpdate, out Original);
    //    }

    //    public static void OnPlantUpdate(IntPtr @this, IntPtr method)
    //    {
    //        var plant = new Plant(@this);
    //        bool notNull = plant != null;
    //        if (notNull) PlantEvent.OnUpdate(plant!, TriggerType.Pre);
    //        Original.Invoke(@this, method);
    //        if (notNull) PlantEvent.OnUpdate(plant!, TriggerType.Post);
    //    }
    //}

    //[ApplyNativeHook]
    //public class PlantFixedUpdateHook
    //{
    //    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //    private delegate void PlantFixedUpdate(IntPtr @this, IntPtr method);

    //    private static PlantFixedUpdate Original = null!;

    //    public static void ApplyHook()
    //    {
    //        LibNativeHook.CreateAndApply(LibNativeHook.GetAndInitMethodAddr(typeof(Plant), nameof(Plant.FixedUpdate)), OnPlantFixedUpdate, out Original);
    //    }

    //    public static void OnPlantFixedUpdate(IntPtr @this, IntPtr method)
    //    {
    //        var plant = new Plant(@this);
    //        bool notNull = plant != null;
    //        if (notNull) PlantEvent.OnFixedUpdate(plant!, plant!, TriggerType.Pre);
    //        Original.Invoke(@this, method);
    //        if (notNull) PlantEvent.OnFixedUpdate(plant!, plant!, TriggerType.Post);
    //    }
    //}
    #endregion
}
