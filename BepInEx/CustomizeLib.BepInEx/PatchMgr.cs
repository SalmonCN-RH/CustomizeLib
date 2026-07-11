// #define DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF // 启用多级词条

using AlmanacData;
using Core;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using CustomizeLib.BepInEx.UnmanagedTools;
using GameLevel;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SingleBuffManager;
using static UnityEngine.Object;

#pragma warning disable
///
///Credit to likefengzi(https://github.com/likefengzi)(https://space.bilibili.com/237491236)
///
namespace CustomizeLib.BepInEx
{
    /// <summary>
    /// 注册融合洋芋配方
    /// </summary>
    [HarmonyPatch(typeof(MixBomb), nameof(MixBomb.AttributeEvent))]
    public static class MixBombPatch
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
    public static class FertilizePatch
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

    [HarmonyPatch(typeof(AlmanacPlantWindow))]
    public static class AlmanacPlantWindowPatch
    {
        [HarmonyPatch(nameof(AlmanacPlantWindow.SetPlant))]
        [HarmonyPostfix]
        public static void PostInitWindow(AlmanacPlantWindow __instance, ref PlantType thePlantType)
        {
            {
                PlantType plantType = thePlantType;
                if (CustomCore.CustomPlantsSkin.ContainsKey(plantType))
                    __instance.skinButton.SetActive(CustomCore.CustomPlantsSkin.ContainsKey(plantType));
            }
            {
                PlantType plantType = thePlantType;
                if (CustomCore.CustomPlantTypes.Contains(plantType))
                    __instance.skinButton.SetActive(CustomCore.CustomPlantsSkin.ContainsKey(plantType));
            }
            {
                PlantType plantType = thePlantType;
                if (CustomCore.CustomPlantsSkinActive.ContainsKey(plantType) && CustomCore.CustomPlantsSkinActive[plantType]) return;
                String fullName = Directory.GetParent(Application.dataPath)?.FullName;
                if (fullName == null)
                    return;
                string skinPath = Path.Combine(fullName, "BepInEx", "plugins", "Skin");
                if (!Directory.Exists(skinPath))
                    return;
                var regex = new Regex($@"^skin_{(int)plantType}(?!\d).*$", RegexOptions.IgnoreCase);
                var files = Directory.GetFiles(skinPath).Where(str => regex.IsMatch(Path.GetFileNameWithoutExtension(str))).ToList();
                __instance.skinButton.SetActive(files.Count > 0);
            }
        }

        [HarmonyPatch(nameof(AlmanacPlantWindow.LeftSkin))]
        [HarmonyPrefix]
        public static void PreLeftSkin(AlmanacPlantWindow __instance, out bool __state)
        {
            __state = __instance.skinButton.active;

            PatchMgr.OnChangeSkin(__instance.currentPlantType, GameAPP.resourcesManager.plantSkinDic[__instance.currentPlantType]);
        }

        [HarmonyPatch(nameof(AlmanacPlantWindow.LeftSkin))]
        [HarmonyPostfix]
        public static void PostLeftSkin(AlmanacPlantWindow __instance, bool __state)
        {
            __instance.skinButton.SetActive(__state);

            PatchMgr.OnChangeSkin(__instance.currentPlantType, GameAPP.resourcesManager.plantSkinDic[__instance.currentPlantType]);
            PatchMgr.SaveSkin();
        }

        [HarmonyPatch(nameof(AlmanacPlantWindow.RightSkin))]
        [HarmonyPrefix]
        public static void PreRightSkin(AlmanacPlantWindow __instance, out bool __state)
        {
            __state = __instance.skinButton.active;

            PatchMgr.OnChangeSkin(__instance.currentPlantType, GameAPP.resourcesManager.plantSkinDic[__instance.currentPlantType]);
        }

        [HarmonyPatch(nameof(AlmanacPlantWindow.RightSkin))]
        [HarmonyPostfix]
        public static void PostRightSkin(AlmanacPlantWindow __instance, bool __state)
        {
            __instance.skinButton.SetActive(__state);

            PatchMgr.OnChangeSkin(__instance.currentPlantType, GameAPP.resourcesManager.plantSkinDic[__instance.currentPlantType]);
            PatchMgr.SaveSkin();
        }
    }

