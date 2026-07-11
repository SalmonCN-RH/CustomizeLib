using AlmanacData;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using static Plant;
using static UltimatePortalSnowGatling.BepInEx.UltimatePortalSnowGatling;

namespace UltimatePortalSnowGatling.BepInEx
{
    [BepInPlugin("salmon.ultimateportalsnowgatling", "UltimatePortalSnowGatling", "1.0")]
    public class UltimatePortalSnowGatling : BasePlugin
    {
        public static PlantType theNewPlantType = (PlantType)1939;
        public static BulletType theNewBulletType = (BulletType)1939;
        public static Dictionary<(PlantType, PlantType), PlantType> Recipes = new() // 配方
        {
            { (PlantType.SnowGatling, PlantType.PortalPea), theNewPlantType },
            { (PlantType.PortalPea, PlantType.SnowGatling), theNewPlantType }
        };

        /// <summary>
        /// 词条起始ID
        /// </summary>
        public const int BuffStartID = -1000000;
        /// <summary>
        /// (Enum名, (索引, 值))
        /// </summary>
        public static readonly Dictionary<string, (int, UltiBuff?)> UltiBuffStrings = new()
        {
            { "UltimatePortalSnowGatling_Ulti_0", (0, null) },
            { "UltimatePortalSnowGatling_Ulti_1", (2, null) }
        };
        /// <summary>
        /// (Enum名,值)
        /// </summary>
        public static readonly Dictionary<string, (int, TravelUnlocks?)> UnlockBuffStrings = new()
        {
            { "UltimatePortalSnowGatling_Unlock_0", (0, null) }
        };
        /// <summary>
        /// (Enum值的名称, 描述)
        /// </summary>
        public static readonly Dictionary<string, string> BuffDesc = new()
        {
            { "UltimatePortalSnowGatling_Ulti_0", "冰河时代：究极超时空冰河射手的子弹能够无限穿透处于传送状态下的僵尸。" },
            { "UltimatePortalSnowGatling_Ulti_1", "远古寒芒：命中僵尸立即冻结的次数下降到5次，场上同时处于传送状态和冻结状态的僵尸死亡后能够触发一次寒冰菇效果。" },
            { "UltimatePortalSnowGatling_Unlock_0",
                "解锁<color=red>究极超时空冰河射手</color>\n寒冰机枪射手+超时空豌豆射手" }
        };
        public static List<UltiBuff> UltiBuffs = new();
        public static List<TravelUnlocks> UnlockBuffs = new();

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static void LoadUltimatePortalSnowGatling()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ZombieExtraData>();
            foreach (var item in GetAssetBundle("ultimateportalsnowgatling").LoadAllAssetsAsync().allAssets)
            {
                if (item.TryCast<GameObject>()?.name == "UltimatePortalSnowGatlingPrefab")
                {
                    // 初始化
                    GameAPP.resourcesManager.plantPrefabs[theNewPlantType] = item.Cast<GameObject>();
                    GameAPP.resourcesManager.allPlants.Add(theNewPlantType);
                    var list = new Il2CppSystem.Collections.Generic.List<GameObject>();
                    list.Add(GameAPP.resourcesManager.plantPrefabs[theNewPlantType]);
                    GameAPP.resourcesManager._plantPrefabs.Add(theNewPlantType, list);
                    PlantDataManager.PlantData_Default.Add(theNewPlantType, new PlantDataManager.PlantData()
                    {
                        attackDamage = 80,
                        attackInterval = 1.5f,
                        cd = 7.5f,
                        cost = 675,
                        maxHealth = 300,
                        produceInterval = 0f,
                        thePlantType = theNewPlantType
                    }); // 设置植物数据
                    var plant = GameAPP.resourcesManager.plantPrefabs[theNewPlantType].AddComponent<SnowPeaShooter>();
                    plant.thePlantType = theNewPlantType;
                    plant.shoot = plant.transform.GetChild(0).FindChild("Shoot"); // 设置shoot
                    plant.gameObject.tag = "Plant"; // 设置tag
                    plant.gameObject.layer = LayerMask.NameToLayer("Plant"); // 设置layer

                }
                if (item.TryCast<GameObject>()?.name == "UltimatePortalSnowGatlingPreview")
                {
                    GameAPP.resourcesManager.plantPreviews[theNewPlantType] = item.Cast<GameObject>();
                    GameAPP.resourcesManager.plantPreviews[theNewPlantType].tag = "Preview"; //设置tag
                    var list = new Il2CppSystem.Collections.Generic.List<GameObject>();
                    list.Add(GameAPP.resourcesManager.plantPreviews[theNewPlantType]);
                    GameAPP.resourcesManager._plantPreviews.Add(theNewPlantType, list);
                }
                if (item.TryCast<GameObject>()?.name == "Bullet_ultimatePortalSnowGatlingPea")
                {
                    GameAPP.resourcesManager.bulletPrefabs[theNewBulletType] = item.Cast<GameObject>();
                    GameAPP.resourcesManager.bulletPrefabs[theNewBulletType].AddComponent<Bullet_portalPea>().theBulletType = theNewBulletType;
                    GameAPP.resourcesManager.allBullets.Add(theNewBulletType);
                }
            }

