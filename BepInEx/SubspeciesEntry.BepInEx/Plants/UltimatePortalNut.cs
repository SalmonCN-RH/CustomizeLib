using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SubspeciesEntry.BepInEx.Plants
{
    #region 金蛋
    [HarmonyPatch(typeof(UltimatePortalNut))]
    public static class UltimatePortalNutPatch
    {
        [HarmonyPatch(nameof(UltimatePortalNut.Revive))]
        [HarmonyPostfix]
        public static void PostRevive(UltimatePortalNut __instance)
        {
            if (__instance == null) return;

            if (CoreTools.TravelUltimate("万劫不复"))
            {
                __instance.TryBeActive();
                if (GameAPP.Instance.GetComponent<DelayAction>() == null) return;
                if (GameAPP.Instance.GetComponent<DelayAction>().actions == null) return;
                for (int i = GameAPP.Instance.GetComponent<DelayAction>().actions.Count - 1; i >= 0; i--)
                {
                    if (GameAPP.Instance.GetComponent<DelayAction>().actions.Count <= 0 || i < 0)
                        break;
                    if (i >= GameAPP.Instance.GetComponent<DelayAction>().actions.Count)
                        continue;
                    var action = GameAPP.Instance.GetComponent<DelayAction>().actions[i];
                    if (action == null) continue;
                    var target = action.action.Target.TryCast<Bullet_doom_ulti.__c__DisplayClass3_0>();
                    if (target != null && target.plant == __instance)
                    {
                        action.active = false;
                        action.action = null;
                    }
                }
                __instance.RemoveBuff(EffectType.Curse);
                if (Lawnf.GetAllZombies().Count > 0)
                {
                    foreach (Zombie zombie in Lawnf.GetAllZombies())
                    {
                        if (zombie != null)
                        {
                            if (zombie.theZombieType == ZombieType.SuperLadderZombie)
                            {
                                var ladder = zombie.transform.GetComponent<SuperLadderZombie>();
                                if (ladder != null && ladder.ladder != null && ladder.ladder.theItemRow == __instance.thePlantRow && 
                                    ladder.ladder.theItemColumn == __instance.thePlantColumn)
                                {
                                    ladder.ladder.Die();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class Plant_UltimatePortalNut_Patch
    {
        [HarmonyPatch(nameof(Plant.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(Plant __instance)
        {
            if (__instance.thePlantType == PlantType.UltimatePortalNut)
            {
                __instance.attributeCountdown = 30f;
            }
        }

        [HarmonyPatch(nameof(Plant.Update))]
        [HarmonyPostfix]
        public static void PostUpdate(Plant __instance)
        {
            if (__instance.thePlantType == PlantType.UltimatePortalNut)
            {
                if (__instance.attributeCountdown - Time.deltaTime <= 0f)
                {
                    if (CoreTools.TravelUltimate("无尽贪婪"))
                    {
                        __instance.GetComponent<UltimatePortalNut>().Revive();
                        __instance.thePlantHealth += 2000;
                        __instance.thePlantHealth = Mathf.Min(__instance.thePlantMaxHealth, __instance.thePlantHealth);
                        __instance.UpdateText();
                    }
                    __instance.attributeCountdown = 30f;
                }
            }
        }
    }
    #endregion
}