    [HarmonyPatch(typeof(AlmanacPlantMenu))]
    public static class AlmanacPlantMenuPatch
    {
        [HarmonyPatch(nameof(AlmanacPlantMenu.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(AlmanacPlantMenu __instance)
        {
            var go = __instance.transform.FindChild("Scroll View/Viewport/Content/LookRedCard").gameObject;
            var newSelect = Instantiate(go, __instance.transform.FindChild("Scroll View/Viewport/Content"));
            Action action = () =>
            {
                Func<PlantType, bool> func = (plantType) => !Enum.IsDefined(plantType);
                __instance.ShowPlants(func);
            };
            UnityEvent unityEvent = new();
            unityEvent.AddListener(action);
            newSelect.GetComponent<UIButton>().clickEvent = unityEvent;
            newSelect.name = "LookCustom";
            newSelect.transform.FindChild("TextShadow").gameObject.GetComponent<TextMeshProUGUI>().text = "二创植物";
            newSelect.transform.localPosition = new Vector3(0f, -44f * newSelect.transform.childCount + 72f, 0f);

            var rect = __instance.transform.FindChild("Scroll View/Viewport/Content").GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 
                rect.sizeDelta.y + 80f);
        }
    }

    [HarmonyPatch(typeof(AlmanacDataLoader))]
    public static class AlmanacDataLoaderPatch
    {
        [HarmonyPatch(nameof(AlmanacDataLoader.LoadZombieData))]
        [HarmonyPostfix]
        public static void PostLoadZombieData()
        {
            foreach (var item in CustomCore.ZombiesAlmanac)
            {
                if (AlmanacDataLoader.zombieDatas.ContainsKey(item.Key)) continue;
                if (item.Value.Item3 != null)
                {
                    AlmanacDataLoader.zombieDatas.Add(item.Key, item.Value.Item3);
                    continue;
                }
                var data = new ZombieInfo();
                var newName = Regex.Replace(item.Value.Item1, @"\([^()]*\)", "");
                data.name = newName;
                data.info = item.Value.Item2;
                data.introduce = "";
                data.theZombieType = item.Key;
                AlmanacDataLoader.zombieDatas.Add(item.Key, data);
            }
        }

        [HarmonyPatch(nameof(AlmanacDataLoader.LoadPlantData))]
        [HarmonyPostfix]
        public static void PostLoadPlantData()
        {
            foreach (var (key, value) in CustomCore.PlantsAlmanac)
            {
                if (AlmanacDataLoader.plantDatas.ContainsKey(key)) continue;
                var data = new PlantInfo();
                var newName = Regex.Replace(value.name, @"\([^()]*\)", "");
                data.name = newName;
                data.info = value.info;
                data.seedType = (int)value.plantType;
                data.cost = value.cost;
                AlmanacDataLoader.plantDatas.Add(key, data);
            }
        }
    }

    [HarmonyPatch(typeof(ConveyManager))]
    public static class ConveyManagerPatch
    {
        [HarmonyPatch(nameof(ConveyManager.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(ConveyManager __instance)
        {
            if (Utils.IsCustomLevel(out var levelData) && levelData.BoardTag.isConvey && levelData.ConveyBeltPlantTypes().Count > 0)
            {
                __instance.plants = levelData.ConveyBeltPlantTypes().ToIl2CppList();
            }
        }

        [HarmonyPatch(nameof(ConveyManager.GetCardPool))]
        [HarmonyPostfix]
        public static void PostGetCardPool(ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            if (Utils.IsCustomLevel(out var levelData) && levelData.BoardTag.isConvey && levelData.ConveyBeltPlantTypes().Count > 0)
            {
                __result = levelData.ConveyBeltPlantTypes().ToIl2CppList();
            }
        }
    }

    [HarmonyPatch(typeof(InGameText), nameof(InGameText.ShowText))]
    public static class InGameTextPatch
    {
        public static bool disable = false;
        [HarmonyPrefix]
        public static bool Prefix(string text, float time)
        {
            if (text == "通关挑战模式解锁配方" && time == 7f && disable)
            {
                disable = false;
                return false;
            }
            return true;
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
        public static void Postfix_SetPlant(CreatePlant __instance, ref int newColumn, ref int newRow, ref Plant __result)
        {
            if (__result != null && __result.TryGetComponent<Plant>(out var plant) &&
                CustomCore.CustomPlantTypes.Contains(plant.thePlantType))
            {
                TypeMgr.GetPlantTag(plant);
            }
        }

        [HarmonyPatch(nameof(CreatePlant.Lim))]
        [HarmonyPostfix]
        public static void PostLim(CreatePlant __instance, ref PlantType theSeedType, ref bool __result)
        {
            // 自定义条件
            {
                if (CustomCore.CustomBanMix.ContainsKey(theSeedType) && CustomCore.CustomBanMix[theSeedType].Item1 != null)
                {
                    if (CustomCore.CustomBanMix[theSeedType].Item1.Invoke())
                    {
                        CustomCore.CustomBanMix[theSeedType].Item2?.Invoke();
                    }
                    else
                    {
                        __result = true;
                        InGameTextPatch.disable = true;
                        CustomCore.CustomBanMix[theSeedType].Item3?.Invoke();
                    }
                }
            }
        }

        [HarmonyPatch(nameof(CreatePlant.LimTravel))]
        [HarmonyPostfix]
        public static void Postfix_LimTravel(CreatePlant __instance, ref PlantType theSeedType, ref bool __result)
        {
            // 判定
            {
                bool isCanSet = false;
                if (TravelMgr.Instance != null && Board.Instance.boardTag.isTravel)
                    isCanSet = true;
                if (__instance.board.boardTag.enableAllTravelPlant || __instance.board.boardTag.enableTravelPlant || __instance.board.boardTag.isTravel)
                    isCanSet = true;

                if (CustomCore.CustomUltimatePlants.Contains(theSeedType) && !isCanSet)
                {
                    __result = true;
                    InGameText.Instance.ShowText("该配方仅旅行生存系列或深渊可用", 3f, false);
                }
            }
            
            // 强究
            {
                if (CustomCore.CustomStrongUltimatePlants.ContainsKey(theSeedType))
                {
                    if (__instance.board == null)
                        __result = false;
                    else
                    {
                        if (!__instance.board.boardTag.enableAllTravelPlant && !__instance.board.boardTag.enableTravelPlant && !__instance.board.boardTag.isSuperRandom && !__instance.board.boardTag.isUltimateSuperRandom)
                        {
                            __result = true;
                            InGameText.Instance.ShowText("该配方仅旅行模式或深渊可用", 4f);
                        }
                        else
                        {
                            if (TravelMgr.Instance == null)
                                __result = false;
                            else
                            {
                                if (TravelMgr.Instance.data.unlockedPlants.Contains((TravelUnlocks)CustomCore.CustomStrongUltimatePlants[theSeedType]) || __instance.board.boardTag.enableAllTravelPlant || __instance.board.boardTag.isSuperRandom || __instance.board.boardTag.isUltimateSuperRandom)
                                    __result = false;
                                else
                                {
                                    __result = true;
                                    InGameText.Instance.ShowText("该配方需要抽取", 4f);
                                }
                            }
                        }
                    }
                }
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

    [HarmonyPatch(typeof(CreateBullet))]
    public static class CreateBulletPatch
    {
        [HarmonyPatch(nameof(CreateBullet.SetBullet))]
        [HarmonyPrefix]
        public static void PreSetBullet(float x, float y, ref BulletType theBulletType, out (bool, BulletType, BulletType, PlantType) __state)
        {
            var colliders = Physics2D.OverlapCircleAll(new Vector2(x - 0.1f, y), 0.2f, LayerMask.GetMask("Plant"));
            foreach (var collider in colliders)
            {
                if (collider == null || collider.gameObject == null || collider.IsDestroyed() || collider.gameObject.IsDestroyed()) continue;
                if (!collider.TryGetComponent<Plant>(out var plant) || plant == null || plant.IsDestroyed()) continue;
                if (CustomCore.CustomBulletsSkinID.ContainsKey((plant.thePlantType, theBulletType)))
                {
                    var ori = theBulletType;
                    var list = CustomCore.CustomBulletsSkinID[(plant.thePlantType, theBulletType)];
                    theBulletType = list[UnityEngine.Random.Range(0, list.Count)];
                    __state = (true, ori, theBulletType, plant.thePlantType);
                    return;
                }
            }

            var circleColliders = Physics2D.OverlapCircleAll(new Vector2(x - 0.1f, y), 0.2f, LayerMask.GetMask("Bullet"));
            foreach (var collider in circleColliders)
            {
                if (collider == null || collider.gameObject == null || collider.IsDestroyed() || collider.gameObject.IsDestroyed()) continue;
                if (!collider.TryGetComponent<Bullet>(out var bullet) || bullet == null || bullet.IsDestroyed()) continue;
                if (bullet.GetData("SkinFromType") == null || bullet.GetData("SkinData") == null) continue;
                var pt = bullet.GetData<PlantType>("SkinFromType");
                if (CustomCore.CustomBulletsSkinID.ContainsKey((pt, theBulletType)))
                {
                    var ori = theBulletType;
                    var list = CustomCore.CustomBulletsSkinID[(pt, theBulletType)];
                    theBulletType = list[UnityEngine.Random.Range(0, list.Count)];
                    __state = (true, ori, theBulletType, pt);
                    return;
                }
            }

            var positions = PositionRecorder.GetRecordPositions(new Vector2(x - 0.1f, y), 0.1f);
            foreach (var item in positions)
            {
                if (CustomCore.CustomBulletsSkinID.ContainsKey((item.plantType, theBulletType)))
                {
                    var ori = theBulletType;
                    var list = CustomCore.CustomBulletsSkinID[(item.plantType, theBulletType)];
                    theBulletType = list[UnityEngine.Random.Range(0, list.Count)];
                    __state = (true, ori, theBulletType, item.plantType);
                    PositionRecorder.RemovePosition(item.index);
                    return;
                }
            }

            __state = (false, (BulletType)(-1), (BulletType)(-1), (PlantType)(-1));
        }

        [HarmonyPatch(nameof(CreateBullet.SetBullet))]
        [HarmonyPostfix]
        public static void PostSetBullet(ref Bullet __result, (bool, BulletType, BulletType, PlantType) __state)
        {
            if (__state.Item1)
            {
                __result.theBulletType = __state.Item2;
                __result.SetData("SkinData", __state.Item3);
                __result.SetData("SkinFromType", __state.Item4);
            }
        }
    }

    /// <summary>
    /// 子弹移动路径
    /// </summary>
    [HarmonyPatch(typeof(Bullet))]
    public static class BulletPatch
    {
        [HarmonyPatch(nameof(Bullet.Update))]
        [HarmonyPostfix]
        public static void PrePostionUpdate(Bullet __instance)
        {
            if (CustomCore.CustomBulletMovingWay.ContainsKey((int)__instance.MoveWay))
            {
                CustomCore.CustomBulletMovingWay[(int)__instance.MoveWay](__instance);
            }
        }

        [HarmonyPatch(nameof(Bullet.Die))]
        [HarmonyPrefix]
        public static void PreDie(Bullet __instance)
        {
            if (__instance.GetData("SkinData") != null)
            {
                PositionRecorder.AddPositonToList(__instance.transform.position, __instance.fromType);
                __instance.theBulletType = __instance.GetData<BulletType>("SkinData");
            }
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

        [HarmonyPatch(nameof(Lawnf.GetName), new Type[] { typeof(PlantType) })]
        [HarmonyPrefix]
        public static bool PreGetName(PlantType thePlantType, ref string __result)
        {
            if (CustomCore.CustomPlantNames.ContainsKey(thePlantType))
            {
                __result = CustomCore.CustomPlantNames[thePlantType];
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Lawnf.GetName), new Type[] { typeof(ZombieType) })]
        [HarmonyPrefix]
        public static bool PreGetName_Zombie(ZombieType theZombieType, ref string __result)
        {
            if (CustomCore.CustomZombieNames.ContainsKey(theZombieType))
            {
                __result = CustomCore.CustomZombieNames[theZombieType];
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Lawnf.TravelAdvanced))]
        [HarmonyPostfix]
        public static void PostTravelAdvanced_0(ref AdvBuff buff, ref bool __result)
        {
            var result = MultiLevelBuff.IsMultiLevelBuff(BuffType.AdvancedBuff, (int)buff);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            if (index < array.Length)
                __result = array[index] > 0;
        }

        [HarmonyPatch(nameof(Lawnf.TravelUltimate))]
        [HarmonyPostfix]
        public static void PostTravelUltimate_0(ref UltiBuff buff, ref bool __result)
        {
            var result = MultiLevelBuff.IsMultiLevelBuff(BuffType.UltimateBuff, (int)buff);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            if (index < array.Length)
                __result = array[index] > 0;
        }

        [HarmonyPatch(nameof(Lawnf.TravelUltimateLevel))]
        [HarmonyPostfix]
        public static void PostTravelUltimateLevel(ref UltiBuff buff, ref int __result)
        {
            var result = MultiLevelBuff.IsMultiLevelBuff(BuffType.UltimateBuff, (int)buff);
            if (!result.Item1)
                return;
            int index2 = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            if ((int)buff < array.Length)
                __result = array[index2];
        }

        [HarmonyPatch(nameof(Lawnf.TravelDebuff), new Type[] { typeof(TravelDebuff) })]
        [HarmonyPostfix]
        public static void PostTravelDebuff_1(ref TravelDebuff buff, ref bool __result)
        {
            var result = MultiLevelBuff.IsMultiLevelBuff(BuffType.Debuff, (int)buff);
            if (!result.Item1)
                return;
            int index = result.Item2;
            if (TravelMgr.Instance == null)
                return;
            var array = TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel");
            if (array is null)
                return;
            if (index < array.Length)
                __result = array[index] > 0;
        }
    }

    [HarmonyPatch(typeof(Lawnf))]
    public static class LawnfPatch_BuffGet
    {
        [HarmonyPatch(nameof(Lawnf.TravelAdvanced), new Type[] { typeof(AdvBuff) })]
        [HarmonyPrefix]
        public static void PreTravelAdvanced_1(ref AdvBuff buff)
        {
            if (CustomCore.CustomBuffIDMapping.ContainsKey((BuffType.AdvancedBuff, (int)buff)))
                buff = (AdvBuff)CustomCore.CustomBuffIDMapping[(BuffType.AdvancedBuff, (int)buff)];
        }

        [HarmonyPatch(nameof(Lawnf.TravelUltimate), new Type[] { typeof(UltiBuff) })]
        [HarmonyPrefix]
        public static void PreTravelUltimate_1(ref UltiBuff buff)
        {
            if (CustomCore.CustomBuffIDMapping.ContainsKey((BuffType.UltimateBuff, (int)buff)))
                buff = (UltiBuff)CustomCore.CustomBuffIDMapping[(BuffType.UltimateBuff, (int)buff)];
        }

        [HarmonyPatch(nameof(Lawnf.TravelDebuff), new Type[] { typeof(TravelDebuff) })]
        [HarmonyPrefix]
        public static void PreTravelDebuff_1(ref TravelDebuff buff)
        {
            if (CustomCore.CustomBuffIDMapping.ContainsKey((BuffType.Debuff, (int)buff)))
                buff = (TravelDebuff)CustomCore.CustomBuffIDMapping[(BuffType.Debuff, (int)buff)];
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
        public static void PostfixStart(UIButton __instance)
        {
            if (SelectCustomPlants.Instance != null && SelectCustomPlants.CustomPage != null && SelectCustomPlants.CustomPage.activeSelf)
            {
                SelectCustomPlants.CustomPage.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(InGameUI))]
    public static class InGameUIPatch
    {
        [HarmonyPatch(nameof(InGameUI.SetUniqueText))]
        [HarmonyPostfix]
        public static void PostSetUniqueText(InGameUI __instance, ref Il2CppSystem.Collections.Generic.List<TextMeshProUGUI> T)
        {
            if (GameAPP.theBoardType is (LevelType)66)
            {
                __instance.ChangeString(T, CustomCore.CustomLevels[GameAPP.theBoardLevel].Name());
            }
        }

        [HarmonyPatch(nameof(InGameUI.MoveCardToTarget))]
        [HarmonyPrefix]
        public static void PreMoveCardToTarget(ref CardUI card)
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
            var travelMgr = GameAPP.Instance.GetOrAddComponent<TravelMgr>();
            var data = travelMgr?.data;
            if (data == null) return;
            foreach (var a in levelData.AdvBuffs())
            {
                if (a >= 0)
                {
                    data.advBuffs.Add((AdvBuff)a);
                }
            }
            foreach (var u in levelData.UltiBuffs())
            {
                if (u.Item1 >= 0 && u.Item2 >= 0)
                {
                    data.ultiBuffs.Add((UltiBuff)u.Item1);
                    if (u.Item2 > 1)
                        data.ultiBuffs_lv2.Add((UltiBuff)u.Item1);
                }
            }
            foreach (var d in levelData.Debuffs())
            {
                if (d >= 0)
                {
                    data.travelDebuffs.Add((TravelDebuff)d);
                }
            }
        }
    }

    [HarmonyPatch(typeof(InitZombieList))]
    public static class InitZombieListAllowZombiePatch
    {
        [HarmonyPatch(nameof(InitZombieList.PickZombie))]
        [HarmonyPrefix]
        public static void PrePickZombie()
        {
            if (Utils.IsCustomLevel(out var levelData))
            {
                foreach (var z in levelData.ZombieList())
                    InitZombieList.zombieToSpawns.Add(z);
            }
        }
    }

    /// <summary>
    /// 花钱开大招
    /// </summary>
    [HarmonyPatch(typeof(Money))]
    public static class MoneyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Money.ReinforcePlant))]
        public static bool PreReinforcePlant(Money __instance, ref Plant plant)
        {
            if (CustomCore.SuperSkills.ContainsKey(plant.thePlantType))
            {
                var cost = CustomCore.SuperSkills[plant.thePlantType].Item1(plant);//实时计算大招花费

                if (Board.Instance.theMoney < cost)//如果钱不够
                {
                    InGameText.Instance.ShowText($"大招需要{cost}金币", 5);//提示
                    return false;//直接返回
                }

                if (plant.SuperSkill())
                {
                    CustomCore.SuperSkills[plant.thePlantType].Item2(plant);//执行大招代码
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
        [HarmonyPatch(nameof(Mouse.GetPlantsOnMouse))]
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

        [HarmonyPatch(nameof(Mouse.Update))]
        [HarmonyPrefix]
        public static bool PreMouseClick(Mouse __instance)
        {
            if (!Input.GetMouseButtonDown(0))
                return true;
            if (__instance.theItemOnMouse == null)
                return true;
            var list = new List<Plant>();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 rayPosition = new Vector2(worldPosition.x, worldPosition.y);

            // 从鼠标位置发射射线检测碰撞
            foreach (var hit in Physics2D.RaycastAll(rayPosition, Vector2.zero))
            {
                if (hit.collider == null || hit.collider.gameObject == null || hit.collider.gameObject.IsDestroyed())
                    continue;
                if (!hit.collider.gameObject.TryGetComponent<Plant>(out var plant))
                    continue;
                if (plant == null)
                    continue;
                list.Add(plant);
            }
            if (list.Count <= 0)
                return true;
            bool found = false;
            bool clear = false;
            List<Action<Plant>> executedActions = [];
            foreach (var item in list)
            {
                if (item == null)
                    continue;
                if (__instance.thePlantOnGlove != null && item == __instance.thePlantOnGlove)
                    continue;
                if (CustomCore.CustomClickCardOnPlantEvents.ContainsKey((item.thePlantType, __instance.thePlantTypeOnMouse)))
                {
                    bool block = false, clearOrigin = false;
                    foreach (var (action, can, onPlant) in CustomCore.CustomClickCardOnPlantEvents[(item.thePlantType, __instance.thePlantTypeOnMouse)])
                    {
                        if (executedActions.Contains(action)) // 判断，不然会多执行一次
                            continue;
                        if (can != null && !can(item))
                            continue;
                        if (onPlant.Trigger == CustomClickCardOnPlant.TriggerType.CardOnly && __instance.thePlantOnGlove != null)
                            continue;
                        if (onPlant.Trigger == CustomClickCardOnPlant.TriggerType.GloveOnly && __instance.thePlantOnGlove == null)
                            continue;
                        action(item);
                        executedActions.Add(action);
                        if (onPlant.BlockFusion)
                            block = true;
                        if (!onPlant.SaveOrigin)
                            clearOrigin = true;
                        found = true;
                    }
                    if (block)
                    {
                        return false;
                    }
                    if (clearOrigin)
                    {
                        clear = true;
                    }
                }
            }
            if (found && clear)
            {
                if (__instance.theCardOnMouse != null)
                {
                    if (__instance.theCardOnMouse.TryGetComponent<DroppedCard>(out var card) && card != null)
                    {
                        card.usedTimes++;
                        if (Board.Instance != null)
                        {
                            Board.Instance.UseSun(card.theSeedCost);

                            // 高级旅行检查
                            if (Lawnf.TravelAdvanced((AdvBuff)5004))
                            {
                                Board.Instance.UseSun(Board.Instance.theSun / 2);
                            }
                        }
                    }
                    else
                    {
                        __instance.theCardOnMouse.CD = 0f;
                        __instance.theCardOnMouse.isPickUp = false;
                        if (Board.Instance != null)
                        {
                            Board.Instance.UseSun(__instance.theCardOnMouse.theSeedCost);

                            // 高级旅行检查
                            if (Lawnf.TravelAdvanced((AdvBuff)5004))
                            {
                                Board.Instance.UseSun(Board.Instance.theSun / 2);
                            }
                        }
                    }
                }
                if (__instance.thePlantOnGlove != null)
                {
                    __instance.thePlantOnGlove.Die(Plant.DieReason.ByShovel);
                    Glove glove = Glove.Instance;
                    if (glove != null)
                    {
                        float gloveCD = Lawnf.GetGloveCD();
                        glove.fullCD = gloveCD;
                        glove.CD = 0f;

                        // 特殊植物类型冷却时间调整
                        if (TypeMgr.IsPuff(__instance.thePlantTypeOnMouse) || TypeMgr.IsPot(__instance.thePlantTypeOnMouse) ||
                            TypeMgr.IsLily(__instance.thePlantTypeOnMouse) || TypeMgr.FlyingPlants(__instance.thePlantTypeOnMouse))
                        {
                            glove.CD = (glove.fullCD + glove.fullCD) / 3f;
                        }
                    }
                }
                Destroy(__instance.theItemOnMouse);
                __instance.ClearItemOnMouse(false);
            }
            if (!clear)
                return true;
            return !found;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Mouse.LeftClickWithNothing))]
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

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Awake))]
        [HarmonyPostfix]
        public static void PostAwake()
        {
            InterfaceCreator.InitInstance();
        }

        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPostfix]
        public static void PostStart(GameAPP __instance)
        {
            __instance.StartCoroutine(CoreTools.Init());
        }

        [HarmonyPatch(nameof(GameAPP.LoadResources))]
        [HarmonyPrefix]
        public static void Prefix()
        {
            try
            {
                #region 自动扩容
                // 扩容particlePrefab
                if (CustomCore.CustomParticles.Count > 0 && (int)CustomCore.CustomParticles.Keys.DefaultIfEmpty().Max() + 1 >= GameAPP.particlePrefab.Length)
                {
                    long size_particlePrefab = (int)CustomCore.CustomParticles.Keys.DefaultIfEmpty().Max();
                    Il2CppReferenceArray<GameObject> particlePrefab = new Il2CppReferenceArray<GameObject>(size_particlePrefab + 1);
                    GameAPP.particlePrefab = particlePrefab;
                }

                // 扩容spritePrefab
                if (CustomCore.CustomSprites.Count > 0 && CustomCore.CustomSprites.Keys.DefaultIfEmpty().Max() + 1 >= GameAPP.spritePrefab.Length)
                {
                    long size_spritePrefab = CustomCore.CustomSprites.Keys.Max();
                    Il2CppReferenceArray<Sprite> spritePrefab = new Il2CppReferenceArray<Sprite>(size_spritePrefab + 1);
                    GameAPP.spritePrefab = spritePrefab;
                }
                #endregion
            }
            catch (InvalidOperationException) { }
            foreach (var plant in CustomCore.CustomPlants)//二创植物
            {
                GameAPP.resourcesManager.plantPrefabs[plant.Key] = plant.Value.Prefab;//注册预制体
                GameAPP.resourcesManager.plantPrefabs[plant.Key].tag = "Plant";//必须打tag
                if (!GameAPP.resourcesManager.allPlants.Contains(plant.Key))
                    GameAPP.resourcesManager.allPlants.Add(plant.Key);//注册植物类型
                if (plant.Value.PlantData is not null)
                {
                    PlantDataManager.PlantData_Default.Add(plant.Key, plant.Value.PlantData);//注册植物数据
                }
                GameAPP.resourcesManager.plantPreviews[plant.Key] = plant.Value.Preview;//注册植物预览
                GameAPP.resourcesManager.plantPreviews[plant.Key].tag = "Preview";//必修打tag
            }
            foreach (var f in CustomCore.CustomFusions)
            {
                MixData.AddOrderedRecipe((PlantType)f.Item2, (PlantType)f.Item3, (PlantType)f.Item1);
            }

            foreach (var z in CustomCore.CustomZombies)//注册二创僵尸
            {
                if (!GameAPP.resourcesManager.allZombieTypes.Contains(z.Key))
                    GameAPP.resourcesManager.allZombieTypes.Add(z.Key);//注册僵尸类型
                GameAPP.resourcesManager.zombiePrefabs[z.Key] = z.Value.Item1;//注册僵尸预制体
                GameAPP.resourcesManager.zombiePrefabs[z.Key].layer = LayerMask.NameToLayer("Zombie"); // 改层级
                GameAPP.resourcesManager.zombiePrefabs[z.Key].tag = "Zombie";//必修打tag
                InitZombieList.allowAllzombies.Add(z.Key);
                if (z.Value.Item2 != null)
                    GameAPP.resourcesManager.zombieSprites[z.Key] = z.Value.Item2;
            }

            // 先注册二创子弹，再注册皮肤，不然注册二创子弹皮肤会出bug
            foreach (var bullet in CustomCore.CustomBullets)//注册二创子弹
            {
                GameAPP.resourcesManager.bulletPrefabs[bullet.Key] = bullet.Value;//注册子弹预制体
                if (!GameAPP.resourcesManager.allBullets.Contains(bullet.Key))
                    GameAPP.resourcesManager.allBullets.Add(bullet.Key);//注册子弹类型
            }

            foreach (var (id, list) in CustomCore.CustomSkinBullet) //注册二创皮肤子弹
            {
                foreach (var (newBulletID, bullet) in list)
                {
                    if (bullet == null) continue;
                    foreach (var comp in GameAPP.resourcesManager.bulletPrefabs[id].GetComponents<Component>())
                        if (bullet != null && !bullet.TryGetComponent(comp.GetIl2CppType(), out var cmp) && cmp == null)
                            bullet.AddComponent(comp.GetIl2CppType());
                    bullet.GetComponent<Bullet>().theBulletType = id;
                    GameAPP.resourcesManager.bulletPrefabs[newBulletID] = bullet;
                    if (!GameAPP.resourcesManager.allBullets.Contains(newBulletID))
                        GameAPP.resourcesManager.allBullets.Add(newBulletID);
                }
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

            foreach (var audio in CustomCore.CustomSounds)
            {
                GameAPP.soundManager.sounds[(SoundType)audio.Key] = audio.Value;
            }
            GameAPP.Instance.StartCoroutine(PatchMgr.RegisterSkin()); // 在所有注册完成之后启动皮肤协程
        }
    }

    [HarmonyPatch(typeof(UIMgr), nameof(UIMgr.EnterMainMenu))]
    public static class NoticeMenuPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                if (PatchMgr.Load) return;
                var behaviour = new GameObject("CustomCore Behaviour");
                behaviour.AddComponent<CoreBehaviour>();
                behaviour.AddComponent<PositionRecorder>();
                behaviour.transform.SetParent(null);
                DontDestroyOnLoad(behaviour);

                // 注册红卡
                {
                    var propertyInfo = typeof(TypeMgr).GetProperty("RedPlant", BindingFlags.Static | BindingFlags.Public);
                    var value = propertyInfo.GetValue(null);
                    var redPlant = (Il2CppSystem.Collections.Generic.HashSet<PlantType>)value;
                    foreach (var (k, v) in CustomCore.TypeMgrExtra.LevelPlants)
                        if (v == CardLevel.Red)
                            redPlant.Add(k);
                    propertyInfo.SetValue(null, redPlant);
                }
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

                PatchMgr.Load = true;
                foreach (var action in CorePlugin.OnGameInitAction)
                    action.Invoke();
            }
            finally
            {
                PatchMgr.Load = true;
            }
        }
    }

