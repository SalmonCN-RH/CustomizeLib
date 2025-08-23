using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(PuffSuperSnowGatling.Core), "PuffSuperSnowGatling", "1.0.0", "Salmon")]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
namespace PuffSuperSnowGatling;
public class Core : MelonMod
{
    public override void OnInitializeMelon()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "puffsupersnowgatling");
        CustomCore.RegisterCustomPlant<SuperGatling, PuffSuperGatling>(
            PuffSuperGatling.PlantID,
            ab.GetAsset<GameObject>("PuffSuperSnowGatlingPrefab"),
            ab.GetAsset<GameObject>("PuffSuperSnowGatlingPreview"),
            new List<(int, int)>
            {
                ((int)PlantType.SmallPuff, (int)PlantType.SuperSnowGatling),
                (1907, (int)PlantType.IceShroom),
                ((int)PlantType.SmallIceShroom, (int)PlantType.SuperGatling)
            },
            1.5f, 0f, 20, 300, 0f, 675
        );
        CustomCore.AddPlantAlmanacStrings(PuffSuperGatling.PlantID,
            $"超级寒冰小喷菇射手({PuffSuperGatling.PlantID})",
            "一次发射六颗小冰锥，有概率一次性发射大量小冰锥。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>20*6/1.5秒;\n20×3/0.02秒（大招）</color>\n<color=#3D1400>特点：</color><color=red>攻击有3%概率散射大量小冰锥，持续5秒。</color>\n<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+超级寒冰机枪射手</color>\n\n<color=#3D1400>曾经参军的小喷菇们，现在被磨练得冷峻而坚毅——当然，没到“冷酷无情”的地步。如果你好奇询问他们在冰原的见闻，他们总会回答“最近在巡逻时总会看见有只戴帽子的怪窝瓜...”</color>"
        );
        CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)PuffSuperGatling.PlantID);
        CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)PuffSuperGatling.PlantID);
        // CustomCore.TypeMgrExtra.IsPuff.Contains(PlantType.SunGatlingPuff);\
    }
}

[RegisterTypeInIl2Cpp]
public class PuffSuperGatling : MonoBehaviour
{
    public static int PlantID = 1910;

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
            __result = BulletType.Bullet_smallIceSpark;
        }
    }
}