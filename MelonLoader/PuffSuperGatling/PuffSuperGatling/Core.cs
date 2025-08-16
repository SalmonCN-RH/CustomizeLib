using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ElectricSuperGatling.Core), "PuffSuperGatling", "1.0.0", "Salmon")]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
namespace ElectricSuperGatling;
public class Core : MelonMod
{
    public override void OnInitializeMelon()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "puffsupergatling");
        CustomCore.RegisterCustomPlant<SuperGatling, PuffSuperGatling>(
            PuffSuperGatling.PlantID,
            ab.GetAsset<GameObject>("PuffSuperGatlingPrefab"),
            ab.GetAsset<GameObject>("PuffSuperGatlingPreview"),
            new List<(int, int)>
            {
                ((int)PlantType.SmallPuff, (int)PlantType.SuperGatling)
            },
            1.5f, 0f, 20, 300, 0f, 600
        );
        CustomCore.AddPlantAlmanacStrings(PuffSuperGatling.PlantID,
            $"超级小喷菇机枪射手({PuffSuperGatling.PlantID})",
            "一次发射六颗小豌豆，有概率一次性发射大量小豌豆\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>20*6/1.5秒;\n20×3/0.02秒（大招）</color>\n<color=#3D1400>特点：</color><color=red>攻击有3%概率散射大量小豌豆，持续5秒。</color>\n<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+超级机枪射手</color>\n\n<color=#3D1400>“聚是一团火，散是满天星”超级小喷菇机枪射手作为喷菇家族和豌豆家族的优秀后裔，小小的身材里藏了大大的能量。戴着低调的头盔，既可单兵作战，也可“团伙作战”，他们最喜欢的一首歌是“豌豆伞，绿杆杆，僵尸碰了躺板板”。</color>"
        );
        CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)PuffSuperGatling.PlantID);
        // CustomCore.TypeMgrExtra.IsPuff.Contains(PlantType.SunGatlingPuff);\

        CustomCore.AddFusion(PuffSuperGatling.PlantID, 1908, (int)PlantType.Peashooter);
    }
}

[RegisterTypeInIl2Cpp]
public class PuffSuperGatling : MonoBehaviour
{
    public static int PlantID = 1907;

    public SuperGatling plant => gameObject.GetComponent<SuperGatling>();

    public void Awake()
    {
        plant.shoot = plant.gameObject.transform.GetChild(0);
        plant.isShort = true;
    }
}

[HarmonyPatch(typeof(SuperGatling), nameof(SuperGatling.GetBulletType))]
public class SuperGatling_GetBulletType
{
    public static void Postfix(SuperGatling __instance, ref BulletType __result)
    {
        if ((int)__instance.thePlantType == PuffSuperGatling.PlantID)
        {
            if (__instance.timer <= 0)
                __result = BulletType.Bullet_puffPea;
            else
            {
                List<BulletType> bulletTypes = new()
                {
                    BulletType.Bullet_puffPea,
                    BulletType.Bullet_smallIceSpark,
                    BulletType.Bullet_firePea_small
                };
                __result = bulletTypes[UnityEngine.Random.Range(0, bulletTypes.Count)];
            }
        }
    }
}