using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using Unity.VisualScripting;
using UnityEngine;

namespace CustomizeLib.MelonLoader
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
        public SceneType SceneType { get; set; } = SceneType.Day;
        public Func<List<PlantType>> SeedRainPlantTypes { get; set; } = () => [];
        public Func<int> Sun { get; set; } = () => 500;
        public Func<List<(int, int)>> UltiBuffs { get; set; } = () => [];
        public Func<List<int>> UnlockPlants { get; set; } = () => [];
        public Func<int> WaveCount { get; set; } = () => 10;
        public Func<int> ZombieHealthRate { get; set; } = () => 1;
        public Func<List<ZombieType>> ZombieList { get; set; } = () => [];
    }

    /// <summary>
    /// 二创植物图鉴数据
    /// </summary>
    public struct CustomPlantAlmanac
    {
        public string Description { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// 二创植物数据
    /// </summary>
    public struct CustomPlantData
    {
        public int ID { get; set; }
        public PlantDataLoader.PlantData_? PlantData { get; set; }
        public GameObject Prefab { get; set; }
        public GameObject Preview { get; set; }
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

    public static class Extensions
    {
        /// <summary>
        /// 换皮肤时备份植物特性
        /// </summary>
        /// <param name="typeMgrExtraSkinFromJson"></param>
        /// <param name="plantType"></param>
        public static void AddValueToTypeMgrExtraSkinBackup(this CustomTypeMgrExtraSkin typeMgrExtraSkinFromJson,
            PlantType plantType)
        {
            CustomCore.TypeMgrExtraSkinBackup.BigNut.Add(plantType, typeMgrExtraSkinFromJson.BigNut);
            //CustomCore.TypeMgrExtraSkinBackup.BigZombie.Add(plantType, typeMgrExtraSkinFromJson.BigZombie);
            CustomCore.TypeMgrExtraSkinBackup.DoubleBoxPlants.Add(plantType, typeMgrExtraSkinFromJson.DoubleBoxPlants);
            //CustomCore.TypeMgrExtraSkinBackup.EliteZombie.Add(plantType, typeMgrExtraSkinFromJson.EliteZombie);
            CustomCore.TypeMgrExtraSkinBackup.FlyingPlants.Add(plantType, typeMgrExtraSkinFromJson.FlyingPlants);
            //CustomCore.TypeMgrExtraSkinBackup.IsAirZombie.Add(plantType, typeMgrExtraSkinFromJson.IsAirZombie);
            CustomCore.TypeMgrExtraSkinBackup.IsCaltrop.Add(plantType, typeMgrExtraSkinFromJson.IsCaltrop);
            CustomCore.TypeMgrExtraSkinBackup.IsCustomPlant.Add(plantType, typeMgrExtraSkinFromJson.IsCustomPlant);
            CustomCore.TypeMgrExtraSkinBackup.IsFirePlant.Add(plantType, typeMgrExtraSkinFromJson.IsFirePlant);
            CustomCore.TypeMgrExtraSkinBackup.IsIcePlant.Add(plantType, typeMgrExtraSkinFromJson.IsIcePlant);
            CustomCore.TypeMgrExtraSkinBackup.IsMagnetPlants.Add(plantType, typeMgrExtraSkinFromJson.IsMagnetPlants);
            CustomCore.TypeMgrExtraSkinBackup.IsNut.Add(plantType, typeMgrExtraSkinFromJson.IsNut);
            CustomCore.TypeMgrExtraSkinBackup.IsPlantern.Add(plantType, typeMgrExtraSkinFromJson.IsPlantern);
            CustomCore.TypeMgrExtraSkinBackup.IsPot.Add(plantType, typeMgrExtraSkinFromJson.IsPot);
            CustomCore.TypeMgrExtraSkinBackup.IsPotatoMine.Add(plantType, typeMgrExtraSkinFromJson.IsPotatoMine);
            CustomCore.TypeMgrExtraSkinBackup.IsPuff.Add(plantType, typeMgrExtraSkinFromJson.IsPuff);
            CustomCore.TypeMgrExtraSkinBackup.IsPumpkin.Add(plantType, typeMgrExtraSkinFromJson.IsPumpkin);
            CustomCore.TypeMgrExtraSkinBackup.IsSmallRangeLantern.Add(plantType, typeMgrExtraSkinFromJson.IsSmallRangeLantern);
            CustomCore.TypeMgrExtraSkinBackup.IsSpecialPlant.Add(plantType, typeMgrExtraSkinFromJson.IsSpecialPlant);
            CustomCore.TypeMgrExtraSkinBackup.IsSpickRock.Add(plantType, typeMgrExtraSkinFromJson.IsSpickRock);
            CustomCore.TypeMgrExtraSkinBackup.IsTallNut.Add(plantType, typeMgrExtraSkinFromJson.IsTallNut);
            CustomCore.TypeMgrExtraSkinBackup.IsTangkelp.Add(plantType, typeMgrExtraSkinFromJson.IsTangkelp);
            CustomCore.TypeMgrExtraSkinBackup.IsWaterPlant.Add(plantType, typeMgrExtraSkinFromJson.IsWaterPlant);
            //CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie.Add(plantType,typeMgrExtraSkinFromJson.NotRandomBungiZombie);
            //CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie.Add(plantType, typeMgrExtraSkinFromJson.NotRandomZombie);
            //CustomCore.TypeMgrExtraSkinBackup.UltimateZombie.Add(plantType, typeMgrExtraSkinFromJson.UltimateZombie);
            CustomCore.TypeMgrExtraSkinBackup.UmbrellaPlants.Add(plantType, typeMgrExtraSkinFromJson.UmbrellaPlants);
            //CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie.Add(plantType, typeMgrExtraSkinFromJson.UselessHypnoZombie);
            //CustomCore.TypeMgrExtraSkinBackup.WaterZombie.Add(plantType, typeMgrExtraSkinFromJson.WaterZombie);
        }

        /// <summary>
        /// 防拆解
        /// </summary>
        /// <param name="plant"></param>
        public static void DisableDisMix(this Plant plant) =>
                    (plant.firstParent, plant.secondParent) = (PlantType.Nothing, PlantType.Nothing);

        /// <summary>
        /// 卡槽中植物贴图换皮肤
        /// </summary>
        /// <param name="parent"></param>
        public static void FindCardUIAndChangeSprite(this Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                CardUI cardUI = parent.GetChild(i).GetComponent<CardUI>();
                if (cardUI != null)
                {
                    Mouse.Instance.ChangeCardSprite((PlantType)cardUI.theSeedType, cardUI);
                }

                // 递归查找子物体的子物体
                FindCardUIAndChangeSprite(parent.GetChild(i));
            }
        }

        //递归，找shoot，但是一些奇怪的植物不行
        public static void FindShoot(this Plant plant, Transform parent)
        {
            // 遍历当前对象的所有组件
            Component[] components = parent.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component.name == "Shoot" || component.name == "Shoot1")
                {
                    plant.shoot = component.transform;
                }

                if (component.name == "Shoot2")
                {
                    plant.shoot2 = component.transform;
                }
            }

            // 递归遍历所有子对象
            for (int i = 0; i < parent.childCount; i++)
            {
                plant.FindShoot(parent.GetChild(i));
            }
        }

        /// <summary>
        /// 从ab包里读取资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="ab">ab包对象</param>
        /// <param name="name">对象名称</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T GetAsset<T>(this AssetBundle ab, string name) where T : UnityEngine.Object
        {
            foreach (var ase in ab.LoadAllAssetsAsync().allAssets)
            {
                if (ase.TryCast<T>()?.name == name)
                {
                    return ase.Cast<T>();
                }
            }

            throw new ArgumentException($"Could not find {name} from {ab.name}");
        }

        public static T GetRandomItem<T>(this IList<T> list) => list[UnityEngine.Random.RandomRangeInt(0, list.Count)];

        /// <summary>
        /// 获取僵尸总血量
        /// </summary>
        /// <param name="zombie"></param>
        /// <returns></returns>
        public static int GetTotalHealth(this Zombie zombie) =>
            (int)zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;

        /// <summary>
        /// 场上是否存在某类型对象
        /// </summary>
        /// <typeparam name="T">要检查的对象类型</typeparam>
        /// <param name="board"></param>
        /// <returns></returns>
        public static bool ObjectExist<T>(this Board board) =>
            board.GameObject().transform.GetComponentsInChildren<T>().Length > 0;

        /// <summary>
        /// 交换换皮肤植物特性
        /// </summary>
        /// <param name="plantType"></param>
        public static void SwapTypeMgrExtraSkinAndBackup(PlantType plantType)
        {
            // BigNut
            if (CustomCore.TypeMgrExtraSkin.BigNut.TryGetValue(plantType, out int value1))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.BigNut.TryAdd(plantType, value1))
                {
                    CustomCore.TypeMgrExtraSkin.BigNut.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.BigNut.TryGetValue(plantType, out int value2))
            {
                if (CustomCore.TypeMgrExtraSkin.BigNut.TryAdd(plantType, value2))
                {
                    CustomCore.TypeMgrExtraSkinBackup.BigNut.Remove(plantType);
                }
            }

            // // BigZombie
            // if (CustomCore.TypeMgrExtraSkin.BigZombie.TryGetValue(plantType, out int value3))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.BigZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.BigZombie[plantType] = value3;
            //         CustomCore.TypeMgrExtraSkin.BigZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.BigZombie.TryGetValue(plantType, out int value4))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.BigZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.BigZombie[plantType] = value4;
            //         CustomCore.TypeMgrExtraSkinBackup.BigZombie.Remove(plantType);
            //     }
            // }

            // DoubleBoxPlants
            if (CustomCore.TypeMgrExtraSkin.DoubleBoxPlants.TryGetValue(plantType, out int value5))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.DoubleBoxPlants.TryAdd(plantType, value5))
                {
                    CustomCore.TypeMgrExtraSkin.DoubleBoxPlants.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.DoubleBoxPlants.TryGetValue(plantType, out int value6))
            {
                if (CustomCore.TypeMgrExtraSkin.DoubleBoxPlants.TryAdd(plantType, value6))
                {
                    CustomCore.TypeMgrExtraSkinBackup.DoubleBoxPlants.Remove(plantType);
                }
            }

            // // EliteZombie
            // if (CustomCore.TypeMgrExtraSkin.EliteZombie.TryGetValue(plantType, out int value7))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.EliteZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.EliteZombie[plantType] = value7;
            //         CustomCore.TypeMgrExtraSkin.EliteZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.EliteZombie.TryGetValue(plantType, out int value8))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.EliteZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.EliteZombie[plantType] = value8;
            //         CustomCore.TypeMgrExtraSkinBackup.EliteZombie.Remove(plantType);
            //     }
            // }

            // FlyingPlants
            if (CustomCore.TypeMgrExtraSkin.FlyingPlants.TryGetValue(plantType, out int value9))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.FlyingPlants.TryAdd(plantType, value9))
                {
                    CustomCore.TypeMgrExtraSkin.FlyingPlants.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.FlyingPlants.TryGetValue(plantType, out int value10))
            {
                if (CustomCore.TypeMgrExtraSkin.FlyingPlants.TryAdd(plantType, value10))
                {
                    CustomCore.TypeMgrExtraSkinBackup.FlyingPlants.Remove(plantType);
                }
            }

            // // IsAirZombie
            // if (CustomCore.TypeMgrExtraSkin.IsAirZombie.TryGetValue(plantType, out int value11))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.IsAirZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.IsAirZombie[plantType] = value11;
            //         CustomCore.TypeMgrExtraSkin.IsAirZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.IsAirZombie.TryGetValue(plantType, out int value12))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.IsAirZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.IsAirZombie[plantType] = value12;
            //         CustomCore.TypeMgrExtraSkinBackup.IsAirZombie.Remove(plantType);
            //     }
            // }

            // IsCaltrop
            if (CustomCore.TypeMgrExtraSkin.IsCaltrop.TryGetValue(plantType, out int value13))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsCaltrop.TryAdd(plantType, value13))
                {
                    CustomCore.TypeMgrExtraSkin.IsCaltrop.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsCaltrop.TryGetValue(plantType, out int value14))
            {
                if (CustomCore.TypeMgrExtraSkin.IsCaltrop.TryAdd(plantType, value14))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsCaltrop.Remove(plantType);
                }
            }

            // IsCustomPlant
            if (CustomCore.TypeMgrExtraSkin.IsCustomPlant.TryGetValue(plantType, out int value15))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsCustomPlant.TryAdd(plantType, value15))
                {
                    CustomCore.TypeMgrExtraSkin.IsCustomPlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsCustomPlant.TryGetValue(plantType, out int value16))
            {
                if (CustomCore.TypeMgrExtraSkin.IsCustomPlant.TryAdd(plantType, value16))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsCustomPlant.Remove(plantType);
                }
            }

            // IsFirePlant
            if (CustomCore.TypeMgrExtraSkin.IsFirePlant.TryGetValue(plantType, out int value17))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsFirePlant.TryAdd(plantType, value17))
                {
                    CustomCore.TypeMgrExtraSkin.IsFirePlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsFirePlant.TryGetValue(plantType, out int value18))
            {
                if (CustomCore.TypeMgrExtraSkin.IsFirePlant.TryAdd(plantType, value18))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsFirePlant.Remove(plantType);
                }
            }

            // IsIcePlant
            if (CustomCore.TypeMgrExtraSkin.IsIcePlant.TryGetValue(plantType, out int value19))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsIcePlant.TryAdd(plantType, value19))
                {
                    CustomCore.TypeMgrExtraSkin.IsIcePlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsIcePlant.TryGetValue(plantType, out int value20))
            {
                if (CustomCore.TypeMgrExtraSkin.IsIcePlant.TryAdd(plantType, value20))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsIcePlant.Remove(plantType);
                }
            }

            // IsMagnetPlants
            if (CustomCore.TypeMgrExtraSkin.IsMagnetPlants.TryGetValue(plantType, out int value21))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsMagnetPlants.TryAdd(plantType, value21))
                {
                    CustomCore.TypeMgrExtraSkin.IsMagnetPlants.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsMagnetPlants.TryGetValue(plantType, out int value22))
            {
                if (CustomCore.TypeMgrExtraSkin.IsMagnetPlants.TryAdd(plantType, value22))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsMagnetPlants.Remove(plantType);
                }
            }

            // IsNut
            if (CustomCore.TypeMgrExtraSkin.IsNut.TryGetValue(plantType, out int value23))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsNut.TryAdd(plantType, value23))
                {
                    CustomCore.TypeMgrExtraSkin.IsNut.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsNut.TryGetValue(plantType, out int value24))
            {
                if (CustomCore.TypeMgrExtraSkin.IsNut.TryAdd(plantType, value24))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsNut.Remove(plantType);
                }
            }

            // IsPlantern
            if (CustomCore.TypeMgrExtraSkin.IsPlantern.TryGetValue(plantType, out int value25))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPlantern.TryAdd(plantType, value25))
                {
                    CustomCore.TypeMgrExtraSkin.IsPlantern.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPlantern.TryGetValue(plantType, out int value26))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPlantern.TryAdd(plantType, value26))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPlantern.Remove(plantType);
                }
            }

            // IsPot
            if (CustomCore.TypeMgrExtraSkin.IsPot.TryGetValue(plantType, out int value27))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPot.TryAdd(plantType, value27))
                {
                    CustomCore.TypeMgrExtraSkin.IsPot.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPot.TryGetValue(plantType, out int value28))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPot.TryAdd(plantType, value28))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPot.Remove(plantType);
                }
            }

            // IsPotatoMine
            if (CustomCore.TypeMgrExtraSkin.IsPotatoMine.TryGetValue(plantType, out int value29))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPotatoMine.TryAdd(plantType, value29))
                {
                    CustomCore.TypeMgrExtraSkin.IsPotatoMine.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPotatoMine.TryGetValue(plantType, out int value30))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPotatoMine.TryAdd(plantType, value30))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPotatoMine.Remove(plantType);
                }
            }

            // IsPuff
            if (CustomCore.TypeMgrExtraSkin.IsPuff.TryGetValue(plantType, out int value31))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPuff.TryAdd(plantType, value31))
                {
                    CustomCore.TypeMgrExtraSkin.IsPuff.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPuff.TryGetValue(plantType, out int value32))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPuff.TryAdd(plantType, value32))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPuff.Remove(plantType);
                }
            }

            // IsPumpkin
            if (CustomCore.TypeMgrExtraSkin.IsPumpkin.TryGetValue(plantType, out int value33))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsPumpkin.TryAdd(plantType, value33))
                {
                    CustomCore.TypeMgrExtraSkin.IsPumpkin.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsPumpkin.TryGetValue(plantType, out int value34))
            {
                if (CustomCore.TypeMgrExtraSkin.IsPumpkin.TryAdd(plantType, value34))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsPumpkin.Remove(plantType);
                }
            }

            // IsSmallRangeLantern
            if (CustomCore.TypeMgrExtraSkin.IsSmallRangeLantern.TryGetValue(plantType, out int value35))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsSmallRangeLantern.TryAdd(plantType, value35))
                {
                    CustomCore.TypeMgrExtraSkin.IsSmallRangeLantern.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsSmallRangeLantern.TryGetValue(plantType, out int value36))
            {
                if (CustomCore.TypeMgrExtraSkin.IsSmallRangeLantern.TryAdd(plantType, value36))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsSmallRangeLantern.Remove(plantType);
                }
            }

            // IsSpecialPlant
            if (CustomCore.TypeMgrExtraSkin.IsSpecialPlant.TryGetValue(plantType, out int value37))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsSpecialPlant.TryAdd(plantType, value37))
                {
                    CustomCore.TypeMgrExtraSkin.IsSpecialPlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsSpecialPlant.TryGetValue(plantType, out int value38))
            {
                if (CustomCore.TypeMgrExtraSkin.IsSpecialPlant.TryAdd(plantType, value38))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsSpecialPlant.Remove(plantType);
                }
            }

            // IsSpickRock
            if (CustomCore.TypeMgrExtraSkin.IsSpickRock.TryGetValue(plantType, out int value39))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsSpickRock.TryAdd(plantType, value39))
                {
                    CustomCore.TypeMgrExtraSkin.IsSpickRock.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsSpickRock.TryGetValue(plantType, out int value40))
            {
                if (CustomCore.TypeMgrExtraSkin.IsSpickRock.TryAdd(plantType, value40))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsSpickRock.Remove(plantType);
                }
            }

            // IsTallNut
            if (CustomCore.TypeMgrExtraSkin.IsTallNut.TryGetValue(plantType, out int value41))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsTallNut.TryAdd(plantType, value41))
                {
                    CustomCore.TypeMgrExtraSkin.IsTallNut.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsTallNut.TryGetValue(plantType, out int value42))
            {
                if (CustomCore.TypeMgrExtraSkin.IsTallNut.TryAdd(plantType, value42))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsTallNut.Remove(plantType);
                }
            }

            // IsTangkelp
            if (CustomCore.TypeMgrExtraSkin.IsTangkelp.TryGetValue(plantType, out int value43))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsTangkelp.TryAdd(plantType, value43))
                {
                    CustomCore.TypeMgrExtraSkin.IsTangkelp.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsTangkelp.TryGetValue(plantType, out int value44))
            {
                if (CustomCore.TypeMgrExtraSkin.IsTangkelp.TryAdd(plantType, value44))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsTangkelp.Remove(plantType);
                }
            }

            // IsWaterPlant
            if (CustomCore.TypeMgrExtraSkin.IsWaterPlant.TryGetValue(plantType, out int value45))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.IsWaterPlant.TryAdd(plantType, value45))
                {
                    CustomCore.TypeMgrExtraSkin.IsWaterPlant.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.IsWaterPlant.TryGetValue(plantType, out int value46))
            {
                if (CustomCore.TypeMgrExtraSkin.IsWaterPlant.TryAdd(plantType, value46))
                {
                    CustomCore.TypeMgrExtraSkinBackup.IsWaterPlant.Remove(plantType);
                }
            }

            // // NotRandomBungiZombie
            // if (CustomCore.TypeMgrExtraSkin.NotRandomBungiZombie.TryGetValue(plantType, out int value47))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie[plantType] = value47;
            //         CustomCore.TypeMgrExtraSkin.NotRandomBungiZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie.TryGetValue(plantType, out int value48))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.NotRandomBungiZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.NotRandomBungiZombie[plantType] = value48;
            //         CustomCore.TypeMgrExtraSkinBackup.NotRandomBungiZombie.Remove(plantType);
            //     }
            // }

            // // NotRandomZombie
            // if (CustomCore.TypeMgrExtraSkin.NotRandomZombie.TryGetValue(plantType, out int value49))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie[plantType] = value49;
            //         CustomCore.TypeMgrExtraSkin.NotRandomZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie.TryGetValue(plantType, out int value50))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.NotRandomZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.NotRandomZombie[plantType] = value50;
            //         CustomCore.TypeMgrExtraSkinBackup.NotRandomZombie.Remove(plantType);
            //     }
            // }

            // // UltimateZombie
            // if (CustomCore.TypeMgrExtraSkin.UltimateZombie.TryGetValue(plantType, out int value51))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.UltimateZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.UltimateZombie[plantType] = value51;
            //         CustomCore.TypeMgrExtraSkin.UltimateZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.UltimateZombie.TryGetValue(plantType, out int value52))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.UltimateZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.UltimateZombie[plantType] = value52;
            //         CustomCore.TypeMgrExtraSkinBackup.UltimateZombie.Remove(plantType);
            //     }
            // }

            // UmbrellaPlants
            if (CustomCore.TypeMgrExtraSkin.UmbrellaPlants.TryGetValue(plantType, out int value53))
            {
                if (CustomCore.TypeMgrExtraSkinBackup.UmbrellaPlants.TryAdd(plantType, value53))
                {
                    CustomCore.TypeMgrExtraSkin.UmbrellaPlants.Remove(plantType);
                }
            }
            else if (CustomCore.TypeMgrExtraSkinBackup.UmbrellaPlants.TryGetValue(plantType, out int value54))
            {
                if (CustomCore.TypeMgrExtraSkin.UmbrellaPlants.TryAdd(plantType, value54))
                {
                    CustomCore.TypeMgrExtraSkinBackup.UmbrellaPlants.Remove(plantType);
                }
            }

            // // UselessHypnoZombie
            // if (CustomCore.TypeMgrExtraSkin.UselessHypnoZombie.TryGetValue(plantType, out int value55))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie[plantType] = value55;
            //         CustomCore.TypeMgrExtraSkin.UselessHypnoZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie.TryGetValue(plantType, out int value56))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.UselessHypnoZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.UselessHypnoZombie[plantType] = value56;
            //         CustomCore.TypeMgrExtraSkinBackup.UselessHypnoZombie.Remove(plantType);
            //     }
            // }

            // // WaterZombie
            // if (CustomCore.TypeMgrExtraSkin.WaterZombie.TryGetValue(plantType, out int value57))
            // {
            //     if (!CustomCore.TypeMgrExtraSkinBackup.WaterZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkinBackup.WaterZombie[plantType] = value57;
            //         CustomCore.TypeMgrExtraSkin.WaterZombie.Remove(plantType);
            //     }
            // }
            // else if (CustomCore.TypeMgrExtraSkinBackup.WaterZombie.TryGetValue(plantType, out int value58))
            // {
            //     if (!CustomCore.TypeMgrExtraSkin.WaterZombie.ContainsKey(plantType))
            //     {
            //         CustomCore.TypeMgrExtraSkin.WaterZombie[plantType] = value58;
            //         CustomCore.TypeMgrExtraSkinBackup.WaterZombie.Remove(plantType);
            //     }
            // }
        }

        public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this List<T> list)
        {
            Il2CppSystem.Collections.Generic.List<T> newList = new();
            foreach (T t in list)
            {
                newList.Add(t);
            }
            return newList;
        }
    }

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

        public static LevelType CustomLevelType => (LevelType)66;
    }

    [RegisterTypeInIl2Cpp]
    public class CustomizeLib : MonoBehaviour
    {
        public CustomizeLib() : base(ClassInjector.DerivedConstructorPointer<CustomizeLib>()) => ClassInjector.DerivedConstructorBody(this);

        public CustomizeLib(IntPtr i) : base(i)
        {
        }

        public CustomCore? CustomCore { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; }
    }

    // json对象
    public class JsonSkinObject
    {
        public Dictionary<int, int> CustomBulletType { get; set; } = [];

        public CustomPlantData CustomPlantData { get; set; }
        public CustomPlantAlmanac PlantAlmanac { get; set; }
        public CustomTypeMgrExtraSkin TypeMgrExtraSkin { get; set; }
    }
}