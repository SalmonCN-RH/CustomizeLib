using BepInEx;
using BepInEx.Unity.IL2CPP;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingUltimateMelonCannon.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.ultimatemeloncannon", "RogueShootingUltimateMelonCannon", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_MelonCannon>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateMelonCannon>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateMelonCannon.UniqueBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateMelonCannon.SuperBuff>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<MelonCannonShooting>();
            ClassInjector.RegisterTypeInIl2Cpp<UltimateMelonCannonShooting>();
        }
    }

    /// <summary>
    /// 瓜炮config
    /// </summary>
    public class Shooting_MelonCannon : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_MelonCannon(IntPtr ptr) : base(ptr) { }
        public Shooting_MelonCannon() : base(ClassInjector.DerivedConstructorPointer<Shooting_MelonCannon>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.MelonCannon;
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
            new UpgradeBuff(PlantType.MelonCannon, PlantType.UltimateMelonCannon)
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.AddComponent<MelonCannonShooting>();
        }
    }

    /// <summary>
    /// 究瓜炮config
    /// </summary>
    public class Shooting_UltimateMelonCannon : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_UltimateMelonCannon(IntPtr ptr) : base(ptr) { }
        public Shooting_UltimateMelonCannon() : base(ClassInjector.DerivedConstructorPointer<Shooting_UltimateMelonCannon>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.UltimateMelonCannon;
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
            new DamageBuff(PlantType.UltimateMelonCannon),
            new SpeedBuff(PlantType.UltimateMelonCannon),
            new UniqueBuff(),
            new SuperBuff()
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.AddComponent<UltimateMelonCannonShooting>();
        }

        internal void ResetQuality()
        {
            CustomBuffs[0] = new DamageBuff(PlantType.UltimateMelonCannon);
            CustomBuffs[1] = new SpeedBuff(PlantType.UltimateMelonCannon);
        }

        public class UniqueBuff : BaseBuff
        {
            #region Il2Cpp构造方法
            public UniqueBuff(IntPtr ptr) : base(ptr) { }
            public UniqueBuff() : base(ClassInjector.DerivedConstructorPointer<UniqueBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.33f;
            public override int MaxCount => 10;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimateMelonCannon;
            public override string Title => "强化：榴弹";
            public override string Description => "每轮发射的炮弹数+1";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<UltimateMelonCannonShooting>().unique++;
                };
                SafeModify(action);
            }
        }

        public class SuperBuff : BaseBuff
        {
            #region Il2Cpp构造方法
            public SuperBuff(IntPtr ptr) : base(ptr) { }
            public SuperBuff() : base(ClassInjector.DerivedConstructorPointer<SuperBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.05f;
            public override int MaxCount => 1;
            public override Quality Rarity => Quality.diamond;
            public override PlantType ShowType => PlantType.UltimateMelonCannon;
            public override string Title => "质变：等价交换";
            public override string Description => "僵尸的速度不能再被降低时将减速变为等量百分比伤害";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<UltimateMelonCannonShooting>().super = true;
                };
                SafeModify(action);
            }
        }
    }

    /// <summary>
    /// 究瓜炮诸神额外特性
    /// </summary>
    public class UltimateMelonCannonShooting : MonoBehaviour
    {
        private const float RestTime = 0.12f;

        public int unique = 0;
        public bool super = false;
        public int round = 0;
        public bool extra = false;

        private GameObject target = null!;

        public void Awake()
        {
            target = Instantiate(GameAPP.itemPrefab[16], Camera.main.ScreenToWorldPoint(Input.mousePosition),
                Quaternion.identity, Mouse.Instance.transform);
            target.name = "cannon";
        }

        public void Update()
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (plant != null && !plant.IsDestroyed() && Time.timeScale != 0f && GameAPP.theGameStatus == GameStatus.InGame)
            {
                if (plant.attributeCountdown > 0.01f)
                    plant.attributeCountdown = 0.01f;
                if (plant.attributeCountdown <= 0f)
                {
                    plant.cannonTarget = pos;
                    plant.StartShoot();
                }
            }
            target.transform.position = new Vector3(pos.x, pos.y, 0f);
        }

        public void AddRound()
        {
            if (extra) return;
            round++;
            if (round >= 4)
            {
                extra = true;

                for (int i = 1; i <= unique; i++)
                {
                    plant.Invoke("AnimShoot", RestTime * i * (1 / plant.attributeSpeed));
                }

                // unique * RestTime * (1 / plant.attributeSpeed)后恢复extra标志位
                Invoke("ResetExtra", RestTime * unique * (1 / plant.attributeSpeed)); 
                round = 0;
            }
        }

        public void OnDestroy()
        {
            if (target != null)
                Destroy(target);
        }

        private void ResetExtra()
        { 
            extra = false;
        }

        public UltimateMelonCannon plant => gameObject.GetComponent<UltimateMelonCannon>();
    }

    /// <summary>
    /// 瓜炮无冷却组件
    /// </summary>
    public class MelonCannonShooting : MonoBehaviour
    {
        public void Update()
        {
            if (plant != null && !plant.IsDestroyed() && plant.attributeCountdown > 0.01f)
            {
                plant.attributeCountdown = 0.01f;
            }
        }

        public MelonCannon plant => gameObject.GetComponent<MelonCannon>();
    }

    #region 基础功能实现
    [HarmonyPatch(typeof(GameLevel.RogueShooting.Melonpult))]
    public static class MelonpultConfigPatch
    {
        [HarmonyPatch(nameof(GameLevel.RogueShooting.Melonpult.Buffs), MethodType.Getter)]
        [HarmonyPostfix]
        public static void PostGetBuffs(ref Il2CppSystem.Collections.Generic.List<BaseBuff> __result)
        {
            __result.Add(new UpgradeBuff(PlantType.Melonpult, PlantType.MelonCannon));
        }
    }

    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.MelonCannon) ||
                !Config.configs.ContainsKey(PlantType.UltimateMelonCannon))
            {
                Config.configs.Add(PlantType.MelonCannon, new Shooting_MelonCannon());
                Config.configs.Add(PlantType.UltimateMelonCannon, new Shooting_UltimateMelonCannon());
            }
            else
                Config.configs[PlantType.UltimateMelonCannon].Cast<Shooting_UltimateMelonCannon>().ResetQuality();
        }
    }
    #endregion

    [HarmonyPatch(typeof(MelonCannon))]
    public static class MelonCannonPatch
    {
        [HarmonyPatch(nameof(MelonCannon.AnimShoot))]
        [HarmonyPostfix]
        public static void PostAnimShoot(MelonCannon __instance)
        {
            if (__instance != null && !__instance.IsDestroyed() && __instance.board != null && !__instance.board.IsDestroyed() &&
                __instance.board.boardTag.rogueShooting && __instance.thePlantType == PlantType.UltimateMelonCannon)
            {
                __instance.GetComponent<UltimateMelonCannonShooting>().AddRound();
            }
        }
    }

    [HarmonyPatch(typeof(SubMelon))]
    public static class SubMelonPatch
    {
        [HarmonyPatch(nameof(SubMelon.Start))]
        [HarmonyPostfix]
        public static void PostStart(SubMelon __instance)
        {
            if (__instance != null && !__instance.IsDestroyed() &&
                __instance.board != null && !__instance.board.IsDestroyed() &&
                __instance.board.boardTag.rogueShooting)
            {
                if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant(PlantType.UltimateMelonCannon, out var plant))
                {
                    __instance.moveSpeed *= 1 + plant.attackSpeedAdder;
                }
            }
        }

        [HarmonyPatch(nameof(SubMelon.SetTarget))]
        [HarmonyPostfix]
        public static void PostSetTarget(SubMelon __instance)
        {
            if (__instance != null && !__instance.IsDestroyed() &&
                __instance.board != null && !__instance.board.IsDestroyed() &&
                __instance.board.boardTag.rogueShooting)
            {
                __instance.targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        [HarmonyPatch(nameof(SubMelon.Die))]
        [HarmonyPostfix]
        public static void PostDie(SubMelon __instance)
        {
            if (__instance != null && !__instance.IsDestroyed() &&
                __instance.board != null && !__instance.board.IsDestroyed() &&
                __instance.board.boardTag.rogueShooting)
            {
                if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant(PlantType.UltimateMelonCannon, out var plant))
                {
                    var comp = plant.GetComponent<UltimateMelonCannonShooting>();
                    foreach (var col in Physics2D.OverlapCircleAll(__instance.transform.position, 2f, plant.zombieLayer))
                    {
                        if (col == null || col.IsDestroyed()) continue;
                        if (!col.TryGetComponent<Zombie>(out var zombie)) continue;
                        if (zombie == null || zombie.IsDestroyed()) continue;
                        if (!zombie.Alive) continue;
                        if (!comp.super) continue;
                        zombie.SetCold(10f);
                        var dmg = (int)(zombie.CurrentAllHealth * 0.0005f);
                        if (!zombie.HasBuff(EffectType.Cold))
                            dmg *= 4;
                        if (zombie.theOriginSpeed <= 0.9f)
                            zombie.TakeDamage(dmg, plant.Cast<IDamageMaker>(), DamageType.IceAll, 
                                plant.thePlantType);
                    }
                }
            }
        }
    }
}
