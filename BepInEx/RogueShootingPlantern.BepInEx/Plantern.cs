using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueShootingPlantern.BepInEx.ShootingPlantern
{
    internal static class PlanternCore
    {
        internal static void OnLoad()
        {
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_Plantern>();
        }
    }

    public class Shooting_Plantern : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_Plantern(IntPtr ptr) : base(ptr) { }
        public Shooting_Plantern() : base(ClassInjector.DerivedConstructorPointer<Shooting_Plantern>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.Plantern;
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
            new UpgradeBuff(PlantType.Plantern, PlantType.IcePlantern),
            new UpgradeBuff(PlantType.Plantern, PlantType.ThreePlantern)
        };

        public override void ReinforcePlant(Plant plant) { }
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
            if (!__instance.AllPlants.Contains(PlantType.Plantern))
                __instance.AllPlants.Add(PlantType.Plantern);
            if (!__instance.RestPlants.Contains(PlantType.Plantern))
                __instance.RestPlants.Add(PlantType.Plantern);
        }

        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            // 添加config
            if (!Config.configs.ContainsKey(PlantType.Plantern))
            {
                Config.configs.Add(PlantType.Plantern, new Shooting_Plantern());
            }
        }
    }
    #endregion
}
