using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace SalmonBepInEx
{
    [BepInPlugin("salmon.salmon", "Salmon", "1.0")]
    public class Core : BasePlugin//304
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<Salmon>();

            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "salmon");
            CustomCore.RegisterCustomPlant<PeaShooter, Salmon>(Salmon.PlantID, ab.GetAsset<GameObject>("SalmonPrefab"),
                ab.GetAsset<GameObject>("SalmonPreview"), [], 0f, 0f, 2147483647, 2147483647, 0, 127);

            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)Salmon.PlantID);
            CustomCore.TypeMgrExtra.IsTallNut.Add((PlantType)Salmon.PlantID);
            CustomCore.AddPlantAlmanacStrings(Salmon.PlantID, $"鲑鱼({Salmon.PlantID})", "疑似代码现出原形。\n\n<color=#3D1400>韧性：</color><color=red>2147483647</color>\n<color=#3D1400>特点：</color><color=red>出场时生成奖杯，在场时代码杀所有非魅惑僵尸，并让所有植物的血量和最大血量提升至2147483647。</color>\n\n<color=#3D1400>鱼类成精了！变成了一个代码？？？“空引用能不能死一死啊！！！”</color>");
        }

        public static void SpawnItem(String resourcePath)
        {
            GameObject gameObject = Resources.Load<GameObject>(resourcePath);
            if (gameObject != null)
            {
                UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector2(0f, 0f), Quaternion.identity, GameAPP.board.transform);
                return;
            }
        }
    }

    public class Salmon : MonoBehaviour
    {
        public static int PlantID = 1905;
        public Salmon() : base(ClassInjector.DerivedConstructorPointer<Salmon>()) => ClassInjector.DerivedConstructorBody(this);

        public Salmon(IntPtr i) : base(i)
        {
        }

        public void Update()
        {
            if (GameAPP.board != null && GameAPP.theGameStatus == GameStatus.InGame)
            {
                if (GameAPP.board.TryGetComponent<Board>(out Board board))
                {
                    if (board.zombieArray != null)
                    {
                        for (int i = 0; i < board.zombieArray.Count; i++)
                        {
                            Zombie zombie = board.zombieArray[i];
                            if (zombie != null && !zombie.isMindControlled)
                            {
                                zombie.Die();
                                if (zombie != null)
                                {
                                    UnityEngine.Object.Destroy(zombie.gameObject);
                                    GameAPP.board.GetComponent<Board>().theTotalNumOfZombie--;
                                }
                            }
                        }
                    }

                    if (board.plantArray != null)
                    {
                        for (int i = 0; i < board.plantArray.Count; i++)
                        {
                            Plant plant = board.plantArray[i];
                            if (plant != null)
                            {
                                plant.thePlantHealth = 2147483647;
                                plant.thePlantMaxHealth = 2147483647;
                            }
                        }
                    }
                }
            }
        }

        public void Start()
        {
            try
            {
                plant.shoot = plant.gameObject.transform.GetChild(0);
                if (GameAPP.theGameStatus == GameStatus.InGame)
                    Core.SpawnItem("Board/Award/TrophyPrefab");
            }
            catch (Exception) { }
        }

        public PeaShooter plant => gameObject.GetComponent<PeaShooter>();
    }
}