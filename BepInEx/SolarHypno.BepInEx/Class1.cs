using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace SolarHypno.BepInEx
{
    [BepInPlugin("salmon.solarhypno", "SolarHypno", "1.0")]
    public class Core : BasePlugin//960
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<SolarHypno>();
            ClassInjector.RegisterTypeInIl2Cpp<UltimateJumpSun>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "solarhypno");
            CustomCore.RegisterCustomPlant<SolarCabbage, SolarHypno>((int)SolarHypno.PlantID, ab.GetAsset<GameObject>("SolarHypnoPrefab").SetSaveMaterial(),
                ab.GetAsset<GameObject>("SolarHypnoPreview").SetSaveMaterial(), new List<(int, int)>
                {
                    ((int)PlantType.UltimateCabbage, (int)PlantType.HypnoShroom),
                    ((int)PlantType.HypnoShroom, (int)PlantType.UltimateCabbage)
                }, 2f, 0f, 100, 300, 7.5f, 450);
            CustomCore.RegisterCustomPlantSkin<SolarCabbage, SolarHypno>((int)SolarHypno.PlantID, ab.GetAsset<GameObject>("SolarHypnoSkinPrefab").SetSaveMaterial(),
                ab.GetAsset<GameObject>("SolarHypnoSkinPreview").SetSaveMaterial(), new List<(int, int)>
                {
                    ((int)PlantType.UltimateCabbage, (int)PlantType.HypnoShroom),
                    ((int)PlantType.HypnoShroom, (int)PlantType.UltimateCabbage)
                }, 2f, 0f, 100, 300, 7.5f, 450);
            CustomCore.AddPlantAlmanacStrings((int)SolarHypno.PlantID, $"究极太阳神魅惑菇({(int)SolarHypno.PlantID})",
                "光明天降，普度众生。\n" +
                "<color=#0000FF>究极太阳神卷心菜同人亚种</color>\n\n" +
                "<color=#3D1400>贴图作者：@方染Fran、@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转化配方：</color><color=red>卷心菜←→魅惑菇</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>100x5/2秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①被啃咬时魅惑啃咬者（5次后消失）\n" +
                "②魅惑或击中僵尸时产生弹力阳光\n" +
                "③攻击有概率魅惑血量低于50%的僵尸，概率最高为25%\n" +
                "④每30秒1次，召唤太阳</color>\n" +
                "<color=#3D1400>弹力阳光：</color><color=red>①命中目标时，造成（子弹伤害x2）的伤害并掉落25阳光，如果目标是魅惑僵尸，则最高造成200伤害并掉落50阳光\n" +
                "②命中目标后，立刻弹向新的目标，最多15次，消失后掉落25阳光</color>\n" +
                "<color=#3D1400>大招：</color><color=red>消耗1000金钱，召唤太阳</color>\n" +
                "<color=#3D1400>词条1：</color><color=red>金光闪闪：究极太阳神魅惑菇的子弹会消耗超过15000阳光部分的0.5%阳光，使该子弹的增加（20x消耗阳光）的伤害</color>\n" +
                "<color=#3D1400>词条2：</color><color=red>人造太阳：太阳伤害x3</color>\n" +
                "<color=#3D1400>连携词条：</color><color=red>星神合一：究极杨桃大帝（及其亚种）与究极太阳神卷心菜（及究极太阳神魅惑菇）的数量均不小于10时：太阳持续时间无限且伤害x5，固定每3秒召唤太阳神流星，伤害为1800×(1+大帝数量)×(1+0.5×太阳神数量)，分裂出180个伤害400的子弹，并掉落1250阳光</color>\n\n" +
                "<color=#3D1400>" +
                "月如盘，映辉光，炽阳降世显威光。\n" +
                "时如梭，驹过隙，光阴似箭倏瞬息。\n" +
                "心如莲，化淤泥，清雅绝尘去污秽。\n身如铁，抵万军，万夫莫开吾当关。\n" +
                "—《朱霞之歌》</color>");
            CustomCore.RegisterSuperSkill((int)SolarHypno.PlantID, (p) => 1000, (plant) =>
            {
                GameAPP.PlaySound(66, 0.5f, 1.0f);

                plant.flashCountDown = 2f;

                if (Solar.Instance != null)
                {
                    if (Solar.Instance.deathTime >= 0f)
                    {
                        Solar.Instance.deathTime += 15f;
                        return;
                    }
                }
                if (plant.board != null)
                {
                    plant.board.solarCountDown = Mathf.Min(plant.board.solarCountDown, 0.5f);
                    plant.board.CreateSolar();
                    Solar.Instance.deathTime = 15f;
                }
            });
            CustomCore.RegisterCustomBullet<Bullet_cabbage>(SolarHypno.BulletID, ab.GetAsset<GameObject>("Bullet_HypnoSunCabbage"));
            CustomCore.RegisterCustomBullet<Bullet_cabbage>(SolarHypno.BulletSkinID, ab.GetAsset<GameObject>("Bullet_HypnoSunCabbageSkin"));
            SolarHypno.SmallSun = ab.GetAsset<GameObject>("SmallSun");
            SolarHypno.SmallSun.AddComponent<UltimateJumpSun>();
            SolarHypno.SmallSunSkin = ab.GetAsset<GameObject>("SmallSunSkin");
            SolarHypno.SmallSunSkin.AddComponent<UltimateJumpSun>();
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, (int)SolarHypno.PlantID, (int)PlantType.Cabbagepult);
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, (int)PlantType.Cabbagepult, (int)SolarHypno.PlantID);
            CustomCore.AddUltimatePlant(SolarHypno.PlantID);
        }
    }

    public class SolarHypno : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1954;
        public static BulletType BulletID = (BulletType)1954;
        public static BulletType BulletSkinID = (BulletType)1955;

        public static GameObject SmallSun;
        public static GameObject SmallSunSkin;

        public SolarCabbage plant => gameObject.GetComponent<SolarCabbage>();

        public void Awake()
        {
            if (plant == null) return;
            plant.shoot = transform.FindChild("Back/Shoot");
            plant.attributeCount = 5;
        }

        public void Update()
        {
            if (plant == null) return;
            if (plant.attributeCount <= 0) plant.Die();
            if (plant.board == null) return;
            if (plant.board.solarCountDown <= 0f)
            {
                if (Solar.Instance == null)
                {
                    plant.anim.SetTrigger("super");
                    plant.board.CreateSolar();
                    Solar.Instance.deathTime = 30f;
                }
                else
                {
                    Solar.Instance.deathTime += 15f;
                }
                plant.board.solarCountDown = plant.board.solarMaxTime;
            }
        }
    }

    public class UltimateJumpSun : MonoBehaviour
    {
        public Board board;
        public int live = 15;
        public int damage = 300;
        public List<Zombie> list;
        public float speed = 4.0f;
        public bool dying = false;

        public void Start()
        {
            board = Board.Instance;
            if (board == null) Destroy(gameObject);

            list = board.zombieArray.ToSystemList().Where(z => z != null).OrderByDescending(z => z.isMindControlled).ToList();
            gameObject.GetComponent<Animator>().SetFloat("Speed", 2f);
        }

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            for (int i = list.Count - 1; i >= 0; i--) if (list[i] == null) list.RemoveAt(i);
            if (list.Count <= 0)
            {
                Die();
                return;
            }

            var zombie = list[0];
            if (zombie == null) return;

            var direction = ((zombie.axis.transform.position + new Vector3(0f, 0.5f, 0f)) - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, zombie.axis.transform.position + new Vector3(0f, 0.5f, 0f)) < 0.1f)
            {
                AttackZombie(zombie);
                list.RemoveAt(0);
                live--;
                if (live <= 0) Die();
            }
        }

        public void AttackZombie(Zombie zombie)
        {
            if (zombie == null) return;
            if (zombie.isMindControlled)
            {
                CreateItem.Instance.SetCoin(0, 0, 1, 0, (zombie.axis.transform.position + new Vector3(0f, 0.5f, 0f)));
                zombie.TakeDamage(DmgType.Normal, 200);
            }
            else
            {
                CreateItem.Instance.SetCoin(0, 0, 0, 0, (zombie.axis.transform.position + new Vector3(0f, 0.5f, 0f)));
                zombie.TakeDamage(DmgType.Normal, damage);
            }

            // 播放粒子特效
            ParticleManager.Instance.SetParticle(
                ParticleType.Splat_sun,
                new Vector2(transform.position.x, transform.position.y),
                zombie.theZombieRow, true
            );
        }

        public void Die()
        {
            if (dying) return;
            dying = true;
            CreateItem.Instance.SetCoin(0, 0, 0, 0, transform.position);
            Destroy(gameObject);
        }
    }

    [HarmonyPatch(typeof(SolarCabbage))]
    public static class SolarCabbage_Patch
    {
        [HarmonyPatch(nameof(SolarCabbage.GetBulletType))]
        [HarmonyPostfix]
        public static void Postfix(SolarCabbage __instance, ref BulletType __result)
        {
            if (__instance.thePlantType == SolarHypno.PlantID)
            {
                if (__instance.name.Contains("Skin"))
                    __result = SolarHypno.BulletSkinID;
                else
                    __result = SolarHypno.BulletID;
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_cabbage))]
    public static class Bullet_cabbage_Patch
    {
        [HarmonyPatch(nameof(Bullet_cabbage.HitZombie))]
        [HarmonyPrefix]
        public static bool Prefix(Bullet_cabbage __instance, ref Zombie zombie)
        {
            if (__instance != null && zombie != null && (__instance.theBulletType == SolarHypno.BulletID || __instance.theBulletType == SolarHypno.BulletSkinID))
            {
                var totalHealth = zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;
                var totalMaxHealth = zombie.theMaxHealth + zombie.theFirstArmorMaxHealth + zombie.theSecondArmorMaxHealth;
                if (totalHealth < totalMaxHealth / 2 && UnityEngine.Random.Range(1, 100) <= UnityEngine.Random.Range(1, 25))
                {
                    zombie.SetMindControl();
                }
                zombie.TakeDamage(DmgType.Normal, __instance.Damage);
                __instance.PlaySound(zombie);
                CreateItem.Instance.SetCoin(
                    0,          // 猜测的参数
                    0,          // 猜测的参数
                    13,         // 硬币类型（13号硬币）
                    0,          // 猜测的参数
                    __instance.transform.position
                );

                if (__instance.from != null && __instance.from.TryGetComponent<SolarHypno>(out var hypno) && hypno != null)
                {
                    if (__instance.from.name.Contains("Skin"))
                    {
                        var sun = UnityEngine.Object.Instantiate(SolarHypno.SmallSunSkin, __instance.transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity,
                            __instance.from.board.transform).GetComponent<UltimateJumpSun>();
                        sun.damage = __instance.Damage * 2;
                    }
                    else
                    {
                        var sun = UnityEngine.Object.Instantiate(SolarHypno.SmallSun, __instance.transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity,
                            __instance.from.board.transform).GetComponent<UltimateJumpSun>();
                        sun.damage = __instance.Damage * 2;
                    }
                }
                __instance.Die();
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Bullet_cabbage.HitLand))]
        [HarmonyPrefix]
        public static bool PreHitLand(Bullet_cabbage __instance)
        {
            if (__instance != null && (__instance.theBulletType == SolarHypno.BulletID || __instance.theBulletType == SolarHypno.BulletSkinID))
            {
                CreateItem.Instance.SetCoin(
                    0,          // 猜测的参数
                    0,          // 猜测的参数
                    13,         // 硬币类型（13号硬币）
                    0,          // 猜测的参数
                    __instance.transform.position
                );
                __instance.hit = true;
                GameAPP.PlaySound(UnityEngine.Random.Range(0, 3), 0.5f, 1);
                __instance.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Solar.__c), nameof(Solar.__c._SetDamage_b__12_1))]
    public static class SolarMethodPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Plant p, ref bool __result)
        {
            if (p != null && p.thePlantType == SolarHypno.PlantID)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(Thrower), nameof(Thrower.Shoot1))]
    public static class Thrower_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Thrower __instance, ref Bullet __result)
        {
            if (__instance != null && __instance.thePlantType == SolarHypno.PlantID && __result != null)
                __result.from = __instance;
        }
    }

    [HarmonyPatch(typeof(Zombie), nameof(Zombie.PlayEatSound))]
    public static class Zombie_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Zombie __instance)
        {
            if (__instance != null && __instance.theAttackTarget != null && __instance.theAttackTarget.IsPlant(out var plant) &&
                plant != null && plant.thePlantType == SolarHypno.PlantID && plant.attributeCount > 0)
            {
                plant.attributeCount--;
                __instance.SetMindControl();
                if (plant.name.Contains("Skin"))
                {
                    var sun = UnityEngine.Object.Instantiate(SolarHypno.SmallSunSkin, plant.axis.transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity,
                        plant.board.transform).GetComponent<UltimateJumpSun>();
                    sun.damage = plant.attackDamage;
                }
                else
                {
                    var sun = UnityEngine.Object.Instantiate(SolarHypno.SmallSun, plant.axis.transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity,
                        plant.board.transform).GetComponent<UltimateJumpSun>();
                    sun.damage = plant.attackDamage;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(UltimateFootballZombie), nameof(UltimateFootballZombie.AttackEffect))]
    public static class UltimateFootballZombie_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Plant plant)
        {
            if (plant != null && plant.thePlantType == SolarHypno.PlantID)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Lawnf))]
    public static class LawnfPatch
    {
        [HarmonyPatch(nameof(Lawnf.GetPlantCount), new Type[] { typeof(PlantType), typeof(Board) })]
        public static void Postfix(ref PlantType theSeedType, ref Board board, ref int __result)
        {
            if (theSeedType == PlantType.UltimateCabbage)
                __result += Lawnf.GetPlantCount(SolarHypno.PlantID, board);
        }
    }
}
