using BepInEx;
using BepInEx.Unity.IL2CPP;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UI;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace RogueShootingIFVStar.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.ifvstar", "RogueShootingIFVStar", "1.0.0")]
    public class Core : BasePlugin
    {
        public static (ParticleType type, GameObject obj) theNewCherry = ((ParticleType)976, null!);
        public static (ParticleType type, GameObject obj) theNewDoom = ((ParticleType)977, null!);

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_JackboxStar>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IFVStar>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IFVStar.UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IFVStar.AttractBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IFVStar.DoomBuff>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<IFVStarShooting>();
            ClassInjector.RegisterTypeInIl2Cpp<ShootingIFVStarBullet>();

            foreach (var item in GetAssetBundle("ifvstar").LoadAllAssetsAsync().allAssets)
            {
                if (item.TryCast<GameObject>()?.name == "BombCloudSmall")
                {
                    theNewCherry.obj = item.Cast<GameObject>();
                }
                if (item.TryCast<GameObject>()?.name == "Doom_magnet")
                {
                    theNewDoom.obj = item.Cast<GameObject>();
                    theNewDoom.obj.AddComponent<Doom>();
                }
            }
        }

        private static AssetBundle GetAssetBundle(string name)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using Stream stream =
                    assembly.GetManifestResourceStream(assembly.FullName!.Split(",")[0] + "." + name) ??
                    assembly.GetManifestResourceStream(name)!;
                using MemoryStream stream1 = new();
                stream.CopyTo(stream1);
                var ab = AssetBundle.LoadFromMemory(stream1.ToArray());
                ArgumentNullException.ThrowIfNull(ab);
                return ab;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to load {name} \n{e}");
            }
        }

        internal static void CreateCherryExplodeCustom(Vector2 v, int theRow, int damage = 1800, PlantType fromType = PlantType.Nothing)
        {
            CreateParticle.SetParticle((int)theNewCherry.type, v, theRow);
            ScreenShake.TriggerShake(0.15f);
            GameAPP.PlaySound(40, 0.2f, 1.0f);
            Action<Zombie> action = (z) =>
            {
                z.TakeDamage(z.theFirstArmorHealth, null, DamageType.Normal, fromType);
                z.TakeDamage(z.theSecondArmorHealth, null, DamageType.Normal, fromType);
            };
            var bomb = new BombCherry
            {
                bombPosition = v,
                damageToZombie = damage,
                bombRow = theRow,
                bombType = CherryBombType.Bullet,
                fromType = fromType,
                bulletFromZombie = false,
                zombieAction = action
            };

            bomb.Explode(null);
        }

        internal static void SetDoomCustom(Board board, Vector2 position, DoomType doomType)
        {
            var doom = UnityEngine.Object.Instantiate(theNewDoom.obj.transform, position, Quaternion.identity, 
                board.transform).GetComponent<Doom>();
            doom.doomType = doomType;
            ScreenShake.TriggerShake(0.15f);
            GameAPP.PlaySound(41, 0.5f, 1.0f);
            // 10. 添加排序组，设置渲染层级
            var sortingGroup = doom.gameObject.AddComponent<SortingGroup>();
            sortingGroup.sortingLayerName = "particle11";
            sortingGroup.sortingOrder = 100;
        }
    }

    public class Shooting_JackboxStar : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_JackboxStar(IntPtr ptr) : base(ptr) { }
        public Shooting_JackboxStar() : base(ClassInjector.DerivedConstructorPointer<Shooting_JackboxStar>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.JackboxStar;
        public override string Role => "输出";
        public override Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }
        private List<BaseBuff> CustomBuffs = new()
        {
            new UpgradeBuff(PlantType.JackboxStar, PlantType.IFVStar)
        };
        public override void ReinforcePlant(Plant plant)
        {
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 2f);
            plant.ModifyDamage(PlantDamageAdder.Shooting, 4f, false, new(float.MaxValue));
        }
    }

    public class Shooting_IFVStar : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_IFVStar(IntPtr ptr) : base(ptr) { }
        public Shooting_IFVStar() : base(ClassInjector.DerivedConstructorPointer<Shooting_IFVStar>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.IFVStar;
        public override string Role => "输出";
        public override Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }
        private List<BaseBuff> CustomBuffs = new()
        {
            new SpeedBuff(PlantType.IFVStar),
            new DamageBuff(PlantType.IFVStar),
            new UniqueUpgrade(),
            new AttractBuff(),
            new DoomBuff()
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.attackDamage = 800;
            plant.AddComponent<IFVStarShooting>();
        }

        internal void ResetQuality()
        {
            CustomBuffs[0] = new SpeedBuff(PlantType.IFVStar);
            CustomBuffs[1] = new DamageBuff(PlantType.IFVStar);
        }

        /// <summary>
        /// 强化：穿透
        /// </summary>
        public class UniqueUpgrade : BaseBuff
        {
            #region Il2Cpp构造函数
            public UniqueUpgrade(IntPtr ptr) : base(ptr) { }
            public UniqueUpgrade() : base(ClassInjector.DerivedConstructorPointer<UniqueUpgrade>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.33f;
            public override int MaxCount => 10;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.IFVStar;
            public override string Title => "强化：穿透";
            public override string Description => "子弹的穿透次数+1";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<IFVStarShooting>().unique++;
                };
                SafeModify(action);
            }
        }

        /// <summary>
        /// 质变：吸星大法
        /// </summary>
        public class AttractBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public AttractBuff(IntPtr ptr) : base(ptr) { }
            public AttractBuff() : base(ClassInjector.DerivedConstructorPointer<AttractBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.05f;
            public override int MaxCount => 1;
            public override Quality Rarity => Quality.diamond;
            public override PlantType ShowType => PlantType.IFVStar;
            public override string Title => "质变：吸星大法";
            public override string Description => "多功能杨桃会聚集周围的僵尸，并在僵尸接触到自己时发生爆炸";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<IFVStarShooting>().attract = true;
                };
                SafeModify(action);
            }
        }

        /// <summary>
        /// 质变：核爆爆炸
        /// </summary>
        public class DoomBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public DoomBuff(IntPtr ptr) : base(ptr) { }
            public DoomBuff() : base(ClassInjector.DerivedConstructorPointer<DoomBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.05f;
            public override int MaxCount => 1;
            public override Quality Rarity => Quality.diamond;
            public override PlantType ShowType => PlantType.IFVStar;
            public override string Title => "质变：电磁爆炸";
            public override string Description => "子弹第一次击中目标时造成一次同等伤害的电磁脉冲爆炸";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<IFVStarShooting>().doom = true;
                };
                SafeModify(action);
            }
        }
    }

    public class IFVStarShooting : MonoBehaviour
    {
        private static float ColumnX
        {
            get
            {
                if (Mouse.Instance != null)
                    return Mouse.Instance.GetBoxXFromColumn(1) - Mouse.Instance.GetBoxXFromColumn(0);
                return 1.35f;
            }
        }
        private static float Speed => ColumnX;

        public int unique = 0;
        public bool attract = false;
        public bool doom = false;
        public float startSpeed;
        
        public void Start()
        {
            if (plant == null || plant.IsDestroyed()) return;
            startSpeed = plant.anim.speed;
        }

        public void Update()
        {
            if (plant == null || plant.IsDestroyed()) return;
            plant.anim.speed = startSpeed * (1 + plant.attackSpeedAdder);

            #region 吸引僵尸
            if (Time.timeScale > 0 && attract)
            {
                var offset = new Vector3(0f, 0.5f, 0f);
                var axis = plant.axis.position;
                foreach (var col in Physics2D.OverlapCircleAll(axis + offset, 3f * ColumnX, plant.zombieLayer))
                {
                    if (col == null || col.IsDestroyed()) continue;
                    if (!col.TryGetComponent<Zombie>(out var zombie)) continue;
                    if (zombie == null || zombie.IsDestroyed()) continue;
                    if (zombie.theZombieType == ZombieType.HorseBoss || zombie.theZombieType == ZombieType.ZombieBoss ||
                        zombie.theZombieType == ZombieType.ZombieBoss2) continue;
                    if (zombie.theZombieRow != plant.thePlantRow)
                    {
                        zombie.ChangeRow(plant.thePlantRow);
                        zombie.theZombieRow = plant.thePlantRow;
                    }
                    var step = Speed * Time.deltaTime;
                    if (Vector3.Distance(axis, zombie.axis.position) > step)
                    {
                        var direction = (axis - zombie.axis.position).normalized * step;
                        zombie.SetPosition(zombie.axis.position + direction);
                    }
                    else
                    {
                        zombie.SetPosition(axis);
                    }
                }
            }
            #endregion

            #region 爆炸
            if (plant.attributeCountdown <= 0f)
            {
                if (attract)
                {
                    int count = unique + 1; // 加上初始能爆的1次
                    var pos = plant.axis.position + new Vector3(0f, 0.3f, 0f);
                    // 如果有碰撞体
                    foreach (var col in Physics2D.OverlapPointAll(pos, plant.zombieLayer))
                    {
                        if (col == null || col.IsDestroyed()) continue;
                        if (!col.TryGetComponent<Zombie>(out var _)) continue;
                        AoeDamage.SmallBomb(pos, 1.5f, plant.zombieLayer, plant.thePlantRow, plant.attackDamage,
                            plant.thePlantType);
                        CreateParticle.SetParticle(41, pos, plant.thePlantRow);
                        GameAPP.PlaySound(43, 0.2f, 1f);
                        count--;
                        if (count <= 0)
                            break;
                    }
                }
                plant.attributeCountdown = 0.5f;
            }
            #endregion
        }
        public IFVStar plant => gameObject.GetComponent<IFVStar>();
    }

    public class ShootingIFVStarBullet : MonoBehaviour
    {
        public void OnTriggerStay2D(Collider2D collision)
        {
            if (bullet.fromType != PlantType.IFVStar) return;
            if (Time.timeScale > 0 && bullet != null && bullet.board != null)
            {
                bullet.OnTriggerEnter2D(collision);
            }
        }

        public Bullet_jackboxStar bullet => gameObject.GetComponent<Bullet_jackboxStar>();
    }

    #region 基础功能实现
    [HarmonyPatch(typeof(GameLevel.RogueShooting.StarFruit))]
    public static class StarFruitConfigPatch
    {
        [HarmonyPatch(nameof(GameLevel.RogueShooting.StarFruit.Buffs), MethodType.Getter)]
        [HarmonyPostfix]
        public static void PostGetBuffs(ref Il2CppSystem.Collections.Generic.List<BaseBuff> __result)
        {
            __result.Add(new UpgradeBuff(PlantType.StarFruit, PlantType.JackboxStar));
        }
    }

    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.JackboxStar) ||
                !Config.configs.ContainsKey(PlantType.IFVStar))
            {
                Config.configs.Add(PlantType.JackboxStar, new Shooting_JackboxStar());
                Config.configs.Add(PlantType.IFVStar, new Shooting_IFVStar());
            }
            else
                Config.configs[PlantType.IFVStar].Cast<Shooting_IFVStar>().ResetQuality();
        }
    }
    #endregion

    #region 注册新粒子
    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            GameAPP.particlePrefab[(int)Core.theNewCherry.type] = Core.theNewCherry.obj;
            GameAPP.resourcesManager.particlePrefabs.Add(Core.theNewCherry.type, Core.theNewCherry.obj);
            GameAPP.resourcesManager.particlePrefabs.Add(Core.theNewDoom.type, Core.theNewDoom.obj);
            GameAPP.resourcesManager.allParticles.Add(Core.theNewCherry.type);
            GameAPP.resourcesManager.allParticles.Add(Core.theNewDoom.type);
        }
    }
    #endregion

    // 多功能杨桃子弹穿透
    [HarmonyPatch(typeof(Bullet_jackboxStar))]
    public static class Bullet_jackboxStarPatch
    {
        [HarmonyPatch(nameof(Bullet_jackboxStar.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_jackboxStar __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == BulletType.Bullet_star_ifv && __instance.board.boardTag.rogueShooting && 
                __instance.fromType == PlantType.IFVStar)
            {
                AoeDamage.SmallBomb(__instance.transform.position, 1.5f, __instance.zombieLayer, zombie.theZombieRow,
                    __instance.Damage, __instance.fromType);

                GameAPP.PlaySound(43, 0.2f, 1f);

                {
                    if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant(PlantType.IFVStar, out var plant) &&
                        plant.GetComponent<IFVStarShooting>().doom && __instance.attributeCount <= 0)
                    {
                        Core.CreateCherryExplodeCustom(__instance.transform.position, zombie.theZombieRow, __instance.Damage,
                            __instance.fromType);
                        //__instance.board.boardAction.SetDoom(zombie.Column, zombie.theZombieRow, false, false, Vector2.zero,
                        //    __instance.Damage, 0, null, false, __instance.fromType);
                        //if (!GameAPP.config.distablexplodeFlash)
                        //    Core.SetDoomCustom(__instance.board, zombie.axis.position, DoomType.Nuclear2);
                    }
                    else
                    {
                        CreateParticle.SetParticle(41, __instance.transform.position, zombie.theZombieRow);
                    }
                }
                {
                    if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant(PlantType.IFVStar, out var plant))
                        __instance.maxHitCount = plant.GetComponent<IFVStarShooting>().unique + 1;
                }

                return false;
            }
            return true;
        }
    }

    //[HarmonyPatch(typeof(Bullet))]
    //public static class BulletPatch
    //{
    //    [HarmonyPatch(nameof(Bullet.TrackZombie))]
    //    [HarmonyPrefix]
    //    public static bool PreTrackZombie(Bullet __instance, ref Zombie zombie)
    //    {
    //        if (__instance.theBulletType == BulletType.Bullet_star_ifv && __instance.board.boardTag.rogueShooting &&
    //            __instance.fromType == PlantType.IFVStar)
    //        {
    //            if (zombie != __instance.targetZombie) return false;
    //            if (zombie.isMindControlled || zombie.theHealth <= 0) return false;
    //            switch (zombie.theStatus)
    //            {
    //                case ZombieStatus.Pol_jump:
    //                case ZombieStatus.Snokle_inWater:
    //                case ZombieStatus.Dolphinrider_jump:
    //                case ZombieStatus.Flying:
    //                case ZombieStatus.Imp_fly:
    //                case ZombieStatus.Boss:
    //                case ZombieStatus.Bungi_wating:
    //                case ZombieStatus.Bungi_down:
    //                case ZombieStatus.Bungi_up:
    //                case ZombieStatus.Bungi_awake:
    //                    return false;
    //            }
    //            __instance.hit = true;
    //            __instance.HitZombie(zombie);
    //            return false;
    //        }
    //        return true;
    //    }
    //}

    // 多功能杨桃子弹追踪
    [HarmonyPatch(typeof(Bullet_star))]
    public static class Bullet_starPatch
    {
        [HarmonyPatch(nameof(Bullet_star.Update))]
        [HarmonyPostfix]
        public static void PostUpdate(Bullet_star __instance)
        {
            if (__instance.theBulletType == BulletType.Bullet_star_ifv && __instance.board.boardTag.rogueShooting &&
                __instance.fromType == PlantType.IFVStar)
            {
                if (__instance.MoveWay == BulletMoveWay.Stable) return;
                if (__instance.blackHole == null && __instance.theExistTime >= 0.3f && __instance.MoveWay != BulletMoveWay.Track)
                    __instance.MoveWay = BulletMoveWay.Track;
            }
        }
    }

    // 多功能杨桃改为全场索敌
    [HarmonyPatch(typeof(StarFruit))]
    public static class StarFruitPatch
    {
        [HarmonyPatch(nameof(StarFruit.SearchZombie))]
        [HarmonyPrefix]
        public static bool PreSearchZombie(StarFruit __instance, ref GameObject __result)
        {
            if (__instance.thePlantType == PlantType.IFVStar && __instance.board.boardTag.rogueShooting)
            {
                foreach (var item in Lawnf.GetAllZombies())
                    if (item != null && __instance.SearchUniqueZombie(item))
                    {
                        __result = item.gameObject;
                        return false;
                    }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CreateBullet))]
    public static class CreateBulletPatch
    {
        [HarmonyPatch(nameof(CreateBullet.SetBullet))]
        [HarmonyPostfix]
        public static void PostSetBullet(CreateBullet __instance, ref Bullet __result)
        {
            if (__result != null && !__result.IsDestroyed())
            {
                if (__result.theBulletType == BulletType.Bullet_star_ifv && __instance.board != null && !__instance.board.IsDestroyed() &&
                    __instance.board.boardTag.rogueShooting)
                {
                    __result.GetOrAddComponent<ShootingIFVStarBullet>();
                }
            }
        }
    }
}
