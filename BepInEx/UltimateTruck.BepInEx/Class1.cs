using AutoChess;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using GameLevel.RogueShooting;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateTruck.BepInEx
{
    [BepInPlugin("salmon.ultimatetruck", "UltimateTruck", "1.0")]
    public class Core : BasePlugin
    {
        public static AudioClip music = null!;
        public static MusicType theNewMusicType = (MusicType)1900;

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            foreach (var item in GetAssetBundle("ultimatetruck").LoadAllAssetsAsync().allAssets)
            {
                if (item.TryCast<AudioClip>()?.name == "music")
                    music = item.Cast<AudioClip>();
            }
        }

        public static AssetBundle GetAssetBundle(string name)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using Stream stream =
                    assembly.GetManifestResourceStream(assembly.FullName!.Split(",")[0] + "." + name) ??
                    assembly.GetManifestResourceStream(name)!;
                using MemoryStream stream1 = new();
                stream.CopyTo(stream1);
                var ab = AssetBundle.LoadFromMemory(stream1.ToArray());
                ArgumentNullException.ThrowIfNull(ab);
                return ab;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to load {name} \n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class SoundManagerPatch
    {
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            GameAPP.soundManager.musics.Add(Core.theNewMusicType, Core.music);
            SoundManager.MusicNames.Add(Core.theNewMusicType, "大运");
        }

        [HarmonyPatch(nameof(GameAPP.PlayMusic))]
        [HarmonyPrefix]
        public static void PrePlayMusic(ref MusicType id)
        {
            if (ShootingManager.Instance != null && ShootingManager.Instance.plantBuffRecords.TryGetValue(PlantType.UltimateBamboo, out var value))
            {
                if ((id is MusicType.Boss or MusicType.Boss2 or MusicType.HorseBoss or MusicType.Night or MusicType.Night_drum) ||
                    ((id == MusicType.Day || id == MusicType.Day_drum) && Board.Instance.sceneType == SceneType.ShootingDay))
                    if (value.TryGetValue("质变：大运", out var _))
                        id = Core.theNewMusicType;
            }
        }
    }

    [HarmonyPatch(typeof(GameLevel.RogueShooting.UltimateBamboo.SuperBuff))]
    public static class UltimateBambooPatch
    {
        [HarmonyPatch(nameof(GameLevel.RogueShooting.UltimateBamboo.SuperBuff.OnGet))]
        [HarmonyPostfix]
        public static void PostOnGet()
        {
            GameAPP.Instance.PlayMusic(Core.theNewMusicType);
        }
    }
}
