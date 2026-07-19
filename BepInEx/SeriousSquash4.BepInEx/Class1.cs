using AlmanacData;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Core;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SeriousSquash4.BepInEx.Core;

namespace SeriousSquash4.BepInEx
{
    [BepInPlugin("salmon.serioussquash4", "SeriousSquash4", "1.0")]
    public class Core : BasePlugin
    {
        public static ZombieType theNewZombieType = (ZombieType)19000;
        public static LevelType levelType = (LevelType)19000;
        public static int levelID = 19000;
        public static List<ZombieType> zombieList = new()
        {
            ZombieType.SquashZombie,
            ZombieType.JalaSquashZombie,
            ZombieType.EndoFlameZombie,
            ZombieType.BungiZombie,
            theNewZombieType
        };
        public static List<ZombieType> BungiPool = new()
        {
            ZombieType.SquashZombie,
            theNewZombieType
        };
        public static KeyCode LevelShowZombieHealth = KeyCode.R;
        public static KeyCode LevelChangeGlove = KeyCode.None;
        public static int EndoFlame_Level = 10;
        public static int EndoFlame_Weight = 2000;
        public static int Jala_Level = 10;
        public static int Jala_Weight = 1000;

        // 关卡数据
        public static Difficult difficult = Difficult.Normal;
        public static Dictionary<Difficult, float> LevelZombieHealthRate = new()
        {
            { Difficult.Easy, 0.5f },
            { Difficult.Normal, 0.7f },
            { Difficult.Hard, 0.85f },
            { Difficult.Purgatory, 1f },
        };
        public static Dictionary<Difficult, float> LevelZombieSpeed = new()
        {
            { Difficult.Easy, 0.5f },
            { Difficult.Normal, 0.7f },
            { Difficult.Hard, 0.85f },
            { Difficult.Purgatory, 1f },
        };
        public static Dictionary<Difficult, float> LevelZombieSpawn = new()
        {
            { Difficult.Easy, 0.5f },
            { Difficult.Normal, 0.5f },
            { Difficult.Hard, 0.75f },
            { Difficult.Purgatory, 1f },
        };

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static void InitSeriousSquash4()
        {
            // 加载梦珞僵尸
            var ab = GetAssetBundle("squalourzombie");
            SquashZombie? squashZombie = null;
            GameObject? zombieSquash = null;
            foreach (var item in ab.LoadAllAssetsAsync().allAssets)
            {
                if (item.TryCast<GameObject>()?.name == "SqualourZombiePrefab")
                {
                    GameAPP.resourcesManager.zombiePrefabs[theNewZombieType] = item.TryCast<GameObject>();
                    var zombie = GameAPP.resourcesManager.zombiePrefabs[theNewZombieType].AddComponent<SquashZombie>();
                    GameAPP.resourcesManager.allZombieTypes.Add(theNewZombieType);
                    zombie.gameObject.layer = LayerMask.NameToLayer("Zombie");
                    zombie.theZombieType = theNewZombieType;
                    zombie.tag = "Zombie";
                    // 初始化僵尸属性
                    var zombieData = new ZombieDataManager.ZombieData
                    {
                        theMaxHealth = 1500,
                        theAttackDamage = 50,
                        summonWeight = 1000,
                        cost = 0,
                        summonLevel = 4
                    };
                    zombie.squashHead = GameAPP.resourcesManager.zombiePrefabs[theNewZombieType].transform.FindChild("Zombie_head/SquashPrefab");
                    squashZombie = zombie;
                    foreach (var child in global::Core.Lawnf.GetChilds(GameAPP.resourcesManager.zombiePrefabs[theNewZombieType].transform))
                    {
                        string tag = "";
                        switch (child.name)
                        {
                            case "Zombie_outerarm_hand":
                            case "Zombie_outerarm_lower": tag = "ZombieHand"; break;
                            case "Zombie_outerarm_upper": tag = "ZombieArmUpper"; break;
                            case "Zombie_head": tag = "ZombieHead"; break;
                            case "Shadow": tag = "other"; break;
                            default: continue;
                        }
                        child.tag = tag;
                    }
                    ZombieDataManager.zombieDataDic[theNewZombieType] = zombieData;
                    InitZombieList.allowAllzombies.Add(theNewZombieType);
                }
                if (item.TryCast<Sprite>()?.name == "SqualourZombiePreview") // 注册预览图
                    GameAPP.resourcesManager.zombieSprites[theNewZombieType] = item.TryCast<Sprite>();
                if (item.TryCast<GameObject>()?.name == "Squash") // 注册窝瓜
                    zombieSquash = item.TryCast<GameObject>();
            }
            if (zombieSquash != null)
                zombieSquash.AddComponent<ZombieSquash>().progress = 1;
            if (squashZombie != null && zombieSquash != null)
                squashZombie.squashPrefab = zombieSquash;
            ClassInjector.RegisterTypeInIl2Cpp<ParentZombie>();
            ClassInjector.RegisterTypeInIl2Cpp<SquashZombieData>();
            // 关卡
            ClassInjector.RegisterTypeInIl2Cpp<ControlPlant>();
        }

