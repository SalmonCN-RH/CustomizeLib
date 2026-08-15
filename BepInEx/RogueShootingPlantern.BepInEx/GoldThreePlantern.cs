using BepInEx.Unity.IL2CPP.Utils;
using Core;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using RogueShootingPlantern.BepInEx.ShootingUltimatePlantern;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingPlantern.BepInEx.ShootingGoldThreePlantern
{
    internal static class GoldThreePlanternCore
    {
        internal static void OnLoad()
        {
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_ThreePlantern>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_GoldThreePlantern>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_GoldThreePlantern.UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_GoldThreePlantern.SuperUpgrade>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<GoldThreePlanternShooting>();
        }
    }

    public class Shooting_ThreePlantern : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_ThreePlantern(IntPtr ptr) : base(ptr) { }
        public Shooting_ThreePlantern() : base(ClassInjector.DerivedConstructorPointer<Shooting_ThreePlantern>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.ThreePlantern;
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
            new UpgradeBuff(PlantType.ThreePlantern, PlantType.GoldThreePlantern)
        };

        public override void ReinforcePlant(Plant plant) { }
    }

    public class Shooting_GoldThreePlantern : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_GoldThreePlantern(IntPtr ptr) : base(ptr) { }
        public Shooting_GoldThreePlantern() : base(ClassInjector.DerivedConstructorPointer<Shooting_GoldThreePlantern>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.GoldThreePlantern;
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
            new DamageBuff(PlantType.GoldThreePlantern),
            new SpeedBuff(PlantType.GoldThreePlantern),
            new UniqueUpgrade(),
            new SuperUpgrade()
        };

        public override void ReinforcePlant(Plant plant)
        {
            if (Money.Instance != null) Money.Instance.EnableMoneyBank();
            else if (InGameUI.Instance != null && InGameUI.Instance.MoneyBank != null) 
                InGameUI.Instance.MoneyBank.SetActive(true);

            plant.attackDamage = 900;
            plant.AddComponent<GoldThreePlanternShooting>();
        }

        internal void ResetQuality()
        {
            CustomBuffs[0] = new DamageBuff(PlantType.GoldThreePlantern);
            CustomBuffs[1] = new SpeedBuff(PlantType.GoldThreePlantern);
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
            public override PlantType ShowType => PlantType.GoldThreePlantern;
            public override string Title => "强化：大富翁";
            public override string Description => "每拥有1金钱，激光的伤害+0.01%";
            public override void OnGet()
            {
                // 由自定义类通过shootingmanger的api实现功能
            }
        }

        public class SuperUpgrade : BaseBuff
        {
            #region Il2Cpp构造函数
            public SuperUpgrade(IntPtr ptr) : base(ptr) { }
            public SuperUpgrade() : base(ClassInjector.DerivedConstructorPointer<SuperUpgrade>()) =>
                ClassInjector.DerivedConstructorBody(this);
            #endregion

            public override float AppearWeight => 0.05f;
            public override int MaxCount => 1;
            public override Quality Rarity => Quality.diamond;
            public override PlantType ShowType => PlantType.GoldThreePlantern;
            public override string Title => "质变：陨星炮";
            public override string Description => 
                "对摇钱三线路灯花使用金咖啡豆使其进入蓄力状态，期间每0.02秒消耗200钱币，再次使用金咖啡豆或钱币数低于200时退出蓄力状态并立即对前方僵尸造成一次蓄力时间倍的伤害，范围随蓄力时间增加而增加";
            public override void OnGet()
            {
                // 由自定义类+patch通过shootingmanger的api实现功能
            }
        }
    }

    public class GoldThreePlanternShooting : MonoBehaviour
    {
        public static bool super => ShootingManager.Instance == null ? false : 
            ShootingManager.Instance.GetBuffChoiceCount(PlantType.GoldThreePlantern, "质变：陨星炮") >= 1;

        private GameObject laser = null!;
        private bool charge = false;
        private float chargeTimer = 0f;
        private int counter = 0;

        public void Awake()
        {
            laser = Resources.Load<GameObject>("plants/plantern/threegoldplantern/YellowLaser");

            plant.attributeCountdown = 1.5f;
        }

        public void Start()
        {
            if (plant == null) return;
            if (plant.healthSlider == null) return;
            foreach (var item in plant.healthSlider.registedTexts)
                Destroy(item.Key.gameObject);
            plant.healthSlider.registedTexts.Clear();
            Func<string> func = () =>
            {
                var timer = 0f;
                if (plant != null)
                    timer = chargeTimer;
                return $"已蓄力:{timer:F2}秒";
            };
            plant.healthSlider.RegisterText(Color.cyan, func, new(new Vector2(100f, 15f)));
        }

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (plant == null) return;
            if (Time.timeScale <= 0f) return;

            if (plant.attributeCountdown <= 0f)
            {
                AttackZombie();
                plant.attributeCountdown = 1.5f;
            }
        }

        public void FixedUpdate()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (plant == null) return;
            if (plant.board == null) return;
            if (Time.timeScale <= 0f) return;

            if (charge)
            {
                chargeTimer += Time.fixedDeltaTime * plant.attributeSpeed;
                plant.board.UseMoney(200);
                counter++;
                if (counter >= 5)
                {
                    ParticleManager.Instance.SetParticle(ParticleType.SuperKillEffect, plant.axis.position + new Vector3(0f, 0.5f, 0f));
                    counter = 0;
                }
                if (plant.Timers.Length > 3)
                    plant.Timers[3] = 1f;
                if (plant.board.theMoney < 200)
                {
                    OnChargeOver();
                    charge = !charge;
                }
                plant.UpdateText();
            }
        }

        public bool CanSuperSkill()
        {
            if (!super) return false;
            if (plant.flashCountDown > 0f) return false;
            return true;
        }

        public void OnSuperSkill()
        {
            charge = !charge;
            plant.flashCountDown = 0f;

            ParticleManager.Instance.SetParticle(ParticleType.SuperKillEffect, plant.axis.position + new Vector3(0f, 0.5f, 0f));

            plant.Recover(plant.thePlantMaxHealth, DamageType.Normal, false);
            GameAPP.PlaySound(SoundType.Prize, 0.5f, 1f);

            if (!charge) // 蓄力结束
            {
                OnChargeOver();
            }
        }

        public void OnChargeOver()
        {
            if (plant == null || plant.board == null) return;

            // 确定范围&找僵尸
            var range = 0;
            if (1 < chargeTimer && chargeTimer <= 3)
                range = 1;
            else if (chargeTimer > 3)
                range = 2;
            SetLines(range);

            var plantx = plant.axis.position.x;
            var zombies = Lawnf.GetAllZombies().ToArray().Where(z => z != null && z.Alive). // 还活着的
                Where(z => plantx < z.axis.position.x). // 在植物右侧的
                Where(z => Mathf.Abs(z.theZombieRow - plant.thePlantRow) <= range);

            var dmg = (int)(GetDamage() * chargeTimer);
            foreach (var zombie in zombies)
            {
                zombie.TakeDamage(dmg, plant.Cast<IDamageMaker>(), DamageType.NormalAll, plant.thePlantType);
                plant.board.GetMoney(150f);
            }

            if (plant.Timers.Length > 3)
                plant.Timers[3] = 0f;
            chargeTimer = 0f;
            plant.UpdateText();
        }

        public void AttackZombie()
        {
            if (charge) return;
            if (plant == null) return;
            if (plant.board == null) return;
            var target = SearchZombie();
            if (target == null) return;

            var line = Instantiate(laser, plant.board.transform).GetComponent<LineRenderer>();
            var offset = new Vector3(0f, 0.5f, 0f);
            var direction = (target.axis.position - plant.axis.position + new Vector3(0f, 0.2f, 0f)).normalized;
            var start = plant.axis.position + offset;
            float distance = (target.deadRight - target.deadLeft) * 10; // 设置距离极大值，这样就省的处理屏幕边界了
            line.SetPosition(0, start);
            line.SetPosition(1, start + offset + direction * distance);
            line.sortingLayerName = "particle11";

            int damage = GetDamage();
            foreach (var hit in Physics2D.RaycastAll(start, direction, float.MaxValue, plant.zombieLayer))
            {
                if (hit.collider == null || hit.collider.gameObject == null) continue;
                if (!hit.collider.gameObject.TryGetComponent<Zombie>(out var zombie)) continue;
                if (zombie == null) continue;
                if (!Lawnf.InLandStatus(zombie.theStatus)) continue;
                zombie.TakeDamage(damage, plant.Cast<IDamageMaker>(), DamageType.NormalAll, plant.thePlantType);
                plant.board.GetMoney(150f);
            }

            GameAPP.PlaySound(UnityEngine.Random.Range(0, 3), 0.5f, 1f);
            plant.board.StartCoroutine(SetLineAlpha(line, 0.5f, 0.7f));
        }

        public Zombie? SearchZombie()
        {
            if (plant == null) return null;
            var list = Lawnf.GetAllZombies().ToArray().
                Where(z => plant.axis.position.x < z.axis.position.x). // 找到所有在植物右侧的
                Where(z => z.Alive). // 还活着的
                Where(z => Lawnf.InLandStatus(z.theStatus)). // 符合状态的
                OrderBy(z => Vector3.Distance(plant.axis.position, z.axis.position)).ToList(); // 按离植物远近排序
            if (list.Count <= 0) return null;
            return list[0];
        }

        public int GetDamage()
        {
            var unique = ShootingManager.Instance.GetBuffChoiceCount(PlantType.GoldThreePlantern, "强化：大富翁");
            return (int)(plant.attackDamage * (1 + plant.board.theMoney * unique * 0.0001f)); // 0.0001 = 0.01%
        }

        public void SetLines(int range)
        {
            int start = Mathf.Max(0, plant.thePlantRow - range);
            int end = Mathf.Min(plant.board.rowNum - 1, plant.thePlantRow + range);
            float offsetX = 0.6f;
            var axis = plant.axis.position + new Vector3(0f, 0.5f, 0f);
            var rowY = 1.67f;
            int center = plant.thePlantRow;
            for (float i = start; i <= end + 0.33f; i += 0.33f)
            {
                var line = Instantiate(laser, plant.board.transform).GetComponent<LineRenderer>();
                line.positionCount = 3;
                float y = axis.y + (center - i) * rowY;
                line.SetPosition(0, axis);
                line.SetPosition(1, new Vector3(axis.x + offsetX, y));
                line.SetPosition(2, new Vector3(axis.x + offsetX + (plant.board.zombieMaxX - plant.board.zombieMinX), y));
                line.sortingLayerName = $"particle11";
                line.SetWidth(0.8f, 0.8f);
                plant.board.StartCoroutine(SetLineAlpha(line, 1f, 1.3f));
            }
        }

        public static IEnumerator SetLineAlpha(LineRenderer renderer, float start, float end)
        {
            if (renderer == null) yield break;

            yield return new WaitForSeconds(start);
            float timer = 0f;
            float total = end - start;
            var color = renderer.startColor;
            while (timer <= end)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, timer / total);
                renderer.startColor = renderer.endColor = color;
                yield return null;
            }

            Destroy(renderer.gameObject);
        }

        public ThreeGoldPlantern plant => gameObject.GetComponent<ThreeGoldPlantern>();
    }

    #region 基础
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.ThreePlantern) ||
                !Config.configs.ContainsKey(PlantType.GoldThreePlantern))
            {
                Config.configs.Add(PlantType.ThreePlantern, new Shooting_ThreePlantern());
                Config.configs.Add(PlantType.GoldThreePlantern, new Shooting_GoldThreePlantern());
            }
            else
                Config.configs[PlantType.GoldThreePlantern].Cast<Shooting_GoldThreePlantern>().ResetQuality();
        }
    }
    #endregion

    #region 质变开大
    [HarmonyPatch(typeof(Money))]
    public static class MoneyPatch
    {
        [HarmonyPatch(nameof(Money.ReinforcePlant))]
        [HarmonyPrefix]
        public static bool PreReinforcePlant(Money __instance, ref Plant plant)
        {
            if (__instance != null &&
                __instance.board != null && __instance.board.boardTag.rogueShooting &&
                plant != null && plant.thePlantType == PlantType.GoldThreePlantern)
            {
                if (plant.GetComponent<GoldThreePlanternShooting>().CanSuperSkill())
                {
                    if (__instance.board.theMoney >= 1000)
                    {
                        plant.GetComponent<GoldThreePlanternShooting>().OnSuperSkill();
                        __instance.UsedEvent(plant.thePlantColumn, plant.thePlantRow, 1000);
                        __instance.OtherSuperSkill(plant);
                    }
                    else
                    {
                        InGameText.Instance.ShowText($"大招需要{1000}金币", 5f);
                    }
                    return false;
                }
            }
            return true;
        }
    }
    #endregion
}
