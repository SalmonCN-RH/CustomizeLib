using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace PuffSuperFireGatling.BepInEx
{
    [BepInPlugin("salmon.puffsuperfiregatling", "PuffSuperFireGatling", "1.0")]
    public class Core : BasePlugin//304
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<PuffFireSuperGatling>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "pufffiresupergatling");
            CustomCore.RegisterCustomPlant<SuperGatling, PuffFireSuperGatling>(
                PuffFireSuperGatling.PlantID,
                ab.GetAsset<GameObject>("PuffFireSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("PuffFireSuperGatlingPreview"),
                new List<(int, int)>
                {
                    (1907, (int)PlantType.Jalapeno),
                    ((int)PlantType.SmallPuff, 1901)
                },
                1.5f, 0f, 60, 300, 0f, 825
            );
            CustomCore.AddPlantAlmanacStrings(PuffFireSuperGatling.PlantID,
                $"超级火焰小喷菇机枪射手({PuffFireSuperGatling.PlantID})",
                "一次发射六颗小火焰豌豆，有概率一次性发射大量小火焰豌豆\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>60x6/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒散射3发小火焰豌豆</color>\n<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+超级机枪射手+火爆辣椒</color>\n\n<color=#3D1400>“我们是火焰的种子，也是火焰的起始，更是火焰的终点。”超级火焰小喷菇机枪射手诞生于火焰之中，他是火焰本身，也是火焰衍生，不知从何时起，火焰开始有了思想，他们好奇这个世界，那股强烈的好奇催生了超级火焰小喷菇机枪射手，他们是火焰的眼睛，也是火焰的嘴巴，他们代替火焰了解这个世界，帮助这个世界</color>"
            );
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)PuffFireSuperGatling.PlantID);
            CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)PuffFireSuperGatling.PlantID);
        }
    }

    public class PuffFireSuperGatling : MonoBehaviour
    {
        public static int PlantID = 1926;

        public SuperGatling plant => gameObject.GetComponent<SuperGatling>();

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.GetChild(0);
            plant.isShort = true;
        }
    }

    [HarmonyPatch(typeof(SuperGatling), nameof(SuperGatling.GetBulletType))]
    public class SuperGatling_GetBulletType
    {
        [HarmonyPostfix]
        public static void Postfix(SuperGatling __instance, ref BulletType __result)
        {
            if ((int)__instance.thePlantType == PuffFireSuperGatling.PlantID)
            {
                __result = BulletType.Bullet_firePea_small;
            }
        }
    }
}