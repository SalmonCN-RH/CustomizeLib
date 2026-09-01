using BepInEx;
using BepInEx.Unity.IL2CPP;
using Cysharp.Threading.Tasks;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace RogueShootingWrath.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.wrath", "RogueShootingWrath", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // 类型初始化
            ClassInjector.RegisterTypeInIl2Cpp<WrathShooting>();
        }
    }

    public class WrathShooting : MonoBehaviour
    {
        public static WrathShooting Instance
        {
            get
            {
                if (ShootingManager.Instance == null) return null!;
                return ShootingManager.Instance.GetOrAddComponent<WrathShooting>();
            }
        }

        public bool wrath = false;
    }

    public class WrathComponent : MonoBehaviour
    {
        public static WrathShooting shooting => WrathShooting.Instance;

        public Lazy<float> ColumnX = new(() =>
        {
            if (Mouse.Instance != null) return Mouse.Instance.GetBoxXFromColumn(1) - Mouse.Instance.GetBoxXFromColumn(0);
            return 1.35f;
        });
        public float Range = -1f;

        public float GetRange()
        {
            if (Range == -1) Range = ColumnX.Value * 5f;
            return Range;
        }

        public void FixedUpdate()
        {
            if (shooting.wrath && plant != null)
            {
                var offset = new Vector3(0f, 0.5f, 0f);
                var axis = plant.axis.position + offset;
                var cols = Physics2D.OverlapCircleAll(axis, GetRange(), plant.zombieLayer);
                if (cols.Count <= 0) return;
                float min = float.PositiveInfinity;
                foreach (var col in cols)
                {
                    if (col == null) continue;
                    if (!col.TryGetComponent<Zombie>(out var zombie) || zombie == null) continue;
                    var distance = Vector3.Distance(axis, zombie.axis.position + offset);
                    if (distance < min)
                        min = distance;
                }
            }
        }

        public static (float power, float speed) GetNewPowerSpeed(float distance)
        {
        }

        public Plant plant = null!;
    }

    [HarmonyPatch(typeof(CreatePlant))]
    public static class CreatePlantPatch
    {
        [HarmonyPatch(nameof(CreatePlant.SetPlant))]
        [HarmonyPostfix]
        public static void PostSetPlant(ref Plant __result)
        {
            if (WrathShooting.Instance != null && __result != null)
            {
                __result.GetOrAddComponent<WrathComponent>().plant = __result;
            }
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
            if ((__instance._lucky * 0.3f + 1f) * 0.1f <= UnityEngine.Random.value) return; // 概率出现
            var comp = WrathShooting.Instance;
            if (comp.wrath) return; // 如果已有不出

            var action = () =>
            {
                if (__instance == null) return;
                if (comp == null) return;
                comp.wrath = true;
            };
            menu.RegisterOption("试炼：暴怒", "植物的属性随僵尸靠近而提升，同时损失血上限并受到诅咒效果", action, frameType: Quality.curse);
        }
    }
}
