using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using Core;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using MonoMod.RuntimeDetour;
using System.Collections;
using System.Reflection;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingPortalSniper.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.portalsniper", "RogueShootingPortalSniper", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_PortalPea>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_PortalSniper>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_PortalSniper.ShotBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_PortalSniper.UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_PortalSniper.SuperBuff>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<PortalSniperShooting>();
        }
    }

    public class Shooting_PortalPea : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_PortalPea(IntPtr ptr) : base(ptr) { }
        public Shooting_PortalPea() : base(ClassInjector.DerivedConstructorPointer<Shooting_PortalPea>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion
        public override PlantType PlantType => PlantType.PortalPea;
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
            new UpgradeBuff(PlantType.PortalPea, PlantType.UltimatePortalSniper)
        };

        public override void ReinforcePlant(Plant plant) { }
    }

    public class Shooting_PortalSniper : BaseConfig
    {
        // 聚精会神
        private static Lazy<UltiBuff> Buff = new(() =>
        {
            var result = (UltiBuff)41;
            foreach (var item in Il2CppSystem.Enum.GetValues(Il2CppType.From(typeof(UltiBuff))))
            {
                if (item.ToString() == "聚精会神")
                {
                    result = item.Unbox<UltiBuff>();
                    break;
                }
            }
            return result;
        });

        #region Il2Cpp构造函数
        public Shooting_PortalSniper(IntPtr ptr) : base(ptr) { }
        public Shooting_PortalSniper() : base(ClassInjector.DerivedConstructorPointer<Shooting_PortalSniper>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion
        public override PlantType PlantType => PlantType.UltimatePortalSniper;
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
            new DamageBuff(PlantType.UltimatePortalSniper),
            new ShotBuff(),
            new UniqueUpgrade(),
            new SuperBuff()
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.AddComponent<PortalSniperShooting>();
            plant.thePlantAttackInterval = 2f;
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 1f);
            InGameText.Instance.ShowText($"在该模式中，点击{Lawnf.GetName(PlantType.UltimatePortalSniper)}后再次点击僵尸会更换其为攻击目标", 3f);
        }

        internal void ResetQuality()
        {
            CustomBuffs[0] = new DamageBuff(PlantType.UltimatePortalSniper);
        }

        /// <summary>
        /// 强化：瞄准
        /// </summary>
        public class ShotBuff : BaseBuff
        {
            #region Il2Cpp构造函数
            public ShotBuff(IntPtr ptr) : base(ptr) { }
            public ShotBuff() : base(ClassInjector.DerivedConstructorPointer<ShotBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion
            public override float AppearWeight => 0.33f;
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimatePortalSniper;
            public override string Title => "强化：瞄准";
            public override string Description => "爆头所需次数-1";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    ++p.GetComponent<PortalSniperShooting>().shot;
                };
                SafeModify(action);
            }
        }

        /// <summary>
        /// 强化：连狙
        /// </summary>
        public class UniqueUpgrade : BaseBuff
        {
            #region Il2Cpp构造函数
            public UniqueUpgrade(IntPtr ptr) : base(ptr) { }
            public UniqueUpgrade() : base(ClassInjector.DerivedConstructorPointer<UniqueUpgrade>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion
            public override float AppearWeight => 0.33f;
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => PlantType.UltimatePortalSniper;
            public override string Title => "强化：连狙";
            public override string Description => "攻击触发连狙的概率+20%";
            public override void OnGet()
            {
                Action<Plant> action = (p) =>
                {
                    ++p.GetComponent<PortalSniperShooting>().unique;
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
            private const float PortalMaxTime = 9.223E+11f;
            internal float BallMaxTime => 5f;
            public override float AppearWeight => 0.05f;
            public override int MaxCount => 1;
            public override Quality Rarity => Quality.diamond;
            public override PlantType ShowType => PlantType.UltimatePortalSniper;
            public override string Title => "质变：领域展开";
            public override string Description => "获得词条：聚精会神\n在全场创建10组竖向传送门，每次爆头时在目标所在列生成一道时空冰流";
            internal bool created = false;
            internal List<ConnectPortal> portals = new();
            internal Dictionary<ConnectPortal.PortalBall, float> BallTimers = new();
            public override void OnGet()
            {
                if (TravelMgr.Instance != null)
                    TravelMgr.Instance.GetUltiBuff(Buff.Value);
                if ((!created) || portals.Any(p => p == null || p.IsDestroyed()))
                {
                    CreatePortals();
                    created = true;
                }
                Action<Plant> action = (p) =>
                {
                    p.GetComponent<PortalSniperShooting>().super = true;
                    foreach (var item in portals)
                        item.damage = p.attackDamage;
                    p.GetComponent<PortalSniperShooting>().portals = new(portals);
                    p.GetComponent<PortalSniperShooting>().buff = this;
                };
                SafeModify(action);
            }

            internal void CreatePortals()
            {
                for (int i = portals.Count - 1; i >= 0; i--)
                {
                    if (portals[i] != null && !portals[i].IsDestroyed())
                        UnityEngine.Object.Destroy(portals[i]);
                }
                portals.Clear();
                for (int i = 0; i < Board.Instance.columnNum; i++)
                    portals.Add(ConnectPortal.CreatePortalGroups(1, 0, Mouse.Instance.GetBoxXFromColumn(i),
                        PortalMaxTime, 0));
            }
        }
    }

    public class PortalSniperShooting : MonoBehaviour
    {
        public int shot = 0; // 爆头减少次数
        public int unique = 0; // 额外触发次数
        public bool super = false; // 
        public List<ConnectPortal> portals = new();
        public Shooting_PortalSniper.SuperBuff buff = null!;
        public Action<Il2CppSystem.Object> OnPlantClick = null!;

        //public void Start()
        //{
        //    if (plant == null) return;
        //    OnPlantClick = (obj) =>
        //    {
        //        var plant = obj.Cast<Plant>();
        //        if (plant == null) return;
        //        if (Mouse.Instance == null) return;
        //        var mouse = Mouse.Instance;
        //        if (plant.board != null && plant.thePlantType == PlantType.UltimatePortalSniper &&
        //            plant.board.boardTag.rogueShooting)
        //        {
        //            mouse.cannonPlant = plant;
        //            mouse.theItemOnMouse = Instantiate(GameAPP.itemPrefab[16], mouse.MousePosition,
        //                Quaternion.identity, plant.board.transform);
        //            mouse.theItemOnMouse.name = "cannon_portalsniper";
        //        }
        //    };
        //    EventManager.AddListener_obj(GameEvent.OnPlantClick, OnPlantClick);
        //}

        //public void OnDestroy()
        //{
        //    EventManager.RemoveListener(GameEvent.OnPlantClick, OnPlantClick);
        //}

        public void PostShoot()
        {
            if (unique == 0) return;
            if (UnityEngine.Random.Range(0, 100) < 20 * unique)
                plant.thePlantAttackCountDown = 0.05f;
        }

        public IEnumerator ResetCount()
        {
            while (plant != null)
            {
                yield return null;
                if (plant == null) yield break;
                if (plant.attackCount >= 6 - shot)
                {
                    plant.attackCount = 0;
                    plant.onShootTimes = 0;
                }
            }
        }

        public void OnAttackZombie(Zombie zombie, bool shot)
        {
            if (zombie == null) return;

            float multi = 1f;
            int flatBonus = 0;
            foreach (var kvp in plant.damageAdder)
            {
                if (kvp.Key == (PlantDamageAdder)24 || kvp.Key == (PlantDamageAdder)38)
                    flatBonus += (int)kvp.Value;
                else
                    multi += kvp.Value;
            }

            int dmg = flatBonus + (int)(multi * plant.attackDamage);
            dmg = Mathf.Max(dmg, 1);

            if (zombie.TryGetEffect<PortalEffect>(EffectType.Portal, out var effect))
                effect.totalDuration += plant.attackDamage * dmg / 15;

            if (!super) return;
            if (!shot) return;
            if (portals.Any(p => p == null || p.IsDestroyed()))
                if (buff != null)
                {
                    buff.CreatePortals();
                    portals.Clear();
                    portals = new(buff.portals);
                    buff.created = true;
                }
            if (portals.Count <= 0) return;
            portals[Mathf.Clamp(zombie.Column, 0, portals.Count - 1)].Shoot();
        }

        public PortalSniper plant => gameObject.GetComponent<PortalSniper>();
    }

    #region 基础
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.PortalPea) ||
                !Config.configs.ContainsKey(PlantType.UltimatePortalSniper))
            {
                Config.configs.Add(PlantType.PortalPea, new Shooting_PortalPea());
                Config.configs.Add(PlantType.UltimatePortalSniper, new Shooting_PortalSniper());
            }
            else
                Config.configs[PlantType.UltimatePortalSniper].Cast<Shooting_PortalSniper>().ResetQuality();
        }
    }

    [HarmonyPatch(typeof(GameLevel.RogueShooting.Peashooter))]
    public static class PeaShooterPatch
    {
        [HarmonyPatch(nameof(GameLevel.RogueShooting.Peashooter.Buffs), MethodType.Getter)]
        [HarmonyPostfix]
        public static void PostGetBuffs(ref Il2CppSystem.Collections.Generic.List<BaseBuff> __result)
        {
            __result.Add(new UpgradeBuff(PlantType.Peashooter, PlantType.PortalPea));
        }
    }
    #endregion

    [HarmonyPatch(typeof(PortalSniper))]
    public static class PortalSniperPatch
    {
        [HarmonyPatch(nameof(PortalSniper.Shoot1))]
        [HarmonyPostfix]
        public static void PostShoot1(PortalSniper __instance)
        {
            if (__instance != null && __instance.board != null && __instance.board.boardTag.rogueShooting &&
                __instance.thePlantType == PlantType.UltimatePortalSniper)
            {
                __instance.GetComponent<PortalSniperShooting>().PostShoot();
            }
        }

        [HarmonyPatch(nameof(PortalSniper.AttackZombie))]
        [HarmonyPrefix]
        public static void PreAttackZombie(PortalSniper __instance, ref Zombie zombie, ref int damage, 
            ref DamageType theDamageType, out long __state)
        {
            if (__instance != null && __instance.board != null && __instance.board.boardTag.rogueShooting &&
                __instance.thePlantType == PlantType.UltimatePortalSniper)
            {
                if (__instance.attackCount >= 6 - __instance.GetComponent<PortalSniperShooting>().shot)
                {
                    damage = 100_0000;
                    theDamageType = DamageType.MaxDamage;
                    __instance.StartCoroutine(__instance.GetComponent<PortalSniperShooting>().ResetCount());
                }
                __instance.GetComponent<PortalSniperShooting>().OnAttackZombie(zombie, theDamageType == DamageType.MaxDamage);
                __state = zombie.CurrentAllHealth;
            }
            else
                __state = -1;
        }

        [HarmonyPatch(nameof(PortalSniper.AttackZombie))]
        [HarmonyPostfix]
        public static void PostAttackZombie(PortalSniper __instance, ref Zombie zombie, ref int damage,
            ref DamageType theDamageType, long __state)
        {
            if (__instance != null && __instance.board != null && __instance.board.boardTag.rogueShooting &&
                __instance.thePlantType == PlantType.UltimatePortalSniper)
            {
                if ((__state != -1 && __state == zombie.CurrentAllHealth) || theDamageType == DamageType.MaxDamage)
                {
                    zombie.SetPortaled(float.PositiveInfinity);
                    if (zombie.TryGetEffect<PortalEffect>(EffectType.Portal, out var effect))
                        effect.totalDuration = float.PositiveInfinity;
                    else
                        zombie.TakeDamage(100_0000, __instance.Cast<IDamageMaker>(), DamageType.Normal, __instance.thePlantType);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_portalPea))]
    public static class Bullet_portalPeaPatch
    {
        [HarmonyPatch(nameof(Bullet_portalPea.HitZombie))]
        [HarmonyPostfix]
        public static void PostHitZombie(Bullet_portalPea __instance, ref Zombie zombie)
        {
            if (__instance != null && __instance.board != null && __instance.board.boardTag.rogueShooting &&
                __instance.fromType == PlantType.PortalPea)
            {
                if (zombie.TryGetEffect<PortalEffect>(EffectType.Portal, out var effect))
                {
                    effect.totalDuration += 60f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ConnectPortal))]
    public static class ConnectPortalPatch
    {
        [HarmonyPatch(nameof(ConnectPortal.Die))]
        [HarmonyPrefix]
        public static bool PreDie(ConnectPortal __instance)
        {
            if (__instance != null && Board.Instance != null && Board.Instance.boardTag.rogueShooting)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ConnectPortal.FixedUpdate))]
        [HarmonyPrefix]
        public static bool PreFixedUpdate(ConnectPortal __instance)
        {
            if (__instance != null && Board.Instance != null && Board.Instance.boardTag.rogueShooting)
            {
                if (__instance.main)
                {
                    for (int i = __instance.balls.Count - 1; i >= 0; i--)
                    {
                        if (__instance.balls[i] != null)
                            __instance.balls[i].OnFixedUpdate();
                    }
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ConnectPortal.AcceptBall))]
        [HarmonyPrefix]
        public static bool PreAcceptBall(ConnectPortal __instance, ref ConnectPortal.PortalBall ball)
        {
            // 给lpp代码打补丁
            if (__instance != null && Board.Instance != null && Board.Instance.boardTag.rogueShooting)
            {
                if (ball == null) return false;
                if (ball.transform == null) return false;
                if (ball.particle == null) return false;
                if (ball.sprite == null) return false;
                if (__instance == null) return false;
                if (__instance.portal_in == null || __instance.portal_out == null) return false;
                if (__instance.connectPortals == null) return false;
                foreach (var item in __instance.connectPortals)
                {
                    if (item.portal_in == null || item.portal_out == null) return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ConnectPortal._AcceptBall_d__8))]
    public static class ConnectPortalAcceptBallPatch
    {
        [HarmonyPatch(nameof(ConnectPortal._AcceptBall_d__8.MoveNext))]
        [HarmonyPrefix]
        public static bool PreMoveNext(ConnectPortal._AcceptBall_d__8 __instance)
        {
            // 给lpp代码打补丁
            if (__instance != null && Board.Instance != null && Board.Instance.boardTag.rogueShooting)
            {
                if (__instance.ball == null)
                {
                    __instance.__1__state = -2;
                    return false;
                }
                if (__instance.ball.transform == null || __instance.ball.particle == null ||
                    __instance.ball.sprite == null)
                {
                    __instance.__1__state = -2;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ConnectPortal.PortalBall))]
    public static class PortalBallPatch
    {
        [HarmonyPatch(nameof(ConnectPortal.PortalBall.OnFixedUpdate))]
        [HarmonyPrefix]
        public static bool PreOnFixedUpdate(ConnectPortal.PortalBall __instance)
        {
            if (__instance != null && Board.Instance != null && Board.Instance.boardTag.rogueShooting &&
                Config.configs.ContainsKey(PlantType.UltimatePortalSniper))
            {
                if (__instance == null) return true;
                var buff = Config.configs[PlantType.UltimatePortalSniper].Buffs[3].Cast<Shooting_PortalSniper.SuperBuff>();
                if (!buff.created || !buff.portals.Contains(__instance.portal)) return true;
                var targetPos = (__instance.direction == ConnectPortal.PortalBall.Direction.OutToIn ? 
                    __instance.portal.portal_in : 
                    __instance.portal.portal_out).transform.position;
                var direction = (targetPos - __instance.transform.position).normalized;
                foreach (var hit in Physics2D.RaycastAll(__instance.transform.position, direction, 0.1f, LayerMask.GetMask("Zombie")))
                {
                    if (hit.collider.TryGetComponent<Zombie>(out var zombie))
                    {
                        zombie.TakeDamage(__instance.portal.damage, null, DamageType.IceShieldless, PlantType.UltimatePortalSniper);
                        zombie.SetPortaled(0.02f);
                        if (zombie.TryGetEffect<PortalEffect>(EffectType.Poison, out var effect))
                            effect.totalDuration += 1f;
                        GameAPP.PlaySound(UnityEngine.Random.Range(0, 3), 0.5f, 1.0f);
                    }
                }

                if (buff.BallTimers.TryGetValue(__instance, out var timer))
                    buff.BallTimers[__instance] = timer + Time.deltaTime;
                else
                    buff.BallTimers.Add(__instance, Time.deltaTime);
                if (buff.BallTimers[__instance] >= buff.BallMaxTime)
                {
                    __instance.Die();
                    buff.BallTimers.Remove(__instance);
                    if (__instance.portal.balls.Contains(__instance))
                        __instance.portal.balls.Remove(__instance);
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ConnectPortal.PortalBall.Die))]
        [HarmonyPrefix]
        public static bool PreDie(ConnectPortal.PortalBall __instance)
        {
            if (__instance != null && Board.Instance != null && Board.Instance.boardTag.rogueShooting &&
                Config.configs.ContainsKey(PlantType.UltimatePortalSniper))
            {
                if (__instance == null) return true;
                var buff = Config.configs[PlantType.UltimatePortalSniper].Buffs[3].Cast<Shooting_PortalSniper.SuperBuff>();
                if (!buff.created || !buff.portals.Contains(__instance.portal)) return true;
                //var pos = __instance.transform.position;
                //Board.Instance.boardAction.CreateCherryExplode(pos, Mouse.Instance.GetRowFromY(pos.x, pos.y), 
                //    CherryBombType.IceCharry, __instance.portal.damage, PlantType.UltimatePortalSniper);
                UnityEngine.Object.Destroy(__instance.transform.gameObject);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPatch(nameof(Mouse.LeftClickWithNothing))]
        [HarmonyPrefix]
        public static bool PreLeftClickWithNothing(Mouse __instance)
        {
            if (__instance.theItemOnMouse == null && __instance.board != null &&  __instance.board.boardTag.rogueShooting)
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                foreach (var plant in __instance.GetPlantsOnMouse(Physics2D.RaycastAll(pos, Vector2.zero)))
                {
                    if (plant == null) continue;
                    if (plant.thePlantType != PlantType.UltimatePortalSniper) continue;
                    var mouse = __instance;
                    mouse.cannonPlant = plant;
                    mouse.theItemOnMouse = UnityEngine.Object.Instantiate(GameAPP.itemPrefab[16], mouse.MousePosition,
                        Quaternion.identity, plant.board.transform);
                    mouse.theItemOnMouse.name = "cannon_portalsniper";
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Mouse.LeftClickWithSomeThing))]
        [HarmonyPostfix]
        public static void PostLeftClickWithSomeThing(Mouse __instance)
        {
            if (__instance.theItemOnMouse != null && __instance.theItemOnMouse.name == "cannon_portalsniper" && 
                __instance.cannonPlant != null &&  __instance.cannonPlant.thePlantType == PlantType.UltimatePortalSniper && 
                __instance.cannonPlant.board != null &&  __instance.cannonPlant.board.boardTag.rogueShooting)
            {
                var plant = __instance.cannonPlant.GetComponent<PortalSniper>();
                foreach (var col in Physics2D.OverlapPointAll(__instance.MousePosition, __instance.cannonPlant.zombieLayer))
                {
                    if (col == null) continue;
                    if (!col.TryGetComponent<Zombie>(out var zombie)) continue;
                    if (zombie == null) continue;
                    if (!plant.SearchUniqueZombie(zombie)) continue;
                    plant.targetZombie = zombie;
                }
                __instance.ClearItemOnMouse(true);
            }
        }
    }
}
