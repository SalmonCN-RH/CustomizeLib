using HarmonyLib;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace ObsidianDollZombie.BepInEx
{
    [HarmonyPatch(typeof(DiamondRandomZombie))]
    public static class DiamondRandomZombiePatch
    {
        [HarmonyPatch("SetRandomZombie")]
        [HarmonyPrefix]
        public static bool PreSetRandomZombie(DiamondRandomZombie __instance, ref GameObject __result)
        {
            if (__instance is not null && __instance.theZombieType is (ZombieType)98)
            {
                Vector3 position = __instance.axis.position;
                List<int> ids = [];
                if (Lawnf.TravelDebuff(ObsidianRandomZombie.Debuff))
                {
                    for (int i = 0; i < GameAPP.resourcesManager.allZombieTypes.Count; i++)
                    {
                        if (TypeMgr.IsBossZombie((ZombieType)i) && GameAPP.resourcesManager.zombiePrefabs[GameAPP.resourcesManager.allZombieTypes[i]] is not null)
                        {
                            ids.Add(i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < GameAPP.resourcesManager.allZombieTypes.Count; i++)
                    {
                        if (GameAPP.resourcesManager.zombiePrefabs[GameAPP.resourcesManager.allZombieTypes[i]] is not null && !TypeMgr.IsBossZombie((ZombieType)i) && !TypeMgr.NotRandomZombie((ZombieType)i))
                        {
                            ids.Add(i);
                        }
                    }
                }
                if (!__instance.isMindControlled)
                {
                    __result = CreateZombie.Instance.SetZombie(__instance.theZombieRow, (ZombieType)ids[UnityEngine.Random.RandomRangeInt(0, ids.Count)], __instance.transform.position.x);
                }
                else
                {
                    __result = CreateZombie.Instance.SetZombieWithMindControl(__instance.theZombieRow, (ZombieType)ids[UnityEngine.Random.RandomRangeInt(0, ids.Count)], __instance.transform.position.x);
                }
                UnityEngine.Object.Instantiate(GameAPP.particlePrefab[11], new Vector3(__instance.transform.position.x, position.y + 1f, 0f), Quaternion.identity).transform.SetParent(GameAPP.board.transform);
                __instance.summoned = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Mower))]
    public static class MowerPatch
    {
        [HarmonyPatch("OnTriggerStay2D")]
        [HarmonyPrefix]
        public static bool PreOnTriggerStay2D(Mower __instance, ref Collider2D collision)
        {
            GameObject gameObject = collision.gameObject;
            if (gameObject.CompareTag("Zombie") && gameObject.TryGetComponent<ObsidianRandomZombie>(out var z) && z.zombie is not null
                && z.zombie.theZombieRow == __instance.theMowerRow && !z.zombie.isMindControlled)
            {
                if (!z.HasMower)
                {
                    GameObject mower = __instance.gameObject;
                    UnityEngine.Object.Destroy(mower.GetComponent<BoxCollider2D>());
                    UnityEngine.Object.Destroy(mower.GetComponent<Animator>());
                    UnityEngine.Object.Destroy(mower.GetComponent<Mower>());
                    mower.transform.SetParent(z.gameObject.transform.FindChild("Zombie_innerarm_hand"));
                    mower.transform.localPosition = new(-0.8f, -1.6f);
                    mower.layer = mower.transform.parent.gameObject.layer;
                }
                return false;
            }
            if (gameObject.CompareTag("Zombie") && gameObject.TryGetComponent<ObsidianDollZombie>(out var z2) && z2.zombie is not null
                && z2.zombie.theZombieRow == __instance.theMowerRow && !z2.zombie.isMindControlled)
            {
                if (!z2.HasMower)
                {
                    GameObject mower = __instance.gameObject;
                    UnityEngine.Object.Destroy(mower.GetComponent<BoxCollider2D>());
                    UnityEngine.Object.Destroy(mower.GetComponent<Animator>());
                    UnityEngine.Object.Destroy(mower.GetComponent<Mower>());
                    mower.transform.SetParent(z2.gameObject.transform.FindChild("Zombie_innerarm_hand"));
                    mower.transform.localPosition = new(-0.8f, -1.6f);
                    mower.layer = mower.transform.parent.gameObject.layer;
                }
                z2.PickUpMower();
                return false;
            }

            return true;
        }
    }

    /*
    [HarmonyPatch(typeof(RandomZombie))]
    public static class RandomZombiePatch
    {
        [HarmonyPatch(nameof(RandomZombie.FirstArmorTakeDamage))]
        [HarmonyPrefix]
        public static bool PostFirstArmorTakeDamage(RandomZombie __instance)
        {
            if (__instance.theZombieType is (ZombieType)98)
            {
                if (__instance.theFirstArmorHealth < __instance.theFirstArmorMaxHealth * 2 / 3)
                {
                    __instance.theFirstArmorBroken = 1;
                    __instance.theFirstArmor.GetComponent<SpriteRenderer>().sprite = GameAPP.spritePrefab[204];
                    return false;
                }
                if (__instance.theFirstArmorHealth < __instance.theFirstArmorMaxHealth / 3)
                {
                    __instance.theFirstArmorBroken = 2;
                    __instance.theFirstArmor.GetComponent<SpriteRenderer>().sprite = GameAPP.spritePrefab[205];
                    return false;
                }
                if (__instance.theFirstArmorHealth >= __instance.theFirstArmorMaxHealth * 2 / 3)
                {
                    __instance.theFirstArmorBroken = 0;
                    __instance.theFirstArmor.GetComponent<SpriteRenderer>().sprite = GameAPP.spritePrefab[207];
                    return false;
                }
            }
            return true;
        }
    }
    */

    [HarmonyPatch(typeof(Zombie))]
    public static class ZombiePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(Zombie __instance)
        {
            if (__instance.TryCast<UltimateGargantuar>() is not null && GameAPP.theGameStatus is GameStatus.InGame)
            {
                if (!__instance.isMindControlled)
                {
                    CreateZombie.Instance.SetZombie(__instance.theZombieRow, (ZombieType)98, __instance.transform.position.x);
                }
                else
                {
                    CreateZombie.Instance.SetZombieWithMindControl(__instance.theZombieRow, (ZombieType)98, __instance.transform.position.x);
                }
            }
            if (__instance.TryCast<DiamondRandomZombie>() is not null && UnityEngine.Random.RandomRangeInt(0, 9) == 1 && GameAPP.theGameStatus is GameStatus.InGame)
            {
                CreateZombie.Instance.SetZombie(__instance.theZombieRow, (ZombieType)99, __instance.transform.position.x);
            }
        }

        [HarmonyPatch("AttackEffect")]
        [HarmonyPrefix]
        public static bool PreAttackEffect(Zombie __instance, ref Plant plant)
        {
            if (__instance.theZombieType is (ZombieType)99 && __instance.gameObject.TryGetComponent<ObsidianDollZombie>(out var z)
                && !__instance.isMindControlled && z.HasMower)
            {
                plant.Die();
                return false;
            }

            return true;
        }

        [HarmonyPatch("FindAndDestoryZombieHead")]
        [HarmonyPatch("SetCold")]
        [HarmonyPatch("SetFreeze")]
        [HarmonyPatch("Warm")]
        [HarmonyPatch("KnockBack")]
        [HarmonyPrefix]
        public static bool PreKnockBack(Zombie __instance) => __instance.theZombieType is not (ZombieType)98 or (ZombieType)99;
    }

    public class ObsidianRandomZombie : MonoBehaviour
    {
        public ObsidianRandomZombie() : base(ClassInjector.DerivedConstructorPointer<ObsidianRandomZombie>()) => ClassInjector.DerivedConstructorBody(this);

        public ObsidianRandomZombie(IntPtr i) : base(i)
        {
        }

        public void Awake()
        {
            if (GameAPP.theGameStatus is 0 && zombie is not null)
            {
                zombie.theFirstArmor = gameObject.transform.FindChild("Zombie_head").GetChild(0).gameObject;
                zombie.butterHead = zombie.theFirstArmor;
                this.StartCoroutine(ChangeRoad());
            }
        }

        [HideFromIl2Cpp]
        public IEnumerator ChangeRoad()
        {
            do
            {
                yield return new WaitForSeconds(3);
                try
                {
                    if (Board.Instance is not null && zombie is not null && !zombie.isPreview && gameObject is not null
                        && !gameObject.IsDestroyed() && zombie is not null)
                    {
                        zombie.Garliced();
                    }
                    else
                    {
                        break;
                    }
                }
                catch { break; }
            } while (Board.Instance is not null && zombie is not null && zombie.isActiveAndEnabled && zombie.theHealth > 0);
        }

        public void Start()
        {
            if (GameAPP.theGameStatus is 0 && zombie is not null)
            {
                zombie!.theFirstArmorType = Zombie.FirstArmorType.Doll;
                zombie.theZombieType = (ZombieType)98;
            }
        }

        public static int Debuff { get; set; } = -1;
        public bool HasMower { get; set; } = false;
        public DiamondRandomZombie? zombie => gameObject.TryGetComponent<DiamondRandomZombie>(out var z) ? z : null;
    }
}