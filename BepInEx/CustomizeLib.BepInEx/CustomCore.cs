﻿using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils;

///
///Credit to likefengzi(https://github.com/likefengzi)(https://space.bilibili.com/237491236)
///
namespace CustomizeLib.BepInEx
{
    public class CoroutineRunner : MonoBehaviour
    {
        public CoroutineRunner() : base(ClassInjector.DerivedConstructorPointer<CoroutineRunner>()) => ClassInjector.DerivedConstructorBody(this);

        public CoroutineRunner(IntPtr i) : base(i)
        {
        }
    }

    [BepInPlugin("inf75.pvzcustomization", "PVZCustomization", "2.7")]
    public class CustomCore : BasePlugin
    {
        public static class TypeMgrExtra
        {
            public static List<PlantType> BigNut { get; set; } = [];
            public static List<ZombieType> BigZombie { get; set; } = [];
            public static List<PlantType> DoubleBoxPlants { get; set; } = [];
            public static List<ZombieType> EliteZombie { get; set; } = [];
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

        /// <summary>
        /// 添加融合配方
        /// </summary>
        /// <param name="target">目标植物</param>
        /// <param name="item1">亲本（地上长的）</param>
        /// <param name="item2">亲本（后融合上去的）</param>
        public static void AddFusion(int target, int item1, int item2) => CustomFusions.Add((target, item1, item2));

        /// <summary>
        /// 添加植物图鉴
        /// </summary>
        /// <param name="id">植物id</param>
        /// <param name="name">植物名称</param>
        /// <param name="description">植物介绍</param>
        public static void AddPlantAlmanacStrings(int id, string name, string description) =>
            PlantsAlmanac.Add((PlantType)id, (name, description));

        /// <summary>
        /// 添加僵尸图鉴
        /// </summary>
        /// <param name="id">僵尸id</param>
        /// <param name="name">僵尸名称</param>
        /// <param name="description">僵尸介绍</param>
        public static void AddZombieAlmanacStrings(int id, string name, string description) =>
            ZombiesAlmanac.Add((ZombieType)id, (name, description));

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
                using Stream stream =
                    assembly.GetManifestResourceStream(assembly.FullName!.Split(",")[0] + "." + name) ??
                    assembly.GetManifestResourceStream(name)!;
                using MemoryStream stream1 = new();
                stream.CopyTo(stream1);
                var ab = AssetBundle.LoadFromMemory(stream1.ToArray());
                ArgumentNullException.ThrowIfNull(ab);
                Instance.Value.Log.LogInfo($"Successfully load AssetBundle {name}.");
                return ab;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to load {name} \n{e}");
            }
        }

        public static AssetBundle GetAssetBundleFromPath(String path, String name)
        {
            try
            {
                AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(path);
                Instance.Value.Log.LogInfo($"Successfully load AssetBundle {name}.");
                return ab.assetBundle;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to load {name} \n{e}");
            }
        }
        public static void PlaySound(AudioClip audio, float volume = 1.0f)
        {
            GameObject soundObj = new("SoundPlayer");
            AudioSource audioSource = soundObj.AddComponent<AudioSource>();
            SoundCtrl newSoundCtrl = soundObj.AddComponent<SoundCtrl>();
            // 初始化组件
            audioSource.clip = audio;

            // 设置音量（应用全局音量调整）
            audioSource.volume = volume * GameAPP.gameSoundVolume;
            GameAPP.sound.Add(newSoundCtrl);
            // 播放音效
            audioSource.Play();
        }

