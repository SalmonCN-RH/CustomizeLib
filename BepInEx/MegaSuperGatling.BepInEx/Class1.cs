using AlmanacData;
using BepInEx;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Unity;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using static Il2CppSystem.String;

namespace MegaSuperGatling.BepInEx
{
    [BepInPlugin("salmon.megasupergatling", "MegaSuperGatling", "1.0")]
    public class Core : CorePlugin
    {
        public static List<BulletType> SuperGatlingBullet = new List<BulletType>
        {
            BulletType.Bullet_pea,
            BulletType.Bullet_snowPea,
            BulletType.Bullet_extremeSnowPea,
            BulletType.Bullet_iceSpark,
            BulletType.Bullet_firePea_garlic,
            BulletType.Bullet_firePea_orange,
            BulletType.Bullet_firePea_purple,
            BulletType.Bullet_firePea_red,
            BulletType.Bullet_firePea_super,
            BulletType.Bullet_firePea_ultimate,
            BulletType.Bullet_firePea_yellow,
            BulletType.Bullet_helmetPea_black_fire,
            BulletType.Bullet_hypnoPea_fire,
            BulletType.Bullet_pea_doom_fire,
            BulletType.Bullet_hypnoPea,
            BulletType.Bullet_pea_jala,
            BulletType.Bullet_pea_garlic
        };
        public static BuffID BuffID = -1;

