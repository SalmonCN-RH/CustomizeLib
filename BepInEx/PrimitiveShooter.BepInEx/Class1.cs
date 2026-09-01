using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace PrimitiveShooter.BepInEx
{
    [BepInPlugin("salmon.primitiveshooter", "PrimitiveShooter", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<PrimitiveShooter>();
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_primitivePea>();
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_stonePea>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "primitiveshooter");
            #region 原始豌豆
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_primitivePea>(Bullet_primitivePea.BulletID, ab.GetAsset<GameObject>("PrimitivePea"));
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_primitivePea>(Bullet_primitivePea.FireID, ab.GetAsset<GameObject>("FirePrimitivePea"));
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_primitivePea>(Bullet_primitivePea.OrangeFireID, ab.GetAsset<GameObject>("FirePrimitivePea Orange"));
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_primitivePea>(Bullet_primitivePea.RedFireID, ab.GetAsset<GameObject>("FirePrimitivePea red"));
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_stonePea>(Bullet_stonePea.BulletID, ab.GetAsset<GameObject>("StonePea"));
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_stonePea>(Bullet_stonePea.FireID, ab.GetAsset<GameObject>("FireStonePea"));
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_stonePea>(Bullet_stonePea.RedFireID, ab.GetAsset<GameObject>("FireStonePeaRed"));
            CustomCore.RegisterCustomParticle(PrimitiveShooter.ParticleID, ab.GetAsset<GameObject>("StonePeaSplat"));
            CustomCore.RegisterCustomParticle(PrimitiveShooter.FireParticleID, ab.GetAsset<GameObject>("FireStonePeaSplat"));
            CustomCore.RegisterCustomPlant<PeaShooter, PrimitiveShooter>(PrimitiveShooter.PlantID, ab.GetAsset<GameObject>("PrimitiveShooterPrefab"),
                ab.GetAsset<GameObject>("PrimitiveShooterPreview"), new() { }, 1.5f, 0f, 40, 300, 7.5f, 225);
            CustomCore.AddPlantAlmanacStrings(PrimitiveShooter.PlantID,
                $"原始豌豆射手({(int)PrimitiveShooter.PlantID})",
                "古老的豌豆射手，发射坚硬的原始豌豆，有概率发射石块豌豆造成范围击退。\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>40/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=#3D1400>①</color><color=red>子弹直击目标造成击退，发射的子弹有50%概率改为原始石块豌豆</color>\n" +
                "<color=#3D1400>原始石块豌豆：</color><color=#3D1400>①</color><color=red>造成半径1格无衰减溅射并击退，击中的目标有50%概率造成1秒定身</color>\n" +
                "<color=#3D1400>②</color><color=red>可经过超级火炬，变为熔岩子弹，伤害x2，再次经过究极火炬时x4</color>\n\n" +
                "<color=#3D1400>面对这位刚被解封的老祖宗，这种跨时代的会面令许多豌豆射手们都颇为好奇，在一段时间的经历下，原始豌豆射手十分珍惜当下的时间，或许岁月带走了他太多东西。\r\n“可我还有一群后辈，不是么”</color>\n\n" +
                "<color=#955300>花费：</color><color=red>225</color>\n" + 
                "<color=#955300>冷却时间：</color><color=red>7.5秒</color>"
            );
            CustomCore.RegisterCustomCardToColorfulCards(PrimitiveShooter.PlantID);
            #endregion
            #region 原始大哥
            ClassInjector.RegisterTypeInIl2Cpp<PrimitiveSuperGatling>();
            CustomCore.RegisterCustomPlant<SuperGatling, PrimitiveSuperGatling>(PrimitiveSuperGatling.PlantID, ab.GetAsset<GameObject>("PrimitiveSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("PrimitiveSuperGatlingPreview"), new List<(int, int)>()
                {
                    ((int)PlantType.SuperGatling, PrimitiveShooter.PlantID)
                }, 1.5f, 0f, 40, 300, 7.5f, 875);
            CustomCore.RegisterCustomPlantSkin<SuperGatling, PrimitiveSuperGatling>(PrimitiveSuperGatling.PlantID, ab.GetAsset<GameObject>("PrimitiveSuperGatlingSkinPrefab"),
                ab.GetAsset<GameObject>("PrimitiveSuperGatlingSkinPreview"), new List<(int, int)>()
                {
                    ((int)PlantType.SuperGatling, PrimitiveShooter.PlantID)
                }, 1.5f, 0f, 40, 300, 7.5f, 875, new List<(BulletType, List<GameObject?>)>()
                {
                    (Bullet_primitivePea.BulletID, new List<GameObject?>() { ab.GetAsset<GameObject>("PrimitivePeaSkin") }),
                    (Bullet_primitivePea.FireID, new List<GameObject?>() { ab.GetAsset<GameObject>("FirePrimitivePeaSkin") }),
                    (Bullet_primitivePea.OrangeFireID, new List<GameObject?>() { ab.GetAsset<GameObject>("FirePrimitivePeaSkin Orange") }),
                    (Bullet_primitivePea.RedFireID, new List<GameObject?>() { ab.GetAsset<GameObject>("FirePrimitivePeaSkin red") }),
                    (Bullet_stonePea.BulletID, new List<GameObject?>() { ab.GetAsset<GameObject>("StonePeaSkin") }),
                    (Bullet_stonePea.FireID, new List<GameObject?>() { ab.GetAsset<GameObject>("FireStonePeaSkin") }),
                    (Bullet_stonePea.RedFireID, new List<GameObject?>() { ab.GetAsset<GameObject>("FireStonePeaRedSkin") }),
                });
            CustomCore.AddPlantAlmanacStrings(PrimitiveSuperGatling.PlantID,
                $"原始超级机枪射手({(int)PrimitiveSuperGatling.PlantID})",
                "一次发射六颗原始豌豆或石块豌豆，并有概率以地震般席卷石质风暴。\n\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin、@白鱼余余丶</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>40x6/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=#3D1400>①</color><color=red>每次攻击有2%概率出发大招，5秒内，每0.02秒散射3发随机子弹（原始豌豆，原始石块豌豆）</color>\n" +
                "<color=#3D1400>②</color><color=red>子弹直击目标造成击退，发射的子弹有50%概率改为原始石块豌豆</color>\n" +
                "<color=#3D1400>原始石块豌豆：</color><color=#3D1400>①</color><color=red>造成半径1格无衰减溅射并击退，击中的目标有50%概率造成1秒定身</color>\n" +
                "<color=#3D1400>②</color><color=red>可经过超级火炬，变为熔岩子弹，伤害x2，再次经过究极火炬时x4</color>\n" +
                "<color=#3D1400>词条1:</color><color=red>五阶升级：原始超级机枪射手的攻击力x10，石块子弹发射概率提升至100%，击中目标有0.5%概率击飞，造成秒杀伤害</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>超级机枪射手+原始豌豆射手</color>\n\n" +
                "<color=#3D1400>原始超级机枪射手在与超级机枪射手的系统学习下，学习了以致密的火力横扫了一片区域，像极了当年的青年雄狮。原始超级机枪射手整了整衣领，“我太爱这种感觉了！”</color>"
            );
            CustomCore.AddUltimatePlant(PrimitiveSuperGatling.PlantID);
            #endregion
        }
    }

    public class PrimitiveShooter : MonoBehaviour
    {
        public static ID PlantID = 1935;
        public static ID ParticleID = 2000;
        public static ID FireParticleID = 2001;
        public static List<Zombie> ZombiesInTimer = new();

        public void Awake()
        {
            plant.shoot = transform.FindChild("PeaShooter_Head/Shoot");
        }

        public static IEnumerator SetTimer(Zombie zombie)
        {
            if (!zombie.IsObjExist()) yield break;
            if (zombie.timers.TryGetValue((ZombieTimer)(int)PlantID, out var time) && time > 0f) yield break;
            // 如果是二爷掉报纸或者阿尔法假死
            if (zombie.theStatus == ZombieStatus.Paper_losePaper || zombie.theStatus == ZombieStatus.Legion_fall) yield break;
            ZombiesInTimer = ZombiesInTimer.Where(z => z.IsObjExist()).ToList(); // 去除Null元素
            if (ZombiesInTimer.Contains(zombie)) yield break;
            ZombiesInTimer.Add(zombie);
            zombie.timers[(ZombieTimer)(int)PlantID] = 1f;
            var origin = 0f; // 实际要设置的值，在交换后就变成了原来的速度
            (origin, zombie.theOriginSpeed) = (zombie.theOriginSpeed, origin);
            yield return new WaitForSeconds(1f);
            if (!zombie.IsObjExist()) yield break;
            (origin, zombie.theOriginSpeed) = (zombie.theOriginSpeed, origin);
            zombie.timers[(ZombieTimer)(int)PlantID] = 0f;
            ZombiesInTimer.Remove(zombie);
            yield break;
        }

        public PeaShooter plant => gameObject.GetComponent<PeaShooter>();
    }

    public class PrimitiveSuperGatling : MonoBehaviour
    {
        public static ID PlantID = 1936;

        public void Awake()
        {
            plant.shoot = transform.FindChild("GatlingPea_head/Shoot");
        }

        public static IEnumerator KillZombie(Zombie z)
        {
            if (!z.IsObjExist()) yield break;
            var deadRight = z.deadRight;
            z.enabled = false;
            z.anim.enabled = false;
            z.col.enabled = false;
            var angle = -720f;
            var speed = new Vector3(UnityEngine.Random.Range(12f, 14f), UnityEngine.Random.Range(1f, 3f)); // 速度向量
            var startCenter = z.gameObject.GetCenterLocalSprite();
            while (z.IsObjExist() && z.axis.position.x <= deadRight)
            {
                z.transform.position += speed * Time.deltaTime;
                var center = z.transform.TransformPoint(startCenter);
                z.transform.RotateAround(center, new Vector3(0f, 0f, 1f), angle * Time.deltaTime);
                yield return null; // 等待一帧
            }
            if (!z.IsObjExist()) yield break;
            z.enabled = true;
            z.Die(2);
            yield break;
        }

        public SuperGatling plant => gameObject.GetComponent<SuperGatling>();
    }

    public class Bullet_primitivePea : MonoBehaviour // 75%的普通子弹
    {
        public static ID BulletID = 2000;
        public static ID FireID = 2001;
        public static ID OrangeFireID = 2002;
        public static ID RedFireID = 2003;
        public static List<BulletType> BulletTypes = new()
        {
            BulletID,
            FireID,
            OrangeFireID,
            RedFireID
        };

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.IsObjExist()) return;
            if (!collision.TryGetComponent<Plant>(out var plant)) return;
            if (!plant.IsObjExist()) return;
            if (GetNextLevel(plant) == BulletType.Bullet_pea) return;
            if (bullet.torchWood == plant) return;
            if (GetNextLevel(plant) == bullet.theBulletType) return;
            var next = GetNextLevel(plant);
            var addDamage = GetAddDamgage(next);
            bullet.board.boardAction.FirePeas(bullet, plant, addDamage, next);
        }

        public BulletType GetNextLevel(Plant p)
        {
            if (IsSuperTorch(p)) return RedFireID;
            else if (p.thePlantType == PlantType.JalaTorch) return OrangeFireID;
            else if (p.thePlantType == PlantType.TorchWood) return FireID;
            return BulletType.Bullet_pea;
        }

        public int GetAddDamgage(BulletType bulletType)
        {
            if (bulletType == FireID) return 40;
            else if (bulletType == OrangeFireID) return 50;
            else if (bulletType == RedFireID) return 60;
            return 0;
        }

        public bool IsSuperTorch(Plant p) => p.TryGetComponent<SuperTorch>(out var _) || p.TryGetComponent<UltimateTorch>(out var _) || 
            p.TryGetComponent<UltimateStarTorch>(out var _);

        public Bullet_pea bullet => gameObject.GetComponent<Bullet_pea>();
    }

    public class Bullet_stonePea : MonoBehaviour // 25%的特殊子弹
    {
        public static ID BulletID = 2004;
        public static ID FireID = 2005;
        public static ID RedFireID = 2006;
        public static List<BulletType> BulletTypes = new()
        {
            BulletID,
            FireID,
            RedFireID
        };

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.IsObjExist()) return;
            if (!collision.TryGetComponent<Plant>(out var plant)) return;
            if (!plant.IsObjExist()) return;
            if (GetNextLevel(plant) == BulletType.Bullet_pea) return;
            if (bullet.torchWood == plant) return;
            if (GetNextLevel(plant) == bullet.theBulletType) return;
            var next = GetNextLevel(plant);
            bullet.board.boardAction.FirePeas(bullet, plant, bullet.Damage, next);
        }

        public BulletType GetNextLevel(Plant p)
        {
            if (IsSuperTorch(p))
            {
                if (bullet.theBulletType == BulletID) return FireID;
                else if (bullet.theBulletType == FireID)
                {
                    if (p != null && p.thePlantType == PlantType.SuperTorch) return FireID;
                    else return RedFireID;
                }
            }
            return BulletType.Bullet_pea;
        }

        public bool IsSuperTorch(Plant p) => p.TryGetComponent<SuperTorch>(out var _) || p.TryGetComponent<UltimateTorch>(out var _) ||
            p.TryGetComponent<UltimateStarTorch>(out var _);

        public Bullet_pea bullet => gameObject.GetComponent<Bullet_pea>();
    }

    [HarmonyPatch(typeof(PeaShooter))]
    public static class PeaShooterPatch
    {
        [HarmonyPatch(nameof(PeaShooter.Shoot1))]
        [HarmonyPrefix]
        public static bool PostGetBulletType(Shooter __instance)
        {
            if (__instance.thePlantType == PrimitiveShooter.PlantID)
            {
                var bulletType = Bullet_primitivePea.BulletID;
                if (UnityEngine.Random.Range(0, 2) == 0) bulletType = Bullet_stonePea.BulletID;
                var bullet = CreateBullet.Instance.SetBullet(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow,
                    bulletType, BulletMoveWay.MoveRight);

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;
                GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1f);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_pea))]
    public static class Bullet_peaPatch
    {
        [HarmonyPatch(nameof(Bullet_pea.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_pea __instance, ref Zombie zombie)
        {
            if (Bullet_primitivePea.BulletTypes.Contains(__instance.theBulletType)) // 普通
            {
                if (__instance.theBulletType == Bullet_primitivePea.BulletID)
                {
                    zombie.TakeDamage(__instance.Damage, __instance, DamageType.Normal, __instance.fromType);
                    ParticleManager.Instance.SetParticle(0, __instance.transform.position, zombie.theZombieRow);
                    zombie.KnockBack(0.3f, Zombie.KnockBackReason.Normal);
                    __instance.PlaySound(zombie);
                    __instance.Die();
                }
                else
                {
                    zombie.KnockBack(0.3f, Zombie.KnockBackReason.Normal);
                    __instance.FireZombie(zombie);
                }
                return false;
            }
            if (Bullet_stonePea.BulletTypes.Contains(__instance.theBulletType)) // 特殊
            {
                if (__instance.theBulletType == Bullet_stonePea.BulletID)
                    CreateParticle.SetParticle(PrimitiveShooter.ParticleID, __instance.transform.position, zombie.theZombieRow);
                else
                    CreateParticle.SetParticle(PrimitiveShooter.FireParticleID, __instance.transform.position, zombie.theZombieRow);
                foreach (var col in Physics2D.OverlapCircleAll(__instance.transform.position, 0.5f * CoreTools.ColumnX, GameInfo.zombieLayer))
                {
                    if (!col.IsObjExist()) continue;
                    if (!col.TryGetComponent<Zombie>(out var z)) continue;
                    if (!z.IsObjExist()) continue;
                    z.TakeDamage(__instance.Damage, __instance, DamageType.Normal, __instance.fromType);
                    z.KnockBack(0.3f, Zombie.KnockBackReason.Normal);
                    if (!TypeMgr.IsBossZombie(z.theZombieType) && UnityEngine.Random.Range(0, 2) == 0) // 如果不是领袖僵尸并且随到50%
                        z.StartCoroutine(PrimitiveShooter.SetTimer(z)); // 触发定身
                }
                __instance.PlaySound(zombie);

                if (CoreTools.TravelAdvanced("五阶升级") && __instance.fromType == PrimitiveSuperGatling.PlantID)
                {
                    if (UnityEngine.Random.Range(0, 200) == 0) // 1/200 = 0.5%
                    {
                        if (!TypeMgr.IsBossZombie(zombie.theZombieType) && zombie.theZombieType != ZombieType.TrainingDummy)
                            zombie.StartCoroutine(PrimitiveSuperGatling.KillZombie(zombie));
                        else
                            zombie.TakeDamage(100_0000, __instance, DamageType.MaxDamage, __instance.fromType);
                    }
                }

                __instance.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SuperGatling))]
    public static class SuperGatlingPatch
    {
        [HarmonyPatch(nameof(SuperGatling.GetBulletType))]
        [HarmonyPostfix]
        public static void PostGetBulletType(SuperGatling __instance, ref BulletType __result)
        {
            if (__instance.thePlantType == PrimitiveSuperGatling.PlantID)
            {
                if (!CoreTools.TravelAdvanced("五阶升级"))
                {
                    switch (UnityEngine.Random.Range(0, 2))
                    {
                        case 0: __result = Bullet_stonePea.BulletID; break;
                        default: __result = Bullet_primitivePea.BulletID; break;
                    }
                }
                else __result = Bullet_stonePea.BulletID;
            }
        }
    }
}
