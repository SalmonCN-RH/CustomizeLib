using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Unity.Collections;
using UnityEngine;

[assembly: MelonInfo(typeof(PuffHypnoSuperGatling.MelonLoader.Core), "PuffHypnoSuperGatling", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace PuffHypnoSuperGatling.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "puffhypnosupergatling");
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
                "一次发射六颗小魅惑豌豆，有概率一次性发射大量小魅惑豌豆\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>30*6/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒散射3发小魅惑豌豆，小魅惑豌豆杀死僵尸时，召唤一个血量仅为原来50%的植物方的小魅惑豌豆射手僵尸。</color>\n<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+超级机枪射手+魅惑菇</color>\n\n<color=#3D1400>咕咕咕</color>"
            );
            CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)PuffHypnoSuperGatling.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
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

    [RegisterTypeInIl2Cpp]
    public class Bullet_puffHypnoPea : MonoBehaviour
    {
        public static int BulletID = 1922;
        public static int ParticleID = 1922;

        /*public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<CaltropTorch>(out var torch))
            {
                if (bullet.torchWood == torch.gameObject || bullet.isZombieBullet || bullet.theBulletRow != torch.thePlantRow)
                    return;
                Bullet newBullet = CreateBullet.Instance.SetBullet(bullet.transform.position.x, bullet.transform.position.y, bullet.theBulletRow, (BulletType)Bullet_puffHypnoPea_fire.BulletID, BulletMoveWay.MoveRight, false);
                newBullet.torchWood = torch.gameObject.GetComponent<Plant>();
                newBullet.Damage = bullet.Damage * 2;
                newBullet.theExistTime = bullet.theExistTime;
                newBullet.rb.velocity = bullet.rb.velocity;
                newBullet.normalSpeed = bullet.normalSpeed;
                newBullet.transform.rotation = bullet.transform.rotation;
                newBullet.rogueStatus = bullet.rogueStatus;
                newBullet.theStatus = bullet.theStatus;
                
                if (newBullet.normalSpeed == 0)
                    newBullet.normalSpeed = 6f;
                torch.count++;
                if (torch.count > 100)
                {
                    torch.count = 0;
                    int startColumn = torch.thePlantColumn + 1;

                    // 遍历至棋盘最右列
                    for (int col = startColumn; col < torch.board.columnNum; col++)
                    {
                        // 尝试创建特殊植物
                        GameObject plantObj = CreatePlant.Instance.SetPlant(
                            col,
                            torch.thePlantRow,
                            PlantType.FireCaltrop
                        );

                        if (plantObj != null)
                        {
                            // 获取植物组件
                            Plant newPlant = plantObj.GetComponent<Plant>();

                            // 在植物位置创建特效
                            Vector3 plantPos = newPlant.axis.position;
                            CreateParticle.SetParticle(11, plantPos, torch.thePlantRow);
                            break;
                        }
                    }
                }
                bullet.Die();
            }
        }*/

        public Bullet_puffHypnoPea() : base(ClassInjector.DerivedConstructorPointer<Bullet_puffPea>()) => ClassInjector.DerivedConstructorBody(this);

        public Bullet_puffHypnoPea(IntPtr i) : base(i)
        {
        }

        public Bullet_puffPea bullet => gameObject.GetComponent<Bullet_puffPea>();
    }

    [RegisterTypeInIl2Cpp]
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

        public Bullet_puffHypnoPea_fire() : base(ClassInjector.DerivedConstructorPointer<Bullet_firePea>()) => ClassInjector.DerivedConstructorBody(this);

        public Bullet_puffHypnoPea_fire(IntPtr i) : base(i)
        {
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
                int originHealth = zombie.theHealth;

                zombie.TakeDamage(DmgType.Normal, __instance.Damage);
                ParticleManager.Instance.SetParticle((ParticleType)Bullet_puffHypnoPea.ParticleID, __instance.transform.position, zombie.theZombieRow);
                __instance.PlaySound(zombie);
                __instance.Die();

                if (zombie.BoxType != BoxType.Water && ((!beforeDying && zombie.beforeDying) || (zombie.theHealth <= 0 && originHealth > 0 && !zombie.beforeDying)))
                {
                    PeaShooterZ component = CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.PeaShooterZombie, zombie.axis.transform.position.x).GetComponent<PeaShooterZ>();
                    if (component != null)
                    {
                        int health = component.theHealth;
                        int maxHealth = component.theMaxHealth;
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
                int originHealth = zombie.theHealth;

                zombie.TakeDamage(DmgType.Normal, __instance.Damage);
                int soundID = UnityEngine.Random.Range(59, 61);
                GameAPP.PlaySound(soundID, 0.5f, 1f);

                // 创建命中粒子特效
                CreateParticle.SetParticle(33, __instance.transform.position, __instance.theBulletRow);
                __instance.Die();
                if (zombie.freezeTimer > 0f)
                    zombie.Unfreezing();
                if (zombie.coldTimer > 0f)
                    zombie.Warm();
                if (Lawnf.TravelAdvanced(15))
                    zombie.SetJalaed();

                if (zombie.BoxType != BoxType.Water && ((!beforeDying && zombie.beforeDying) || (zombie.theHealth <= 0 && originHealth > 0 && !zombie.beforeDying)))
                {
                    PeaShooterZ component = CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.PeaShooterZombie, zombie.axis.transform.position.x).GetComponent<PeaShooterZ>();
                    if (component != null)
                    {
                        int health = component.theHealth;
                        int maxHealth = component.theMaxHealth;
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