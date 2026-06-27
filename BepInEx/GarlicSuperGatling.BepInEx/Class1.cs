using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using Unity.VisualScripting;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Unity;

namespace GarlicSuperGatling.BepInEx
{
    [BepInPlugin("salmon.garlicsupergatling", "GarlicSuperGatling", "1.0")]
    public class Core : BasePlugin
    {
        public static int ParticleID = 500;
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_pea_garlic>();
            ClassInjector.RegisterTypeInIl2Cpp<GarlicSuperGatling>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "garlicsupergatling");
            // CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_pea_garlic>((BulletType)Bullet_pea_garlic.BulletID, ab.GetAsset<GameObject>("Bullet_pea_garlic"));
            CustomCore.RegisterCustomBullet<Bullet_pea_garlic>(Bullet_garlicPea_super.BulletID, ab.GetAsset<GameObject>("Bullet_garlicPea_super"));
            // CustomCore.RegisterCustomParticle((ParticleType)ParticleID, ab.GetAsset<GameObject>("GarlicPeaSplat"));
            CustomCore.RegisterCustomPlant<SuperSnowGatling, GarlicSuperGatling>(GarlicSuperGatling.PlantID, ab.GetAsset<GameObject>("GarlicSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("GarlicSuperGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.Garlic),
                    ((int)PlantType.GarlicSniper, (int)PlantType.Peashooter)
                }, 1.5f, 0f, 30, 300, 7.5f, 650);
            CustomCore.AddPlantAlmanacStrings(GarlicSuperGatling.PlantID, $"超级病毒机枪射手({GarlicSuperGatling.PlantID})",
                "一次发射六颗病毒豌豆，有概率一次性发射大量病毒豌豆。\n\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>30x6/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①子弹命中时赋予1蒜值和1毒素。如果目标处于中毒状态，在其位置生成一个剧毒毒瘴\n" +
                "②毒素上限为5层。若目标拥有5层毒素时，则清空全部毒素，造成一次300伤害的爆炸，在其位置生成一个剧毒毒瘴\n" +
                "③每次攻击有2%概率触发大招，5秒内，每0.02秒散射3发蒜豌豆，命中后造成300伤害的爆炸，在其位置生成一个剧毒毒瘴\n" +
                "④可以和病毒狙击射手互相转化</color>\n" +
                "<color=#3D1400>剧毒毒瘴：</color><color=red>①会在场上无规则移动\n" +
                "②半径0.74格，持续10秒。每1秒，赋予范围内的僵尸1点蒜值\n" +
                "③赋予蒜值时，造成（子弹攻击力x蒜值x0.1）的伤害，如果目标免疫蒜毒，则造成300点伤害\n" +
                "④如果场上的剧毒毒瘴数量达到30个，生成剧毒毒瘴时，改为使最近的剧毒毒瘴增加范围和2%伤害比例</color>\n" +
                "<color=#3D1400>词条1:</color><color=red>五阶升级：超级病毒机枪射手的攻击力x10，剧毒毒瘴将赋予蒜黄油效果</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>超级机枪射手+大蒜</color>\n" +
                "<color=#3D1400>转化配方：</color><color=red>豌豆射手←→豌豆射手</color>\n\n" +
                "<color=#3D1400>永远不要招惹一位退役的老兵，尤其是特立独行的家伙。虽然不知道是怎么传播开来的，但是招惹它的僵尸基本都后悔了。</color>");
            CustomCore.AddUltimatePlant((PlantType)GarlicSuperGatling.PlantID);
            CustomCore.AddFusion(PlantType.GarlicSniper, (PlantType)GarlicSuperGatling.PlantID, PlantType.Peashooter);
            // 资源初始化
            ClassInjector.RegisterTypeInIl2Cpp<PoisonMiasma>();
            PoisonMiasma.MiasmaObj = ab.GetAsset<GameObject>("PoisonMiasma");
            PoisonMiasma.MiasmaObj.AddComponent<PoisonMiasma>();
            PoisonMiasma.MiasmaObj.transform.SetLayer("fog");
        }
    }

    public class GarlicSuperGatling : MonoBehaviour
    {
        public static ID PlantID = 1918;

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.GetChild(0).FindChild("Shoot");
        }

        public SuperSnowGatling plant => gameObject.GetComponent<SuperSnowGatling>();
    }

    public class PoisonMiasma : MonoBehaviour
    {
        public static GameObject MiasmaObj;
        public static List<PoisonMiasma> miasmas = new();

        public float range = 1f;
        public float dmgMultiplier = 1f;
        public LayerMask zombieLayer;
        public float attackTimer = 1f; // 攻击间隔1s
        public float liveTimer = 0f; // 存活时间正计时，上限10s
        public int damage = 30;

        public void Awake()
        {
            zombieLayer = LayerMask.GetMask("Zombie");
        }

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                attackTimer = 1f; // 重置攻击CD
                foreach (var col in Physics2D.OverlapCircleAll(transform.position, range, zombieLayer))
                {
                    if (!col.IsObjExist() || !col.TryGetComponent<Zombie>(out var zombie) || !zombie.IsObjExist()) continue;
                    if (!Lawnf.InLandStatus(zombie.theStatus)) continue;
                    zombie.AddPoisonLevel();
                    if (zombie.poisonLevel > 0)
                        zombie.TakeDamage(damage * zombie.poisonLevel / 10, null, DamageType.Shieldless, GarlicSuperGatling.PlantID);
                    else
                        zombie.TakeDamage(300, null, DamageType.Shieldless, GarlicSuperGatling.PlantID);
                }
            }
            // 随机移动
            var pos = new Vector3(UnityEngine.Random.Range(-1f, 1f) * Time.deltaTime, UnityEngine.Random.Range(-1f, 1f) * Time.deltaTime);
            transform.position += pos;
            // 存活时间判定
            liveTimer += Time.deltaTime;
            if (liveTimer >= 10f)
                Destroy(gameObject);
        }

        public void OnDestroy()
        {
            if (miasmas.Contains(this))
                miasmas.Remove(this);
        }

        public static PoisonMiasma SetMiasma(Vector2 pos, int damage, int row, Board board)
        {
            if (miasmas.Count > 30)
            {
                var nearest = miasmas.OrderBy(m => Vector2.Distance(pos, m.transform.position)).FirstOrDefault();
                if (nearest != null)
                {
                    nearest.range += 0.1f;
                    nearest.dmgMultiplier += 0.02f;
                    return nearest;
                }
            }
            var miasma = Instantiate(MiasmaObj, pos, Quaternion.identity, board.transform).GetComponent<PoisonMiasma>();
            miasmas.Add(miasma);
            return miasma;
        }
    }

    public class Bullet_garlicPea_skin : MonoBehaviour // 实现过火逻辑
    {
        public static ID BulletSkinID = 11900;
    }

    public class Bullet_garlicPea_fire_skin : MonoBehaviour
    {
        public static ID BulletSkinID = 11901;
    }

    public class Bullet_garlicPea_super : MonoBehaviour // 实现过火逻辑
    {
        public static ID BulletID = 11902;
        public static ID BulletSkinID = 11903;
    }

    public class Bullet_garlicPea_fire_super : MonoBehaviour
    {
        public static ID BulletID = 11904;
        public static ID BulletSkinID = 11905;
    }

    [HarmonyPatch(typeof(Zombie), nameof(Zombie.EatEffect))]
    public class Zombie_EatEffect
    {
        [HarmonyPrefix]
        public static bool Prefix(Zombie __instance, ref Plant plant, ref bool __result)
        {
            if (__instance != null && plant != null && !TypeMgr.IsDriverZombie(__instance.theZombieType) && !__instance.isChangingRow)
            {
                if ((int)plant.thePlantType == GarlicSuperGatling.PlantID)
                {
                    __instance.EatGarlic(plant);
                    __instance.eatGarlic = true;
                    __instance.garlicSpeed = 0f;
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SuperSnowGatling))]
    public class SuperSnowGatlingPatch
    {
        [HarmonyPatch(nameof(SuperSnowGatling.TakeDamage))]
        [HarmonyPrefix]
        public static void Prefix(SuperSnowGatling __instance, ref int damage, ref DamageType damageType)
        {
            if ((int)__instance.thePlantType == GarlicSuperGatling.PlantID && damageType == DamageType.Normal)
            {
                if (__instance.PotType == PlantType.GarlicPot)
                    damage = 10;
                else
                    damage = 15;
            }
        }

        [HarmonyPatch(nameof(SuperSnowGatling.GetBulletType))]
        [HarmonyPostfix]
        public static void PostGetBulletType(SuperSnowGatling __instance, ref BulletType __result)
        {
            if (__instance.thePlantType == GarlicSuperGatling.PlantID)
            {
                if (__instance.timer <= 0f)
                    __result = BulletType.Bullet_pea_garlic;
                else
                    __result = Bullet_garlicPea_super.BulletID;
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_pea_garlic))]
    public static class Bullet_pea_garlicPatch
    {
        [HarmonyPatch(nameof(Bullet_pea_garlic.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_pea_garlic __instance, ref Zombie zombie)
        {
            if (__instance.fromType == GarlicSuperGatling.PlantID)
            {
                __instance.PlaySound(zombie);
                zombie.AddToxin(1);
                if (zombie != null && ParticleManager.Instance != null)
                    ParticleManager.Instance.SetParticle(ParticleType.Splat_white, __instance.transform.position, zombie.theZombieRow);

                zombie.TakeDamage(__instance.Damage, __instance, DamageType.Normal, __instance.fromType);
                zombie.AddPoisonLevel();

                if (zombie.GetToxin() >= 5)
                {
                    // 生成爆炸 r=1
                    foreach (var col in Physics2D.OverlapCircleAll(__instance.transform.position, CoreTools.ColumnX, __instance.zombieLayer))
                    {
                        if (!col.IsObjExist()) continue;
                        if (!col.TryGetComponent<Zombie>(out var z) || !z.IsObjExist()) continue;
                        z.TakeDamage(300, __instance, DamageType.NormalAll, __instance.fromType);
                    }
                    PoisonMiasma.SetMiasma(zombie.axis.transform.position, __instance.Damage, zombie.theZombieRow, __instance.board);
                    zombie.SetToxin(0);
                }

                if (__instance.theBulletType == Bullet_garlicPea_super.BulletID)
                {
                    // 生成爆炸 r=1
                    foreach (var col in Physics2D.OverlapCircleAll(__instance.transform.position, CoreTools.ColumnX, __instance.zombieLayer))
                    {
                        if (!col.IsObjExist()) continue;
                        if (!col.TryGetComponent<Zombie>(out var z) || !z.IsObjExist()) continue;
                        z.TakeDamage(300, __instance, DamageType.NormalAll, __instance.fromType);
                    }
                    PoisonMiasma.SetMiasma(zombie.axis.transform.position, __instance.Damage, zombie.theZombieRow, __instance.board);
                }
                if (zombie.HasBuff(EffectType.Poison))
                    PoisonMiasma.SetMiasma(zombie.axis.transform.position, __instance.Damage, zombie.theZombieRow, __instance.board);

                __instance.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_firePea_garlic))]
    public static class Bullet_firePea_garlicPatch
    {
        [HarmonyPatch(nameof(Bullet_firePea_garlic.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_firePea_garlic __instance, ref Zombie zombie)
        {
            if (__instance.fromType == GarlicSuperGatling.PlantID)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(UltimateFootballZombie), nameof(UltimateFootballZombie.AttackEffect))]
    public class UltimateFootballZombie_TakeDamage
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Plant plant)
        {
            if (plant != null && (int)plant.thePlantType == GarlicSuperGatling.PlantID)
            {
                return false;
            }
            return true;
        }
    }

    public static class ZombieGarlicExtension
    {
        public static void AddToxin(this Zombie zombie, int level) => zombie.GetData<int>("GarlicSuperGatling_Toxin").val += level;
        public static void SetToxin(this Zombie zombie, int level) => zombie.GetData<int>("GarlicSuperGatling_Toxin").val = level;

        public static int GetToxin(this Zombie zombie) => zombie.GetData<int>("GarlicSuperGatling_Toxin").val;
    }
}