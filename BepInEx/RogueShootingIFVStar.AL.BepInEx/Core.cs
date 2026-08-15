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

namespace RogueShootingIFVStar.AL.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.ifvstar.al", "RogueShootingIFVStarAL", "1.0.0")]
    public class Core : BasePlugin
    {
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
            // other
            ClassInjector.RegisterTypeInIl2Cpp<IFVStarShooting>();
        }
    }

    // 丑桃config
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
            plant.ModifyDamage(PlantDamageAdder.Shooting, 4f, false, new());
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
        public override string Role => "输出/辅助";
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
            new DamageBuff(PlantType.IFVStar),
            new SpeedBuff(PlantType.IFVStar)
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.attackDamage = 400;
            plant.AddComponent<IFVStarShooting>();
        }

        internal void ResetQuality()
        {
            CustomBuffs[0].Cast<DamageBuff>().randomQuality = ShootingManager.Instance.GetRandomQuality();
            CustomBuffs[1].Cast<SpeedBuff>().randomQuality = ShootingManager.Instance.GetRandomQuality();
        }

        /// <summary>
        /// 强化：炸药
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
            public override string Title => "强化：炸药";
            public override string Description => "释放的小丑爆炸伤害+100%\n多功能子弹释放小丑爆炸的概率提升8%";

            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    ++p.GetComponent<IFVStarShooting>().unique;
                };
                SafeModify(action);
            }
        }

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
            public override string Description => "多功能杨桃会小范围聚集僵尸，对大范围的目标造成减速效果";

            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<IFVStarShooting>().attract = true;
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
        public bool boom = false;
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
                foreach (var col in Physics2D.OverlapCircleAll(axis + offset, 1.7f * ColumnX, plant.zombieLayer))
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
        }

        public IFVStar plant => gameObject.GetComponent<IFVStar>();
    }

    [HarmonyPatch(typeof(Bullet_jackboxStar))]
    public static class Bullet_jackboxStarPatch
    {
        [HarmonyPatch(nameof(Bullet_jackboxStar.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_jackboxStar __instance, ref Zombie zombie)
        {
            if (__instance != null && __instance.board != null && __instance.board.boardTag.rogueShooting && 
                __instance.theBulletType == BulletType.Bullet_star_ifv && __instance.fromType == PlantType.IFVStar)
            {
                AoeDamage.Bomb(__instance.transform.position, 1.5f, __instance.zombieLayer, zombie.theZombieRow,
                    __instance.Damage, int.MaxValue, __instance.fromType);
                CreateParticle.SetParticle(41, __instance.transform.position, zombie.theZombieRow);

                // 播放音效（ID: 43，音量0.2，音调1.0）
                GameAPP.PlaySound(43, 0.2f, 1.0f);

                // 子弹销毁
                __instance.attributeCount++;
                if (__instance.attributeCount > 3)
                    __instance.Die();
                else
                    __instance.hit = false;
                return false;
            }
            return true;
        }
    }
}
