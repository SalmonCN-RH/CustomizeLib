using BepInEx.Unity.IL2CPP;
using BepInEx;
using UnityEngine;
using CustomizeLib.BepInEx;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Unity.VisualScripting;

namespace CrazySqualourBepInEx
{
    [BepInPlugin("salmon.crazysqualour", "CrazySqualour", "1.0")]
    public class Core : BasePlugin
    {
        public static int levelID = -1;
        public static List<ZombieType> zombieList = new List<ZombieType>()
        {
            ZombieType.NormalZombie,
            ZombieType.ConeZombie,
            ZombieType.BucketZombie,
            ZombieType.SquashZombie,
            ZombieType.ElitePaperZombie,
            ZombieType.TallNutFootballZombie,
            ZombieType.SuperJackboxZombie,
            ZombieType.DriverZombie,
            ZombieType.SuperDriver,
            ZombieType.Gargantuar,
            ZombieType.RedGargantuar
        };
        public static CustomLevelData data = default(CustomLevelData);
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<EndoFlameSaver>();
            AssetBundle ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "icon");
            CustomLevelData customLevelData = new CustomLevelData();
            customLevelData.Name = (() => "严肃窝瓜3");
            Board.BoardTag boardTag = default(Board.BoardTag);
            boardTag.isConvey = true;
            customLevelData.BoardTag = boardTag;
            customLevelData.SceneType = SceneType.Day_6;
            customLevelData.RowCount = 6;
            customLevelData.WaveCount = () => 3;
            customLevelData.BgmType = MusicType.Day_drum;
            customLevelData.Logo = Extensions.GetAsset<Sprite>(ab, "icon");
            customLevelData.ZombieList = () => new List<ZombieType>()
            {
                ZombieType.ConeZombie,
                ZombieType.NormalZombie,
                ZombieType.BucketZombie,
                ZombieType.SquashZombie,
                ZombieType.ElitePaperZombie,
                ZombieType.TallNutFootballZombie,
                ZombieType.SuperJackboxZombie,
                ZombieType.DriverZombie,
                ZombieType.SuperDriver,
                ZombieType.Gargantuar,
                ZombieType.RedGargantuar
            };
            customLevelData.ConveyBeltPlantTypes = (() => new List<PlantType> {
                PlantType.Squalour,
                PlantType.EndoFlame,
                PlantType.JalaSquash,
                PlantType.Squash,
                PlantType.FireSquash,
                PlantType.IceSquash,
                PlantType.HypnoSquash,
                PlantType.SquashPumpkin,
                PlantType.IceShroom,
                PlantType.Jalapeno
            });
            customLevelData.NeedSelectCard = false;
            int oringinalDIff = 0;
            customLevelData.PreInitBoard = () =>
            {
                oringinalDIff = GameAPP.difficulty;
            };
            customLevelData.PostInitBoard = (board) =>
            {
                GameAPP.difficulty = oringinalDIff;
                ShowTextPatch.showed = false;
            };
            CustomLevelData customLevelData1 = customLevelData;
            levelID = CustomCore.RegisterCustomLevel(customLevelData1);
            data = customLevelData1;
        }
    }

    /*[HarmonyPatch(typeof(GameAPP), nameof(GameAPP.Start))]
    public class GameAPPPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GameAPP __instance)
        {
            GameAPP.ultimatePlantStartID = 0;
        }
    }*/

    [HarmonyPatch(typeof(InGameUI), nameof(InGameUI.Start))]
    public class InGameUIPatch
    {
        [HarmonyPostfix]
        public static void Postfix(InGameUI __instance)
        {
            if (GameAPP.theBoardLevel == Core.levelID && (int)GameAPP.theBoardType == 66)
            {
                if (__instance != null && (int)GameAPP.theBoardType == 66)
                {
                    __instance.ShovelBank.SetActive(false);
                    __instance.SeedBank.SetActive(false);
                }
            }
        }
    }

    /*[HarmonyPatch(typeof(InitZombieList), nameof(InitZombieList.InitList))]
    public class CreateZombiePatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (GameAPP.theBoardLevel == Core.levelID && (int)GameAPP.theBoardType == 66)
            {
                for (int i = 0; i < Core.zombieList.Count; i++)
                {
                    InitZombieList.AddZombieToList(Core.zombieList[i], 0);
                    int type = (int)Core.zombieList[i];
                    InitZombieList.allow[type] = true;
                    InitZombieList.zombieTypeList.Add(Core.zombieList[i]);
                }
            }
            /*Il2CppSystem.Array array = InitZombieList.zombieList.Cast<Il2CppSystem.Array>();
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    ZombieType type = array.GetValue(i, j).ConvertTo<ZombieType>();
                    if (type != ZombieType.Nothing)
                        MelonLogger.Msg(type);
                }
            }
        }
    }*/

    [HarmonyPatch(typeof(Squash))]
    public class SqualourUpdatePatch
    {
        [HarmonyPatch(nameof(Squash.SquashUpdate))]
        [HarmonyPrefix]
        public static bool Prefix(Squash __instance)
        {
            if ((__instance.thePlantType == PlantType.Squalour) || ((int)__instance.thePlantType == 1913) || ((int)__instance.thePlantType == 1914))
            {
                List<Plant> nearPlant = Lawnf.Get1x1Plants(__instance.thePlantColumn + 1, __instance.thePlantRow).ToArray().ToList();

                foreach (Plant plant in nearPlant)
                {
                    if (plant != null && plant.thePlantType == PlantType.EndoFlame)
                    {
                        Plant endoFlame = plant;
                        bool success = __instance.TryGetComponent<EndoFlameSaver>(out var component);
                        if (success)
                        {
                            component.endoFlame = plant;

                            // 设置攻击状态
                            __instance.isJump = true;

                            // 确定攻击方向（左/右）
                            __instance.anim.SetTrigger("lookleft"); // 向右看

                            // 设置父级对象（跟随棋盘）
                            __instance.transform.SetParent(__instance.board.transform);

                            // 记录攻击开始时间
                            __instance.startTime = Time.time;

                            // 保存目标位置
                            __instance.endPos = plant.axis.transform.position;
                            __instance.targetZombie = null;

                            // 执行攻击动作

                            // 鼠标选择特殊处理
                            __instance.invincible = true;

                            // 禁用碰撞器
                            foreach (var boxCol in __instance.boxCols)
                            {
                                boxCol.enabled = false;
                            }

                            // 处理鼠标交互
                            if (__instance.mouseCollider != null && __instance.mouseCollider == __instance)
                            {
                                __instance.mouseCollider = null;
                                __instance.isLily = false;
                                UnityEngine.Object.Destroy(__instance.mouseCollider.gameObject);
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Squash.AnimMove))]
        [HarmonyPrefix]
        public static bool Prefix_(Squash __instance)
        {
            if ((__instance.thePlantType == PlantType.Squalour) || ((int)__instance.thePlantType == 1913) || ((int)__instance.thePlantType == 1914))
            {
                bool findEndoFlame = false;
                Plant endoFlame = null;
                foreach (Plant plant in Lawnf.Get1x1Plants(__instance.thePlantColumn + 1, __instance.thePlantRow).ToArray().ToList())
                    if (plant.thePlantType == PlantType.EndoFlame)
                    {
                        findEndoFlame = true;
                        endoFlame = plant;
                        bool success = __instance.TryGetComponent<EndoFlameSaver>(out var component);
                        if (success)
                            component.endoFlame = plant;
                        break;
                    }

                if (findEndoFlame)
                {
                    SpriteRenderer renderer = __instance.axis.GetComponent<SpriteRenderer>();
                    UnityEngine.Object.Destroy(renderer);

                    __instance.freeMoving = false;
                    __instance.RemoveFromList();  // 从植物列表中移除自身


                    Vector3 axisPos = __instance.axis.transform.position;
                    Vector2 targetPoint = new Vector2(endoFlame.axis.transform.position.x, endoFlame.axis.transform.position.y + 1.75f);
                    __instance.startPos = __instance.axis.transform.position;
                    __instance.endPos = targetPoint;

                    // 启动跳跃协程
                    __instance.StartCoroutine(__instance.MoveToZombie(targetPoint, 5f));
                }
                return !findEndoFlame;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Board), nameof(Board.NewZombieUpdate))]
    public class ShowTextPatch
    {
        public static bool showed = false;
        [HarmonyPostfix]
        public static void Postfix(Board __instance)
        {
            if (GameAPP.theBoardLevel == Core.levelID && (int)GameAPP.theBoardType == 66 && __instance.theWave == 0 && !showed)
            {
                InGameText.Instance.ShowText("让猫瓜砸扁火红莲可以获取奖励！", 5f);
                InGameUI.Instance.BackToMenu.SetActive(false);
                showed = true;
            }
        }
    }

    [HarmonyPatch(typeof(Squash), nameof(Squash.AttackZombie))]
    public class SqualourDiePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Squash __instance)
        {
            if (((__instance.thePlantType == PlantType.Squalour) || ((int)__instance.thePlantType == 1913) || ((int)__instance.thePlantType == 1914)) && __instance.TryGetComponent<EndoFlameSaver>(out var component) && component.endoFlame != null)
            {
                for (int i = 0; i < component.endoFlame.attributeCount; i++)
                    if (component.endoFlame.TryGetComponent<EndoFlame>(out var endoFlame))
                        SpawnSun(endoFlame.axis.transform.position);
                component.endoFlame.Crashed();
                if (GameAPP.theBoardLevel == Core.levelID && (int)GameAPP.theBoardType == 66)
                    GetRandomEvent(__instance);
                if ((int)__instance.thePlantType == 1913)
                    GameAPP.board.GetComponent<Board>().CreateFireLine(Mouse.Instance.GetRowFromY(__instance.axis.transform.position.x, __instance.axis.transform.position.y) + 1, 1800, false, false, true);
                if ((int)__instance.thePlantType == 1914)
                    GameAPP.board.GetComponent<Board>().CreateFreeze(Vector2.zero);
                __instance.GetComponent<Squalour>().LourDie();
            }
        }

        public static void GetRandomEvent(Squash plant)
        {
            int currentCol = Mouse.Instance.GetColumnFromX(plant.axis.transform.position.x);
            int currentRow = (Mouse.Instance.GetRowFromY(plant.axis.transform.position.x, plant.axis.transform.position.y)) + 1;
            // MelonLogger.Msg(currentCol + " " + currentRow);
            int rand = UnityEngine.Random.Range(0, 100);
            if (rand < 5)
            {
                CreatePlant.Instance.SetPlant(currentCol + 1, currentRow - 1, PlantType.JalaSquash, null, default(Vector2), false, true, null);
                CreatePlant.Instance.SetPlant(currentCol + 1, currentRow, PlantType.JalaSquash, null, default(Vector2), false, true, null);
                CreatePlant.Instance.SetPlant(currentCol + 1, currentRow + 1, PlantType.JalaSquash, null, default(Vector2), false, true, null);
                CreatePlant.Instance.SetPlant(currentCol, currentRow, PlantType.JalaSquash, null, default(Vector2), false, true, null);
                CreatePlant.Instance.SetPlant(currentCol, currentRow + 1, PlantType.JalaSquash, null, default(Vector2), false, true, null);
                CreatePlant.Instance.SetPlant(currentCol, currentRow - 1, PlantType.JalaSquash, null, default(Vector2), false, true, null);
                CreatePlant.Instance.SetPlant(currentCol - 1, currentRow - 1, PlantType.JalaSquash, null, default(Vector2), false, true, null);
                CreatePlant.Instance.SetPlant(currentCol - 1, currentRow, PlantType.JalaSquash, null, default(Vector2), false, true, null);
                CreatePlant.Instance.SetPlant(currentCol - 1, currentRow + 1, PlantType.JalaSquash, null, default(Vector2), false, true, null);
            }
            else if (rand < 25)
            {
                int random = UnityEngine.Random.Range(1, 3);
                switch (random)
                {
                    case 0:
                        if (plant.thePlantType == PlantType.Squalour)
                            Lawnf.SetDroppedCard(plant.axis.transform.position, PlantType.Squalour, 0);
                        else if ((int)plant.thePlantType == 1913)
                            Lawnf.SetDroppedCard(plant.axis.transform.position, (PlantType)1913, 0);
                        else if ((int)plant.thePlantType == 1914)
                            Lawnf.SetDroppedCard(plant.axis.transform.position, (PlantType)1914, 0);
                        break;
                    case 1:
                        for (int i = 0; i < 2; i++)
                        {
                            if (plant.thePlantType == PlantType.Squalour)
                                Lawnf.SetDroppedCard(plant.axis.transform.position, PlantType.Squalour, 0);
                            else if ((int)plant.thePlantType == 1913)
                                Lawnf.SetDroppedCard(plant.axis.transform.position, (PlantType)1913, 0);
                            else if ((int)plant.thePlantType == 1914)
                                Lawnf.SetDroppedCard(plant.axis.transform.position, (PlantType)1914, 0);
                        }
                        break;
                    case 2:
                        CreatePlant.Instance.SetPlant(currentCol, currentRow, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol + 1, currentRow - 1, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol + 1, currentRow, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol + 1, currentRow + 1, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol, currentRow + 1, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol, currentRow - 1, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol, currentRow - 1, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol - 1, currentRow - 1, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol - 1, currentRow, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        CreatePlant.Instance.SetPlant(currentCol - 1, currentRow + 1, PlantType.FireSquash, null, default(Vector2), false, true, null);
                        break;
                }
            }
            else if (rand < 100)
            {
                Lawnf.SetDroppedCard(plant.axis.transform.position, PlantType.SquashPumpkin, 0);
            }
            InGameText.Instance.ShowText("猫瓜的馈赠！", 1.5f);
        }

        public static void SpawnSun(Vector3 position)
        {
            Vector3 coinPos = position;
            CreateItem.Instance.SetCoin(
                0, 0, 0, 0,
                coinPos,
                false);
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Start))]
    public class PlantStartPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Plant __instance)
        {
            if ((__instance.thePlantType == PlantType.Squalour) || ((int)__instance.thePlantType == 1913) || ((int)__instance.thePlantType == 1914))
                __instance.AddComponent<EndoFlameSaver>().endoFlame = null;
        }
    }

    public class EndoFlameSaver : MonoBehaviour
    {
        public Plant endoFlame = null;
    }
}