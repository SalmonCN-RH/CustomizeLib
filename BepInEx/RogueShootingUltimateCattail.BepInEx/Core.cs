using BepInEx;
using BepInEx.Unity.IL2CPP;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Core = global::Core;

namespace RogueShootingUltimateCattail.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.ultimatecaattail", "RogueShootingUltimateCattail", "1.0.0")]
    public class Core : BasePlugin
    {
        public static List<PlantType> ToLandPlants = new()
        {
            PlantType.CattailPlant,
            PlantType.FireCattail,
            PlantType.UltimateCattail
        };

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_Cattail>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_FireCattail>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateCattail>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateCattail.UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateCattail.BulletBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateCattail.SuperBuff>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<UltimateCattailShooting>();
        }
    }

    public class Shooting_Cattail : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_Cattail(IntPtr ptr) : base(ptr) { }
        public Shooting_Cattail() : base(ClassInjector.DerivedConstructorPointer<Shooting_Cattail>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.CattailPlant;
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
            new UpgradeBuff(PlantType.CattailPlant, PlantType.FireCattail)
        };
        public override void ReinforcePlant(Plant plant)
        {
            plant.ModifyDamage(PlantDamageAdder.Shooting, 17f, false, new());
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 1f);
        }
    }

    public class Shooting_FireCattail : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_FireCattail(IntPtr ptr) : base(ptr) { }
        public Shooting_FireCattail() : base(ClassInjector.DerivedConstructorPointer<Shooting_FireCattail>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.FireCattail;
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
            new UpgradeBuff(PlantType.FireCattail, PlantType.UltimateCattail)
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.ModifyDamage(PlantDamageAdder.Shooting, 11f, false, new());
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 1f);
        }
    }

    public class Shooting_UltimateCattail : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_UltimateCattail(IntPtr ptr) : base(ptr) { }
        public Shooting_UltimateCattail() : base(ClassInjector.DerivedConstructorPointer<Shooting_UltimateCattail>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.UltimateCattail;
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
            new DamageBuff(PlantType.UltimateCattail),
            new UniqueUpgrade(),
            new BulletBuff(),
            new SuperBuff()
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.AddComponent<UltimateCattailShooting>();
        }

        internal void ResetQuality()
        {
            CustomBuffs[0] = new DamageBuff(PlantType.UltimateCattail);
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
            public override PlantType ShowType => PlantType.UltimateCattail;
            public override string Title => "强化：节能";
            public override string Description => "发射射线所需能量-24";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<UltimateCattailShooting>().unique++;
                };
                SafeModify(action);
            }
        }

        public class BulletBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public BulletBuff(IntPtr ptr) : base(ptr) { }
            public BulletBuff() : base(ClassInjector.DerivedConstructorPointer<BulletBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.167f;
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimateCattail;
            public override string Title => "强化：走火";
            public override string Description => "大招期间子弹发射间隔-0.1";

            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<UltimateCattailShooting>().bullet++;
                };
                SafeModify(action);
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
            public override PlantType ShowType => PlantType.UltimateCattail;
            public override string Title => "质变：波与粒的境界";
            public override string Description => "究极爆焱猫尾草的射线会以 境符「波与粒的境界」方式发射";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<UltimateCattailShooting>().super = true;
                    p.GetComponent<UltimateCattailShooting>().InitSuper();
                    p.GetComponent<UltimateCattail>().laserFrom.gameObject.SetActive(false);
                };
                SafeModify(action);
            }
        }
    }

    public class UltimateCattailShooting : MonoBehaviour
    {
        private const float startSpeed = 60f;
        private readonly (float min, float max) range = (0, 300);

        public List<GameObject> Lines = new();
        public bool laser = false; // 质变开大
        public int unique = 0; // 减大招所需子弹
        public int bullet = 0; // 大招发射子弹
        public bool super = false;
        private bool set = false;
        private float timer = 0f;
        private bool shooting = false;
        private int counter = 0;
        private bool smallToBig = true;

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (Time.timeScale <= 0f) return;
            if (plant == null) return;
            if (plant.theStatus == PlantStatus.Default && !set)
            {
                if (plant.attributeCount > 240 - unique * 24 || unique >= 10)
                {
                    plant.attributeCount = 0;
                    plant.anim.SetTrigger("shoot2");
                    plant.theStatus = PlantStatus.UltimateCattail_preShoot;
                    plant.UpdateText();
                }
            }
            if (smallToBig)
            {
                timer += Time.deltaTime;
                if (timer >= range.max)
                    smallToBig = false;
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer <= range.min)
                    smallToBig = true;
            }
            if (laser)
            {
                foreach (var line in Lines)
                {
                    line.transform.Rotate(0f, 0f, startSpeed * timer * Time.deltaTime);
                }
            }
        }

        public void FixedUpdate()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (Time.timeScale <= 0f) return;
            if (shooting)
            {
                counter += 1;
                if (counter >= -5 * bullet + 30) // t = -5x+30（单位：0.02s）
                {
                    if (super)
                    {
                        for (int i = 0; i < Lines.Count; i++)
                        {
                            var line = Lines[i].transform;
                            var bullet = CreateBullet.Instance.SetBullet(line.position.x, line.position.y, plant.thePlantRow,
                                BulletType.Bullet_ultimateCattail, BulletMoveWay.Free);
                            var direction = global::Core.Lawnf.GetVectorFromQuaternion(line.transform.localRotation);
                            bullet.transform.right = direction;
                            bullet.Damage = plant.attackDamage;
                            bullet.fromType = plant.thePlantType;
                        }
                    }
                    else
                    {
                        var line = plant.laserFrom;
                        var bullet = CreateBullet.Instance.SetBullet(line.position.x, line.position.y, plant.thePlantRow,
                            BulletType.Bullet_ultimateCattail, BulletMoveWay.Free);
                        var direction = global::Core.Lawnf.GetVectorFromQuaternion(line.transform.localRotation);
                        bullet.transform.right = direction;
                        bullet.Damage = plant.attackDamage;
                        bullet.fromType = plant.thePlantType;
                    }
                    counter = 0;
                }
            }
            else
                counter = 0;
            if (laser)
            {
                foreach (var line in Lines)
                {
                    var direction = global::Core.Lawnf.GetVectorFromQuaternion(line.transform.localRotation);
                    foreach (var hit in Physics2D.RaycastAll(line.transform.position, direction, float.MaxValue, plant.zombieLayer))
                    {
                        if (hit.collider == null) continue; 
                        if (!hit.collider.TryGetComponent<Zombie>(out var zombie)) continue;
                        if (zombie == null) continue;
                        switch (zombie.theStatus)
                        {
                            case ZombieStatus.Miner_digging:
                            case ZombieStatus.Miner_rising:
                            case ZombieStatus.Boss:
                            case ZombieStatus.Bungi_down:
                            case ZombieStatus.Bungi_up:
                            case ZombieStatus.Bungi_awake:
                                continue;
                        }

                        zombie.SetJalaed();
                        plant.board.boardAction.CreateCherryExplode(hit.collider.bounds.center, zombie.theZombieRow,
                            CherryBombType.BulletAll, 6 * plant.attackDamage, plant.thePlantType);
                        zombie.KnockBack(2f, Zombie.KnockBackReason.ByJalapeno);
                    }
                }
            }
        }

        public void InitSuper()
        {
            for (int i = 0; i < 3; i++)
            {
                var newLine = Instantiate(plant.laserFrom.gameObject, plant.transform).gameObject;
                newLine.transform.FindChild("ShootFire/Line").GetComponent<ParticleSystem>().main.simulationSpeed = 0.3f;
                newLine.transform.Rotate(0f, 0f, 120f * i);
                Lines.Add(newLine);
            }
        }

        public void OnLaserStart()
        {
            shooting = true;
            if (super)
            {
                foreach (var line in Lines)
                {
                    line.transform.GetChild(0).gameObject.SetActive(true);
                }
                laser = true;
            }
        }

        public void OnLaserEnd()
        {
            if (unique >= 10)
            {
                plant.anim.Play("shoot2", 1, 0.32f); // 试出来0.32是开始射击
                set = true;
            }
            else
                shooting = false;
            if (super && unique < 10)
            {
                foreach (var line in Lines)
                {
                    line.transform.GetChild(0).gameObject.SetActive(false);
                }
                laser = false;
            }
            if (super && unique >= 10)
                laser = true;
        }

        public UltimateCattail plant => gameObject.GetComponent<UltimateCattail>();
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
            if (!__instance.AllPlants.Contains(PlantType.CattailPlant))
                __instance.AllPlants.Add(PlantType.CattailPlant);
            if (!__instance.RestPlants.Contains(PlantType.CattailPlant))
                __instance.RestPlants.Add(PlantType.CattailPlant);
        }

        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            // 添加config
            if (!Config.configs.ContainsKey(PlantType.CattailPlant) ||
                !Config.configs.ContainsKey(PlantType.FireCattail) ||
                !Config.configs.ContainsKey(PlantType.UltimateCattail))
            {
                Config.configs.Add(PlantType.CattailPlant, new Shooting_Cattail());
                Config.configs.Add(PlantType.FireCattail, new Shooting_FireCattail());
                Config.configs.Add(PlantType.UltimateCattail, new Shooting_UltimateCattail());
            }
            else
                Config.configs[PlantType.UltimateCattail].Cast<Shooting_UltimateCattail>().ResetQuality();
        }
    }
    #endregion

    #region 猫猫上陆地
    [HarmonyPatch(typeof(TypeMgr))]
    public static class TypeMgrPatch
    {
        [HarmonyPatch(nameof(TypeMgr.IsWaterPlant))]
        [HarmonyPostfix]
        public static void PostIsWaterPlant(PlantType theSeedType, ref bool __result)
        {
            if (Board.Instance != null && Board.Instance.boardTag.rogueShooting)
            {
                if (Core.ToLandPlants.Contains(theSeedType))
                    __result = false;
            }
        }
    }
    #endregion

    #region 猫猫增幅
    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch(nameof(Plant.Start))]
        [HarmonyPostfix]
        public static void PostStart(Plant __instance)
        {
            if (__instance != null && __instance.thePlantType == PlantType.CattailPlant &&
                __instance.board != null && __instance.board.boardTag.rogueShooting)
            {
                __instance.attackDamage = 360;
                __instance.ModifySpeed(PlantSpeedAdder.Shooting, 1f);
            }
        }
    }
    #endregion

    #region 究火猫词条
    [HarmonyPatch(typeof(UltimateCattail))]
    public static class UltimateCattailPatch
    {
        [HarmonyPatch(nameof(UltimateCattail.AttributeEvent))]
        [HarmonyPrefix]
        public static bool PreAttributeEvent(UltimateCattail __instance)
        {
            if (__instance != null && __instance.board != null && __instance.board.boardTag.rogueShooting &&
                __instance.GetComponent<UltimateCattailShooting>().super)
            {
                __instance.attributeCountdown = 0.15f;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(UltimateCattail.AnimLaserUp))]
        [HarmonyPostfix]
        public static void PostAnimLaserUp(UltimateCattail __instance)
        {
            if (__instance != null && __instance.board != null && __instance.board.boardTag.rogueShooting &&
                __instance.thePlantType == PlantType.UltimateCattail)
            {
                __instance.GetComponent<UltimateCattailShooting>().OnLaserStart();
            }
        }

        [HarmonyPatch(nameof(UltimateCattail.AnimLaserOver))]
        [HarmonyPostfix]
        public static void PostAnimLaserOver(UltimateCattail __instance)
        {
            if (__instance != null && __instance.board != null && __instance.board.boardTag.rogueShooting &&
                __instance.thePlantType == PlantType.UltimateCattail)
            {
                __instance.GetComponent<UltimateCattailShooting>().OnLaserEnd();
            }
        }
    }
    #endregion
}
