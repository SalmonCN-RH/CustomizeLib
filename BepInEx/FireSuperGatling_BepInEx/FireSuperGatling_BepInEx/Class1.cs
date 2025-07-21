using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace FireSuperGatling_BepInEx
{
    [BepInPlugin("salmon.firesupergatling", "FireSuperGatling", "1.0")]
    public class Core : BasePlugin//304
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<FireSuperGatling>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "firesupergatling");
            CustomCore.RegisterCustomPlant<SuperGatling, FireSuperGatling>(
                FireSuperGatling.PlantID,
                ab.GetAsset<GameObject>("FireSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("FireSuperGatlingPreview"),
                new List<(int, int)>
                {
                ((int)PlantType.SuperGatling, (int)PlantType.Jalapeno),
                ((int)PlantType.Jalapeno, (int)PlantType.SuperGatling),
                ((int)PlantType.FireSniper, (int)PlantType.Peashooter),
                ((int)PlantType.Peashooter, (int)PlantType.FireSniper)
                },
                1.5f,
                0f,
                80,
                300,
                0f,
                725
            );
            CustomCore.AddPlantAlmanacStrings(FireSuperGatling.PlantID,
                $"超级火焰机枪射手({FireSuperGatling.PlantID})",
                "一次发射六颗火焰豌豆，有概率一次性发射大量火焰豌豆\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>80/100/120*6/1.5秒;\n80/100/120*3/0.02秒（大招）</color>\n<color=#3D1400>特点：</color><color=red>攻击有3%概率散射大量火焰豌豆，持续5秒。可以与火焰狙击射手相转化</color>\n<color=#3D1400>转换配方：</color><color=red>豌豆射手←→豌豆射手</color>\n<color=#3D1400>融合配方：</color><color=red>超级机枪射手+火爆辣椒</color>\n\n<color=#3D1400>前“第七维度大排档”首席炭火师。它说豌豆是未成形的烤串，50/60/70？那是炭基经济学在流泪！3%的‘星火低语时刻’？嘘——别问，那是它撒孜然时打了个来自旧维度的喷嚏！警告：请勿在易燃草坪、物价局职员或试图索要发票的僵尸面前种植… 毕竟，它递出的不是豌豆，是账单——熔岩做的，且不找零（包括小费）。</color>"
            );
            CustomCore.AddFusion((int)PlantType.FireSniper, FireSuperGatling.PlantID, (int)PlantType.Peashooter);
            CustomCore.AddFusion((int)PlantType.FireSniper, (int)PlantType.Peashooter, FireSuperGatling.PlantID);
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)FireSuperGatling.PlantID);
        }
    }

    public class FireSuperGatling : MonoBehaviour
    {
        public static int PlantID = 1901;

        public SuperGatling plant => gameObject.GetComponent<SuperGatling>();

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.GetChild(0).FindChild("Shoot");
        }
    }

    [HarmonyPatch(typeof(SuperGatling), nameof(SuperGatling.GetBulletType))]
    public class SuperGatling_GetBulletType
    {
        public static void Postfix(SuperGatling __instance, ref BulletType __result)
        {
            if ((int)__instance.thePlantType == FireSuperGatling.PlantID)
            {
                int random = UnityEngine.Random.RandomRange(0, 6);
                switch (random)
                {
                    case 0:
                    case 1:
                        __result = BulletType.Bullet_firePea_red;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        __result = BulletType.Bullet_firePea_orange;
                        break;
                    default:
                        __result = BulletType.Bullet_firePea_yellow;
                        break;
                }
                bool isDamageUnChange = __instance.attackDamage == 80 ||
                    __instance.attackDamage == 100 ||
                    __instance.attackDamage == 120;
                if (isDamageUnChange)
                {
                    switch (__result)
                    {
                        case BulletType.Bullet_firePea_yellow:
                            __instance.attackDamage = 80;
                            break;
                        case BulletType.Bullet_firePea_orange:
                            __instance.attackDamage = 100;
                            break;
                        case BulletType.Bullet_firePea_red:
                            __instance.attackDamage = 120;
                            break;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Shooter), nameof(Shooter.Update))]
    public class Shooter_Update
    {
        [HarmonyPrefix]
        public static void Prefix(Shooter __instance)
        {
            if ((int)__instance.thePlantType == FireSuperGatling.PlantID)
            {
                // Debug.Log(__instance.shoot == null);
                __instance.shoot = __instance.gameObject.transform.GetChild(0).FindChild("Shoot");
            }
        }
    }
}