using BepInEx;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Unity.VisualScripting;
using UnityEngine;

namespace SuperSniperGatling.BepInEx
{
    [BepInPlugin("salmon.supersnipergatling", "SuperSniperGatling", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "supersnipergatling");
            CustomCore.RegisterCustomPlant<SniperPea, SuperSniperGatling>(SuperSniperGatling.PlantID, ab.GetAsset<GameObject>("SuperSniperGatlingPrefab"),
                ab.GetAsset<GameObject>("SuperSniperGatlingPreview"),
                new List<(PlantType, PlantType)>()
                {
                    (PlantType.SuperGatling, PlantType.SniperPea),
                    (PlantType.SniperPea, PlantType.SuperGatling)
                }.ToIntegerList(), 1.5f, 0f, 300, 300, 7.5f, 1000);
            CustomCore.AddPlantAlmanacStrings(SuperSniperGatling.PlantID, $"超级狙击射手",
                "介绍\n\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300x6/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①全图索敌，优先对空\n" +
                "②每60下狙击，对目标造成爆头伤害\n" +
                "③每次狙击有2%概率开大，5秒内，每0.02秒对索敌目标造成一次狙击</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>超级机枪射手+狙击射手</color>\n\n" +
                "<color=#3D1400>宝开鱼</color>");
        }
    }

    public class SuperSniperGatling : MonoBehaviour
    {
        public static ID PlantID = 1937;

        public float timer = 0f;
        public float super = 0f;

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame || plant == null) return;
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer = 0f;
                    plant.anim.SetBool("shooting", false);
                }
            }
            if (super > 0f && timer > 0f)
            {
                super -= Time.deltaTime;
                if (super <= 0f) SuperEvent();
            }
        }

        public void Shoot1()
        {
            GameAPP.PlaySound(40, 0.2f, 1.0f);
            var shootCount = CoreTools.TravelUltimate("精准射击") ? 40 : 60;

            if (plant.targetZombie != null)
            {
                if (!plant.SearchUniqueZombie(plant.targetZombie) && plant.SearchZombie() == null && 
                    plant.targetZombie != null && (plant.targetZombie.theStatus == ZombieStatus.Dying || plant.targetZombie.beforeDying))
                {
                    plant.targetZombie = null;
                    return;
                }

                plant.attackCount++;
                if (plant.attackCount % shootCount != 0)
                    plant.AttackZombie(plant.targetZombie, plant.attackDamage, DamageType.Shieldless);
                else
                    plant.AttackZombie(plant.targetZombie, 100_0000, DamageType.MaxDamage);
            }

            if (UnityEngine.Random.Range(1, 101) <= (CoreTools.TravelUltimate("极速爆发") ? 6 : 2) && timer <= 0f)
            {
                timer = 5f;
                SuperEvent();
                plant.flashCountDown = 5f;
                plant.anim.SetBool("shooting", true);
                plant.Recover(plant.thePlantMaxHealth);
            }
        }

        public void SuperEvent()
        {
            try
            {
                if (plant.targetZombie == null || !plant.SearchUniqueZombie(plant.targetZombie))
                    plant.targetZombie = plant.SearchZombie().GetComponent<Zombie>();
                plant.Shoot1();
                super = 0.02f;
            }
            catch (NullReferenceException) { }
        }

        public GameObject SearchZombie()
        {
            plant.zombieList.Clear();

            float closestDistance = float.MaxValue;
            Zombie nearestZombie = null;
            var list = Lawnf.GetAllZombies().ToArray().ToList().
                OrderByDescending(z => !Lawnf.InLandStatus(z.theStatus)).ThenBy(z => Vector2.Distance(z.axis.position, plant.axis.position));
            foreach (var zombie in list)
            {
                if (zombie != null && plant.SearchUniqueZombie(zombie))
                {
                    float distance = Vector2.Distance(zombie.axis.position, plant.axis.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        nearestZombie = zombie;
                    }
                }
            }

            if (nearestZombie != null)
            {
                plant.targetZombie = nearestZombie;
                return nearestZombie.gameObject;
            }

            return null;
        }

        public SniperPea plant => gameObject.GetComponent<SniperPea>();
    }

    [HarmonyPatch(typeof(SniperPea))]
    public static class SniperPeaPatch
    {
        [HarmonyPatch(nameof(SniperPea.Shoot1))]
        [HarmonyPrefix]
        public static bool PreShoot1(SniperPea __instance)
        {
            if (__instance.thePlantType == SuperSniperGatling.PlantID)
            {
                __instance.GetOrAddComponent<SuperSniperGatling>()?.Shoot1();
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(SniperPea.SearchZombie))]
        [HarmonyPrefix]
        public static bool PreSearchZombie(SniperPea __instance, ref GameObject __result)
        {
            if (__instance.thePlantType == SuperSniperGatling.PlantID)
            {
                __result = __instance.GetOrAddComponent<SuperSniperGatling>()?.SearchZombie();
                return false;
            }
            return true;
        }
    }
}
