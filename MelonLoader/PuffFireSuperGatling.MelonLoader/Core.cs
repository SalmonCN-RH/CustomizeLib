using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(PuffFireSuperGatling.MelonLoader.Core), "PuffFireSuperGatling", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace PuffFireSuperGatling.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "pufffiresupergatling");
            CustomCore.RegisterCustomPlant<SuperGatling, PuffFireSuperGatling>(
                PuffFireSuperGatling.PlantID,
                ab.GetAsset<GameObject>("PuffFireSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("PuffFireSuperGatlingPreview"),
                new List<(int, int)>
                {
                    (1907, (int)PlantType.Jalapeno),
                    ((int)PlantType.SmallPuff, 1901)
                },
                1.5f, 0f, 40, 300, 0f, 825
            );
            CustomCore.AddPlantAlmanacStrings(PuffFireSuperGatling.PlantID,
                $"超级火焰小喷菇机枪射手({PuffFireSuperGatling.PlantID})",
                "一次发射六颗小火焰豌豆，有概率一次性发射大量小火焰豌豆\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>40*6/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒散射3发小火焰豌豆</color>\n<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+超级机枪射手+火爆辣椒</color>\n\n<color=#3D1400>咕咕咕</color>"
            );
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)PuffFireSuperGatling.PlantID);
            CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)PuffFireSuperGatling.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class PuffFireSuperGatling : MonoBehaviour
    {
        public static int PlantID = 1926;

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
        [HarmonyPostfix]
        public static void Postfix(SuperGatling __instance, ref BulletType __result)
        {
            if ((int)__instance.thePlantType == PuffFireSuperGatling.PlantID)
            {
                __result = BulletType.Bullet_firePea_small;
            }
        }
    }
}