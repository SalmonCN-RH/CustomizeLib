using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using static Il2CppSystem.String;

namespace UltimateCornFume.BepInEx
{
    [BepInPlugin("salmon.ultimatecornfume", "UltimateCornFume", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "ultimatecornfume");
            CustomCore.RegisterCustomPlant<CornFume, UltimateCornFume>(UltimateCornFume.PlantID, ab.GetAsset<GameObject>("UltimateCornFumePrefab"),
                ab.GetAsset<GameObject>("UltimateCornFumePreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperCaltrop, (int)PlantType.FumeShroom),
                    ((int)PlantType.FumeShroom, (int)PlantType.SuperCaltrop)
                }, 2f, 0f, 100, 300, 7.5f, 350);
            CustomCore.AddPlantAlmanacStrings(UltimateCornFume.PlantID, $"窝油帝菇",
                "黏糊糊的黄油蘑菇，令大范围僵尸感受“醍醐灌顶”\n" +
                "<color=#0000FF>窝油帝刺同人亚种</color>\n\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                "<color=#3D1400>贴图作者：@屑红leong、@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转化配方：</color><color=red>大喷菇←→地刺</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>100</color>\n" +
                "<color=#3D1400>攻击范围：</color><color=red>本行及邻行半径3.7格内</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①自动索敌，受伤或僵尸在攻击范围内时，攻击一次\n" +
                "②攻击时，损失30血量，并对周围每个僵尸各投射1个黄油，使其定身4秒，若血量低于15%时则不再攻击\n" +
                "③血量低于1倍韧性且无攻击行为1秒后，每1秒，回复3%韧性的血量\n" +
                "④攻击时有25%概率投射黄油窝瓜，对周围的目标施加4秒定身，随后向最近的僵尸跳跃最远1.5格后落地，对压到的每个僵尸施加1秒定身，弹跳6次后消失</color>\n" +
                "<color=#3D1400>词条1:</color><color=red>尸愁之路II：攻击时固定损失的韧性减至10</color>\n" +
                "<color=#3D1400>词条2:</color><color=red>滑滑梯：待机回复的韧性x3，黄油窝瓜攻击僵尸时对免疫黄油定身的非车类僵尸强制定身1秒</color>\n\n" +
                "<color=#3D1400>黄油帝菇不喜欢别人叫他大叔，刚上植物高中的黄油帝菇，年纪轻轻就“一把年纪”，“太糟糕了，和我一起到校的老师问我教务处在哪里，我才第一天到学校！”黄油帝菇气呼呼的，像是一个充满气的气球！</color>");
            CustomCore.RegisterCustomBullet<Bullet_butter>(UltimateCornFume.BulletID, ab.GetAsset<GameObject>("Bullet_superCornSquash"));
            CustomCore.AddFusion((int)PlantType.SuperCaltrop, UltimateCornFume.PlantID, (int)PlantType.Caltrop);
        }
    }

    public class UltimateCornFume : MonoBehaviour
    {
        public static ID PlantID = 1966;
        public static ID BulletID = 1967;

        public float recover = 1f;

        public void Awake()
        {
            plant.shoot = transform.FindChild("FumeShroom_head/Shoot");
        }

        public void Update()
        {
            if (plant != null)
            {
                if (plant.thePlantHealth < plant.thePlantMaxHealth * 0.15f)
                    plant.thePlantAttackCountDown = 0.5f;
                if (plant.attributeCountdown <= 0f)
                {
                    recover -= Time.deltaTime * plant.attributeSpeed;
                    if (recover <= 0f)
                    {
                        plant.Recover(plant.thePlantMaxHealth * 0.05f * (CoreTools.TravelAdvanced("滑滑梯") ? 3f : 1f));
                        recover = 1f;
                    }
                }
                if (plant.thePlantAttackCountDown > 0f)
                {
                    plant.UpdateAttackCountDown();
                    if (plant.thePlantAttackCountDown <= 0f)
                    {
                        plant.thePlantAttackCountDown = plant.thePlantAttackInterval;
                        var existZombie = false;
                        foreach (var zombie in plant.GetZombies())
                        {
                            if (!plant.SearchUniqueZombie(zombie)) continue;
                            existZombie = true;
                            break;
                        }
                        if (existZombie)
                            plant.anim.SetTrigger("shoot");
                    }
                }
            }
        }

        public CornFume plant => gameObject.GetComponent<CornFume>();
    }

    [HarmonyPatch(typeof(CornFume))]
    public static class CornFumePatch
    {
        [HarmonyPatch(nameof(CornFume.AnimShoot))]
        [HarmonyPrefix]
        public static bool PreAnimShoot(CornFume __instance)
        {
            if (__instance.thePlantType == UltimateCornFume.PlantID)
            {
                if (__instance.thePlantHealth < __instance.thePlantMaxHealth * 0.15f)
                    return false;
                var zombies = __instance.GetZombies();
                
                if (zombies.Count > 0)
                {
                    foreach (var zombie in zombies)
                    {
                        Vector3 shootPos = __instance.shoot.position;
                        var bulletType = BulletType.Bullet_butter;
                        var existPot = __instance.PotType == PlantType.SuperCaltropPot;
                        if (UnityEngine.Random.Range(1, 101) <= (existPot ? 50 : 25))
                            bulletType = UltimateCornFume.BulletID;
                        var bullet = CreateBullet.Instance.SetBullet(shootPos.x, shootPos.y, zombie.theZombieRow, bulletType, BulletMoveWay.Throw);
                        bullet.Damage = __instance.attackDamage;
                        bullet.fromType = __instance.thePlantType;
                        bullet.ThrowTo(zombie, new(), 1.5f.GetNullable());
                    }

                    GameAPP.PlaySound(106, 0.5f, 1f);
                    __instance.thePlantHealth -= (CoreTools.TravelAdvanced("尸愁之路II") ? 10 : 30);
                    __instance.FlashOnce();
                    __instance.UpdateText();
                    __instance.attributeCountdown = 2f;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(CornFume.GetZombies))]
        [HarmonyPrefix]
        public static bool PreGetZombies(CornFume __instance, ref Il2CppSystem.Collections.Generic.List<Zombie> __result)
        {
            if (__instance.thePlantType == UltimateCornFume.PlantID)
            {
                __result = new();
                foreach (var collider in Physics2D.OverlapCircleAll(__instance.axis.transform.position, CoreTools.ColumnX * 3.7f, LayerMask.GetMask("Zombie")))
                {
                    if (!collider.IsObjExist() || !collider.TryGetComponent<Zombie>(out var zombie) || !zombie.IsObjExist()) continue;
                    if (!__instance.SearchUniqueZombie(zombie)) continue;
                    if (Mathf.Abs(__instance.thePlantRow - zombie.theZombieRow) <= 1)
                        __result.Add(zombie);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch(nameof(Plant.SearchZombie))]
        [HarmonyPostfix]
        public static void PostSearchZombie(Plant __instance, ref GameObject __result)
        {
            if (__instance.thePlantType == UltimateCornFume.PlantID)
            {
                foreach (var zombie in __instance.GetComponent<CornFume>().GetZombies())
                {
                    __result = zombie.gameObject;
                    return;
                }
            }
        }

        [HarmonyPatch(nameof(Plant.Die))]
        [HarmonyPrefix]
        public static void PostDieEvent(Plant __instance, Plant.DieReason reason)
        {
            if (__instance.thePlantType == UltimateCornFume.PlantID && reason == Plant.DieReason.ByMix && 
                __instance.board.GetBoxType(__instance.thePlantColumn, __instance.thePlantRow) is BoxType.Water or BoxType.Roof or BoxType.River)
            {
                Lawnf.SetDroppedCard(__instance.axis.transform.position, PlantType.SuperCaltrop);
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_butter))]
    public static class Bullet_butterPatch
    {
        [HarmonyPatch(nameof(Bullet_butter.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == UltimateCornFume.BulletID)
            {
                GameAPP.PlaySound(100, 0.5f, 1f);

                CreateParticle.SetParticle(44, __instance.transform.position, zombie.theZombieRow);

                zombie.TakeDamage(DmgType.Shieldless, __instance.Damage, __instance.fromType);
                zombie.Buttered(4);
                __instance.hitCount = int.MinValue;
                __instance.velocity = new(0.5f, 0f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Bullet_butter.HitLand))]
        [HarmonyPrefix]
        public static bool PreHitLand(Bullet __instance)
        {
            if (__instance.theBulletType == UltimateCornFume.BulletID)
            {
                var butter = Resources.Load<GameObject>("items/littlesquash/LittleSquash_butter");
                if (butter == null) return false;
                var littleSquash = UnityEngine.Object.Instantiate(butter, __instance.transform.position, Quaternion.identity, __instance.board.transform).GetComponent<LittleSquash>();

                if (littleSquash != null)
                {
                    littleSquash.theDamage = 300;
                    littleSquash.theRow = __instance.theBulletRow;
                    littleSquash.thePlantType = __instance.fromType;
                    Action<int, int> action = (row, damage) =>
                    {
                        Vector2 center = littleSquash.axis.position;
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, 1f, LayerMask.GetMask("Zombie"));

                        foreach (Collider2D col in colliders)
                        {
                            if (col.TryGetComponent(out Zombie zombie))
                            {
                                if (Lawnf.InLandStatus(zombie.theStatus) && zombie.theZombieRow == row)
                                {
                                    zombie.Buttered();
                                    if (CoreTools.TravelAdvanced("滑滑梯") && zombie.GetAttrTimers().butterTimer <= 0f && !TypeMgr.IsDriverZombie(zombie.theZombieType))
                                    {
                                        zombie.butterSpeed = 0f;
                                        zombie.GetAttrTimers().butterTimer = 1f;
                                    }
                                }
                            }
                        }
                    };
                    littleSquash.crashAction = action;
                    GameAPP.PlaySound(22, 0.5f, 1.0f);
                }
                __instance.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CreatePlant))]
    public static class CreatePlantPatch
    {
        [HarmonyPatch(nameof(CreatePlant.CheckBox))]
        [HarmonyPrefix]
        public static bool PreCheckBox(CreatePlant __instance, int theBoxColumn, int theBoxRow, PlantType theSeedType, ref bool __result)
        {
            if (__instance.board.GetBoxType(theBoxColumn, theBoxRow) is BoxType.Water or BoxType.Roof or BoxType.River && theSeedType == PlantType.Caltrop)
            {
                var list = Lawnf.Get1x1Plants(theBoxColumn, theBoxRow).ToSystemList();
                if (list.Any(p => p.thePlantType == UltimateCornFume.PlantID))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(CreatePlant.MixFail))]
        [HarmonyPrefix]
        public static bool PreMixFail(CreatePlant __instance, int theBoxColumn, int theBoxRow, PlantType newPlantType, ref bool __result)
        {
            if (__instance.board.GetBoxType(theBoxColumn, theBoxRow) is BoxType.Water or BoxType.Roof or BoxType.River && newPlantType == PlantType.SuperCaltrop)
            {
                var list = Lawnf.Get1x1Plants(theBoxColumn, theBoxRow).ToSystemList();
                if (list.Any(p => p.thePlantType == UltimateCornFume.PlantID))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(CreatePlant.CheckMix))]
        [HarmonyPostfix]
        public static void PostCheckMix(CreatePlant __instance, int theColumn, int theRow, ref Plant __result)
        {
            if (!__result.IsObjExist()) return;
            if (__instance.board.GetBoxType(theColumn, theRow) is BoxType.Water or BoxType.Roof or BoxType.River && __result.thePlantType == PlantType.SuperCaltrop)
            {
                var tuple = (__result.firstParent, __result.secondParent);
                if (tuple == ((PlantType)UltimateCornFume.PlantID, PlantType.Caltrop) || tuple == (PlantType.Caltrop, (PlantType)UltimateCornFume.PlantID))
                    __result.Die();
            }
        }
    }
}
