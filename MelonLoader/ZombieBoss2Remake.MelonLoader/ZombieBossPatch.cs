using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;
using static Il2Cpp.ZombieBoss;
using static MelonLoader.MelonLogger;

namespace ZombieBoss2Remake.MelonLoader
{
    [HarmonyPatch(typeof(BigWallNut))]
    public static class BigWallNutPatch
    {
        [HarmonyPatch("OnTriggerEnter2D")]
        [HarmonyPatch("OnTriggerStay2D")]
        [HarmonyPrefix]
        public static bool PreOnTriggerStay2D(BigWallNut __instance, Collider2D collision)
        {
            if (collision.TryGetComponent<ZombieBoss2Remake>(out var boss2))
            {
                __instance.Die();
                UnityEngine.Object.Destroy(__instance.gameObject);
                for (int i = 0; i < 20; i++)
                {
                    CreateItem.Instance.SetCoin(Mouse.Instance.GetColumnFromX(boss2.transform.position.x), boss2.zombie.theZombieRow, (int)ItemType.DiamondCoin, 1);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Board))]
    public static class BoardPatch
    {
        [HarmonyPatch(nameof(Board.GetMoney))]
        [HarmonyPostfix]
        public static void PostGetMoney(ref int count)
        {
            if (Board.Instance.ObjectExist<ZombieBoss2Remake>())
            {
                foreach (var item in Board.Instance.GetComponentsInChildren<ZombieBoss2Remake>())
                {
                    item.zombie.theHealth += count / 10;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bullet))]
    public static class BulletPatch
    {
        [HarmonyPatch(nameof(Bullet.PostionUpdate))]
        [HarmonyPrefix]
        public static bool PrePositionUpdate(Bullet __instance)
        {
            return __instance.theMovingWay is not 99;
        }
    }

    [HarmonyPatch(typeof(InGameBtn))]
    public static class InGameBtnPatch
    {
        [HarmonyPatch(nameof(InGameBtn.OnMouseUpAsButton))]
        [HarmonyPatch(nameof(InGameBtn.Update))]
        [HarmonyPrefix]
        public static bool PreOnMouseUpAsButton(InGameBtn __instance) => __instance.buttonNumber is not 3 || !Board.Instance.ObjectExist<ZombieBoss2Remake>();
    }

    [HarmonyPatch(typeof(Money), "UsedEvent")]
    public static class MoneyPatch
    {
        public static void Postfix(ref int cost)
        {
            if (Board.Instance.ObjectExist<ZombieBoss2Remake>())
            {
                foreach (var item in Board.Instance.gameObject.GetComponentsInChildren<ZombieBoss2Remake>())
                {
                    item.zombie.theHealth -= cost / 10;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch("Crashed")]
        [HarmonyPrefix]
        public static void Crashed(Plant __instance, ref Zombie zombie)
        {
            if (zombie is not null && zombie.TryCast<ZombieBoss2>() is not null)
            {
                CreateZombie.Instance.SetZombie(__instance.thePlantRow, ZombieType.RandomPlusZombie, __instance.transform.position.x);
            }
        }
    }

    [HarmonyPatch(typeof(ZombieBoss2))]
    public static class ZombieBoss2Patch
    {
        [HarmonyPatch("BodyTakeDamage")]
        [HarmonyPrefix]
        public static bool PreBodyTakeDamage(ZombieBoss2 __instance, ref int theDamage)
        {
            if (__instance.bossStatus is not BossStatus.head_idle) return false;
            __instance.Remake().BodyTakeDamage(theDamage);
            return false;
        }
    }

    [HarmonyPatch(typeof(ZombieBoss))]
    public static class ZombieBossPatch
    {
        [HarmonyPatch("AnimCrash")]
        [HarmonyPatch("AnimBungi")]
        [HarmonyPostfix]
        public static void AnimBungi(ZombieBoss __instance)
        {
            if (__instance.theZombieType is not ZombieType.ZombieBoss2) return;
            __instance.Remake().SummonMoney();
        }

        [HarmonyPatch("AnimPutBall")]
        [HarmonyPrefix]
        public static bool AnimPutBall(ZombieBoss __instance) => __instance.theZombieType is not ZombieType.ZombieBoss2;

        [HarmonyPatch("AnimSpawn")]
        [HarmonyPrefix]
        public static bool AnimSpawn(ZombieBoss __instance)
        {
            if (__instance.theZombieType is not ZombieType.ZombieBoss2) return true;
            if (__instance.spawnCount <= 0) return false;
            var zt = __instance.GetZombieType();
            CreateZombie.Instance.SetZombie(__instance.targetRow, zt, __instance.spawnPosition.position.x);

            if (__instance.Remake().ThirdStage)
            {
                for (int i = 0; i < Board.Instance.rowNum; i++)
                {
                    CreateZombie.Instance.SetZombie(i, zt);
                    __instance.summonCount++;
                }
            }
            __instance.spawnCount--;
            __instance.summonCount++;

            __instance.bossStatus = BossStatus.idle;
            return false;
        }

        public static T GetRandomItem<T>(this IList<T> list) => list[UnityEngine.Random.RandomRangeInt(0, list.Count)];

        [HarmonyPatch("DieExplode")]
        [HarmonyPostfix]
        public static void PostDieExplode(ZombieBoss __instance)
        {
            if (__instance.theZombieType is not ZombieType.ZombieBoss2) return;
            if (__instance.transform.TryGetComponent<ZombieBoss2Remake>(out var remake))
            {
                remake.Alive = false;
            }
        }

        [HarmonyPatch(nameof(ZombieBoss.GetZombieType))]
        [HarmonyPrefix]
        public static bool PreGetZombieType(ZombieBoss __instance, ref ZombieType __result)
        {
            if (__instance.theZombieType is not ZombieType.ZombieBoss2) return true;
            __result = __instance.Remake().GetZombieType();
            return false;
        }

        public static ZombieBoss2Remake Remake(this ZombieBoss zomboss2) => zomboss2.GetComponent<ZombieBoss2Remake>();

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update(ZombieBoss __instance)
        {
            if (__instance.theZombieType is not ZombieType.ZombieBoss2 || GameAPP.theGameStatus is not GameStatus.InGame) return true;
            __instance.Remake().UpdateEx();
            return false;
        }
    }
}