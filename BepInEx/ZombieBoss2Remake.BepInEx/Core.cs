using BepInEx.Unity.IL2CPP;
using BepInEx;
using UnityEngine;
using CustomizeLib.BepInEx;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace ZombieBoss2Remake.BepInEx
{
    [BepInPlugin("inf75.zombieboss2remake", "ZombieBoss2Remake", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<ZombieBoss2Remake>();
            ClassInjector.RegisterTypeInIl2Cpp<BlackHole>();
            ClassInjector.RegisterTypeInIl2Cpp<Rv>();
            ClassInjector.RegisterTypeInIl2Cpp<FakeTrophyAnim>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "zombieboss2remake");
            CustomCore.RegisterCustomZombie<ZombieBoss2, ZombieBoss2Remake>((ZombieType)46,
                ab.GetAsset<GameObject>("ZombieBoss2"), 0, 50, 45000, 0, 0);
            Snow2 = ab.GetAsset<GameObject>("Snow2");
            VaseParticle = ab.GetAsset<GameObject>("Vase");
            FakeTrophyAnim = ab.GetAsset<GameObject>("FakeTrophyAnim");
            FakeTrophyAnim.AddComponent<FakeTrophyAnim>();
            FakeTrophySound = ab.GetAsset<AudioClip>("FakeTrophy");
            CustomLevelData levelData = new()
            {
                Name = () => "僵王博士的复仇2",
                BoardTag = new()
                {
                    isBoss2 = true,
                    isBoss = true,
                    enableAllTravelPlant = true,
                    enableTravelBuff = true,
                    isFreeCardSelect = true,
                },
                SceneType = SceneType.Travel_roof_dusk,
                RowCount = 6,
                RealBoss2 = true,
                AdvBuffs = () => [8, 9, 13, 14, 16, 21, 22],
                Debuffs = () => [34],
                UnlockPlants = () =>
                {
                    List<int> unlocks = [];
                    foreach (var u in TravelMgr.unlocks.Keys)
                    {
                        unlocks.Add(u);
                    }
                    return unlocks;
                },
                WaveCount = () => 0,
                Sun = () => 50000,
                BgmType = MusicType.Boss2,
                Logo = ab.GetAsset<Sprite>("LevelPreview"),
                PostInitBoard = (initBoard) =>
                {
                    for (int i = 0; i < initBoard.board.rowNum; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            initBoard.board.GetComponent<CreatePlant>().SetPlant(j, i, PlantType.Pot);
                        }
                    }
                    for (int k = 0; k < initBoard.board.rowNum; k++)
                    {
                        for (int l = 0; l < initBoard.board.columnNum; l++)
                        {
                            initBoard.board.boxType.Cast<Il2CppSystem.Array>().SetValue((int)BoxType.Roof, l, k);
                        }
                    }
                },
            };
            LevelID = CustomCore.RegisterCustomLevel(levelData);
        }

        public static GameObject? FakeTrophyAnim { get; set; }
        public static AudioClip? FakeTrophySound { get; set; }
        public static Core Instance { get; set; } = new();
        public static int LevelID { get; set; }
        public static ManualLogSource Mlogger => Instance.Log;
        public static GameObject? Snow2 { get; set; }
        public static GameObject? VaseParticle { get; set; }
    }
}