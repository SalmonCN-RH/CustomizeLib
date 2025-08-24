using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(UltimateFireThreeGatling.MelonLoader.Core), "UltimateFireThreeGatling", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace UltimateFireThreeGatling.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatefiresupergatling");
            CustomCore.RegisterCustomPlant<SuperThreeGatling, UltimateFireThreeGatling>(UltimateFireThreeGatling.PlantID,
                ab.GetAsset<GameObject>("UltimateFireThreeGatlingPrefab"),
                ab.GetAsset<GameObject>("UltimateFireThreeGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.SuperThreePeater),
                    (1901, (int)PlantType.AshThreePeater),
                    (AshThreeGatling.PlantID, (int)PlantType.Jalapeno)
                }, 1.5f, 0f, 160, 300, 0f, 1250);
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)UltimateFireThreeGatling.PlantID);
            CustomCore.AddPlantAlmanacStrings(UltimateFireThreeGatling.PlantID, $"浴火三线超级机枪射手({UltimateFireThreeGatling.PlantID})",
                "向三行发射浴火豌豆的超级机枪射手。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>融合配方：</color><color=red>超级机枪射手（底座）+浴火三线射手</color>\n<color=#3D1400>伤害：</color><color=red>(160x6)x3/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>拥有浴火三线射手的全部特性。每次攻击有2%概率触发大招，每0.02秒向三行各发射1个伤害三倍伤害的浴火豌豆</color>\n<color=#3D1400>词条1：</color><color=red>怒火攻心：火焰豌豆会施加红温状态，红温增伤增加至150%</color>\n<color=#3D1400>词条2：</color><color=red>百步穿杨：欲火豌豆能无限穿透，且附带红温爆炸伤害</color>\n\n<color=#3D1400>“我们来自未来，也来自过去，并存在于现在”浴火三线超级机枪射手的三个脑袋分别代表过去现在和未来，过去放眼未来，现在不被过去约束，未来不再重蹈覆辙，浴火三线超级机枪射手穿越到过去和未来，“我们找遍了所有的未来，只有一个未来是和平的，温暖的，就是我们正在进行，将要到达的未来”，他们异口同声，“就算是没有我们的未来，也要保持一颗热忱的心，就像火焰一样，温暖”</color>");

            var ab_ash = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ashthreegaling");
            CustomCore.RegisterCustomPlant<Plant, AshThreeGatling>(AshThreeGatling.PlantID,
                ab_ash.GetAsset<GameObject>("AshThreeGatlingPrefab"),
                ab_ash.GetAsset<GameObject>("AshThreeGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.AshThreePeater),
                    (1901, (int)PlantType.DarkThreePeater),
                    (1921, (int)PlantType.Jalapeno)
                }, 0f, 0f, 0, 300, 0f, 1125);
            CustomCore.AddPlantAlmanacStrings(AshThreeGatling.PlantID, $"灰烬三线超级机枪射手({AshThreeGatling.PlantID})",
                "被彻底烧焦的三线超级机枪射手，无法攻击。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>融合配方：</color><color=red>三线超级机枪射手（底座）+火爆辣椒+火爆辣椒</color>\n<color=#3D1400>特点：</color><color=red>失去攻击能力</color>\n\n<color=#3D1400>“我们曾无限接近死亡，生命无法一直保持濒死状态，他们只有生或死两个状态，”灰烬三线超级机枪射手长舒一口气，对着面前的植物说道。“与其说我们是灰烬，不如说灰烬创造了我们，我们是灰烬的孩子，就像大家喜欢阳光一样，我们喜欢如同灰烬一般的环境。”灰烬三线超级机枪射手知道他们体内又多大的能量，他们必须要学会控制并抑制这股力量，“我们不想让世界变成我们梦中那样，就像你们看到的那样，我们不具备任何攻击能力。”他们一直保持濒死的状态，自己身体剩余的能量只能拿来修复一直损坏的细胞，“我们喜欢这个世界。”他们在说完这句话之后，就沉沉的睡去了</color>");
        }
    }

    [RegisterTypeInIl2Cpp]
    public class UltimateFireThreeGatling : MonoBehaviour
    {
        public static int PlantID = 1923;

        public UltimateFireThreeGatling() : base(ClassInjector.DerivedConstructorPointer<UltimateFireThreeGatling>()) => ClassInjector.DerivedConstructorBody(this);

        public UltimateFireThreeGatling(IntPtr i) : base(i)
        {
        }

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("headPos2/ThreePeater_head2/ThreePeater_mouth/Shoot");
            if (Board.Instance is not null && GameAPP.theGameStatus == GameStatus.InGame)
                for (int i = 0; i < Board.Instance.rowNum; i++)
                    Board.Instance.CreateFireLine(i);
        }

        public SuperThreeGatling plant => gameObject.GetComponent<SuperThreeGatling>();
    }

    [RegisterTypeInIl2Cpp]
    public class AshThreeGatling : MonoBehaviour
    {
        public static int PlantID = 1924;

        public AshThreeGatling() : base(ClassInjector.DerivedConstructorPointer<AshThreeGatling>()) => ClassInjector.DerivedConstructorBody(this);

        public AshThreeGatling(IntPtr i) : base(i)
        {
        }

        public Plant plant => gameObject.GetComponent<Plant>();
    }

    [HarmonyPatch(typeof(ThreePeater), nameof(ThreePeater.GetBulletType))]
    public class ThreePeater_GetBulletType
    {
        [HarmonyPrefix]
        public static bool Prefix(ThreePeater __instance, ref BulletType __result)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
            {
                __result = BulletType.Bullet_firePea_super;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SuperThreeGatling))]
    public class SuperThreeGatling_SuperShoot
    {
        [HarmonyPatch(nameof(SuperThreeGatling.SuperShoot))]
        [HarmonyPrefix]
        public static bool Prefix(SuperThreeGatling __instance, ref float angle, ref float speed, ref float x, ref float y, ref BulletMoveWay bulletMoveWay, ref int row)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
            {
                CreateBullet creator = CreateBullet.Instance;

                Bullet bullet = CreateBullet.Instance.SetBullet(x, y, row, __instance.GetBulletType(), bulletMoveWay, false);
                // 配置子弹属性
                if (bullet != null)
                {
                    // 设置子弹旋转角度
                    bullet.transform.Rotate(0, 0, angle);

                    // 设置子弹移动速度
                    bullet.normalSpeed = speed;

                    // 设置三倍攻击伤害
                    bullet.Damage = 3 * __instance.attackDamage;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(SuperThreeGatling.Update))]
        [HarmonyPrefix]
        public static void Prefix_Update(SuperThreeGatling __instance, out bool __state)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
            {
                if (__instance.timer > 0 && __instance.timer - Time.deltaTime <= 0f)
                {
                    __state = true;
                    return;
                }
            }
            __state = false;
        }

        [HarmonyPatch(nameof(SuperThreeGatling.Update))]
        [HarmonyPostfix]
        public static void Postfix_Update(SuperThreeGatling __instance, bool __state)
        {
            if (__state)
                __instance.anim.SetTrigger("shoot");
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.OnDestroy))]
    public class Plant_OnDestroy
    {
        [HarmonyPostfix]
        public static void Prefix(Plant __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
                if (Board.Instance is not null && GameAPP.theGameStatus == GameStatus.InGame)
                    for (int i = 0; i < Board.Instance.rowNum; i++)
                        Board.Instance.CreateFireLine(i);
        }
    }
}