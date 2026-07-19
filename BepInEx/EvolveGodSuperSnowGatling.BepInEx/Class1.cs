using BepInEx;
using BepInEx.Unity.IL2CPP;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace EvolveGodSuperSnowGatling.BepInEx
{
#pragma warning disable
    [BepInPlugin("salmon.evolvegodsupersnowgatling", "EvolveGodSuperSnowGatling", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // ClassInjector.RegisterTypeInIl2Cpp<CustomGatlingPea>();
            ClassInjector.RegisterTypeInIl2Cpp<SnowGatling>();
            ClassInjector.RegisterTypeInIl2Cpp<SuperSnowGatling_Shooting>();
            ClassInjector.RegisterTypeInIl2Cpp<UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<BombBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<BulletBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<ShootingDataSave>();
        }
    }

    public class ShootingDataSave : MonoBehaviour
    {
        public static ShootingDataSave Instance;

        public bool ChangeBullet = false;
        public bool Explode = false;

        public void Awake() => Instance = this;
    }


    public class SnowGatling : BaseConfig
    {
        // 实现IntPtr构造方法
        public SnowGatling(IntPtr ptr) : base(ptr) { }
        public SnowGatling() : base(ClassInjector.DerivedConstructorPointer<SnowGatling>()) => ClassInjector.DerivedConstructorBody(this);
        // 实现抽象类的方法
        public PlantType PlantType => PlantType.SnowGatling;
        public Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }
        public override void ReinforcePlant(Plant plant)
        {
            plant.ModifyDamage(PlantDamageAdder.Shooting, 19.0f, false, new Il2CppSystem.Nullable<float>(float.MaxValue));
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 1f);
        }

        public override string Role => "输出";

        // 自定义的方法

        private List<BaseBuff> CustomBuffs = new List<BaseBuff> { new UpgradeBuff(PlantType.SnowGatling, PlantType.SuperSnowGatling) };
    }

    public class SuperSnowGatling_Shooting : BaseConfig
    {
        // 实现IntPtr构造方法
        public SuperSnowGatling_Shooting(IntPtr ptr) : base(ptr) { }
        public SuperSnowGatling_Shooting() : base(ClassInjector.DerivedConstructorPointer<SuperSnowGatling_Shooting>()) => ClassInjector.DerivedConstructorBody(this);
        // 实现抽象类的方法
        public override PlantType PlantType => PlantType.SuperSnowGatling;
        public override Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }

        public override string Role => "输出";

        public override void ReinforcePlant(Plant plant)
        {
            plant.ModifyDamage(PlantDamageAdder.Shooting, 14.0f, false, new Il2CppSystem.Nullable<float>(float.MaxValue));
        }
        // 自定义的方法

        public void ResetQuality()
        {
            CustomBuffs[0].Cast<DamageBuff>().randomQuality = ShootingManager.Instance.GetRandomQuality();
            CustomBuffs[1].Cast<SpeedBuff>().randomQuality = ShootingManager.Instance.GetRandomQuality();
        }

        private List<BaseBuff> CustomBuffs = new List<BaseBuff> { new DamageBuff(PlantType.SuperSnowGatling), new SpeedBuff(PlantType.SuperSnowGatling), 
            new UniqueUpgrade(), new BombBuff(), new BulletBuff() };
    }

    public class UniqueUpgrade : BaseBuff
    {
        // 实现IntPtr构造方法
        public UniqueUpgrade(IntPtr ptr) : base(ptr) { }
        public UniqueUpgrade() : base(ClassInjector.DerivedConstructorPointer<UniqueUpgrade>()) => ClassInjector.DerivedConstructorBody(this);

        // 实现抽象类的方法
        public override PlantType ShowType => PlantType.SuperSnowGatling;
        public override string Title => "强化：极冰";
        public override string Description => "植物施加的冻结值+100%。寒冰豌豆每增加1点冻结值，子弹伤害+1%";
        public override void OnGet()
        {
            if (ShootingManager.Instance.TryGetPlant(PlantType.SuperSnowGatling, out var plant) && plant != null)
            {
                plant.attributeFloat = (int)plant.attributeFloat + 1;
            }
        }
        public override int MaxCount => 10;
        public override float AppearWeight => 0.33f;
        public override Quality Rarity => Quality.gold;
    }

    public class BombBuff : BaseBuff
    {
        // 实现IntPtr构造方法
        public BombBuff(IntPtr ptr) : base(ptr) { }
        public BombBuff() : base(ClassInjector.DerivedConstructorPointer<BombBuff>()) => ClassInjector.DerivedConstructorBody(this);
        // 实现抽象类的方法
        public override PlantType ShowType => PlantType.SuperSnowGatling;
        public override string Title => "质变：凝冰之心";
        public override string Description => "寒冰豌豆击中目标时，额外造成1次100%原伤害的范围伤害";
        public override void OnGet()
        {
            if (Board.Instance != null)
                Board.Instance.gameObject.GetOrAddComponent<ShootingDataSave>().Explode = true;
        }
        public override int MaxCount => 1;
        public override float AppearWeight => 0.05f;
        public override Quality Rarity => Quality.diamond;
    }

    public class BulletBuff : BaseBuff
    {
        // 实现IntPtr构造方法
        public BulletBuff(IntPtr ptr) : base(ptr) { }
        public BulletBuff() : base(ClassInjector.DerivedConstructorPointer<BulletBuff>()) => ClassInjector.DerivedConstructorBody(this);
        // 实现抽象类的方法
        public override PlantType ShowType => PlantType.SuperSnowGatling;
        public override string Title => "质变：冰封王座";
        public override string Description => "寒冰豌豆升级为极冰豆\n其子弹赋予的冻结值翻倍";
        public override void OnGet()
        {
            if (Board.Instance != null)
                Board.Instance.gameObject.GetOrAddComponent<ShootingDataSave>().ChangeBullet = true;
            if (TravelMgr.Instance != null)
                TravelMgr.Instance.GetNormalBuff((AdvBuff)1011);
        }
        public override int MaxCount => 1;
        public override float AppearWeight => 0.05f;
        public override Quality Rarity => Quality.diamond;
    }

    [HarmonyPatch(typeof(Zombie))]
    public static class ZombiePatch
    {
        [HarmonyPatch(nameof(Zombie.AddfreezeLevel))]
        [HarmonyPrefix]
        public static void PreAddfreezeLevel(ref int level)
        {
            if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant(PlantType.SuperSnowGatling, out var plant))
                level = level * (1 + (int)plant.attributeFloat);
        }
    }

    [HarmonyPatch(typeof(Bullet_snowPea))]
    public static class Bullet_snowPeaPatch
    {
        [HarmonyPatch(nameof(Bullet_snowPea.HitZombie))]
        [HarmonyPrefix]
        public static void PreHitZombie(Bullet_snowPea __instance)
        {
            if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant(PlantType.SuperSnowGatling, out var plant) &&
                __instance.theBulletType == BulletType.Bullet_snowPea && plant != null)
            {
                __instance.Damage += (int)(__instance.Damage * ((10 * (int)plant.attributeFloat) * 0.01f * (int)plant.attributeFloat));
                if (Board.Instance == null || Board.Instance.GetOrAddComponent<ShootingDataSave>() == null) return;

                if (Board.Instance.GetOrAddComponent<ShootingDataSave>().Explode)
                {
                    foreach (var col in Physics2D.OverlapCircleAll(__instance.transform.position, 1.5f, LayerMask.GetMask("Zombie")))
                    {
                        if (col == null || col.IsDestroyed() || col.gameObject == null || col.gameObject.IsDestroyed()) continue;
                        if (!col.gameObject.TryGetComponent<Zombie>(out var z) || z == null || z.IsDestroyed()) continue;
                        if (!Lawnf.InLandStatus(z.theStatus)) continue;
                        var damage = __instance.Damage;
                        if (z.HasBuff(EffectType.Freeze)) damage *= 4;
                        z.TakeDamage(damage, __instance.Cast<IDamageMaker>(), DamageType.IceAll, __instance.fromType);
                        z.AddfreezeLevel(5);
                        z.SetCold(10f);
                    }
                    CreateParticle.SetParticle(97, __instance.transform.position, __instance.theBulletRow, true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_extremeSnowPea))]
    public static class Bullet_extremeSnowPeaPatch
    {
        [HarmonyPatch(nameof(Bullet_extremeSnowPea.HitZombie))]
        [HarmonyPrefix]
        public static void PreHitZombie(Bullet_extremeSnowPea __instance)
        {
            if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant(PlantType.SuperSnowGatling, out var plant) &&
                __instance.theBulletType == BulletType.Bullet_extremeSnowPea && plant != null)
            {
                __instance.Damage += (int)(__instance.Damage * ((20 * (int)plant.attributeFloat * 2) * 0.01f * (int)plant.attributeFloat));
                if (Board.Instance == null || Board.Instance.GetOrAddComponent<ShootingDataSave>() == null) return;
                if (Board.Instance.GetOrAddComponent<ShootingDataSave>().Explode)
                {
                    foreach (var col in Physics2D.OverlapCircleAll(__instance.transform.position, 1.5f, LayerMask.GetMask("Zombie")))
                    {
                        if (col == null || col.IsDestroyed() || col.gameObject == null || col.gameObject.IsDestroyed()) continue;
                        if (!col.gameObject.TryGetComponent<Zombie>(out var z) || z == null || z.IsDestroyed()) continue;
                        if (!Lawnf.InLandStatus(z.theStatus)) continue;
                        z.TakeDamage(__instance.Damage / 2, __instance.Cast<IDamageMaker>(), DamageType.IceAll, __instance.fromType);
                        z.AddfreezeLevel(5);
                        z.SetCold(10f);
                    }
                    CreateParticle.SetParticle(97, __instance.transform.position, __instance.theBulletRow, true);
                }
            }

        }
    }

    [HarmonyPatch(typeof(SuperSnowGatling))]
    public static class SuperSnowGatlingPatch
    {
        [HarmonyPatch(nameof(SuperSnowGatling.GetBulletType))]
        [HarmonyPostfix]
        public static void PostGetBulletType(SuperSnowGatling __instance, ref BulletType __result)
        {
            if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant(PlantType.SuperSnowGatling, out var plant) &&
                Board.Instance.GetOrAddComponent<ShootingDataSave>().ChangeBullet)
            {
                __instance.attributeCount = -1;
                __result = BulletType.Bullet_extremeSnowPea;
            }
        }
    }

    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void ShowBuff()
        {
            if (Config.configs != null)
            {
                if (!Config.configs.ContainsKey(PlantType.SnowGatling))
                {
                    // Config.configs[PlantType.Peashooter] = new CustomGatlingPea();
                    Config.configs.Add(PlantType.SnowGatling, new SnowGatling());
                    Config.configs.Add(PlantType.SuperSnowGatling, new SuperSnowGatling_Shooting());
                }
                else
                    Config.configs[PlantType.SuperSnowGatling].Cast<SuperSnowGatling_Shooting>().ResetQuality();
            }
        }
    }

    [HarmonyPatch(typeof(PlantDataMenu))]
    public static class PlantDataMenuPatch
    {
        [HarmonyPatch(nameof(PlantDataMenu.Start))]
        [HarmonyPostfix]
        public static void PostStart(PlantDataMenu __instance)
        {
            if (__instance != null && __instance.gameObject != null && !__instance.IsDestroyed() && !__instance.gameObject.IsDestroyed() &&
                __instance.plant != null && __instance.plant.gameObject != null && !__instance.plant.IsDestroyed() && !__instance.plant.gameObject.IsDestroyed()
                && __instance.plant.thePlantType == PlantType.SuperSnowGatling && ShootingManager.Instance != null)
            {
                var str = $"子弹攻击力加成：{(((Board.Instance.GetOrAddComponent<ShootingDataSave>().ChangeBullet ? 20 : 10) * (int)__instance.plant.attributeFloat * (int)__instance.plant.attributeFloat))}%\n";
                foreach (var text in __instance.infoText)
                    text.text += str;
            }
        }
    }

    [HarmonyPatch(typeof(Peashooter))]
    public static class PeashooterPatch
    {
        [HarmonyPatch(nameof(Peashooter.Buffs), MethodType.Getter)]
        [HarmonyPostfix]
        public static void PostGetBuffs(ref Il2CppSystem.Collections.Generic.List<BaseBuff> __result)
        {
            __result.Add(new UpgradeBuff(PlantType.Peashooter, PlantType.SnowGatling));
        }
    }
}
