using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using System.Reflection;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingSuperCaltrop.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.supercaltrop", "RogueShootingSuperCaltrop", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            // 类型初始化
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_SquashSpike>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_SuperCaltrop>();
            ClassInjector.RegisterTypeInIl2Cpp<SuperCaltropShooting>();

            ClassInjector.RegisterTypeInIl2Cpp<Shooting_SuperCaltrop.UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_SuperCaltrop.SuperBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_SuperCaltrop.LadderBuff>();
        }
    }

    // 植物config
    public class Shooting_SquashSpike : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_SquashSpike(IntPtr ptr) : base(ptr) { }
        public Shooting_SquashSpike() : base(ClassInjector.DerivedConstructorPointer<Shooting_SquashSpike>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }
        public override PlantType PlantType => PlantType.SquashSpike;
        public override string Role => "输出/辅助";
        // 私有buff列表
        private List<BaseBuff> CustomBuffs = new()
        {
            new UpgradeBuff(PlantType.SquashSpike, PlantType.SuperCaltrop)
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 2f);
            plant.ModifyDamage(PlantDamageAdder.Shooting, 4f, false, new(float.MaxValue));
            plant.anim.speed *= 3f;
        }
    }

    public class Shooting_SuperCaltrop : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_SuperCaltrop(IntPtr ptr) : base(ptr) { }
        public Shooting_SuperCaltrop() : base(ClassInjector.DerivedConstructorPointer<Shooting_SuperCaltrop>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }
        public override PlantType PlantType => PlantType.SuperCaltrop;
        public override string Role => "输出/辅助";
        // 私有buff列表
        private List<BaseBuff> CustomBuffs = new()
        {
            new SpeedBuff(PlantType.SuperCaltrop),
            new UniqueUpgrade(),
            new LadderBuff(),
            new SuperBuff()
        };
        // 滑滑梯buff
        private Lazy<AdvBuff> Buff = new(() =>
        {
            var result = (AdvBuff)33;
            foreach (var item in Il2CppSystem.Enum.GetValues(Il2CppType.From(typeof(AdvBuff))))
            {
                if (item.ToString() == "滑滑梯")
                {
                    result = item.Unbox<AdvBuff>();
                    break;
                }
            }
            return result;
        });

        public override void ReinforcePlant(Plant plant)
        {
            TravelMgr.Instance.GetNormalBuff(Buff.Value);
            plant.AddComponent<SuperCaltropShooting>();
            plant.attackDamage = 300;
        }

        internal void ResetQuality()
        {
            CustomBuffs[0].Cast<SpeedBuff>().randomQuality = ShootingManager.Instance.GetRandomQuality();
        }

        /// <summary>
        /// 强化：半径
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
            public override PlantType ShowType => PlantType.SuperCaltrop;
            public override string Title => "强化：领域";
            public override string Description => "本体和窝瓜的影响范围+0.5";

            public override void OnGet()
            {
                Action<Plant> action = (plant) =>
                {
                    plant.GetComponent<SuperCaltropShooting>().range++;
                };
                SafeModify(action);
            }
        }

        public class LadderBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public LadderBuff(IntPtr ptr) : base(ptr) { }
            public LadderBuff() : base(ClassInjector.DerivedConstructorPointer<LadderBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion
            public override float AppearWeight => 0.167f;
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.SuperCaltrop;
            public override string Title => "强化：滑滑梯";
            public override string Description => "击退僵尸速度+100%";

            public override void OnGet()
            {
                Action<Plant> action = (plant) =>
                {
                    plant.GetComponent<SuperCaltropShooting>().knockback++;
                };
                SafeModify(action);
            }
        }

        /// <summary>
        /// 质变：让你飞起来！
        /// </summary>
        public class SuperBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public SuperBuff(IntPtr ptr) : base(ptr) { }
            public SuperBuff() : base(ClassInjector.DerivedConstructorPointer<SuperBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.05f;
            public override int MaxCount => 1;
            public override Quality Rarity => Quality.diamond;
            public override PlantType ShowType => PlantType.SuperCaltrop;
            public override string Title => "质变：窝们都能飞";
            public override string Description => "黄油窝瓜可以击飞僵尸，有概率直接震出场外";

            public override void OnGet()
            {
                Action<Plant> action = (plant) =>
                {
                    plant.GetComponent<SuperCaltropShooting>().super = true;
                };
                SafeModify(action);
            }
        }
    }

    public class SuperCaltropShooting : MonoBehaviour
    {
        private const float KnockbackSpeed = 0.3f;
        public static Vector3 AxisOffset => new(0f, 0.5f, 0f);
        public static float ColumnX
        {
            get
            {
                if (Mouse.Instance == null) return 1.35f;
                return Mouse.Instance.GetBoxXFromColumn(1) - Mouse.Instance.GetBoxXFromColumn(0);
            }
        }

        public GameObject LittleSquash = null!;
        public int range = 0;
        public int knockback = 0;
        public bool super = false;

        public void Awake()
        {
            LittleSquash = Resources.Load<GameObject>("items/littlesquash/LittleSquash_butter");
        }

        public void Start()
        {
            if (plant == null)
                Destroy(this);
        }

        public void FixedUpdate()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (plant == null || plant.IsDestroyed()) return;
            foreach (var col in Physics2D.OverlapCircleAll(plant.axis.position + AxisOffset, (1f + 0.5f * range) * ColumnX, 
                plant.zombieLayer))
            {
                if (col == null || !col.TryGetComponent<Zombie>(out var zombie) || zombie == null) continue;
                if (!zombie.Alive) continue;
                if (!Lawnf.InLandStatus(zombie.theStatus)) continue;
                // 对boss、蹦极不生效
                switch (zombie.theZombieType)
                {
                    case ZombieType.FootballBoss:
                    case ZombieType.HorseBoss:
                    case ZombieType.ZombieBoss:
                    case ZombieType.ZombieBoss2:
                    case ZombieType.BungiZombie:
                    case ZombieType.GoldBungiZombie:
                        continue;
                }
                zombie.RealKnockBack(Time.fixedDeltaTime * KnockbackSpeed * (1 + knockback));
            }
        }

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (plant == null || plant.IsDestroyed()) return;
            if (plant.attributeCountdown <= 0f)
            {
                var squash = Instantiate(LittleSquash, plant.axis.position, Quaternion.identity, plant.board.transform).
                    GetComponent<LittleSquash>();
                if (squash != null)
                {
                    squash.theRow = plant.thePlantRow;
                    squash.thePlantType = plant.thePlantType;
                    squash.theDamage = plant.attackDamage * 6;
                    Action<int, int> action = (row, damage) =>
                    {
                        if (plant == null) return;
                        var axis = squash.axis.position;
                        foreach (var col in Physics2D.OverlapCircleAll(axis + AxisOffset, (1 + 0.5f * range) * ColumnX, 
                            plant.zombieLayer))
                        {
                            if (col != null && col.TryGetComponent<Zombie>(out var zombie) && zombie != null)
                            {
                                if (!Lawnf.InLandStatus(zombie.theStatus)) continue;
                                zombie.Buttered(1f);
                                zombie.TakeDamage(plant.attackDamage, plant.Cast<IDamageMaker>(), DamageType.Normal,
                                    plant.thePlantType);
                                // 质变
                                if (!super) continue;

                                // 除已死亡、boss都击飞
                                if (UnityEngine.Random.Range(1, 101) <= 5) // 5%
                                {
                                    // 让僵尸飞起来
                                    zombie.StartCoroutine(BlowZombie(zombie));
                                }

                                if (!zombie.Alive) continue;
                                if (TypeMgr.IsBossZombie(zombie.theZombieType)) continue;
                                if (zombie.theZombieType == ZombieType.HorseBoss || zombie.theZombieType == ZombieType.ZombieBoss ||
                                    zombie.theZombieType == ZombieType.ZombieBoss2) continue;

                                if (zombie.TryCast<PogoZombie>() || zombie.TryCast<BungiZombie>()) continue;
                                if (TypeMgr.IsDriverZombie(zombie.theZombieType)) continue;
                                if (zombie.TryCast<ImpZombie>())
                                {
                                    zombie.ChangeStatus(ZombieStatus.Imp_fly);
                                    zombie.AddComponent<Blow>();
                                    zombie.theOriginSpeed = 0f;
                                    zombie.rb.velocity = Vector2.zero;
                                    continue;
                                }
                                if (zombie.rb.velocity.sqrMagnitude <= 0.0001f && !zombie.HasBuff(EffectType.Launch))
                                {
                                    zombie.SetEffect(EffectType.Launch, TypeMgr.IsGargantuar(zombie.theZombieType) ? 1.5f : 3.5f);
                                }
                            }
                        }
                    };
                    squash.crashAction = action;
                }

                plant.attributeCountdown = 3f;
            }
        }

        public IEnumerator BlowZombie(Zombie zombie)
        {
            if (zombie == null || zombie.IsDestroyed()) yield break;
            if (!zombie.Alive) yield break;
            if (TypeMgr.IsBossZombie(zombie.theZombieType) || 
                (zombie.theZombieType == ZombieType.HorseBoss || zombie.theZombieType == ZombieType.ZombieBoss ||
                    zombie.theZombieType == ZombieType.ZombieBoss2))
            {
                //zombie.TakeDamage((int)(zombie.CurrentAllHealth / 10), plant.Cast<IDamageMaker>(), DamageType.Normal,
                //    plant.thePlantType);
                yield break;
            }
            var deg = UnityEngine.Random.Range(35f, 55f);
            // 获取一个位于第一象限的与x轴夹角为35°-55°的归一化向量
            var direction = (Quaternion.Euler(0f, 0f, deg) * Vector2.right).normalized;
            var speed = UnityEngine.Random.Range(13f, 25f);
            var deadRight = zombie.deadRight;
            zombie.enabled = false;
            zombie.anim.enabled = false;
            zombie.col.enabled = false;
            if (zombie != null && !zombie.IsDestroyed() && zombie.board != null && !zombie.board.IsDestroyed())
                zombie.board.damageReporter.Report(PlantType.SuperCaltrop, zombie.CurrentAllHealth, zombie.axis.position + new Vector3(0f, 0.5f, 0f), new());
            while (zombie != null && !zombie.IsDestroyed() && zombie.axis.position.x <= deadRight)
            {
                zombie.transform.position += direction * speed * Time.deltaTime;
                yield return null;
            }
            if (zombie == null || zombie.IsDestroyed()) yield break;
            zombie.enabled = true;
            zombie.Die(2);
            yield break;
        }

        private SuperCaltrop plant => gameObject.GetComponent<SuperCaltrop>();
    }

    // 植物修改
    [HarmonyPatch(typeof(SuperCaltrop))]
    public class SuperCaltropPatch
    {
        [HarmonyPatch(nameof(SuperCaltrop.OnTriggerStay2D))]
        [HarmonyPrefix]
        public static bool PreOnTriggerStay2D(SuperCaltrop __instance)
        {
            if (__instance.board != null && __instance.board.boardTag.rogueShooting) return false;
            return true;
        }
    }

    // 基础功能
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.SquashSpike) ||
                !Config.configs.ContainsKey(PlantType.SuperCaltrop))
            {
                Config.configs.Add(PlantType.SquashSpike, new Shooting_SquashSpike());
                Config.configs.Add(PlantType.SuperCaltrop, new Shooting_SuperCaltrop());
            }
            else
                Config.configs[PlantType.SuperCaltrop].Cast<Shooting_SuperCaltrop>().ResetQuality();
        }
    }

    [HarmonyPatch(typeof(GameLevel.RogueShooting.Caltrop))]
    public static class CaltropPatch
    {
        [HarmonyPatch(nameof(GameLevel.RogueShooting.Caltrop.Buffs), MethodType.Getter)]
        [HarmonyPostfix]
        public static void PostGetBuffs(ref Il2CppSystem.Collections.Generic.List<BaseBuff> __result)
        {
            __result.Add(new UpgradeBuff(PlantType.Caltrop, PlantType.SquashSpike));
        }
    }
}
