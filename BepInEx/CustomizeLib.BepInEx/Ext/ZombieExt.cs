using CustomizeLib.BepInEx.ExtensionData.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx
{
    public static class ZombieExtensions
    {
        public static void UnCold(this Zombie zombie) => zombie.GetAttrTimers().coldTimer = 0f;
        public static void TakeDamage(this Zombie zombie, DamageType theDamageType, int theDamage, PlantType reportType = PlantType.Nothing, bool fix = false)
            => zombie.TakeDamage(theDamage, CustomDamageMaker.DamageMaker, theDamageType, reportType, fix);

        public static ZombieAttrTimers GetAttrTimers(this Zombie zombie)
        {
            if (zombie.GetData("CustomizeLib_AttrTimers") == null) // 如果尚未被获取过
            {
                var timer = new ZombieAttrTimers
                {
                    zombie = zombie
                };
                zombie.SetData("CustomizeLib_AttrTimers", timer);
                return timer;
            }
            return zombie.GetData<ZombieAttrTimers>("CustomizeLib_AttrTimers"); // 如果获取过就直接返回
        }

        public static ZombieAttrTimers GetAttrTimers(this Zombie zombie, out ZombieAttrTimers timers)
        {
            if (zombie.GetData("CustomizeLib_AttrTimers") == null) // 如果尚未被获取过
            {
                var timer = new ZombieAttrTimers
                {
                    zombie = zombie
                };
                zombie.SetData("CustomizeLib_AttrTimers", timer);
                timers = timer;
                return timer;
            }
            timers = zombie.GetData<ZombieAttrTimers>("CustomizeLib_AttrTimers"); // 如果获取过就直接返回
            return zombie.GetData<ZombieAttrTimers>("CustomizeLib_AttrTimers");
        }
    }

    public class ZombieAttrTimers
    {
        public Zombie zombie;

        #region 黄油
        public float butterTimer
        {
            get => zombie.TryGetEffect<ButterEffect>(EffectType.Butter, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<ButterEffect>(EffectType.Butter, out var effect))
                    effect.duration = value;
            }
        }
        public bool isButter => butterTimer > 0f;
        #endregion
        #region 寒冷
        public float coldTimer
        {
            get => zombie.TryGetEffect<ColdEffect>(EffectType.Cold, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<ColdEffect>(EffectType.Cold, out var effect))
                    effect.duration = value;
            }
        }
        public bool isCold => coldTimer > 0f;
        #endregion
        #region 冻结
        public float freezeTimer
        {
            get => zombie.TryGetEffect<FreezeEffect>(EffectType.Freeze, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<FreezeEffect>(EffectType.Freeze, out var effect))
                    effect.duration = value;
            }
        }
        public bool isFreeze => freezeTimer > 0f;
        #endregion
        #region 免疫
        public float immuneTimer
        {
            get => zombie.TryGetEffect<ImmuneEffect>(EffectType.Immune, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<ImmuneEffect>(EffectType.Immune, out var effect))
                    effect.duration = value;
            }
        }
        public bool isImmune => immuneTimer > 0f;
        #endregion
        #region 水草
        public float kelpTimer
        {
            get => zombie.TryGetEffect<KelpEffect>(EffectType.Kelp, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<KelpEffect>(EffectType.Kelp, out var effect))
                    effect.duration = value;
            }
        }
        public bool isKelp => kelpTimer > 0f;
        #endregion
        #region 毒
        public float poisonTimer
        {
            get => zombie.TryGetEffect<PoisonEffect>(EffectType.Poison, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<PoisonEffect>(EffectType.Poison, out var effect))
                    effect.duration = value;
            }
        }
        public bool isPoison => poisonTimer > 0f;
        #endregion
        #region 超时空
        public float portaledTimer
        {
            get => zombie.TryGetEffect<PortalEffect>(EffectType.Portal, out var effect) ? effect.duration : -1f;
            set
            {
                if (zombie.TryGetEffect<PortalEffect>(EffectType.Portal, out var effect))
                    effect.duration = value;
            }
        }
        public bool isPortaled => portaledTimer > 0f;
        #endregion
        #region 红温
        public bool isJalaed => zombie.TryGetEffect<JalaEffect>(EffectType.Jala, out var _);
        #endregion
        #region 余烬
        public bool isEmbered => zombie.TryGetEffect<EmberEffect>(EffectType.Ember, out var _);
        #endregion
    }
}
