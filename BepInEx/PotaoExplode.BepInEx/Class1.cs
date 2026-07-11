using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace PotaoExplode.BepInEx
{
    [BepInPlugin("salmon.potaoexplode", "PotaoExplode", "1.0")]
    public class Core : BasePlugin
    {
        public static GameObject PotatoPrefab = null!;
        public static GameObject ObsidianPotatoPrefab = null!;
        public static ParticleType theNewParticleType = (ParticleType)750;

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            var ab = GetAssetBundle();
            foreach (var ase in ab.LoadAllAssetsAsync().allAssets)
            {
                if (ase.TryCast<GameObject>()?.name == "PotaoExplode")
                    PotatoPrefab = ase.Cast<GameObject>();
                if (ase.TryCast<GameObject>()?.name == "ObsidianPotaoExplode")
                    ObsidianPotatoPrefab = ase.Cast<GameObject>();
            }
        }

        public static AssetBundle GetAssetBundle()
        {
            using Stream stream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().FullName!.Split(",")[0] + "." + "potaoexplode") ??
                Assembly.GetExecutingAssembly().GetManifestResourceStream("potaoexplode")!;
            using MemoryStream stream1 = new();
            stream.CopyTo(stream1);
            var ab = AssetBundle.LoadFromMemory(stream1.ToArray());
            ArgumentNullException.ThrowIfNull(ab);
            return ab;
        }
    }

    [HarmonyPatch(typeof(GameAPP), nameof(GameAPP.Start))]
    public static class GameAPPPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            GameAPP.resourcesManager.particlePrefabs[ParticleType.PotaoExplode] = Core.PotatoPrefab;
            // 新粒子类型
            var particleArr = new Il2CppReferenceArray<GameObject>(Mathf.Max((int)Core.theNewParticleType + 1, GameAPP.particlePrefab.Length));
            Il2CppSystem.Array.Copy(GameAPP.particlePrefab.Cast<Il2CppSystem.Array>(), particleArr.Cast<Il2CppSystem.Array>(), 
                GameAPP.particlePrefab.Length);
            GameAPP.particlePrefab[(int)Core.theNewParticleType] = Core.ObsidianPotatoPrefab;
            GameAPP.resourcesManager.particlePrefabs[Core.theNewParticleType] = Core.ObsidianPotatoPrefab;
            GameAPP.resourcesManager.allParticles.Add(Core.theNewParticleType);
        }
    }

    [HarmonyPatch(typeof(ObsidianPotatoNut))]
    public static class ObsidianPotatoNutPatch
    {
        [HarmonyPatch(nameof(ObsidianPotatoNut.TakeDamage))]
        [HarmonyPrefix]
        public static bool PreTakeDamage(ObsidianPotatoNut __instance, int damage, IDamageMaker damageFrom, DamageType damageType, PlantType reportType, bool fix)
        {
            if (damage > 0)
            {
                __instance.storgedDamage += damage;

                if (__instance.storgedDamage > 1000f && __instance.axis != null)
                {
                    AoeDamage.SmallBombPotato(__instance.axis.position, 1.0f, __instance.zombieLayer, __instance.thePlantRow, 500, __instance.thePlantType);
                    ParticleManager.Instance.SetParticle(Core.theNewParticleType, __instance.axis.position);
                    GameAPP.PlaySound(SoundType.PotatoMine, 0.5f, 1.0f);
                    __instance.storgedDamage = 0f;
                }
            }

            // 调用基类的伤害处理
            BaseType.TakeDamage(__instance, damage, __instance.Cast<IDamageMaker>(), damageType, PlantType.Nothing, false);
            return false;
        }
    }

    [HarmonyPatch(typeof(UltimateTallNut))]
    public static class BaseType
    {
        [HarmonyPatch(nameof(UltimateTallNut.TakeDamage))]
        [HarmonyReversePatch]
        public static void TakeDamage(object instance, int damage, IDamageMaker damageFrom, DamageType damageType, PlantType reportType, bool fix) =>
            throw new NotImplementedException(); // Harmony存根方法，用于base调用
    }
}
