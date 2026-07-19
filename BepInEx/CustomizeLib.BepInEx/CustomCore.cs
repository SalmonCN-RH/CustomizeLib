// #define DEBUG_FEATURE__ENABLE_MULTI_LEVEL_BUFF // 启用多级词条

using AlmanacData;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Core;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using CustomizeLib.BepInEx.ExtensionData.Unity;
using CustomizeLib.BepInEx.UnmanagedTools;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

#pragma warning disable
///
///Credit to likefengzi(https://github.com/likefengzi)(https://space.bilibili.com/237491236)
///
namespace CustomizeLib.BepInEx
{
    [BepInPlugin("salmon.inf75.pvzcustomization", "PVZCustomization", "3.8")]
    public class CustomCore : BasePlugin
    {
        #region TypeMgrExtra
        public static class TypeMgrExtra
        {
            public static List<PlantType> BigNut { get; set; } = [];
            public static List<ZombieType> BigZombie { get; set; } = [];
            public static List<PlantType> DoubleBoxPlants { get; set; } = [];
            public static List<ZombieType> EliteZombie { get; set; } = [];
            public static List<ZombieType> DriverZombie { get; set; } = [];
            public static List<PlantType> FlyingPlants { get; set; } = [];
            public static List<ZombieType> IsAirZombie { get; set; } = [];
            public static List<PlantType> IsCaltrop { get; set; } = [];
            public static List<PlantType> IsCustomPlant { get; set; } = [];
            public static List<PlantType> IsFirePlant { get; set; } = [];
            public static List<PlantType> IsIcePlant { get; set; } = [];
            public static List<PlantType> IsMagnetPlants { get; set; } = [];
            public static List<PlantType> IsNut { get; set; } = [];
            public static List<PlantType> IsPlantern { get; set; } = [];
            public static List<PlantType> IsPot { get; set; } = [];
            public static List<PlantType> IsPotatoMine { get; set; } = [];
            public static List<PlantType> IsPuff { get; set; } = [];
            public static List<PlantType> IsPumpkin { get; set; } = [];
            public static List<PlantType> IsSmallRangeLantern { get; set; } = [];
            public static List<PlantType> IsSpecialPlant { get; set; } = [];
            public static List<PlantType> IsSpickRock { get; set; } = [];
            public static List<PlantType> IsTallNut { get; set; } = [];
            public static List<PlantType> IsTangkelp { get; set; } = [];
            public static List<PlantType> IsWaterPlant { get; set; } = [];
            public static List<ZombieType> NotRandomBungiZombie { get; set; } = [];
            public static List<ZombieType> NotRandomZombie { get; set; } = [];
            public static List<ZombieType> UltimateZombie { get; set; } = [];
            public static List<PlantType> UmbrellaPlants { get; set; } = [];
            public static List<ZombieType> UselessHypnoZombie { get; set; } = [];
            public static List<ZombieType> WaterZombie { get; set; } = [];
            public static List<PlantType> UncrashablePlants { get; set; } = [];
            public static Dictionary<PlantType, CardLevel> LevelPlants { get; set; } = [];
        }

        /// <summary>
        /// 用于储存皮肤的数据
        /// </summary>
        public static class TypeMgrExtraSkin
        {
            public static Dictionary<PlantType, int> BigNut { get; set; } = [];
            public static Dictionary<ZombieType, int> BigZombie { get; set; } = [];
            public static Dictionary<PlantType, int> DoubleBoxPlants { get; set; } = [];
            public static Dictionary<ZombieType, int> EliteZombie { get; set; } = [];
            public static Dictionary<PlantType, int> FlyingPlants { get; set; } = [];
            public static Dictionary<ZombieType, int> IsAirZombie { get; set; } = [];
            public static Dictionary<PlantType, int> IsCaltrop { get; set; } = [];
            public static Dictionary<PlantType, int> IsCustomPlant { get; set; } = [];
            public static Dictionary<PlantType, int> IsFirePlant { get; set; } = [];
            public static Dictionary<PlantType, int> IsIcePlant { get; set; } = [];
            public static Dictionary<PlantType, int> IsMagnetPlants { get; set; } = [];
            public static Dictionary<PlantType, int> IsNut { get; set; } = [];
            public static Dictionary<PlantType, int> IsPlantern { get; set; } = [];
            public static Dictionary<PlantType, int> IsPot { get; set; } = [];
            public static Dictionary<PlantType, int> IsPotatoMine { get; set; } = [];
            public static Dictionary<PlantType, int> IsPuff { get; set; } = [];
            public static Dictionary<PlantType, int> IsPumpkin { get; set; } = [];
            public static Dictionary<PlantType, int> IsSmallRangeLantern { get; set; } = [];
            public static Dictionary<PlantType, int> IsSpecialPlant { get; set; } = [];
            public static Dictionary<PlantType, int> IsSpickRock { get; set; } = [];
            public static Dictionary<PlantType, int> IsTallNut { get; set; } = [];
            public static Dictionary<PlantType, int> IsTangkelp { get; set; } = [];
            public static Dictionary<PlantType, int> IsWaterPlant { get; set; } = [];
            public static Dictionary<ZombieType, int> NotRandomBungiZombie { get; set; } = [];
            public static Dictionary<ZombieType, int> NotRandomZombie { get; set; } = [];
            public static Dictionary<ZombieType, int> UltimateZombie { get; set; } = [];
            public static Dictionary<PlantType, int> UmbrellaPlants { get; set; } = [];
            public static Dictionary<ZombieType, int> UselessHypnoZombie { get; set; } = [];
            public static Dictionary<ZombieType, int> WaterZombie { get; set; } = [];
        }

        /// <summary>
        /// 用于储存皮肤的数据
        /// </summary>
        public static class TypeMgrExtraSkinBackup
        {
            public static Dictionary<PlantType, int> BigNut { get; set; } = [];
            public static Dictionary<ZombieType, int> BigZombie { get; set; } = [];
            public static Dictionary<PlantType, int> DoubleBoxPlants { get; set; } = [];
            public static Dictionary<ZombieType, int> EliteZombie { get; set; } = [];
            public static Dictionary<PlantType, int> FlyingPlants { get; set; } = [];
            public static Dictionary<ZombieType, int> IsAirZombie { get; set; } = [];
            public static Dictionary<PlantType, int> IsCaltrop { get; set; } = [];
            public static Dictionary<PlantType, int> IsCustomPlant { get; set; } = [];
            public static Dictionary<PlantType, int> IsFirePlant { get; set; } = [];
            public static Dictionary<PlantType, int> IsIcePlant { get; set; } = [];
            public static Dictionary<PlantType, int> IsMagnetPlants { get; set; } = [];
            public static Dictionary<PlantType, int> IsNut { get; set; } = [];
            public static Dictionary<PlantType, int> IsPlantern { get; set; } = [];
            public static Dictionary<PlantType, int> IsPot { get; set; } = [];
            public static Dictionary<PlantType, int> IsPotatoMine { get; set; } = [];
            public static Dictionary<PlantType, int> IsPuff { get; set; } = [];
            public static Dictionary<PlantType, int> IsPumpkin { get; set; } = [];
            public static Dictionary<PlantType, int> IsSmallRangeLantern { get; set; } = [];
            public static Dictionary<PlantType, int> IsSpecialPlant { get; set; } = [];
            public static Dictionary<PlantType, int> IsSpickRock { get; set; } = [];
            public static Dictionary<PlantType, int> IsTallNut { get; set; } = [];
            public static Dictionary<PlantType, int> IsTangkelp { get; set; } = [];
            public static Dictionary<PlantType, int> IsWaterPlant { get; set; } = [];
            public static Dictionary<ZombieType, int> NotRandomBungiZombie { get; set; } = [];
            public static Dictionary<ZombieType, int> NotRandomZombie { get; set; } = [];
            public static Dictionary<ZombieType, int> UltimateZombie { get; set; } = [];
            public static Dictionary<PlantType, int> UmbrellaPlants { get; set; } = [];
            public static Dictionary<ZombieType, int> UselessHypnoZombie { get; set; } = [];
            public static Dictionary<ZombieType, int> WaterZombie { get; set; } = [];
        }
        #endregion

        /// <summary>
        /// 添加植物图鉴
        /// </summary>
        /// <param name="id">植物id</param>
        /// <param name="name">植物名称</param>
        /// <param name="description">植物介绍</param>
        public static void AddPlantAlmanacStrings(int id, string name, string description)
        {
            String iName = name;
            if (!Regex.Match(name, @"\(\d+\)$").Success)
                iName = $"{name}({id})";
            PlantsAlmanac.Add((PlantType)id, new PlantAlmanac()
            {
                name = iName,
                info = description,
                plantType = (PlantType)id
            });
            var str = Regex.Replace(name, @"[\(（].*[\)）]", "");
            CustomPlantNames.Add((PlantType)id, str);
        }

        /// <summary>
        /// 添加植物图鉴
        /// </summary>
        /// <param name="plantType">植物id</param>
        /// <param name="name">植物名称</param>
        /// <param name="info">植物介绍</param>
        /// <param name="introduce">植物介绍</param>
        /// <param name="cost">植物价格</param>
        public static void AddPlantAlmanacStrings(PlantType plantType, string name, string info, string introduce, int cost)
        {
            String iName = name;
            if (!Regex.Match(name, @"\(\d+\)$").Success)
                iName = $"{name}({(int)plantType})";
            PlantsAlmanac.Add(plantType, new PlantAlmanac()
            {
                name = iName,
                info = info,
                introduce = introduce,
                cost = cost.ToString(),
                plantType = plantType
            });
            var str = Regex.Replace(name, @"[\(（].*[\)）]", "");
            CustomPlantNames.Add(plantType, str);
        }