        /// <summary>
        /// 注册自定义词条
        /// </summary>
        /// <param name="text">词条描述</param>
        /// <param name="buffType">词条类型(普通，强究，僵尸)</param>
        /// <param name="canUnlock">解锁条件</param>
        /// <param name="cost">词条商店花费积分</param>
        /// <param name="color">词条颜色</param>
        /// <param name="plantType">选词条时展示植物的类型</param>
        /// <returns>分到的词条id</returns>
        public static int RegisterCustomBuff(string text, BuffType buffType, Func<bool> canUnlock, int cost,
            string? color = null, PlantType plantType = PlantType.Nothing)
        {
            //if (color is not null) text = $"<color={color}>{text}</color>";
            switch (buffType)
            {
                case BuffType.AdvancedBuff:
                    {
                        int i = TravelMgr.advancedBuffs.Count;
                        CustomAdvancedBuffs.Add(i, (plantType, text, canUnlock, cost, color));
                        TravelMgr.advancedBuffs.Add(i, text);
                        return i;
                    }
                case BuffType.UltimateBuff:
                    {
                        int i = TravelMgr.ultimateBuffs.Count;
                        CustomUltimateBuffs.Add(i, (plantType, text, cost, color));
                        TravelMgr.ultimateBuffs.Add(i, text);
                        return i;
                    }
                case BuffType.Debuff:
                    {
                        int i = TravelMgr.debuffs.Count;
                        CustomDebuffs.Add(i, text);
                        TravelMgr.debuffs.Add(i, text);
                        return i;
                    }
                default:
                    return -1;
            }
        }

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
                        field_Public_PlantType_0 = (PlantType)id,
                        field_Public_Single_0 = attackInterval,
                        field_Public_Single_1 = produceInterval,
                        field_Public_Int32_0 = maxHealth,
                        field_Public_Single_2 = cd,
                        field_Public_Int32_1 = sun
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
                        field_Public_PlantType_0 = (PlantType)id,
                        field_Public_Single_0 = attackInterval,
                        field_Public_Single_1 = produceInterval,
                        field_Public_Int32_0 = maxHealth,
                        field_Public_Single_2 = cd,
                        field_Public_Int32_1 = sun
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
        /// 注册自定义点击植物事件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="action"></param>
        public static void RegisterCustomPlantClickEvent([NotNull] int id, [NotNull] Action<Plant> action) =>
            CustomPlantClicks.Add((PlantType)id, action);

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
        public static void RegisterCustomPlantSkin<TBase, TClass>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview,
            List<(int, int)> fusions, float attackInterval, float produceInterval, int attackDamage, int maxHealth,
            float cd, int sun)
            where TBase : Plant where TClass : MonoBehaviour
        {
            //植物预制体挂载植物脚本
            prefab.tag = "Plant";
            preview.tag = "Preview";
            prefab.AddComponent<TBase>().thePlantType = (PlantType)id;
            prefab.AddComponent<TClass>();
            CustomPlantsSkinActive.Add((PlantType)id, false);
            //植物id不重复才进行注册
            if (!CustomPlantsSkin.ContainsKey((PlantType)id))
            {
                //CustomPlantTypes.Add((PlantType)id);
                CustomPlantsSkin.Add((PlantType)id, new CustomPlantData()
                {
                    ID = id,
                    Prefab = prefab,
                    Preview = preview,
                    PlantData = new()
                    {
                        attackDamage = attackDamage,
                        field_Public_PlantType_0 = (PlantType)id,
                        //攻击间隔
                        field_Public_Single_0 = attackInterval,
                        //生产间隔
                        field_Public_Single_1 = produceInterval,
                        //最大HP
                        field_Public_Int32_0 = maxHealth,
                        //种植冷却
                        field_Public_Single_2 = cd,
                        //花费阳光
                        field_Public_Int32_1 = sun
                    }
                });
                foreach (var f in fusions)
                {
                    //添加融合配方
                    AddFusion(id, f.Item1, f.Item2);
                }
            }
            else
            {
                Instance.Value.Log.LogError($"Duplicate Plant ID: {id}");
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
        public static void RegisterCustomPlantSkin<TBase>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview,
            List<(int, int)> fusions, float attackInterval, float produceInterval, int attackDamage, int maxHealth,
            float cd, int sun)
            where TBase : Plant
        {
            prefab.tag = "Plant";
            preview.tag = "Preview";
            //植物预制体挂载植物脚本
            prefab.AddComponent<TBase>().thePlantType = (PlantType)id;
            CustomPlantsSkinActive.Add((PlantType)id, false);
            if (!CustomPlantsSkin.ContainsKey((PlantType)id))
            {
                //植物id不重复才进行注册
                //CustomPlantTypes.Add((PlantType)id);
                CustomPlantsSkin.Add((PlantType)id, new CustomPlantData()
                {
                    ID = id,
                    Prefab = prefab,
                    Preview = preview,
                    PlantData = new()
                    {
                        attackDamage = attackDamage,
                        //攻击间隔
                        field_Public_Single_0 = attackInterval,
                        //生产间隔
                        field_Public_Single_1 = produceInterval,
                        //最大HP
                        field_Public_Int32_0 = maxHealth,
                        //种植冷却
                        field_Public_Single_2 = cd,
                        //花费阳光
                        field_Public_Int32_1 = sun
                    }
                });
                foreach (var f in fusions)
                {
                    AddFusion(id, f.Item1, f.Item2);
                }
            }
            else
            {
                //添加融合配方
                Instance.Value.Log.LogError($"Duplicate Plant ID: {id}");
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
        public static void RegisterCustomPlantSkin<TBase>([NotNull] int id, [NotNull] GameObject prefab,
            [NotNull] GameObject preview, Action<TBase> ctor)
            where TBase : Plant
        {
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
                CustomPlantsSkin.Add((PlantType)id, new CustomPlantData()
                {
                    ID = id,
                    Prefab = prefab,
                    Preview = preview,
                    PlantData = null
                });
            }
            else
            {
                Instance.Value.Log.LogError($"Duplicate Plant ID: {id}");
            }
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
        public static void RegisterCustomUseFertilizeOnPlantEvent([NotNull] PlantType id, [NotNull] Action<Plant> callback) => CustomUseFertilize.Add(id, callback);

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

        /// <summary>
        /// 注册自定义卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型</param>
        /// <param name="parent">父对象，所有Func的返回值都应为想要设置的父对象</param>
        public static void RegisterCustomCard([NotNull] PlantType thePlantType, [NotNull] List<Func<Transform?>> parent)
        {
            if (!CustomCards.ContainsKey(thePlantType))
                CustomCards.Add(thePlantType, parent);
            else
                foreach (Func<Transform?> action in parent)
                    CustomCards[thePlantType].Add(action);
        }

        /// <summary>
        /// 注册自定义卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型，父对象将在实例化时自动设置</param>
        public static void RegisterCustomCard([NotNull] PlantType thePlantType)
        {
            if (!CustomCards.ContainsKey(thePlantType))
                CustomCards.Add(thePlantType, new List<Func<Transform?>>()
                {
                    () => Utils.GetNormalCardParent()
                });
            else
                CustomCards[thePlantType].Add(
                    () => Utils.GetNormalCardParent());
        }

        /// <summary>
        /// 注册自定义卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型，父对象将在实例化时自动设置</param>
        public static void RegisterCustomCardToColorfulCards([NotNull] PlantType thePlantType) => RegisterCustomCard(thePlantType, new List<Func<Transform?>>
        {
            () => Utils.GetColorfulCardParent()
        });

        /// <summary>
        /// 注册自定义普通卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型</param>
        /// <param name="parent">父对象，所有Func的返回值都应为想要设置的父对象</param>
        public static void RegisterCustomNormalCard([NotNull] PlantType thePlantType, List<Func<Transform?>> parent)
        {
            if (!CustomNormalCards.ContainsKey(thePlantType))
                CustomNormalCards.Add(thePlantType, parent);
            else
                foreach (Func<Transform?> action in parent)
                    CustomNormalCards[thePlantType].Add(action);
        }

        /// <summary>
        /// 注册自定义普通卡牌
        /// </summary>
        /// <param name="thePlantType">植物类型，父对象将在实例化时自动设置</param>
        public static void RegisterCustomNormalCard([NotNull] PlantType thePlantType)
        {
            if (!CustomNormalCards.ContainsKey(thePlantType))
                CustomNormalCards.Add(thePlantType, new List<Func<Transform?>>()
                {
                    () => Utils.GetNormalCardParent()
                });
            else
                CustomNormalCards[thePlantType].Add(
                    () => Utils.GetNormalCardParent());
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
        public static void RegisterCustomZombie<TBase, TClass>(ZombieType id, GameObject zombie, int spriteId,
            int theAttackDamage, int theMaxHealth, int theFirstArmorMaxHealth, int theSecondArmorMaxHealth)
            where TBase : Zombie where TClass : MonoBehaviour
        {
            zombie.AddComponent<TBase>().theZombieType = id;
            zombie.AddComponent<TClass>();

            if (!CustomZombieTypes.Contains(id))
            {
                CustomZombieTypes.Add(id);
                CustomZombies.Add(id, (zombie, spriteId, new()
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
        /// 注册植物大招
        /// </summary>
        /// <param name="id">植物id</param>
        /// <param name="cost">开大花费</param>
        /// <param name="skill">要运行的大招函数</param>
        public static void RegisterSuperSkill([NotNull] int id, [NotNull] Func<Plant, int> cost,
            [NotNull] Action<Plant> skill) => SuperSkills.Add((PlantType)id, (cost, skill));

        public void LateInit()
        {
            GameObject gameObject = new("TextureReplacer");
            ReplaceTextureRoutine = gameObject.AddComponent<CoroutineRunner>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            ReplaceTextureRoutine.StartCoroutine(TextureStore.ReplaceTexturesCoroutine());
        }

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<CoroutineRunner>();
            ClassInjector.RegisterTypeInIl2Cpp<CustomPlantMonoBehaviour>();
            ClassInjector.RegisterTypeInIl2Cpp<SelectCustomPlants>();
            ClassInjector.RegisterTypeInIl2Cpp<CheckCardState>();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Instance = new(this);
            TextureStore.Init();
        }

        public override bool Unload()
        {
            ReplaceTextureRoutine!.StopAllCoroutines();
            return true;
        }

        public static Dictionary<int, (PlantType, string, Func<bool>, int, string?)> CustomAdvancedBuffs { get; set; } = [];
        public static Dictionary<BulletType, GameObject> CustomBullets { get; set; } = [];
        public static Dictionary<int, string> CustomDebuffs { get; set; } = [];
        public static List<(int, int, int)> CustomFusions { get; set; } = [];
        public static List<CustomLevelData> CustomLevels { get; set; } = [];
        public static Dictionary<ParticleType, GameObject> CustomParticles { get; set; } = [];

        public static Dictionary<PlantType, Action<Plant>> CustomPlantClicks { get; set; } = [];

        public static Dictionary<PlantType, CustomPlantData> CustomPlants { get; set; } = [];

        /// <summary>
        /// 自定义植物皮肤列表
        /// </summary>
        public static Dictionary<PlantType, CustomPlantData> CustomPlantsSkin { get; set; } = [];

        /// <summary>
        /// 自定义皮肤是否激活
        /// </summary>
        public static Dictionary<PlantType, bool> CustomPlantsSkinActive { get; set; } = [];

        public static List<PlantType> CustomPlantTypes { get; set; } = [];

        public static Dictionary<int, Sprite> CustomSprites { get; set; } = [];

        public static Dictionary<int, (PlantType, string, int, string?)> CustomUltimateBuffs { get; set; } = [];

        public static Dictionary<(PlantType, BucketType), Action<Plant>> CustomUseItems { get; set; } = [];


        /// <summary>
        /// 自定义肥料物品事件列表
        /// </summary>
        public static Dictionary<PlantType, Action<Plant>> CustomUseFertilize { get; set; } = [];

        /// <summary>
        /// 自定义彩色卡牌列表
        /// </summary>
        public static Dictionary<PlantType, List<Func<Transform?>>> CustomCards { get; set; } = [];

        /// <summary>
        /// 自定义普通卡牌列表
        /// </summary>
        public static Dictionary<PlantType, List<Func<Transform?>>> CustomNormalCards { get; set; } = [];

        public static Dictionary<ZombieType, (GameObject, int, ZombieData.ZombieData_)> CustomZombies { get; set; } = [];

        public static List<ZombieType> CustomZombieTypes { get; set; } = [];

        public static Lazy<CustomCore> Instance { get; set; } = new();

        public static Dictionary<PlantType, (string, string)> PlantsAlmanac { get; set; } = [];

        /// <summary>
        /// 皮肤图鉴
        /// </summary>
        public static Dictionary<PlantType, (string, string)?> PlantsSkinAlmanac { get; set; } = [];

        public static CoroutineRunner? ReplaceTextureRoutine { get; set; } = null;
        public static Dictionary<PlantType, (Func<Plant, int>, Action<Plant>)> SuperSkills { get; set; } = [];
        public static Dictionary<ZombieType, (string, string)> ZombiesAlmanac { get; set; } = [];

        /// <summary>
        /// 存卡片检查的列表，用于管理Packet显示，你不应该使用它
        /// </summary>
        public static List<CheckCardState> checkBehaviours = new List<CheckCardState>();
    }
}