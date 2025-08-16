using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using Unity.VisualScripting;
using CustomizeLib.BepInEx;

namespace NullNut.BepInEx
{
    [BepInPlugin("inf75.nullnut", "NullNut", "1.0")]
    public class Core : BasePlugin//960
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "nullnut");
            ClassInjector.RegisterTypeInIl2Cpp<NullNut>();
            CustomCore.RegisterCustomPlant<WallNut, NullNut>(NullNut.PlantID, ab.GetAsset<GameObject>("NullNutPrefab"),
                ab.GetAsset<GameObject>("NullNutPreview"), [], 0f, 0f, 0, 4000, 60f, 750);
            CustomCore.AddPlantAlmanacStrings(NullNut.PlantID, $"&$%?坚果({NullNut.PlantID})",
                "#$%^&@!@#$%?>*&^$#^!#$%@\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>韧性：</color><color=red>4000</color>\n<color=#3D1400>特点：</color><color=red>防碾压，每次受伤使啃咬自己的僵尸随机获得以下状态：红温状态、减速10秒、中毒10秒、不留坑的毁灭菇效果、余烬状态、金色状态，并且有20%的概率使该僵尸立刻死亡，每次受伤随机在0~50点范围</color>\n花费：<color=red>750</color>\n冷却时间：<color=red>60秒</color>\n<color=red>内。</color>\n\n<color=#3D1400>传说在融合版的初期，有个叫什么什么75的人制作了修改器和二创前置库，为后世的开发者们留下了一笔宝贵的财富，在融合1周年之际，他选择离开这片自己曾倾注心血的大地，而修改器和二创前置库则被后来的新生力量接力，延续着他的精神，这些新生力量们也同样相信，终有一天，待到他们也不得不离开自己亲手创造</color>\n花费：<color=red>750</color>\n冷却时间：<color=red>60秒</color>\n<color=#3D1400>的天地时，必有下一个盘古，延续这份力量……</color>\n\n\n\n\n\n\n\n\n\n花费：<color=red>750</color>\n冷却时间：<color=red>60秒</color>");
            CustomCore.RegisterCustomCardToColorfulCards((PlantType)NullNut.PlantID);
            CustomCore.TypeMgrExtra.IsNut.Add((PlantType)NullNut.PlantID);
        }
    }

    public class NullNut : MonoBehaviour
    {
        public static int PlantID = 1919;

        public NullNut() : base(ClassInjector.DerivedConstructorPointer<NullNut>()) => ClassInjector.DerivedConstructorBody(this);

        public NullNut(IntPtr i) : base(i)
        {
        }

        public WallNut plant => gameObject.GetComponent<WallNut>();
    }

    [HarmonyPatch(typeof(Zombie), nameof(Zombie.AttackPlant))]
    public class ZombieAttackPlantPatch
    {

        [HarmonyPatch(nameof(Zombie.PlayEatSound))]
        [HarmonyPostfix]
        public static void Postfix_(Zombie __instance)
        {
            if (__instance is not null && __instance.theAttackTarget is not null)
            {
                __instance.theAttackTarget.TryGetComponent<Plant>(out var plant);
                if (plant is not null && (int)plant.thePlantType == NullNut.PlantID)
                {
                    switch (UnityEngine.Random.Range(0, 6))
                    {
                        case 0:
                            __instance.SetJalaed();
                            break;
                        case 1:
                            __instance.SetCold(10f);
                            break;
                        case 2:
                            __instance.isDoom = true;
                            __instance.doomWithPit = false;
                            __instance.SetColor(Zombie.ZombieColor.Doom);
                            break;
                        case 3:
                            __instance.SetPoison();
                            break;
                        case 4:
                            __instance.SetEmbered();
                            break;
                        case 5:
                            __instance.SetGold();
                            break;
                    }
                }
                if (plant is not null && (int)plant.thePlantType == NullNut.PlantID && UnityEngine.Random.Range(0, 5) == 0)
                {
                    CreateParticle.SetParticle((int)ParticleType.RandomCloud, __instance.axis.transform.position, __instance.theZombieRow);
                    __instance.Die();
                }
            }
        }
    }

    [HarmonyPatch(typeof(TypeMgr), nameof(TypeMgr.UncrashablePlant))]
    public class TypeMgrUncrashablePlantPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Plant plant, ref bool __result)
        {
            if (plant is not null && (int)plant.thePlantType == NullNut.PlantID)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(WallNut), nameof(WallNut.TakeDamage))]
    public class WallNutTakeDamagePatch
    {
        [HarmonyPrefix]
        public static void Prefix(WallNut __instance, ref int damage)
        {
            if (__instance is not null && __instance.thePlantType == (PlantType)NullNut.PlantID)
            {
                damage = UnityEngine.Random.Range(0, 51);
            }
        }
    }
}