        public override void OnStart()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            BuffID = CustomCore.RegisterCustomBuff("五阶升级：超级机枪射手系列的攻击力x10，其效果大幅加强", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<SuperGatling>(),
                15000, PlantType.SuperGatling);
        }
    }

    public class DamageAdder : MonoBehaviour
    {
        public Plant plant;
        public int oriDamage = 20;

        public void Update()
        {
            plant.attackDamage = oriDamage * 10;
        }
    }

    [HarmonyPatch(typeof(AlmanacDataLoader))]
    public static class AlmanacDataLoaderPatch
    {
        private static bool init = false;

        [HarmonyPatch(nameof(AlmanacDataLoader.LoadPlantData))]
        [HarmonyPostfix]
        public static void PostLoadPlantData()
        {
            if (init) return;
            AlmanacDataLoader.plantDatas[PlantType.SuperGatling].info += "\n<color=#3D1400>词条1:</color><color=red>五阶升级：超级机枪射手的攻击力x10，发射的子弹改为超级机枪射手系列的随机子弹，每次发射时有50%概率额外散射20发随机子弹</color>";
            AlmanacDataLoader.plantDatas[PlantType.SuperSnowGatling].info += "\n<color=#3D1400>词条1:</color><color=red>五阶升级：超级寒冰机枪射手的攻击力x10，发射的子弹改为冰锥，子弹命中的首个目标赋予1秒冻结，对冻结或免疫寒冷的僵尸伤害x10</color>";
            AlmanacDataLoader.plantDatas[PlantType.SuperThreeGatling].info += "\n<color=#3D1400>词条1:</color><color=red>五阶升级：三线超级机枪射手的攻击力x10，大招效果改为每0.02秒散射9颗伤害为3倍的豌豆</color>";
            init = true;
        }
    }

    [HarmonyPatch(typeof(SuperSnowGatling))]
    public static class SuperSnowGatlingPatch
    {
        [HarmonyPatch(nameof(SuperSnowGatling.Shoot1))]
        [HarmonyPostfix]
        public static void PostShoo1(SuperSnowGatling __instance)
        {
            if (__instance.thePlantType == PlantType.SuperGatling)
            {
                if (Lawnf.TravelAdvanced(Core.BuffID) && UnityEngine.Random.Range(1, 101) <= 15)
                {
                    __instance.StartCoroutine(StartExtraShoot(__instance));
                }
            }
        }

        [HarmonyPatch(nameof(SuperSnowGatling.SuperShoot), [typeof(float), typeof(float), typeof(float), typeof(float), typeof(BulletMoveWay), typeof(int)])]
        [HarmonyPostfix]
        public static void PostSuperShoot(SuperSnowGatling __instance)
        {
            if (__instance.thePlantType == PlantType.SuperGatling)
            {
                if (Lawnf.TravelAdvanced(Core.BuffID) && UnityEngine.Random.Range(0, 1) == 0)
                {
                    for (int i = 0; i < UnityEngine.Random.Range(1, 3); i++)
                    {
                        var bullet = CreateBullet.Instance.SetBullet(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow,
                            Core.SuperGatlingBullet[UnityEngine.Random.Range(0, Core.SuperGatlingBullet.Count)], BulletMoveWay.Free);
                        bullet.Damage = __instance.attackDamage * UnityEngine.Random.Range(1, 4);
                        bullet.transform.Rotate(0f, 0f, UnityEngine.Random.Range(-6f, 6f));
                        bullet.normalSpeed *= 2;
                    }
                }
            }
        }

        [HarmonyPatch(nameof(SuperSnowGatling.GetBulletType))]
        [HarmonyPostfix]
        public static void PostGetBulletType(SuperSnowGatling __instance, ref BulletType __result)
        {
            if (__instance.thePlantType == PlantType.SuperSnowGatling && Lawnf.TravelAdvanced(Core.BuffID))
            {
                __result = BulletType.Bullet_iceSpark;
            }
        }

        public static IEnumerator StartExtraShoot(SuperSnowGatling plant)
        {
            int count = 20;
            while (count > 0)
            {
                if (plant == null) yield break;
                for (int i = 0; i < UnityEngine.Random.Range(3, 10); i++)
                {
                    if (count <= 0) break;
                    var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow,
                        Core.SuperGatlingBullet[UnityEngine.Random.Range(0, Core.SuperGatlingBullet.Count)], BulletMoveWay.Free);
                    bullet.Damage = plant.attackDamage * UnityEngine.Random.Range(1, 4);
                    bullet.transform.Rotate(0f, 0f, UnityEngine.Random.Range(-6f, 6f));
                    bullet.normalSpeed *= 2;
                    count--;
                }
                yield return null; // 等待一帧再发射
            }
            yield break;
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch(nameof(Plant.Start))]
        [HarmonyPostfix]
        public static void PostStart(Plant __instance)
        {
            if (Lawnf.TravelAdvanced(Core.BuffID) && (__instance.TryGetComponent<SuperSnowGatling>(out var _) || __instance.TryGetComponent<SuperThreeGatling>(out var _)))
            {
                var comp = __instance.AddComponent<DamageAdder>();
                comp.oriDamage = __instance.attackDamage;
                comp.plant = __instance;
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_iceSpark))]
    public static class Bullet_iceSparkPatch
    {
        [HarmonyPatch(nameof(Bullet_iceSpark.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_iceSpark __instance, ref Zombie zombie)
        {
            if (__instance.fromType == PlantType.SuperSnowGatling)
            {
                __instance.penetrationTimes = 3;
                if (CoreTools.TravelAdvanced("势如破竹"))
                    __instance.penetrationTimes = 100_0000;
                if (__instance.hitTimes == 1)
                    zombie.SetFreeze(1f);
                zombie.AddfreezeLevel(10);
                zombie.SetCold(10f);

                int damage = __instance.Damage;

                if (zombie.HasBuff(EffectType.Freeze) || !zombie.HasBuff(EffectType.Cold))
                    damage *= 10;

                ParticleManager.Instance.SetParticle(ParticleType.SnowPeaSplat, __instance.transform.position, zombie.theZombieRow);

                zombie.TakeDamage(DmgType.IceAll, damage, __instance.fromType);

                __instance.PlaySound(zombie);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SuperThreeGatling))]
    public static class SuperThreeGatlingPatch
    {
        [HarmonyPatch(nameof(SuperThreeGatling.SuperShoot))]
        [HarmonyPrefix]
        public static bool PreSuperShoot(SuperThreeGatling __instance)
        {
            if (__instance.thePlantType == PlantType.SuperThreeGatling && Lawnf.TravelAdvanced(Core.BuffID))
            {
                for (int i = 0; i < 3; i++)
                {
                    var bullet = CreateBullet.Instance.SetBullet(__instance.shoot.position.x + UnityEngine.Random.Range(-0.1f, 0.1f),
                        __instance.shoot.position.y + UnityEngine.Random.Range(-0.1f, 0.1f), __instance.thePlantRow, BulletType.Bullet_pea, BulletMoveWay.Free);
                    bullet.Damage = __instance.attackDamage * 3;
                    bullet.transform.Rotate(0f, 0f, UnityEngine.Random.Range(-8f, 8f));
                    bullet.normalSpeed *= 2;
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPostfix]
        public static void PostStart(GameAPP __instance) => __instance.SetData("MegaSuperGatling_BuffID", Core.BuffID);
    }
}
