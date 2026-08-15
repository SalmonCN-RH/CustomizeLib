using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace PuffUltimateChomper.BepInEx
{
    [BepInPlugin("salmon.puffultimatechomper", "PuffUltimateChomper", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Tools.GetAssembly(), null);
            ClassInjector.RegisterTypeInIl2Cpp<PuffUltimateChomper>();
            AssetBundle ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "puffultimatechomper");
            CustomCore.RegisterCustomPlant<UltimateChomper, PuffUltimateChomper>(PuffUltimateChomper.PlantID, ab.GetAsset<GameObject>("PuffUltimateChomperPrefab"),
                ab.GetAsset<GameObject>("PuffUltimateChomperPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SmallPuff, (int)PlantType.UltimateChomper)
                }, 1.75f, 0f, 1000, 6000, 7.5f, 400);
            CustomCore.TypeMgrExtra.IsNut.Add(PuffUltimateChomper.PlantID);
            CustomCore.TypeMgrExtra.IsPuff.Add(PuffUltimateChomper.PlantID);
            CustomCore.AddPlantAlmanacStrings(PuffUltimateChomper.PlantID, $"究极小樱桃战神",
                    "攻防一体，成群结队的迷你樱桃战神\n\n" +
                    "<color=#3D1400>贴图作者：@屑红leong、@林秋-AutumnLin</color>\n" +
                    "<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+究极樱桃战神</color>\n" +
                    "<color=#3D1400>韧性：</color><color=red>6000，限伤1/3韧性</color>\n" +
                    "<color=#3D1400>伤害：</color><color=red>1000/1.75秒</color>\n" +
                    "<color=#3D1400>特性：</color><color=red>低矮，可密植</color>\n" +
                    "<color=#3D1400>特点：</color><color=red>①免疫碾压并击退\n" +
                    "②血量最高为4倍韧性，超过韧性的血量以（10%韧性/1秒）的速率流失\n" +
                    "③免疫樱桃爆炸\n" +
                    "④啃咬前方1格内所有僵尸，附加僵尸韧性的（1.5+0.2x吞噬层数）%的伤害，之后回复等于造成伤害的血量，吐出200的迷你爆炸樱桃\n" +
                    "⑤吞噬能吞噬非领袖僵尸和巨型僵尸并恢复4倍韧性血量，获得1点吞噬层数，该效果触发后，需要消化40秒\n" +
                    "⑦不死机制：收到致命伤害时触发，5秒内血量最多因受伤而降为1，该效果有10秒冷却，出场时进入冷却</color>\n" +
                    "<color=#3D1400>词条1：</color><color=red>嗜血如命：回复量x3</color>\n" +
                    "<color=#3D1400>词条2：</color><color=red>极速吞噬：吞食消化时间变为15秒</color>\n" +
                    "<color=#3D1400>连携词条:</color><color=red>我是传奇：究极樱桃战神（或其亚种）和究极黑曜石（或其亚种）同时在场时：究极小樱桃战神血量无上限</color>\n\n" +
                    "<color=#3D1400>“樱桃！菠萝苹果！”究极小樱桃战神说话咿咿呀呀，充满着童心。因为他独特的表达方式，植物们只能从特定动作来猜测它表达的意思，他看到自己喜欢的东西就会开心的说“橘子！橘子！”，考试出成绩他也会举着卷子说“葡萄！葡萄！”，或许是在说自己聪明吧～</color>");
            TypeMgr.UncrashablePlants.Add(PuffUltimateChomper.PlantID);
        }
    }

    public class PuffUltimateChomper : MonoBehaviour
    {
        public static ID PlantID = 1959;

        public UltimateChomper plant => gameObject.GetComponent<UltimateChomper>();

        public void Awake()
        {
            if (plant == null) return;
            plant.isShort = true;
            plant.uncrashable = true;
            plant.shoot = transform.FindChild("PuffCherrychomper/Shoot");
        }

        public void Update()
        {
            if (plant == null) return;
            if (plant.targetZombie == null && GameAPP.theGameStatus == GameStatus.InGame)
                plant.ChomperSearchZombie();
        }
    }

    [HarmonyPatch(typeof(BombCherry), nameof(BombCherry.PlantTakeDamage))]
    public static class BombCherryTakeDamagePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Plant plant)
        {
            if (plant.thePlantType == PuffUltimateChomper.PlantID)
            {
                plant.Recover(200f);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Lawnf), nameof(Lawnf.GetPlantCount), new Type[] { typeof(PlantType), typeof(Board) })]
    public static class LawnfGetCountPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlantType theSeedType, Board board, ref int __result)
        {
            if (theSeedType == PlantType.UltimateChomper)
                __result += Lawnf.GetPlantCount(PuffUltimateChomper.PlantID, board);
        }
    }

    [HarmonyPatch(typeof(UltimateChomper))]
    public static class UltimateChomperPatch
    {
        [HarmonyPatch(nameof(UltimateChomper.Bite))]
        [HarmonyPrefix]
        public static bool PreBite(UltimateChomper __instance, ref Zombie zombie)
        {
            if (__instance.thePlantType == PuffUltimateChomper.PlantID)
            {
                __instance.anim.SetBool("bite", false);
                if (zombie == null)
                    return false;

                var zombieType = zombie.theZombieType;
                if (__instance.attributeCountdown == 0.0f && !TypeMgr.IsBossZombie(zombieType) && !TypeMgr.IsGargantuar(zombieType))
                {
                    zombie.Die(2);

                    if (Lawnf.TravelUltimate((UltiBuff)1))
                        __instance.attributeCountdown = 15.0f;
                    else
                        __instance.attributeCountdown = 40.0f;

                    if (__instance.attributeCount < 485)
                        __instance.attributeCount++;

                    __instance.Recover(__instance.thePlantMaxHealth);

                    __instance.chomperUndead = true;
                    __instance.undeadTimer = 0;

                    GameAPP.PlaySound(49, 0.5f, 1.0f);

                    if (__instance.starUp && __instance.anim != null)
                        __instance.anim.SetTrigger("supershoot");
                }
                else
                {
                    float totalDamage =
                        __instance.attackDamage +
                        (__instance.attackDamage * 0.001f) * zombie.TotalFirstHealth *
                         (1.5f + __instance.attributeCount * 0.2f) * 0.01f;

                    if (Lawnf.TravelAdvanced(CoreTools.GetAdvBuffByString("Rogue_究极樱桃战神专精II")))
                        totalDamage *= 1.5f;

                    zombie.TakeDamage(DmgType.NormalAll, (int)totalDamage, __instance.thePlantType);

                    __instance.Recover(__instance.thePlantMaxHealth);
                    GameAPP.PlaySound(49, 0.5f, 1.0f);
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(UltimateChomper.OnAfterInitText))]
        [HarmonyPostfix]
        public static void PostOnAfterInitText(UltimateChomper __instance)
        {
            if (__instance.thePlantType == PuffUltimateChomper.PlantID)
            {
                foreach (var kvp in __instance.healthSlider.registedTexts)
                    UnityEngine.Object.Destroy(kvp.Key.gameObject);
                __instance.healthSlider.registedTexts = new();
            }
        }
    }

    [HarmonyPatch(typeof(Chomper), nameof(Chomper.Start))]
    public static class ChomperStartPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Chomper __instance)
        {
            if (__instance.thePlantType == PuffUltimateChomper.PlantID)
            {
                __instance._ChompRange_k__BackingField = 2f;
            }
        }
    }

    [HarmonyPatch(typeof(SuperChomper), nameof(SuperChomper.ReplaceSprite))]
    public static class SuperChomperSpritePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(SuperChomper __instance)
        {
            if (__instance.thePlantType == PuffUltimateChomper.PlantID)
            {
                if (__instance.thePlantHealth > __instance.thePlantMaxHealth * 2 / 3)
                {
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth1").gameObject.SetActive(true);
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth2").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth3").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body1").gameObject.SetActive(true);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body2").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body3").gameObject.SetActive(false);
                }
                else if (__instance.thePlantHealth > __instance.thePlantMaxHealth / 3 && __instance.thePlantHealth <= __instance.thePlantMaxHealth * 2 /3)
                {
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth1").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth2").gameObject.SetActive(true);
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth3").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body1").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body2").gameObject.SetActive(true);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body3").gameObject.SetActive(false);
                }
                else if (__instance.thePlantHealth <= __instance.thePlantMaxHealth / 3)
                {
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth1").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth2").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Head/face/mouth3").gameObject.SetActive(true);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body1").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body2").gameObject.SetActive(false);
                    __instance.transform.FindChild("PuffCherrychomper/Body/Body3").gameObject.SetActive(true);
                }
            }
            return true;
        }
    }
}
