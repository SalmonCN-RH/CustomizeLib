using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingSeaShroom.BepInEx.ShootingUltimateSeaShroom
{
    internal static class UltimateSeaShroomCore
    {
        internal static void OnLoad()
        {
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_SuperSeaShroom>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateSeaShroom>();
            // buff
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_UltimateSeaShroom.UniqueUpgrade>();
            // other
            ClassInjector.RegisterTypeInIl2Cpp<UltimateSeaShroomShooting>();
        }
    }

    public class Shooting_SuperSeaShroom : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_SuperSeaShroom(IntPtr ptr) : base(ptr) { }
        public Shooting_SuperSeaShroom() : base(ClassInjector.DerivedConstructorPointer<Shooting_SuperSeaShroom>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.BigSeaShroom;
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
            new UpgradeBuff(PlantType.BigSeaShroom, PlantType.UltimateSeaShroom)
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.attackDamage *= 30;
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 2f);
        }
    }

    public class Shooting_UltimateSeaShroom : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_UltimateSeaShroom(IntPtr ptr) : base(ptr) { }
        public Shooting_UltimateSeaShroom() : base(ClassInjector.DerivedConstructorPointer<Shooting_UltimateSeaShroom>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        private Lazy<UltiBuff> Buff = new(() =>
        {
            var result = (UltiBuff)45;
            foreach (var item in Il2CppSystem.Enum.GetValues(Il2CppType.From(typeof(UltiBuff))))
            {
                if (item.ToString() == "群体出动")
                {
                    result = item.Unbox<UltiBuff>();
                    break;
                }
            }
            return result;
        });

        public override PlantType PlantType => PlantType.UltimateSeaShroom;
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
            new DamageBuff(PlantType.UltimateSeaShroom),
            new SpeedBuff(PlantType.UltimateSeaShroom),
            new UniqueUpgrade()
        };

        public override void ReinforcePlant(Plant plant)
        {
            // 属性修改
            plant.attackDamage *= 4;
            plant.AddSpeed(PlantSpeedAdder.Shooting, 1f, new());
            // 加自定义组件
            plant.AddComponent<UltimateSeaShroomShooting>();
            // 解锁词条
            TravelMgr.Instance.GetUltiBuff(Buff.Value);
        }

        internal void ResetQuality()
        {
            CustomBuffs[0] = new DamageBuff(PlantType);
            CustomBuffs[1] = new SpeedBuff(PlantType);
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
            public override PlantType ShowType => PlantType.UltimateSeaShroom;
            public override string Title => "强化：眷族";
            public override string Description => "每次召唤的眷族+1，召唤眷族与触手击退的时间-1秒";

            public override void OnGet()
            {
                // 由自定义类通过ShootingManager获取
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
            public override PlantType ShowType => PlantType.UltimateSeaShroom;
            public override string Title => "质变：";
        }
    }

    public class UltimateSeaShroomShooting : MonoBehaviour
    {
        internal static int unique => ShootingManager.Instance != null ? 
            ShootingManager.Instance.GetBuffChoiceCount(PlantType.UltimateSeaShroom, "强化：眷族") : 0;
        private float speed = 0f;
        private bool inExtra = false;

        public void Start()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (plant == null) return;
            if (Time.timeScale <= 0f) return;
            speed = plant.anim.speed;
        }

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (plant == null) return;
            if (Time.timeScale <= 0f) return;
            plant.anim.speed = speed * (1 + plant.attackSpeedAdder);

            if (plant.attributeCountdown > 15 - unique)
                plant.AttributeCountdown = 15 - unique;
        }

        public static void SetPlantInExtra(Plant plant, bool value)
        {
            plant.GetOrAddComponent<UltimateSeaShroomShooting>().inExtra = value;
        }

        public static bool GetPlantInExtra(Plant plant)
        {
            return plant.GetOrAddComponent<UltimateSeaShroomShooting>().inExtra;
        }

        public UltimateSeaShroom plant => gameObject.GetComponent<UltimateSeaShroom>();
    }

    #region 基础
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.BigSeaShroom) ||
                !Config.configs.ContainsKey(PlantType.UltimateSeaShroom))
            {
                Config.configs.Add(PlantType.BigSeaShroom, new Shooting_SuperSeaShroom());
                Config.configs.Add(PlantType.UltimateSeaShroom, new Shooting_UltimateSeaShroom());
            }
            else
                Config.configs[PlantType.UltimateSeaShroom].Cast<Shooting_UltimateSeaShroom>().ResetQuality();
        }
    }
    #endregion

    #region 海妖上陆地
    [HarmonyPatch(typeof(TypeMgr))]
    public static class TypeMgrPatch
    {
        [HarmonyPatch(nameof(TypeMgr.IsWaterPlant))]
        [HarmonyPostfix]
        public static void PostIsWaterPlant(PlantType theSeedType, ref bool __result)
        {
            if (Board.Instance != null && Board.Instance.boardTag.rogueShooting)
            {
                if (theSeedType == PlantType.BigSeaShroom || theSeedType == PlantType.UltimateSeaShroom)
                    __result = false;
            }
        }
    }
    #endregion

    #region 海妖词条
    [HarmonyPatch(typeof(UltimateSeaShroom))]
    public static class UltimateSeaShroomPatch
    {
        [HarmonyPatch(nameof(UltimateSeaShroom.Shoot2))]
        [HarmonyPostfix]
        public static void PostShoot2(UltimateSeaShroom __instance)
        {
            if (__instance != null &&
                __instance.board != null && __instance.board.boardTag.rogueShooting)
            {
                if (UltimateSeaShroomShooting.GetPlantInExtra(__instance)) return;
                UltimateSeaShroomShooting.SetPlantInExtra(__instance, true);
                for (int i = 0; i < UltimateSeaShroomShooting.unique; i++)
                    __instance.Shoot2();
                UltimateSeaShroomShooting.SetPlantInExtra(__instance, false);
            }
        }
    }
    #endregion
}
