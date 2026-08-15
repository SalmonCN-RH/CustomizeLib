using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Hook;
using BepInEx.Unity.IL2CPP.Utils;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace RogueShootingUltimateJalapeno.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.ultimatejalapeno", "RogueShootingUltimateJalapeno", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_Jalapeno>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_ObsidianJalapeno>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateJalapeno>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateJalapeno.UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateJalapeno.ShieldBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateJalapeno.RealDamageBuff>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<UltimateJalapenoShooting>();
            ClassInjector.RegisterTypeInIl2Cpp<ZombieData>();

            ZombieHook.ApplyHook();
        }
    }

    public static class ZombieHook
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ZombieTakeDamage(IntPtr instance, int theDamage, IntPtr damageFrom, int theDamageType, 
            int reportType, [MarshalAs(UnmanagedType.I1)] bool fix, IntPtr method);

        private static ZombieTakeDamage Origin = null!;
        public static INativeDetour Detour = null!;

        public static unsafe void ApplyHook()
        {
            var cls = IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "", "Zombie");
            IL2CPP.il2cpp_runtime_class_init(cls);
            var method = IL2CPP.GetIl2CppMethod(cls, false, "TakeDamage", "System.Void", new[]
                {
                    IL2CPP.RenderTypeName<int>(), 
                    IL2CPP.RenderTypeName<IDamageMaker>(),
                    IL2CPP.RenderTypeName<DamageType>(), 
                    IL2CPP.RenderTypeName<int>(), 
                    IL2CPP.RenderTypeName<bool>()
                });
            var methodStrc = UnityVersionHandler.Wrap((Il2CppMethodInfo*)method);
            Detour = INativeDetour.CreateAndApply(methodStrc.MethodPointer, OnZombieTakeDamage, out Origin);
        }

        public static void OnZombieTakeDamage(IntPtr instance, int theDamage, IntPtr damageFrom, int theDamageType, int reportType, 
            [MarshalAs(UnmanagedType.I1)] bool fix, IntPtr method)
        {
            var @this = new Zombie(instance);
            if (@this != null &&
                @this.board != null && @this.board.boardTag.rogueShooting &&
                ShootingManager.Instance != null && UltimateJalapenoShooting.InShieldsPlants.Contains((PlantType)reportType))
            {
                if (!ZombieData.GetZombieInFixDamage(@this))
                {
                    ZombieData.SetZombieInFixDamage(@this, true);
                    Origin.Invoke(instance,
                        theDamage / 10 * ShootingManager.Instance.GetBuffChoiceCount(PlantType.UltimateJalapeno, "强化：真伤"),
                        damageFrom, theDamageType, reportType, true, method);
                    ZombieData.SetZombieInFixDamage(@this, false);
                }
            }
            Origin.Invoke(instance, theDamage, damageFrom, theDamageType, reportType, fix, method);
        }
    }

    public class Shooting_Jalapeno : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_Jalapeno(IntPtr ptr) : base(ptr) { }
        public Shooting_Jalapeno() : base(ClassInjector.DerivedConstructorPointer<Shooting_Jalapeno>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.Jalapeno;
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
            new UpgradeBuff(PlantType.Jalapeno, PlantType.ObsidianJalapeno)
        };

        public override void ReinforcePlant(Plant plant) { }
    }

    public class Shooting_ObsidianJalapeno : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_ObsidianJalapeno(IntPtr ptr) : base(ptr) { }
        public Shooting_ObsidianJalapeno() : base(ClassInjector.DerivedConstructorPointer<Shooting_ObsidianJalapeno>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.ObsidianJalapeno;
        public override string Role => "输出/防御";
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
            new UpgradeBuff(PlantType.ObsidianJalapeno, PlantType.UltimateJalapeno)
        };

        public override void ReinforcePlant(Plant plant) { }
    }

    public class Shooting_UltimateJalapeno : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_UltimateJalapeno(IntPtr ptr) : base(ptr) { }
        public Shooting_UltimateJalapeno() : base(ClassInjector.DerivedConstructorPointer<Shooting_UltimateJalapeno>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        private Lazy<(UltiBuff buff1, UltiBuff buff2)> UnlockBuffs = new(() =>
        {
            var result = ((UltiBuff)46, (UltiBuff)47);
            foreach (var item in Il2CppSystem.Enum.GetValues(Il2CppType.From(typeof(UltiBuff))))
            {
                if (item.ToString() == "斩将祭旗")
                    result.Item1 = item.Unbox<UltiBuff>();
                if (item.ToString() == "黑曜护体")
                    result.Item2 = item.Unbox<UltiBuff>();
            }
            return result;
        });

        public override PlantType PlantType => PlantType.UltimateJalapeno;
        public override string Role => "辅助/防御";
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
            new UniqueUpgrade(),
            new ShieldBuff(),
            new RealDamageBuff()
        };

        public override void ReinforcePlant(Plant plant) 
        {
            if (TravelMgr.Instance != null)
            {
                var (buff1, buff2) = UnlockBuffs.Value;
                TravelMgr.Instance.GetUltiBuff(buff1);
                TravelMgr.Instance.GetUltiBuff(buff2);
            }
            plant.AddComponent<UltimateJalapenoShooting>();
        }

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
            public override PlantType ShowType => PlantType.UltimateJalapeno;
            public override string Title => "强化：充能";
            public override string Description => "充能上限+1";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<UltimateJalapenoShooting>().unique++;
                };
                SafeModify(action);
            }
        }

        public class ShieldBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public ShieldBuff(IntPtr ptr) : base(ptr) { }
            public ShieldBuff() : base(ClassInjector.DerivedConstructorPointer<ShieldBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.167f;
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimateJalapeno;
            public override string Title => "强化：魂环";
            public override string Description => "自身获得1个黑耀晶环，可转移给其他植物";

            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<UltimateJalapenoShooting>().CreateShield();
                };
                SafeModify(action);
            }
        }

        public class RealDamageBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public RealDamageBuff(IntPtr ptr) : base(ptr) { }
            public RealDamageBuff() : base(ClassInjector.DerivedConstructorPointer<RealDamageBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.167f;
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimateJalapeno;
            public override string Title => "强化：真伤";
            public override string Description => "拥有黑耀晶环的植物伤害目标时额外造成10%的真实伤害";

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override void OnGet()
            {
                 // 这部分代码由patch经shootingmanager的api调用，所以留空
            }
        }
    }

    public class UltimateJalapenoShooting : MonoBehaviour
    {
        public static HashSet<PlantType> InShieldsPlants = new();
        public int unique = 0; // +充能上限

        private List<(GameObject shield, Plant target)> Shields = new();
        private GameObject ShieldPrefab = null!;

        public void Awake()
        {
            ShieldPrefab = Resources.Load<GameObject>("plants/smallpuff/ultimatejalapuff/ShieldEffect");
            plant.AttributeCountdown = 0.5f;
        }

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (Time.timeScale <= 0f) return;
            if (plant == null) return;

            if (plant.attributeCount > unique)
            {
                plant.attributeCount = unique;
                plant.UpdateText();
            }
            if (plant.attributeCountdown <= 0f)
            {
                ShieldAttack();
                plant.attributeCountdown = 0.5f;
            }
            UpdateShieldPostion();
        }

        public void ShieldAttack()
        {
            bool hit = false;
            int damage = plant.attackDamage / 6;
            foreach (var (shield, target) in Shields)
            {
                var row = Mouse.Instance.GetRowFromY(shield.transform.position.x, shield.transform.position.y);
                foreach (var col in Physics2D.OverlapCircleAll(shield.transform.position, shield.transform.localScale.x, plant.zombieLayer))
                {
                    if (!col.TryGetComponent<Zombie>(out var zombie)) continue;
                    if (!Lawnf.InLandStatus(zombie.theStatus)) continue;
                    if (zombie.theZombieRow != row) continue;

                    zombie.TakeDamage(damage, plant.Cast<IDamageMaker>(), DamageType.Ice, plant.thePlantType);

                    if (target != null)
                        target.GetShield(damage / 10);
                    hit = true;
                }
            }
            if (hit) GameAPP.PlaySound(UnityEngine.Random.Range(0, 3));
        }

        public void CreateShield()
        {
            var shield = Instantiate(ShieldPrefab, plant.axis.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity, 
                plant.board.transform);
            shield.GetComponent<SortingGroup>().sortingLayerName = $"particle{plant.thePlantRow}";
            Shields.Add((shield, plant));
            InShieldsPlants.Add(plant.thePlantType);
        }

        public void OnDestroy()
        {
            foreach (var (shield, _) in Shields)
            {
                Destroy(shield);
            }
            Shields.Clear();
            InShieldsPlants.Clear();
        }

        public void SetTargetByMouse(Mouse mouse)
        {
            if (Shields.Count <= 0) return;
            if (plant == null) return;
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var shield = Shields.ToList().OrderByDescending(tuple => tuple.target == plant).First();
            foreach (var p in mouse.GetPlantsOnMouse(Physics2D.RaycastAll(pos, Vector2.zero)))
            {
                if (p == null) continue;
                InShieldsPlants.Remove(shield.target.thePlantType);
                Shields.Remove(shield);
                Shields.Add((shield.shield, p)); // 把挪过的放到后面
                InShieldsPlants.Add(p.thePlantType);
                break;
            }
        }

        public void UpdateShieldPostion()
        {
            if (plant == null) return;
            var offset = new Vector3(0f, 0.4f, 0f);
            foreach (var (shield, target) in Shields)
            {
                var pos = plant.axis.position + offset;
                int row = plant.thePlantRow;
                if (target != null)
                {
                    pos = target.axis.position + offset;
                    row = target.thePlantRow;
                }
                shield.GetComponent<SortingGroup>().sortingLayerName = $"particle{row}";
                shield.transform.position = pos;
                shield.transform.localScale = Vector3.one * 0.7f;
            }
        }

        public UltimateJalapeno plant => gameObject.GetComponent<UltimateJalapeno>();
    }

    public class ZombieData : MonoBehaviour
    {
        public bool isInFixDamage = false;

        public static void SetZombieInFixDamage(Zombie zombie, bool inFixDamage)
        {
            if (zombie == null) return;
            zombie.GetOrAddComponent<ZombieData>().isInFixDamage = inFixDamage;
        }

        public static bool GetZombieInFixDamage(Zombie zombie)
        {
            if (zombie == null) return false;
            return zombie.GetOrAddComponent<ZombieData>().isInFixDamage;
        }
    }

    #region 基础
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.Start))]
        [HarmonyPostfix]
        public static void PostAwake(ShootingManager __instance)
        {
            // 添加基础植物
            if (!__instance.AllPlants.Contains(PlantType.Jalapeno))
                __instance.AllPlants.Add(PlantType.Jalapeno);
            if (!__instance.RestPlants.Contains(PlantType.Jalapeno))
                __instance.RestPlants.Add(PlantType.Jalapeno);
        }

        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            // 添加config
            if (!Config.configs.ContainsKey(PlantType.Jalapeno) ||
                !Config.configs.ContainsKey(PlantType.ObsidianJalapeno) ||
                !Config.configs.ContainsKey(PlantType.UltimateJalapeno))
            {
                Config.configs.Add(PlantType.Jalapeno, new Shooting_Jalapeno());
                Config.configs.Add(PlantType.ObsidianJalapeno, new Shooting_ObsidianJalapeno());
                Config.configs.Add(PlantType.UltimateJalapeno, new Shooting_UltimateJalapeno());
            }
        }
    }
    #endregion

    #region 点击
    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPatch(nameof(Mouse.LeftClickWithNothing))]
        [HarmonyPrefix]
        public static bool PreLeftClickWithNothing(Mouse __instance)
        {
            if (__instance.theItemOnMouse == null && __instance.board != null && __instance.board.boardTag.rogueShooting)
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                foreach (var plant in __instance.GetPlantsOnMouse(Physics2D.RaycastAll(pos, Vector2.zero)))
                {
                    if (plant == null) continue;
                    if (plant.thePlantType != PlantType.UltimateJalapeno) continue;
                    var mouse = __instance;
                    mouse.cannonPlant = plant;
                    mouse.theItemOnMouse = UnityEngine.Object.Instantiate(GameAPP.itemPrefab[16], mouse.MousePosition,
                        Quaternion.identity, plant.board.transform);
                    mouse.theItemOnMouse.name = "cannon_ultimatejalapeno";
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Mouse.LeftClickWithSomeThing))]
        [HarmonyPostfix]
        public static void PostLeftClickWithSomeThing(Mouse __instance)
        {
            if (__instance.theItemOnMouse != null && __instance.theItemOnMouse.name == "cannon_ultimatejalapeno" &&
                __instance.cannonPlant != null && __instance.cannonPlant.thePlantType == PlantType.UltimateJalapeno &&
                __instance.cannonPlant.board != null && __instance.cannonPlant.board.boardTag.rogueShooting)
            {
                __instance.cannonPlant.GetComponent<UltimateJalapenoShooting>().SetTargetByMouse(__instance);
                __instance.ClearItemOnMouse(true);
            }
        }
    }
    #endregion

    #region 辣椒增幅
    [HarmonyPatch(typeof(Jalapeno))]
    public static class JalapenoPatch
    {
        [HarmonyPatch(nameof(Jalapeno.Start))]
        [HarmonyPostfix]
        public static void PostStart(Jalapeno __instance)
        {
            if (__instance != null && __instance.thePlantType == PlantType.Jalapeno &&
                __instance.board != null && __instance.board.boardTag.rogueShooting)
            {
                __instance.attackDamage *= 2;
            }
        }
    }
    #endregion
}
