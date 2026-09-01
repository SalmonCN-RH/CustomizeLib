using CustomizeLib.BepInEx.Utility;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent
{
    [HarmonyPatch(typeof(Mouse))]
    [HarmonyPriority(Priority.First)] // 数值越大执行顺序越靠后
    public static class MousePatch
    {
        private static ResetSig sig = false;

        [HarmonyPatch(nameof(Mouse.LeftClickWithNothing))]
        [HarmonyPrefix]
        public static bool PreLeftClickWithNothing(Mouse __instance)
        {
            var block = false;
            var other = false;
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            foreach (var plant in __instance.GetPlantsOnMouse(Physics2D.RaycastAll(pos, Vector2.zero)))
            {
                if (plant == null) continue;
                var (res, success) = PlantEvent.OnClicked(plant, __instance, other, TriggerType.Pre);
                block |= res;
                other |= success;
            }
            return !block;
        }

        [HarmonyPatch(nameof(Mouse.LeftClickWithNothing))]
        [HarmonyPostfix]
        public static void PostLeftClickWithNothing(Mouse __instance)
        {
            var other = false;
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            foreach (var plant in __instance.GetPlantsOnMouse(Physics2D.RaycastAll(pos, Vector2.zero)))
            {
                if (plant == null) continue;
                var (_, success) = PlantEvent.OnClicked(plant, __instance, other, TriggerType.Post);
                other |= success;
            }
        }

        [HarmonyPatch(nameof(Mouse.LeftClickWithSomeThing))]
        [HarmonyPrefix]
        public static bool PreLeftClickWithSomeThing(Mouse __instance, out Plant? __state)
        {
            var block = false;
            if (__instance.theItemOnMouse != null && __instance.theItemOnMouse.name == "cannon")
            {
                if (__instance.mouseX > -6.5f && __instance.cannonPlant != null)
                {
                    block |= PlantEvent.SetTargetByMouse(__instance.cannonPlant, __instance, TriggerType.Pre);
                    if (PlantEvent.GetCachedCompCount(__instance.cannonPlant!) > 0)
                        if (!block) sig.Set();
                }
            }
            __state = __instance.cannonPlant;
            return !block;
        }

        [HarmonyPatch(nameof(Mouse.LeftClickWithSomeThing))]
        [HarmonyPostfix]
        public static void PostLeftClickWithSomeThing(Mouse __instance, Plant? __state)
        {
            if (__instance.theItemOnMouse != null && __instance.theItemOnMouse.name == "cannon")
            {
                if (__instance.mouseX > -6.5f && __state != null)
                    PlantEvent.SetTargetByMouse(__state, __instance, TriggerType.Post);
                __instance.ClearItemOnMouse(true);
            }
        }

        [HarmonyPatch(nameof(Mouse.ClearItemOnMouse))]
        [HarmonyPrefix]
        public static bool PreClearItemOnMouse()
        {
            if (sig.Reset())
                return false;
            return true;
        }

        [HarmonyPatch(nameof(Mouse.Awake))]
        [HarmonyPrefix]
        public static void PreAwake(Mouse __instance)
        {
            __instance.GetOrAddComponent<MouseBehaviour>()?.mouse = __instance;
        }

        [HarmonyPatch(nameof(Mouse.Update))]
        [HarmonyPrefix]
        public static void PreUpdate()
        {
            MouseBehaviour.Instance.ProcMouse(TriggerType.Pre);
        }

        [HarmonyPatch(nameof(Mouse.Update))]
        [HarmonyPostfix]
        public static void PostUpdate()
        {
            MouseBehaviour.Instance.ProcMouse(TriggerType.Post);
        }
    }
}