    [HarmonyPatch]
    public static class OptionMenuPatch
    {
        [HarmonyTargetMethod]
        public static MethodBase GetTargetMethod()
        {
            foreach (var type in typeof(OptionMenu).GetNestedTypes(BindingFlags.Public | BindingFlags.Instance))
            {
                var method = type.GetMethod("_OnLockAlmanacMenu_b__0", BindingFlags.Public | BindingFlags.Instance);
                if (method != null) return method;
            }
            return null;
        }

        [HarmonyPostfix]
        public static void PostOnLockAlmanacMenu()
        {
            foreach (var pt in GameAPP.resourcesManager.allPlants)
            {
                if (!GameAPP.config.meetPlant_runTime.Contains(pt))
                    GameAPP.config.meetPlant_runTime.Add(pt);
            }
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Plant.UseItem))]
        public static void PostUseItem(Plant __instance, ref BucketType type, ref Bucket bucket)
        {
            if (CustomCore.CustomUseItems.ContainsKey((__instance.thePlantType, type)))
            {
                CustomCore.CustomUseItems[(__instance.thePlantType, type)](__instance);
                UnityEngine.Object.Destroy(bucket.gameObject);
            }
        }

        [HarmonyPatch(nameof(Plant.Start))]
        [HarmonyPostfix]
        public static void PostStart(Plant __instance)
        {
            if (__instance != null && CustomCore.CustomOnMixEvent.ContainsKey((__instance.firstParent, __instance.secondParent)))
            {
                foreach (var action in CustomCore.CustomOnMixEvent[(__instance.firstParent, __instance.secondParent)])
                    action.Invoke(__instance);
            }
        }
    }

    /// <summary>
    /// 显示自定义卡
    /// </summary>
    [HarmonyPatch(typeof(SeedLibrary))]
    public static class SeedLibraryPatch
    {
        [HarmonyPatch(nameof(SeedLibrary.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(SeedLibrary __instance)
        {
            SelectCustomPlants.InitButton();
            // 注册自定义卡牌
            PatchMgr.ShowCustomCards(__instance);
        }
    }

    /// <summary>
    /// 显示自定义卡
    /// </summary>
    [HarmonyPatch(typeof(PlantCardPackageBuilder))]
    public static class PlantCardPackageBuilderPatch
    {
        [HarmonyPatch(nameof(PlantCardPackageBuilder.Start))]
        [HarmonyPostfix]
        public static void PostStart(PlantCardPackageBuilder __instance)
        {
            SelectCustomPlants.InitButton();
            // 注册自定义卡牌
            PatchMgr.ShowCustomCards(__instance);
        }
    }

    [HarmonyPatch(typeof(Board))]
    public static class Board_Patch
    {
        [HarmonyPatch(nameof(Board.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            if (TravelMgr.Instance == null)
                return;
            if (TravelMgr.Instance.GetData("LoadByEndless") is null)
                TravelMgr.Instance.SetData("LoadByEndless", false);
            if ((TravelMgr.Instance.GetData("CustomBuffsLevel") is null ||
                (TravelMgr.Instance.GetData("CustomBuffsLevel") != null && TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel").SequenceEqual(new int[CustomCore.CustomAdvancedBuffs.Count]))) &&
                !TravelMgr.Instance.GetData<bool>("LoadByEndless"))
            {
                TravelMgr.Instance.SetData("CustomBuffsLevel", new int[CustomCore.CustomAdvancedBuffs.Count]);
            }
        }

        [HarmonyPatch(nameof(Board.OnDestroy))]
        [HarmonyPostfix]
        public static void PostOnDestroy()
        {
            try
            {
                if (TravelMgr.Instance == null)
                    return;
                if (TravelMgr.Instance.GetData("LoadByEndless") is null)
                    TravelMgr.Instance.SetData("LoadByEndless", false);
                if ((TravelMgr.Instance.GetData("CustomBuffsLevel") is null ||
                    (TravelMgr.Instance.GetData("CustomBuffsLevel") != null && TravelMgr.Instance.GetData<int[]>("CustomBuffsLevel").SequenceEqual(new int[CustomCore.CustomAdvancedBuffs.Count]))) &&
                    !TravelMgr.Instance.GetData<bool>("LoadByEndless"))
                {
                    TravelMgr.Instance.SetData("CustomBuffsLevel", new int[CustomCore.CustomAdvancedBuffs.Count]);
                }
            }
            catch { }
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
                    var result = MultiLevelBuff.IsMultiLevelBuff(key.Item1, key.Item2);
                    if (!result.Item1)
                        continue;
                    int index = result.Item2;
                    if (index >= array.Length)
                        continue;
                    var data = TravelMgr.Instance.data;
                    var id = new BuffID(key.Item2);
                    switch (key.Item1)
                    {
                        case BuffType.AdvancedBuff:
                            {
                                if (!data.advBuffs.Contains(id))
                                    array[index] = 0;
                                if (array[index] <= 0 && data.advBuffs.Contains(id))
                                    array[index] = 1;
                            }
                            break;
                        case BuffType.UltimateBuff:
                            {
                                if (!data.ultiBuffs.Contains(id) && !data.ultiBuffs_lv2.Contains(id))
                                    array[index] = 0;
                                if (array[index] <= 0 && data.ultiBuffs.Contains(id))
                                    array[index] = 1;
                                if (array[index] <= 0 && data.ultiBuffs_lv2.Contains(id))
                                    array[index] = 2;
                            }
                            break;
                        case BuffType.Debuff:
                            {
                                if (!data.travelDebuffs.Contains(id))
                                    array[index] = 0;
                                if (array[index] <= 0 && data.travelDebuffs.Contains(id))
                                    array[index] = 1;
                            }
                            break;
                        case BuffType.UnlockPlant:
                            {
                                if (!data.unlockedPlants.Contains(id))
                                    array[index] = 0;
                                if (array[index] <= 0 && data.unlockedPlants.Contains(id))
                                    array[index] = 1;
                            }
                            break;
                    }
                    TravelMgr.Instance.SetData("CustomBuffsLevel", array);
                }
            }
            catch (ArgumentException) { }
        }

        [HarmonyPatch(nameof(Board.WheatLimit))]
        [HarmonyPrefix]
        public static bool PreWheatLimit(ref PlantType plantType, ref bool __result)
        {
            if (CustomCore.CustomUltimatePlants.Contains(plantType))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BoardAction))]
    public static class BoardActionPatch
    {
        [HarmonyPatch(nameof(BoardAction.CreateCherryExplode))]
        [HarmonyPrefix]
        public static bool PreCreateCherryExplode(Board __instance, ref Vector2 v, ref int theRow,
            ref CherryBombType bombType, ref int damage, ref PlantType fromType, ref Il2CppSystem.Action<Zombie> action, ref bool immediately, ref BombCherry __result)
        {
            if (CustomCore.CustomCherrys.ContainsKey(bombType) && __instance != null)
            {
                CreateParticle.SetParticle(CustomCore.CustomCherryStartID + (int)bombType, v, 11);
                ScreenShake.TriggerShake(0.15f);
                GameAPP.PlaySound(40, 0.5f, 1.0f);

                BombCherry cherry = new BombCherry();
                cherry.board = __instance;
                cherry.damageToZombie = damage;
                cherry.bombRow = theRow;
                cherry.bombType = bombType;
                cherry.zombieAction = action;
                cherry.bombPosition = v;
                cherry.fromType = fromType;
                cherry.targetPlant = null;

                if (immediately)
                {
                    cherry.Explode(CustomDamageMaker.DamageMaker);
                }

                __result = cherry;
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
        [HarmonyPrefix]
        public static void PreSetBuff(TravelBuffOptionButton __instance, Il2CppSystem.Object buff)
        {
            __instance.GeneralSet(buff);
        }

        /// <summary>
        /// 强究词条显示植物修复
        /// </summary>
        [HarmonyPatch(nameof(TravelBuffOptionButton.SetPlant), new Type[] { })]
        [HarmonyPostfix]
        public static void PostSetPlant(TravelBuffOptionButton __instance)
        {
            var (buffType, buffIndex) = __instance.TryGetTypeAndID();
            var list = CustomCore.CustomUltimateBuffs.
                Where(kvp => kvp.Key == buffIndex).
                ToList();
            if (buffType == BuffType.UltimateBuff && list.Count > 0)
            {
                foreach (var value in list)
                {
                    if (value.Value.Item1 == PlantType.Nothing)
                        __instance.SetPlant(PlantType.EndoFlame);
                    else
                        __instance.SetPlant(value.Value.Item1);
                }
            }
        }
    }

    [HarmonyPatch(typeof(TravelBuffOptionButton))]
    public static class TravelBuffOptionButtonIconPatch
    {
        [HarmonyPatch(nameof(TravelBuffOptionButton.SetBuff))]
        [HarmonyPrefix]
        public static void PreSetBuff(TravelBuffOptionButton __instance, Il2CppSystem.Object buff)
        {
            __instance.GeneralSet(buff);
        }

        [HarmonyPatch(nameof(TravelBuffOptionButton.SetBuff))]
        [HarmonyPostfix]
        public static void PostSetBuff(TravelBuffOptionButton __instance)
        {
            var tuple = __instance.TryGetTypeAndID();
            if (CustomCore.CustomBuffsBg.ContainsKey(tuple))
            {
                __instance.SetBackground(CustomCore.CustomBuffsBg[tuple]);
            }
            if (CustomCore.CustomBuffIcon.ContainsKey(tuple))
            {
                if (__instance.show.IsObjExist())
                    Destroy(__instance.show.gameObject);
                __instance.SetPlant((CustomCore.CustomBuffIcon[tuple]));
            }
        }
    }

    //[HarmonyPatch(typeof(TravelBuff))]
    //public static class TravelBuffPatch
    //{
    //    [HarmonyPrefix]
    //    [HarmonyPatch(nameof(TravelBuff.ChangeSprite))]
    //    public static void PreChangeSprite(TravelBuff __instance)
    //    {
    //        var list = CustomCore.CustomUltimateBuffs.
    //                Where(kvp => kvp.Key == __instance.theBuffNumber).
    //                Select(kvp => kvp.Value).
    //                ToList();
    //        if (__instance.theBuffType == (int)BuffType.UltimateBuff && list.Count > 0)
    //        {
    //            foreach (var item in list)
    //            {
    //                if (item.Item1 == PlantType.Nothing)
    //                    __instance.thePlantType = PlantType.EndoFlame;
    //                else
    //                    __instance.thePlantType = item.Item1;
    //            }
    //        }

    //        if (__instance.theBuffType == 1 && CustomCore.CustomAdvancedBuffs.ContainsKey(__instance.theBuffNumber))
    //        {
    //            __instance.thePlantType = CustomCore.CustomAdvancedBuffs[__instance.theBuffNumber].Item1;
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(TravelLookBuff))]
    public static class TravelLookBuffPatch
    {
        [HarmonyPatch(nameof(TravelLookBuff.SetBuff))]
        [HarmonyPrefix]
        public static void PreSetBuff(TravelLookBuff __instance, Il2CppSystem.Object buff)
        {
            __instance.GeneralSet(buff);
        }

        [HarmonyPatch(nameof(TravelLookBuff.SetBuff))]
        [HarmonyPostfix]
        public static void PostSetBuff(TravelLookBuff __instance)
        {
            var (buffType, buffIndex) = __instance.TryGetTypeAndID();
            if (CustomCore.CustomBuffIcon.ContainsKey((buffType, buffIndex)))
            {
                if (__instance.show != null)
                    Destroy(__instance.show);
                __instance.SetPlant(CustomCore.CustomBuffIcon[(buffType, buffIndex)]);
            }
            if (CustomCore.CustomBuffsBg.ContainsKey((buffType, buffIndex)))
                __instance.SetBackground(CustomCore.CustomBuffsBg[(buffType, buffIndex)]);
            if (CustomCore.CustomDebuffs.ContainsKey(buffIndex))
            {
                if (__instance.show != null)
                    Destroy(__instance.show);
                __instance.SetZombie(CustomCore.CustomDebuffs[buffIndex].Item2);
            }

            // 多级词条文本显示
            var result = MultiLevelBuff.IsMultiLevelBuff(buffType, buffIndex);
            try
            {
                // 如果是多级词条
                if (result.Item1)
                {
                    var array = MultiLevelBuff.GetBuffArray();
                    if (array is null) return; // 如果数据数组为空直接返回
                    int index = result.Item2;
                    int maxLevel = MultiLevelBuff.GetBuffMaxLevel(buffType, buffIndex);
                    if (TravelLookMenu.Instance.showAll) // 如果是iz的全选模式
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
                            __instance.SetText($"{array[index]}级");
                        }
                        else if (array[index] >= maxLevel && maxLevel != 1)
                        {
                            __instance.SetText("已满级");
                        }
                        TravelMgr.Instance.SetData(LevelBuffData.LEVEL_BUFF_ARR, array);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                CustomCore.CLogger.LogWarning($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 高级词条升级处理
        /// </summary>
        [HarmonyPatch(nameof(TravelLookBuff.OnMouseUpAsButton))]
        [HarmonyPrefix]
        public static bool PreOnMouseUpAsButton(TravelLookBuff __instance)
        {
            var (buffType, buffIndex) = __instance.TryGetTypeAndID();
            var result = MultiLevelBuff.IsMultiLevelBuff(buffType, buffIndex);
            bool reset = false; // 重置升级词条
            if (result.Item1)
            {
                try
                {
                    var array = MultiLevelBuff.GetBuffArray();
                    if (array is null) return true;
                    int index = result.Item2;
                    int maxLevel = MultiLevelBuff.GetBuffMaxLevel(buffType, buffIndex);
                    if (TravelLookMenu.Instance.showAll) // 如果是iz的全选
                    {
                        MultiLevelBuff.AddBuffLevel(buffType, buffIndex);
                        __instance.SetText(array[index] != 0, array[index]); // 设置文本
                        if (array[index] <= maxLevel && array[index] != 0)
                        {
                            if (maxLevel > 1)
                                __instance.SetText($"已开启（{array[index]}级）");
                            else
                                __instance.SetText($"已开启");
                        }
                        TravelMgr.Instance.SetData(LevelBuffData.LEVEL_BUFF_ARR, array);
                        return false;
                    }
                    else
                    {
                        if (array[index] < maxLevel && CoreTools.TravelAdvanced("升级") && maxLevel != 1)
                        {
                            array[index] = array[index] + 1; // 升级
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
                catch (ArgumentException ex)
                {
                    CustomCore.CLogger.LogWarning($"StackTrace: {ex.StackTrace}");
                }
            }
            if (reset)
            {
                __instance.manager.data.advBuffs.Remove(CoreTools.GetAdvBuffByString("升级")); // 移除升级
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AlmanacBuffMenu))]
    public static class AlmanacBuffMenuPatch
    {
        [HarmonyPatch(nameof(AlmanacBuffMenu.OnToolClick))]
        [HarmonyPrefix]
        public static bool PreOnToolClick(AlmanacBuffMenu __instance, ref UIButton button)
        {
            if (__instance.current == null) return true;
            var buff = __instance.cardInfos[__instance.current].buff;
            var (buffType, buffIndex) = TravelExtensions.GetTypeAndID(buff);
            var result = MultiLevelBuff.IsMultiLevelBuff(buffType, buffIndex);
            bool reset = false; // 重置升级词条
            if (result.Item1)
            {
                try
                {
                    var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    var array = MultiLevelBuff.GetBuffArray();
                    if (array is null) return true;
                    int index = result.Item2;
                    int maxLevel = MultiLevelBuff.GetBuffMaxLevel(buffType, buffIndex);
                    if (__instance.editMode) // 如果是iz的全选
                    {
                        MultiLevelBuff.AddBuffLevel(buffType, buffIndex);
                        MultiLevelBuff.SetToolText(button, buffType, buffIndex, array[index] != 0); // 设置文本
                        TravelMgr.Instance.SetData(LevelBuffData.LEVEL_BUFF_ARR, array);
                        // 更新卡片UI的透明度
                        var hasBuff = Lawnf.HasTravelBuff(buff) ? 0f : 1f;
                        __instance.current.GetComponent<Image>().color = new Color(hasBuff, 1f, hasBuff, 1f);
                        return false;
                    }
                    else
                    {
                        if (array[index] < maxLevel && CoreTools.TravelAdvanced("升级") && maxLevel != 1)
                        {
                            array[index] = array[index] + 1; // 升级
                            reset = true;
                            if (array[index] >= maxLevel)
                                buttonText.text = "已满级";
                            else
                                buttonText.text = $"{array[index]}级";
                        }
                        if (array[index] >= maxLevel) buttonText.text = "已满级";
                        TravelMgr.Instance.SetData("CustomBuffsLevel", array);
                        // 更新卡片UI的透明度
                        var hasBuff = Lawnf.HasTravelBuff(buff) ? 0f : 1f;
                        __instance.current.GetComponent<Image>().color = new Color(hasBuff, 1f, hasBuff, 1f);
                    }
                }
                catch (ArgumentException ex)
                {
                    CustomCore.CLogger.LogWarning($"StackTrace: {ex.StackTrace}");
                }
            }
            if (reset)
            {
                TravelMgr.Instance.data.advBuffs.Remove(CoreTools.GetAdvBuffByString("升级")); // 移除升级
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(AlmanacBuffMenu.OnCardClick))]
        [HarmonyPostfix]
        public static void PostOnCardClick(AlmanacBuffMenu __instance, ref AlmanacCardUI card)
        {
            var button = __instance.transform.FindChild("Tool").GetComponent<UIButton>();
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (__instance.current == null || !__instance.cardInfos.ContainsKey(__instance.current)) return;
            var buff = __instance.cardInfos[__instance.current].buff;
            var (buffType, buffIndex) = TravelExtensions.GetTypeAndID(buff);
            var array = MultiLevelBuff.GetBuffArray();
            if (array is null) return;
            var result = MultiLevelBuff.IsMultiLevelBuff(buffType, buffIndex);
            if (result.Item1)
            {
                MultiLevelBuff.SetToolText(button, buffType, buffIndex, array[result.Item2] != 0); // 设置文本
            }
        }

        [HarmonyPatch(nameof(AlmanacBuffMenu.Start))]
        [HarmonyPostfix]
        public static void PostStart(AlmanacBuffMenu __instance)
        {
            {
                var curse = __instance.transform.FindChild("Scroll View/Viewport/Content/curseBuffs").gameObject;
                var customBuffs = Instantiate(curse, __instance.transform.FindChild("Scroll View/Viewport/Content"));
                customBuffs.name = "customBuffs";
                customBuffs.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "二创词条";
                var list = new Il2CppSystem.Collections.Generic.List<AlmanacCardUI>();
                int cnt = 0; // 当前是第几次循环
                foreach (var ((buffType, id), (desc, icon, zt)) in CustomCore.CustomBuffs)
                {
                    var obj = new Il2CppSystem.Object();
                    switch (buffType)
                    {
                        case BuffType.UnlockPlant:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<TravelUnlocks>(id);
                            break;
                        case BuffType.AdvancedBuff:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            break;
                        case BuffType.UltimateBuff:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<UltiBuff>(id);
                            break;
                        case BuffType.Debuff:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<TravelDebuff>(id);
                            break;
                        case BuffType.InvestmentBuff:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<InvestBuff>(id);
                            break;
                    }
                    if (!Lawnf.HasTravelBuff(obj) && !__instance.editMode && !AlmanacBuffMenu.lookBuff) continue;
                    var cardInfo = new AlmanacBuffMenu.CardInfo
                    {
                        buff = obj,
                        description = desc,
                        isZombie = buffType == BuffType.Debuff
                    };
                    if (buffType == BuffType.Debuff)
                        cardInfo.zombieType = zt;
                    else
                        cardInfo.plantType = icon;
                    __instance.CreateCardUI(cardInfo, list);
                    var hasBuff = Lawnf.HasTravelBuff(obj) ? 0f : 1f;
                    list[cnt].GetComponent<Image>().color = new Color(hasBuff, 1f, hasBuff, 1f);
                    cnt++;
                }
                foreach (var cardUI in list)
                    cardUI.gameObject.SetActive(false);

                Action action = () =>
                {
                    __instance.SetAllCards(false);
                    foreach (var cardUI in list)
                        cardUI.gameObject.SetActive(true);
                };

                UnityEvent unityEvent = new UnityEvent();
                unityEvent.AddListener(action);
                customBuffs.GetComponent<UIButton>().clickEvent = unityEvent;
            }

            {
                foreach (var ((buffType, id), (almanacType, icon, zt)) in CustomCore.CustomAlmanacBuffType)
                {
                    var obj = new Il2CppSystem.Object();
                    var list = new Il2CppSystem.Collections.Generic.List<AlmanacCardUI>();
                    switch (almanacType)
                    {
                        case AlmanacBuffType.WeakUltimate:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.weakUltiBuffs;
                            break;
                        case AlmanacBuffType.StrongUltimate:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<UltiBuff>(id);
                            list = __instance.strongUltiBuffs;
                            break;
                        case AlmanacBuffType.General:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.generalBuffs;
                            break;
                        case AlmanacBuffType.Random:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.randomBuffs;
                            break;
                        case AlmanacBuffType.Curse:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.curseBuffs;
                            break;
                        case AlmanacBuffType.Rogue:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.rogueBuffs;
                            break;
                        case AlmanacBuffType.Combo:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.comboBuffs;
                            break;
                        case AlmanacBuffType.Tiny:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.tinyBuffs;
                            break;
                        case AlmanacBuffType.Zombie:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.zombieBuffs;
                            break;
                        case AlmanacBuffType.Shooting:
                            obj = Il2CppExtensions.BoxEnumToIl2Object<AdvBuff>(id);
                            list = __instance.shootingBuffs;
                            break;
                    }
                    if (!Lawnf.HasTravelBuff(obj) && !__instance.editMode && !AlmanacBuffMenu.lookBuff) continue;
                    var cardInfo = new AlmanacBuffMenu.CardInfo
                    {
                        buff = obj,
                        description = TravelMgr.Instance.GetText(obj),
                        plantType = icon
                    };
                    __instance.CreateCardUI(cardInfo, list);
                }
            }
        }

        [HarmonyPatch(nameof(AlmanacBuffMenu.OnCardClick))]
        [HarmonyPostfix]
        public static void PostOnCardClick(AlmanacBuffMenu __instance, AlmanacCardUI card)
        {
            AlmanacBuffMenu.CardInfo cardInfo = __instance.cardInfos[card];
            var (type, id) = TravelExtensions.GetTypeAndID(cardInfo.buff);
            if (CustomCore.CustomBuffsBg.ContainsKey((type, id)))
            {
                if (CustomCore.CustomBuffsBg[(type, id)].BgType == BuffBgType.Day)
                    __instance.windowBackground.sprite = __instance.day;
                else if (CustomCore.CustomBuffsBg[(type, id)].BgType == BuffBgType.Night)
                    __instance.windowBackground.sprite = __instance.night;
                else if (CustomCore.CustomBuffsBg[(type, id)].BgType == BuffBgType.Night)
                    __instance.windowBackground.sprite = __instance.pool;
            }
        }
    }

    [HarmonyPatch(typeof(TravelMgr))]
    public static class TravelMgrPatch
    {
        [HarmonyPatch(nameof(TravelMgr.OnBoardStart))]
        [HarmonyPostfix]
        public static void PostOnBoardStart(TravelMgr __instance)
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

        [HarmonyPatch(nameof(TravelMgr.GetAdvancedBuffPool))]
        [HarmonyPostfix]
        public static void PostGetAdvancedBuffPool(ref Il2CppSystem.Collections.Generic.List<AdvBuff> __result)
        {
            foreach (var (key, value) in CustomCore.CustomAdvancedBuffs)
            {
                if (value.Item3.Invoke() && !TravelMgr.Instance.data.advBuffs.Contains((AdvBuff)key))
                    __result.Add((AdvBuff)key);
            }

            foreach (var (key, list) in CustomCore.CustomPlantInfo)
            {
                if (Lawnf.GetPlantCount(key, Board.Instance) > 0 && __result.Contains((AdvBuff)key))
                {
                    foreach (var (buffType, id) in list)
                        if (buffType == BuffType.AdvancedBuff && __result.Contains((AdvBuff)id))
                            for (int i = 0; i < __result.Count / 8; i++)
                                __result.Add((AdvBuff)id);
                }
            }
        }

        [HarmonyPatch(nameof(TravelMgr.GetDebuffPool))]
        [HarmonyPostfix]
        public static void GetDebuffPool(ref Il2CppSystem.Collections.Generic.List<TravelDebuff> __result)
        {
            foreach (var (key, value) in CustomCore.CustomDebuffs)
            {
                if (value.Item3.Invoke() && !TravelMgr.Instance.data.travelDebuffs.Contains((TravelDebuff)key))
                {
                    __result.Add((TravelDebuff)key);
                }
            }
        }

        [HarmonyPatch(nameof(TravelMgr.GetText))]
        [HarmonyPostfix]
        public static void PostGetText(Il2CppSystem.Object buff, ref string __result)
        {
            var (type, id) = TravelExtensions.GetTypeAndID(buff);
            if (CustomCore.CustomBuffText.ContainsKey((type, id)))
                __result = CustomCore.CustomBuffText[(type, id)];
        }
    }

    [HarmonyPatch(typeof(TravelPackage))]
    public static class TravelPackagePatch
    {
        [HarmonyPatch(nameof(TravelPackage.Init))]
        [HarmonyPostfix]
        public static void PostInit(TravelPackage __instance)
        {
            foreach (var (key, value) in CustomCore.CustomDebuffs)
            {
                if (value.Item3.Invoke() && !TravelMgr.Instance.data.travelDebuffs.Contains((TravelDebuff)key))
                {
                    __instance.Debuffs.Add((TravelDebuff)key);
                }
            }
        }
    }

    [HarmonyPatch(typeof(TravelHelper))]
    public static class TravelHelperPatch
    {
        [HarmonyPatch(nameof(TravelHelper.GetAllUltimatePlantTypes))]
        [HarmonyPostfix]
        public static void PostGetAllUltimatePlantTypes(ref Il2CppSystem.Collections.Generic.List<PlantType> __result, ref bool isStrongUltimate)
        {
            if (isStrongUltimate)
            {
                foreach (var (pt, _) in CustomCore.CustomStrongUltimatePlants)
                    __result.Add(pt);
            }
            else
            {
                foreach (var pt in CustomCore.CustomUltimatePlants)
                    if (!CustomCore.CustomStrongUltimatePlants.ContainsKey(pt)) // 排除强究
                        __result.Add(pt);
            }
        }
    }

    [HarmonyPatch(typeof(TravelLookMenu))]
    public static class TravelLookMenuPatch
    {
        [HarmonyPatch(nameof(TravelLookMenu.GetAdvBuffs))]
        [HarmonyPostfix]
        public static void PostGetAdvBuffs(TravelLookMenu __instance, ref Il2CppSystem.Collections.Generic.List<AdvBuff> __result)
        {
            if (CustomCore.CustomAdvancedBuffs.Count <= 0)
                return;
            foreach (var (id, _) in CustomCore.CustomAdvancedBuffs)
                if (__instance.showAll)
                    __result.Add((AdvBuff)id);
        }

        [HarmonyPatch(nameof(TravelLookMenu.GetDebuffs))]
        [HarmonyPostfix]
        public static void PostGetDebuffs(TravelLookMenu __instance, ref Il2CppSystem.Collections.Generic.List<TravelDebuff> __result)
        {
            if (CustomCore.CustomDebuffs.Count <= 0)
                return;
            foreach (var (id, _) in CustomCore.CustomDebuffs)
                if (__instance.showAll)
                    __result.Add((TravelDebuff)id);
        }

        [HarmonyPatch(nameof(TravelLookMenu.GetUltiBuffs))]
        [HarmonyPostfix]
        public static void PostGetUltimateBuffs(TravelLookMenu __instance,
            ref Il2CppSystem.ValueTuple<Il2CppSystem.Collections.Generic.List<UltiBuff>, Il2CppSystem.Collections.Generic.List<UltiBuff>>
            __result)
        {
            if (CustomCore.CustomUltimateBuffs.Count <= 0)
                return;
            foreach (var (id, _) in CustomCore.CustomUltimateBuffs)
            {
                if (__instance.showAll)
                    __result.Item1.Add((UltiBuff)id);
                if (__instance.showAll)
                    __result.Item2.Add((UltiBuff)id);
            }
        }
    }

    [HarmonyPatch(typeof(TravelStore))]
    public static class TravelStorePatch
    {
        [HarmonyPatch(nameof(TravelStore.SetCost))]
        [HarmonyPostfix]
        public static void PostRefreshBuff(ref TravelStoreWindow window)
        {
            var (buffType, buffIndex) = window.TryGetTypeAndID();
            if (CustomCore.CustomBuffCost.ContainsKey((buffType, buffIndex)))
            {
                window.cost = CustomCore.CustomBuffCost[(buffType, buffIndex)];
                if (Lawnf.TravelCurse() || TravelMgr.Instance.data.Invest)
                {
                    if (window.cost > 15000)
                    {
                        window.UpdateButtonText("过于昂贵", UnityEngine.Color.red);
                        window.canBuy = false;
                        return;
                    }
                }
                window.UpdateButtonText($"{window.cost}分", UnityEngine.Color.yellow);
                window.canBuy = true;
            }
        }
    }

    [HarmonyPatch(typeof(TravelStoreWindow))]
    public static class TravelStoreWindowPatch
    {
        [HarmonyPatch(nameof(TravelStoreWindow.SetType))]
        [HarmonyPostfix]
        public static void Postfix(TravelStoreWindow __instance, Il2CppSystem.Object buff)
        {
            var (buffType, index) = __instance.GeneralSet(buff);
            if (CustomCore.CustomBuffsBg.ContainsKey((buffType, index)))
            {
                __instance.SetBackground(CustomCore.CustomBuffsBg[(buffType, index)]);
            }
            if (CustomCore.CustomBuffIcon.ContainsKey((buffType, index)))
            {
                if (__instance.show != null)
                    Destroy(__instance.show);
                __instance.SetPlant(CustomCore.CustomBuffIcon[(buffType, index)]);
            }
        }
    }

    [HarmonyPatch(typeof(TypeMgr))]
    public static class TypeMgrPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TypeMgr.BigNut))]
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
        [HarmonyPatch(nameof(TypeMgr.IsDriverZombie))]
        public static bool PreDriverZombie(ref ZombieType theZombieType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.DriverZombie.Contains(theZombieType))
            {
                __result = true;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TypeMgr.BigZombie))]
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
        [HarmonyPatch(nameof(TypeMgr.DoubleBoxPlants))]
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
        [HarmonyPatch(nameof(TypeMgr.FlyingPlants))]
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
        [HarmonyPatch(nameof(TypeMgr.GetPlantTag))]
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
        [HarmonyPatch(nameof(TypeMgr.IsCaltrop))]
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
        [HarmonyPatch(nameof(TypeMgr.IsFirePlant))]
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
        [HarmonyPatch(nameof(TypeMgr.IsIcePlant))]
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
        [HarmonyPatch(nameof(TypeMgr.IsMagnetPlants))]
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
        [HarmonyPatch(nameof(TypeMgr.IsNut))]
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
        [HarmonyPatch(nameof(TypeMgr.IsPlantern))]
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
        [HarmonyPatch(nameof(TypeMgr.IsPot))]
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
        [HarmonyPatch(nameof(TypeMgr.IsPotatoMine))]
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
        [HarmonyPatch(nameof(TypeMgr.IsPuff))]
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
        [HarmonyPatch(nameof(TypeMgr.IsPumpkin))]
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
        [HarmonyPatch(nameof(TypeMgr.IsSmallRangeLantern))]
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
        [HarmonyPatch(nameof(TypeMgr.IsPurplePlant))]
        public static bool PreIsPurplePlant(ref PlantType theSeedType, ref bool __result)
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
        [HarmonyPatch(nameof(TypeMgr.IsSpickRock))]
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
        [HarmonyPatch(nameof(TypeMgr.IsTallNut))]
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
        [HarmonyPatch(nameof(TypeMgr.IsTangkelp))]
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
        [HarmonyPatch(nameof(TypeMgr.IsWaterPlant))]
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
        [HarmonyPatch(nameof(TypeMgr.UmbrellaPlants))]
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

    [HarmonyPatch(typeof(CustomMenu))]
    public static class CustomMenuPatch
    {
        [HarmonyPatch(nameof(CustomMenu.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(CustomMenu __instance)
        {
            if (GameAPP.canvas.IsObjExist()&& GameAPP.canvas.childCount > 0 && GameAPP.canvas.GetChild(0).name == "ChallengeMenu(Clone)" && 
                GameAPP.canvas.GetChild(0).FindChild("Levels").IsObjExist())
            {
                var child = GameAPP.canvas.GetChild(0).FindChild("Levels").FindChild("FirstBtns").FindChild("CustomLevels");
                if (child.IsObjExist())
                    child.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
                Action action = () =>
                {
                    if (GameAPP.canvas.IsObjExist() && GameAPP.canvas.GetChild(0).FindChild("Levels").IsObjExist())
                    {
                        var child = GameAPP.canvas.GetChild(0).FindChild("Levels").FindChild("FirstBtns").FindChild("CustomLevels");
                        if (child.IsObjExist())
                            child.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
                    }
                };
                __instance.transform.FindChild("LowerButtons/Exit").GetComponent<UIButton>().clickEvent.AddListener(action);
            }
        }
    }

    [HarmonyPatch(typeof(UIMgr))]
    public static class UIMgrPatch
    {
        private static Vector3 CalculatePosition(int col, int row)
        {
            return new Vector3(-300f + col * 150, 160f - row * 130);
        }

        [HarmonyPatch(nameof(UIMgr.EnterChallengeMenu))]
        [HarmonyPostfix]
        public static void PostEnterChallengeMenu()
        {
            GameAPP.Instance.StartCoroutine(init());
            IEnumerator init()
            {
                yield return null;
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
                    window.GetComponent<FirstBtns>().originPosition = custom.transform.localPosition;
                    UnityEngine.Object.Destroy(pageSample);
                    UnityEngine.Object.Destroy(levelSample);
                }
                //foreach (var item in CustomCore.CustomLevels)
                //    LevelManager.registry.RegisterPredefinedLevel(item.LevelData);
            }
        }

        [HarmonyPatch(nameof(UIMgr.EnterGame))]
        [HarmonyPrefix]
        public static bool PreEnterGame(ref LevelType levelType, ref int levelNumber, ref int id, ref string name)
        {
            if ((int)levelType is not 66) return true;
            var levelData = CustomCore.CustomLevels[levelNumber];

            // 清理UI资源
            SynergyManager.Instance.ClearAllSynergies();
            EventManager.ClearAllEvents();
            GameAPP.UIManager.PopAll();

            // 重置相机
            CamaraFollowMouse.Instance.ResetCamera();

            // 设置游戏速度
            Time.timeScale = GameAPP.config.gameSpeed;

            // 设置当前关卡信息
            GameAPP.theBoardType = levelType;
            GameAPP.theBoardLevel = levelNumber;

            RogueManager.Instance.Clear();
            // 清理现有的Travel管理器
            if (TravelMgr.Instance != null)
            {
                UnityEngine.Object.Destroy(TravelMgr.Instance);
                TravelMgr._instance = null;
            }

            // 创建游戏板
            GameObject boardGO = new("Board");
            GameAPP.board = boardGO;
            Board board = boardGO.AddComponent<Board>();
            var bt = levelData.BoardTag;
            bt.disableSelectCard = !levelData.NeedSelectCard;
            board.boardTag = bt;
            board.rowNum = levelData.RowCount;
            board.theMaxWave = levelData.WaveCount();
            board.theSun = levelData.Sun();
            board.config.zombieHealthMultiplier = levelData.ZombieHealthRate();
            board.seedPool = levelData.SeedRainPlantTypes().ToIl2CppList();
            levelData.PostBoard(board);
            // 加载并实例化地图
            var map = MapData_cs.GetMap(levelData.SceneType, board);

            InitZombieList.InitZombie(levelType, levelNumber);

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

            for (int i = 0; i < board.rowNum; i++)
            {
                var floor = map.transform.FindChild($"floor{i}");
                board.plane.Add(floor);
                if (board.boardTag.isRoof)
                {
                    var floor_roof = new GameObject("floor_roof");
                    floor_roof.transform.SetParent(floor);
                    floor_roof.transform.localPosition = new Vector3(0f, 0f, 0f);
                }
                var iceRoad = Instantiate(GamePrefabs.IceRoad, new Vector3(19.7f, 0.8f, 0f), Quaternion.identity, floor).GetComponent<IceRoad>();
                iceRoad.theRow = i;
                iceRoad.roadStartX = iceRoad.x = 19.7f;
                iceRoad.transform.localPosition = new Vector3(19.7f, 0.8f, 0f);
                board.iceRoads.Add(iceRoad);
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

    [HarmonyPatch(typeof(SynergyDisplay))]
    public static class SynergyDisplayPatch
    {
        [HarmonyPatch(nameof(SynergyDisplay.Start))]
        [HarmonyPrefix]
        public static void Prefix()
        {
            if (Utils.IsCustomLevel(out var _))
            {
                {
                    var go = SynergyManager.Instance.gameObject;
                    Destroy(SynergyManager.Instance);
                    SynergyManager._instance = go.AddComponent<SynergyManager>();
                }
                {
                    var go = TravelMgr.Instance.gameObject;
                    Destroy(TravelMgr.Instance);
                    TravelMgr._instance = go.AddComponent<TravelMgr>();
                }
            }
        }
    }

    [HarmonyPatch(typeof(SaveInfo))]
    public static class SaveInfoPatch
    {
        [HarmonyPatch(nameof(SaveInfo.SaveSurvivalData), new Type[] { typeof(SurvivalData), typeof(int), typeof(int) })]
        [HarmonyPostfix]
        public static void PostSaveSurvivalDataByButton(ref int level, ref int id)
        {
            PatchMgr.SaveEndlessData(level, id);
        }

        [HarmonyPatch(nameof(SaveInfo.SaveSurvivalData), new Type[] { typeof(int), typeof(bool), typeof(int), typeof(string) })]
        [HarmonyPostfix]
        public static void PostSaveSurvivalDataByAuto(ref int level, ref int id)
        {
            PatchMgr.SaveEndlessData(level, id);
        }
    }

    [HarmonyPatch(typeof(SaveMgr))]
    public static class SaveMgrPatch
    {
        [HarmonyPatch(nameof(SaveMgr.SaveBoard))]
        [HarmonyPostfix]
        public static void PostSaveBoard(SaveMgr __instance, ref int level, ref int id)
        {
            PatchMgr.SaveEndlessData(level, id);
        }

        [HarmonyPatch(nameof(SaveMgr.LoadBoard))]
        [HarmonyPostfix]
        public static void PostLoadBoard(SaveMgr __instance, ref int level, ref int id)
        {
            if (TravelMgr.Instance == null || SaveInfo.Instance == null)
                return;
            var idGet = SaveInfo.Instance.GetData("endlessID");
            if (idGet is null)
                return;
            var idG = (int)idGet;
            PatchMgr.LoadEndlessData(level, id, idG);
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

    [HarmonyPatch(typeof(AlmanacZombieMenu))]
    public class AlmanacZombieMenuPatch
    {
        [HarmonyPatch(nameof(AlmanacZombieMenu.Start))]
        [HarmonyPostfix]
        public static void Postfix(AlmanacZombieMenu __instance)
        {
            if (__instance.transform.Find("LoolAll_Other") == null)
            {
                var customButton = Instantiate(__instance.transform.Find("LookAll_1").gameObject, __instance.transform);
                customButton.transform.localPosition = new Vector2(10, 0);
                customButton.name = "LoolAll_Other";
                customButton.transform.localPosition = new Vector2(440, -499);
                // 修改按钮文本
                foreach (var text in customButton.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text != null)
                        text.text = "二创僵尸";
                }

                var uiButton = customButton.GetComponent<UIButton>();
                UnityEvent unityEvent = new UnityEvent();
                Action action = () =>
                {
                    Func<ZombieType, bool> func = (zt) => !Enum.IsDefined<ZombieType>(zt);
                    __instance.ShowZombieCards(func);
                };
                unityEvent.AddListener(action);
                customButton.GetComponent<UIButton>().clickEvent = unityEvent;
            }
        }
    }

    [HarmonyPatch(typeof(Entity))]
    public static class EntityPatch
    {
        [HarmonyPatch(nameof(Entity.GetSpriteRenderers))]
        [HarmonyPrefix]
        public static bool PreGetSpriteRenderers(Entity __instance)
        {
            if (__instance.TryGetComponent<SaveMaterial>(out var _))
            {
                foreach (var child in Core.Lawnf.GetChilds(__instance.transform))
                    if (child.TryGetComponent<SpriteRenderer>(out var renderer) && child.name != "Shadow")
                        __instance.spriteRenderers.Add(renderer);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PatchMgr))]
    public static class PatchMgrPatch
    {
        [HarmonyPatch(nameof(PatchMgr.ShowCards))]
        [HarmonyFinalizer]
        public static Exception ShowFinalizer()
        {
            return null;
        }
    }

    public static class PatchMgr
    {
        public static CustomSkinData SkinData = new();
        public static bool Load = false;

        public struct CustomSkinData
        {
            public Dictionary<PlantType, int>? PlantSkinDic { get; set; } = null;
            public Dictionary<PlantType, Il2CppSystem.Collections.Generic.List<GameObject>>? _plantPrefabs { get; set; } = null;
            public Dictionary<PlantType, Il2CppSystem.Collections.Generic.List<GameObject>>? _plantPreviews { get; set; } = null;
            public CustomSkinData()
            {
                PlantSkinDic = null;
                _plantPrefabs = null;
                _plantPreviews = null;
            }
        }

        #region 无尽
        public static void SaveEndlessData(int level, int id)
        {
            SaveEndlessBuffArray(level, id);
            SaveDataArray(level, id);
        }

        public static void SaveEndlessBuffArray(int level, int id)
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
            String originalPath = SaveInfo.Instance.GetPath(level, id);
            String? directoryPath = Path.GetDirectoryName(originalPath);
            if (directoryPath is null)
                return;
            String fileName = Path.GetFileName(originalPath);
            String filePath = Path.Combine(directoryPath, $"{fileName}.extra.json");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            if (!File.Exists(filePath))
                File.Create(filePath).Dispose();
            File.WriteAllText(filePath, json);
        }

        public static void SaveDataArray(int level, int id)
        {
            //var plantDatas = new List<CustomEndlessPlantData>();
            //foreach (var plant in Lawnf.GetAllPlants())
            //{
            //    foreach (var comp in plant.GetComponents<Component>())
            //        if (CustomCore.CustomEndlessSaveData.ContainsKey(comp.GetIl2CppType()))
            //        {
            //            plantDatas.Add(new CustomEndlessPlantData()
            //            {
            //                pt = plant.thePlantType,
            //                col = plant.thePlantColumn,
            //                row = plant.thePlantRow,
            //                value = GetValueByName(comp, CustomCore.CustomEndlessSaveData[comp.GetIl2CppType()])
            //            });
            //        }
            //}
        }

        public static object? GetValueByName(Component comp, string name)
        {
            if (comp == null) return null;

            var prop = comp.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (prop != null && prop.CanRead)
                return prop?.GetValue(comp);

            var field = comp.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null)
                return field.GetValue(comp);

            return null;
        }

        public static void SetValueByName(Component comp, string name, object? val)
        {
            if (comp == null) return;

            var prop = comp.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (prop != null && prop.CanWrite)
            {
                prop?.SetValue(comp, val);
                return;
            }

            var field = comp.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null)
            {
                field.SetValue(comp, val);
                return;
            }
        }

        public static void LoadEndlessData(int level, int id, int idG)
        {
            LoadEndlessBuffArray(level, idG);
        }

        public static void LoadEndlessBuffArray(int level, int id)
        {
            String originalPath = SaveInfo.Instance.GetPath(level, id);
            String? directoryPath = Path.GetDirectoryName(originalPath);
            if (directoryPath is null)
                return;
            String fileName = Path.GetFileName(originalPath);
            String filePath = Path.Combine(directoryPath, $"{fileName}.extra.json");
            if (!File.Exists(filePath))
                File.Create(filePath).Dispose();
            String text = File.ReadAllText(filePath);
            if (text == null || text == "")
            {
                text = JsonSerializer.Serialize<int[]>(new int[CustomCore.CustomAdvancedBuffs.Count]);
            }
            int[]? array = JsonSerializer.Deserialize<int[]>(text);
            if (array is null)
                return;
            TravelMgr.Instance.SetData("CustomBuffsLevel", array);
            TravelMgr.Instance.SetData("LoadByEndless", true);
            SaveInfo.Instance.SetData("endlessID", null);
        }
        #endregion

        public static void OnChangeSkin(PlantType almanacType, int index)
        {
            if (CustomCore.CustomBulletSkinReplace.ContainsKey((almanacType, index)))
            {
                var list = CustomCore.CustomBulletSkinReplace[(almanacType, index)];
                foreach (var (origin, replace) in list)
                    CustomCore.CustomBulletsSkinID[(almanacType, origin)] = replace;
            }
            foreach (var ((pt, i), list) in CustomCore.CustomBulletSkinReplace)
            {
                foreach (var (ori, _) in list)
                {
                    if (GameAPP.resourcesManager.plantSkinDic.ContainsKey(pt) && GameAPP.resourcesManager.plantSkinDic[pt] != i)
                    {
                        CustomCore.CustomBulletsSkinID[(almanacType, ori)] = new List<BulletType> { ori };
                    }
                }
            }
            SetEnableSkin();
        }

        public static void UpdateSkin()
        {
            foreach (var ((pt, i), list) in CustomCore.CustomBulletSkinReplace)
            {
                foreach (var (ori, rep) in list)
                {
                    if (GameAPP.resourcesManager.plantSkinDic.ContainsKey(pt))
                    {
                        if (GameAPP.resourcesManager.plantSkinDic[pt] != i)
                            CustomCore.CustomBulletsSkinID[(pt, ori)] = new List<BulletType> { ori };
                        else
                            CustomCore.CustomBulletsSkinID[(pt, ori)] = rep;
                    }
                }
            }
            SetEnableSkin();
        }

        public static void SetEnableSkin()
        {
            var enableList = new List<PlantType>();
            foreach (var (type, list) in CustomCore.CustomPlantSkinIndex)
            {
                foreach (var index in list)
                    if (GameAPP.resourcesManager.plantSkinDic.ContainsKey(type) && GameAPP.resourcesManager.plantSkinDic[type] == index)
                        enableList.Add(type);
            }
            var newDic = new Dictionary<PlantType, bool>();
            foreach (var (type, _) in CustomCore.CustomPlantsSkin)
            {
                if (enableList.Contains(type))
                {
                    if (newDic.ContainsKey(type))
                        newDic[type] = true;
                    else
                        newDic.Add(type, true);
                }
                else
                {
                    if (newDic.ContainsKey(type))
                        newDic[type] = false;
                    else
                        newDic.Add(type, false);
                }
            }
            CustomCore.EnableSkin = newDic;
        }

        public static Dictionary<TKey, TValue>? Clone<TKey, TValue>(this Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue> dic1) where TKey : notnull
        {
            var dic2 = new Dictionary<TKey, TValue>();
            foreach (var (key, value) in dic1)
                dic2.Add(key, value);
            return dic2;
        }

        public static Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>? Clone<TKey, TValue>(this Dictionary<TKey, TValue> dic1) where TKey : notnull
        {
            var dic2 = new Il2CppSystem.Collections.Generic.Dictionary<TKey, TValue>();
            foreach (var (key, value) in dic1)
                dic2.Add(key, value);
            return dic2;
        }

        public static void InitWithValue<T>(this List<T> list, T value)
        {
            for (int i = list.Count - 1; i >= 0; i--)
                list[i] = value;
        }

        public static void InitWithValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TValue value) where TKey : notnull
        {
            foreach (var key in dic.Keys.ToList())  // 复制键集合
            {
                dic[key] = value;
            }
        }

        #region 注册皮肤
        public static IEnumerator RegisterSkin()
        {
            foreach (var item in CustomCore.CustomPlantsSkin)
            {
                var plantType = item.Key;
                if (!CustomCore.CustomPlantsSkinActive[plantType])
                {
                    if (!GameAPP.resourcesManager.plantSkinDic.TryGetValue(plantType, out var _))
                        GameAPP.resourcesManager.plantSkinDic.Add(plantType, 0);
                    foreach (var it in item.Value)
                    {
                        var prefab = it.Prefab;
                        var preview = it.Preview;

                        if (prefab != null)
                        {
                            if (GameAPP.resourcesManager._plantPrefabs.ContainsKey(plantType))
                                GameAPP.resourcesManager._plantPrefabs[plantType].Add(prefab);
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
                                GameAPP.resourcesManager._plantPreviews[plantType].Add(preview);
                            else
                            {
                                Il2CppSystem.Collections.Generic.List<GameObject> list = new();
                                list.Add(GameAPP.resourcesManager.plantPreviews[plantType]);
                                list.Add(preview);
                                GameAPP.resourcesManager._plantPreviews.Add(plantType, list);
                            }
                        }

                        {
                            var index_prefab = GameAPP.resourcesManager._plantPrefabs[plantType].IndexOf(prefab);
                            var index_preview = GameAPP.resourcesManager._plantPreviews[plantType].IndexOf(preview);
                            if (index_prefab == -1 || index_preview == -1) continue;
                            if (index_prefab != index_preview) continue;
                            if (CustomCore.CustomPlantSkinIndex.ContainsKey(plantType))
                                CustomCore.CustomPlantSkinIndex[plantType].Add(index_prefab);
                            else
                                CustomCore.CustomPlantSkinIndex.Add(plantType, new List<int> { index_prefab });
                        }

                        CustomCore.CustomPlantsSkinActive[plantType] = true;

                        // 注册皮肤子弹
                        {
                            var index = GameAPP.resourcesManager._plantPrefabs[plantType].IndexOf(prefab);
                            if (index == -1) continue;
                            if (it.BulletList == null)
                                continue;
                            foreach (var (bulletID, list) in it.BulletList)
                            {
                                if (bulletID == (BulletType)(-1)) continue;
                                foreach (var bullet in list)
                                {
                                    if (bullet != null)
                                    {
                                        if (!CustomCore.CustomBulletSkinReplace.ContainsKey((plantType, index)))
                                            CustomCore.CustomBulletSkinReplace.Add((plantType, index), new Dictionary<BulletType, List<BulletType>>
                                        {
                                            { bulletID, CustomCore.CustomBulletsSkinID[(plantType, bulletID)] }
                                        });
                                        else
                                        {
                                            if (CustomCore.CustomBulletSkinReplace[(plantType, index)].ContainsKey(bulletID))
                                            {
                                                for (int i = CustomCore.CustomBulletsSkinID[(plantType, bulletID)].Count - 1; i >= 0; i--)
                                                {
                                                    var itb = CustomCore.CustomBulletsSkinID[(plantType, bulletID)][i];
                                                    CustomCore.CustomBulletSkinReplace[(plantType, index)][bulletID].Add(itb);
                                                }
                                            }
                                            else
                                            {
                                                CustomCore.CustomBulletSkinReplace[(plantType, index)].Add(bulletID, CustomCore.CustomBulletsSkinID[(plantType, bulletID)]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            String? fullName = Directory.GetParent(Application.dataPath)?.FullName;
            if (fullName != null)
            {
                string skinPath = Path.Combine(fullName, "BepInEx", "plugins", "Skin");
                if (Directory.Exists(skinPath))
                {
                    var regex = new Regex(@"^skin_(\d+)(?!\d).*$", RegexOptions.IgnoreCase);
                    foreach (var path in Directory.GetFiles(skinPath))
                    {
                        var match = regex.Match(Path.GetFileNameWithoutExtension(path));
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
                        {
                            var plantType = (PlantType)id;
                            if (CustomCore.CustomPlantsSkinActive.ContainsKey(plantType) && CustomCore.CustomPlantsSkinActive[plantType]) continue;
                            var ab = AssetBundle.LoadFromFile(path);

                            var json = new SkinConfig();
                            if (ab.TryGetAsset<TextAsset>("config", out var text))
                                json = JsonSerializer.Deserialize<SkinConfig>(text.text);

                            CustomCore.LoadedSkinAssetBundle.Add(ab);
                            GameObject? prefab = null;
                            GameObject? preview = null;
                            List<(BulletType, GameObject?)> bullets = new();
                            try
                            {
                                prefab = ab.GetAsset<GameObject>("Prefab");
                                prefab.tag = "Plant";
                            }
                            catch { continue; }
                            try
                            {
                                preview = ab.GetAsset<GameObject>("Preview");
                                preview.tag = "Preview";
                            }
                            catch { continue; }

                            if (json.SaveMaterial)
                            {
                                prefab.SetSaveMaterial();
                                preview.SetSaveMaterial();
                            }

                            try
                            {
                                var bulletRegex = new Regex(@"Bullet_(\d+)");
                                foreach (var name in ab.GetAssetBundleAssetNames())
                                {
                                    var bulletMatch = bulletRegex.Match(name);
                                    if (bulletMatch.Success)
                                    {
                                        var bulletID = (BulletType)int.Parse(bulletMatch.Groups[1].Value);
                                        var bullet = ab.GetAsset<GameObject>(name);
                                        foreach (var comp in GameAPP.resourcesManager.bulletPrefabs[bulletID].GetComponents<Component>())
                                            if (!bullet.TryGetComponent(comp.GetIl2CppType(), out var cmp) && cmp == null)
                                                bullet.AddComponent(comp.GetIl2CppType());
                                        bullet.GetComponent<Bullet>().theBulletType = bulletID;
                                        bullets.Add((bulletID, bullet));
                                    }
                                }
                            }
                            catch { continue; }

                            while (!PlantDataManager.PlantData_Default.ContainsKey(plantType)) yield return new WaitForSeconds(0.1f);
                            while (!GameAPP.resourcesManager.plantPrefabs.ContainsKey(plantType)) yield return new WaitForSeconds(0.1f);
                            while (!GameAPP.resourcesManager.plantPreviews.ContainsKey(plantType)) yield return new WaitForSeconds(0.1f);

                            CustomPlantData data = new()
                            {
                                ID = id,
                                PlantData = PlantDataManager.PlantData_Default[plantType],
                                Prefab = GameAPP.resourcesManager.plantPrefabs[plantType],
                                Preview = GameAPP.resourcesManager.plantPreviews[plantType]
                            };
                            if (!GameAPP.resourcesManager.plantSkinDic.TryGetValue(plantType, out var _))
                            {
                                GameAPP.resourcesManager.plantSkinDic.Add(plantType, 0);
                            }
                            if (prefab != null)
                            {
                                foreach (var comp in GameAPP.resourcesManager.plantPrefabs[plantType].GetComponents<Component>())
                                    if (!prefab.TryGetComponent(comp.GetIl2CppType(), out var cmp) && cmp == null)
                                        prefab.AddComponent(comp.GetIl2CppType());
                                prefab.GetComponent<Plant>().thePlantType = plantType;

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
                                data.Prefab = prefab;
                            }

                            if (preview != null)
                            {
                                foreach (var comp in GameAPP.resourcesManager.plantPreviews[plantType].GetComponents<Component>())
                                    if (!preview.TryGetComponent(comp.GetIl2CppType(), out var cmp) && cmp == null)
                                        preview.AddComponent(comp.GetIl2CppType());

                                if (GameAPP.resourcesManager._plantPreviews.ContainsKey(plantType))
                                    GameAPP.resourcesManager._plantPreviews[plantType].Add(preview);
                                else
                                {
                                    Il2CppSystem.Collections.Generic.List<GameObject> list = new();
                                    list.Add(GameAPP.resourcesManager.plantPreviews[plantType]);
                                    list.Add(preview);
                                    GameAPP.resourcesManager._plantPreviews.Add(plantType, list);
                                }

                                data.Preview = preview;
                            }
                            if (CustomCore.CustomPlantsSkin.ContainsKey(plantType))
                                CustomCore.CustomPlantsSkin[plantType].Add(data);
                            else
                                CustomCore.CustomPlantsSkin.Add(plantType, new List<CustomPlantData> { data });

                            {
                                var index_prefab = GameAPP.resourcesManager._plantPrefabs[plantType].IndexOf(prefab);
                                var index_preview = GameAPP.resourcesManager._plantPreviews[plantType].IndexOf(preview);
                                if (index_prefab == -1 || index_preview == -1) continue;
                                if (index_prefab != index_preview) continue;
                                if (CustomCore.CustomPlantSkinIndex.ContainsKey(plantType))
                                    CustomCore.CustomPlantSkinIndex[plantType].Add(index_prefab);
                                else
                                    CustomCore.CustomPlantSkinIndex.Add(plantType, new List<int> { index_prefab });
                            }

                            // 注册皮肤子弹
                            {
                                var index = GameAPP.resourcesManager._plantPrefabs[plantType].IndexOf(prefab);
                                foreach (var (bulletID, bullet) in bullets)
                                {
                                    if (bullet == null) continue;
                                    var skinBulletID = (BulletType)(CustomCore.CustomBulletSkinStartID + CustomCore.RegisteredSkinBulletCount);
                                    CustomCore.RegisterCustomSkinBullet(bulletID, skinBulletID, bullet);
                                    if (bulletID != (BulletType)(-1) && bullets != null && index != -1)
                                    {
                                        if (!CustomCore.CustomBulletSkinReplace.ContainsKey((plantType, index)))
                                            CustomCore.CustomBulletSkinReplace.Add((plantType, index), new Dictionary<BulletType, List<BulletType>>
                                        {
                                            { bulletID, new List<BulletType> { skinBulletID } }
                                        });
                                        else
                                        {
                                            if (CustomCore.CustomBulletSkinReplace[(plantType, index)].ContainsKey(bulletID))
                                            {
                                                CustomCore.CustomBulletSkinReplace[(plantType, index)][bulletID].Add(skinBulletID);
                                            }
                                            else
                                            {
                                                CustomCore.CustomBulletSkinReplace[(plantType, index)].Add(bulletID, new List<BulletType> { skinBulletID });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // 读取存档的皮肤
            {
                var directory = Path.Combine(Application.persistentDataPath, "Skin");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, "skin.json");
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }
                else
                {
                    var content = File.ReadAllText(path);
                    try
                    {
                        var skinDic = JsonSerializer.Deserialize<Dictionary<PlantType, int>>(content);
                        if (skinDic != null)
                        {
                            foreach (var (key, value) in skinDic)
                            {
                                if (GameAPP.resourcesManager.plantSkinDic.ContainsKey(key))
                                {
                                    if (GameAPP.resourcesManager._plantPrefabs.ContainsKey(key) && GameAPP.resourcesManager._plantPrefabs[key].Count > value &&
                                        GameAPP.resourcesManager._plantPreviews.ContainsKey(key) && GameAPP.resourcesManager._plantPreviews[key].Count > value)
                                    {
                                        GameAPP.resourcesManager.plantPrefabs[key] = GameAPP.resourcesManager._plantPrefabs[key][value];
                                        GameAPP.resourcesManager.plantPreviews[key] = GameAPP.resourcesManager._plantPreviews[key][value];
                                        GameAPP.resourcesManager.plantSkinDic[key] = value;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            GameAPP.resourcesManager.plantPrefabs[key] = GameAPP.resourcesManager._plantPrefabs[key][0];
                                            GameAPP.resourcesManager.plantPreviews[key] = GameAPP.resourcesManager._plantPreviews[key][0];
                                            GameAPP.resourcesManager.plantSkinDic[key] = 0;
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                else
                                    continue;
                            }
                        }
                    }
                    catch (JsonException) { }
                }
            }
            UpdateSkin();
            SetEnableSkin();
            {
                if (SkinData.PlantSkinDic == null)
                    SkinData.PlantSkinDic = GameAPP.resourcesManager.plantSkinDic.Clone();
                if (SkinData._plantPrefabs == null)
                {
                    SkinData._plantPrefabs = new Dictionary<PlantType, Il2CppSystem.Collections.Generic.List<GameObject>>();
                    foreach (var (key, list) in GameAPP.resourcesManager._plantPrefabs)
                        SkinData._plantPrefabs.Add(key, list);
                }
                if (SkinData._plantPreviews == null)
                {
                    SkinData._plantPreviews = new Dictionary<PlantType, Il2CppSystem.Collections.Generic.List<GameObject>>();
                    foreach (var (key, list) in GameAPP.resourcesManager._plantPreviews)
                        SkinData._plantPreviews.Add(key, list);
                }
            }
            yield break;
        }
        #endregion
        
        public static void ShowCustomCards(MonoBehaviour mono)
        {
            mono.StartCoroutine(ShowCardCoroutine());
        }

        public static IEnumerator ShowCardCoroutine()
        {
            // 1.5s等待初始化
            yield return new WaitForSeconds(1.5f);
            ShowCards();
        }

        public static void ShowCards()
        {
            GameObject? MyColorfulCard = Utils.GetColorfulCardGameObject();
            List<PlantType> cardsOnSeedBank = new List<PlantType>();
            Dictionary<PlantType, List<bool>> cardsOnSeedBankExtra = new Dictionary<PlantType, List<bool>>();
            GameObject? seedGroup = null;
            if (Board.Instance != null && !Board.Instance.boardTag.isIZ)
                seedGroup = InGameUI.Instance.SeedBank.transform.GetChild(0).gameObject;
            else if (Board.Instance != null && Board.Instance.boardTag.isIZ)
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
            var isIZ = Board.Instance.boardTag.isIZ;
            foreach (var (pt, (list, times)) in CustomCore.CustomCards)
            {
                var repeat = isIZ ? times : times + 1;
                foreach (var cardFunc in list)
                {
                    Transform? result = cardFunc();
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
                        image.sprite = GameAPP.resourcesManager.plantPreviews[pt].GetComponent<SpriteRenderer>().sprite;
                        image.SetNativeSize();
                        // 设置背景价格
                        TempCard.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataManager.PlantData_Default[pt].cost.ToString();
                        RectTransform bgRect = TempCard.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                        //卡片
                        for (int i = 0; i < repeat; i++)
                        {
                            var packet = Instantiate(TempCard.transform.GetChild(1), TempCard.transform);
                            CardUI component = packet.GetComponent<CardUI>();
                            component.gameObject.SetActive(true);
                            //修改图片
                            Mouse.Instance.ChangeCardSprite(pt, component);
                            // 修改缩放
                            packet.GetComponent<BoxCollider2D>().enabled = true;
                            RectTransform packetRect = packet.GetChild(0).GetComponent<RectTransform>();
                            bgRect.localScale = packetRect.localScale;
                            bgRect.sizeDelta = packetRect.sizeDelta;
                            //设置数据
                            component.thePlantType = pt;
                            component.theSeedType = (int)pt;
                            component.theSeedCost = PlantDataManager.PlantData_Default[pt].cost;
                            component.fullCD = PlantDataManager.PlantData_Default[pt].cd;
                            component.CD = component.fullCD;
                            component.parent = TempCard;
                            if (cardsOnSeedBank.Contains(pt))
                                packet.gameObject.SetActive(false);
                            CheckCardState? customComponent = TempCard.GetOrAddComponent<CheckCardState>();
                            if (customComponent == null)
                                continue;
                            customComponent.card = TempCard;
                            customComponent.cardType = component.thePlantType;
                        }
                        Destroy(TempCard.transform.GetChild(1).gameObject);
                    }
                }
            }

            GameObject? MyNormalCard = Utils.GetNormalCardGameObject();
            if (MyNormalCard == null)
                return;
            foreach (var (pt, (list, times)) in CustomCore.CustomNormalCards)
            {
                var repeat = isIZ ? times : times + 1;
                foreach (var cardFunc in list)
                {
                    Transform? result = cardFunc();
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
                        image.sprite = GameAPP.resourcesManager.plantPreviews[pt].GetComponent<SpriteRenderer>().sprite;
                        image.SetNativeSize();
                        // 设置背景价格
                        TempCard.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataManager.PlantData_Default[pt].cost.ToString();
                        RectTransform bgRect = TempCard.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                        for (int i = 0; i < repeat; i++)
                        {
                            //卡片
                            var packet = Instantiate(TempCard.transform.GetChild(2), TempCard.transform);
                            var packet1 = Instantiate(TempCard.transform.GetChild(1), TempCard.transform);
                            CardUI component = packet.GetComponent<CardUI>(); // 主卡
                            component.gameObject.SetActive(true);
                            CardUI component1 = packet1.GetComponent<CardUI>(); // 副卡
                            component1.gameObject.SetActive(true);
                            //修改图片
                            Mouse.Instance.ChangeCardSprite(pt, component);
                            Mouse.Instance.ChangeCardSprite(pt, component1);
                            // 修改缩放
                            packet.GetComponent<BoxCollider2D>().enabled = true;
                            packet1.GetComponent<BoxCollider2D>().enabled = true;
                            RectTransform packetRect = packet.GetChild(0).GetComponent<RectTransform>();
                            bgRect.localScale = packetRect.localScale;
                            bgRect.sizeDelta = packetRect.sizeDelta;
                            //设置数据
                            component.thePlantType = pt;
                            component.theSeedType = (int)pt;
                            component.theSeedCost = PlantDataManager.PlantData_Default[pt].cost;
                            component.fullCD = PlantDataManager.PlantData_Default[pt].cd;
                            //设置副卡数据
                            component1.thePlantType = pt;
                            component1.theSeedType = (int)pt;
                            component1.theSeedCost = PlantDataManager.PlantData_Default[pt].cost * 2;
                            component1.fullCD = PlantDataManager.PlantData_Default[pt].cd;
                            if (cardsOnSeedBankExtra.ContainsKey(pt) && cardsOnSeedBankExtra[pt].Contains(true))
                                packet1.gameObject.SetActive(false);
                            if (cardsOnSeedBankExtra.ContainsKey(pt) && cardsOnSeedBankExtra[pt].Contains(false))
                                packet.gameObject.SetActive(false);
                            CheckCardState customComponent = TempCard.AddComponent<CheckCardState>();
                            customComponent.card = TempCard;
                            customComponent.cardType = component.thePlantType;
                            customComponent.isNormalCard = true;
                        }
                        Destroy(TempCard.transform.GetChild(1).gameObject);
                    }
                }
            }
        }

        public static void SaveSkin()
        {
            Dictionary<PlantType, int> skinDic = new();
            foreach (var (key, value) in GameAPP.resourcesManager.plantSkinDic)
            {
                if (CustomCore.CustomPlantsSkin.ContainsKey(key))
                {
                    skinDic.Add(key, value);
                }
            }

            var jsonText = JsonSerializer.Serialize(skinDic);
            var directory = Path.Combine(Application.persistentDataPath, "Skin");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "skin.json");
            if (!File.Exists(path))
                File.Create(path).Dispose();
            File.WriteAllText(path, jsonText);
        }
    }
}