        /// <summary>
        /// 添加僵尸图鉴
        /// </summary>
        /// <param name="id">僵尸id</param>
        /// <param name="name">僵尸名称</param>
        /// <param name="description">僵尸介绍</param>
        public static void AddZombieAlmanacStrings(int id, string name, string description)
        {
            String iName = name;
            if (!Regex.Match(name, @"\(\d+\)$").Success)
                iName = $"{name}({id})";
            ZombiesAlmanac.Add((ZombieType)id, (iName, description, null));
            var str = Regex.Replace(name, @"[\(（].*[\)）]", "");
            CustomZombieNames.Add((ZombieType)id, str);
        }

        /// <summary>
        /// 添加僵尸图鉴
        /// </summary>
        /// <param name="id">僵尸id</param>
        /// <param name="name">僵尸名称</param>
        /// <param name="description">僵尸介绍</param>
        public static void AddZombieAlmanacStrings(int id, ZombieInfo info)
        {
            if (!Regex.Match(info.name, @"\(\d+\)$").Success)
                info.name = $"{info.name}({id})";
            ZombiesAlmanac.Add((ZombieType)id, (null, null, info));
            var str = Regex.Replace(info.name, @"[\(（].*[\)）]", "");
            CustomZombieNames.Add((ZombieType)id, str);
        }

        /// <summary>
        /// 获取嵌入dll里的ab包
        /// </summary>
        /// <param name="assembly">要获取ab包的dll</param>
        /// <param name="name">名称</param>
        /// <returns>ab包</returns>
        /// <exception cref="ArgumentException"></exception>
        public static AssetBundle GetAssetBundle(Assembly assembly, string name)
        {
            try
            {
                if (LoadedAssetBundles.TryGetValue(name, out var assetBundle))
                {
                    Instance.Value.Log.LogInfo($"Successfully load AssetBundle {name}.");
                    return assetBundle;
                }
                using Stream stream =
                    assembly.GetManifestResourceStream(assembly.FullName!.Split(",")[0] + "." + name) ??
                    assembly.GetManifestResourceStream(name)!;
                using MemoryStream stream1 = new();
                stream.CopyTo(stream1);
                var ab = AssetBundle.LoadFromMemory(stream1.ToArray());
                ArgumentNullException.ThrowIfNull(ab);
                Instance.Value.Log.LogInfo($"Successfully load AssetBundle {name}.");
                LoadedAssetBundles.Add(name, ab);
                return ab;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to load {name} \n{e}");
            }
        }

        /// <summary>
        /// 获取嵌入dll(调用方)里的ab包
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>ab包</returns>
        /// <exception cref="ArgumentException"></exception>
        public static AssetBundle GetAssetBundle(string name) => GetAssetBundle(Assembly.GetCallingAssembly(), name);

