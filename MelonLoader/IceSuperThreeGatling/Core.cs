using CustomizeLib.MelonLoader;
using MelonLoader;
using Il2Cpp;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.UIElements;

[assembly: MelonInfo(typeof(IceSuperThreeGatling.Core), "IceSuperThreeGatling", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace IceSuperThreeGatling
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "icesuperthreegatling");
            CustomCore.RegisterCustomPlant<SuperThreeGatling, IceSuperThreeGatling>(IceSuperThreeGatling.PlantID, ab.GetAsset<GameObject>("IceSuperThreeGatlingPrefab"),
                ab.GetAsset<GameObject>("IceSuperThreeGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperThreeGatling, (int)PlantType.IceShroom),
                    ((int)PlantType.SuperSnowGatling, (int)PlantType.ThreePeater)
                }, 1.5f, 0f, 20, 300, 0f, 950);
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)IceSuperThreeGatling.PlantID);
            CustomCore.AddPlantAlmanacStrings(IceSuperThreeGatling.PlantID, $"寒冰三线超级机枪射手({IceSuperThreeGatling.PlantID})",
                "向三行发射寒冰豌豆的超级机枪射手。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>(20x6)x3/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>每次攻击有2%概率触发大招，5秒内，每0.02秒向三行各发射1个伤害为60的寒冰豌豆。</color>\n<color=#3D1400>融合配方：</color><color=red>超级机枪射手（底座）+三线射手+寒冰菇</color>\n\n<color=#3D1400>“我们喜欢待在冰箱里，也喜欢去南极北极旅游，看英纽特缓慢变成僵尸的过程……”寒冰三线超级机枪射手在新品发布会侃侃而谈，“我们对于绝对零度的研究已经接近尾声了，不管是现在还是未来，我们都想尽可能的帮助大家，帮助科学”三兄弟把自己的一生献给了科学，只为了能够成为对抗僵王博士的中流砥柱，“其实我们没做什么，”三兄弟一直都很谦虚，尽管他们已经是僵王博士暗杀名单的榜首。</color>");
        }
    }

    [RegisterTypeInIl2Cpp]
    public class IceSuperThreeGatling : MonoBehaviour
    {
        public static int PlantID = 1920;

        public IceSuperThreeGatling() : base(ClassInjector.DerivedConstructorPointer<IceSuperThreeGatling>()) => ClassInjector.DerivedConstructorBody(this);

        public IceSuperThreeGatling(IntPtr i) : base(i)
        {
        }

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("headPos2/ThreePeater_head2/ThreePeater_mouth/Shoot");
        }

        public SuperThreeGatling plant => gameObject.GetComponent<SuperThreeGatling>();
    }

    [HarmonyPatch(typeof(ThreePeater), nameof(ThreePeater.GetBulletType))]
    public class ThreePeater_GetBulletType
    {
        [HarmonyPrefix]
        public static bool Prefix(ThreePeater __instance, ref BulletType __result)
        {
            if (__instance != null && (int)__instance.thePlantType == IceSuperThreeGatling.PlantID)
            {
                foreach (Plant plant in Lawnf.Get1x1Plants(__instance.thePlantColumn, __instance.thePlantRow))
                {
                    if (plant is not null && plant.thePlantType == PlantType.IceBean)
                    {
                        __result = BulletType.Bullet_extremeSnowPea;
                        return false;
                    }
                }
                __result = BulletType.Bullet_snowPea;
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
            if (__instance != null && (int)__instance.thePlantType == IceSuperThreeGatling.PlantID)
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
            if (__instance != null && (int)__instance.thePlantType == IceSuperThreeGatling.PlantID)
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
}