// #define DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF // 启用多级词条

using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using GameLevel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Debug = UnityEngine.Debug;

namespace CustomizeLib.BepInEx
{
    public struct CustomLevelData
    {
        public CustomLevelData()
        {
        }

        public Func<List<int>> AdvBuffs { get; set; } = () => [];
        public MusicType BgmType { get; set; } = MusicType.Day;
        public Board.BoardTag BoardTag { get; set; } = default;
        public Func<List<PlantType>> ConveyBeltPlantTypes { get; set; } = () => [];
        public Func<List<int>> Debuffs { get; set; } = () => [];
        public int ID { get; set; }
        public Sprite Logo { get; set; } = new();
        public Func<string> Name { get; set; } = () => "";
        public bool NeedSelectCard { get; set; } = true;
        public Action<Board> PostBoard { get; set; } = (_) => { };
        public Action<InitBoard> PostInitBoard { get; set; } = (_) => { };
        public Action PreInitBoard { get; set; } = () => { };
        public Func<List<(int, int, PlantType)>> PrePlants { get; set; } = () => [];
        public Func<List<PlantType>> PreSelectCards { get; set; } = () => [];
        public bool RealBoss2 { get; set; } = false;
        public int RowCount { get; set; } = 5;
        public int ColumnCount { get; set; } = 10;
        public SceneType SceneType { get; set; } = SceneType.Day;
        public Func<List<PlantType>> SeedRainPlantTypes { get; set; } = () => [];
        public Func<int> Sun { get; set; } = () => 500;
        public Func<List<(int, int)>> UltiBuffs { get; set; } = () => [];
        public Func<int> WaveCount { get; set; } = () => 10;
        public Func<int> ZombieHealthRate { get; set; } = () => 1;
        public Func<List<ZombieType>> ZombieList { get; set; } = () => [];
        public GameLevel.LevelData? LevelData { get; set; } = null;
    }