        /// <summary>
        /// 获取存储在路径下的ab包
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static AssetBundle GetAssetBundleFromPath(string path, string name)
        {
            try
            {
                if (LoadedAssetBundles.TryGetValue(name, out var assetBundle))
                {
                    Instance.Value.Log.LogInfo($"Successfully load AssetBundle {name}.");
                    return assetBundle;
                }
                AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(path);
                Instance.Value.Log.LogInfo($"Successfully load AssetBundle {name}.");
                LoadedAssetBundles.Add(name, ab.assetBundle);
                return ab.assetBundle;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to load {name} \n{e}");
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="audio">音效</param>
        /// <param name="volume">ID</param>
        public static void PlaySound(AudioClip audio, float volume = 1.0f)
        {
            GameObject soundObj = new("SoundPlayer");
            AudioSource audioSource = soundObj.AddComponent<AudioSource>();
            SoundCtrl newSoundCtrl = soundObj.AddComponent<SoundCtrl>();
            // 初始化组件
            audioSource.clip = audio;

            // 设置音量（应用全局音量调整）
            audioSource.volume = volume * GameAPP.config.gameSoundVolume;
            GameAPP.music.PlayOneShot(audio, volume);
        }

        /// <summary>
        /// 注册自定义音效
        /// </summary>
        /// <param name="audio">音效</param>
        /// <param name="soundID">ID</param>
        public static void RegisterCustomSound(AudioClip audio, int soundID)
        {
            if (!CustomSounds.ContainsKey(soundID))
                CustomSounds.Add(soundID, audio);
            else
                Instance.Value.Log.LogError($"Duplicate sound id {soundID}");
        }

        #region 注册词条相关
        /// <summary>
        /// 注册自定义词条
        /// </summary>
        /// <param name="text">词条描述</param>
        /// <param name="buffType">词条类型</param>
        /// <param name="canUnlock">解锁条件</param>
        /// <param name="cost">价格</param>
        /// <param name="plantType">显示的植物类型</param>
        /// <param name="level">最大等级</param>
        /// <param name="bg">背景</param>
        /// <returns>词条ID</returns>
        public static int RegisterCustomBuff(string text, BuffType buffType, Func<bool> canUnlock, int cost,
            PlantType plantType = PlantType.Nothing, int level = 1, BuffBgType bg = default) =>
            RegisterCustomBuff(text, buffType, canUnlock, cost, PlantType.Nothing, false, plantType, level, bg);

        /// <summary>
        /// 注册自定义词条
        /// </summary>
        /// <param name="text">词条描述</param>
        /// <param name="buffType">词条类型</param>
        /// <param name="canUnlock">解锁条件</param>
        /// <param name="cost">价格</param>
        /// <param name="plantType">显示的植物类型</param>
        /// <param name="level">最大等级</param>
        /// <param name="bg">背景</param>
        /// <param name="infoType">判定类型</param>
        /// <param name="addProbability">增加植物在场时词条抽取概率</param>
        /// <returns>词条ID</returns>
        public static int RegisterCustomBuff(string text, BuffType buffType, Func<bool> canUnlock, int cost, PlantType infoType, bool addProbability,
            PlantType plantType = PlantType.Nothing, int level = 1, BuffBgType bg = default) =>
            RegisterCustomBuff(text, buffType, canUnlock, cost, plantType, level, bg, plantType: infoType, addProbability: addProbability);

        /// <summary>
        /// 注册自定义僵尸词条
        /// </summary>
        /// <param name="text">词条描述</param>
        /// <param name="zombieType">显示的僵尸类型</param>
        /// <param name="level">等级</param>
        /// <param name="bg">背景</param>
        /// <returns></returns>
        public static int RegisterCustomDebuff(string text, Func<bool> unlock = null, ZombieType zombieType = ZombieType.NormalZombie, int level = 1, BuffBgType bg = default) =>
            RegisterCustomBuff(text, BuffType.Debuff, unlock, 0, PlantType.Nothing, level: level, bgType: bg, zombieType);

        /// <summary>
        /// 注册自定义词条
        /// </summary>
        /// <param name="text">词条描述</param>
        /// <param name="buffType">词条类型(普通，强究，僵尸)</param>
        /// <param name="canUnlock">解锁条件</param>
        /// <param name="cost">词条商店花费积分</param>
        /// <param name="icon">选词条时展示植物的类型</param>
        /// <param name="level">词条最高等级</param>
        /// <param name="bgType">词条背景类型</param>
        /// <param name="zombieType">僵尸类型(仅词条类型为僵尸时使用)</param>
        /// <param name="buffID">指定词条ID(不自动分配)</param>
        /// <param name="plantType">判定的植物类型</param>
        /// <param name="addProbability">增加植物在场时词条抽取概率</param>
        /// <returns>分到的词条id</returns>
        [Obsolete("This method is only for internal implementation. For external calls, please use CustomCore.RegisterCustomBuff(BuffConfig)")]
        public static int RegisterCustomBuff(string text, BuffType buffType, Func<bool> canUnlock, int cost,
            PlantType icon, int level,
            BuffBgType bgType = default, ZombieType zombieType = ZombieType.NormalZombie,
            int buffID = -1, PlantType plantType = PlantType.Nothing, bool addProbability = false)
        {
            CoreTools.InitBuffDic();
            int i = -1;
            switch (buffType)
            {
                case BuffType.AdvancedBuff:
                    i = CustomBuffStartID + CustomAdvancedBuffs.Count;
                    if (buffID != -1) i = buffID;
                    CustomAdvancedBuffs.Add(i, (icon, text, canUnlock, cost));
                    TravelDictionary.advancedBuffsText.Add((AdvBuff)i, text);
                    TravelDictionary.AdvBuffPlantPairs.Add((AdvBuff)i, icon);
                    break;
                case BuffType.UltimateBuff:
                    i = CustomBuffStartID + CustomUltimateBuffs.Count;
                    if (buffID != -1) i = buffID;
                    CustomUltimateBuffs.Add(i, (icon, text, cost));
                    TravelDictionary.ultimateBuffsText.Add((UltiBuff)i, text);
                    break;
                case BuffType.Debuff:
                    i = CustomBuffStartID + CustomDebuffs.Count;
                    if (buffID != -1) i = buffID;
                    CustomDebuffs.Add(i, (text, zombieType, canUnlock));
                    TravelDictionary.debuffData.Add((TravelDebuff)i, new Il2CppSystem.ValueTuple<string, ZombieType>(
                        text, zombieType));
                    TravelHelper.LeaderAppear.Add((TravelDebuff)i);
                    break;
                case BuffType.UnlockPlant:
                    i = CustomBuffStartID + CustomUnlockBuffs.Count;
                    if (buffID == -1) i = buffID;
                    CustomUnlockBuffs.Add(i, (icon, text, cost));
                    TravelDictionary.unlocksText.Add((TravelUnlocks)i, text);
                    break;
                    //case BuffType.InvestmentBuff: // 投资注册还没写完
                    //    if (ibuff == null) return -1;
                    //    i = CustomBuffStartID + CustomInvestBuffs.Count;
                    //    if (buffID != -1) i = buffID;
                    //    CustomInvestBuffs.Add(i, (icon, canUnlock, ibuff));
                    //    TravelMgr.InvestBuffsData.Add((InvestBuff)i, ibuff);
                    //    break;
            }
            CustomBuffCost.Add((buffType, i), cost);
            CustomBuffText.Add((buffType, i), text);
            if (buffType != BuffType.Debuff)
                CustomBuffIcon.Add((buffType, i), icon);
            if (buffID != -1 && !CustomBuffIDMapping.ContainsKey((buffType, buffID)))
                CustomBuffIDMapping.Add((buffType, i), buffID);
            if (level != 1)
                CustomBuffsLevel.Add((buffType, i), (CustomBuffsLevel.Count, level));
            if (!CustomBuffsBg.ContainsKey((buffType, i)))
                CustomBuffsBg.Add((buffType, i), bgType);
            if (plantType != PlantType.Nothing && addProbability)
            {
                if (CustomPlantInfo.ContainsKey(plantType))
                    CustomPlantInfo[plantType].Add((buffType, i));
                else
                    CustomPlantInfo.Add(plantType, new List<(BuffType, int)> { (buffType, i) });
            }
            CustomBuffs.Add((buffType, i), (text, icon, zombieType));
            return i;
        }

        /// <summary>
        /// 注册此综艺词条
        /// </summary>
        /// <param name="config">词条配置</param>
        /// <returns>词条ID</returns>
        public static int RegisterCustomBuff(BuffConfig config)
        {
            return RegisterCustomBuff(config.desc, config.type, config.unlock, config.cost, config.iconPlant, config.maxLevel, config.backGround, config.iconZombie,
                config.ID.HasValue ? config.ID.Value : -1, config.lockPlantType, config.probably);
        }

        /// <summary>
        /// 设置二创词条在词条图鉴中的类型
        /// </summary>
        /// <param name="buffType">词条类型</param>
        /// <param name="id">词条ID</param>
        /// <param name="type">类型</param>
        /// <param name="icon">图标</param>
        public static void SetCustomBuffAlmanacType(BuffType buffType, int id, AlmanacBuffType type, PlantType icon) =>
            CustomAlmanacBuffType.Add((buffType, id), (type, icon, (ZombieType)(-1)));

        /// <summary>
        /// 设置二创词条在词条图鉴中的类型
        /// </summary>
        /// <param name="buffType">词条类型</param>
        /// <param name="id">词条ID</param>
        /// <param name="type">类型</param>
        /// <param name="icon">图标</param>
        public static void SetCustomBuffAlmanacType(BuffType buffType, int id, AlmanacBuffType type, ZombieType icon) =>
            CustomAlmanacBuffType.Add((buffType, id), (type, (PlantType)(-1), icon));
        #endregion

        /// <summary>
        /// 注册自定义子弹
        /// </summary>
        /// <typeparam name="TBullet">子弹基类</typeparam>
        /// <param name="id">子弹id</param>
        /// <param name="bulletPrefab">子弹预制体</param>
        public static void RegisterCustomBullet<TBullet>(BulletType id, GameObject bulletPrefab) where TBullet : Bullet
        {
            if (!CustomBullets.ContainsKey(id))
            {
                bulletPrefab.AddComponent<TBullet>().theBulletType = id;
                CustomBullets.Add(id, bulletPrefab);
            }
            else
                Instance.Value.Log.LogError($"Duplicate Bullet ID: {id}");
        }

        /// <summary>
        /// 注册自定义子弹
        /// </summary>
        /// <typeparam name="TBase">子弹基类</typeparam>
        /// <typeparam name="TBullet">子弹自定义对象类</typeparam>
        /// <param name="id">子弹id</param>
        /// <param name="bulletPrefab">子弹预制体</param>
        public static void RegisterCustomBullet<TBase, TBullet>(BulletType id, GameObject bulletPrefab)
            where TBase : Bullet where TBullet : MonoBehaviour
        {
            if (!CustomBullets.ContainsKey(id))
            {
                bulletPrefab.AddComponent<TBase>().theBulletType = id;
                bulletPrefab.AddComponent<TBullet>();
                CustomBullets.Add(id, bulletPrefab);
            }
            else
                Instance.Value.Log.LogError($"Duplicate Bullet ID: {id}");
        }

        /// <summary>
        /// 注册自定义关卡
        /// </summary>
        /// <param name="ldata">自定义关卡数据</param>
        /// <returns>关卡ID</returns>
        public static int RegisterCustomLevel(CustomLevelData ldata)
        {
            int id = CustomLevels.Count;
            ldata.ID = id;
            CustomLevels.Add(ldata);
            return id;
        }

        /// <summary>
        /// 注册自定义粒子效果
        /// </summary>
        /// <param name="id">粒子效果id</param>
        /// <param name="particle">粒子效果预制体</param>
        public static void RegisterCustomParticle(ParticleType id, GameObject particle) =>
            CustomParticles.Add(id, particle);

        #region 注册植物&皮肤
        /// <summary>
        /// 注册自定义植物
        /// </summary>
        /// <typeparam name="TBase">植物基类</typeparam>
        /// <typeparam name="TClass">植物自定义对象类</typeparam>
        /// <param name="id">植物id</param>
        /// <param name="prefab">植物预制体</param>
        /// <param name="preview">植物预览预制体</param>
        /// <param name="fusions">植物融合配方</param>
        /// <param name="attackInterval">攻击间隔</param>
        /// <param name="produceInterval">生产间隔</param>
        /// <param name="attackDamage">攻击伤害</param>
        /// <param name="maxHealth">血量</param>
        /// <param name="cd">卡槽cd</param>
        /// <param name="sun">阳光</param>
        public static void RegisterCustomPlant<TBase, TClass>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview,
            List<(int, int)> fusions, float attackInterval, float produceInterval, int attackDamage, int maxHealth,
            float cd, int sun)
            where TBase : Plant where TClass : MonoBehaviour
        {
            prefab.AddComponent<TBase>().thePlantType = (PlantType)id;
            prefab.AddComponent<TClass>();
            if (!CustomPlantTypes.Contains((PlantType)id))
            {
                CustomPlantTypes.Add((PlantType)id);
                CustomPlants.Add((PlantType)id, new CustomPlantData()
                {
                    ID = id,
                    Prefab = prefab,
                    Preview = preview,
                    PlantData = new()
                    {
                        attackDamage = attackDamage,
                        thePlantType = (PlantType)id,
                        attackInterval = attackInterval,
                        produceInterval = produceInterval,
                        maxHealth = maxHealth,
                        cd = cd,
                        cost = sun
                    }
                });
                foreach (var f in fusions)
                {
                    AddFusion(id, f.Item1, f.Item2);
                }
            }
            else
            {
                Instance.Value.Log.LogError($"Duplicate Plant ID: {id}");
            }
        }

        /// <summary>
        /// 注册自定义植物
        /// </summary>
        /// <typeparam name="TBase">植物基类</typeparam>
        /// <param name="id">植物id</param>
        /// <param name="prefab">植物预制体</param>
        /// <param name="preview">植物预览预制体</param>
        /// <param name="fusions">植物融合配方</param>
        /// <param name="attackInterval">攻击间隔</param>
        /// <param name="produceInterval">生产间隔</param>
        /// <param name="attackDamage">攻击伤害</param>
        /// <param name="maxHealth">血量</param>
        /// <param name="cd">卡槽cd</param>
        /// <param name="sun">阳光</param>
        public static void RegisterCustomPlant<TBase>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview,
            List<(int, int)> fusions, float attackInterval, float produceInterval, int attackDamage, int maxHealth,
            float cd, int sun)
            where TBase : Plant
        {
            prefab.AddComponent<TBase>().thePlantType = (PlantType)id;
            if (!CustomPlantTypes.Contains((PlantType)id))
            {
                CustomPlantTypes.Add((PlantType)id);
                CustomPlants.Add((PlantType)id, new CustomPlantData()
                {
                    ID = id,
                    Prefab = prefab,
                    Preview = preview,
                    PlantData = new()
                    {
                        attackDamage = attackDamage,
                        thePlantType = (PlantType)id,
                        attackInterval = attackInterval,
                        produceInterval = produceInterval,
                        maxHealth = maxHealth,
                        cd = cd,
                        cost = sun
                    }
                });
                foreach (var f in fusions)
                {
                    AddFusion(id, f.Item1, f.Item2);
                }
            }
            else
            {
                Instance.Value.Log.LogError($"Duplicate Plant ID: {id}");
            }
        }

