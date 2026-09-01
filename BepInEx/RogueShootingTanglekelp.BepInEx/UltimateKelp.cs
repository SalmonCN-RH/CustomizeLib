using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace RogueShootingTanglekelp.BepInEx.UltimateKelp
{
    internal static class UltimateKelp
    {
        internal static void OnLoad()
        {
            // 类型初始化
            // config
        }
    }

    public class Shooting_SuperKelp : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_SuperKelp(IntPtr ptr) : base(ptr) { }
        public Shooting_SuperKelp() : base(ClassInjector.DerivedConstructorPointer<Shooting_SuperKelp>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.SuperKelp;
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
            new UpgradeBuff(PlantType.SuperKelp, PlantType.UltimateKelp)
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.attackDamage *= 5;
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 1f);
        }
    }

    public class Shooting_UltimateKelp : BaseConfig
    {
        #region Il2Cpp构造函数
        public Shooting_UltimateKelp(IntPtr ptr) : base(ptr) { }
        public Shooting_UltimateKelp() : base(ClassInjector.DerivedConstructorPointer<Shooting_UltimateKelp>()) =>
            ClassInjector.DerivedConstructorBody(this);
        #endregion

        public override PlantType PlantType => PlantType.UltimateKelp;
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
            new DamageBuff(PlantType.UltimateKelp),
            new SpeedBuff(PlantType.UltimateKelp)
        };

        public override void ReinforcePlant(Plant plant)
        {
            plant.attackDamage *= 5;
            plant.board.GetPoint(1000);
            plant.board.currentRoundPoint = float.MinValue;
            if (UIZombieNum.Instance != null)
            {
                Action<TextMeshProUGUI> action = (text) =>
                {
                    if (Board.Instance != null)
                        text.text += $"积分：{Board.Instance.thePoints}";
                };
                UIZombieNum.Instance.textUpdate += action;
            }
        }
    }
}
