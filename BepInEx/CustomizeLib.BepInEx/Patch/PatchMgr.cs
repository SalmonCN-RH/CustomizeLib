using CustomizeLib.BepInEx.ExtensionData.Basic;
using CustomizeLib.BepInEx.UnmanagedTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace CustomizeLib.BepInEx.Patch
{
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
                {
                    foreach (var item in replace)
                        CustomCore.CustomBulletsSkinID[(almanacType, origin, GameAPP.resourcesManager.plantSkinDic[almanacType])] = replace;
                }
            }
            //foreach (var ((pt, i), list) in CustomCore.CustomBulletSkinReplace)
            //{
            //    bool shouldReset = GameAPP.resourcesManager.plantSkinDic.ContainsKey(pt) && GameAPP.resourcesManager.plantSkinDic[pt] != i;
            //    if (!resetDic.TryGetValue(pt, out var val))
            //        resetDic[pt] = shouldReset;
            //    else
            //        resetDic[pt] = val && shouldReset;
            //    if (!resetDic[pt])
            //}
            //foreach (var ((pt, _), list) in CustomCore.CustomBulletSkinReplace)
            //{
            //    if (resetDic.TryGetValue(pt, out var val) && val)
            //        foreach (var (ori, _) in list)
            //            CustomCore.CustomBulletsSkinID[(almanacType, ori)] = new List<BulletType> { ori };
            //}
            SetEnableSkin();
        }

        public static void RunSkinScript(PlantType pt, int oldIndex, int newIndex)
        {
            SkinMgr.RunScript(pt, oldIndex, "OnDisable"); // 原来皮肤被禁用
            SkinMgr.RunScript(pt, newIndex, "OnEnable"); // 新皮肤被启用
        }

        public static void UpdateSkin()
        {
            foreach (var ((pt, i), list) in CustomCore.CustomBulletSkinReplace)
            {
                foreach (var (ori, rep) in list)
                {
                    if (GameAPP.resourcesManager.plantSkinDic.ContainsKey(pt))
                    {
                        if (GameAPP.resourcesManager.plantSkinDic[pt] == i)
                            CustomCore.CustomBulletsSkinID[(pt, ori, GameAPP.resourcesManager.plantSkinDic[pt])] = rep;
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
                                            { bulletID, CustomCore.CustomBulletsSkinID[(plantType, bulletID, index)] }
                                        });
                                        else
                                        {
                                            if (CustomCore.CustomBulletSkinReplace[(plantType, index)].ContainsKey(bulletID))
                                            {
                                                for (int i = CustomCore.CustomBulletsSkinID[(plantType, bulletID, index)].Count - 1; i >= 0; i--)
                                                {
                                                    var itb = CustomCore.CustomBulletsSkinID[(plantType, bulletID, index)][i];
                                                    CustomCore.CustomBulletSkinReplace[(plantType, index)][bulletID].Add(itb);
                                                }
                                            }
                                            else
                                                CustomCore.CustomBulletSkinReplace[(plantType, index)].Add(bulletID, CustomCore.CustomBulletsSkinID[(plantType, bulletID, index)]);
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

                                if (ab.TryGetAsset<TextAsset>("script", out var script))
                                {
                                    SkinMgr.AddScript(plantType, index_prefab, script.text);
                                }
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
                                    OnChangeSkin(key, value);
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

        internal static class TravelData
        {
            internal static bool load = false;

            internal static void SetBuffArr()
            {
                // advancedBuffsText
                var newAdvancedBuffsText = new Il2CppSystem.Collections.Generic.Dictionary<AdvBuff, string>();
                // 复制原来的值
                foreach (var item in TravelDictionary.advancedBuffsText)
                    newAdvancedBuffsText[item.Key] = item.Value;
                // 复制新的值
                foreach (var item in CustomCore.CustomAdvancedBuffs)
                    newAdvancedBuffsText[(AdvBuff)item.Key] = item.Value.Item2;
                // 复制引用
                TravelDictionary.advancedBuffsText = newAdvancedBuffsText;

                // AdvBuffPlantPairs
                var newAdvBuffPlantPairs = new Il2CppSystem.Collections.Generic.Dictionary<AdvBuff, PlantType>();
                foreach (var item in TravelDictionary.AdvBuffPlantPairs)
                    newAdvBuffPlantPairs[item.Key] = item.Value;
                foreach (var item in CustomCore.CustomAdvancedBuffs)
                    newAdvBuffPlantPairs[(AdvBuff)item.Key] = item.Value.Item1;
                TravelDictionary.AdvBuffPlantPairs = newAdvBuffPlantPairs;

                // ultimateBuffsText
                var newUltimateBuffsText = new Il2CppSystem.Collections.Generic.Dictionary<UltiBuff, string>();
                foreach (var item in TravelDictionary.ultimateBuffsText)
                    newUltimateBuffsText[item.Key] = item.Value;
                foreach (var item in CustomCore.CustomUltimateBuffs)
                    newUltimateBuffsText[(UltiBuff)item.Key] = item.Value.Item2;
                TravelDictionary.ultimateBuffsText = newUltimateBuffsText;

                // unlocksText
                var newUnlocksText = new Il2CppSystem.Collections.Generic.Dictionary<TravelUnlocks, string>();
                foreach (var item in TravelDictionary.unlocksText)
                    newUnlocksText[item.Key] = item.Value;
                foreach (var item in CustomCore.CustomUnlockBuffs)
                    newUnlocksText[(TravelUnlocks)item.Key] = item.Value.Item2;
                TravelDictionary.unlocksText = newUnlocksText;

                // PlantToUnlock
                var newPlantToUnlock = new Il2CppSystem.Collections.Generic.Dictionary<PlantType, TravelUnlocks>();
                foreach (var item in TravelDictionary.PlantToUnlock)
                    newPlantToUnlock[item.Key] = item.Value;
                foreach (var item in CustomCore.CustomUnlockBuffs)
                    newPlantToUnlock[item.Value.Item1] = (TravelUnlocks)item.Key;
                TravelDictionary.PlantToUnlock = newPlantToUnlock;

                // UnlockToPlant
                var newUnlockToPlant = new Il2CppSystem.Collections.Generic.Dictionary<TravelUnlocks, PlantType>();
                foreach (var item in TravelDictionary.UnlockToPlant)
                    newUnlockToPlant[item.Key] = item.Value;
                foreach (var item in CustomCore.CustomUnlockBuffs)
                    newUnlockToPlant[(TravelUnlocks)item.Key] = item.Value.Item1;
                TravelDictionary.UnlockToPlant = newUnlockToPlant;

                // debuffData
                var newDebuffData = new Il2CppSystem.Collections.Generic.Dictionary<TravelDebuff, Il2CppSystem.ValueTuple<string, ZombieType>>();
                foreach (var item in TravelDictionary.debuffData)
                    newDebuffData.DictionarySetItem(item.Key, new(item.Value.Pointer));
                foreach (var item in CustomCore.CustomDebuffs)
                    newDebuffData.DictionarySetItem((TravelDebuff)item.Key,
                        new Il2CppSystem.ValueTuple<string, ZombieType>(item.Value.Item1, item.Value.Item2));
                TravelDictionary.debuffData = newDebuffData;

                var newPlantInfo = new Il2CppSystem.Collections.Generic.Dictionary
                    <PlantType, Il2CppSystem.ValueTuple<Il2CppSystem.Nullable<PlantType>, Il2CppSystem.Object, Il2CppSystem.Object, bool>>();
                foreach (var item in TravelDictionary.PlantInfo)
                    newPlantInfo.DictionarySetItem(item.Key, item.Value);
                foreach (var (key, value) in CustomCore.CustomPlantInfos)
                {
                    Il2CppSystem.Nullable<PlantType> nullable = value.subType.HasValue ? new(value.subType.Value) : new();
                    Il2CppSystem.Object buff1 = null!;
                    if (value.buff1 != null) Il2CppExtensions.BoxEnumToIl2Object(value.buff1, value.buff1.GetType());
                    Il2CppSystem.Object buff2 = null!;
                    if (value.buff2 != null) Il2CppExtensions.BoxEnumToIl2Object(value.buff2, value.buff2.GetType());
                    var strongUltimate = value.isStrongUltimate;
                    var tuple = new Il2CppSystem.ValueTuple<Il2CppSystem.Nullable<PlantType>, Il2CppSystem.Object, Il2CppSystem.Object, bool>
                        (nullable, buff1, buff2, strongUltimate);
                    newPlantInfo.DictionarySetItem(key, tuple);
                }
            }

            internal static void RegisterTypes()
            {
                // 以备后用
            }
        }
    }
}
