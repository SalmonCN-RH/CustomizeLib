using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using Unity.VisualScripting;
using CustomizeLib.BepInEx;

namespace FireSuperThreeGatling.BepInEx
{
    [BepInPlugin("salmon.firesuperthreegatling", "FireSuperThreeGatling", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<FireSuperThreeGatling>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "firesuperthreegatling");
            CustomCore.RegisterCustomPlant<SuperThreeGatling, FireSuperThreeGatling>(FireSuperThreeGatling.PlantID, ab.GetAsset<GameObject>("FireSuperThreeGatlingPrefab"),
                ab.GetAsset<GameObject>("FireSuperThreeGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperThreeGatling, (int)PlantType.Jalapeno),
                    (1901, (int)PlantType.ThreePeater),
                    ((int)PlantType.SuperGatling, (int)PlantType.DarkThreePeater)
                }, 1.5f, 0f, 100, 300, 0f, 1000);
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)FireSuperThreeGatling.PlantID);
            CustomCore.AddPlantAlmanacStrings(FireSuperThreeGatling.PlantID, $"火焰三线超级机枪射手({FireSuperThreeGatling.PlantID})",
                "向三行发射火焰豌豆的超级机枪射手。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>(80/100/120)x6/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒向三行各发射1个伤害为3倍的火焰豌豆。</color>\n<color=#3D1400>融合配方：</color><color=red>超级机枪射手（底座）+三线射手+火爆辣椒</color>\n\n<color=#3D1400>“想了解火焰的力量么，想知道火焰是如何燃烧的么，想清楚火焰的起源么”“那就加入我们，”“对加入我们”最先说话的是老大，他是火爆辣椒的首席徒弟，曾经出过《我与火焰》《我在火焰中诞生》等一系列小说，是火爆辣椒最满意的徒弟，第二个说话是老二，他最喜欢的事情就是跟着大哥一起出门做宣传，对于大哥来说，他是可以摆在眼前的宣传标语，对于三弟来说，他是最可靠的哥哥，最后说话的是老三，他最喜欢的，是他的两个哥哥……</color>");
        }
    }

    public class FireSuperThreeGatling : MonoBehaviour
    {
        public static int PlantID = 1921;

        public FireSuperThreeGatling() : base(ClassInjector.DerivedConstructorPointer<FireSuperThreeGatling>()) => ClassInjector.DerivedConstructorBody(this);

        public FireSuperThreeGatling(IntPtr i) : base(i)
        {
        }

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("headPos2/ThreePeater_head2/ThreePeater_mouth/Shoot");
        }

        public SuperThreeGatling plant => gameObject.GetComponent<SuperThreeGatling>();
    }

    [HarmonyPatch(typeof(ThreePeater), nameof(ThreePeater.GetBulletType))]
    public class ThreePeater_GetBulletType
    {
        [HarmonyPrefix]
        public static bool Prefix(ThreePeater __instance, ref BulletType __result)
        {
            if (__instance != null && (int)__instance.thePlantType == FireSuperThreeGatling.PlantID)
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
            if (__instance != null && (int)__instance.thePlantType == FireSuperThreeGatling.PlantID)
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
            if (__instance != null && (int)__instance.thePlantType == FireSuperThreeGatling.PlantID)
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