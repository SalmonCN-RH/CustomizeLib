using CustomizeLib.BepInEx.ExtensionData.Unity;
using CustomizeLib.BepInEx.LibTools;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx.GameExtra
{
    #region 僵尸颜色
    public static partial class ZombieExtra
    {
        public static void AddColor(this Zombie zombie, Color color)
        {
            if (zombie == null) return;
            var newData = zombie.GetData<ZombieColorData>(Strings.ZombieColorData).val.SetZombie(zombie).AddColor(color);
            zombie.SetData(Strings.ZombieColorData, newData);
        }

        public static void AddColor(this Zombie zombie, Color color, Zombie.ZombieColor id)
        {
            if (zombie == null) return;
            var newData = zombie.GetData<ZombieColorData>(Strings.ZombieColorData).val.SetZombie(zombie).AddColor(color, id);
            zombie.SetData(Strings.ZombieColorData, newData);
        }

        public static void RemoveColor(this Zombie zombie, Color color)
        {
            if (zombie == null) return;
            var newData = zombie.GetData<ZombieColorData>(Strings.ZombieColorData).val.SetZombie(zombie).RemoveColor(color);
            zombie.SetData(Strings.ZombieColorData, newData);
        }

        public static void RemoveColor(this Zombie zombie, Zombie.ZombieColor id)
        {
            if (zombie == null) return;
            var newData = zombie.GetData<ZombieColorData>(Strings.ZombieColorData).val.SetZombie(zombie).RemoveColor(id);
            zombie.SetData(Strings.ZombieColorData, newData);
        }
    }

    public struct ZombieColorData
    {
        public ZombieColorData() { init = true; }

        public void Construct()
        {
            if (init) return;
            ColorMaps ??= [];
            CustomColors ??= [];
            init = true;
        }

        public ZombieColorData SetZombie(Zombie zombie)
        {
            this.zombie = zombie;
            return this;
        }

        public ZombieColorData AddColor(Color color, Zombie.ZombieColor id = (Zombie.ZombieColor)(-1))
        {
            Construct();
            if (id != (Zombie.ZombieColor)(-1))
            {
                if (ColorMaps.ContainsKey(id))
                    return this;
                ColorMaps.Add(id, color);
            }
            CustomColors.Add(color);
            ApplyChange();
            return this;
        }

        public ZombieColorData RemoveColor(Color color)
        {
            Construct();
            CustomColors.Remove(color);
            ApplyChange();
            return this;
        }

        public ZombieColorData RemoveColor(Zombie.ZombieColor color)
        {
            Construct();
            CustomColors.Remove(ColorMaps[color]);
            ColorMaps.Remove(color);
            ApplyChange();
            return this;
        }

        public ZombieColorData ApplyChange()
        {
            Construct();
            zombie.SetData(Strings.ZombieColorData, this);
            zombie.UpdateColor();
            return this;
        }

        public List<Color> GetCurrentColors()
        {
            return CustomColors ?? [];
        }

        private bool init = false;
        public Dictionary<Zombie.ZombieColor, Color> ColorMaps { get; set; } = new();
        public List<Color> CustomColors { get; set; } = new();
        public Zombie zombie { get; set; } = null!;
    }

    [HarmonyPatch(typeof(Zombie))]
    public static class ZombieColorPatch
    {
        [HarmonyPatch(nameof(Zombie.UpdateColor))]
        [HarmonyPrefix]
        public static bool PreUpdateColor(Zombie __instance, ref Zombie.ZombieColor zombieColor)
        {
            __instance.colorsBuffers.Clear();
            var data = __instance.GetData<ZombieColorData>(Strings.ZombieColorData).val;
            foreach (var color in data.GetCurrentColors())
                __instance.colorsBuffers.Add(color);
            Color finalColor;
            if (zombieColor != Zombie.ZombieColor.Default)
            {
                finalColor = __instance.GetColor(zombieColor);
            }
            else
            {
                if (__instance.HasBuff(EffectType.Jala))
                    __instance.colorsBuffers.Add(__instance.GetColor(Zombie.ZombieColor.Jalaed));
                if (__instance.HasBuff(EffectType.Ember))
                    __instance.colorsBuffers.Add(__instance.GetColor(Zombie.ZombieColor.Ember));
                if (__instance.HasBuff(EffectType.Cold) || __instance.HasBuff(EffectType.Freeze))
                    __instance.colorsBuffers.Add(__instance.GetColor(Zombie.ZombieColor.Cold));
                if (__instance.HasBuff(EffectType.Poison))
                    __instance.colorsBuffers.Add(__instance.GetColor(Zombie.ZombieColor.Poison));
                if (__instance.isGold)
                    __instance.colorsBuffers.Add(__instance.GetColor(Zombie.ZombieColor.Gold));
                if (__instance.isDoom || __instance.garlicDoom)
                    __instance.colorsBuffers.Add(__instance.GetColor(Zombie.ZombieColor.Doom));
                if (__instance.isMindControlled)
                    __instance.colorsBuffers.Add(__instance.GetColor(Zombie.ZombieColor.MindConrolled));

                finalColor = __instance.GetMixColor(__instance.colorsBuffers);
            }
            Color current = __instance.currentColor;
            float diffR = current.r - finalColor.r;
            float diffG = current.g - finalColor.g;
            float diffB = current.b - finalColor.b;
            float diffA = current.a - finalColor.a;

            if (diffR * diffR + diffG * diffG + diffB * diffB + diffA * diffA < 0.0000000001f)
                return false;

            for (int i = 0; i < __instance.spriteRenderers.Count; i++)
            {
                var renderer = __instance.spriteRenderers[i];
                if (renderer != null)
                    renderer.color = finalColor;
            }

            __instance.currentColor = finalColor;
            return false;
        }
    }
    #endregion
}
