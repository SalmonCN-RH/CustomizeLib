using CustomizeLib.MelonLoader;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using ZombieBoss2Remake.MelonLoader;

[assembly: MelonInfo(typeof(Core), "ZombieBoss2Remake", "1.0", "Infinite75", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace ZombieBoss2Remake.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Instance = this;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "zombieboss2remake");
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
        public static MelonLogger.Instance Mlogger => Instance.LoggerInstance;
        public static GameObject? Snow2 { get; set; }
        public static GameObject? VaseParticle { get; set; }
    }
}