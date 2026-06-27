using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    [HarmonyPatch(typeof(NutFume))]
    public static class NutFumePatch
    {
        [HarmonyPatch(nameof(NutFume.ReplaceSprite))]
        [HarmonyPrefix]
        public static void PreReplaceSprite(NutFume __instance)
        {
            if (SkinMgr.IsPlantSkinEnable(__instance.thePlantType) && __instance.changes.Count <= 0)
            {
                __instance.changes.Add(__instance.transform.FindChild("FumeShroom_head").gameObject);
                __instance.changes.Add(__instance.transform.FindChild("FumeShroom_body").gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_pea_bombCherry))]
    public static class Bullet_pea_bombCherryPatch
    {
        [HarmonyPatch(nameof(Bullet_pea_bombCherry.HitZombie))]
        [HarmonyPrefix]
        public static void PreHitZombie(Bullet_pea_threeCherry __instance)
        {
            if (Regex.IsMatch(__instance.gameObject.name, @"Bullet_(\d+)"))
            {
                __instance.bombPrefab = Resources.Load<GameObject>("items/timebomb/TimeBomb");
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_pea_threeCherry))]
    public static class Bullet_pea_threeCherryPatch
    {
        [HarmonyPatch(nameof(Bullet_pea_threeCherry.HitZombie))]
        [HarmonyPrefix]
        public static void PreHitZombie(Bullet_pea_threeCherry __instance)
        {
            if (Regex.IsMatch(__instance.gameObject.name, @"Bullet_(\d+)"))
            {
                __instance.bombPrefab = Resources.Load<GameObject>("items/timebomb/TimeBomb");
            }
        }
    }


    [HarmonyPatch(typeof(UltimateIceDoom))]
    public static class UltimateIceDoomPatch
    {
        [HarmonyPatch(nameof(UltimateIceDoom.Start))]
        [HarmonyPostfix]
        public static void PostStart(UltimateIceDoom __instance)
        {
            if (SkinMgr.IsPlantSkinEnable(__instance.thePlantType))
            {
                __instance.targetPrefab = Resources.Load<GameObject>("plants/doomshroom/ultimateicedoom/Target");
                __instance.portalIn = __instance.transform.FindChild("PortalIn").gameObject;
                __instance.portalOut = __instance.transform.FindChild("PortalOut").gameObject;
                foreach (var trans in  __instance.transform.GetComponentsInChildren<Transform>(true))
                    if (trans.gameObject.name.StartsWith("bodies_"))
                        __instance.bodies.Add(trans.gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(UltimateWinterMelon))]
    public static class UltimateWinterMelonPatch
    {
        [HarmonyPatch(nameof(UltimateWinterMelon.Start))]
        [HarmonyPrefix]
        public static void PreStart(UltimateWinterMelon __instance)
        {
            if (SkinMgr.IsPlantSkinEnable(__instance.thePlantType))
            {
                __instance.stage2 = __instance.transform.FindChild("WinterMelon/2").gameObject;
                __instance.stage3_1 = __instance.transform.FindChild("WinterMelon_melon/3_1").gameObject;
                __instance.stage3_2 = __instance.transform.FindChild("WinterMelon_melon/3_2").gameObject;
                __instance.AddComponent<UltimateWinterMelonSkin>().plant = __instance;
            }
        }
    }
}
