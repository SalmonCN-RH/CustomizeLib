using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using System.Reflection;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;

namespace SuperDiamondNut.BepInEx
{
    [HarmonyPatch(typeof(SuperSunNut), "TakeDamage")]
    public static class SuperSunNutPatch
    {
        [HarmonyPrefix]
        public static unsafe bool PreTakeDamage(SuperSunNut __instance)
        {
            if (__instance.thePlantType is (PlantType)161)
            {
                var damage = Lawnf.TravelAdvanced(5) ? 10 : 50;
                CreateItem.Instance.SetCoin(__instance.thePlantColumn, __instance.thePlantRow, 34, 0);
                IL2CPP.Il2CppObjectBaseToPtrNotNull(__instance);
                IntPtr* ptr = stackalloc IntPtr[2];
                *ptr = (nint)(&damage);
                int i = 0;
                *(int**)((byte*)ptr + checked(1u * unchecked((nuint)sizeof(IntPtr)))) = &i;
                System.Runtime.CompilerServices.Unsafe.SkipInit(out IntPtr exc);
                IL2CPP.il2cpp_runtime_invoke((IntPtr)typeof(Plant).GetField("NativeMethodInfoPtr_TakeDamage_Public_Virtual_New_Void_Int32_Int32_0", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!, IL2CPP.Il2CppObjectBaseToPtrNotNull(__instance), (void**)ptr, ref exc);
                Il2CppException.RaiseExceptionIfNecessary(exc);
                __instance.ReplaceSprite();
                return false;
            }
            else
            {
                return true;
            }
        }

        public static MethodInfo TakeDamage => typeof(Plant).GetMethod("TakeDamage")!;
    }

    [BepInPlugin("inf75.superdiamondnut", "SuperDiamondNut", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<SuperDiamondNut>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "superdiamondnut");
            CustomCore.RegisterCustomPlant<SuperSunNut, SuperDiamondNut>(161, ab.GetAsset<GameObject>("SuperDiamondNutPrefab"),
                ab.GetAsset<GameObject>("SuperDiamondNutPreview"), [(905, 31)], 3, 0, 20, 4000, 7.5f, 800);
            CustomCore.RegisterCustomPlantClickEvent(161, SuperDiamondNut.SummonAndRecover);
            CustomCore.AddFusion(905, 161, 1);
            CustomCore.RegisterCustomPlant<BigWallNut>(162, ab.GetAsset<GameObject>("BigDiamondNutPrefab"),
                ab.GetAsset<GameObject>("BigDiamondNutPreview"), [], 3, 0, 1800, 4000, 7.5f, 150);
            CustomCore.TypeMgrExtra.IsNut.Add((PlantType)161);
            CustomCore.TypeMgrExtra.BigNut.Add((PlantType)162);
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)162);
            CustomCore.AddPlantAlmanacStrings(161, "钻石帝果(161)", "点击生成钻石保龄球\n<color=#3D1400>贴图作者：@林秋AutumnLin</color>\n<color=#3D1400>特点：</color><color=red>阳光帝果亚种，使用金盏花、向日葵切换，花费3000钱币生成1800/帧伤的钻石保龄球</color>\n<color=#3D1400>融合配方：</color><color=red>阳光帝果+金盏花</color>\n<color=#3D1400>钻石帝果每次和阳光帝果一起出场时，僵尸们总是四散而逃，每当记者采访他时，他总说：“阳光帝果生产阳光时，我反射的光照就会闪瞎他们的眼睛。”这时记者都会一口同声说一句：“天怎么黑了？”</color>");
            CustomCore.AddPlantAlmanacStrings(162, "钻石保龄球(162)", "就是个换皮大保龄球...吗？\n<color=#3D1400>贴图作者：@林秋AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>1800/帧伤</color>\n<color=#3D1400>！</color>");
        }
    }

    public class SuperDiamondNut : MonoBehaviour
    {
        public SuperDiamondNut() : base(ClassInjector.DerivedConstructorPointer<SuperDiamondNut>()) => ClassInjector.DerivedConstructorBody(this);

        public SuperDiamondNut(IntPtr i) : base(i)
        {
        }

        public static void SummonAndRecover(Plant plant)
        {
            var ssn = plant.TryCast<SuperSunNut>();

            if (plant.board.theMoney >= 3000 && ssn is not null)
            {
                Money.Instance?.UsedEvent(plant.thePlantColumn, plant.thePlantRow, 3000);
                plant.Recover(Lawnf.TravelAdvanced(4) ? 4000 : 1500);

                if (ssn.targetPlant != null && ssn.targetPlant.theStatus is not PlantStatus.BigSunNut_round)
                {
                    // 如果是 BigSunNut 类型则强化它
                    if (ssn.targetPlant is BigSunNut bigSunNut)
                    {
                        bigSunNut.Larger();
                        return;
                    }
                }

                GameObject newPlantObj = CreatePlant.Instance.SetPlant(
                    ssn.thePlantColumn + 1,
                    ssn.thePlantRow,
                    (PlantType)162
                );

                if (newPlantObj != null)
                {
                    // 获取 BigSunNut 组件并设置为目标植物
                    ssn.targetPlant = newPlantObj.GetComponent<BigSunNut>();

                    // 在植物位置播放粒子效果
                    if (ssn.targetPlant != null && ssn.targetPlant.axis != null)
                    {
                        Vector3 position = ssn.targetPlant.axis.position;
                        ParticleManager.Instance.SetParticle(
                            ParticleType.RandomCloud,
                            new Vector2(position.x, position.y + 0.5f),
                            ssn.targetPlant.thePlantRow
                        );
                    }
                    /*
                    GameObject gameObject = CreatePlant.Instance.SetPlant(plant.thePlantColumn + 1, plant.thePlantRow, (PlantType)162, null, default, true);
                    if (gameObject is not null)
                    {
                        Vector3 position = gameObject.GetComponent<Plant>().axis.position;
                        Instantiate(GameAPP.particlePrefab[11], position + new Vector3(0f, 0.5f, 0f), Quaternion.identity, plant.board.transform);
                    }*/
                }
            }
        }

        public void Awake()
        {
            if (GameAPP.theGameStatus is (int)GameStatus.InGame && !Board.Instance.isIZ && !Board.Instance.isEveStart && gameObject.GetComponent<SuperSunNut>().thePlantType is (PlantType)161)
            {
                InGameUI.Instance.MoneyBank.SetActive(true);
            }
            plant.DisableDisMix();
        }

        public SuperSunNut plant => gameObject.GetComponent<SuperSunNut>();
    }
}