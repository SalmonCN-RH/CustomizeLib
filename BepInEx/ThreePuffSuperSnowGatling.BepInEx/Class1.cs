using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace ThreePuffSuperSnowGatling.BepInEx
{
    [BepInPlugin("salmon.threepuffsupersnowgatling", "ThreePuffSuperSnowGatling", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<ThreePuffSuperSnowGatling>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "threepuffsupersnowgatling");
            CustomCore.RegisterCustomPlant<SuperThreeGatling, ThreePuffSuperSnowGatling>(
                ThreePuffSuperSnowGatling.PlantID,
                ab.GetAsset<GameObject>("ThreePuffSuperSnowGatlingPrefab"),
                ab.GetAsset<GameObject>("ThreePuffSuperSnowGatlingPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.SmallPuff, 1920),
                    (1910, (int)PlantType.ThreePeater),
                    (1927, (int)PlantType.IceShroom),
                    ((int)PlantType.IcePuff, (int)PlantType.SuperThreeGatling)
                },
                1.5f, 0f, 20, 300, 0f, 1000
            );
            CustomCore.AddPlantAlmanacStrings(ThreePuffSuperSnowGatling.PlantID,
                $"三线超级寒冰机枪小喷菇({ThreePuffSuperSnowGatling.PlantID})",
                "向三行发射小冰锥的小超级机枪射手。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>(20x3)x6/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒向三行各发射1个伤害为3倍的小冰锥。</color>\n<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+三线超级寒冰机枪射手</color>\n\n<color=#3D1400>咕咕嘎嘎</color>"
            );
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)ThreePuffSuperSnowGatling.PlantID);
            CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)ThreePuffSuperSnowGatling.PlantID);
        }
    }

    public class ThreePuffSuperSnowGatling : MonoBehaviour
    {
        public static int PlantID = 1928;

        public SuperThreeGatling plant => gameObject.GetComponent<SuperThreeGatling>();

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("PuffShroom_body").FindChild("Shoot");
            plant.isShort = true;
        }
    }

    [HarmonyPatch(typeof(ThreePeater), nameof(ThreePeater.GetBulletType))]
    public class ThreePeater_GetBulletType
    {
        [HarmonyPrefix]
        public static bool Prefix(ThreePeater __instance, ref BulletType __result)
        {
            if (__instance != null && (int)__instance.thePlantType == ThreePuffSuperSnowGatling.PlantID)
            {
                __result = BulletType.Bullet_smallIceSpark;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SuperThreeGatling))]
    public class SuperThreeGatling_SuperShoot
    {
        [HarmonyPatch(nameof(SuperThreeGatling.SuperShoot))]
        [HarmonyPrefix]
        public static bool Prefix(SuperThreeGatling __instance, ref float angle, ref float speed, ref float x, ref float y, ref BulletMoveWay bulletMoveWay, ref int row)
        {
            if (__instance != null && (int)__instance.thePlantType == ThreePuffSuperSnowGatling.PlantID)
            {
                CreateBullet creator = CreateBullet.Instance;

                Bullet bullet = CreateBullet.Instance.SetBullet(x, y, row, __instance.GetBulletType(), bulletMoveWay, false);
                // 配置子弹属性
                if (bullet != null)
                {
                    // 设置子弹旋转角度
                    bullet.transform.Rotate(0, 0, angle);

                    // 设置子弹移动速度
                    bullet.normalSpeed = speed;

                    // 设置三倍攻击伤害
                    bullet.Damage = 3 * __instance.attackDamage;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(SuperThreeGatling.Update))]
        [HarmonyPrefix]
        public static void Prefix_Update(SuperThreeGatling __instance, out bool __state)
        {
            if (__instance != null && (int)__instance.thePlantType == ThreePuffSuperSnowGatling.PlantID)
            {
                if (__instance.timer > 0 && __instance.timer - Time.deltaTime <= 0f)
                {
                    __state = true;
                    return;
                }
            }
            __state = false;
        }

        [HarmonyPatch(nameof(SuperThreeGatling.Update))]
        [HarmonyPostfix]
        public static void Postfix_Update(SuperThreeGatling __instance, bool __state)
        {
            if (__state)
                __instance.anim.SetTrigger("shoot");
        }
    }
}