using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using Unity.VisualScripting;
using CustomizeLib.BepInEx;

namespace GoldenSalmon.BepInEx
{
    [BepInPlugin("salmon.goldensalmon", "GoldenSalmon", "1.0")]
    public class Core : BasePlugin//960
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<GoldenSalmon>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "goldensalmon");
            CustomCore.RegisterCustomPlant<PeaShooter, GoldenSalmon>(GoldenSalmon.PlantID, ab.GetAsset<GameObject>("GoldenSalmonPrefab"),
                ab.GetAsset<GameObject>("GoldenSalmonPreview"), [], 0f, 0f, 2147483647, 2147483647, 120, 950);

            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)GoldenSalmon.PlantID);
            CustomCore.TypeMgrExtra.IsTallNut.Add((PlantType)GoldenSalmon.PlantID);
            CustomCore.AddPlantAlmanacStrings(GoldenSalmon.PlantID, $"金鱼({GoldenSalmon.PlantID})", "纯纯的机制怪。\n\n<color=#3D1400>韧性：</color><color=red>2147483647</color>\n<color=#3D1400>特点：</color><color=red>出场3秒秒杀一次全体僵尸，随后消失。</color>\n\n<color=#3D1400>代这写.jpg</color>\n\n\n\n\n花费：<color=red>950</color>\n冷却时间：<color=red>120秒</color>");
            CustomCore.RegisterCustomCardToColorfulCards((PlantType)GoldenSalmon.PlantID);
        }
    }

    public class GoldenSalmon : MonoBehaviour
    {
        public static int PlantID = 1909;
        public GoldenSalmon() : base(ClassInjector.DerivedConstructorPointer<GoldenSalmon>()) => ClassInjector.DerivedConstructorBody(this);

        public GoldenSalmon(IntPtr i) : base(i)
        {
        }

        public void Update()
        {
            if (plant != null && GameAPP.board != null && GameAPP.theGameStatus == GameStatus.InGame && plant.attributeCountdown <= 0)
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
                            }
                        }
                    }
                    CreateParticle.SetParticle((int)ParticleType.RandomCloud, plant.axis.transform.position, plant.thePlantRow);
                    GameAPP.PlaySound(UnityEngine.Random.Range(0, 3));
                    plant.Die();
                }
            }
        }

        public void Start()
        {
            plant.attributeCountdown = 3f;
        }

        public PeaShooter plant => gameObject.GetComponent<PeaShooter>();
    }
}