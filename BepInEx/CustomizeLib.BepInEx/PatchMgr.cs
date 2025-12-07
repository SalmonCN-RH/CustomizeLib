// #define DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF // 启用多级词条

using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Reflection;
using System.Text;
using System.Text.Json;
using TMPro;
using Unity.Collections;
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
    /// 注册融合洋芋配方
    /// </summary>
    [HarmonyPatch(typeof(MixBomb), nameof(MixBomb.AttributeEvent))]
    public class MixBombPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MixBomb __instance)
        {
            bool success = false;
            if (__instance != null)
            {
                List<Plant> plants = Lawnf.Get1x1Plants(__instance.thePlantColumn, __instance.thePlantRow).ToArray().ToList();
                if (plants is null)
                    return true;
                foreach (Plant plant in plants)
                {
                    if (plant != null && CustomCore.CustomMixBombFusions.Keys.Any(k => k.Item2 == plant.thePlantType))
                    {
                        List<(PlantType, PlantType, PlantType)> mixBombFusions = CustomCore.CustomMixBombFusions
                            .Where(kvp => kvp.Key.Item2 == plant.thePlantType)
                            .Select(kvp => kvp.Key)
                            .ToList();
                        List<Plant> leftPlant = Lawnf.Get1x1Plants(__instance.thePlantColumn - 1, __instance.thePlantRow).ToArray().ToList();
                        List<Plant> rightPlant = Lawnf.Get1x1Plants(__instance.thePlantColumn + 1, __instance.thePlantRow).ToArray().ToList();
                        foreach ((PlantType, PlantType, PlantType) fusion in mixBombFusions)
                        {
                            Plant? firstLeftPlant = leftPlant.FirstOrDefault(p => p.thePlantType == fusion.Item1);
                            Plant? firstRightPlant = rightPlant.FirstOrDefault(p => p.thePlantType == fusion.Item3);
                            if (firstLeftPlant == null || firstRightPlant == null)
                            {
                                CustomCore.CustomMixBombFusions[fusion].Item2[UnityEngine.Random.Range(0, CustomCore.CustomMixBombFusions[fusion].Item2.Count)](firstLeftPlant, plant, firstRightPlant);
                                continue;
                            }
                            if (leftPlant.Any(p => p.thePlantType == fusion.Item1) && rightPlant.Any(p => p.thePlantType == fusion.Item3))
                            {
                                CustomCore.CustomMixBombFusions[fusion].Item1[UnityEngine.Random.Range(0, CustomCore.CustomMixBombFusions[fusion].Item1.Count)](firstLeftPlant, plant, firstRightPlant);
                                success = true;
                            }
                            else
                            {
                                CustomCore.CustomMixBombFusions[fusion].Item2[UnityEngine.Random.Range(0, CustomCore.CustomMixBombFusions[fusion].Item2.Count)](firstLeftPlant, plant, firstRightPlant);
                            }
                        }
                    }
                }
            }
            if (__instance != null && success)
                __instance.Die();
            if (success)
                return false;
            return true;
        }
    }

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
            __instance.transform.FindChild("AlmanacPlant2").FindChild("Cards").GetComponent<GridManager>().maxY = GameAPP.resourcesManager.allPlants.Count / 9 * 1.5f;
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
            {
                PlantType plantType = (PlantType)__instance.theSeedType;
                if (CustomCore.CustomPlantsSkinActive.ContainsKey(plantType) && CustomCore.CustomPlantsSkinActive[plantType]) goto DIR_SEARCH;

                if (CustomCore.CustomPlantTypes.Contains(plantType))
                    __instance.skinButton.SetActive(CustomCore.CustomPlantsSkin.ContainsKey(plantType));

                if (!CustomCore.CustomPlantsSkin.TryGetValue(plantType, out var data)) goto DIR_SEARCH;
                if (!GameAPP.resourcesManager.plantSkinDic.TryGetValue((PlantType)__instance.theSeedType, out var _))
                    GameAPP.resourcesManager.plantSkinDic.Add(plantType, 0);

                var prefab = data.Prefab;
                var preview = data.Preview;

                if (prefab != null)
                {
                    if (GameAPP.resourcesManager._plantPrefabs.ContainsKey(plantType))
                        GameAPP.resourcesManager._plantPrefabs[(PlantType)__instance.theSeedType].Add(prefab);
                    else
                    {
                        Il2CppSystem.Collections.Generic.List<GameObject> list = new();
                        list.Add(GameAPP.resourcesManager.plantPrefabs[plantType]);
                        list.Add(prefab);
                        GameAPP.resourcesManager._plantPrefabs.Add(plantType, list);
                    }
                }
                if (preview != null)
                {
                    if (GameAPP.resourcesManager._plantPreviews.ContainsKey(plantType))
                        GameAPP.resourcesManager._plantPreviews[(PlantType)__instance.theSeedType].Add(preview);
                    else
                    {
                        Il2CppSystem.Collections.Generic.List<GameObject> list = new();
                        list.Add(GameAPP.resourcesManager.plantPreviews[plantType]);
                        list.Add(preview);
                        GameAPP.resourcesManager._plantPreviews.Add(plantType, list);
                    }
                }
                CustomCore.CustomPlantsSkinActive[plantType] = true;
            }
        DIR_SEARCH:
            {
                PlantType plantType = (PlantType)__instance.theSeedType;
                if (CustomCore.CustomPlantsSkinActive.ContainsKey(plantType) && CustomCore.CustomPlantsSkinActive[plantType]) return;
                String fullName = Directory.GetParent(Application.dataPath)?.FullName;
                if (fullName == null)
                    return;
                string skinPath = Path.Combine(fullName, "BepInEx", "plugins", "Skin");
                if (!Directory.Exists(skinPath))
                    return;

                String[] files = Directory.GetFiles(skinPath, $"skin_{__instance.theSeedType}");
                foreach (var item in files)
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(item);
                    GameObject? prefab = null;
                    try
                    {
                        prefab = ab.GetAsset<GameObject>("Prefab");
                        prefab.tag = "Plant";
                    }
                    catch
                    {
                        return;
                    }
                    GameObject? preview = null;
                    try
                    {
                        preview = ab.GetAsset<GameObject>("Preview");
                        preview.tag = "Preview";
                    }
                    catch
                    {
                        return;
                    }

                    CustomPlantData newCustomPlantData = new()
                    {
                        ID = (int)plantType,
                        PlantData = PlantDataLoader.plantDatas[plantType],
                        Prefab = GameAPP.resourcesManager.plantPrefabs[plantType],
                        Preview = GameAPP.resourcesManager.plantPreviews[plantType]
                    };

                    if (!GameAPP.resourcesManager.plantSkinDic.TryGetValue((PlantType)__instance.theSeedType, out var _))
                        GameAPP.resourcesManager.plantSkinDic.Add(plantType, 0);

                    if (prefab != null)
                    {
                        GameObject oldPrefab = GameAPP.resourcesManager.plantPrefabs[plantType];
                        var components = oldPrefab.GetComponents<Component>();
                        // 复制旧预制体上的组件
                        foreach (var component in components)
                        {
                            if (!prefab.TryGetComponent(component.GetIl2CppType(), out var comp) && comp == null)
                                prefab.AddComponent(component.GetIl2CppType());
                        }
                        // 赋值植物类型
                        prefab.GetComponent<Plant>().thePlantType = oldPrefab.GetComponent<Plant>().thePlantType;

                        if (GameAPP.resourcesManager._plantPrefabs.ContainsKey(plantType))
                            GameAPP.resourcesManager._plantPrefabs[plantType].Add(prefab);
                        else
                        {
                            Il2CppSystem.Collections.Generic.List<GameObject> list = new();
                            list.Add(GameAPP.resourcesManager.plantPrefabs[plantType]);
                            list.Add(prefab);
                            GameAPP.resourcesManager._plantPrefabs.Add(plantType, list);
                        }
                        prefab.GetComponent<Plant>().FindShoot(prefab.GetComponent<Plant>().transform);
                        newCustomPlantData.Prefab = prefab;
                    }
                    if (preview != null)
                    {
                        if (GameAPP.resourcesManager._plantPreviews.ContainsKey(plantType))
                            GameAPP.resourcesManager._plantPreviews[plantType].Add(preview);
                        else
                        {
                            Il2CppSystem.Collections.Generic.List<GameObject> list = new();
                            list.Add(GameAPP.resourcesManager.plantPreviews[plantType]);
                            list.Add(preview);
                            GameAPP.resourcesManager._plantPreviews.Add(plantType, list);
                        }

                        GameObject oldPreview = GameAPP.resourcesManager.plantPreviews[plantType];
                        var components = oldPreview.GetComponents<Component>();
                        // 复制旧预制体上的组件
                        foreach (var component in components)
                        {
                            if (!preview.TryGetComponent(component.GetIl2CppType(), out var comp) && comp == null)
                                preview.AddComponent(component.GetIl2CppType());
                        }
                        newCustomPlantData.Preview = preview;
                    }
                    /*Msg("bullet");
                    GameObject? bulletPrefab = null;
                    BulletType bulletType = (BulletType)(-1);
                    try
                    {
                        var strArray = ab.GetAssetsNames();
                        Regex regex = new(@"^BulletPrefab_(\d+)$");
                        foreach (var str in strArray)
                        {
                            Match match = regex.Match(str);
                            if (match.Success)
                                if (ab.GetAsset<GameObject>(str) != null && int.TryParse(match.Groups[1].Value, out var type))
                                {
                                    bulletPrefab = ab.GetAsset<GameObject>(str);
                                    bulletType = (BulletType)type;
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg(e);
                    }
                    if (bulletPrefab != null)
                    {
                        foreach (var component in GameAPP.resourcesManager.bulletPrefabs[bulletType].GetComponents<Component>())
                            if (!bulletPrefab.TryGetComponent(component.GetIl2CppType(), out var comp) && comp == null)
                                bulletPrefab.AddComponent(component.GetIl2CppType());
                        CustomCore.RegisterCustomBulletSkin(bulletPrefab, plantType, bulletType);
                    }*/
                    CustomCore.CustomPlantsSkin.Add(plantType, newCustomPlantData);
                    __instance.skinButton.SetActive(true);

                    //加载图鉴
                    try
                    {
                        CustomCore.PlantsSkinAlmanac.Add(plantType, null);//无json则不换图鉴内容
                    }
                    catch (Exception e)
                    {
                        CustomCore.CLogger.LogInfo(e);
                    }
                }
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

    /// <summary>
    /// 为二创植物附加植物特性
    /// </summary>
    [HarmonyPatch(typeof(CreatePlant))]
    public static class CreatePlantPatch
    {
        [HarmonyPatch(nameof(CreatePlant.SetPlant))]
        [HarmonyPostfix]
        public static void Postfix_SetPlant(CreatePlant __instance, ref int newColumn, ref int newRow, ref GameObject __result)
        {
            if (__result != null && __result.TryGetComponent<Plant>(out var plant) &&
                CustomCore.CustomPlantTypes.Contains(plant.thePlantType))
            {
                TypeMgr.GetPlantTag(plant);
            }
        }

        [HarmonyPatch(nameof(CreatePlant.LimTravel))]
        [HarmonyPostfix]
        public static void Postfix_LimTravel(CreatePlant __instance, ref PlantType theSeedType, ref bool __result)
        {
            bool isCanSet = false;
            if (TravelMgr.Instance != null && TravelMgr.Instance.ulockTemp.Contains(theSeedType))
                isCanSet = true;
            if (__instance.board.boardTag.enableAllTravelPlant || __instance.board.boardTag.enableTravelPlant || __instance.board.boardTag.isTravel)
                isCanSet = true;

            if (CustomCore.CustomUltimatePlants.Contains(theSeedType) && !isCanSet)
            {
                __result = true;
                InGameText.Instance.ShowText("该配方仅旅行生存系列或深渊可用", 3f, false);
            }
        }

        [HarmonyPatch(nameof(CreatePlant.MixBombCheck))]
        [HarmonyPrefix]
        public static bool Prefix_MixBombCheck(CreatePlant __instance, ref int theBoxColumn, ref int theBoxRow, ref bool __result)
        {
            List<Plant> plants = Lawnf.Get1x1Plants(theBoxColumn, theBoxRow).ToArray().ToList();
            foreach (var plant in plants)
            {
                if (plant == null) continue;
                if (CustomCore.CustomMixBombFusions.Any(kvp => kvp.Key.Item2 == plant.thePlantType))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Lawnf))]
    public class LawnfPatch
    {
        [HarmonyPatch(nameof(Lawnf.GetUpgradedPlantCost))]
        [HarmonyPrefix]
        public static bool Prefix(ref PlantType thePlantType, ref int targetLevel, ref int __result)
        {
            if (CustomCore.CustomUltimatePlants.Contains(thePlantType))
            {
                __result = 1500 * (targetLevel) * (targetLevel + 1) / 2;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Lawnf.IsUltiPlant))]
        [HarmonyPrefix]
        public static bool Prefix(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.CustomPlantTypes.Contains(thePlantType))
            {
                __result = CustomCore.CustomUltimatePlants.Contains(thePlantType);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Lawnf.GetUltimatePlants))]
        [HarmonyPostfix]
        public static void Postfix(ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            foreach (PlantType plantType in CustomCore.CustomUltimatePlants)
            {
                if (!__result.Contains(plantType))
                {
                    __result.Add(plantType);
                }
            }
        }

        [HarmonyPatch(nameof(Lawnf.TravelAdvanced), new Type[] { typeof(AdvBuff) })]
        [HarmonyPostfix]
        public static void PostTravelAdvanced_0(ref AdvBuff buff, ref bool __result)
        {
            var result = Utils.IsMultiLevelBuff(BuffType.AdvancedBuff, (int)buff);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            __result = array[index] > 0;
        }

        [HarmonyPatch(nameof(Lawnf.TravelAdvanced), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        public static void PostTravelAdvanced_1(ref int i, ref bool __result)
        {
            var result = Utils.IsMultiLevelBuff(BuffType.AdvancedBuff, i);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            __result = array[index] > 0;
        }

        [HarmonyPatch(nameof(Lawnf.TravelUltimate), new Type[] { typeof(UltiBuffs) })]
        [HarmonyPostfix]
        public static void PostTravelUltimate_0(ref UltiBuffs i, ref bool __result)
        {
            var result = Utils.IsMultiLevelBuff(BuffType.UltimateBuff, (int)i);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            __result = array[index] > 0;
        }

        [HarmonyPatch(nameof(Lawnf.TravelUltimate), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        public static void PostTravelUltimate_1(ref int i, ref bool __result)
        {
            var result = Utils.IsMultiLevelBuff(BuffType.UltimateBuff, i);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            __result = array[index] > 0;
        }

        [HarmonyPatch(nameof(Lawnf.TravelUltimateLevel))]
        [HarmonyPostfix]
        public static void PostTravelUltimateLevel(ref int index, ref int __result)
        {
            var result = Utils.IsMultiLevelBuff(BuffType.UltimateBuff, index);
            if (!result.Item1)
                return;
            int index2 = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            __result = array[index2];
        }

        [HarmonyPatch(nameof(Lawnf.TravelDebuff), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        public static void PostTravelDebuff_0(ref int i, ref bool __result)
        {
            var result = Utils.IsMultiLevelBuff(BuffType.Debuff, i);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            __result = array[index] > 0;
        }

        [HarmonyPatch(nameof(Lawnf.TravelDebuff), new Type[] { typeof(TravelDebuff) })]
        [HarmonyPostfix]
        public static void PostTravelDebuff_1(ref TravelDebuff travelDebuff, ref bool __result)
        {
            var result = Utils.IsMultiLevelBuff(BuffType.Debuff, (int)travelDebuff);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            __result = array[index] > 0;
        }
    }

    /// <summary>
    /// 点击其他Button，隐藏二创植物界面
    /// </summary>
    [HarmonyPatch(typeof(UIButton))]
    public static class HideCustomPlantCards
    {
        [HarmonyPatch(nameof(UIButton.OnMouseUpAsButton))]
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (SelectCustomPlants.MyPageParent != null && SelectCustomPlants.MyPageParent.active && GameAPP.theGameStatus != GameStatus.BigGarden)
                SelectCustomPlants.MyPageParent.SetActive(false);
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
        public static void PreMoveCard(ref CardUI card)
        {
            foreach (CheckCardState check in CustomCore.checkBehaviours)
            {
                if (check != null)
                {
                    check.movingCardUI = card;
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
                    check.movingCardUI = card;
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
                if (__instance.board != null)
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
                if (__instance.board == null) return false;

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
        [HarmonyPatch(nameof(InitZombieList.SetAllowZombieTypeSpawn))]
        [HarmonyPrefix]
        public static void PostInitZombie(ref LevelType theLevelType, ref int theLevelNumber)
        {
            if (Utils.IsCustomLevel(out var levelData))
            {
                Il2CppSystem.Collections.Generic.List<ZombieType> list = new();
                foreach (var z in levelData.ZombieList())
                    list.Add(z);
                InitZombieList.AllowZombies(list);
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
                if (__result.ToArray()[i] != null && TypeMgr.BigNut(__result.ToArray()[i].thePlantType))
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

    [HarmonyPatch(typeof(NoticeMenu), nameof(NoticeMenu.Start))]
    public static class NoticeMenuPatch
    {
        [HarmonyPrefix]
        public static void Postfix()
        {
            #region 自动扩容
            // 扩容plantData
            if (CustomCore.CustomPlants.Count > 0)
            {
                long size_plantData = (int)CustomCore.CustomPlants.Keys.Max() < PlantDataLoader.plantData.Length ? PlantDataLoader.plantData.Length : (int)CustomCore.CustomPlants.Keys.Max();
                PlantDataLoader.PlantData_[] plantData = new PlantDataLoader.PlantData_[size_plantData + 1];
                Array.Copy(PlantDataLoader.plantData, plantData, PlantDataLoader.plantData.Length);
                PlantDataLoader.plantData = plantData;
            }

            // 扩容particlePrefab
            if (CustomCore.CustomParticles.Count > 0)
            {
                long size_particlePrefab = (int)CustomCore.CustomParticles.Keys.Max() < GameAPP.particlePrefab.Length ? GameAPP.particlePrefab.Length : (int)CustomCore.CustomParticles.Keys.Max();
                GameObject[] particlePrefab = new GameObject[size_particlePrefab + 1];
                Array.Copy(GameAPP.particlePrefab, particlePrefab, GameAPP.particlePrefab.Length);
                GameAPP.particlePrefab = particlePrefab;
            }

            // 扩容spritePrefab
            if (CustomCore.CustomSprites.Count > 0)
            {
                long size_spritePrefab = CustomCore.CustomSprites.Keys.Max() < GameAPP.spritePrefab.Length ? GameAPP.spritePrefab.Length : CustomCore.CustomSprites.Keys.Max();
                Sprite[] spritePrefab = new Sprite[size_spritePrefab + 1];
                Array.Copy(GameAPP.spritePrefab, spritePrefab, GameAPP.spritePrefab.Length);
                GameAPP.spritePrefab = spritePrefab;
            }

            // 扩容data融合数组
            if (CustomCore.CustomPlants.Count > 0)
            {
                var arr = MixData.data.Cast<Il2CppSystem.Array>();
                long max = (int)CustomCore.CustomPlants.Keys.Max() + 1;
                var length_0 = arr.GetLength(0) < max ? max : arr.GetLength(0);
                var length_1 = arr.GetLength(1) < max ? max : arr.GetLength(1);
                var length = length_0 < length_1 ? length_1 : length_0;
                var type = arr.GetValue(0, 0).GetIl2CppType();
                var result = Il2CppSystem.Array.CreateInstance(type, length, length);
                Il2CppSystem.Array.Copy(arr, result, arr.Length);
                MixData.data = result;
            }

            // 扩容disMixDatas拆分数组
            if (CustomCore.CustomPlants.Count > 0)
            {
                long size_disMixDatas = (int)CustomCore.CustomPlants.Keys.Max() < MixData.disMixDatas.Length ? MixData.disMixDatas.Length : (int)CustomCore.CustomPlants.Keys.Max();
                MixData.DisMixData[] disMixDatas = new MixData.DisMixData[size_disMixDatas + 1];
                Array.Copy(MixData.disMixDatas, disMixDatas, MixData.disMixDatas.Length);
                MixData.disMixDatas = disMixDatas;
            }

            // 扩容randomData随机融合数组
            if (CustomCore.CustomPlants.Count > 0)
            {
                var arr = MixData.randomData.Cast<Il2CppSystem.Array>();
                long max = (int)CustomCore.CustomPlants.Keys.Max() + 1;
                var length_0 = arr.GetLength(0) < max ? max : arr.GetLength(0);
                var length_1 = arr.GetLength(1) < max ? max : arr.GetLength(1);
                var length = length_0 < length_1 ? length_1 : length_0;
                var type = arr.GetValue(0, 0).GetIl2CppType();
                var result = Il2CppSystem.Array.CreateInstance(type, length, length);
                Il2CppSystem.Array.Copy(arr, result, arr.Length);
                MixData.randomData = result;
            }
            #endregion

            foreach (var plant in CustomCore.CustomPlants)//二创植物
            {
                GameAPP.resourcesManager.plantPrefabs[plant.Key] = plant.Value.Prefab;//注册预制体
                GameAPP.resourcesManager.plantPrefabs[plant.Key].tag = "Plant";//必须打tag
                if (!GameAPP.resourcesManager.allPlants.Contains(plant.Key))
                    GameAPP.resourcesManager.allPlants.Add(plant.Key);//注册植物类型
                if (plant.Value.PlantData is not null)
                {
                    PlantDataLoader.plantData[(int)plant.Key] = plant.Value.PlantData;//注册植物数据
                    PlantDataLoader.plantDatas.Add(plant.Key, plant.Value.PlantData);
                }
                GameAPP.resourcesManager.plantPreviews[plant.Key] = plant.Value.Preview;//注册植物预览
                GameAPP.resourcesManager.plantPreviews[plant.Key].tag = "Preview";//必修打tag
            }
            Il2CppSystem.Array array = MixData.data.Cast<Il2CppSystem.Array>();//注册融合配方
            foreach (var f in CustomCore.CustomFusions)
            {
                array.SetValue(f.Item1, f.Item2, f.Item3);
            }

            foreach (var z in CustomCore.CustomZombies)//注册二创僵尸
            {
                if (!GameAPP.resourcesManager.allZombieTypes.Contains(z.Key))
                    GameAPP.resourcesManager.allZombieTypes.Add(z.Key);//注册僵尸类型
                GameAPP.resourcesManager.zombiePrefabs[z.Key] = z.Value.Item1;//注册僵尸预制体
                GameAPP.resourcesManager.zombiePrefabs[z.Key].tag = "Zombie";//必修打tag
            }

            foreach (var bullet in CustomCore.CustomBullets)//注册二创子弹
            {
                GameAPP.resourcesManager.bulletPrefabs[bullet.Key] = bullet.Value;//注册子弹预制体
                if (!GameAPP.resourcesManager.allBullets.Contains(bullet.Key))
                    GameAPP.resourcesManager.allBullets.Add(bullet.Key);//注册子弹类型
            }

            foreach (var par in CustomCore.CustomParticles)//注册粒子效果
            {
                GameAPP.particlePrefab[(int)par.Key] = par.Value;
                GameAPP.resourcesManager.particlePrefabs[par.Key] = par.Value;//注册粒子效果预制体
                if (!GameAPP.resourcesManager.allParticles.Contains(par.Key))
                    GameAPP.resourcesManager.allParticles.Add(par.Key);//注册粒子效果类型
            }

            foreach (var spr in CustomCore.CustomSprites)//注册自定义精灵贴图
            {
                GameAPP.spritePrefab[spr.Key] = spr.Value;
            }

            // 注册红卡
            {
                var propertyInfo = typeof(TypeMgr).GetProperty("RedPlant", BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo is null)
                    goto Lable1;
                var value = propertyInfo.GetValue(null);
                if (value is null)
                    goto Lable1;
                var redPlant = (Il2CppSystem.Collections.Generic.HashSet<PlantType>)value;
                foreach (var (k, v) in CustomCore.TypeMgrExtra.LevelPlants)
                    if (v == CardLevel.Red)
                        redPlant.Add(k);
                propertyInfo.SetValue(null, redPlant);
            }
        Lable1:
            // 注册防碾压植物
            {
                var propertyInfo = typeof(TypeMgr).GetProperty("UncrashablePlants", BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo is null)
                    return;
                var value = propertyInfo.GetValue(null);
                if (value is null)
                    return;
                var uncrashablePlants = (Il2CppSystem.Collections.Generic.HashSet<PlantType>)value;
                foreach (var item in CustomCore.TypeMgrExtra.UncrashablePlants)
                    uncrashablePlants.Add(item);
                propertyInfo.SetValue(null, uncrashablePlants);
            }

        END_BLOCK:
            return;
        }
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
        [HarmonyPatch(nameof(SeedLibrary.Start))]
        [HarmonyPostfix]
        public static void PostStart(SeedLibrary __instance)
        {
            // 注册自定义卡牌
            GameObject? MyColorfulCard = Utils.GetColorfulCardGameObject();
            Dictionary<PlantType, List<Transform?>> parents_colorful = new Dictionary<PlantType, List<Transform?>>();
            List<PlantType> cardsOnSeedBank = new List<PlantType>();
            Dictionary<PlantType, List<bool>> cardsOnSeedBankExtra = new Dictionary<PlantType, List<bool>>();
            GameObject? seedGroup = null;
            if (Board.Instance != null && !Board.Instance.isIZ)
                seedGroup = InGameUI.Instance.SeedBank.transform.GetChild(0).gameObject;
            else if (Board.Instance != null && Board.Instance.isIZ)
                seedGroup = InGameUI_IZ.Instance.transform.FindChild("SeedBank/SeedGroup").gameObject;
            if (seedGroup == null)
                return;
            for (int i = 0; i < seedGroup.transform.childCount; i++)
            {
                GameObject seed = seedGroup.transform.GetChild(i).gameObject;
                if (seed.transform.childCount > 0)
                {
                    cardsOnSeedBank.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType);
                    if (!cardsOnSeedBankExtra.ContainsKey(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType))
                        cardsOnSeedBankExtra.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType, new List<bool>() { seed.transform.GetChild(0).GetComponent<CardUI>().isExtra });
                    else
                        cardsOnSeedBankExtra[seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType].Add(seed.transform.GetChild(0).GetComponent<CardUI>().isExtra);
                }
            }
            if (MyColorfulCard == null)
                return;
            foreach (var card in CustomCore.CustomCards)
            {
                foreach (Func<Transform?> cardFunc in card.Value)
                {
                    Transform? result = cardFunc();
                    if (!(parents_colorful.ContainsKey(card.Key) && parents_colorful[card.Key].Contains(result)))
                    {
                        GameObject TempCard = Instantiate(MyColorfulCard, result);
                        if (TempCard != null)
                        {
                            //设置父节点
                            //激活
                            TempCard.SetActive(true);
                            //设置位置
                            TempCard.transform.position = MyColorfulCard.transform.position;
                            TempCard.transform.localPosition = MyColorfulCard.transform.localPosition;
                            TempCard.transform.localScale = MyColorfulCard.transform.localScale;
                            TempCard.transform.localRotation = MyColorfulCard.transform.localRotation;
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
                            TempCard.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
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
                            if (!parents_colorful.ContainsKey(card.Key))
                                parents_colorful.Add(card.Key, new List<Transform?>() { result });
                            else
                                parents_colorful[card.Key].Add(result);
                        }
                    }
                }
            }

            GameObject? MyNormalCard = Utils.GetNormalCardGameObject();
            Dictionary<PlantType, List<Transform?>> parents_normal = new Dictionary<PlantType, List<Transform?>>();
            if (MyNormalCard == null)
                return;
            foreach (var card in CustomCore.CustomNormalCards)
            {
                foreach (Func<Transform?> cardFunc in card.Value)
                {
                    Transform? result = cardFunc();
                    if (!(parents_normal.ContainsKey(card.Key) && parents_normal[card.Key].Contains(result)))
                    {
                        GameObject TempCard = Instantiate(MyNormalCard, result);
                        if (TempCard != null)
                        {
                            //设置父节点
                            //激活
                            TempCard.SetActive(true);
                            //设置位置
                            TempCard.transform.position = MyNormalCard.transform.position;
                            TempCard.transform.localPosition = MyNormalCard.transform.localPosition;
                            TempCard.transform.localScale = MyNormalCard.transform.localScale;
                            TempCard.transform.localRotation = MyNormalCard.transform.localRotation;
                            //背景图片
                            // 设置背景植物图标
                            Image image = TempCard.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[card.Key].GetComponent<SpriteRenderer>().sprite;
                            image.SetNativeSize();
                            // 设置背景价格
                            TempCard.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1.ToString();
                            //卡片
                            CardUI component = TempCard.transform.GetChild(2).GetComponent<CardUI>(); // 主卡
                            component.gameObject.SetActive(true);
                            CardUI component1 = TempCard.transform.GetChild(1).GetComponent<CardUI>(); // 副卡
                            component1.gameObject.SetActive(true);
                            //修改图片
                            Mouse.Instance.ChangeCardSprite(card.Key, component);
                            Mouse.Instance.ChangeCardSprite(card.Key, component1);
                            // 修改缩放
                            TempCard.transform.GetChild(2).GetComponent<BoxCollider2D>().enabled = true;
                            TempCard.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
                            RectTransform bgRect = TempCard.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                            RectTransform packetRect = TempCard.transform.GetChild(2).GetChild(0).GetComponent<RectTransform>();
                            bgRect.localScale = packetRect.localScale;
                            bgRect.sizeDelta = packetRect.sizeDelta;
                            //设置数据
                            component.thePlantType = card.Key;
                            component.theSeedType = (int)card.Key;
                            component.theSeedCost = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1;
                            component.fullCD = PlantDataLoader.plantDatas[card.Key].field_Public_Single_2;
                            //设置副卡数据
                            component1.thePlantType = card.Key;
                            component1.theSeedType = (int)card.Key;
                            component1.theSeedCost = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1 * 2;
                            component1.fullCD = PlantDataLoader.plantDatas[card.Key].field_Public_Single_2;
                            if (cardsOnSeedBankExtra.ContainsKey(card.Key) && cardsOnSeedBankExtra[card.Key].Contains(true))
                                TempCard.transform.GetChild(1).gameObject.SetActive(false);
                            if (cardsOnSeedBankExtra.ContainsKey(card.Key) && cardsOnSeedBankExtra[card.Key].Contains(false))
                                TempCard.transform.GetChild(2).gameObject.SetActive(false);
                            CheckCardState customComponent = TempCard.AddComponent<CheckCardState>();
                            customComponent.card = TempCard;
                            customComponent.cardType = component.thePlantType;
                            customComponent.isNormalCard = true;
                            if (!parents_normal.ContainsKey(card.Key))
                                parents_normal.Add(card.Key, new List<Transform?>() { result });
                            else
                                parents_normal[card.Key].Add(result);
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 进入一局游戏，显示二创植物Button
    /// </summary>
    [HarmonyPatch(typeof(Board))]
    public static class Board_Patch
    {
        [HarmonyPatch(nameof(Board.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            SelectCustomPlants.InitCustomCards();
            if (TravelMgr.Instance == null)
                return;
            if (TravelMgr.Instance.GetData("LoadByEndless") is null)
                TravelMgr.Instance.SetData("LoadByEndless", false);
            if ((TravelMgr.Instance.GetData("CustomBuffsLevel") is null ||
                TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel").SequenceEqual(new int[CustomCore.CustomAdvancedBuffs.Count])) &&
                !TravelMgr.Instance.GetData<bool>("LoadByEndless"))
            {
                TravelMgr.Instance.SetData("CustomBuffsLevel", new int[CustomCore.CustomAdvancedBuffs.Count]);
            }
        }

        [HarmonyPatch(nameof(Board.Update))]
        [HarmonyPostfix]
        public static void PostUpdate()
        {
            if (TravelMgr.Instance == null)
                return;
            try
            {
                var array = (int[])TravelMgr.Instance.GetData("CustomBuffsLevel");
                if (array is null)
                    return;

                foreach (var (key, value) in CustomCore.CustomBuffsLevel)
                {
                    var result = Utils.IsMultiLevelBuff(key.Item1, key.Item2);
                    if (!result.Item1)
                        continue;
                    int index = result.Item2;
                    switch (key.Item1)
                    {
                        case BuffType.AdvancedBuff:
                            {
                                if (!TravelMgr.Instance.advancedUpgrades[key.Item2])
                                    array[index] = 0;
                                if (array[index] <= 0 && TravelMgr.Instance.advancedUpgrades[key.Item2])
                                    array[index] = 1;
                            }
                            break;
                        case BuffType.UltimateBuff:
                            {
                                if (TravelMgr.Instance.ultimateUpgrades[key.Item2] <= 0)
                                    array[index] = 0;
                                if (array[index] <= 0 && TravelMgr.Instance.ultimateUpgrades[key.Item2] >= 1)
                                    array[index] = TravelMgr.Instance.ultimateUpgrades[key.Item2];
                            }
                            break;
                        case BuffType.Debuff:
                            {
                                if (!TravelMgr.Instance.debuff[key.Item2])
                                    array[index] = 0;
                                if (array[index] <= 0 && TravelMgr.Instance.debuff[key.Item2])
                                    array[index] = 1;
                            }
                            break;
                    }
                }
            }
            catch (ArgumentException) { }
        }
    }

#if DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF
    #region 多级词条同步
    [HarmonyPatch(typeof(RandomZombie))]
    public static class RandomZombie_Patch
    {
        [HarmonyPatch(nameof(RandomZombie.FirstArmorFall))]
        [HarmonyPostfix]
        public static void Postfix()
        {
            // 普通词条对应升级
            for (int i = (int)CustomCore.variables[0]; i < TravelMgr.advancedBuffs.Count; i++)
            {
                var result = Utils.IsMultiLevelBuff(BuffType.AdvancedBuff, i);
                var index = CustomCore.CustomBuffsLevel.Where(kvp => kvp.Key.Item1 == BuffType.AdvancedBuff && kvp.Key.Item3 == i).Select(kvp => kvp.Key.Item2).ToList();
                foreach (var ii in index)
                    if (result.Item1 && TravelMgr.Instance.ultimateUpgrades[ii] == 0 && TravelMgr.Instance.advancedUpgrades[i])
                        foreach (var value in result.Item2)
                            TravelMgr.Instance.ultimateUpgrades[(int)CustomCore.variables[0] + value.Item2] = 1;
            }
            // Debuff词条对应升级
            for (int i = (int)CustomCore.variables[0]; i < TravelMgr.debuffs.Count; i++)
            {
                var result = Utils.IsMultiLevelBuff(BuffType.Debuff, i);
                var index = CustomCore.CustomBuffsLevel.Where(kvp => kvp.Key.Item1 == BuffType.Debuff && kvp.Key.Item3 == i).Select(kvp => kvp.Key.Item2).ToList();
                foreach (var ii in index)
                    if (result.Item1 && TravelMgr.Instance.ultimateUpgrades[i] == 0 && TravelMgr.Instance.debuff[i])
                        foreach (var value in result.Item2)
                            TravelMgr.Instance.ultimateUpgrades[(int)CustomCore.variables[0] + value.Item2] = 1;
            }
        }
    }
    #endregion
#endif
    
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
        
        /// <summary>
         /// 强究词条显示植物修复
         /// </summary>
        [HarmonyPatch(nameof(TravelBuffOptionButton.SetPlant), new Type[] { })]
        [HarmonyPrefix]
        public static bool PreSetPlant(TravelBuffOptionButton __instance)
        {
            var list = CustomCore.CustomUltimateBuffs.
                Where(kvp => kvp.Key == __instance.buffIndex).
                ToList();
            if (__instance.buffType == BuffType.UltimateBuff && list.Count > 0)
            {
                foreach (var value in list)
                {
                    if (value.Value.Item1 == PlantType.Nothing)
                        __instance.SetPlant(PlantType.EndoFlame);
                    else
                        __instance.SetPlant(value.Value.Item1);
                }
                return false;
            }
            return true;
        }
    }
#if DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF
    #region 多级词条修复数组
    [HarmonyPatch(typeof(TravelLookMenu))]
    public static class TravelLookMenuPatch
    {
        /// <summary>
        /// 修复数组，不然会多一个
        /// </summary>
        [HarmonyPatch(nameof(TravelLookMenu.GetUltiBuffs))]
        [HarmonyPostfix]
        public static void PostGetUltiBuffs(TravelLookMenu __instance, ref Il2CppSystem.Collections.Generic.List<Vector2Int> __result)
        {
            Il2CppSystem.Collections.Generic.List<Vector2Int> result = new Il2CppSystem.Collections.Generic.List<Vector2Int>();

            // 遍历升级数组
            for (int i = 0; i < __instance.manager.ultimateUpgrades.Length; i++)
            {
                if (CustomCore.CustomBuffsLevel.Count > 0 && CustomCore.CustomBuffsLevel.Any(kvp => ((int)CustomCore.variables[0] + kvp.Key.Item2) == i && kvp.Key.Item1 != BuffType.UltimateBuff))
                    continue;
                if (CustomCore.CustomBuffsLevel.Count > 0 && CustomCore.CustomBuffsLevel.Any(kvp => kvp.Key.Item1 == BuffType.UltimateBuff) && i == __instance.manager.ultimateUpgrades.Length - 1)
                    break;
                // 检查是否已解锁或显示所有
                if (__instance.manager.ultimateUpgrades[i] != 0 || __instance.showAll)
                {
                    // 添加索引和等级到结果列表
                    result.Add(new Vector2Int(i, __instance.manager.ultimateUpgrades[i]));
                }
            }
            __result = result;
        }
    }
    #endregion
#endif

    [HarmonyPatch(typeof(TravelBuff))]
    public static class TravelBuffPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ChangeSprite")]
        public static void PreChangeSprite(TravelBuff __instance)
        {
            var list = CustomCore.CustomUltimateBuffs.
                    Where(kvp => kvp.Key == __instance.theBuffNumber).
                    Select(kvp => kvp.Value).
                    ToList();
            if (__instance.theBuffType == (int)BuffType.UltimateBuff && list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (item.Item1 == PlantType.Nothing)
                        __instance.thePlantType = PlantType.EndoFlame;
                    else
                        __instance.thePlantType = item.Item1;
                }
            }

            if (__instance.theBuffType == 1 && CustomCore.CustomAdvancedBuffs.ContainsKey(__instance.theBuffNumber))
            {
                __instance.thePlantType = CustomCore.CustomAdvancedBuffs[__instance.theBuffNumber].Item1;
            }
        }
    }

    /// <summary>
    /// 二创词条文本染色
    /// </summary>
    /// <summary>
    /// 二创词条文本染色
    /// </summary>
    [HarmonyPatch(typeof(TravelLookBuff))]
    public static class TravelLookBuffPatch
    {
        [HarmonyPatch(nameof(TravelLookBuff.SetBuff))]
        [HarmonyPostfix]
        public static void PostSetBuff(TravelLookBuff __instance, ref BuffType buffType, ref int buffIndex)
        {
            if (buffType is BuffType.AdvancedBuff && CustomCore.CustomAdvancedBuffs.ContainsKey(buffIndex)
                && CustomCore.CustomAdvancedBuffs[buffIndex].Item5 is not null)
            {
                __instance.introduce.text = $"<color={CustomCore.CustomAdvancedBuffs[buffIndex].Item5}>{__instance.introduce.text}</color>";
            }

            if (CustomCore.CustomBuffsBg.ContainsKey((buffType, buffIndex)))
                __instance.SetBackground(CustomCore.CustomBuffsBg[(buffType, buffIndex)]);

            var result = Utils.IsMultiLevelBuff(__instance.buffType, __instance.buffIndex);
            try
            {
                if (result.Item1)
                {
                    var array = (int[])TravelMgr.Instance.GetData("CustomBuffsLevel");
                    if (array is null)
                        return;
                    int index = result.Item2;
                    var list = CustomCore.CustomBuffsLevel.Where(kvp => kvp.Key.Item1 == __instance.buffType && kvp.Key.Item2 == __instance.buffIndex).ToList();
                    int maxLevel = 1;
                    if (list.Count > 0)
                        maxLevel = list[0].Value.Item2;
                    if (TravelLookMenu.Instance.showAll)
                    {
                        __instance.SetText(array[index] != 0, array[index]);
                        if (array[index] <= maxLevel &&
                            array[index] != 0)
                        {
                            if (maxLevel > 1)
                                __instance.SetText($"已开启（{array[index]}级）");
                            else
                                __instance.SetText($"已开启");
                        }
                        return;
                    }
                    else
                    {
                        if (array[index] < maxLevel && maxLevel != 1)
                        {
                            if (array[index] >= maxLevel)
                                __instance.SetText("已满级");
                            else
                                __instance.SetText($"{array[index]}级");
                        }
                        if (array[index] >= maxLevel)
                        {
                            __instance.SetText("已满级");
                        }
                        TravelMgr.Instance.SetData("CustomBuffsLevel", array);
                    }
                }
            }
            catch (ArgumentException)
            {
                CustomCore.CLogger.LogInfo("Can't get data");
            }

#if DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF
            #region 多级词条显示修复
            // 多级词条显示修复
            if (__instance == null)
                return;
            var result = Utils.IsMultiLevelBuff(__instance.buffType, __instance.buffIndex);
            if (result.Item1)
            {
                foreach (var value in result.Item2)
                {
                    int index = (int)CustomCore.variables[0] + value.Item2;
                    Il2CppStructArray<int> upgrades = __instance.manager.ultimateUpgrades;
                    if (TravelLookMenu.Instance.showAll)
                    {
                        __instance.SetText(upgrades[index] != 0, upgrades[index]);
                        if (upgrades[index] <= CustomCore.CustomBuffsLevel[value] &&
                            upgrades[index] != 0)
                        {
                            if (CustomCore.CustomBuffsLevel[value] > 1)
                                __instance.SetText($"已开启（{upgrades[index]}级）");
                            else
                                __instance.SetText($"已开启");
                        }
                        else
                        {
                            __instance.SetText("已关闭");
                        }
                    }
                    else
                    {
                        if (upgrades[index] < CustomCore.CustomBuffsLevel[value] && CustomCore.CustomBuffsLevel[value] != 1)
                        {
                            if (upgrades[index] >= CustomCore.CustomBuffsLevel[value])
                            {
                                __instance.SetText("已满级");
                            }
                            else
                                __instance.SetText($"{upgrades[index]}级");
                        }
                        if (upgrades[index] >= CustomCore.CustomBuffsLevel[value])
                        {
                            __instance.SetText("已满级");
                        }
                    }
                }
            }
            #endregion
#endif
        }
        /// <summary>
        /// 高级词条升级处理
        /// </summary>
        [HarmonyPatch(nameof(TravelLookBuff.OnMouseUpAsButton))]
        [HarmonyPrefix]
        public static bool PreOnMouseUpAsButton(TravelLookBuff __instance)
        {
            var result = Utils.IsMultiLevelBuff(__instance.buffType, __instance.buffIndex);
            bool reset = false;
            if (result.Item1)
            {
                try
                {
                    var array = (int[])TravelMgr.Instance.GetData("CustomBuffsLevel");
                    if (array is null)
                        return true;
                    int index = result.Item2;
                    var list = CustomCore.CustomBuffsLevel.Where(kvp => kvp.Key.Item1 == __instance.buffType && kvp.Key.Item2 == __instance.buffIndex).ToList();
                    int maxLevel = 1;
                    if (list.Count > 0)
                        maxLevel = list[0].Value.Item2;
                    if (TravelLookMenu.Instance.showAll)
                    {
                        array[index] = array[index] + 1;
                        if (array[index] > maxLevel)
                        {
                            array[index] = 0;
                        }
                        if (array[index] == 0)
                        {
                            switch (__instance.buffType)
                            {
                                case BuffType.AdvancedBuff:
                                    TravelMgr.Instance.advancedUpgrades[__instance.buffIndex] = false;
                                    break;
                                case BuffType.UltimateBuff:
                                    TravelMgr.Instance.ultimateUpgrades[__instance.buffIndex] = 0;
                                    break;
                                case BuffType.Debuff:
                                    TravelMgr.Instance.debuff[__instance.buffIndex] = false;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (__instance.buffType)
                            {
                                case BuffType.AdvancedBuff:
                                    TravelMgr.Instance.advancedUpgrades[__instance.buffIndex] = true;
                                    break;
                                case BuffType.UltimateBuff:
                                    TravelMgr.Instance.ultimateUpgrades[__instance.buffIndex] = array[index];
                                    break;
                                case BuffType.Debuff:
                                    TravelMgr.Instance.debuff[__instance.buffIndex] = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                        __instance.SetText(array[index] != 0, array[index]);
                        if (array[index] <= maxLevel &&
                            array[index] != 0)
                        {
                            if (maxLevel > 1)
                                __instance.SetText($"已开启（{array[index]}级）");
                            else
                                __instance.SetText($"已开启");
                        }
                        TravelMgr.Instance.SetData("CustomBuffsLevel", array);
                        return false;
                    }
                    else
                    {
                        if (array[index] < maxLevel && Lawnf.TravelAdvanced(54) && maxLevel != 1)
                        {
                            array[index] = array[index] + 1;
                            reset = true;
                            if (array[index] >= maxLevel)
                                __instance.SetText("已满级");
                            else
                                __instance.SetText($"{array[index]}级");
                        }
                        if (array[index] >= maxLevel)
                        {
                            __instance.SetText("已满级");
                        }
                        TravelMgr.Instance.SetData("CustomBuffsLevel", array);
                    }
                }
                catch (ArgumentException)
                {
                    CustomCore.CLogger.LogInfo("Can't get data");
                }
            }
            if (reset)
            {
                __instance.manager.advancedUpgrades[54] = false;
                return false;
            }
            return true;
        }
#if DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF
        #region 多级词条升级
        /// <summary>
        /// 高级词条升级处理
        /// </summary>
        [HarmonyPatch(nameof(TravelLookBuff.OnMouseUpAsButton))]
        [HarmonyPrefix]
        public static bool PreOnMouseUpAsButton(TravelLookBuff __instance)
        {
            var result = Utils.IsMultiLevelBuff(__instance.buffType, __instance.buffIndex);
            bool reset = false;
            if (result.Item1)
            {
                foreach (var value in result.Item2)
                {
                    int index = (int)CustomCore.variables[0] + value.Item2;
                    if (TravelLookMenu.Instance.showAll)
                    {
                        Il2CppStructArray<int> upgrades = __instance.manager.ultimateUpgrades;
                        upgrades[index] = upgrades[index] + 1;
                        if (upgrades[index] > CustomCore.CustomBuffsLevel[value])
                            upgrades[index] = 0;
                        __instance.SetText(upgrades[index] != 0, upgrades[index]);
                        if (upgrades[index] <= CustomCore.CustomBuffsLevel[value] &&
                            upgrades[index] != 0)
                        {
                            if (CustomCore.CustomBuffsLevel[value] > 1)
                                __instance.SetText($"已开启（{upgrades[index]}级）");
                            else
                                __instance.SetText($"已开启");
                        }
                        return false;
                    }
                    else
                    {
                        Il2CppStructArray<int> upgrades = __instance.manager.ultimateUpgrades;
                        if (upgrades[index] < CustomCore.CustomBuffsLevel[value] && Lawnf.TravelAdvanced(54) && CustomCore.CustomBuffsLevel[value] != 1)
                        {
                            upgrades[index] = upgrades[index] + 1;
                            reset = true;
                            if (upgrades[index] >= CustomCore.CustomBuffsLevel[value])
                                __instance.SetText("已满级");
                            else
                                __instance.SetText($"{upgrades[index]}级");
                        }
                        if (upgrades[index] >= CustomCore.CustomBuffsLevel[value])
                        {
                            __instance.SetText("已满级");
                        }
                    }
                }
            }
            if (reset)
            {
                __instance.manager.advancedUpgrades[54] = false;
                return false;
            }
            return true;
        }
        #endregion
#endif
    }

    [HarmonyPatch(typeof(TravelMgr))]
    public static class TravelMgrPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void PreAwake(TravelMgr __instance)
        {
            if (CustomCore.CustomAdvancedBuffs.Count > 0)
            {
                bool[] newAdv = new bool[__instance.advancedUpgrades.Count + CustomCore.CustomAdvancedBuffs.Count];
                Array.Copy(__instance.advancedUpgrades, newAdv, __instance.advancedUpgrades.Length);
                __instance.advancedUpgrades = newAdv;
            }
            if (CustomCore.CustomUltimateBuffs.Count > 0)//强究词条
            {
                int[] newUlti = new int[__instance.ultimateUpgrades.Count + CustomCore.CustomUltimateBuffs.Count];
                // 多级词条初始化，可能适配时无需取消注释 int[] newUlti = new int[__instance.ultimateUpgrades.Count + CustomCore.CustomBuffsLevel.Count(kvp => kvp.Key.Item1 == BuffType.UltimateBuff && kvp.Value != 1)];
                Array.Copy(__instance.ultimateUpgrades, newUlti, __instance.ultimateUpgrades.Length);
                __instance.ultimateUpgrades = newUlti;
            }
            if (CustomCore.CustomDebuffs.Count > 0)
            {
                bool[] newdeb = new bool[__instance.debuff.Count + CustomCore.CustomDebuffs.Count];
                Array.Copy(__instance.debuff, newdeb, __instance.debuff.Length);
                __instance.debuff = newdeb;
            }

#if DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF
            #region 多级词条扩容
            if (CustomCore.CustomBuffsLevel.Count > 0)//高级词条
            {
                CustomCore.variables[0] = __instance.ultimateUpgrades.Length;
                int length = CustomCore.CustomBuffsLevel.Count(kvp => kvp.Value != 1);
                int[] newLevel = new int[__instance.ultimateUpgrades.Length + length];
                Array.Copy(__instance.ultimateUpgrades, newLevel, __instance.ultimateUpgrades.Length);
                __instance.ultimateUpgrades = newLevel;
            }
            #endregion
#endif

            foreach (PlantType plantType in CustomCore.CustomUltimatePlants) // 注册强究植物
            {
                TravelMgr.allStrongUltimtePlant.Add(plantType);
            }
        }

        [HarmonyPatch(nameof(TravelMgr.Start))]
        [HarmonyPostfix]
        public static void PostStart(TravelMgr __instance)
        {
            if (__instance.GetData("CustomBuffsLevel") is null)
            {
                __instance.SetData("CustomBuffsLevel", new int[CustomCore.CustomBuffsLevel.Count]);
            }
            if (__instance.GetData("LoadByEndless") is null)
                __instance.SetData("LoadByEndless", false);
            if (!__instance.GetData<bool>("LoadByEndless"))
            {
                __instance.SetData("CustomBuffsLevel", new int[CustomCore.CustomBuffsLevel.Count]);
            }
            TravelMgr.Instance.SetData("LoadByEndless", false); // 重置标志位，避免进入其他模式后不重置
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

#if DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF
        #region 多级词条同步
        [HarmonyPatch(nameof(TravelMgr.Start))]
        [HarmonyPostfix]
        public static void PostStart(TravelMgr __instance)
        {
            // 普通词条对应升级
            for (int i = (int)CustomCore.variables[0]; i < __instance.advancedUpgrades.Count; i++)
            {
                var result = Utils.IsMultiLevelBuff(BuffType.AdvancedBuff, i);
                foreach (var ii in result.Item2)
                    if (result.Item1 && __instance.ultimateUpgrades[(int)CustomCore.variables[0] + ii.Item2] == 0 && __instance.advancedUpgrades[i])
                        foreach (var value in result.Item2)
                            __instance.ultimateUpgrades[(int)CustomCore.variables[0] + value.Item2] = 1;
            }
            // Debuff词条对应升级
            for (int i = (int)CustomCore.variables[0]; i < __instance.debuff.Count; i++)
            {
                var result = Utils.IsMultiLevelBuff(BuffType.Debuff, i);
                foreach (var ii in result.Item2)
                    if (result.Item1 && __instance.ultimateUpgrades[(int)CustomCore.variables[0] + ii.Item2] == 0 && __instance.debuff[i])
                        foreach (var value in result.Item2)
                            __instance.ultimateUpgrades[(int)CustomCore.variables[0] + value.Item2] = 1;
            }
        }
        #endregion
#endif
    }

    [HarmonyPatch(typeof(TravelLookMenu))]
    public static class TravelLookMenuPatch
    {
        [HarmonyPatch(nameof(TravelLookMenu.GetAdvBuffs))]
        [HarmonyPostfix]
        public static void PostGetAdvBuffs(ref Il2CppSystem.Collections.Generic.List<int> __result)
        {
            if (!(CustomCore.CustomAdvancedBuffs.Count > 0))
                return;
            for (int i = __result.Count - 1; i >= CustomCore.BuffArrayCount.Item1; i--)
            {
                if (!CustomCore.CustomAdvancedBuffs.ContainsKey(i) && i < __result.Count && i >= 0)
                {
                    __result.RemoveAt(i);
                }
            }
        }

        [HarmonyPatch(nameof(TravelLookMenu.GetDebuffs))]
        [HarmonyPostfix]
        public static void PostGetDebuffs(ref Il2CppSystem.Collections.Generic.List<int> __result)
        {
            if (!(CustomCore.CustomDebuffs.Count > 0))
                return;
            for (int i = __result.Count - 1; i >= CustomCore.BuffArrayCount.Item2; i--)
            {
                if (!CustomCore.CustomDebuffs.ContainsKey(i) && i < __result.Count && i >= 0)
                {
                    __result.RemoveAt(i);
                }
            }
        }

        [HarmonyPatch(nameof(TravelLookMenu.GetUltiBuffs))]
        [HarmonyPostfix]
        public static void PostGetUltimateBuffs(ref Il2CppSystem.Collections.Generic.List<Vector2Int> __result)
        {
            if (!(CustomCore.CustomUltimateBuffs.Count > 0))
                return;
            for (int i = __result.Count - 1; i >= CustomCore.BuffArrayCount.Item3; i--)
            {
                if (!CustomCore.CustomUltimateBuffs.ContainsKey(i) && i < __result.Count && i >= 0)
                {
                    __result.RemoveAt(i);
                }
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
                if (travelBuff.theBuffType is (int)BuffType.AdvancedBuff &&
                    CustomCore.CustomAdvancedBuffs.ContainsKey(travelBuff.theBuffNumber))
                {
                    travelBuff.cost = CustomCore.CustomAdvancedBuffs[travelBuff.theBuffNumber].Item4;
                    travelBuff.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text =
                        $"￥{CustomCore.CustomAdvancedBuffs[travelBuff.theBuffNumber].Item4}";
                }

                if (travelBuff.theBuffType is (int)BuffType.UltimateBuff &&
                    CustomCore.CustomUltimateBuffs.ContainsKey(travelBuff.theBuffNumber))
                {
                    travelBuff.cost = CustomCore.CustomUltimateBuffs[travelBuff.theBuffNumber].Item3;
                    travelBuff.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text =
                        $"￥{CustomCore.CustomUltimateBuffs[travelBuff.theBuffNumber].Item3.ToString()}";
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

        /*[HarmonyPrefix]
        [HarmonyPatch("EliteZombie")]
        public static bool PreEliteZombie(ref ZombieType theZombieType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.EliteZombie.Contains(theZombieType))
            {
                __result = true;
                return false;
            }

            return true;
        }*/

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
        public static Vector3 CalculatePosition(int col,int row)
        {
            return new Vector3(-300f + col * 150, 160f - row * 130);
        }
        [HarmonyPatch(nameof(UIMgr.EnterChallengeMenu))]
        [HarmonyPostfix]
        public static void PostEnterChallengeMenu()
        {
            var levels = GameAPP.canvas.GetChild(0).FindChild("Levels");
            var firstBtns = levels.FindChild("FirstBtns");
            if (firstBtns.FindChild("CustomLevels") == null || firstBtns.FindChild("CustomLevels").IsDestroyed())
            {
                GameObject custom = UnityEngine.Object.Instantiate(firstBtns.GetChild(0).gameObject, firstBtns);
                custom.name = "CustomLevels";
                custom.transform.localPosition = CalculatePosition((firstBtns.childCount - 1) % 6, (firstBtns.childCount - 1) / 6);
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
        public static bool PreEnterGame(ref LevelType levelType, ref int levelNumber, ref int id, ref string name)
        {
            if ((int)levelType is not 66) return true;
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
            // 加载并实例化地图
            GameObject mapInstance = UnityEngine.Object.Instantiate(MapData_cs.GetMap(levelData.SceneType, board), boardGO.transform);
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

    [HarmonyPatch(typeof(WaveManager))]
    public static class WaveManagerPatch
    {
        [HarmonyPatch(nameof(WaveManager.GetMaxWave))]
        [HarmonyPostfix]
        public static void PostGetMaxWave(ref int __result)
        {
            if (Utils.IsCustomLevel(out var levelData))
            {
                __result = levelData.WaveCount();
            }
        }
    }

    [HarmonyPatch(typeof(ZombieDataManager))]
    public static class ZombieDataPatch
    {
        [HarmonyPatch(nameof(ZombieDataManager.LoadData))]
        [HarmonyPostfix]
        public static void InitZombieData()
        {
            foreach (var z in CustomCore.CustomZombies)
            {
                ZombieDataManager.zombieDataDic[z.Key] = z.Value.Item3;
            }
        }
    }

    /// <summary>
    /// 子弹移动路径
    /// </summary>
    [HarmonyPatch(typeof(Bullet))]
    public static class BulletPatch
    {
        [HarmonyPatch(nameof(Bullet.PostionUpdate))]
        [HarmonyPrefix]
        public static bool PostionUpdate(Bullet __instance)
        {
            if (CustomCore.CustomBulletMovingWay.ContainsKey(__instance.theMovingWay))
            {
                CustomCore.CustomBulletMovingWay[__instance.theMovingWay](__instance);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SaveInfo))]
    public static class SaveInfoPatch_SaveLevelData
    {
        [HarmonyPatch(nameof(SaveInfo.SaveSurvivalData), new Type[] { typeof(int), typeof(bool), typeof(int) })]
        [HarmonyPostfix]
        public static void PostSaveSurvivalData_1(SaveInfo __instance, ref int level, ref int id)
        {
            if (TravelMgr.Instance == null)
                return;
            var array = (int[])TravelMgr.Instance.GetData("CustomBuffsLevel");
            if (array is null)
            {
                array = new int[CustomCore.CustomBuffsLevel.Count];
                TravelMgr.Instance.SetData("CustomBuffsLevel", array);
                return;
            }
            if (array.SequenceEqual(new int[CustomCore.CustomBuffsLevel.Count]))
                return;
            String json = JsonSerializer.Serialize(array);
            String originalPath = __instance.GetPath(level, id);
            String? directoryPath = Path.GetDirectoryName(originalPath);
            if (directoryPath is null)
                return;
            String fileName = Path.GetFileName(originalPath);
            String filePath = Path.Combine(directoryPath, $"{fileName}.extra.json");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, json);
        }

        [HarmonyPatch(nameof(SaveInfo.SaveSurvivalData), new Type[] { typeof(SurvivalData), typeof(int), typeof(int) })]
        [HarmonyPostfix]
        public static void PostSaveSurvivalData_2(SaveInfo __instance, ref int level, ref int id)
        {
            if (TravelMgr.Instance == null)
                return;
            var array = (int[])TravelMgr.Instance.GetData("CustomBuffsLevel");
            if (array is null)
            {
                array = new int[CustomCore.CustomBuffsLevel.Count];
                TravelMgr.Instance.SetData("CustomBuffsLevel", array);
                return;
            }
            if (array.SequenceEqual(new int[CustomCore.CustomBuffsLevel.Count]))
                return;
            String json = JsonSerializer.Serialize(array);
            String originalPath = __instance.GetPath(level, id);
            String? directoryPath = Path.GetDirectoryName(originalPath);
            if (directoryPath is null)
                return;
            String fileName = Path.GetFileName(originalPath);
            String filePath = Path.Combine(directoryPath, $"{fileName}.extra.json");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, json);
        }
    }

    [HarmonyPatch(typeof(SaveMgr))]
    public static class SaveMgrPatch
    {
        [HarmonyPatch(nameof(SaveMgr.LoadBoard))]
        [HarmonyPostfix]
        public static void PostLoadBoard(SaveMgr __instance, ref int level)
        {
            if (TravelMgr.Instance == null || SaveInfo.Instance == null)
                return;
            var idGet = SaveInfo.Instance.GetData("endlessID");
            if (idGet is null)
                return;
            var id = (int)idGet;
            String originalPath = SaveInfo.Instance.GetPath(level, id);
            String? directoryPath = Path.GetDirectoryName(originalPath);
            if (directoryPath is null)
                return;
            String fileName = Path.GetFileName(originalPath);
            String filePath = Path.Combine(directoryPath, $"{fileName}.extra.json");
            if (!File.Exists(filePath))
                return;
            String text = File.ReadAllText(filePath);
            int[]? array = JsonSerializer.Deserialize<int[]>(text);
            if (array is null)
                return;
            TravelMgr.Instance.SetData("CustomBuffsLevel", array);
            TravelMgr.Instance.SetData("LoadByEndless", true);
            SaveInfo.Instance.SetData("endlessID", null);
        }
    }


    [HarmonyPatch(typeof(TreasureData))]
    public static class TreasureDataPatch
    {
        [HarmonyPatch(nameof(TreasureData.GetCardLevel))]
        [HarmonyPrefix]
        public static bool GetCardLevel(TreasureData __instance, ref PlantType thePlantType, ref CardLevel __result)
        {
            if (CustomCore.TypeMgrExtra.LevelPlants.ContainsKey(thePlantType))
            {
                __result = CustomCore.TypeMgrExtra.LevelPlants[thePlantType];
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(UIMgr))]
    public static class UIMgrPatch_0
    {
        [HarmonyPatch(nameof(UIMgr.EnterGame))]
        [HarmonyPrefix]
        public static void PreEnterGame(UIMgr __instance, ref int levelNumber, ref int id, ref LevelType levelType)
        {
            if (SaveInfo.Instance == null)
                return;
            if (!Lawnf.IsTravelLevel(levelType, levelNumber))
                return;
            SaveInfo.Instance.SetData("endlessID", id);
        }
    }
}
