using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Text.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

///
///Credit to likefengzi(https://github.com/likefengzi)(https://space.bilibili.com/237491236)
///
namespace CustomizeLib.BepInEx
{
    /// <summary>
    /// 注册肥料使用事件
    /// </summary>
    [HarmonyPatch(typeof(Fertilize))]
    public class FertilizePatch
    {
        [HarmonyPatch(nameof(Fertilize.Upgrade))]
        [HarmonyPostfix]
        public static void PostUpgrade(Fertilize __instance)
        {
            if (__instance == null || __instance.theTargetPlant == null) return;

            int column = __instance.theTargetPlant.thePlantColumn;
            int row = __instance.theTargetPlant.thePlantRow;

            List<Plant> plants = Lawnf.Get1x1Plants(column, row).ToArray().ToList<Plant>(); // 获取植物，il2cpp窝爱你
            if (plants == null) return;

            for (int i = 0; i < plants.Count; i++)
            {
                Plant plant = plants[i];
                if (plant == null) continue;
                if (plant.thePlantColumn != column || plant.thePlantRow != row) continue;
                if (Board.Instance == null) return;

                if (CustomCore.CustomUseFertilize.ContainsKey(plant.thePlantType))
                {
                    CustomCore.CustomUseFertilize[plant.thePlantType](plant);
                }
            }

            UnityEngine.Object.Destroy(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(AlmanacMenu))]
    public static class AlmanacMenuPatch
    {
        [HarmonyPatch(nameof(AlmanacMenu.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(AlmanacMenu __instance)
        {
            __instance.transform.FindChild("AlmanacPlant2").FindChild("Cards").GetComponent<GridManager>().maxY = 75;
        }
    }

    /// <summary>
    /// 初始化结束显示换肤按钮，加载皮肤
    /// </summary>
    /// <param name="__instance"></param>
    /// <returns></returns>
    /// <summary>
    /// 植物图鉴
    /// </summary>
    [HarmonyPatch(typeof(AlmanacPlantBank))]
    public static class AlmanacMgrPatch
    {
        /// <summary>
        /// 初始化结束显示换肤按钮，加载皮肤
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(AlmanacPlantBank __instance)
        {
            PlantType plantType = (PlantType)__instance.theSeedType;
            //初次加载皮肤
            if (!CustomCore.CustomPlantsSkin.ContainsKey(plantType))
            {
                //是否有皮肤成功
                bool buttonFlag = __instance.skinButton.active;
                //exe的位置
                string? fullName = Directory.GetParent(Application.dataPath)?.FullName;
                if (fullName != null)
                {
                    //寻找Mods/Skin/
                    string modsPath = Path.Combine(fullName, "BepInEx", "plugins", "Skin");
                    if (Directory.Exists(modsPath))
                    {
                        //只要skin_开头的文件
                        string[] files = Directory.GetFiles(modsPath, "skin_*");

                        foreach (string file in files)
                        {
                            try
                            {
                                //如果文件名"Skin_"后面的id匹配
                                if (((int)plantType).ToString() == Path.GetFileName(file)[5..])
                                {
                                    //加载资源文件
                                    AssetBundle ab = AssetBundle.LoadFromFile(file);
                                    //尝试加载json
                                    bool jsonFlag = false;
                                    CustomPlantData plantDataFromJson = default;
                                    CustomPlantAlmanac plantAlmanac = default;
                                    Dictionary<int, int> bulletTypesFormJson = [];
                                    foreach (string jsonFile in files)
                                    {
                                        try
                                        {
                                            if (((int)plantType) + ".json" ==
                                                Path.GetFileName(jsonFile)[5..])
                                            {
                                                // 读取 JSON 文件内容
                                                string jsonContent = File.ReadAllText(jsonFile);

                                                // 反序列化 JSON 内容
                                                var options = new JsonSerializerOptions
                                                {
                                                    PropertyNameCaseInsensitive = true // 允许不区分大小写的属性名称匹配
                                                };

                                                JsonSkinObject? root =
                                                    JsonSerializer.Deserialize<JsonSkinObject>(jsonContent, options);

                                                // 访问数据
                                                if (root != null)
                                                {
                                                    plantDataFromJson = root.CustomPlantData;
                                                    root.TypeMgrExtraSkin.AddValueToTypeMgrExtraSkinBackup(plantType);
                                                    bulletTypesFormJson = root.CustomBulletType;
                                                    plantAlmanac = root.PlantAlmanac;
                                                }

                                                //找到了json文件并成功加载
                                                jsonFlag = true;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    }

                                    //获得新皮肤预制体
                                    GameObject? newPrefab = null;
                                    try
                                    {
                                        newPrefab = ab.GetAsset<GameObject>("Prefab");
                                        newPrefab.tag = "Plant";
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }

                                    //获得新皮肤预览图
                                    GameObject? newPreview = null;
                                    try
                                    {
                                        newPreview = ab.GetAsset<GameObject>("Preview");
                                        newPreview.tag = "Preview";
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }

                                    //成功加载预制体
                                    if (newPrefab != null)
                                    {
                                        //旧的预制体
                                        GameObject prefab;
                                        try
                                        {
                                            prefab = GameAPP.resourcesManager.plantPrefabs[jsonFlag ? (PlantType)plantDataFromJson.ID : plantType];
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                            prefab = GameAPP.resourcesManager.plantPrefabs[plantType];
                                        }

                                        //拿到脚本
                                        Plant plant = prefab.GetComponent<Plant>();
                                        //添加到新的预制体上
                                        newPrefab.AddComponent(plant.GetIl2CppType());
                                        CustomPlantMonoBehaviour temp =
                                            newPrefab.AddComponent<CustomPlantMonoBehaviour>();
                                        CustomPlantMonoBehaviour.BulletTypes.Add(plantType, bulletTypesFormJson);

                                        Plant newPlant = newPrefab.GetComponent<Plant>();

                                        //指定id
                                        newPlant.thePlantType = plantType;

                                        //shoot成员都有问题，清空
                                        newPlant.shoot = null;
                                        newPlant.shoot2 = null;
                                        //指定shoot
                                        try
                                        {
                                            newPlant.FindShoot(newPrefab.transform);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    }

                                    CustomPlantData newCustomPlantData = default;
                                    //判断是否成功加载对应的json
                                    if (jsonFlag)
                                    {
                                        //使用json中的数据
                                        newCustomPlantData = new()
                                        {
                                            ID = (int)plantType,
                                            PlantData = plantDataFromJson.PlantData,
                                            Prefab = GameAPP.resourcesManager.plantPrefabs[plantType],
                                            Preview = GameAPP.resourcesManager.plantPreviews[plantType]
                                        };
                                    }
                                    else
                                    {
                                        //没有json文件，使用默认数据
                                        //数据加载到自定义皮肤中
                                        newCustomPlantData = new()
                                        {
                                            ID = (int)plantType,
                                            PlantData = PlantDataLoader.plantDatas[plantType],
                                            Prefab = GameAPP.resourcesManager.plantPrefabs[plantType],
                                            Preview = GameAPP.resourcesManager.plantPreviews[plantType]
                                        };
                                    }

                                    //成功读取了谁就加载谁
                                    if (newPrefab != null)
                                    {
                                        newCustomPlantData.Prefab = newPrefab;
                                    }

                                    if (newPreview != null)
                                    {
                                        newCustomPlantData.Preview = newPreview;
                                    }

                                    CustomCore.CustomPlantsSkin.Add(plantType, newCustomPlantData);
                                    //加载图鉴
                                    try
                                    {
                                        CustomCore.PlantsSkinAlmanac.Add(plantType, jsonFlag ?
                                            (plantAlmanac.Name, plantAlmanac.Description) : null);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }

                                    //有皮肤，按钮可以显示
                                    buttonFlag = true;
                                    CustomCore.CustomPlantsSkinActive[plantType] = false;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                }

                __instance.skinButton.SetActive(buttonFlag);
            }
            else
            {
                //有皮肤，按钮可以显示
                __instance.skinButton.SetActive(true);
            }

            if (CustomCore.CustomPlants.ContainsKey(plantType))
            {
                //二创植物，按钮可以显示
                __instance.skinButton.SetActive(CustomCore.CustomPlantsSkin.ContainsKey(plantType));
            }
        }

        /// <summary>
        /// 从json加载植物信息
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch("InitNameAndInfoFromJson")]
        [HarmonyPrefix]
        public static bool PreInitNameAndInfoFromJson(AlmanacPlantBank __instance)
        {
            //如果自定义植物图鉴信息包含
            if (CustomCore.PlantsAlmanac.ContainsKey((PlantType)__instance.theSeedType))
            {
                //遍历图鉴上的组件
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    Transform childTransform = __instance.transform.GetChild(i);
                    if (childTransform == null)
                    {
                        continue;
                    }

                    //植物姓名
                    if (childTransform.name == "Name")
                    {
                        childTransform.GetComponent<TextMeshPro>().text =
                            CustomCore.PlantsAlmanac[(PlantType)__instance.theSeedType].Item1;
                        childTransform.GetChild(0).GetComponent<TextMeshPro>().text =
                            CustomCore.PlantsAlmanac[(PlantType)__instance.theSeedType].Item1;
                    }

                    //植物信息
                    if (childTransform.name == "Info")
                    {
                        TextMeshPro info = childTransform.GetComponent<TextMeshPro>();
                        info.overflowMode = TextOverflowModes.Page;
                        info.fontSize = 40;
                        info.text = CustomCore.PlantsAlmanac[(PlantType)__instance.theSeedType].Item2;
                        __instance.introduce = info;
                    }

                    //植物阳光
                    if (childTransform.name == "Cost")
                    {
                        childTransform.GetComponent<TextMeshPro>().text = "";
                    }
                }

                //阻断原始的加载
                return false;
            }

            if (CustomCore.CustomPlantsSkinActive.ContainsKey((PlantType)__instance.theSeedType) && CustomCore.PlantsSkinAlmanac.ContainsKey((PlantType)__instance.theSeedType) && CustomCore.CustomPlantsSkinActive[(PlantType)__instance.theSeedType])
            {
                var alm = CustomCore.PlantsSkinAlmanac[(PlantType)__instance.theSeedType];
                if (alm is null) return true;
                var almanac = alm.Value;
                //遍历图鉴上的组件
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    Transform childTransform = __instance.transform.GetChild(i);
                    if (childTransform == null)
                    {
                        continue;
                    }

                    //植物姓名
                    if (childTransform.name == "Name")
                    {
                        childTransform.GetComponent<TextMeshPro>().text = almanac.Item1;
                        childTransform.GetChild(0).GetComponent<TextMeshPro>().text = almanac.Item1;
                    }

                    //植物信息
                    if (childTransform.name == "Info")
                    {
                        TextMeshPro info = childTransform.GetComponent<TextMeshPro>();
                        info.overflowMode = TextOverflowModes.Page;
                        info.fontSize = 40;
                        info.text = almanac.Item2;
                        __instance.introduce = info;
                    }

                    //植物阳光
                    if (childTransform.name == "Cost")
                    {
                        childTransform.GetComponent<TextMeshPro>().text = "";
                    }
                }

                //阻断原始的加载
                return false;
            }

            return true;
        }

        /// <summary>
        /// 图鉴中鼠标按下，用于翻页
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch("OnMouseDown")]
        [HarmonyPrefix]
        public static bool PreOnMouseDown(AlmanacPlantBank __instance)
        {
            //右侧显示
            __instance.introduce =
                __instance.gameObject.transform.FindChild("Info").gameObject.GetComponent<TextMeshPro>();
            //页数
            __instance.pageCount = __instance.introduce.m_pageNumber * 1;
            //下一页
            if (__instance.currentPage <= __instance.introduce.m_pageNumber)
            {
                ++__instance.currentPage;
            }
            else
            {
                __instance.currentPage = 1;
            }

            //翻页
            __instance.introduce.pageToDisplay = __instance.currentPage;

            //阻断原始翻页
            return false;
        }
    }

    [HarmonyPatch(typeof(AlmanacMgrZombie))]
    public static class AlmanacMgrZombiePatch
    {
        [HarmonyPatch("InitNameAndInfoFromJson")]
        [HarmonyPrefix]
        public static bool PreInitNameAndInfoFromJson(AlmanacMgrZombie __instance)
        {
            if (CustomCore.ZombiesAlmanac.ContainsKey(__instance.theZombieType))
            {
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    Transform childTransform = __instance.transform.GetChild(i);
                    if (childTransform == null)
                        continue;
                    if (childTransform.name == "Name")
                    {
                        childTransform.GetComponent<TextMeshPro>().text = CustomCore.ZombiesAlmanac[__instance.theZombieType].Item1;
                        childTransform.GetChild(0).GetComponent<TextMeshPro>().text = CustomCore.ZombiesAlmanac[__instance.theZombieType].Item1;
                    }
                    if (childTransform.name == "Info")
                    {
                        TextMeshPro info = childTransform.GetComponent<TextMeshPro>();
                        info.overflowMode = TextOverflowModes.Page;
                        info.fontSize = 40;
                        info.text = CustomCore.ZombiesAlmanac[__instance.theZombieType].Item2;
                        __instance.introduce = info;
                    }
                    if (childTransform.name == "Cost")
                        childTransform.GetComponent<TextMeshPro>().text = "";
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ConveyBeltMgr))]
    public static class ConveyBeltMgrPatch
    {
        [HarmonyPatch(nameof(ConveyBeltMgr.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(ConveyBeltMgr __instance)
        {
            if (Utils.IsCustomLevel(out var levelData) && levelData.BoardTag.isConvey && levelData.ConveyBeltPlantTypes().Count > 0)
            {
                __instance.plants = levelData.ConveyBeltPlantTypes().ToIl2CppList();
            }
        }

        [HarmonyPatch(nameof(ConveyBeltMgr.GetCardPool))]
        [HarmonyPostfix]
        public static void PostGetCardPool(ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            if (Utils.IsCustomLevel(out var levelData) && levelData.BoardTag.isConvey && levelData.ConveyBeltPlantTypes().Count > 0)
            {
                __result = levelData.ConveyBeltPlantTypes().ToIl2CppList();
            }
        }
    }

    [HarmonyPatch(typeof(CreatePlant), "SetPlant")]
    public static class CreatePlantPatch
    {
        public static void Postfix(ref GameObject __result)
        {
            if (__result is not null && __result.TryGetComponent<Plant>(out var plant) && CustomCore.CustomPlantTypes.Contains(plant.thePlantType))
            {
                TypeMgr.GetPlantTag(plant);
            }
        }
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("LoadResources")]
        public static void LoadResources()
        {
            foreach (var plant in CustomCore.CustomPlants)
            {
                GameAPP.resourcesManager.plantPrefabs[plant.Key] = plant.Value.Prefab;
                GameAPP.resourcesManager.plantPrefabs[plant.Key].tag = "Plant";
                if (!GameAPP.resourcesManager.allPlants.Contains(plant.Key)) GameAPP.resourcesManager.allPlants.Add(plant.Key);
                if (plant.Value.PlantData is not null)
                {
                    PlantDataLoader.plantData[(int)plant.Key] = plant.Value.PlantData;//注册植物数据
                    PlantDataLoader.plantDatas.Add(plant.Key, plant.Value.PlantData);
                }
                GameAPP.resourcesManager.plantPreviews[plant.Key] = plant.Value.Preview;
                GameAPP.resourcesManager.plantPreviews[plant.Key].tag = "Preview";
            }
            Il2CppSystem.Array array = MixData.data.Cast<Il2CppSystem.Array>();
            foreach (var f in CustomCore.CustomFusions)
            {
                array.SetValue(f.Item1, f.Item2, f.Item3);
            }
            foreach (var z in CustomCore.CustomZombies)
            {
                if (!GameAPP.resourcesManager.allZombieTypes.Contains(z.Key)) GameAPP.resourcesManager.allZombieTypes.Add(z.Key);
                GameAPP.resourcesManager.zombiePrefabs[z.Key] = z.Value.Item1;
                GameAPP.resourcesManager.zombiePrefabs[z.Key].tag = "Zombie";
            }
            foreach (var bullet in CustomCore.CustomBullets)
            {
                GameAPP.resourcesManager.bulletPrefabs[bullet.Key] = bullet.Value;
                if (!GameAPP.resourcesManager.allBullets.Contains(bullet.Key)) GameAPP.resourcesManager.allBullets.Add(bullet.Key);
            }
            foreach (var par in CustomCore.CustomParticles)
            {
                GameAPP.particlePrefab[(int)par.Key] = par.Value;
                GameAPP.resourcesManager.particlePrefabs[par.Key] = par.Value;
                if (!GameAPP.resourcesManager.allParticles.Contains(par.Key)) GameAPP.resourcesManager.allParticles.Add(par.Key);
            }
            foreach (var spr in CustomCore.CustomSprites)
            {
                GameAPP.spritePrefab[spr.Key] = spr.Value;
            }
        }
    }

    /// <summary>
    /// 点击其他Button，隐藏二创植物界面
    /// </summary>
    [HarmonyPatch(typeof(UIButton), nameof(UIButton.OnMouseUpAsButton))]
    public static class HideCustomPlantCards
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            SelectCustomPlants.CloseCustomPlantCards();
        }

        [HarmonyPatch(nameof(UIButton.Start))]
        [HarmonyPostfix]
        public static void PostfixStart(UIButton __instance)
        {
            if (__instance.name == "LastPage" && Board.Instance != null && Board.Instance.isIZ)
            {
                SelectCustomPlants.InitCustomCards_IZ();
            }
        }
    }

    [HarmonyPatch(typeof(InGameUI))]
    public static class InGameUIPatch
    {
        [HarmonyPatch(nameof(InGameUI.SetUniqueText))]
        [HarmonyPostfix]
        public static void PostSetUniqueText(InGameUI __instance, ref Il2CppReferenceArray<TextMeshProUGUI> T)
        {
            if (GameAPP.theBoardType is (LevelType)66)
            {
                __instance.ChangeString(T, CustomCore.CustomLevels[GameAPP.theBoardLevel].Name());
            }
        }

        [HarmonyPatch(nameof(InGameUI.MoveCard))]
        [HarmonyPrefix]
        public static void PostMoveCard(ref CardUI card)
        {
            foreach (CheckCardState check in CustomCore.checkBehaviours)
            {
                if (check != null)
                {
                    check.movingCardUI = card.gameObject;
                    check.CheckState();
                }
            }
        }

        [HarmonyPatch(nameof(InGameUI.RemoveCardFromBank))]
        [HarmonyPostfix]
        public static void PostReMoveCardFromBank(ref CardUI card)
        {
            foreach (CheckCardState check in CustomCore.checkBehaviours)
            {
                if (check != null)
                {
                    check.movingCardUI = card.gameObject;
                    check.CheckState();
                }
            }
        }
    }

    [HarmonyPatch(typeof(InitBoard))]
    public static class InitBoardPatch
    {
        [HarmonyPatch(nameof(InitBoard.PreSelectCard))]
        [HarmonyPostfix]
        public static void PostPreSelectCard(InitBoard __instance)
        {
            if (GameAPP.theBoardType is (LevelType)66)
            {
                foreach (var c in CustomCore.CustomLevels[GameAPP.theBoardLevel].PreSelectCards())
                {
                    __instance.PreSelect(c);
                }
            }
        }

        [HarmonyPatch(nameof(InitBoard.RightMoveCamera))]
        [HarmonyPostfix]
        public static void PostRightMoveCamera()
        {
            if (GameAPP.theBoardType is not (LevelType)66) return;
            var levelData = CustomCore.CustomLevels[GameAPP.theBoardLevel];
            var travelMgr = GameAPP.gameAPP.GetOrAddComponent<TravelMgr>();
            foreach (var a in levelData.AdvBuffs())
            {
                if (a >= 0 && a < travelMgr.advancedUpgrades.Count)
                {
                    travelMgr.advancedUpgrades[a] = true;
                }
            }
            foreach (var u in levelData.UltiBuffs())
            {
                if (u.Item1 >= 0 && u.Item1 < travelMgr.ultimateUpgrades.Count && u.Item2 >= 0)
                {
                    travelMgr.ultimateUpgrades[u.Item1] = u.Item2;
                }
            }
            foreach (var p in levelData.UnlockPlants())
            {
                if (p >= 0 && p < travelMgr.unlockPlant.Count)
                {
                    travelMgr.unlockPlant[p] = true;
                }
            }
            foreach (var d in levelData.Debuffs())
            {
                if (d >= 0 && d < travelMgr.debuff.Count)
                {
                    travelMgr.debuff[d] = true;
                }
            }
        }

        [HarmonyPatch(nameof(InitBoard.MoveOverEvent))]
        [HarmonyPrefix]
        public static bool PreMoveOverEvent(InitBoard __instance, ref string direction)
        {
            if (GameAPP.theBoardType is not (LevelType)66) return true;
            var levelData = CustomCore.CustomLevels[GameAPP.theBoardLevel];
            if (direction == "right")
            {
                if (__instance.board is not null)
                {
                    if (__instance.board.cardSelectable)
                    {
                        // 设置游戏状态
                        GameAPP.theGameStatus = GameStatus.Selecting;

                        // UI控制
                        InGameUI.Instance.ConveyorBelt.SetActive(false);
                        InGameUI.Instance.Bottom.SetActive(true);

                        // 启动协程移动UI元素
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.SeedBank, 79f, 0));
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.Bottom, 525f, 1));
                    }
                    else
                    {
                        // 延迟执行方法
                        __instance.Invoke("LeftMoveCamera", 1.5f);
                        InGameUI.Instance.Bottom.SetActive(false);
                    }
                }
            }
            else if (direction == "left")
            {
                if (__instance.board is null) return false;

                if (!__instance.board.cardSelectable)
                {
                    if (__instance.board.cardBank)
                    {
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.SeedBank, 79f, 0));
                        __instance.AddCard();
                    }
                    else
                    {
                        InGameUI.Instance.SeedBank.SetActive(false);
                    }
                    InGameUI.Instance.Bottom.SetActive(false);
                }

                // 音量渐变协程
                __instance.StartCoroutine(__instance.DecreaseVolume());

                // 降低UI位置
                InGameUI.Instance.LowerUI();

                // 初始化割草机（特定模式下）
                if (!__instance.board.boardTag.disableMower)
                {
                    __instance.InitMower();
                }

                // 雾效果移动
                if (__instance.board.fog != null)
                {
                    Vector3 fogPosition = __instance.board.fog.transform.position;
                    Vector3 boardPosition = __instance.board.background.transform.position;

                    FogMgr.Instance.MoveObject(
                        new(fogPosition.x,
                        fogPosition.y,
                        boardPosition.z),
                        10f  // 移动速度
                    );
                }

                // BOSS战特殊处理
                float invokeDelay = 0.5f;
                if (__instance.board.boardTag.isBoss || __instance.board.boardTag.isBoss2)
                {
                    GameObject zombie = CreateZombie.Instance.SetZombie(0, levelData.RealBoss2 ? ZombieType.ZombieBoss2 : ZombieType.ZombieBoss, 0f);
                    Zombie zombieComp = zombie.GetComponent<Zombie>();

                    if (__instance.board.boss2)
                    {
                        Lawnf.SetZombieHealth(zombieComp, 5f);
                    }
                    invokeDelay = 3.5f;
                    __instance.board.boss2 = levelData.RealBoss2;
                }

                // 延迟调用方法
                __instance.Invoke("ReadySetPlant", invokeDelay);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(InitZombieList))]
    public static class InitZombieListPatch
    {
        [HarmonyPatch(nameof(InitZombieList.InitList))]
        [HarmonyPostfix]
        public static void PostInitList()
        {
            if (GameAPP.theBoardType is (LevelType)66)
            {
                foreach (var z in CustomCore.CustomLevels[GameAPP.theBoardLevel].ZombieList())
                {
                    InitZombieList.zombieTypeList.Add(z);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Money))]
    public static class MoneyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ReinforcePlant")]
        public static bool PreReinforcePlant(Money __instance, ref Plant plant)
        {
            if (CustomCore.SuperSkills.ContainsKey(plant.thePlantType))
            {
                var cost = CustomCore.SuperSkills[plant.thePlantType].Item1(plant);

                if (Board.Instance.theMoney < cost)
                {
                    InGameText.Instance.ShowText($"大招需要{cost}金币", 5);
                    return false;
                }
                if (plant.SuperSkill())
                {
                    CustomCore.SuperSkills[plant.thePlantType].Item2(plant);
                    plant.AnimSuperShoot();
                    __instance.UsedEvent(plant.thePlantColumn, plant.thePlantRow, cost);
                    __instance.OtherSuperSkill(plant);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetPlantsOnMouse")]
        public static void PostGetPlantsOnMouse(ref Il2CppSystem.Collections.Generic.List<Plant> __result)
        {
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                if (__result[i] is not null && TypeMgr.BigNut(__result[i].thePlantType))
                {
                    __result.RemoveAt(i);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("LeftClickWithNothing")]
        public static void PostLeftClickWithNothing()
        {
            foreach (GameObject gameObject in (List<GameObject>)[..from RaycastHit2D raycastHit2D in
                                           (RaycastHit2D[])Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                                           Vector2.zero) select raycastHit2D.collider.gameObject])
            {
                if (gameObject.TryGetComponent<Plant>(out var plant) && CustomCore.CustomPlantClicks.ContainsKey(plant.thePlantType))
                {
                    CustomCore.CustomPlantClicks[plant.thePlantType](plant);
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(NoticeMenu), "Awake")]
    public static class NoticeMenuPatch
    {
        public static void Postfix() => CustomCore.Instance.Value.LateInit();
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("UseItem")]
        public static void PostUseItem(Plant __instance, ref BucketType type, ref Bucket bucket)
        {
            if (CustomCore.CustomUseItems.ContainsKey((__instance.thePlantType, type)))
            {
                CustomCore.CustomUseItems[(__instance.thePlantType, type)](__instance);
                UnityEngine.Object.Destroy(bucket.gameObject);
            }
        }
    }
    /// <summary>
    /// 刷新卡牌贴图
    /// </summary>
    [HarmonyPatch(typeof(SeedLibrary))]
    public static class SeedLibraryPatch
    {
        [HarmonyPatch(nameof(SeedLibrary.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(SeedLibrary __instance)
        {
            //为什么PostShowNormalCard会无限递归？？？
            //Grid
            __instance.transform.FindCardUIAndChangeSprite();
        }

        [HarmonyPatch(nameof(SeedLibrary.Start))]
        [HarmonyPostfix]
        public static void PostStart(SeedLibrary __instance)
        {
            // 注册自定义卡牌
            GameObject? MyCard = Utils.GetCardGameObject();
            // MelonLogger.Msg(MyCard == null);
            if (MyCard == null)
                return;
            foreach (var card in CustomCore.CustomCards)
            {
                List<Transform?> parents = new List<Transform?>();
                List<PlantType> cardsOnSeedBank = new List<PlantType>();
                GameObject seedGroup = InGameUI.Instance.SeedBank.transform.GetChild(0).gameObject;
                for (int i = 0; i < seedGroup.transform.childCount; i++)
                {
                    GameObject seed = seedGroup.transform.GetChild(i).gameObject;
                    if (seed.transform.childCount > 0)
                        cardsOnSeedBank.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType);
                }
                foreach (Func<Transform?> cardFunc in card.Value)
                {
                    Transform? result = cardFunc();
                    if (!parents.Contains(result))
                    {
                        GameObject TempCard = Instantiate(MyCard, result);
                        if (TempCard != null)
                        {
                            //设置父节点
                            //激活
                            TempCard.SetActive(true);
                            //设置位置
                            TempCard.transform.position = MyCard.transform.position;
                            TempCard.transform.localPosition = MyCard.transform.localPosition;
                            TempCard.transform.localScale = MyCard.transform.localScale;
                            TempCard.transform.localRotation = MyCard.transform.localRotation;
                            //背景图片
                            // 设置背景植物图标
                            Image image = TempCard.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[card.Key].GetComponent<SpriteRenderer>().sprite;
                            image.SetNativeSize();
                            // 设置背景价格
                            TempCard.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1.ToString();
                            //卡片
                            CardUI component = TempCard.transform.GetChild(1).GetComponent<CardUI>();
                            component.gameObject.SetActive(true);
                            //修改图片
                            Mouse.Instance.ChangeCardSprite(card.Key, component);
                            // 修改缩放
                            RectTransform bgRect = TempCard.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                            RectTransform packetRect = TempCard.transform.GetChild(1).GetChild(0).GetComponent<RectTransform>();
                            bgRect.localScale = packetRect.localScale;
                            bgRect.sizeDelta = packetRect.sizeDelta;
                            //设置数据
                            component.thePlantType = card.Key;
                            component.theSeedType = (int)card.Key;
                            component.theSeedCost = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1;
                            component.fullCD = PlantDataLoader.plantDatas[card.Key].field_Public_Single_2;
                            if (cardsOnSeedBank.Contains(card.Key))
                                TempCard.transform.GetChild(1).gameObject.SetActive(false);
                            CheckCardState customComponent = TempCard.AddComponent<CheckCardState>();
                            customComponent.card = TempCard;
                            customComponent.cardType = component.thePlantType;
                            parents.Add(cardFunc());
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 进入一局游戏，显示二创植物Button
    /// </summary>
    [HarmonyPatch(typeof(Board), nameof(Board.Start))]
    public static class ShowCustomPlantCards
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            SelectCustomPlants.InitCustomCards();
        }
    }

    /// <summary>
    /// 点击换肤
    /// </summary>
    [HarmonyPatch(typeof(SkinButton), nameof(SkinButton.OnMouseUpAsButton))]
    public static class SkinButton_OnMouseUpAsButton
    {
        [HarmonyPrefix]
        public static bool Prefix(SkinButton __instance)
        {
            PlantType plantType = (PlantType)__instance.showPlant.theSeedType;
            if (CustomCore.CustomPlantsSkin.ContainsKey(plantType))
            {
                CustomPlantData customPlantData = CustomCore.CustomPlantsSkin[plantType];
                //交换预制体引用
                (GameAPP.resourcesManager.plantPrefabs[plantType], customPlantData.Prefab) =
                    (customPlantData.Prefab, GameAPP.resourcesManager.plantPrefabs[plantType]);

                //交换预览图
                (GameAPP.resourcesManager.plantPreviews[plantType], customPlantData.Preview) =
                    (customPlantData.Preview, GameAPP.resourcesManager.plantPreviews[plantType]);

                //交换植物数据
                if (customPlantData.PlantData is not null)
                {
                    (PlantDataLoader.plantData[(int)plantType], customPlantData.PlantData) =
                        (customPlantData.PlantData, PlantDataLoader.plantData[(int)plantType]);
                    PlantDataLoader.plantDatas[plantType] = PlantDataLoader.plantData[(int)plantType];
                }
                CustomCore.CustomPlantsSkin[plantType] = customPlantData;

                //交换特性列表
                Extensions.SwapTypeMgrExtraSkinAndBackup(plantType);

                //GameObject prefab = GameAPP.resourcesManager.plantPrefabs[(PlantType)__instance.showPlant.theSeedType];

                //Transform transform = AlmanacMenu.Instance.currentShowCtrl.localShowPlant.transform.parent;

                //旧的，传递完数据就销毁
                GameObject oldGameObject = AlmanacMenu.Instance.currentShowCtrl.localShowPlant;
                oldGameObject.name = "ToDestroy";
                // //实例化新的
                // AlmanacMenu.Instance.currentShowCtrl.localShowPlant = UnityEngine.Object.Instantiate(prefab, transform);
                // //同步位置
                // AlmanacMenu.Instance.currentShowCtrl.localShowPlant.transform.position =
                //     oldGameObject.transform.position;
                // AlmanacMenu.Instance.currentShowCtrl.localShowPlant.transform.localPosition =
                //     oldGameObject.transform.localPosition;

                //销毁旧的
                UnityEngine.Object.Destroy(oldGameObject);

                //标记是否换肤
                CustomCore.CustomPlantsSkinActive[plantType] = !CustomCore.CustomPlantsSkinActive[plantType];
                //__instance.showPlant.gameObject.SetActive(false);
                __instance.showPlant.InitNameAndInfoFromJson();
                AlmanacMenu.Instance.currentShowCtrl.localShowPlant =
                    AlmanacMenu.Instance.currentShowCtrl.SetPlant((int)plantType);

                if (AlmanacMenu.Instance.currentShowCtrl.localShowPlant.GetComponent<CustomPlantMonoBehaviour>() !=
                    null)
                {
                    UnityEngine.Object.Destroy(AlmanacMenu.Instance.currentShowCtrl.localShowPlant
                        .GetComponent<CustomPlantMonoBehaviour>());
                }

                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 二创词条文本染色
    /// </summary>
    [HarmonyPatch(typeof(TravelBuffOptionButton))]
    public static class TravelBuffOptionButtonPatch
    {
        [HarmonyPatch(nameof(TravelBuffOptionButton.SetBuff))]
        public static void PostSetBuff(TravelBuffOptionButton __instance, ref BuffType buffType, ref int buffIndex)
        {
            if (buffType is BuffType.AdvancedBuff && CustomCore.CustomAdvancedBuffs.ContainsKey(buffIndex)
                && CustomCore.CustomAdvancedBuffs[buffIndex].Item5 is not null)
            {
                __instance.introduce.text = $"<color={CustomCore.CustomAdvancedBuffs[buffIndex].Item5}>{__instance.introduce.text}</color>";
            }
        }
    }

    [HarmonyPatch(typeof(TravelBuff))]
    public static class TravelBuffPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ChangeSprite")]
        public static void PreChangeSprite(TravelBuff __instance)
        {
            if (__instance.theBuffType == 1 && CustomCore.CustomAdvancedBuffs.ContainsKey(__instance.theBuffNumber))
            {
                __instance.thePlantType = CustomCore.CustomAdvancedBuffs[__instance.theBuffNumber].Item1;
            }
            if (__instance.theBuffType == 2 && CustomCore.CustomUltimateBuffs.ContainsKey(__instance.theBuffNumber))
            {
                __instance.thePlantType = CustomCore.CustomUltimateBuffs[__instance.theBuffNumber].Item1;
            }
        }
    }

    /// <summary>
    /// 二创词条文本染色
    /// </summary>
    [HarmonyPatch(typeof(TravelLookBuff))]
    public static class TravelLookBuffPatch
    {
        [HarmonyPatch(nameof(TravelLookBuff.SetBuff))]
        public static void PostSetBuff(TravelLookBuff __instance, ref BuffType buffType, ref int buffIndex)
        {
            if (buffType is BuffType.AdvancedBuff && CustomCore.CustomAdvancedBuffs.ContainsKey(buffIndex)
                && CustomCore.CustomAdvancedBuffs[buffIndex].Item5 is not null)
            {
                __instance.introduce.text = $"<color={CustomCore.CustomAdvancedBuffs[buffIndex].Item5}>{__instance.introduce.text}</color>";
            }
        }
    }

    [HarmonyPatch(typeof(TravelMgr))]
    public static class TravelMgrPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void PostAwake(TravelMgr __instance)
        {
            if (CustomCore.CustomAdvancedBuffs.Count > 0)
            {
                bool[] newAdv = new bool[__instance.advancedUpgrades.Count + CustomCore.CustomAdvancedBuffs.Count];
                int[] newAdvUnlock = new int[__instance.advancedUnlockRound.Count + CustomCore.CustomAdvancedBuffs.Count];
                Array.Copy(__instance.advancedUpgrades, newAdv, __instance.advancedUpgrades.Length);
                Array.Copy(__instance.advancedUnlockRound, newAdvUnlock, __instance.advancedUnlockRound.Length);
                __instance.advancedUpgrades = newAdv;
                __instance.advancedUnlockRound = newAdvUnlock;
            }
            if (CustomCore.CustomUltimateBuffs.Count > 0)
            {
                int[] newUlti = new int[__instance.ultimateUpgrades.Count + CustomCore.CustomUltimateBuffs.Count];
                Array.Copy(__instance.ultimateUpgrades, newUlti, __instance.ultimateUpgrades.Length);
                __instance.ultimateUpgrades = newUlti;
            }
            if (CustomCore.CustomDebuffs.Count > 0)
            {
                bool[] newdeb = new bool[__instance.debuff.Count + CustomCore.CustomDebuffs.Count];
                Array.Copy(__instance.debuff, newdeb, __instance.debuff.Length);
                __instance.debuff = newdeb;
            }
        }

        [HarmonyPatch("GetAdvancedBuffPool")]
        [HarmonyPostfix]
        public static void PostGetAdvancedBuffPool(ref Il2CppSystem.Collections.Generic.List<int> __result)
        {
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                if (CustomCore.CustomAdvancedBuffs.ContainsKey(__result[i]) && !CustomCore.CustomAdvancedBuffs[__result[i]].Item3())
                {
                    __result.Remove(__result[i]);
                }
            }
        }

        [HarmonyPatch(nameof(TravelMgr.GetAdvancedText))]
        [HarmonyPostfix]
        public static void PostGetAdvancedText(ref int index, ref string __result)
        {
            if (CustomCore.CustomAdvancedBuffs.ContainsKey(index) && CustomCore.CustomAdvancedBuffs[index].Item5 is not null)
            {
                __result = $"<color={CustomCore.CustomAdvancedBuffs[index].Item5}>{__result}</color>";
            }
        }

        [HarmonyPatch(nameof(TravelMgr.GetPlantTypeByAdvBuff))]
        [HarmonyPostfix]
        public static void PostGetPlantTypeByAdvBuff(ref int index, ref PlantType __result)
        {
            if (CustomCore.CustomAdvancedBuffs.ContainsKey(index) && CustomCore.CustomAdvancedBuffs[index].Item1 is not PlantType.Nothing)
            {
                __result = CustomCore.CustomAdvancedBuffs[index].Item1;
            }
        }
    }

    [HarmonyPatch(typeof(TravelStore))]
    public static class TravelStorePatch
    {
        [HarmonyPatch("RefreshBuff")]
        [HarmonyPostfix]
        public static void PostRefreshBuff(TravelStore __instance)
        {
            foreach (var travelBuff in __instance.gameObject.GetComponentsInChildren<TravelBuff>())
            {
                if (travelBuff.theBuffType is (int)BuffType.AdvancedBuff && CustomCore.CustomAdvancedBuffs.ContainsKey(travelBuff.theBuffNumber))
                {
                    travelBuff.cost = CustomCore.CustomAdvancedBuffs[travelBuff.theBuffNumber].Item4;
                    travelBuff.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"￥{CustomCore.CustomAdvancedBuffs[travelBuff.theBuffNumber].Item4}";
                }
                if (travelBuff.theBuffType is (int)BuffType.UltimateBuff && CustomCore.CustomUltimateBuffs.ContainsKey(travelBuff.theBuffNumber))
                {
                    travelBuff.cost = CustomCore.CustomUltimateBuffs[travelBuff.theBuffNumber].Item3;
                    travelBuff.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"￥{CustomCore.CustomUltimateBuffs[travelBuff.theBuffNumber].Item4}";
                }
            }
        }
    }

    [HarmonyPatch(typeof(TypeMgr))]
    public static class TypeMgrPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("BigNut")]
        public static bool PreBigNut(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.BigNut.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.BigNut.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("BigZombie")]
        public static bool PreBigZombie(ref ZombieType theZombieType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.BigZombie.Contains(theZombieType))
            {
                __result = true;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("DoubleBoxPlants")]
        public static bool PreDoubleBoxPlants(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.DoubleBoxPlants.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.DoubleBoxPlants.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("EliteZombie")]
        public static bool PreEliteZombie(ref ZombieType theZombieType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.EliteZombie.Contains(theZombieType))
            {
                __result = true;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("FlyingPlants")]
        public static bool PreFlyingPlants(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.FlyingPlants.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.FlyingPlants.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetPlantTag")]
        public static bool PreGetPlantTag(ref Plant plant)
        {
            if (CustomCore.CustomPlantTypes.Contains(plant.thePlantType))
            {
                plant.plantTag = new()
                {
                    icePlant = TypeMgr.IsIcePlant(plant.thePlantType),
                    caltropPlant = TypeMgr.IsCaltrop(plant.thePlantType),
                    doubleBoxPlant = TypeMgr.DoubleBoxPlants(plant.thePlantType),
                    firePlant = TypeMgr.IsFirePlant(plant.thePlantType),
                    flyingPlant = TypeMgr.FlyingPlants(plant.thePlantType),
                    lanternPlant = TypeMgr.IsPlantern(plant.thePlantType),
                    smallLanternPlant = TypeMgr.IsSmallRangeLantern(plant.thePlantType),
                    magnetPlant = TypeMgr.IsMagnetPlants(plant.thePlantType),
                    nutPlant = TypeMgr.IsNut(plant.thePlantType),
                    tallNutPlant = TypeMgr.IsTallNut(plant.thePlantType),
                    potatoPlant = TypeMgr.IsPotatoMine(plant.thePlantType),
                    potPlant = TypeMgr.IsPot(plant.thePlantType),
                    puffPlant = TypeMgr.IsPuff(plant.thePlantType),
                    pumpkinPlant = TypeMgr.IsPumpkin(plant.thePlantType),
                    spickRockPlant = TypeMgr.IsSpickRock(plant.thePlantType),
                    tanglekelpPlant = TypeMgr.IsTangkelp(plant.thePlantType),
                    waterPlant = TypeMgr.IsWaterPlant(plant.thePlantType),
                };

                return false;
            }

            if (CustomCore.CustomPlantsSkin.ContainsKey(plant.thePlantType))
            {
                plant.plantTag = new()
                {
                    icePlant = TypeMgr.IsIcePlant(plant.thePlantType),
                    caltropPlant = TypeMgr.IsCaltrop(plant.thePlantType),
                    doubleBoxPlant = TypeMgr.DoubleBoxPlants(plant.thePlantType),
                    firePlant = TypeMgr.IsFirePlant(plant.thePlantType),
                    flyingPlant = TypeMgr.FlyingPlants(plant.thePlantType),
                    lanternPlant = TypeMgr.IsPlantern(plant.thePlantType),
                    smallLanternPlant = TypeMgr.IsSmallRangeLantern(plant.thePlantType),
                    magnetPlant = TypeMgr.IsMagnetPlants(plant.thePlantType),
                    nutPlant = TypeMgr.IsNut(plant.thePlantType),
                    tallNutPlant = TypeMgr.IsTallNut(plant.thePlantType),
                    potatoPlant = TypeMgr.IsPotatoMine(plant.thePlantType),
                    potPlant = TypeMgr.IsPot(plant.thePlantType),
                    puffPlant = TypeMgr.IsPuff(plant.thePlantType),
                    pumpkinPlant = TypeMgr.IsPumpkin(plant.thePlantType),
                    spickRockPlant = TypeMgr.IsSpickRock(plant.thePlantType),
                    tanglekelpPlant = TypeMgr.IsTangkelp(plant.thePlantType),
                    waterPlant = TypeMgr.IsWaterPlant(plant.thePlantType)
                };

                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsCaltrop")]
        public static bool PreIsCaltrop(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsCaltrop.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsCaltrop.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsFirePlant")]
        public static bool PreIsFirePlant(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsFirePlant.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsFirePlant.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsIcePlant")]
        public static bool PreIsIcePlant(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsIcePlant.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsIcePlant.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsMagnetPlants")]
        public static bool PreIsMagnetPlants(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsMagnetPlants.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsMagnetPlants.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsNut")]
        public static bool PreIsNut(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsNut.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsNut.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPlantern")]
        public static bool PreIsPlantern(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPlantern.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPlantern.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPot")]
        public static bool PreIsPot(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPot.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPot.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPotatoMine")]
        public static bool PreIsPotatoMine(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPotatoMine.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPotatoMine.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPuff")]
        public static bool PreIsPuff(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPuff.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPuff.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPumpkin")]
        public static bool PreIsPumpkin(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPumpkin.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPumpkin.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsSmallRangeLantern")]
        public static bool PreIsSmallRangeLantern(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsSmallRangeLantern.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsSmallRangeLantern.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsSpecialPlant")]
        public static bool PreIsSpecialPlant(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsSpecialPlant.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsSpecialPlant.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsSpickRock")]
        public static bool PreIsSpickRock(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsSpickRock.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsSpickRock.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsTallNut")]
        public static bool PreIsTallNut(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsTallNut.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsTallNut.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsTangkelp")]
        public static bool PreIsTangkelp(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsTangkelp.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsTangkelp.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsWaterPlant")]
        public static bool PreIsWaterPlant(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsWaterPlant.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsWaterPlant.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UmbrellaPlants")]
        public static bool PreUmbrellaPlants(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.UmbrellaPlants.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.UmbrellaPlants.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(UIMgr))]
    public static class UIMgrPatch
    {
        [HarmonyPatch(nameof(UIMgr.EnterChallengeMenu))]
        [HarmonyPostfix]
        public static void PostEnterChallengeMenu()
        {
            var levels = GameAPP.canvas.GetChild(0).FindChild("Levels");
            var firstBtns = levels.FindChild("FirstBtns");
            if (firstBtns.FindChild("CustomLevels") is null || firstBtns.FindChild("CustomLevels").IsDestroyed())
            {
                GameObject custom = UnityEngine.Object.Instantiate(firstBtns.GetChild(0).gameObject, firstBtns);
                custom.name = "CustomLevels";
                custom.transform.localPosition = new(-150, 30, 0);
                var window = custom.transform.FindChild("Window");
                window.FindChild("Name").GetComponent<TextMeshProUGUI>().text = "二创关卡";
                var adv = levels.FindChild("PageAdvantureLevel");
                var customLevels = UnityEngine.Object.Instantiate(adv.gameObject, levels);
                customLevels.active = false;
                customLevels.name = "PageCustomLevel";
                var pages = customLevels.transform.FindChild("Pages");
                var levelSample = UnityEngine.Object.Instantiate(pages.FindChild("Page1").FindChild("Lv1").gameObject);
                foreach (var l in pages.FindChild("Page1").GetComponentsInChildren<Transform>(true))
                {
                    UnityEngine.Object.Destroy(l.gameObject);
                }
                var pageSample = UnityEngine.Object.Instantiate(pages.FindChild("Page1").gameObject);
                UnityEngine.Object.Destroy(pages.FindChild("Page1").gameObject);
                UnityEngine.Object.Destroy(pages.FindChild("Page2").gameObject);
                UnityEngine.Object.Destroy(pages.FindChild("Page3").gameObject);
                int levelIndex = 0;
                int columnIndex = 0;
                int rowIndex = 0;
                int pageIndex = 0;
                foreach (var level in CustomCore.CustomLevels)
                {
                    if (levelIndex % 18 is 0)
                    {
                        UnityEngine.Object.Instantiate(pageSample, pages).name = $"Pages{levelIndex / 18 + 1}";
                    }
                    columnIndex = levelIndex % 6;
                    rowIndex = levelIndex / 6;
                    pageIndex = rowIndex / 3;
                    var levelBtn = UnityEngine.Object.Instantiate(levelSample, pages.FindChild($"Pages{levelIndex / 18 + 1}"));
                    levelBtn.transform.localPosition = new(-50 + 150 * columnIndex, 60 - 130 * rowIndex, 0);
                    levelBtn.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = level.Logo;
                    levelBtn.transform.GetChild(1).GetComponent<Advanture_Btn>().levelType = (LevelType)66;
                    levelBtn.transform.GetChild(1).GetComponent<Advanture_Btn>().buttonNumber = level.ID;
                    levelBtn.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = level.Name();
                    levelIndex++;
                }
                window.GetComponent<FirstBtns>().pageToOpen = customLevels;
                window.GetComponent<FirstBtns>().originPosition = new(-150, 30, 0);
                UnityEngine.Object.Destroy(pageSample);
                UnityEngine.Object.Destroy(levelSample);
            }
        }

        [HarmonyPatch(nameof(UIMgr.EnterGame))]
        [HarmonyPrefix]
        public static bool PreEnterGame(ref int levelType, ref int levelNumber, ref int id, ref string name)
        {
            if (levelType is not 66) return true;
            var levelData = CustomCore.CustomLevels[levelNumber];

            // 清理UI资源
            GameAPP.UIManager.PopAll();

            // 重置相机
            CamaraFollowMouse.Instance.ResetCamera();

            // 设置游戏速度
            Time.timeScale = GameAPP.gameSpeed;

            // 设置当前关卡信息
            GameAPP.theBoardType = (LevelType)levelType;
            GameAPP.theBoardLevel = levelNumber;

            // 清理现有的Travel管理器
            if (TravelMgr.Instance != null)
            {
                UnityEngine.Object.Destroy(TravelMgr.Instance);
                TravelMgr.Instance = null;
            }

            // 创建游戏板
            GameObject boardGO = new("Board");
            GameAPP.board = boardGO;
            Board board = boardGO.AddComponent<Board>();
            board.boardTag = levelData.BoardTag;
            board.rowNum = levelData.RowCount;
            board.theMaxWave = levelData.WaveCount();
            board.cardSelectable = levelData.NeedSelectCard;
            board.theSun = levelData.Sun();
            board.zombieDamageAdder = levelData.ZombieHealthRate();
            board.seedPool = levelData.SeedRainPlantTypes().ToIl2CppList();
            levelData.PostBoard(board);
            // 获取场景类型和地图路径
            string mapPath = MapData_cs.GetMapPath(levelData.SceneType);

            // 加载并实例化地图
            GameObject mapInstance = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(mapPath), boardGO.transform);
            board.ChangeMap(mapInstance);

            InitZombieList.InitZombie((LevelType)levelType, levelNumber);

            // 播放音乐并开始游戏
            GameAPP.Instance.PlayMusic(MusicType.SelectCard);
            GameAPP.theGameStatus = GameStatus.InInterlude;

            // 初始化游戏板
            levelData.PreInitBoard();

            levelData.PostInitBoard(board.gameObject.AddComponent<InitBoard>());
            foreach (var p in levelData.PrePlants())
            {
                CreatePlant.Instance.SetPlant(p.Item1, p.Item2, p.Item3);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ZombieData))]
    public static class ZombieDataPatch
    {
        [HarmonyPatch("InitZombieData")]
        [HarmonyPostfix]
        public static void InitZombieData()
        {
            foreach (var z in CustomCore.CustomZombies)
            {
                ZombieData.zombieData[(int)z.Key] = z.Value.Item3;
            }
        }
    }
}