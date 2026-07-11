using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Unity;

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
                30,
                300,
                0f,
                725
            );
            var ab_skin1 = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "skin1");
            CustomCore.RegisterCustomPlantSkin<SuperGatling, FireSuperGatling>(
                FireSuperGatling.PlantID,
                ab_skin1.GetAsset<GameObject>("FireSuperGatlingPrefab1"),
                ab_skin1.GetAsset<GameObject>("FireSuperGatlingPreview1"),
                new List<(int, int)>
                {
                ((int)PlantType.SuperGatling, (int)PlantType.Jalapeno),
                ((int)PlantType.Jalapeno, (int)PlantType.SuperGatling),
                ((int)PlantType.FireSniper, (int)PlantType.Peashooter),
                ((int)PlantType.Peashooter, (int)PlantType.FireSniper)
                },
                1.5f,
                0f,
                30,
                300,
                0f,
                725
            );
            CustomCore.AddPlantAlmanacStrings(FireSuperGatling.PlantID,
                $"超级火焰机枪射手({FireSuperGatling.PlantID})",
                $"一次发射六颗火辣豌豆，有概率一次性发射大量火辣豌豆\n\n" +
                $"<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                $"<color=#3D1400>贴图作者：</color><color=red>@林秋-AutumnLin</color>\n" +
                $"<color=#3D1400>伤害：</color><color=red>30x6/1.5秒</color>\n" +
                $"<color=#3D1400>特点：①</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒散射3发火辣豌豆</color>\n" +
                $"<color=#3D1400>②</color><color=red>可以和火焰狙击射手互相转化</color>\n" +
                $"<color=#3D1400>词条1:</color><color=red>五阶升级：超级火辣机枪射手的攻击力x10，子弹赋予的红温增伤额外提升500%</color>\n" +
                $"<color=#3D1400>融合配方：</color><color=red>超级机枪射手+火爆辣椒</color>\n" +
                $"<color=#3D1400>转化配方：</color><color=red>豌豆射手←→豌豆射手</color>\n\n" +
                $"<color=#3D1400>宝开鱼</color>"
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
                __result = BulletType.Bullet_pea_jala;
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_pea_jala))]
    public static class Bullet_pea_jalaPatch
    {
        [HarmonyPatch(nameof(Bullet_pea_jala.HitZombie))]
        [HarmonyPrefix]
        public static void PreTakeDamage(Bullet_pea_jala __instance)
        {
            if ((int)__instance.fromType == FireSuperGatling.PlantID && Lawnf.TravelAdvanced(GameAPP.Instance.GetData<BuffID>("MegaSuperGatling_BuffID").val))
            {
                __instance.Damage *= 5;
            }
        }

        [HarmonyPatch(nameof(Bullet_pea_jala.HitZombie))]
        [HarmonyPostfix]
        public static void PostTakeDamage(Bullet_pea_jala __instance)
        {
            if ((int)__instance.fromType == FireSuperGatling.PlantID && Lawnf.TravelAdvanced(GameAPP.Instance.GetData<BuffID>("MegaSuperGatling_BuffID").val))
            {
                __instance.Damage /= 5;
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_firePea_super))]
    public static class Bullet_firePea_superPatch
    {
        [HarmonyPatch(nameof(Bullet_firePea_super.HitZombie))]
        [HarmonyPrefix]
        public static void PreTakeDamage(Bullet_firePea_super __instance)
        {
            if ((int)__instance.fromType == FireSuperGatling.PlantID && Lawnf.TravelAdvanced(GameAPP.Instance.GetData<BuffID>("MegaSuperGatling_BuffID").val))
            {
                __instance.Damage *= 5;
            }
        }

        [HarmonyPatch(nameof(Bullet_firePea_super.HitZombie))]
        [HarmonyPostfix]
        public static void PostTakeDamage(Bullet_firePea_super __instance)
        {
            if ((int)__instance.fromType == FireSuperGatling.PlantID && Lawnf.TravelAdvanced(GameAPP.Instance.GetData<BuffID>("MegaSuperGatling_BuffID").val))
            {
                __instance.Damage /= 5;
            }
        }
    }
}