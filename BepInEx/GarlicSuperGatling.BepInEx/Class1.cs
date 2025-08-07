using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using Unity.VisualScripting;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace GarlicSuperGatling.BepInEx
{
    [BepInPlugin("salmon.garlicsupergatling", "IceDoomCherryJalapeno", "1.0")]
    public class Core : BasePlugin//177
    {
        public static int ParticleID = 500;
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_pea_garlic>();
            ClassInjector.RegisterTypeInIl2Cpp<GarlicSuperGatling>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "garlicsupergatling");
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_pea_garlic>((BulletType)Bullet_pea_garlic.BulletID, ab.GetAsset<GameObject>("Bullet_pea_garlic"));
            CustomCore.RegisterCustomParticle((ParticleType)ParticleID, ab.GetAsset<GameObject>("GarlicPeaSplat"));
            CustomCore.RegisterCustomPlant<SuperGatling, GarlicSuperGatling>(GarlicSuperGatling.PlantID, ab.GetAsset<GameObject>("GarlicSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("GarlicSuperGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.Garlic),
                    ((int)PlantType.Garlic, (int)PlantType.SuperGatling)
                }, 1.5f, 0f, 40, 300, 7.5f, 650);
            CustomCore.AddPlantAlmanacStrings(GarlicSuperGatling.PlantID, $"超级蒜机枪射手({GarlicSuperGatling.PlantID})",
                "一次发射六颗蒜豌豆，有概率一次性发射大量蒜豌豆。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>40x6/1.5秒</color>\n<color=#3D1400>特点：</color><color=red>攻击有3%概率散射大量蒜豌豆，持续5秒。直击和爆炸获得蒜值增伤后赋予1点蒜值，命中陷入中毒的僵尸有10%概率造成（子弹伤害+僵尸蒜值x40）伤害的大蒜爆炸，并对周围的僵尸陷入中毒效果。</color>\n<color=#3D1400>词条1：</color><color=red>蒜毒骤发：赋予蒜值会施加中毒。每层蒜值增伤提升至80，大蒜爆炸概率提升至25%，蒜爆蒜值增伤提升至400。</color>\n\n<color=#3D1400>永远不要招惹一位退役的老兵，尤其是特立独行的家伙。虽然不知道是怎么传播开来的，但是招惹它的僵尸基本都后悔了。</color>");
        }
    }

    public class Bullet_pea_garlic : MonoBehaviour
    {
        public static int BulletID = 1906;

        public Bullet_pea_garlic() : base(ClassInjector.DerivedConstructorPointer<Bullet_pea_garlic>()) => ClassInjector.DerivedConstructorBody(this);

        public Bullet_pea_garlic(IntPtr i) : base(i)
        {
        }

        public Bullet_pea bullet => gameObject.GetComponent<Bullet_pea>();
    }

    public class GarlicSuperGatling : MonoBehaviour
    {
        public static int PlantID = 1918;

        public GarlicSuperGatling() : base(ClassInjector.DerivedConstructorPointer<GarlicSuperGatling>()) => ClassInjector.DerivedConstructorBody(this);

        public GarlicSuperGatling(IntPtr i) : base(i)
        {
        }

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.GetChild(0).GetChild(0);
        }

        public SuperGatling plant => gameObject.GetComponent<SuperGatling>();
    }

    [HarmonyPatch(typeof(SuperGatling), nameof(SuperGatling.GetBulletType))]
    public class SuperGatling_GetBulletType
    {
        [HarmonyPrefix]
        public static bool Prefix(SuperGatling __instance, ref BulletType __result)
        {
            if ((int)__instance.thePlantType == GarlicSuperGatling.PlantID)
            {
                __result = (BulletType)Bullet_pea_garlic.BulletID;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_pea), nameof(Bullet_pea.HitZombie))]
    public class Bullet_pea_HitZombie
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Bullet_doom __instance, ref Zombie zombie)
        {
            if ((int)__instance.theBulletType == Bullet_pea_garlic.BulletID)
            {
                int damage = __instance.Damage + 20 * zombie.poisonLevel;
                if (Lawnf.TravelUltimate(13))
                {
                    zombie.SetPoison();
                    damage = __instance.Damage + 80 * zombie.poisonLevel;
                }
                zombie.TakeDamage(DmgType.NormalAll, __instance.Damage + 20 * zombie.poisonLevel);
                if (!Lawnf.TravelUltimate(13))
                    zombie.AddPoisonLevel();
                int r = UnityEngine.Random.Range(0, 10);
                if (Lawnf.TravelUltimate(13))
                    r = UnityEngine.Random.Range(0, 4);
                if (zombie.poisonTimer > 0 && r == 2)
                {
                    Vector3 position = __instance.transform.position;

                    // 在当前位置生成粒子特效（类型78）
                    CreateParticle.SetParticle(
                        78, // 大蒜炸弹爆炸特效
                        position: position,
                        row: __instance.theBulletRow,
                        true
                    );
                    // 检测范围内的僵尸（半径1.5）
                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
                        new Vector2(position.x, position.y),
                        1.5f,
                        __instance.zombieLayer.value);
                    foreach (Collider2D collider in hitColliders)
                    {
                        // 尝试获取僵尸组件
                        Zombie component = collider.GetComponent<Zombie>();
                        if (component == null) continue;

                        // 行号检查（只对相邻行的僵尸生效）
                        int rowDifference = Mathf.Abs(component.theZombieRow - __instance.theBulletRow);
                        if (rowDifference > 1) continue;

                        // 范围伤害判定
                        if (!AoeDamage.InLandAoeRange(component.theStatus)) continue;
                        int damageBomb = __instance.Damage + 40 * component.poisonLevel;
                        if (Lawnf.TravelUltimate(13))
                            damageBomb = __instance.Damage + 400 * component.poisonLevel;
                        // 应用伤害和冰冻效果
                        component.TakeDamage(DmgType.NormalAll, damageBomb);
                        component.SetPoison();
                        component.AddPoisonLevel();
                        component.AddPoisonLevel();
                        component.AddPoisonLevel();
                        component.AddPoisonLevel();
                        component.AddPoisonLevel();
                    }

                    GameAPP.PlaySound(40, 0.2f, 1.0f);
                }
                // 销毁子弹对象
                __instance.PlaySound(zombie);
                CreateParticle.SetParticle(Core.ParticleID, __instance.transform.position, __instance.theBulletRow);
                __instance.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Zombie), nameof(Zombie.EatEffect))]
    public class Zombie_EatEffect
    {
        [HarmonyPrefix]
        public static bool Prefix(Zombie __instance, ref Plant plant, ref bool __result)
        {
            if (__instance != null && plant != null && !TypeMgr.IsDriverZombie(__instance.theZombieType) && !__instance.isChangingRow)
            {
                if ((int)plant.thePlantType == GarlicSuperGatling.PlantID)
                {
                    __instance.EatGarlic(plant, withSound: true);
                    __instance.eatGarlic = true;
                    __instance.garlicSpeed = 0f;
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SuperSnowGatling), nameof(SuperSnowGatling.TakeDamage))]
    public class SuperSnowGatling_TakeDamage
    {
        [HarmonyPrefix]
        public static void Prefix(SuperSnowGatling __instance, ref int damage, ref int damageType)
        {
            if ((int)__instance.thePlantType == GarlicSuperGatling.PlantID && damageType == 0)
            {
                if (__instance.PotType == PlantType.GarlicPot)
                    damage = 10;
                else
                    damage = 15;
            }
        }
    }

    [HarmonyPatch(typeof(UltimateFootballZombie), nameof(UltimateFootballZombie.AttackEffect))]
    public class UltimateFootballZombie_TakeDamage
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Plant plant)
        {
            if (plant != null && (int)plant.thePlantType == GarlicSuperGatling.PlantID)
            {
                return false;
            }
            return true;
        }
    }
}