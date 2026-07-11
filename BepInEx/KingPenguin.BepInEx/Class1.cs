using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;
using static KingPenguin.BepInEx.Class1;

namespace KingPenguin.BepInEx
{
    [BepInPlugin("salmon.kingpenguin", "KingPenguin", "1.0.0")]
    public class Class1 : BasePlugin
    {
        public static ZombieType theNewZombieType = (ZombieType)20000;

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static void InitKingPenguin()
        {
            // 加载僵尸
            var ab = GetAssetBundle("kingpenguin");
            ClassInjector.RegisterTypeInIl2Cpp<KingPenguin>();
            foreach (var item in ab.LoadAllAssetsAsync().allAssets)
            {
                if (item.TryCast<GameObject>()?.name == "KingPenguinZombie")
                {
                    GameAPP.resourcesManager.zombiePrefabs[theNewZombieType] = item.TryCast<GameObject>();
                    GameAPP.resourcesManager.zombiePrefabs[theNewZombieType].AddComponent<KingPenguin>();
                    var zombie = GameAPP.resourcesManager.zombiePrefabs[theNewZombieType].AddComponent<PenguinZombie>();
                    GameAPP.resourcesManager.allZombieTypes.Add(theNewZombieType);
                    zombie.gameObject.layer = LayerMask.NameToLayer("Zombie");
                    zombie.theZombieType = theNewZombieType;
                    zombie.tag = "Zombie";
                    // 初始化僵尸属性
                    var zombieData = new ZombieDataManager.ZombieData
                    {
                        theMaxHealth = 1350,
                        theAttackDamage = 100,
                        summonWeight = 1500,
                        cost = 200,
                        summonLevel = 5
                    };
                    foreach (var child in Core.Lawnf.GetChilds(GameAPP.resourcesManager.zombiePrefabs[theNewZombieType].transform))
                    {
                        string tag = "";
                        switch (child.name)
                        {
                            case "head1": tag = "ZombieHead"; break;
                            case "Shadow": tag = "other"; break;
                            default: continue;
                        }
                        child.tag = tag;
                    }
                    ZombieDataManager.zombieDataDic[theNewZombieType] = zombieData;
                    InitZombieList.allowAllzombies.Add(theNewZombieType);
                }
                if (item.TryCast<GameObject>()?.name == "KingPenguinZombiePreview") // 注册预览图
                    GameAPP.resourcesManager.zombieSprites[theNewZombieType] = item.TryCast<GameObject>()?.GetComponent<SpriteRenderer>().sprite;
            }
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

    public class KingPenguin : MonoBehaviour
    {
        public void ResetDirection()
        {
            transform.rotation = Quaternion.identity;
        }

        public void ChangeDirection()
        {
            transform.rotation = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
        }

        public PenguinZombie zombie => gameObject.GetComponent<PenguinZombie>();
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Awake))]
        [HarmonyPostfix]
        public static void PostAwake()
        {
            InitKingPenguin();
        }
    }

    [HarmonyPatch(typeof(CreateZombie))]
    public static class CreateZombiePatch
    {
        [HarmonyPatch(nameof(CreateZombie.SetZombie))]
        [HarmonyPrefix]
        public static void PreSetZombie(ref ZombieType theZombieType)
        {
            if (UnityEngine.Random.Range(0, 2) == 0 && GameAPP.theGameStatus == GameStatus.InGame)
            {
                if (theZombieType == ZombieType.PenguinZombie)
                    theZombieType = theNewZombieType;
            }
        }
    }
}