            foreach (var ((a, b), c) in Recipes)
                MixData.AddOrderedRecipe(a, b, c); // 底, 融合上去的, 目标

            // 词条
            // 注册词条进enum, api bug导致实际值跟注册填的不一样
            // 转换规则：(name, StartID + Index)
            EnumInjector.InjectEnumValues<UltiBuff>(UltiBuffStrings.ToDictionary(kvp => kvp.Key, kvp => (object)(BuffStartID + kvp.Value.Item1)));
            EnumInjector.InjectEnumValues<TravelUnlocks>(UnlockBuffStrings.ToDictionary(kvp => kvp.Key, kvp => (object)(BuffStartID + kvp.Value.Item1)));
            // 处理UltiBuff的注册
            foreach (var (key, value) in UltiBuffStrings)
            {
                var enumVal = Il2CppSystem.Enum.Parse(Il2CppType.From(typeof(UltiBuff)), key).Unbox<UltiBuff>();
                UltiBuffStrings[key] = (value.Item1, enumVal); // 将注册的Enum值重新写回到字典
                UltiBuffs.Add(enumVal);
            }
            // 处理TravelUnlocks的注册
            foreach (var (key, value) in UnlockBuffStrings)
            {
                var enumVal = Il2CppSystem.Enum.Parse(Il2CppType.From(typeof(TravelUnlocks)), key).Unbox<TravelUnlocks>();
                UnlockBuffStrings[key] = (value.Item1, enumVal); // 将注册的Enum值重新写回到字典
                UnlockBuffs.Add(enumVal);
            }
        }

