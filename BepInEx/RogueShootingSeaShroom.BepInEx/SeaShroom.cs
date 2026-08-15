using GameLevel.RogueShooting;
using Il2CppInterop.Runtime.Injection;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueShootingSeaShroom.BepInEx.ShootingSeaShroom
{
    internal static class SeaShroomCore
    {
        internal static void OnLoad()
        {
            // 类型初始化
            // config
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_SeaShroom>();
        }
    }

    public class Shooting_SeaShroom : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_SeaShroom(IntPtr ptr) : base(ptr) { }
        public Shooting_SeaShroom() : base(ClassInjector.DerivedConstructorPointer<Shooting_SeaShroom>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.SeaShroom;
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
            new UpgradeBuff(PlantType.SeaShroom, PlantType.BigSeaShroom)
        };

        public override void ReinforcePlant(Plant plant)
        {
            // 基础植物填这个没啥用，全靠patch
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
            if (!__instance.AllPlants.Contains(PlantType.SeaShroom))
                __instance.AllPlants.Add(PlantType.SeaShroom);
            if (!__instance.RestPlants.Contains(PlantType.SeaShroom))
                __instance.RestPlants.Add(PlantType.SeaShroom);
        }

        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (!Config.configs.ContainsKey(PlantType.SeaShroom))
                Config.configs.Add(PlantType.SeaShroom, new Shooting_SeaShroom());
        }
    }
    #endregion

    #region 海蘑菇增益
    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch(nameof(Plant.Start))]
        [HarmonyPostfix]
        public static void PostStart(Plant __instance)
        {
            if (__instance != null &&
                __instance.board != null && __instance.board.boardTag.rogueShooting)
            {
                if (__instance.thePlantType == PlantType.SeaShroom)
                {
                    __instance.attackDamage = 400;
                    __instance.ModifySpeed(PlantSpeedAdder.Shooting, 1f);
                }
            }
        }
    }
    #endregion

    #region 海蘑菇上陆地
    [HarmonyPatch(typeof(TypeMgr))]
    public static class TypeMgrPatch
    {
        [HarmonyPatch(nameof(TypeMgr.IsWaterPlant))]
        [HarmonyPostfix]
        public static void PostIsWaterPlant(PlantType theSeedType, ref bool __result)
        {
            if (Board.Instance != null && Board.Instance.boardTag.rogueShooting)
            {
                if (theSeedType == PlantType.SeaShroom)
                    __result = false;
            }
        }
    }
    #endregion
}
