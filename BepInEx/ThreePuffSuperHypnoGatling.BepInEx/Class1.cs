using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace ThreePuffSuperHypnoGatling.BepInEx
{
    [BepInPlugin("salmon.threepuffhypnosupergatling", "ThreePuffSuperHypnoGatling", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<ThreePuffHypnoSuperGatling>();
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_puffHypnoPea>();
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_puffHypnoPea_fire>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "threepuffsuperhypnogatling");
            CustomCore.RegisterCustomBullet<Bullet_puffPea, Bullet_puffHypnoPea>((BulletType)Bullet_puffHypnoPea.BulletID, ab.GetAsset<GameObject>("Bullet_puffHypnoPea"));
            CustomCore.RegisterCustomBullet<Bullet_firePea, Bullet_puffHypnoPea_fire>((BulletType)Bullet_puffHypnoPea_fire.BulletID, ab.GetAsset<GameObject>("Bullet_puffHypnoPea_fire"));
            CustomCore.RegisterCustomPlant<SuperThreeGatling, ThreePuffHypnoSuperGatling>(
                ThreePuffHypnoSuperGatling.PlantID,
                ab.GetAsset<GameObject>("ThreePuffSuperHypnoGatlingPrefab"),
                ab.GetAsset<GameObject>("ThreePuffSuperHypnoGatlingPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.SmallPuff, 1932),
                    (1922, (int)PlantType.ThreePeater),
                    ((int)PlantType.HypnoPuff, (int)PlantType.SuperThreeGatling),
                    (1927, (int)PlantType.HypnoShroom)
                }, 1.5f, 0f, 30, 300, 7.5f, 1000
            );
            CustomCore.AddPlantAlmanacStrings(ThreePuffHypnoSuperGatling.PlantID,
                $"三线超级魅惑机枪小喷菇({ThreePuffHypnoSuperGatling.PlantID})",
                "向三行发射小魅惑豌豆的小超级机枪射手。\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>（30x3）x6/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒向3行发射1个伤害为3倍的魅惑小豌豆。</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+魅惑三线超级机枪射手</color>\n\n" +
                "<color=#3D1400>三线超级魅惑机枪小喷菇是学校里著名的“风云人物”，如果有“学园捣蛋奖”的话，他是毋庸置疑的第一，“上幼儿园的第一天，其他学生都在哇哇大哭，知道为什么吗？我在老师的桌子里边放了个假的僵尸脑袋！”三线超级魅惑机枪小喷菇沾沾自喜的说。</color>"
            );
            CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)ThreePuffHypnoSuperGatling.PlantID);
        }
    }

    public class ThreePuffHypnoSuperGatling : MonoBehaviour
    {
        public static ID PlantID = 1964;

        public SuperThreeGatling plant => gameObject.GetComponent<SuperThreeGatling>();

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("PuffShroom_body").FindChild("Shoot");
            plant.isShort = true;
        }
    }

    public class Bullet_puffHypnoPea : MonoBehaviour
    {
        public static int BulletID = 1965;
        public static int ParticleID = 800;

        public Bullet_puffPea bullet => gameObject.GetComponent<Bullet_puffPea>();
    }

    public class Bullet_puffHypnoPea_fire : MonoBehaviour
    {
        public static int BulletID = 1966;

        public void Update()
        {
            Vector2 velocity = bullet.rb.velocity;

            // 根据速度方向旋转尾焰
            if (bullet.tail != null)
            {
                float angle = Mathf.Atan2(bullet.rb.velocity.y, bullet.rb.velocity.x) * Mathf.Rad2Deg;
                bullet.tail.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public Bullet_firePea bullet => gameObject.GetComponent<Bullet_firePea>();
    }

    [HarmonyPatch(typeof(Shooter), nameof(Shooter.GetBulletType))]
    public class Shooter_GetBulletType
    {
        [HarmonyPrefix]
        public static bool Prefix(Shooter __instance, ref BulletType __result)
        {
            if (__instance != null && (int)__instance.thePlantType == ThreePuffHypnoSuperGatling.PlantID)
            {
                __result = (BulletType)Bullet_puffHypnoPea.BulletID;
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
            if (__instance != null && (int)__instance.thePlantType == ThreePuffHypnoSuperGatling.PlantID)
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
            if (__instance != null && (int)__instance.thePlantType == ThreePuffHypnoSuperGatling.PlantID)
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

    [HarmonyPatch(typeof(Bullet_puffPea), nameof(Bullet_puffPea.HitZombie))]
    public class Bullet_puffPea_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Bullet_puffPea __instance, ref Zombie zombie)
        {
            if ((int)__instance.theBulletType == Bullet_puffHypnoPea.BulletID)
            {
                if (zombie == null)
                    return false;

                bool beforeDying = zombie.beforeDying;
                var originHealth = zombie.theHealth;

                zombie.TakeDamage(DmgType.Normal, __instance.Damage);
                CreateParticle.SetParticle(Bullet_puffHypnoPea.ParticleID, __instance.transform.position, zombie.theZombieRow);
                __instance.PlaySound(zombie);
                __instance.Die();

                if (zombie.BoxType != BoxType.Water && ((!beforeDying && zombie.beforeDying) || (zombie.theHealth <= 0 && originHealth > 0 && !zombie.beforeDying)))
                {
                    PeaShooterZ component = CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.PeaShooterZombie, zombie.axis.transform.position.x).GetComponent<PeaShooterZ>();
                    if (component != null)
                    {
                        var health = component.theHealth;
                        var maxHealth = component.theMaxHealth;
                        component.hypnoPea = true;
                        component.normalHead.SetActive(false);
                        component.hypnoHead.SetActive(true);
                        ParticleManager.Instance.SetParticle(ParticleType.HypnoEmperorSkinCloud, zombie.axis.transform.position, zombie.theZombieRow);
                        component.SetMindControl();
                        component.BeSmall();
                        component.theHealth = health / 2;
                        component.theMaxHealth = maxHealth / 2;
                        component.UpdateHealthText();
                    }
                }

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_firePea), nameof(Bullet_firePea.HitZombie))]
    public class Bullet_firePea_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Bullet_firePea __instance, ref Zombie zombie)
        {
            if ((int)__instance.theBulletType == Bullet_puffHypnoPea_fire.BulletID)
            {
                if (zombie == null)
                    return false;

                bool beforeDying = zombie.beforeDying;
                var originHealth = zombie.theHealth;

                zombie.TakeDamage(DmgType.Normal, __instance.Damage);
                var soundID = UnityEngine.Random.Range(59, 61);
                GameAPP.PlaySound(soundID, 0.5f, 1f);

                // 创建命中粒子特效
                CreateParticle.SetParticle(33, __instance.transform.position, __instance.theBulletRow);
                __instance.Die();
                if (zombie.GetAttrTimers().freezeTimer > 0f)
                    zombie.Unfreezing();
                if (zombie.GetAttrTimers().coldTimer > 0f)
                    zombie.Warm();
                if (CoreTools.TravelAdvanced("怒火攻心"))
                    zombie.SetJalaed();

                if (zombie.BoxType != BoxType.Water && ((!beforeDying && zombie.beforeDying) || (zombie.theHealth <= 0 && originHealth > 0 && !zombie.beforeDying)))
                {
                    PeaShooterZ component = CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.PeaShooterZombie, zombie.axis.transform.position.x).GetComponent<PeaShooterZ>();
                    if (component != null)
                    {
                        var health = component.theHealth;
                        var maxHealth = component.theMaxHealth;
                        component.hypnoPea = true;
                        component.normalHead.SetActive(false);
                        component.hypnoHead.SetActive(true);
                        ParticleManager.Instance.SetParticle(ParticleType.HypnoEmperorSkinCloud, zombie.axis.transform.position, zombie.theZombieRow);
                        component.SetMindControl();
                        component.BeSmall();
                        component.theHealth = health / 2;
                        component.theMaxHealth = maxHealth / 2;
                        component.UpdateHealthText();
                    }
                }

                return false;
            }
            return true;
        }
    }
}