        public static AssetBundle GetAssetBundle(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            try
            {
                using Stream stream =
                    assembly.GetManifestResourceStream(assembly.FullName!.Split(",")[0] + "." + name) ??
                    assembly.GetManifestResourceStream(name)!;
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

    public class ZombieExtraData : MonoBehaviour
    {
        public int hitCount = 0;
        public Zombie zombie = null!;

        public void AddHitCount(Zombie init)
        {
            if (zombie == null) zombie = init;
            hitCount++;
            if (hitCount % (Lawnf.TravelUltimate(UltiBuffs[1]) ? 5 : 10) == 0) // 如果有词条2判定变成5
            {
                hitCount = 0;
                zombie.SetFreeze(5f);
            }
        }
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            LoadUltimatePortalSnowGatling();
        }
    } // 初始化

    [HarmonyPatch(typeof(SnowPeaShooter))]
    public static class SnowPeaShooterPatch
    {
        [HarmonyPatch(nameof(SnowPeaShooter.GetBulletType))]
        [HarmonyPostfix]
        public static void PostGetBulletType(SnowPeaShooter __instance, ref BulletType __result)
        {
            if (__instance.thePlantType == theNewPlantType)
                __result = theNewBulletType;
        }
    } // 子弹类型

    [HarmonyPatch(typeof(Bullet_portalPea))]
    public static class Bullet_portalPeaPatch
    {
        [HarmonyPatch(nameof(Bullet_portalPea.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_portalPea __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == theNewBulletType)
            {
                var portal = zombie.HasBuff(EffectType.Portal);
                zombie.GetOrAddComponent<ZombieExtraData>().AddHitCount(zombie);
                zombie.SetCold(30f);
                zombie.SetPortaled();
                zombie.AddfreezeLevel(10, 0);
                var damage = __instance.Damage;
                if (zombie.freezeSpeed == 0f) // 如果是冻结状态
                    damage *= 4;
                zombie.TakeDamage(damage, __instance.Cast<IDamageMaker>(), DamageType.Ice, __instance.fromType);

                CreateParticle.SetParticle(109, __instance.transform.position, __instance.theBulletRow);

                __instance.PlaySound(zombie);
                if (!portal || !Lawnf.TravelUltimate(UltiBuffs[0]))
                    __instance.Die();
                __instance.hit = false;
                return false;
            }
            return true;
        }
    } // 子弹

    [HarmonyPatch(typeof(AlmanacDataLoader))]
    public static class AlmanacDataLoaderPatch
    {
        [HarmonyPatch(nameof(AlmanacDataLoader.LoadPlantData))]
        [HarmonyPostfix]
        public static void PostLoadPlantData()
        {
            if (AlmanacDataLoader.plantDatas.ContainsKey(theNewPlantType)) return;
            var data = new PlantInfo
            {
                name = "究极超时空冰河射手",
                info = "从远古苏醒的究极超时空冰河射手，将时间永久冻结",
                introduce =
                    $"<color=#3D1400>使用条件：</color><color=red>旅行模式购买配方</color>\n" +
                    $"<color=#3D1400>融合配方：</color><color=red>寒冰机枪射手+超时空豌豆射手</color>\n" +
                    $"<color=#3D1400>韧性：</color><color=red>300</color>\n" +
                    $"<color=#3D1400>伤害：</color><color=red>80x4/1.5s</color>\n" +
                    $"<color=#3D1400>特点：" +
                    $"①</color><color=red>攻击施加寒冷状态，赋予10点冻结值与1.5传送秒状态，对冰冻的僵尸造成4倍伤害。</color>\n" +
                    $"<color=#3D1400>②</color><color=red>攻击命中同一僵尸10次会立即冻结。</color>\n" +
                    $"<color=#3D1400>词条1：</color><color=red>冰河时代：究极超时空冰河射手的子弹能够无限穿透处于传送状态下的僵尸。</color>\n" +
                    $"<color=#3D1400>词条2：</color><color=red>远古寒芒：命中僵尸立即冻结的次数下降到5次，场上同时处于传送状态和冻结状态的僵尸死亡后能够触发一次寒冰菇效果。</color>\n\n" +
                    $"<color=#3D1400>究极超时空冰河射手并不是当今的植物，而是从遥远的冰河时代穿越而来，他头盔上的缎带便是用猛犸象的皮毛制作而成……</color>",
                seedType = (int)theNewPlantType
            };
            AlmanacDataLoader.plantDatas.Add(theNewPlantType, data);
        }
    } // 图鉴

    [HarmonyPatch(typeof(TravelHelper))]
    public static class TravelHelperPatch
    {
        [HarmonyPatch(nameof(TravelHelper.GetAllUltimatePlantTypes))]
        [HarmonyPostfix]
        public static void PostGetAllUltimatePlantTypes(ref bool isStrongUltimate, ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            if (isStrongUltimate)
                __result.Add(theNewPlantType);
        }
    } // 强究列表获取

    [HarmonyPatch(typeof(CreatePlant))]
    public static class CreatePlantPatch
    {
        [HarmonyPatch(nameof(CreatePlant.LimTravel))]
        [HarmonyPostfix]
        public static void PostLimTravel(CreatePlant __instance, PlantType theSeedType, ref bool __result)
        {
            if (theSeedType == theNewPlantType)
            {
                Board board = __instance.board;
                if (board == null)
                {
                    __result = false;
                    return;
                }
                if (!board.boardTag.enableAllTravelPlant && !board.boardTag.enableTravelPlant)
                {
                    __result = true;
                    Core.InGameText.Instance.ShowText("该配方仅旅行模式或深渊可用", 3f);
                    return;
                }
                if (Lawnf.TravelUnlock(UnlockBuffs[0]) || board.boardTag.enableAllTravelPlant)
                {
                    __result = false;
                }
                else
                {
                    __result = true;
                    Core.InGameText.Instance.ShowText("该配方需要抽取", 4f);
                }
            }
        }
    } // 强究判定

    [HarmonyPatch(typeof(Lawnf))]
    public static class LawnfPatch 
    {
        [HarmonyPatch(nameof(Lawnf.IsUltiPlant))]
        [HarmonyPostfix]
        public static void PostIsUltiPlant(ref PlantType thePlantType, ref bool __result)
        {
            if (thePlantType == theNewPlantType)
                __result = true;
        }
    } // 究极植物判定

    [HarmonyPatch(typeof(TravelMgr))]
    public static class TravelMgrPatch
    {
        [HarmonyPatch(nameof(TravelMgr.GetText))]
        [HarmonyPostfix]
        public static void PostGetText(Il2CppSystem.Object buff, ref string __result)
        {
            // 词条文本
            if (buff != null)
            {
                string buffStr = buff.ToString();
                if (BuffDesc.TryGetValue(buffStr, out var desc))
                {
                    __result = desc;
                }
            }
        }
    } // 词条文本

    [HarmonyPatch(typeof(TravelBuffOptionButton))]
    public static class TravelBuffOptionButtonPatch
    {
        [HarmonyPatch(nameof(TravelBuffOptionButton.SetPlant), new Type[] { })]
        [HarmonyPrefix]
        public static bool PreSetPlant(TravelBuffOptionButton __instance)
        {
            if (BuffDesc.ContainsKey(__instance.buff.ToString())) // 如果是二创词条
            {
                __instance.SetPlant(theNewPlantType);
                return false;
            }
            return true;
        }
    } // 词条界面植物

    [HarmonyPatch(typeof(Zombie))]
    public static class ZombiePatch
    {
        [HarmonyPatch(nameof(Zombie.Die))]
        [HarmonyPrefix]
        public static void PreDie(Zombie __instance, ref int reason)
        {
            if (__instance.theStatus == ZombieStatus.Dying && __instance.dieReason == reason)
                return;
            if (!Lawnf.TravelUltimate(UltiBuffs[1])) return;
            if (__instance.HasBuff(EffectType.Freeze) && __instance.HasBuff(EffectType.Portal)) // 如果有冻结和超时空
            {
                __instance.board.boardAction.CreateFreeze(__instance.axis.position, 5f);
            }
        }
    }

    [HarmonyPatch(typeof(AlmanacPlantMenu))]
    public static class AlmanacPlantAwakeMenuPatch
    {
        [HarmonyPatch(nameof(AlmanacPlantMenu.Awake))]
        [HarmonyPostfix]
        public static void Postfix(AlmanacPlantMenu __instance)
        {
            __instance.transform.Find("Scroll View/Viewport/Content").transform?.Find("LookUlti_2").GetComponent<UIButton>().clickEvent.
                AddListener(new Action(() =>
            {
                __instance.ShowPlants(TravelHelper.GetAllUltimatePlantTypes(true));
            }));
        }
    }
}
