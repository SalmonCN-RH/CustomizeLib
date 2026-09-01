using BepInEx;
using BepInEx.Unity.IL2CPP;
using Cysharp.Threading.Tasks;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingUltimateHugeNut.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.ultimatehugenut", "RogueShootingUltimateHugeNut", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_HugeWallNut>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateHugeNut>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateHugeNut.EnergyBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateHugeNut.TemperingBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateHugeNut.SuperBuff>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<UltimateHugeNutShooting>();
        }
    }

    public class Shooting_HugeWallNut : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_HugeWallNut(IntPtr ptr) : base(ptr) { }
        public Shooting_HugeWallNut() : base(ClassInjector.DerivedConstructorPointer<Shooting_HugeWallNut>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.HugeWallNut;
        public override string Role => "防御";
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
            new UpgradeBuff(PlantType.HugeWallNut, PlantType.UltimateHugeNut)
        };

        public override void ReinforcePlant(Plant plant) { }
    }

    public class Shooting_UltimateHugeNut : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_UltimateHugeNut(IntPtr ptr) : base(ptr) { }
        public Shooting_UltimateHugeNut() : base(ClassInjector.DerivedConstructorPointer<Shooting_UltimateHugeNut>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.UltimateHugeNut;
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
            new DamageBuff(PlantType.UltimateHugeNut),
            new EnergyBuff(),
            new TemperingBuff(),
            new SuperBuff()
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.AddComponent<UltimateHugeNutShooting>();
            (UltimateHugeNut.ObsidianPlants, UltimateHugeNutShooting.ShootingHashSet) =
                (UltimateHugeNutShooting.ShootingHashSet, UltimateHugeNut.ObsidianPlants);
        }

        public void ResetQuality()
        {
            CustomBuffs[0] = new DamageBuff(PlantType);
        }

        public class EnergyBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public EnergyBuff(IntPtr ptr) : base(ptr) { }
            public EnergyBuff() : base(ClassInjector.DerivedConstructorPointer<EnergyBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.167f;
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimateHugeNut;
            public override string Title => "强化：引擎";
            public override string Description => "获取能量的效率+100%";

            public override void OnGet()
            {
                // 空方法
                // 通过ShootingManager获取选取次数
            }
        }

        public class TemperingBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public TemperingBuff(IntPtr ptr) : base(ptr) { }
            public TemperingBuff() : base(ClassInjector.DerivedConstructorPointer<TemperingBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.33f;
            public override int MaxCount => 10;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimateHugeNut;
            public override string Title => "强化：淬炼";
            public override string Description => "淬炼造成的伤害+100%，每次造成淬炼时消耗0.5%能量增加(20×消耗能量)的伤害";

            public override void OnGet()
            {
                // 空方法
                // 通过ShootingManager获取选取次数
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
            public override PlantType ShowType => PlantType.UltimateHugeNut;
            public override string Title => "质变：我们的羁绊";
            public override string Description => "立即召唤一株随机黑曜石类植物（黑曜石大麦除外），每30秒切换一次种类，其每次出场或变身时消耗自身30%能量，使其获得(0.1%消耗能量)的属性加成";

            public override void OnGet()
            {
                Plant.GetOrAddComponent<UltimateHugeNutShooting>().super = true;
            }
        }
    }

    public class UltimateHugeNutShooting : MonoBehaviour
    {
        public static Il2CppSystem.Collections.Generic.HashSet<PlantType> ShootingHashSet = new();
        public Il2CppSystem.Collections.Generic.HashSet<PlantType> OriginalPlants = new();

        private float timer = 0f;
        public bool super = false;

        public static void Constructor()
        {
            // 初始化诸神hashset
            ShootingHashSet = new();
            foreach (var item in GameAPP.resourcesManager.allPlants)
                ShootingHashSet.Add(item);
        }

        public void Awake()
        {
            OriginalPlants = new(UltimateHugeNut.ObsidianPlants.Cast<Il2CppSystem.Collections.Generic.IEnumerable<PlantType>>());
            OriginalPlants.Remove(PlantType.ObsidianWheat);
            OriginalPlants.Remove(PlantType.UltimateHugeNut);
            timer = 0f;
        }

        public void Update()
        {
            if (plant == null || GameAPP.theGameStatus != GameStatus.InGame || Time.timeScale <= 0f) return;
            //plant.attributeCount = Mathf.Min((int)1E6f, plant.attributeCount); // 能量上限100w
            //plant.UpdateText();
            if (plant.attributeCount < -10_0000_0000)
            {
                plant.attributeCount = int.MaxValue; // 防止能量溢出
                plant.UpdateText();
            }

            if (super)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    var column = current == null ? 0 : current.thePlantColumn;
                    var row = current == null ? 0 : current.thePlantRow;

                    var newSet = new Il2CppSystem.Collections.Generic.HashSet<PlantType>(OriginalPlants.Cast<Il2CppSystem.Collections.Generic.IEnumerable<PlantType>>());

                    if (current != null)
                    {
                        _ = newSet.Remove(current.thePlantType);
                        current.Die();
                    }

                    var cnt = UnityEngine.Random.Range(0, newSet.Count);
                    var enumerator = newSet.GetEnumerator();
                    while (cnt-- > 0 && enumerator.MoveNext()) { }
                    _ = enumerator.MoveNext();
                    var pt = enumerator.Current;

                    current = CreatePlant.Instance.SetPlant(column, row, pt, isFreeSet: true);

                    var use = (int)(plant.attributeCount * 0.3f);
                    if (plant.attributeCount >= use)
                    {
                        plant.attributeCount -= use;
                        plant.UpdateText();
                        current.attackDamage = (int)(current.attackDamage * (1 + use * 0.001f));
                        current.ModifyHealth(PlantHealthAdder.Shooting, use * 0.001f);
                        current.ModifySpeed(PlantSpeedAdder.Shooting, use * 0.001f);
                        if ((current.thePlantHealth > 1E9f || current.thePlantMaxHealth > 1E9f) ||  // 大于10亿血
                            (current.thePlantHealth <= 1 || current.thePlantMaxHealth <= 1)) // 或者为1血（飘的溢出
                            current.thePlantHealth = plant.thePlantMaxHealth = (int)1E9f;
                    }

                    timer = 30f;
                }
            }
        }

        public void OnDestroy()
        {
            if (current != null)
                current.Die();
            (UltimateHugeNut.ObsidianPlants, ShootingHashSet) = (ShootingHashSet, UltimateHugeNut.ObsidianPlants);
        }

        public Plant current = null!;
        public UltimateHugeNut plant => gameObject.GetComponent<UltimateHugeNut>();
    }

    #region 基础
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.HugeWallNut) ||
                !Config.configs.ContainsKey(PlantType.UltimateHugeNut))
            {
                Config.configs.Add(PlantType.HugeWallNut, new Shooting_HugeWallNut());
                Config.configs.Add(PlantType.UltimateHugeNut, new Shooting_UltimateHugeNut());
            }
            else
                Config.configs[PlantType.UltimateHugeNut].Cast<Shooting_UltimateHugeNut>().ResetQuality();
        }

        // 神秘复制中心bug
        [HarmonyPatch(nameof(ShootingManager.LosePlant))]
        [HarmonyPrefix]
        public static bool PreLosePlant(ShootingManager __instance, ref Plant plant)
        {
            if (__instance.TryGetPlant(PlantType.UltimateHugeNut, out var nut) && nut != null)
            {
                if (plant == nut.GetComponent<UltimateHugeNutShooting>().current)
                    return false;
            }
            return true;
        }
    }
    #endregion

    #region 坚果加路线
    [HarmonyPatch(typeof(GameLevel.RogueShooting.WallNut))]
    public static class WallNutPatch
    {
        [HarmonyPatch(nameof(GameLevel.RogueShooting.WallNut.Buffs), MethodType.Getter)]
        [HarmonyPostfix]
        public static void PostGetBuffs(ref Il2CppSystem.Collections.Generic.List<BaseBuff> __result)
        {
            __result.Add(new UpgradeBuff(PlantType.WallNut, PlantType.HugeWallNut));
        }
    }
    #endregion

    #region 字段初始化
    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            UltimateHugeNutShooting.Constructor();
        }
    }
    #endregion

    #region 黑堡词条增益
    [HarmonyPatch(typeof(UltimateHugeNut))]
    public static class UltimateHugeNutPatch
    {
        [HarmonyPatch(nameof(UltimateHugeNut.GetEnergy))]
        [HarmonyPrefix]
        public static void PreGetEnergy(UltimateHugeNut __instance, ref int value)
        {
            if (__instance != null && __instance.thePlantType == PlantType.UltimateHugeNut && 
                __instance.board != null && __instance.board.boardTag.rogueShooting &&
                ShootingManager.Instance != null)
            {
                value = (int)(value * (1 + ShootingManager.Instance.GetBuffChoiceCount(__instance.thePlantType, "强化：引擎") * 0.5f));
            }
        }

        [HarmonyPatch(nameof(UltimateHugeNut.TemperDamage), MethodType.Getter)]
        [HarmonyPostfix]
        public static void PostGetTemperDamage(UltimateHugeNut __instance, ref int __result)
        {
            if (__instance != null && __instance.thePlantType == PlantType.UltimateHugeNut &&
                __instance.board != null && __instance.board.boardTag.rogueShooting &&
                ShootingManager.Instance != null)
            {
                var count = ShootingManager.Instance.GetBuffChoiceCount(__instance.thePlantType, "强化：淬炼");
                __result *= 1 + count;
                if (count > 0)
                {
                    var use = (int)(__instance.attributeCount * 0.005f);
                    if (__instance.attributeCount >= use)
                    {
                        __instance.attributeCount -= use;
                        __instance.UpdateText();
                        __result += use * 20;
                    }
                }
            }
        }
    }
    #endregion
}