        public static AssetBundle GetAssetBundle(string name)
        {
            try
            {
                using Stream stream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().FullName!.Split(",")[0] + "." + name) ??
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(name)!;
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

    public enum Difficult
    {
        Easy,
        Normal,
        Hard,
        Purgatory
    }

    public class ControlPlant : MonoBehaviour
    {
        public Plant plant => gameObject.GetComponent<Plant>();

        public float recoverTimer = 10f;

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            if (plant == null) return;

            if (Input.GetKeyDown(KeyCode.A))
                CreatePlant.Instance.SetPlant(plant.thePlantColumn - 1, plant.thePlantRow, PlantType.EndoFlame, plant);
            if (Input.GetKeyDown(KeyCode.D))
                CreatePlant.Instance.SetPlant(plant.thePlantColumn + 1, plant.thePlantRow, PlantType.EndoFlame, plant);
            if (Input.GetKeyDown(KeyCode.W))
                CreatePlant.Instance.SetPlant(plant.thePlantColumn, plant.thePlantRow - 1, PlantType.EndoFlame, plant);
            if (Input.GetKeyDown(KeyCode.S))
                CreatePlant.Instance.SetPlant(plant.thePlantColumn, plant.thePlantRow + 1, PlantType.EndoFlame, plant);

            recoverTimer -= Time.deltaTime;
            if (recoverTimer <= 0f)
            {
                plant.Recover(plant.thePlantMaxHealth / 3);
                recoverTimer = 10f;
            }
        }
    }

    public class SquashZombieData : MonoBehaviour
    {
        public bool summon = false;
    }

