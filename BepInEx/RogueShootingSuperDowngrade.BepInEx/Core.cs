using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Hook;
using Core;
using Cysharp.Threading.Tasks;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingSuperDowngrade.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.superdowngrade", "RogueShootingSuperDowngrade", "1.0.0")]
    public class Core : BasePlugin
    {
        /// <summary>
        /// 廉价审美 高贵审美
        /// </summary>
        internal static Lazy<(TravelDebuff low, TravelDebuff high, TravelDebuff diamond, TravelDebuff silver)> Buffs = new(() =>
        {
            var result = ((TravelDebuff)10036, (TravelDebuff)10037, (TravelDebuff)10038, (TravelDebuff)10040);
            foreach (var val in Il2CppSystem.Enum.GetValues(Il2CppType.From(typeof(TravelDebuff))))
            {
                if (val == null) continue;
                if (val.ToString() == "Shooting_廉价审美") result.Item1 = val.Unbox<TravelDebuff>();
                else if (val.ToString() == "Shooting_高贵审美") result.Item2 = val.Unbox<TravelDebuff>();
                else if (val.ToString() == "Shooting_点钻成金") result.Item3 = val.Unbox<TravelDebuff>();
                else if (val.ToString() == "Shooting_白银时代") result.Item4 = val.Unbox<TravelDebuff>();
            }
            return result;
        });

        /// <summary>
        /// 两极分化
        /// </summary>
        internal static Lazy<AdvBuff> polarization = new(() =>
        {
            var result = (AdvBuff)13101;
            foreach (var val in Il2CppSystem.Enum.GetValues(Il2CppType.Of<TravelDebuff>()))
            {
                if (val == null) continue;
                if (val.ToString() == "Shooting_两极分化") result = val.Unbox<AdvBuff>();
            }
            return result;
        });

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // 类型初始化
            ClassInjector.RegisterTypeInIl2Cpp<SuperDowngradeShooting>();

            DamageBuffQualityHook.ApplyHook();
            SpeedBuffQualityHook.ApplyHook();
        }

        public static class DamageBuffQualityHook
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void DamageBuffConstruct(IntPtr @this, int type, IntPtr method);
            private static DamageBuffConstruct Origin = null!;
            public static INativeDetour Detour = null!;

            public static unsafe void ApplyHook()
            {
                var cls = IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "GameLevel.RogueShooting", "DamageBuff");
                IL2CPP.il2cpp_init(cls);
                var method = IL2CPP.GetIl2CppMethod(cls, false, ".ctor", "System.Void", IL2CPP.RenderTypeName<PlantType>());
                var strc = UnityVersionHandler.Wrap((Il2CppMethodInfo*)method);
                Detour = INativeDetour.CreateAndApply(strc.MethodPointer, OnDamageBuffConstructHook, out Origin);
            }

            public static void OnDamageBuffConstructHook(IntPtr instance, int type, IntPtr method)
            {
                Origin.Invoke(instance, type, method);
                var @this = new DamageBuff(instance);
                if (ShootingManager.Instance != null && ShootingManager.Instance.GetOrAddComponent<SuperDowngradeShooting>().downGrade)
                    @this.randomQuality = Quality.random;
            }
        }

        public static class SpeedBuffQualityHook
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SpeedBuffConstruct(IntPtr @this, int type, IntPtr method);
            private static SpeedBuffConstruct Origin = null!;
            public static INativeDetour Detour = null!;

            public static unsafe void ApplyHook()
            {
                var cls = IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "GameLevel.RogueShooting", "SpeedBuff");
                IL2CPP.il2cpp_init(cls);
                var method = IL2CPP.GetIl2CppMethod(cls, false, ".ctor", "System.Void", IL2CPP.RenderTypeName<PlantType>());
                var strc = UnityVersionHandler.Wrap((Il2CppMethodInfo*)method);
                Detour = INativeDetour.CreateAndApply(strc.MethodPointer, OnSpeedBuffConstructHook, out Origin);
            }

            public static void OnSpeedBuffConstructHook(IntPtr instance, int type, IntPtr method)
            {
                Origin.Invoke(instance, type, method);
                var @this = new SpeedBuff(instance);
                if (ShootingManager.Instance != null && ShootingManager.Instance.GetOrAddComponent<SuperDowngradeShooting>().downGrade)
                    @this.randomQuality = Quality.random;
            }
        }
    }

    public class SuperDowngradeShooting : MonoBehaviour
    {
        public bool downGrade = false;
        public int combo = 0;
        public bool first = false;

        public static SuperDowngradeShooting GetInstance()
        {
            return ShootingManager.Instance.GetOrAddComponent<SuperDowngradeShooting>();
        }

        public void Awake()
        {
            first = true;
        }

        public static async Task RecreateMenu(string newText)
        {
            await UniTask.Yield();
            var comp = ShootingManager.Instance.GetOrAddComponent<SuperDowngradeShooting>();
            if (comp.combo < 3 && comp.downGrade)
            {
                if (UnityEngine.Random.value <= 0.5f)
                {
                    var menu = GameAPP.UIManager.Push(UIType.MultipleChoiceMenu2).GetComponent<MultipleChoiceMenu>();
                    ShootingManager.Instance.RegisterCoreBuff(menu);
                    Il2CppSystem.Collections.Generic.List<MultipleChoiceMenu.OptionData> options = new();
                    foreach (var option in menu.optionDatas)
                    {
                        if (option.title.Contains("强化：速度") || option.title.Contains("强化：力量"))
                            options.Add(option);
                    }
                    menu.SetOrdered(true);
                    menu.ClearOptions();
                    foreach (var option in options)
                        menu.RegisterOption(option.title, option.text, option.call, option.thePlantType, option.theZombieType, Quality.random, option.interactable);
                    menu.RegisterWindow(options.Count.AtMost(5));
                    menu.SetCancelable(false);
                    menu.SetRefreshable(false, enableTurnPage: true);
                    menu.Start();
                    menu.KeySelect = true;
                    comp.combo++;
                    InGameText.Instance.ShowText(newText, 3f, true);
                }
            }
            else
                comp.combo = 0;
        }
    }

    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.RefisterMissionBuff))]
        [HarmonyPostfix]
        public static void PostRegisterMissionBuff(ShootingManager __instance, ref MultipleChoiceMenu menu)
        {
            // 检测有没有出试炼词条
            foreach (var item in menu.optionDatas)
                if (item.title.Contains("试炼")) return;
            // 如果没出
            // if ((__instance._lucky * 0.3f + 1f) * 0.1f <= UnityEngine.Random.value) return; // 概率出现
            var comp = __instance.GetOrAddComponent<SuperDowngradeShooting>();
            if (comp.downGrade) return; // 如果已有不出
            var action = () =>
            {
                if (__instance == null) return;
                if (comp == null) return;
                comp.downGrade = true;
            };
            menu.RegisterOption("试炼：步步塌方", "所有词条一定是随机品质，且随机范围扩大，受两极分化等词条影响", action, frameType: Quality.curse);
        }
    }

    [HarmonyPatch(typeof(DamageBuff))]
    public static class DamageBuffPatch
    {
        [HarmonyPatch(nameof(DamageBuff.OnGet))]
        [HarmonyPrefix]
        public static bool PreOnGet(DamageBuff __instance)
        {
            if (ShootingManager.Instance != null && ShootingManager.Instance.GetOrAddComponent<SuperDowngradeShooting>().downGrade &&
                __instance.randomQuality == Quality.random)
            {
                float min = -1f / 2f;
                float max = 2.3f / 2f;

                if (Lawnf.TravelDebuff(Core.Buffs.Value.low))
                {
                    min *= 0.7f;
                    max *= 0.7f;
                }
                if (Lawnf.TravelDebuff(Core.Buffs.Value.high))
                {
                    min *= 1.1f;
                    max *= 1.1f;
                }

                // 点钻成金: 上限-0.3
                if (Lawnf.TravelDebuff(Core.Buffs.Value.diamond))
                    max -= 0.3f / 2f;

                if (ShootingManager.Instance.superUpgrade) max = 7.5f;

                min *= 2f;
                max *= 2f;
                var val = UnityEngine.Random.Range(min, max);
                var ori = val;
                // 两极分化: 增加0.1~0.5或-0.1~-0.5
                if (Lawnf.TravelAdvanced(Core.polarization.Value))
                {
                    var offset = UnityEngine.Random.Range(0.1f, 0.5f);
                    val += UnityEngine.Random.Range(0, 2) == 0 ? offset : -offset;
                }

                // 白银时代: 加成x0.9
                if (Lawnf.TravelDebuff(Core.Buffs.Value.silver))
                    val *= 0.9f;

                // 第一抽给必定>=0保底
                if (SuperDowngradeShooting.GetInstance().first)
                {
                    if (val < 0)
                    {
                        val = -val;
                        ori = -ori;
                    }
                    SuperDowngradeShooting.GetInstance().first = false;
                }

                var show = $"获得了{(val * 100f):F0}%力量增幅 (词条影响：{(val - ori) * 100:F0}%)";
                _ = SuperDowngradeShooting.RecreateMenu(show);
                InGameText.Instance.ShowText(show, 3f, true);

                TravelMgr.Instance.data.AddDamage(__instance.plantType, val);

                return false;
            }
            return true;
        }
    }



    [HarmonyPatch(typeof(SpeedBuff))]
    public static class SpeedBuffPatch
    {
        [HarmonyPatch(nameof(SpeedBuff.OnGet))]
        [HarmonyPrefix]
        public static bool PreOnGet(SpeedBuff __instance)
        {
            if (ShootingManager.Instance != null && ShootingManager.Instance.GetOrAddComponent<SuperDowngradeShooting>().downGrade &&
                __instance.randomQuality == Quality.random)
            {
                float min = -0.8f / 2f;
                float max = 1.7f / 2f;

                if (Lawnf.TravelDebuff(Core.Buffs.Value.low))
                {
                    min *= 0.7f;
                    max *= 0.7f;
                }
                if (Lawnf.TravelDebuff(Core.Buffs.Value.high))
                {
                    min *= 1.1f;
                    max *= 1.1f;
                }

                // 点钻成金: 上限-0.2
                if (Lawnf.TravelDebuff(Core.Buffs.Value.diamond))
                    max -= 0.2f / 2f;

                if (ShootingManager.Instance.superUpgrade) max = 5f;

                min *= 2f;
                max *= 2f;
                var val = UnityEngine.Random.Range(min, max);
                var ori = val;
                // 两极分化: 增加0.08~0.4或-0.08~-0.4
                if (Lawnf.TravelAdvanced(Core.polarization.Value))
                {
                    var offset = UnityEngine.Random.Range(0.08f, 0.4f);
                    val += UnityEngine.Random.Range(0, 2) == 0 ? offset : -offset;
                }
                
                // 白银时代: 加成x0.9
                if (Lawnf.TravelDebuff(Core.Buffs.Value.silver))
                    val *= 0.9f;

                // 第一抽给必定>=0保底
                if (SuperDowngradeShooting.GetInstance().first)
                {
                    if (val < 0)
                    {
                        val = -val;
                        ori = -ori;
                    }
                    SuperDowngradeShooting.GetInstance().first = false;
                }

                var show = $"获得了{(val * 100f):F0}%速度增幅 (词条影响：{(val - ori) * 100:F0}%)";
                _ = SuperDowngradeShooting.RecreateMenu(show);
                InGameText.Instance.ShowText(show, 3f, true);

                TravelMgr.Instance.data.AddSpeed(__instance.plantType, val);
                if (ShootingManager.Instance.TryGetPlant(__instance.plantType, out var plant))
                    plant.AddSpeed(val);

                return false;
            }
            return true;
        }
    }
}