        public static void SetBulletSkinList([NotNull] int id, List<(BulletType, List<GameObject?>)>? bulletSkinList = null)
        {
            if (bulletSkinList != null)
            {
                var index = CustomSkinIndex.TryGetValue((PlantType)id, out var val) ? (val + 1) : (0 + 1); // 主皮肤(不换肤)是0，后续每加一个索引加一
                foreach (var (origin, list) in bulletSkinList)
                {
                    foreach (var bullet in list)
                    {
                        var bulletID = (BulletType)(CustomBulletSkinStartID + RegisteredSkinBulletCount);
                        if (!CustomSkinBullet.ContainsKey(origin))
                            CustomSkinBullet.Add(origin, new List<(BulletType, GameObject?)> { (bulletID, bullet) });
                        else
                            CustomSkinBullet[origin].Add((bulletID, bullet));
                        if (!CustomBulletsSkinID.ContainsKey(((PlantType)id, origin, index)))
                            CustomBulletsSkinID.Add(((PlantType)id, origin, index), new List<BulletType> { bulletID });
                        else
                            CustomBulletsSkinID[((PlantType)id, origin, index)].Add(bulletID);
                        RegisteredSkinBulletCount++;
                    }
                }
                CustomSkinIndex[(PlantType)id] = index;
            }
        }

        /// <summary>
        /// 注册自定义植物皮肤
        /// </summary>
        /// <typeparam name="TBase">植物基类</typeparam>
        /// <typeparam name="TClass">植物自定义对象类</typeparam>
        /// <param name="id">植物id</param>
        /// <param name="prefab">植物预制体</param>
        /// <param name="preview">植物预览预制体</param>
        /// <param name="fusions">植物融合配方</param>
        /// <param name="attackInterval">攻击间隔</param>
        /// <param name="produceInterval">生产间隔</param>
        /// <param name="attackDamage">攻击伤害</param>
        /// <param name="maxHealth">血量</param>
        /// <param name="cd">卡槽cd</param>
        /// <param name="sun">阳光</param>
        /// <param name="bulletSkinList">切换皮肤时切换的皮肤子弹</param>
        public static void RegisterCustomPlantSkin<TBase, TClass>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview,
            List<(int, int)> fusions, float attackInterval, float produceInterval, int attackDamage, int maxHealth,
            float cd, int sun, List<(BulletType, List<GameObject?>)>? bulletSkinList = null)
            where TBase : Plant where TClass : MonoBehaviour
        {
            SetBulletSkinList(id, bulletSkinList);
            //植物预制体挂载植物脚本
            prefab.tag = "Plant";
            preview.tag = "Preview";
            prefab.AddComponent<TBase>().thePlantType = (PlantType)id;
            prefab.AddComponent<TClass>();
            if (!CustomPlantsSkinActive.ContainsKey((PlantType)id))
                CustomPlantsSkinActive.Add((PlantType)id, false);
            //植物id不重复才进行注册
            if (!CustomPlantsSkin.ContainsKey((PlantType)id))
            {
                //CustomPlantTypes.Add((PlantType)id);
                CustomPlantsSkin.Add((PlantType)id, new List<CustomPlantData> {
                    new CustomPlantData()
                    {
                        ID = id,
                        Prefab = prefab,
                        Preview = preview,
                        PlantData = new()
                        {
                            attackDamage = attackDamage,
                            thePlantType = (PlantType)id,
                            //攻击间隔
                            attackInterval = attackInterval,
                            //生产间隔
                            produceInterval = produceInterval,
                            //最大HP
                            maxHealth = maxHealth,
                            //种植冷却
                            cd = cd,
                            //花费阳光
                            cost = sun
                        },
                        BulletList = bulletSkinList
                    }
                });
            }
            else
            {
                CustomPlantsSkin[(PlantType)id].Add(new CustomPlantData()
                {
                    ID = id,
                    Prefab = prefab,
                    Preview = preview,
                    PlantData = new()
                    {
                        attackDamage = attackDamage,
                        thePlantType = (PlantType)id,
                        //攻击间隔
                        attackInterval = attackInterval,
                        //生产间隔
                        produceInterval = produceInterval,
                        //最大HP
                        maxHealth = maxHealth,
                        //种植冷却
                        cd = cd,
                        //花费阳光
                        cost = sun
                    },
                    BulletList = bulletSkinList
                });
            }
            foreach (var f in fusions)
            {
                //添加融合配方
                AddFusion(id, f.Item1, f.Item2);
            }
        }

