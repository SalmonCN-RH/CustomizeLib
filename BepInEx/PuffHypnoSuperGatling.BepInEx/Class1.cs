using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace PuffHypnoSuperGatling.BepInEx
{
    [BepInPlugin("salmon.puffhypnosupergatling", "PuffHypnoSuperGatling", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "puffhypnosupergatling");
            CustomCore.RegisterCustomParticle((ParticleType)Bullet_puffHypnoPea.ParticleID, ab.GetAsset<GameObject>("PuffHypnoPeaSplat"));
            CustomCore.RegisterCustomBullet<Bullet_puffPea, Bullet_puffHypnoPea>((BulletType)Bullet_puffHypnoPea.BulletID, ab.GetAsset<GameObject>("Bullet_puffHypnoPea"));
            CustomCore.RegisterCustomBullet<Bullet_firePea, Bullet_puffHypnoPea_fire>((BulletType)Bullet_puffHypnoPea_fire.BulletID, ab.GetAsset<GameObject>("Bullet_puffHypnoPea_fire"));
            CustomCore.RegisterCustomPlant<SuperGatling, PuffHypnoSuperGatling>(
                PuffHypnoSuperGatling.PlantID,
                ab.GetAsset<GameObject>("PuffHypnoSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("PuffHypnoSuperGatlingPreview"),
                new List<(int, int)>
                {
                    (1907, (int)PlantType.HypnoShroom),
                    ((int)PlantType.SmallPuff, (int)PlantType.SuperHypnoGatling),
                    ((int)PlantType.HypnoPuff, (int)PlantType.SuperGatling)
                },
                1.5f, 0f, 30, 300, 0f, 275
            );
            CustomCore.AddPlantAlmanacStrings(PuffHypnoSuperGatling.PlantID,
                $"超级魅惑小喷菇机枪射手({PuffHypnoSuperGatling.PlantID})",
                "一次发射六颗小魅惑豌豆，有概率一次性发射大量小魅惑豌豆\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>30x6/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①每次攻击有2%概率触发大招，5秒内，每0.02秒散射3发魅惑小豌豆\n" +
                "②杀死僵尸时，召唤一个植物方的迷你魅惑豌豆射手僵尸</color>\n" +
                "<color=#3D1400>迷你魅惑豌豆射手僵尸：</color><color=red>韧性135。每1.5秒发射一个伤害为20的魅惑豌豆</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+超级机枪射手+魅惑菇</color>\n\n" +
                "<color=#3D1400>“如果你魅惑了一千种僵尸，你就会发现，这个世界上，压根没有素食的僵尸，”超级魅惑机枪小喷菇扶了下眼镜“研究发现，僵尸大都是依据自身的生存本能在行动，不管在它对面的是植物还是其它僵尸，只要是挡到它的，它都会慢慢吃掉，这样的生存本能并非饥饿，而是像底层代码一样，印刻在了僵尸的意识里边，”超级魅惑机枪小喷菇继续说道“但是在他们的意识不是不是单一的，更像是一种参杂体，而我们正在做的，就是让它的另外一个意识占据身体，并服务于我们，”</color>"
            );
            CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)PuffHypnoSuperGatling.PlantID);
        }
    }

    public class PuffHypnoSuperGatling : MonoBehaviour
    {
        public static int PlantID = 1922;

        public SuperGatling plant => gameObject.GetComponent<SuperGatling>();

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.GetChild(0);
            plant.isShort = true;
        }
    }

    public class Bullet_puffHypnoPea : MonoBehaviour
    {
        public static int BulletID = 1922;
        public static int ParticleID = 800;

        public Bullet_puffPea bullet => gameObject.GetComponent<Bullet_puffPea>();
    }

    public class Bullet_puffHypnoPea_fire : MonoBehaviour
    {
        public static int BulletID = 1923;

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

    [HarmonyPatch(typeof(SuperGatling), nameof(SuperGatling.GetBulletType))]
    public class SuperGatling_GetBulletType
    {
        [HarmonyPostfix]
        public static void Postfix(SuperGatling __instance, ref BulletType __result)
        {
            if ((int)__instance.thePlantType == PuffHypnoSuperGatling.PlantID)
            {
                __result = (BulletType)Bullet_puffHypnoPea.BulletID;
            }
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
                int soundID = UnityEngine.Random.Range(59, 61);
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
