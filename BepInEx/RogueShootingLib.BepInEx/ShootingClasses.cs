using GameLevel.RogueShooting;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueShootingLib.BepInEx
{
    public abstract partial class CustomConfig : BaseConfig
    {
        public CustomConfig(IntPtr ptr) : base(ptr) { }
        
        public abstract PlantType CustomPlantType { get; }
        public abstract List<BaseBuff> CustomBuffs { get; }
        public abstract string CustomRole { get; }
        public abstract void CustomReinforcePlant(Plant plant);

        #region 实现baseconfig的东西
        public override PlantType PlantType => CustomPlantType;
        public override Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }
        public override string Role => CustomRole;
        public override void ReinforcePlant(Plant plant) =>
            CustomReinforcePlant(plant);
        #endregion
    }
}