    public class ParentZombie : MonoBehaviour
    {
        public Zombie parent = null;
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Awake))]
        [HarmonyPostfix]
        public static void PostAwake()
        {
            InitSeriousSquash4();
        }
    }

    #region 珞僵尸
    [HarmonyPatch(typeof(AlmanacDataLoader))]
    public static class AlmanacDataLoaderPatch
    {
        [HarmonyPatch(nameof(AlmanacDataLoader.LoadZombieData))]
        [HarmonyPostfix]
        public static void PostLoadZombieData()
        {
            if (AlmanacDataLoader.zombieDatas.ContainsKey(theNewZombieType)) return;
            var data = new ZombieInfo
            {
                name = $"猫瓜僵尸",
                info =
                "能压扁身边的植物，召唤窝瓜僵尸！\n\n" +
                "<color=#3D1400>韧性：</color><color=red>1500</color>\n" +
                "<color=#3D1400>特点：</color><color=red>会碾压附近的植物并死亡。该方式死亡时始终产生3个窝瓜僵尸，有5%的概率额外生成1个火爆窝瓜僵尸</color>",
                introduce = "<color=#3D1400>猫瓜僵尸紧盯着树荫下，似乎是等待着什么，这一举动被很多窝瓜僵尸看到眼中。或许她并不知道，那里并没有电子火红莲。</color>",
                theZombieType = theNewZombieType
            };
            AlmanacDataLoader.zombieDatas.Add(theNewZombieType, data);
        }
    }

    [HarmonyPatch(typeof(SquashZombie))]
    public static class SquashZombiePatch
    {
        [HarmonyPatch(nameof(SquashZombie.OnTriggerStay2D))]
        [HarmonyPostfix]
        public static void PreOnTriggerStay2D(SquashZombie __instance)
        {
            if (__instance.theZombieType == theNewZombieType)
            {
                if (__instance.squash != null && __instance.squash.progress == 0)
                {
                    __instance.squash.progress = 1;
                    __instance.squash.AddComponent<ParentZombie>().parent = __instance;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ZombieSquash))]
    public static class ZombieSquashPatch
    {
        [HarmonyPatch(nameof(ZombieSquash.Update))]
        [HarmonyPostfix]
        public static void PostUpdate(ZombieSquash __instance)
        {
            if (__instance.GetComponent<ParentZombie>() != null && __instance.GetComponent<ParentZombie>().parent != null && __instance.progress == 4)
            {
                var zombie = __instance.GetComponent<ParentZombie>().parent;
                ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, zombie.axis.position, zombie.theZombieRow);
                CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.SquashZombie,
                    zombie.axis.position.x + UnityEngine.Random.Range(-0.15f, 0.15f), zombie.isMindControlled);
                CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.SquashZombie,
                    zombie.axis.position.x + UnityEngine.Random.Range(-0.15f, 0.15f), zombie.isMindControlled);
                CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.SquashZombie,
                    zombie.axis.position.x + UnityEngine.Random.Range(-0.15f, 0.15f), zombie.isMindControlled);
                if (UnityEngine.Random.Range(0, 20) < 1)
                    CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.JalaSquashZombie, zombie.axis.position.x, zombie.isMindControlled);

                if (GameAPP.theBoardType == levelType && GameAPP.theBoardLevel == levelID)
                {
                    if (UnityEngine.Random.Range(1, 100) <= (zombie.board.GetComponentsInChildren<ZombieEndoFlame>().Count > 0 ? 50 : 15))
                    {
                        Lawnf.SetDroppedCard(zombie.axis.position, PlantType.Squalour);
                    }
                }

                __instance.GetComponent<ParentZombie>().parent = null;
            }
        }
    }
    #endregion
    #region 关卡初始化
    [HarmonyPatch(typeof(UIMgr))]
    public static class UIMgrPatch
    {
        [HarmonyPatch(nameof(UIMgr.EnterChallengeMenu))]
        [HarmonyPostfix]
        public static void PostEnterChallengeMenu(UIMgr __instance)
        {
            GameAPP.Instance.StartCoroutine(Init().WrapToIl2Cpp());
            IEnumerator Init()
            {
                yield return null;
                var page = GameAPP.canvas.FindChild($"ChallengeMenu(Clone)/Levels/PageMiniGames/Pages");
                var targetPage = page.FindChild($"Page{page.childCount}");
                var level = targetPage.GetChild(0);
                var levelCount = 0;
                for (int i = 0; i < targetPage.childCount; i++)
                    if (targetPage.GetChild(i).gameObject.activeSelf)
                        levelCount++;
                var rowCount = levelCount / 6; // 获取有多少行
                var rowLevelCount = levelCount % 6; // 获取当前行有多少关卡
                // f(x) = 150x-300 x坐标函数
                // g(x) = -130x+160 y坐标函数
                int x = 150 * rowLevelCount - 300;
                int y = -130 * rowCount + 160;
                var pos = new Vector2(x, y);
                var newLevel = UnityEngine.Object.Instantiate(level.gameObject, targetPage);
                newLevel.transform.localPosition = pos;
                newLevel.GetComponent<Image>().sprite = Resources.Load<Sprite>("image/Almanac_GroundDay"); // 设置图标背景
                newLevel.transform.GetChild(0).GetComponent<Image>().sprite =
                    GameAPP.resourcesManager._plantPreviews[PlantType.Squalour][0].GetComponent<SpriteRenderer>().sprite; // 设置植物图标
                newLevel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "严肃窝瓜4"; // 设置关卡名称
                newLevel.transform.GetChild(1).GetComponent<Advanture_Btn>().levelType = levelType;
                newLevel.transform.GetChild(1).GetComponent<Advanture_Btn>().buttonNumber = levelID;
                newLevel.SetActive(true);
                yield break;
            }
        }

        [HarmonyPatch(nameof(UIMgr.EnterGame))]
        [HarmonyPrefix]
        public static bool PreEnterGame(LevelType levelType, int levelNumber, int id, string name)
        {
            if (levelType != Core.levelType || levelNumber != levelID) return true;

            SynergyManager.Instance.ClearAllSynergies();
            EventManager.ClearAllEvents();
            GameAPP.UIManager.PopAll();
            CamaraFollowMouse.Instance.ResetCamera();

            Time.timeScale = GameAPP.config.gameSpeed;

            GameAPP.theBoardType = levelType;
            GameAPP.theBoardLevel = levelNumber;

            RogueManager.Instance.Clear();

            GameObject boardGO = new("Board");
            GameAPP.board = boardGO;
            Board board = boardGO.AddComponent<Board>();
            var tag = board.boardTag;
            tag.isNight = false;
            tag.disableSelectCard = true;
            board.boardTag = tag;
            board.rowNum = 5;
            board.theMaxWave = 40;
            board.theSun = 500;
            board.config.firstWaveArrivedTimer = 7.5f;
            var map = MapData_cs.GetMap(SceneType.ShootingDay, board);
            UnityEngine.Object.Destroy(map.transform.GetChild(0).GetComponent<GiveFertilize>());
            InitZombieList.InitZombie(levelType, levelNumber);
            TravelMgr.Instance.GetNormalBuff((AdvBuff)1000); // 获得至极手速
            // 播放音乐并开始游戏
            GameAPP.Instance.PlayMusic(MusicType.SelectCard);
            GameAPP.theGameStatus = GameStatus.InInterlude;
            board.gameObject.AddComponent<InitBoard>();

            for (int i = 0; i < board.rowNum; i++)
            {
                var floor = map.transform.FindChild($"floor{i}");
                board.plane.Add(floor);
                var iceRoad = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("background/IceRoad"), new Vector3(19.7f, 0.8f, 0f), Quaternion.identity,
                    floor).GetComponent<IceRoad>();
                iceRoad.theRow = i;
                iceRoad.roadStartX = iceRoad.x = 19.7f;
                iceRoad.transform.localPosition = new Vector3(19.7f, 0.8f, 0f);
                board.iceRoads.Add(iceRoad);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(InGameUI))]
    public static class InGameUIPatch
    {
        [HarmonyPatch(nameof(InGameUI.SetUniqueText))]
        [HarmonyPostfix]
        public static void PostSetUniqueText(InGameUI __instance, ref Il2CppSystem.Collections.Generic.List<TextMeshProUGUI> T)
        {
            if (GameAPP.theBoardType == levelType && GameAPP.theBoardLevel == levelID)
            {
                __instance.ChangeString(T, "严肃窝瓜4");
            }
        }

        [HarmonyPatch(nameof(InGameUI.Start))]
        [HarmonyPostfix]
        public static void Postfix(InGameUI __instance)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType && __instance != null)
            {
                __instance.StartCoroutine(SetLevel().WrapToIl2Cpp());
                __instance.ShovelBank.SetActive(false);
                __instance.SeedBank.SetActive(false);
                __instance.WheelBank.SetActive(false);
                IEnumerator SetLevel()
                {
                    while (GameAPP.theGameStatus != GameStatus.InGame) yield return null;
                    __instance.ShovelBank.SetActive(false);
                    __instance.SeedBank.SetActive(false);
                    __instance.WheelBank.SetActive(false);
                    GameAPP.Instance.PlayMusic(MusicType.Loon);
                    Board.Instance.timeUntilNextWave = 7.5f;
                    // 显示难度选择菜单
                    var type = UIType.MultipleChoiceMenu;
                    foreach (var kvp in GameAPP.UIManager.UIPrefabs) // 飘的UIType天天动，没招了只能动态搜索了
                        if (kvp.Value.name == "MultipleChoiceMenu" || kvp.Value.name == "MultipleChoiceMenu2")
                        {
                            type = kvp.Key;
                            break;
                        }
                    var choiceMenu = GameAPP.UIManager.Push(type, GameAPP.canvasUp).GetComponent<MultipleChoiceMenu>();
                    if (choiceMenu != null)
                    {
                        choiceMenu.SetRefreshable(false);
                        choiceMenu.SetCancelable(false);
                        choiceMenu.RegisterWindow(4);
                        choiceMenu.ClearOptions();
                        choiceMenu.SetOrdered(true);
                        // 简单
                        Action easy = () => difficult = Difficult.Easy;
                        choiceMenu.RegisterOption("简单", "僵尸血量，速度减少50%\n出怪数量减少50%", easy);
                        // 普通
                        Action normal = () => difficult = Difficult.Normal;
                        choiceMenu.RegisterOption("普通", "僵尸血量，速度减少30%\n出怪数量减少50%", normal);
                        // 困难
                        Action hard = () => difficult = Difficult.Hard;
                        choiceMenu.RegisterOption("困难", "僵尸血量，速度减少15%\n出怪数量减少25%", hard);
                        // 炼狱
                        Action purgatory = () => difficult = Difficult.Purgatory;
                        choiceMenu.RegisterOption("炼狱", "僵尸正常数值", purgatory);
                        // debug
                        Action onExis = () =>
                        {
                            for (int i = 0; i < InitZombieList.zombieList.Count; i++)
                            {
                                if (InitZombieList.zombieList[i].Count <= 1) continue;
                                int count = (int)(InitZombieList.zombieList[i].Count * LevelZombieSpawn[difficult]);
                                int end = InitZombieList.zombieList[i].Count - count + 1;
                                var startIndex = UnityEngine.Random.Range(0, end);
                                InitZombieList.zombieList[i] = InitZombieList.zombieList[i].GetRange(startIndex, count);
                            }
                        };
                        choiceMenu.actionOnExit = onExis;
                    }
                    yield break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(InitZombieList))]
    public static class InitZombieListPatch
    {
        [HarmonyPatch(nameof(InitZombieList.PickZombie))]
        [HarmonyPrefix]
        public static void PrePickZombie()
        {
            if (GameAPP.theBoardType == levelType && GameAPP.theBoardLevel == levelID)
            {
                foreach (var zt in zombieList)
                    InitZombieList.zombieToSpawns.Add(zt);
                InitZombieList.zombieToSpawns.Remove(ZombieType.NormalZombie);
            }
        }
    }

    [HarmonyPatch(typeof(WaveManager))]
    public static class WaveManagerPatch
    {
        [HarmonyPatch(nameof(WaveManager.GetMaxWave))]
        [HarmonyPostfix]
        public static void PostGetMaxWave(ref int __result)
        {
            if (GameAPP.theBoardType == levelType && GameAPP.theBoardLevel == levelID)
            {
                __result = 40;
            }
        }
    }

    [HarmonyPatch(typeof(Board))]
    public static class BoardPatch
    {
        [HarmonyPatch(nameof(Board.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            if (GameAPP.theBoardType == levelType && GameAPP.theBoardLevel == levelID)
            {
                var plant = CreatePlant.Instance.SetPlant(0, 2, PlantType.EndoFlame);
                plant.AddComponent<ControlPlant>();
                plant.uncrashable = true;
                var dataDic = ZombieDataManager.zombieDataDic;
                (KeyCodeManager.ShowZombieHealth, LevelShowZombieHealth) = (LevelShowZombieHealth, KeyCodeManager.ShowZombieHealth);
                (KeyCodeManager.ZombieGlove, LevelChangeGlove) = (LevelChangeGlove, KeyCodeManager.ZombieGlove);
                (dataDic[ZombieType.EndoFlameZombie].summonLevel, EndoFlame_Level) = (EndoFlame_Level, dataDic[ZombieType.EndoFlameZombie].summonLevel);
                (dataDic[ZombieType.EndoFlameZombie].summonWeight, EndoFlame_Weight) = (EndoFlame_Weight, dataDic[ZombieType.EndoFlameZombie].summonWeight);
                (dataDic[ZombieType.JalaSquashZombie].summonLevel, Jala_Level) = (Jala_Level, dataDic[ZombieType.JalaSquashZombie].summonLevel);
                (dataDic[ZombieType.JalaSquashZombie].summonWeight, Jala_Weight) = (Jala_Weight, dataDic[ZombieType.JalaSquashZombie].summonWeight);
            }
        }

        [HarmonyPatch(nameof(Board.Die))]
        [HarmonyPostfix]
        public static void PostDie()
        {
            if (GameAPP.theBoardType == levelType && GameAPP.theBoardLevel == levelID)
            {
                var dataDic = ZombieDataManager.zombieDataDic;
                (KeyCodeManager.ShowZombieHealth, LevelShowZombieHealth) = (LevelShowZombieHealth, KeyCodeManager.ShowZombieHealth);
                (KeyCodeManager.ZombieGlove, LevelChangeGlove) = (LevelChangeGlove, KeyCodeManager.ZombieGlove);
                (dataDic[ZombieType.EndoFlameZombie].summonLevel, EndoFlame_Level) = (EndoFlame_Level, dataDic[ZombieType.EndoFlameZombie].summonLevel);
                (dataDic[ZombieType.EndoFlameZombie].summonWeight, EndoFlame_Weight) = (EndoFlame_Weight, dataDic[ZombieType.EndoFlameZombie].summonWeight);
                (dataDic[ZombieType.JalaSquashZombie].summonLevel, Jala_Level) = (Jala_Level, dataDic[ZombieType.JalaSquashZombie].summonLevel);
                (dataDic[ZombieType.JalaSquashZombie].summonWeight, Jala_Weight) = (Jala_Weight, dataDic[ZombieType.JalaSquashZombie].summonWeight);
                difficult = Difficult.Normal;
            }
        }
    }
    #endregion
    #region 植物
    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch(nameof(Plant.TakeDamage))]
        [HarmonyPrefix]
        public static void PreTakeDamage(Plant __instance, ref int damage)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType && __instance.thePlantType == PlantType.EndoFlame)
            {
                damage = __instance.thePlantMaxHealth / 3;
            }
        }

        [HarmonyPatch(nameof(Plant.Die))]
        [HarmonyPostfix]
        public static void PostDie(Plant __instance)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType && __instance.thePlantType == PlantType.EndoFlame && 
                __instance.GetComponent<ControlPlant>() != null)
            {
                UIMgr.EnterLoseMenu(UnityEngine.Random.Range(0, 100) < 1 ? "飘老板似了！" : "火红莲死亡了");
            }
        }
    }

    [HarmonyPatch(typeof(ZombieEndoFlame))]
    public static class ZombieEndoFlamePatch
    {
        [HarmonyPatch(nameof(ZombieEndoFlame.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(ZombieEndoFlame __instance)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType)
            {
                __instance.zombieTypes = new();
                // 窝瓜 : 辣窝 = 4 : 1
                __instance.zombieTypes.Add(ZombieType.SquashZombie);
                __instance.zombieTypes.Add(ZombieType.SquashZombie);
                __instance.zombieTypes.Add(ZombieType.SquashZombie);
                __instance.zombieTypes.Add(ZombieType.SquashZombie);
                __instance.zombieTypes.Add(ZombieType.JalaSquashZombie);
            }
        }
    }

    [HarmonyPatch(typeof(ZombieEndoFlame), nameof(ZombieEndoFlame.DieEvent))]
    public static class ZombieEndoFlameDieEventPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ZombieEndoFlame __instance)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType)
            {
                __instance.zombieFertilize = null;
            }
        }

        public static Exception Finalizer()
        {
            return null;
        }
    }

    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPatch(nameof(Mouse.LeftClickWithNothing))]
        [HarmonyPostfix]
        public static void PostLeftClickWithNothing(Mouse __instance)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType && __instance.theItemOnMouse == null)
            {
                __instance.TryToPickPlant();
            }
        }
    }
    #endregion
    #region 僵尸
    [HarmonyPatch(typeof(BungiZombie))]
    public static class BungiZombiePatch
    {
        [HarmonyPatch(nameof(BungiZombie.Awake))]
        [HarmonyPrefix]
        public static void PreAwake(BungiZombie __instance)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType)
            {
                __instance.setZombie = true;
                __instance.theSetZombieType = BungiPool[UnityEngine.Random.Range(0, BungiPool.Count)];
            }
        }
    }

    [HarmonyPatch(typeof(Lawnf))]
    public static class LawnfPatch
    {
        [HarmonyPatch(nameof(Lawnf.GetRandomBungiType))]
        [HarmonyPostfix]
        public static void PostGetRandomBungiType(ref ZombieType __result)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType)
            {
                __result = BungiPool[UnityEngine.Random.Range(0, BungiPool.Count)];
            }
        }
    }

    [HarmonyPatch(typeof(FlagZombie))]
    public static class FlagZombiePatch
    {
        [HarmonyPatch(nameof(FlagZombie.Start))]
        [HarmonyPostfix]
        public static void PostStart(FlagZombie __instance)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType)
            {
                foreach (var child in global::Core.Lawnf.GetChilds(__instance.transform))
                    if (child.GetComponent<SpriteRenderer>() != null)
                        child.GetComponent<SpriteRenderer>().enabled = false;
                __instance.Die();
            }
        }
    }

    [HarmonyPatch(typeof(CreateZombie))]
    public static class CreateZombiePatch
    {
        [HarmonyPatch(nameof(CreateZombie.SetZombie))]
        [HarmonyPrefix]
        public static bool PreSetZombie(ref ZombieType theZombieType)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType)
            {
                if (TypeMgr.UltimateZombie(theZombieType) && theZombieType != theNewZombieType)
                    return false;
                //// 僵尸保留
                //if (UnityEngine.Random.Range(0f, 1f) > LevelZombieSpawn[difficult])
                //    return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(CreateZombie.SetZombie))]
        [HarmonyPostfix]
        public static void PostSetZombie(ref Zombie __result)
        {
            if (GameAPP.theBoardLevel == levelID && GameAPP.theBoardType == levelType && __result != null && GameAPP.theGameStatus == GameStatus.InGame)
            {
                // 设置血量
                __result.theHealth = (int)(__result.theHealth * LevelZombieHealthRate[difficult]);
                __result.theMaxHealth = __result.theHealth;
                // 设置速度
                __result.theOriginSpeed *= LevelZombieSpeed[difficult];
            }
        }
    }
    #endregion
}
