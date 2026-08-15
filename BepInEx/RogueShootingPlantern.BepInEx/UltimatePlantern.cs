using BepInEx.Unity.IL2CPP.Utils;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingPlantern.BepInEx.ShootingUltimatePlantern
{
    internal static class UltimatePlanternCore
    {
        internal static void OnLoad()
        {
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IcePlantern>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimatePlantern>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimatePlantern.UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimatePlantern.SuperBuff>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<UltimatePlanternShooting>();
        }
    }

    public class Shooting_IcePlantern : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_IcePlantern(IntPtr ptr) : base(ptr) { }
        public Shooting_IcePlantern() : base(ClassInjector.DerivedConstructorPointer<Shooting_IcePlantern>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.IcePlantern;
        public override string Role => "辅助";
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
            new UpgradeBuff(PlantType.IcePlantern, PlantType.UltimatePlantern)
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 1f);
        }
    }

    public class Shooting_UltimatePlantern : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_UltimatePlantern(IntPtr ptr) : base(ptr) { }
        public Shooting_UltimatePlantern() : base(ClassInjector.DerivedConstructorPointer<Shooting_UltimatePlantern>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.IcePlantern;
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
            new DamageBuff(PlantType.UltimatePlantern),
            new SpeedBuff(PlantType.UltimatePlantern),
            new UniqueUpgrade(),
            new SuperBuff()
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.attackDamage *= 2;
            plant.AddComponent<UltimatePlanternShooting>();
        }

        internal void ResetQuality()
        {
            CustomBuffs[0] = new DamageBuff(PlantType.UltimatePlantern);
            CustomBuffs[1] = new SpeedBuff(PlantType.UltimatePlantern);
        }

        public class UniqueUpgrade : BaseBuff
        {
            #region Il2Cpp构造函数
            public UniqueUpgrade(IntPtr ptr) : base(ptr) { }
            public UniqueUpgrade() : base(ClassInjector.DerivedConstructorPointer<UniqueUpgrade>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.167f;
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimatePlantern;
            public override string Title => "强化：大功率";
            public override string Description => "每次攻击为全场额外提供1点光照，持续10秒";

            public override void OnGet()
            {
                // 效果通过shootingmanager实现
            }
        }

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
            public override PlantType ShowType => PlantType.UltimatePlantern;
            public override string Title => "质变：天基屠龙炮";
            public override string Description => 
                $"{PlantName}每次攻击会从天空中召下一道冰爆激光，锁定场上血量最高的僵尸，对其造成自身攻击力×5×光照等级的伤害";

            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<UltimatePlanternShooting>().super = true;
                };
                SafeModify(action);
            }
        }
    }

    public class UltimatePlanternShooting : MonoBehaviour
    {
        private GameObject laser = null!;
        public bool super = false;

        public void Awake()
        {
            plant.thePlantHealth = plant.thePlantMaxHealth = 20000;
            plant.UpdateText();
        }

        public void Start()
        {
            laser = Resources.Load<GameObject>("plants/plantern/ultimateplantern/BlueLaser");
            if (plant != null)
                plant.uncrashable = true;
        }

        public void OnAttack()
        {
            if (!super) return;
            if (plant == null) return;
            var target = FindTargetZombie();
            if (target == null) return;
            var line = Instantiate(laser, plant.board.transform).GetComponent<LineRenderer>();
            var axis = target.axis.position;
            line.SetPosition(0, new Vector3(axis.x, plant.board.boardMaxY + 1f, 0f));
            line.SetPosition(1, axis);
            line.SetWidth(0.8f, 0.8f);

            plant.board.StartCoroutine(SetLineAlpha(line));
            line.sortingLayerName = $"particle{target.theZombieRow}";

            target.TakeDamage(plant.attackDamage * plant.currentLightLevel * 5, plant.Cast<IDamageMaker>(), DamageType.IceAll,
                plant.thePlantType);
            ScreenShake.TriggerShake(0.15f);
            AddLightLevel();
        }

        public static IEnumerator SetLineAlpha(LineRenderer renderer)
        {
            yield return new WaitForSeconds(0.25f);
            var total = 0.35f - 0.25f; // 隐藏 - 开始
            var current = 0f;
            var color = renderer.colorGradient.colorKeys;
            while (current < total)
            {
                current += Time.deltaTime;
                var progress = current / total;
                var newAlpha = Mathf.Lerp(1f, 0f, progress);
                var gradient = new Gradient();
                gradient.SetKeys(color, new GradientAlphaKey[]
                {
                    new(newAlpha, 0f),
                    new(newAlpha, 1f)
                });
                renderer.colorGradient = gradient;
                yield return null;
            }

            var final = new Gradient();
            final.SetKeys(
                color,
                new GradientAlphaKey[] {
                    new(0f, 0f),
                    new(0f, 1f)
                }
            );
            renderer.colorGradient = final;
            Destroy(renderer.gameObject);
        }

        public static Zombie? FindTargetZombie()
        {
            var list = Lawnf.GetAllZombies().ToArray().
                Where(z => z != null && Lawnf.InLandStatus(z.theStatus) && !z.beforeDying && z.TotalAllHealth > 0). // 找到所有满足条件的
                OrderByDescending(z => z.CurrentAllHealth).ThenBy(z => z.axis.position.x).ToList(); // 先按血量排序，再按离家远近排序
            if (list.Count <= 0) return null;
            return list[0];
        }

        public static void AddLightLevel()
        {
            if (Board.Instance == null) return;
            var level = ShootingManager.Instance.GetBuffChoiceCount(PlantType.UltimatePlantern, "强化：大功率");
            Board.Instance.AddLightLevel(level);
            Action action = () =>
            {
                if (Board.Instance != null)
                    Board.Instance.AddLightLevel(-level);
            };
            GameAPP.delayAction.SetAction(action, 5f);
        }

        public UltimatePlantern plant => gameObject.GetComponent<UltimatePlantern>();
    }

    #region 基础
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.IcePlantern) ||
                !Config.configs.ContainsKey(PlantType.UltimatePlantern))
            {
                Config.configs.Add(PlantType.IcePlantern, new Shooting_IcePlantern());
                Config.configs.Add(PlantType.UltimatePlantern, new Shooting_UltimatePlantern());
            }
            else
                Config.configs[PlantType.UltimatePlantern].Cast<Shooting_UltimatePlantern>().ResetQuality();
        }
    }
    #endregion

    #region 究灯
    [HarmonyPatch(typeof(UltimatePlantern))]
    public static class UltimatePlanternPatch
    {
        [HarmonyPatch(nameof(UltimatePlantern.AttributeEvent))]
        [HarmonyPostfix]
        public static void PostAttributeEvent(UltimatePlantern __instance)
        {
            if (__instance != null &&
                __instance.board != null && __instance.board.boardTag.rogueShooting &&
                ShootingManager.Instance != null)
            {
                var center = __instance.axis.position + new Vector3(0f, 0.5f, 0f);
                bool hit = false;
                foreach (var col in Physics2D.OverlapCircleAll(center, 2.5f, __instance.zombieLayer))
                {
                    if (col == null) continue;
                    if (!col.TryGetComponent<Zombie>(out var zombie)) continue;
                    if (zombie == null) continue;
                    if (Mathf.Abs(__instance.thePlantRow - zombie.theZombieRow) > 1) continue;
                    if (zombie.theStatus == ZombieStatus.Miner_digging) continue;

                    hit = true;
                }

                if (hit)
                {
                    UltimatePlanternShooting.AddLightLevel();
                }
                __instance.GetComponent<UltimatePlanternShooting>().OnAttack();
            }
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch(nameof(Plant.GetDamage))]
        [HarmonyPostfix]
        public static void PostGetDamage(Plant __instance, ref int __result)
        {
            if (__instance != null && __instance.thePlantType == PlantType.UltimatePlantern &&
                __instance.board != null && __instance.board.boardTag.rogueShooting)
            {
                __result = Mathf.Min(__result, 200);
            }
        }
    }
    #endregion
}
