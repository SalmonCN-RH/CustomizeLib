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
        public const int PlantID = 1921;
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<FireSuperThreeGatling>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "firesuperthreegatling");
            CustomCore.RegisterCustomPlant<SuperThreeGatling, FireSuperThreeGatling>(PlantID, ab.GetAsset<GameObject>("FireSuperThreeGatlingPrefab"),
                ab.GetAsset<GameObject>("FireSuperThreeGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperThreeGatling, (int)PlantType.Jalapeno),
                    (1901, (int)PlantType.ThreePeater),
                    ((int)PlantType.SuperGatling, (int)PlantType.DarkThreePeater)
                }, 1.5f, 0f, 30, 300, 7.5f, 1000);
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)PlantID);
            CustomCore.AddPlantAlmanacStrings(PlantID, $"火焰三线超级机枪射手({PlantID})",
                "向三行发射火焰豌豆的超级机枪射手。\n\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>（30x6）x3/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒前三行各发射1个3倍伤害火辣豌豆</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>三线超级机枪射手+火爆辣椒</color>\n\n" +
                "<color=#3D1400>“想了解火焰的力量么，想知道火焰是如何燃烧的么，想清楚火焰的起源么”“那就加入我们，”“对加入我们”最先说话的是老大，他是火爆辣椒的首席徒弟，曾经出过《我与火焰》《我在火焰中诞生》等一系列小说，是火爆辣椒最满意的徒弟，第二个说话是老二，他最喜欢的事情就是跟着大哥一起出门做宣传，对于大哥来说，他是可以摆在眼前的宣传标语，对于三弟来说，他是最可靠的哥哥，最后说话的是老三，他最喜欢的，是他的两个哥哥……</color>");

        }
    }

    public class FireSuperThreeGatling : MonoBehaviour
    {
        public static ID BulletID = BulletType.Bullet_pea_jala;

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("headPos2/ThreePeater_head2/ThreePeater_mouth/Shoot");
        }

        public SuperThreeGatling plant => gameObject.GetComponent<SuperThreeGatling>();
    }

    [HarmonyPatch(typeof(ThreePeater))]
    public static class ThreePeaterPatch
    {
        [HarmonyPatch(nameof(ThreePeater.Shoot1))]
        [HarmonyPrefix]
        public static bool Prefix(ThreePeater __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == Core.PlantID)
            {
                if (__instance.shoot == null) return false;

                var bullet = CreateBullet.Instance.SetBullet(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow, 
                    FireSuperThreeGatling.BulletID, BulletMoveWay.MoveRight);

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;

                GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1f);

                // 三线射手特殊逻辑：根据所在行发射额外子弹
                if (__instance.thePlantRow == 0)
                    __instance.ShootLower(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow + 1);
                else if (__instance.thePlantRow == __instance.board.rowNum - 1)
                    __instance.ShootUpper(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow - 1);
                else
                {
                    __instance.ShootLower(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow + 1);
                    __instance.ShootUpper(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow - 1);
                    return false;
                }

                __instance.Invoke("ExtraBullet", 0.2f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ThreePeater.ExtraBullet))]
        [HarmonyPrefix]
        public static bool PreExtraBullet(ThreePeater __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == Core.PlantID)
            {
                var bullet = CreateBullet.Instance.SetBullet(__instance.shoot.position.x, __instance.shoot.position.y,
                    __instance.thePlantRow, FireSuperThreeGatling.BulletID, BulletMoveWay.MoveRight);

                if (bullet == null) return false;

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ThreePeater.ShootLower))]
        [HarmonyPrefix]
        public static bool PreShootLower(ThreePeater __instance, float X, float Y, int row)
        {
            if (__instance != null && (int)__instance.thePlantType == Core.PlantID)
            {
                var bullet = CreateBullet.Instance.SetBullet(X, Y, row, FireSuperThreeGatling.BulletID, BulletMoveWay.MoveRight_threePeater);

                if (bullet == null) return false;

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ThreePeater.ShootUpper))]
        [HarmonyPrefix]
        public static bool PreShootUpper(ThreePeater __instance, float X, float Y, int row)
        {
            if (__instance != null && (int)__instance.thePlantType == Core.PlantID)
            {
                var bullet = CreateBullet.Instance.SetBullet(X, Y, row, FireSuperThreeGatling.BulletID, BulletMoveWay.MoveRight_threePeater);

                if (bullet == null) return false;

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;
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
            if (__instance != null && (int)__instance.thePlantType == Core.PlantID)
            {
                var bullet = CreateBullet.Instance.SetBullet(x, y, row, FireSuperThreeGatling.BulletID, bulletMoveWay, false);
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
            if (__instance != null && (int)__instance.thePlantType == Core.PlantID)
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