    public struct CustomPlantAlmanac
    {
        public string Description { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public struct CustomPlantData
    {
        public int ID { get; set; }
        public PlantDataManager.PlantData PlantData { get; set; }
        public GameObject Prefab { get; set; }
        public GameObject Preview { get; set; }

        public List<(BulletType, List<GameObject?>)>? BulletList { get; set; }
    }

    /// <summary>
    /// 用于储存皮肤数据
    /// </summary>
    public struct CustomTypeMgrExtraSkin
    {
        public CustomTypeMgrExtraSkin()
        {
        }

        public int BigNut { get; set; } = -1;
        public int BigZombie { get; set; } = -1;
        public int DoubleBoxPlants { get; set; } = -1;
        public int EliteZombie { get; set; } = -1;
        public int FlyingPlants { get; set; } = -1;
        public int IsAirZombie { get; set; } = -1;
        public int IsCaltrop { get; set; } = -1;
        public int IsCustomPlant { get; set; } = -1;
        public int IsFirePlant { get; set; } = -1;
        public int IsIcePlant { get; set; } = -1;
        public int IsMagnetPlants { get; set; } = -1;
        public int IsNut { get; set; } = -1;
        public int IsPlantern { get; set; } = -1;
        public int IsPot { get; set; } = -1;
        public int IsPotatoMine { get; set; } = -1;
        public int IsPuff { get; set; } = -1;
        public int IsPumpkin { get; set; } = -1;
        public int IsSmallRangeLantern { get; set; } = -1;
        public int IsSpecialPlant { get; set; } = -1;
        public int IsSpickRock { get; set; } = -1;
        public int IsTallNut { get; set; } = -1;
        public int IsTangkelp { get; set; } = -1;
        public int IsWaterPlant { get; set; } = -1;
        public int NotRandomBungiZombie { get; set; } = -1;
        public int NotRandomZombie { get; set; } = -1;
        public int UltimateZombie { get; set; } = -1;
        public int UmbrellaPlants { get; set; } = -1;
        public int UselessHypnoZombie { get; set; } = -1;
        public int WaterZombie { get; set; } = -1;
    }

    public struct CustomClickCardOnPlant
    {
        public bool BlockFusion { get; set; } = false;
        public TriggerType Trigger { get; set; } = TriggerType.All;
        public bool SaveOrigin { get; set; } = false;

        public CustomClickCardOnPlant()
        {
            BlockFusion = false;
            Trigger = TriggerType.All;
            SaveOrigin = false;
        }

        public enum TriggerType
        {
            All = 0,
            CardOnly = 1,
            GloveOnly = 2
        }
    }
    public struct BuffBgType
    {
        public int BgType = 0;

        public static BuffBgType Day = new BuffBgType(0);
        public static BuffBgType Night = new BuffBgType(1);
        public static BuffBgType Pool = new BuffBgType(2);

        public BuffBgType() { BgType = 0; }
        public BuffBgType(int bgType) { BgType = bgType; }
        public BuffBgType(TravelBuffOptionButton.BgType bgType) { BgType = (int)bgType; }
        public BuffBgType(TravelStoreWindow.BgType bgType) { BgType = (int)bgType; }

        public static implicit operator int(BuffBgType bgType) => bgType.BgType;
        public static implicit operator TravelBuffOptionButton.BgType(BuffBgType bgType) => (TravelBuffOptionButton.BgType)bgType.BgType;
        public static implicit operator TravelStoreWindow.BgType(BuffBgType bgType) => (TravelStoreWindow.BgType)bgType.BgType;
        public static implicit operator BuffBgType(int bgType) => new BuffBgType(bgType);
        public static implicit operator BuffBgType(TravelBuffOptionButton.BgType bgType) => new BuffBgType(bgType);
        public static implicit operator BuffBgType(TravelStoreWindow.BgType bgType) => new BuffBgType(bgType);
    }

    /// <summary>
    /// 自定义词条类型(在词条图鉴中显示)
    /// </summary>
    public enum AlmanacBuffType
    {
        Default = -1,
        /// <summary>
        /// 弱究
        /// </summary>
        WeakUltimate = 0,
        /// <summary>
        /// 强究
        /// </summary>
        StrongUltimate,
        /// <summary>
        /// 通用
        /// </summary>
        General,
        /// <summary>
        /// 随机
        /// </summary>
        Random,
        /// <summary>
        /// 诅咒
        /// </summary>
        Curse,
        /// <summary>
        /// 进化
        /// </summary>
        Rogue,
        /// <summary>
        /// 连携
        /// </summary>
        Combo,
        /// <summary>
        /// 小小词条
        /// </summary>
        Tiny,
        /// <summary>
        /// 僵尸
        /// </summary>
        Zombie,
        /// <summary>
        /// 诸神
        /// </summary>
        Shooting
    }

    public struct PlantAlmanac
    {
        public string info = "";
        public string cost = "";
        public string introduce = "";
        public string name = "";
        public PlantType plantType = PlantType.Nothing;
        public PlantAlmanac() { }
    }

    #region 无尽额外信息
    public struct CustomEndlessPlantData
    {
        public object value;
        public Type type;
        public int row;
        public int col;
        public PlantType pt;
    }

    public struct CustomEndlessData
    {
        public List<CustomEndlessPlantData> plantDatas;
    }
    #endregion

    public static class Utils
    {
        public static bool InGame() => GameAPP.theGameStatus is GameStatus.InGame or GameStatus.Pause;

        public static bool IsCustomLevel(out CustomLevelData levelData)
        {
            if (GameAPP.theBoardType == CustomLevelType)
            {
                levelData = CustomCore.CustomLevels[GameAPP.theBoardLevel];
                return true;
            }
            else
            {
                levelData = default;
                return false;
            }
        }

        public static bool IsGameRunning() => GameAPP.theGameStatus is GameStatus.InGame;

        public static bool IsNotNull<T>(this T obj) => obj is not null;

        public static int ToInt(this bool value) => value ? 1 : 0;

        public static LevelType CustomLevelType => (LevelType)66;

        /// <summary>
        /// 获取卡牌GameObject
        /// </summary>
        /// <returns>卡牌GameObject，Child 0:PacketBg背景图，Child 1：默认展示，1有CardUI组件</returns>
        public static GameObject? GetColorfulCardGameObject()
        {
            if (Board.Instance is not null && !Board.Instance.boardTag.isIZ)
            {
                GameObject? MyCard = null;
                MyCard = InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/ColorCards/SampleGrid(Clone)").GetChild(0).gameObject;
                return MyCard;
            }
            else if (Board.Instance is not null && Board.Instance.boardTag.isIZ)
            {

                GameObject? MyCard = null;
                MyCard = IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/ColorfulCards/Page1/CattailGirl").gameObject;
                return MyCard;
            }
            return null;
        }

        /// <summary>
        /// 获取卡牌GameObject
        /// </summary>
        /// <returns>卡牌GameObject，Child 0:PacketBg背景图，Child 1:二次选卡，Child 2：默认展示，12均有CardUI组件</returns>
        public static GameObject? GetNormalCardGameObject()
        {
            if (Board.Instance is not null && !Board.Instance.boardTag.isIZ)
            {
                GameObject? MyCard = null;
                MyCard = InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/NormalCards/SampleGrid(Clone)").GetChild(0).gameObject;
                return MyCard;
            }
            else if (Board.Instance is not null && Board.Instance.boardTag.isIZ)
            {
                GameObject? MyCard = null;
                MyCard = IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/Main/Page1/PeaShooter").gameObject;
                return MyCard;
            }
            return null;
        }

        /// <summary>
        /// 获取彩卡选卡父级
        /// </summary>
        /// <returns></returns>
        public static Transform? GetColorfulCardParent()
        {
            if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                return InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/ColorCards/SampleGrid(Clone)");
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                return IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/ColorfulCards/Page1");
            }
            return null;
        }

        public static Transform? GetNormalCardParent()
        {
            if (Board.Instance != null && Board.Instance.boardTag.isTowerDefence)
            {
                return InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/TowerCards/Page1");
            }
            else if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
            {
                return InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/NormalCards/SampleGrid(Clone)");
            }
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
            {
                return IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/Pages/Page1");
            }
            return null;
        }

        public static bool IsCheat() => GameAPP.developerMode;
        public static bool EnableTravelPlant() => Board.Instance.boardTag.enableAllTravelPlant || Board.Instance.boardTag.isSuperRandom || Board.Instance.boardTag.isUltimateSuperRandom || IsCheat() || Board.Instance.boardTag.isTravel;
    }

    // json对象
    public class JsonSkinObject
    {
        public Dictionary<int, int> CustomBulletType { get; set; } =
            [];

        public CustomPlantData CustomPlantData { get; set; }
        public CustomPlantAlmanac PlantAlmanac { get; set; }
        public CustomTypeMgrExtraSkin TypeMgrExtraSkin { get; set; }
    }
}