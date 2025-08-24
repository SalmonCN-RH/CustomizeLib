using CustomizeLib.MelonLoader;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(PuffUltimateGatling.Core), "PuffUltimateGatling", "1.0.0", "Administrator", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace PuffUltimateGatling
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "threepuffsupersnowgatling");
            CustomCore.RegisterCustomPlant<SuperThreeGatling, ThreePuffSuperSnowGatling>(
                ThreePuffSuperSnowGatling.PlantID,
                ab.GetAsset<GameObject>("ThreePuffSuperSnowGatlingPrefab"),
                ab.GetAsset<GameObject>("ThreePuffSuperSnowGatlingPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.SmallPuff, 1920),
                    (1910, (int)PlantType.ThreePeater),
                    (1927, (int)PlantType.IceShroom),
                    ((int)PlantType.IcePuff, (int)PlantType.SuperThreeGatling)
                },
                1.5f, 0f, 20, 300, 0f, 1000
            );
            CustomCore.AddPlantAlmanacStrings(ThreePuffSuperSnowGatling.PlantID,
                $"三线超级寒冰机枪小喷菇({ThreePuffSuperSnowGatling.PlantID})",
                "向三行发射小冰锥的小超级机枪射手。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>(20x3)x6/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒向三行各发射1个伤害为3倍的小冰锥。</color>\n<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+三线超级寒冰机枪射手</color>\n\n<color=#3D1400>咕咕嘎嘎</color>"
            );
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)ThreePuffSuperSnowGatling.PlantID);
            CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)ThreePuffSuperSnowGatling.PlantID);
        }
    }
}