        /// <summary>
        /// 注册自定义植物皮肤
        /// </summary>
        /// <typeparam name="TBase">植物基类</typeparam>
        /// <param name="id">植物id</param>
        /// <param name="prefab">植物预制体</param>
        /// <param name="preview">植物预览预制体</param>
        /// <param name="fusions">植物融合配方</param>
        /// <param name="attackInterval">攻击间隔</param>
        /// <param name="produceInterval">生产间隔</param>
        /// <param name="attackDamage">攻击伤害</param>
        /// <param name="maxHealth">血量</param>
        /// <param name="cd">卡槽cd</param>
        /// <param name="sun">阳光</param>
        /// <param name="bulletSkinList">切换皮肤时切换的皮肤子弹</param>
        public static void RegisterCustomPlantSkin<TBase>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview,
            List<(int, int)> fusions, float attackInterval, float produceInterval, int attackDamage, int maxHealth,
            float cd, int sun, List<(BulletType, List<GameObject?>)>? bulletSkinList = null)
            where TBase : Plant
        {
            SetBulletSkinList(id, bulletSkinList);
            prefab.tag = "Plant";
            preview.tag = "Preview";
            //植物预制体挂载植物脚本
            prefab.AddComponent<TBase>().thePlantType = (PlantType)id;
            CustomPlantsSkinActive.Add((PlantType)id, false);
            if (!CustomPlantsSkin.ContainsKey((PlantType)id))
            {
                //CustomPlantTypes.Add((PlantType)id);
                CustomPlantsSkin.Add((PlantType)id, new List<CustomPlantData> {
                    new CustomPlantData()
                    {
                        ID = id,
                        Prefab = prefab,
                        Preview = preview,
                        PlantData = new()
                        {
                            attackDamage = attackDamage,
                            thePlantType = (PlantType)id,
                            //攻击间隔
                            attackInterval = attackInterval,
                            //生产间隔
                            produceInterval = produceInterval,
                            //最大HP
                            maxHealth = maxHealth,
                            //种植冷却
                            cd = cd,
                            //花费阳光
                            cost = sun
                        },
                        BulletList = bulletSkinList
                    }
                });
            }
            else
            {
                CustomPlantsSkin[(PlantType)id].Add(new CustomPlantData()
                {
                    ID = id,
                    Prefab = prefab,
                    Preview = preview,
                    PlantData = new()
                    {
                        attackDamage = attackDamage,
                        thePlantType = (PlantType)id,
                        //攻击间隔
                        attackInterval = attackInterval,
                        //生产间隔
                        produceInterval = produceInterval,
                        //最大HP
                        maxHealth = maxHealth,
                        //种植冷却
                        cd = cd,
                        //花费阳光
                        cost = sun
                    },
                    BulletList = bulletSkinList
                });
            }
            foreach (var f in fusions)
            {
                //添加融合配方
                AddFusion(id, f.Item1, f.Item2);
            }
        }

        /// <summary>
        /// 注册自定义植物皮肤(用于给原有植物添加皮肤)
        /// </summary>
        /// <typeparam name="TBase">植物基类</typeparam>
        /// <param name="id">植物id</param>
        /// <param name="prefab">植物预制体</param>
        /// <param name="preview">植物预览预制体</param>
        /// <param name="ctor">数据绑定函数</param>
        /// <param name="bulletSkinList">切换皮肤时切换的皮肤子弹</param>
        public static void RegisterCustomPlantSkin<TBase>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview, Action<TBase> ctor, List<(BulletType, List<GameObject?>)>? bulletSkinList = null)
            where TBase : Plant
        {
            SetBulletSkinList(id, bulletSkinList);
            prefab.tag = "Plant";
            preview.tag = "Preview";
            //植物预制体挂载植物脚本
            prefab.AddComponent<TBase>().thePlantType = (PlantType)id;
            ctor(prefab.GetComponent<TBase>());
            CustomPlantsSkinActive.Add((PlantType)id, false);
            if (!CustomPlantsSkin.ContainsKey((PlantType)id))
            {
                //植物id不重复才进行注册
                //CustomPlantTypes.Add((PlantType)id);
                CustomPlantsSkin.Add((PlantType)id, new List<CustomPlantData> {
                    new CustomPlantData()
                    {
                        ID = id,
                        Prefab = prefab,
                        Preview = preview,
                        PlantData = null,
                        BulletList = bulletSkinList
                    }
                });
            }
            else
            {
                CustomPlantsSkin[(PlantType)id].Add(
                    new CustomPlantData()
                    {
                        ID = id,
                        Prefab = prefab,
                        Preview = preview,
                        PlantData = null,
                        BulletList = bulletSkinList
                    }
                );
            }
        }

        /// <summary>
        /// 注册自定义植物皮肤(用于给原有植物添加皮肤)
        /// </summary>
        /// <typeparam name="TBase">植物基类</typeparam>
        /// <typeparam name="TClass">植物自定义对象类</typeparam>
        /// <param name="id">植物id</param>
        /// <param name="prefab">植物预制体</param>
        /// <param name="preview">植物预览预制体</param>
        /// <param name="ctor">数据绑定函数</param>
        /// <param name="bulletType">皮肤子弹类型</param>
        /// <param name="bullet">皮肤子弹预制体</param>
        /// <param name="bulletSkinList">切换皮肤时切换的皮肤子弹</param>
        public static void RegisterCustomPlantSkin<TBase, TClass>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview, Action<TBase> ctor, List<(BulletType, List<GameObject?>)>? bulletSkinList = null)
            where TBase : Plant where TClass : MonoBehaviour
        {
            SetBulletSkinList(id, bulletSkinList);
            prefab.tag = "Plant";
            preview.tag = "Preview";
            //植物预制体挂载植物脚本
            prefab.AddComponent<TBase>().thePlantType = (PlantType)id;
            prefab.AddComponent<TClass>();
            ctor(prefab.GetComponent<TBase>());
            CustomPlantsSkinActive.Add((PlantType)id, false);
            if (!CustomPlantsSkin.ContainsKey((PlantType)id))
            {
                //植物id不重复才进行注册
                //CustomPlantTypes.Add((PlantType)id);
                CustomPlantsSkin.Add((PlantType)id, new List<CustomPlantData> {
                    new CustomPlantData()
                    {
                        ID = id,
                        Prefab = prefab,
                        Preview = preview,
                        PlantData = null,
                        BulletList = bulletSkinList
                    }
                });
            }
            else
            {
                CustomPlantsSkin[(PlantType)id].Add(
                    new CustomPlantData()
                    {
                        ID = id,
                        Prefab = prefab,
                        Preview = preview,
                        PlantData = null,
                        BulletList = bulletSkinList
                    }
                );
            }
        }
        #endregion

        /// <summary>
        /// 注册皮肤子弹(内部使用)
        /// </summary>
        /// <param name="bulletType">皮肤子弹类型</param>
        /// <param name="bullet">子弹Prefab</param>
        public static void RegisterCustomSkinBullet(BulletType origin, BulletType bulletType, GameObject bullet)
        {
            if (CustomSkinBullet.ContainsKey(origin))
                CustomSkinBullet[origin].Add((bulletType, bullet));
            else
                CustomSkinBullet.Add(origin, new List<(BulletType, GameObject?)> { (bulletType, bullet) });
            GameAPP.resourcesManager.bulletPrefabs[bulletType] = bullet;//注册子弹预制体
            if (!GameAPP.resourcesManager.allBullets.Contains(bulletType))
                GameAPP.resourcesManager.allBullets.Add(bulletType);//注册子弹类型
            RegisteredSkinBulletCount++;
        }

        /// <summary>
        /// 注册自定义精灵图
        /// </summary>
        /// <param name="id">贴图id</param>
        /// <param name="sprite">贴图对象</param>
        public static void RegisterCustomSprite(int id, Sprite sprite) => CustomSprites.Add(id, sprite);

        /// <summary>
        /// 注册对植物使用物品事件
        /// </summary>
        /// <param name="id">目标植物id</param>
        /// <param name="bucketType">物品类型</param>
        /// <param name="callback">事件</param>
        public static void RegisterCustomUseItemOnPlantEvent([NotNull] PlantType id, [NotNull] BucketType bucketType,
            [NotNull] Action<Plant> callback) => CustomUseItems.Add((id, bucketType), callback);

        /// <summary>
        /// 注册物品融合配方
        /// </summary>
        /// <param name="id">亲本植物id</param>
        /// <param name="bucketType">物品类型</param>
        /// <param name="newPlant">新植物类型</param>
        public static void RegisterCustomUseItemOnPlantEvent([NotNull] PlantType id, [NotNull] BucketType bucketType,
            [NotNull] PlantType newPlant)
            => CustomUseItems.Add((id, bucketType), (p) =>
            {
                p.Die();
                CreatePlant.Instance.SetPlant(p.thePlantColumn, p.thePlantRow, newPlant);
            });

        /// <summary>
        /// 注册肥料使用事件
        /// </summary>
        /// <param name="id">目标植物id</param>
        /// <param name="callback">事件</param>
        public static void RegisterCustomUseFertilizeOnPlantEvent([NotNull] PlantType id, [NotNull] Action<Plant> callback) =>
            CustomUseFertilize.Add(id, callback);

        /// <summary>
        /// 注册肥料融合配方
        /// </summary>
        /// <param name="id">亲本植物id</param>
        /// <param name="newPlant">新植物类型</param>
        public static void RegisterCustomUseFertilizeOnPlantEvent([NotNull] PlantType id, [NotNull] PlantType newPlant)
            => CustomUseFertilize.Add(id, (p) =>
            {
                p.Die();
                CreatePlant.Instance.SetPlant(p.thePlantColumn, p.thePlantRow, newPlant);
            });

        #region 注册卡牌
        /// <summary>
        /// 注册自定义卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型</param>
        /// <param name="parent">父对象，所有Func的返回值都应为想要设置的父对象</param>
        public static void RegisterCustomCard([NotNull] PlantType thePlantType, [NotNull] List<Func<Transform?>> parent, int repeatTime = 1)
        {
            if (!CustomCards.ContainsKey(thePlantType))
                CustomCards.Add(thePlantType, (parent, repeatTime));
            else
                foreach (Func<Transform?> action in parent)
                    CustomCards[thePlantType].Item1.Add(action);
        }

        /// <summary>
        /// 注册自定义卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型，父对象将在实例化时自动设置</param>
        public static void RegisterCustomCard([NotNull] PlantType thePlantType, int repeatTime = 1)
        {
            if (!CustomCards.ContainsKey(thePlantType))
                CustomCards.Add(thePlantType, (new List<Func<Transform?>>()
                {
                    () => Utils.GetNormalCardParent()
                }, repeatTime));
            else
                CustomCards[thePlantType].Item1.Add(
                    () => Utils.GetNormalCardParent());
        }

        /// <summary>
        /// 注册自定义卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型，父对象将在实例化时自动设置</param>
        public static void RegisterCustomCardToColorfulCards([NotNull] PlantType thePlantType, int repeatTime = 1) => RegisterCustomCard(thePlantType, new List<Func<Transform?>>
        {
            () => Utils.GetColorfulCardParent()
        }, repeatTime);

        /// <summary>
        /// 注册自定义普通卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型</param>
        /// <param name="parent">父对象，所有Func的返回值都应为想要设置的父对象</param>
        public static void RegisterCustomNormalCard([NotNull] PlantType thePlantType, List<Func<Transform?>> parent, int repeatTime = 1)
        {
            if (!CustomNormalCards.ContainsKey(thePlantType))
                CustomNormalCards.Add(thePlantType, (parent, repeatTime));
            else
                foreach (Func<Transform?> action in parent)
                    CustomNormalCards[thePlantType].Item1.Add(action);
        }

        /// <summary>
        /// 注册自定义普通卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型，父对象将在实例化时自动设置</param>
        public static void RegisterCustomNormalCard([NotNull] PlantType thePlantType, int repeatTime = 1)
        {
            if (!CustomNormalCards.ContainsKey(thePlantType))
                CustomNormalCards.Add(thePlantType, (new List<Func<Transform?>>()
                {
                    () => Utils.GetNormalCardParent()
                }, repeatTime));
            else
                CustomNormalCards[thePlantType].Item1.Add(
                    () => Utils.GetNormalCardParent());
        }
        #endregion

        #region 僵尸注册
        /// <summary>
        /// 注册自定义僵尸
        /// </summary>
        /// <typeparam name="TBase">僵尸基类</typeparam>
        /// <typeparam name="TClass">僵尸自定义对象类</typeparam>
        /// <param name="id">僵尸id</param>
        /// <param name="zombie">僵尸预制体</param>
        /// <param name="spriteId">僵尸头贴图id</param>
        /// <param name="theAttackDamage">攻击伤害</param>
        /// <param name="theMaxHealth">本体血量</param>
        /// <param name="theFirstArmorMaxHealth">一类防具血量</param>
        /// <param name="theSecondArmorMaxHealth">二类防具血量</param>
        public static void RegisterCustomZombie<TBase, TClass>(ZombieType id, GameObject zombie, Sprite preview,
            int theAttackDamage, int theMaxHealth, int theFirstArmorMaxHealth, int theSecondArmorMaxHealth)
            where TBase : Zombie where TClass : MonoBehaviour
        {
            zombie.AddComponent<TBase>().theZombieType = id;
            zombie.AddComponent<TClass>();

            if (!CustomZombieTypes.Contains(id))
            {
                CustomZombieTypes.Add(id);
                CustomZombies.Add(id, (zombie, preview, new()
                {
                    theAttackDamage = theAttackDamage,
                    theFirstArmorMaxHealth = theFirstArmorMaxHealth,
                    theMaxHealth = theMaxHealth,
                    theSecondArmorMaxHealth = theSecondArmorMaxHealth
                }));
            }
            else
                Instance.Value.Log.LogError($"Duplicate ZombieType: {id}");
        }

        /// <summary>
        /// 注册自定义僵尸
        /// </summary>
        /// <typeparam name="TBase">僵尸基类</typeparam>
        /// <typeparam name="TClass">僵尸自定义对象类</typeparam>
        /// <param name="id">僵尸id</param>
        /// <param name="zombie">僵尸预制体</param>
        /// <param name="spriteId">僵尸头贴图id</param>
        /// <param name="theAttackDamage">攻击伤害</param>
        /// <param name="theMaxHealth">本体血量</param>
        /// <param name="theFirstArmorMaxHealth">一类防具血量</param>
        /// <param name="theSecondArmorMaxHealth">二类防具血量</param>
        public static void RegisterCustomZombie<TBase>(ZombieType id, GameObject zombie, Sprite preview,
            int theAttackDamage, int theMaxHealth, int theFirstArmorMaxHealth, int theSecondArmorMaxHealth)
            where TBase : Zombie
        {
            zombie.AddComponent<TBase>().theZombieType = id;

            if (!CustomZombieTypes.Contains(id))
            {
                CustomZombieTypes.Add(id);
                CustomZombies.Add(id, (zombie, preview, new()
                {
                    theAttackDamage = theAttackDamage,
                    theFirstArmorMaxHealth = theFirstArmorMaxHealth,
                    theMaxHealth = theMaxHealth,
                    theSecondArmorMaxHealth = theSecondArmorMaxHealth
                }));
            }
            else
                Instance.Value.Log.LogError($"Duplicate ZombieType: {id}");
        }

        /// <summary>
        /// 注册自定义僵尸
        /// </summary>
        /// <typeparam name="TBase">僵尸基类</typeparam>
        /// <typeparam name="TClass">僵尸自定义对象类</typeparam>
        /// <param name="id">僵尸id</param>
        /// <param name="zombie">僵尸预制体</param>
        /// <param name="data">僵尸数据</param>
        public static void RegisterCustomZombie<TBase, TClass>(ZombieType id, GameObject zombie, Sprite preview, ZombieDataManager.ZombieData data)
            where TBase : Zombie where TClass : MonoBehaviour
        {
            zombie.AddComponent<TBase>().theZombieType = id;
            zombie.AddComponent<TClass>();

            if (!CustomZombieTypes.Contains(id))
            {
                CustomZombieTypes.Add(id);
                CustomZombies.Add(id, (zombie, preview, data));
            }
            else
                Instance.Value.Log.LogError($"Duplicate ZombieType: {id}");
        }

        /// <summary>
        /// 注册自定义僵尸
        /// </summary>
        /// <typeparam name="TBase">僵尸基类</typeparam>
        /// <typeparam name="TClass">僵尸自定义对象类</typeparam>
        /// <param name="id">僵尸id</param>
        /// <param name="zombie">僵尸预制体</param>
        /// <param name="data">僵尸数据</param>
        public static void RegisterCustomZombie<TBase>(ZombieType id, GameObject zombie, Sprite preview, ZombieDataManager.ZombieData data)
            where TBase : Zombie
        {
            zombie.AddComponent<TBase>().theZombieType = id;

            if (!CustomZombieTypes.Contains(id))
            {
                CustomZombieTypes.Add(id);
                CustomZombies.Add(id, (zombie, preview, data));
            }
            else
                Instance.Value.Log.LogError($"Duplicate ZombieType: {id}");
        }
        #endregion

        /// <summary>
        /// 注册植物大招
        /// </summary>
        /// <param name="id">植物id</param>
        /// <param name="cost">开大花费</param>
        /// <param name="skill">要运行的大招函数</param>
        /// <param name="defaultCost">默认开大花费</param>
        public static void RegisterSuperSkill([NotNull] int id, [NotNull] Func<Plant, int> cost,
            [NotNull] Action<Plant> skill, [NotNull] int defaultCost = 1000) => SuperSkills.Add((PlantType)id, (cost, skill, defaultCost));


        /// <summary>
        /// 添加自定义究极植物
        /// </summary>
        /// <param name="plantType">要设为究极植物的植物类型</param>
        public static void AddUltimatePlant([NotNull] PlantType plantType) => CustomUltimatePlants.Add(plantType);

        /// <summary>
        /// 注册自定义强究植物
        /// </summary>
        /// <param name="plantType">植物类型</param>
        /// <param name="id">强究解锁id（注册时返回的）</param>
        public static void RegisterCustomStrongUltimatePlant([NotNull] PlantType plantType, [NotNull] int id)
        {
            if (!TravelDictionary.allStrongUltimtePlant.Contains(plantType))
            {
                TravelDictionary.allStrongUltimtePlant.Add(plantType);
            }
            if (!TravelDictionary.PlantInfo.ContainsKey(plantType))
                TravelDictionary.PlantInfo.Add(plantType, new Il2CppSystem.ValueTuple<Il2CppSystem.Nullable<PlantType>, Il2CppSystem.Object, Il2CppSystem.Object, bool>(new Il2CppSystem.Nullable<PlantType>(plantType), null, null, true));
            else
                TravelDictionary.PlantInfo[plantType].Item4 = true;
            if (!CustomStrongUltimatePlants.ContainsKey(plantType))
                CustomStrongUltimatePlants.Add(plantType, id);
            else
                CLogger.LogError($"Duplicate strong ultimate type: {plantType}");
        }

        #region 注册融合配方
        /// <summary>
        /// 注册自定义融合洋芋配方
        /// </summary>
        /// <param name="left">左植物</param>
        /// <param name="center">中间植物</param>
        /// <param name="right">右植物</param>
        /// <param name="actions">融合成功执行事件列表</param>
        /// <param name="failActions">融合失败执行事件列表</param>
        public static void RegisterCustomMixBombFusion([NotNull] PlantType left, [NotNull] PlantType center, [NotNull] PlantType right,
            [NotNull] List<Action<Plant, Plant, Plant>> actions, [NotNull] List<Action<Plant, Plant, Plant>> failActions)
        {
            if (CustomMixBombFusions.ContainsKey((left, center, right)))
            {
                foreach (var success in actions) CustomMixBombFusions[(left, center, right)].Item1.Add(success);
                foreach (var fail in failActions) CustomMixBombFusions[(left, center, right)].Item2.Add(fail);
            }
            else
                CustomMixBombFusions.Add((left, center, right), (actions, failActions));
        }

        /// <summary>
        /// 注册自定义融合洋芋配方
        /// </summary>
        /// <param name="left">左植物</param>
        /// <param name="center">中间植物</param>
        /// <param name="right">右植物</param>
        /// <param name="action">融合成功执行事件</param>
        /// <param name="failAction">融合失败执行事件</param>
        public static void RegisterCustomMixBombFusion([NotNull] PlantType left, [NotNull] PlantType center, [NotNull] PlantType right,
            [NotNull] Action<Plant, Plant, Plant> action, [NotNull] Action<Plant, Plant, Plant> failAction)
        {
            if (CustomMixBombFusions.ContainsKey((left, center, right)))
            {
                CustomMixBombFusions[(left, center, right)].Item1.Add(action);
                CustomMixBombFusions[(left, center, right)].Item2.Add(failAction);
            }
            else
                CustomMixBombFusions.Add((left, center, right),
                    new()
                    {
                        Item1 = new() { action },
                        Item2 = new() { failAction }
                    });
        }

        /// <summary>
        /// 注册自定义融合洋芋配方
        /// </summary>
        /// <param name="left">左植物</param>
        /// <param name="center">中间植物</param>
        /// <param name="right">右植物</param>
        /// <param name="action">融合成功执行事件</param>
        /// <param name="failMessage">融合失败显示消息</param>
        public static void RegisterCustomMixBombFusion([NotNull] PlantType left, [NotNull] PlantType center, [NotNull] PlantType right,
            [NotNull] Action<Plant, Plant, Plant> action, [NotNull] String failMessage = "") => RegisterCustomMixBombFusion(left, center, right, action, (p1, p2, p3) =>
            {
                if (failMessage != "" && InGameText.Instance is not null)
                    InGameText.Instance.ShowText(failMessage, 5f);
            });

        /// <summary>
        /// 注册自定义融合洋芋配方
        /// </summary>
        /// <param name="left">左植物</param>
        /// <param name="center">中间植物</param>
        /// <param name="right">右植物</param>
        /// <param name="target">生成卡片类型</param>
        /// <param name="failMessage">融合失败显示消息</param>
        public static void RegisterCustomMixBombFusion([NotNull] PlantType left, [NotNull] PlantType center, [NotNull] PlantType right,
            [NotNull] PlantType target, [NotNull] String failMessage = "") => RegisterCustomMixBombFusion(left, center, right, (p1, p2, p3) =>
            {
                Lawnf.SetDroppedCard(p2.axis.transform.position, target);
                p1.Die();
                p2.Die();
                p3.Die();
                GameAPP.PlaySound(125, 0.5f, 1f);
            }, (p1, p2, p3) =>
            {
                if (failMessage != "" && InGameText.Instance is not null)
                    InGameText.Instance.ShowText(failMessage, 5f);
            });

        /// <summary>
        /// 注册自定义融合洋芋配方
        /// </summary>
        /// <param name="left">左植物</param>
        /// <param name="center">中间植物</param>
        /// <param name="right">右植物</param>
        /// <param name="target">生成卡片类型</param>
        /// <param name="failAction">融合失败执行事件</param>
        public static void RegisterCustomMixBombFusion([NotNull] PlantType left, [NotNull] PlantType center, [NotNull] PlantType right,
            [NotNull] PlantType target, [NotNull] Action<Plant, Plant, Plant> failAction) => RegisterCustomMixBombFusion(left, center, right, (p1, p2, p3) =>
            {
                Lawnf.SetDroppedCard(p2.axis.transform.position, target);
                p1.Die();
                p2.Die();
                p3.Die();
            }, failAction);

        /// <summary>
        /// 添加融合配方
        /// </summary>
        /// <param name="target">目标植物</param>
        /// <param name="item1">亲本（地上长的）</param>
        /// <param name="item2">亲本（后融合上去的）</param>
        public static void AddFusion(int target, int item1, int item2) => CustomFusions.Add((target, item1, item2));

        /// <summary>
        /// 添加融合配方
        /// </summary>
        /// <param name="target">目标植物</param>
        /// <param name="item1">亲本（地上长的）</param>
        /// <param name="item2">亲本（后融合上去的）</param>
        public static void AddFusion(PlantType target, PlantType item1, PlantType item2) => CustomFusions.Add(((int)target, (int)item1, (int)item2));
        #endregion

        /// <summary>
        /// 注册自定义子弹移动方式
        /// </summary>
        /// <param name="id">移动方式id</param>
        /// <param name="action">移动逻辑</param>
        public static void RegisterCustomBulletMovingWay(int id, Action<Bullet> action) => CustomBulletMovingWay.Add(id, action);

        /// <summary>
        /// 注册自定义樱桃爆炸
        /// </summary>
        /// <param name="id">类型</param>
        /// <param name="prefab">樱桃爆炸预制体</param>
        public static void RegisterCustomCherry(CherryBombType id, [NotNull] GameObject prefab)
        {
            RegisterCustomParticle((ParticleType)(CustomCherryStartID + (int)id), prefab);
            CustomCherrys.Add(id, prefab);
        }

        /// <summary>
        /// 注册自定义樱桃爆炸
        /// </summary>
        /// <typeparam name="T">樱桃爆炸组件</typeparam>
        /// <param name="id">类型</param>
        /// <param name="prefab">樱桃爆炸预制体</param>
        public static void RegisterCustomCherry<T>(CherryBombType id, [NotNull] GameObject prefab) where T : MonoBehaviour
        {
            if (!ClassInjector.IsTypeRegisteredInIl2Cpp<T>())
                ClassInjector.RegisterTypeInIl2Cpp<T>();
            prefab.AddComponent<T>();
            RegisterCustomParticle((ParticleType)(CustomCherryStartID + (int)id), prefab);
            CustomCherrys.Add(id, prefab);
        }

        /// <summary>
        /// 注册自定义植物点击在另一植物上事件
        /// </summary>
        /// <param name="plantType">底层植物（原有）</param>
        /// <param name="cardType">卡槽植物（点击上去的）</param>
        /// <param name="canClick">是否能执行</param>
        /// <param name="action">执行的事件</param>
        /// <param name="onPlant">执行时的配置</param>
        public static void RegisterCustomClickCardOnPlantEvent([NotNull] PlantType plantType, [NotNull] PlantType cardType, [NotNull] Action<Plant> action, Func<Plant, bool> canClick = null, [NotNull] CustomClickCardOnPlant onPlant = default)
        {
            if (CustomClickCardOnPlantEvents.ContainsKey((plantType, cardType)))
                CustomClickCardOnPlantEvents[(plantType, cardType)].Add((action, canClick, onPlant));
            else
                CustomClickCardOnPlantEvents.Add((plantType, cardType), new() { (action, canClick, onPlant) });
        }

        /// <summary>
        /// 注册是否允许融合
        /// </summary>
        /// <param name="plantType">目标植物(融合后的)</param>
        /// <param name="func">判断条件</param>
        /// <param name="success">融合成功执行函数</param>
        /// <param name="fail">融合失败执行函数</param>
        public static void RegisterCustomBanMix([NotNull] PlantType plantType, Func<bool>? func = null, Action? success = null, Action? fail = null)
        {
            if (!CustomBanMix.ContainsKey(plantType))
                CustomBanMix.Add(plantType, (func, success, fail));
            else
                CLogger.LogError($"Duplicate ban mix plant type: {plantType}");
        }

        /// <summary>
        /// 注册自定义融合事件
        /// </summary>
        /// <param name="baseType">底植物</param>
        /// <param name="newType">种植植物</param>
        /// <param name="actions">融合事件列表</param>
        public static void RegisterCustomOnMixEvent(PlantType baseType, PlantType newType, List<Action<Plant>> actions)
        {
            if (!CustomOnMixEvent.ContainsKey((baseType, newType)))
                CustomOnMixEvent.Add((baseType, newType), actions);
            else
                foreach (var item in actions)
                    CustomOnMixEvent[(baseType, newType)].Add(item);
        }

        /// <summary>
        /// 注册自定义融合事件
        /// </summary>
        /// <param name="baseType">底植物</param>
        /// <param name="newType">种植植物</param>
        /// <param name="action">融合事件</param>
        public static void RegisterCustomOnMixEvent(PlantType baseType, PlantType newType, Action<Plant> action) =>
            RegisterCustomOnMixEvent(baseType, newType, new List<Action<Plant>> { action });

        /// <summary>
        /// 注册自定义PlantInfo
        /// </summary>
        /// <param name="ultiType">究极植物本体类型</param>
        /// <param name="subType">亚种类型</param>
        /// <param name="buff1">词条1(传词条Enum, 如(AdvBuff)0)</param>
        /// <param name="buff2">词条2(传词条Enum, 如(AdvBuff)0)</param>
        /// <param name="isStrongUltimate">是否是强究</param>
        public static void RegisterCustomPlantInfo(PlantType ultiType, PlantType? subType, object buff1, object buff2, bool isStrongUltimate) =>
            CustomPlantInfos.Add(ultiType, (subType, buff1, buff2, isStrongUltimate));
        public static void AddPlantName(PlantType plantType, string name) => CustomPlantNames.Add(plantType, name);
        public static void AddZombieName(ZombieType zombieType, string name) => CustomZombieNames.Add(zombieType, name);

        /// <summary>
        /// 注册自定义音乐
        /// </summary>
        /// <param name="musicType">音乐ID</param>
        /// <param name="clip">音乐</param>
        public static void RegisterCustomMusic(int musicType, [NotNull] AudioClip clip) =>
            CustomMusics.Add((MusicType)musicType, clip);

        ///// <summary>
        ///// 添加无尽保存信息
        ///// </summary>
        ///// <param name="type">组件类型</param>
        ///// <param name="name">组件字段(属性)名</param>
        //public static void RegisterCustomEndlessData(Type type, string name) => CustomEndlessSaveData.Add(Il2CppType.From(type), name);

        ///// <summary>
        ///// 添加无尽保存信息
        ///// </summary>
        ///// <typeparam name="T">组件类型</typeparam>
        ///// <param name="name">组件字段(属性)名</param>
        //public static void RegisterCustomEndlessData<T>(string name) where T : Component => RegisterCustomEndlessData(typeof(T), name);

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<CustomPlantMonoBehaviour>();
            ClassInjector.RegisterTypeInIl2Cpp<SelectCustomPlants>();
            ClassInjector.RegisterTypeInIl2Cpp<CheckCardState>();
            ClassInjector.RegisterTypeInIl2Cpp<ExtensionDataComponent>();
            ClassInjector.RegisterTypeInIl2Cpp<DataComponent>();
            ClassInjector.RegisterTypeInIl2Cpp<CoreBehaviour>();
            ClassInjector.RegisterTypeInIl2Cpp<PositionRecorder>();
            ClassInjector.RegisterTypeInIl2Cpp<EmptyDie>();
            ClassInjector.RegisterTypeInIl2Cpp<InterfaceBehaviour>();
            ClassInjector.RegisterTypeInIl2Cpp<CustomHealthText>();
            ClassInjector.RegisterTypeInIl2Cpp<SaveMaterial>();
            ClassInjector.RegisterTypeInIl2Cpp<SelectCustomPlants>();
            SkinBehaviourMgr.Init();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            InitCoreData();
            MapValue.InitDatas();
            Internal.HookCall.RegisterTypes();
        }

        public void InitCoreData()
        {
            CoreData.Instance = new(this);
            CoreData.Logger = new(Log);
            CoreData.Harmony = new(new Harmony("salmon.inf75.pvzcustomization"));
        }

        public override bool Unload()
        {
            // 释放已固定的对象
            UnmanagedTools.ObjectPinner.Release();
            return true;
        }

        public static ManualLogSource CLogger => CoreData.Logger?.Value;
        public static Lazy<CustomCore> Instance => new(CoreData.Instance?.Value);

        #region 数据
        public static Dictionary<int, (PlantType, string, Func<bool>, int)> CustomAdvancedBuffs { get; set; } = [];
        public static Dictionary<BulletType, GameObject> CustomBullets { get; set; } = [];

        /// <summary>
        /// 自定义僵尸词条列表
        /// </summary>
        public static Dictionary<int, (string, ZombieType, Func<bool>)> CustomDebuffs { get; set; } = [];
        public static List<(int, int, int)> CustomFusions { get; set; } = [];
        public static List<CustomLevelData> CustomLevels { get; set; } = [];
        public static Dictionary<ParticleType, GameObject> CustomParticles { get; set; } = [];

        public static Dictionary<PlantType, Action<Plant>> CustomPlantClicks { get; set; } = [];

        public static Dictionary<PlantType, CustomPlantData> CustomPlants { get; set; } = [];

        /// <summary>
        /// 自定义植物皮肤列表
        /// </summary>
        public static Dictionary<PlantType, List<CustomPlantData>> CustomPlantsSkin { get; set; } = [];

        /// <summary>
        /// 自定义皮肤是否激活
        /// </summary>
        public static Dictionary<PlantType, bool> CustomPlantsSkinActive { get; set; } = [];

        public static List<PlantType> CustomPlantTypes { get; set; } = [];

        public static Dictionary<int, Sprite> CustomSprites { get; set; } = [];

        public static Dictionary<int, (PlantType, string, int)> CustomUltimateBuffs { get; set; } = [];

        public static Dictionary<(PlantType, BucketType), Action<Plant>> CustomUseItems { get; set; } = [];


        /// <summary>
        /// 自定义肥料物品事件列表
        /// </summary>
        public static Dictionary<PlantType, Action<Plant>> CustomUseFertilize { get; set; } = [];

        /// <summary>
        /// 自定义彩色卡牌列表
        /// </summary>
        public static Dictionary<PlantType, (List<Func<Transform?>>, int)> CustomCards { get; set; } = [];

        /// <summary>
        /// 自定义普通卡牌列表
        /// </summary>
        public static Dictionary<PlantType, (List<Func<Transform?>>, int)> CustomNormalCards { get; set; } = [];

        public static Dictionary<ZombieType, (GameObject, Sprite, ZombieDataManager.ZombieData)> CustomZombies { get; set; } = [];

        public static List<ZombieType> CustomZombieTypes { get; set; } = [];

        public static Dictionary<PlantType, PlantAlmanac> PlantsAlmanac { get; set; } = [];

        /// <summary>
        /// 皮肤图鉴
        /// </summary>
        public static Dictionary<PlantType, (string, string)?> PlantsSkinAlmanac { get; set; } = [];
        public static Dictionary<PlantType, (Func<Plant, int>, Action<Plant>, int)> SuperSkills { get; set; } = [];
        public static Dictionary<ZombieType, (string?, string?, ZombieInfo?)> ZombiesAlmanac { get; set; } = [];

        /// <summary>
        /// 自定义融合洋芋配方
        /// </summary>
        public static Dictionary<(PlantType, PlantType, PlantType), (List<Action<Plant?, Plant?, Plant?>>, List<Action<Plant?, Plant?, Plant?>>)> CustomMixBombFusions { get; set; } = []; // (左植物, 中植物, 右植物), (成功事件, 失败事件)

        /// <summary>
        /// 自定义子弹移动方式
        /// </summary>
        public static Dictionary<int, Action<Bullet>> CustomBulletMovingWay { get; set; } = [];

        /// <summary>
        /// 自定义多级词条列表（Buff类型，ID）->（在列表的index，等级）
        /// </summary>
        public static Dictionary<(BuffType, int), (int, int)> CustomBuffsLevel { get; set; } = [];

        /// <summary>
        /// 自定义词条ID映射（Buff类型，指定ID）-> 内部ID
        /// </summary>
        public static Dictionary<(BuffType, int), int> CustomBuffIDMapping { get; set; } = [];

        /// <summary>
        /// 自定义究极植物列表
        /// </summary>
        public static List<PlantType> CustomUltimatePlants { get; set; } = [];

        /// <summary>
        /// 自定义词条背景
        /// </summary>
        public static Dictionary<(BuffType, int), BuffBgType> CustomBuffsBg { get; set; } = [];

        /// <summary>
        /// 存卡片检查的列表，用于管理Packet显示，你不应该使用它
        /// </summary>
        public static List<CheckCardState> checkBehaviours = new List<CheckCardState>();

        /// <summary>
        /// 自定义解锁强究列表
        /// </summary>
        public static Dictionary<int, (PlantType, string, int)> CustomUnlockBuffs { get; set; } = [];

        /// <summary>
        /// 自定义强究列表 (强究类型, 解锁词条id)
        /// </summary>
        public static Dictionary<PlantType, int> CustomStrongUltimatePlants { get; set; } = [];

        /// <summary>
        /// 自定义种植植物在另一植物上事件（当前位置的植物的类型，鼠标上的植物类型），Action参数：当前位置的植物
        /// </summary>
        public static Dictionary<(PlantType, PlantType), List<(Action<Plant>, Func<Plant, bool>, CustomClickCardOnPlant)>> CustomClickCardOnPlantEvents { get; set; } = [];

        /// <summary>
        /// 自定义强究-强究词条列表
        /// </summary>
        public static Dictionary<PlantType, List<int>> CustomStrongUltimatePlantBuffs { get; set; } = [];

        /// <summary>
        /// 自定义融合条件列表 植物类型-(是否成功, 成功事件, 失败事件)
        /// </summary>
        public static Dictionary<PlantType, (Func<bool>?, Action?, Action?)> CustomBanMix { get; set; } = [];

        /// <summary>
        /// 自定义植物融合事件 (底植物,融合植物)-触发事件(融合后植物)
        /// </summary>
        public static Dictionary<(PlantType, PlantType), List<Action<Plant>>> CustomOnMixEvent { get; set; } = [];

        /// <summary>
        /// 自定义樱桃爆炸
        /// </summary>
        public static Dictionary<CherryBombType, GameObject> CustomCherrys { get; set; } = [];

        /// <summary>
        /// 自定义词条与植物信息
        /// </summary>
        public static Dictionary<PlantType, List<(BuffType, int)>> CustomPlantInfo { get; set; } = new();

        ///// <summary>
        ///// 自定义投资词条
        ///// </summary>
        //public static Dictionary<int, (PlantType, Func<bool>, IBuff)> CustomInvestBuffs { get; set; } = new();


        /// <summary>
        /// 自定义词条价格
        /// </summary>
        public static Dictionary<(BuffType, int), int> CustomBuffCost { get; set; } = new();

        public static int CustomBuffStartID = 17500;

        /// <summary>
        /// 已加载的ab包
        /// </summary>
        public static Dictionary<string, AssetBundle> LoadedAssetBundles { get; set; } = new();

        #region 皮肤子弹
        /// <summary>
        /// 自定义皮肤子弹列表 ((植物类型, 在SkinDic中的索引), (原来的子弹类型, 替换后的子弹类型列表(所有可能随机出来的)))
        /// </summary>
        public static Dictionary<(PlantType, int), Dictionary<BulletType, List<BulletType>>> CustomBulletSkinReplace { get; set; } = new();

        /// <summary>
        /// 自定义皮肤子弹列表 ((植物类型, 原子弹类型, 皮肤索引), 对应的所有新子弹类型)
        /// </summary>
        public static Dictionary<(PlantType pt, BulletType oriBulletType, int index), List<BulletType>> CustomBulletsSkinID { get; set; } = new();

        /// <summary>
        /// 自定义皮肤子弹类型 (原子弹类型, 所有对应的(新子弹类型, 子弹预制体))
        /// </summary>
        public static Dictionary<BulletType, List<(BulletType, GameObject?)>> CustomSkinBullet { get; set; } = new();

        /// <summary>
        /// 自定义皮肤索引
        /// </summary>
        public static Dictionary<PlantType, int> CustomSkinIndex { get; set; } = new();

        public static int CustomBulletSkinStartID = 2750;

        public static int RegisteredSkinBulletCount = 0;
        #endregion

        /// <summary>
        /// 已启用的自定义皮肤
        /// </summary>
        public static Dictionary<PlantType, bool> EnableSkin { get; set; } = new();

        /// <summary>
        /// 自定义皮肤 类型-索引 列表
        /// </summary>
        public static Dictionary<PlantType, List<int>> CustomPlantSkinIndex { get; set; } = new();

        public static List<AssetBundle> LoadedSkinAssetBundle { get; set; } = new();

        public static int CustomCherryStartID = 2000;

        /// <summary>
        /// 二创植物名称
        /// </summary>
        public static Dictionary<PlantType, string> CustomPlantNames { get; set; } = new();

        /// <summary>
        /// 二创僵尸名称
        /// </summary>
        public static Dictionary<ZombieType, string> CustomZombieNames { get; set; } = new();

        /// <summary>
        /// 自定义词条文本
        /// </summary>
        public static Dictionary<(BuffType, int), string> CustomBuffText { get; set; } = new();

        /// <summary>
        /// 自定义词条图标
        /// </summary>
        public static Dictionary<(BuffType, int), PlantType> CustomBuffIcon { get; set; } = new();

        /// <summary>
        /// 自定义音效
        /// </summary>
        public static Dictionary<int, AudioClip> CustomSounds { get; set; } = new();

        /// <summary>
        /// 二创词条
        /// </summary>
        public static Dictionary<(BuffType, int), (string, PlantType, ZombieType)> CustomBuffs { get; set; } = new();

        /// <summary>
        /// 设置词条图鉴内词条类型
        /// </summary>
        public static Dictionary<(BuffType, int), (AlmanacBuffType, PlantType, ZombieType)> CustomAlmanacBuffType { get; set; } = new();

        public static Dictionary<Il2CppSystem.Type, string> CustomEndlessSaveData { get; set; } = new();
        // public static List<PlantType> CustomWeakUltimatePlant = new();
        /// <summary>
        /// 自定义PlantInfo (究极, (亚种, 词条1, 词条2, 强究))
        /// </summary>
        public static Dictionary<PlantType, (PlantType? subType, object buff1, object buff2, bool isStrongUltimate)> CustomPlantInfos { get; set; } = new();

        /// <summary>
        /// 自定义音乐
        /// </summary>
        public static Dictionary<MusicType, AudioClip> CustomMusics { get; set; } = new();
        #endregion
    }

    public class CoreData
    {
        public static Lazy<CustomCore>? Instance = new(() => new());
        public static Lazy<ManualLogSource>? Logger = new(() => Instance.Value.Log);
        public static Lazy<Harmony>? Harmony = new(() => new Harmony("salmon.inf75.pvzcustomization"));
    }
}