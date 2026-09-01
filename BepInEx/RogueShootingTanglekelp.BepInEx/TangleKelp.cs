using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueShootingTanglekelp.BepInEx.TangleKelp
{
    internal static class TangleKelpCore
    {
        internal static void OnLoad()
        {
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_Tanglekelp>();
        }
    }

    public class Shooting_Tanglekelp : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_Tanglekelp(IntPtr ptr) : base(ptr) { }
        public Shooting_Tanglekelp() : base(ClassInjector.DerivedConstructorPointer<Shooting_Tanglekelp>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.Tanglekelp;
        public override string Role => "empty";
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
            new UpgradeBuff(PlantType.Tanglekelp, PlantType.SuperKelp)
        };

        public override void ReinforcePlant(Plant plant)
        {
            // 基础植物填这个没啥用
        }
    }

    #region 基础
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.Start))]
        [HarmonyPostfix]
        public static void PostStart(ShootingManager __instance)
        {
            if (!__instance.AllPlants.Contains(PlantType.Tanglekelp))
                __instance.AllPlants.Add(PlantType.Tanglekelp);
            if (!__instance.RestPlants.Contains(PlantType.Tanglekelp))
                __instance.RestPlants.Add(PlantType.Tanglekelp);
        }

        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.Tanglekelp))
                Config.configs.Add(PlantType.Tanglekelp, new Shooting_Tanglekelp());
        }
    }
    #endregion

    #region 水草上陆地
    [HarmonyPatch(typeof(TypeMgr))]
    public static class TypeMgrPatch
    {
        [HarmonyPatch(nameof(TypeMgr.IsWaterPlant))]
        [HarmonyPostfix]
        public static void PostIsWaterPlant(PlantType theSeedType, ref bool __result)
        {
            if (Board.Instance != null && Board.Instance.boardTag.rogueShooting)
            {
                if (theSeedType == PlantType.Tanglekelp)
                    __result = false;
            }
        }
    }
    #endregion

    #region 水草陆地抓人
    [HarmonyPatch(typeof(Tanglekelp))]
    public static class TanglekelpPatch
    {
        [HarmonyPatch(nameof(Tanglekelp))]
    }
    #endregion
}
