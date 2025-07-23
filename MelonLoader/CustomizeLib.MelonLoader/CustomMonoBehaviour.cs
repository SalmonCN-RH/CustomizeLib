using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Random = System.Random;

namespace CustomizeLib.MelonLoader;
/*    /// <summary>
    /// 大哥的获取子弹，现在可以开大了
    /// </summary>
[HarmonyPatch(typeof(SuperSnowGatling), nameof(SuperSnowGatling.GetBulletType))]
[HarmonyPatch(typeof(SuperGatling), nameof(SuperGatling.GetBulletType))]
public static class SuperSnowGatling_GetBulletType
{
    [HarmonyPrefix]
    public static bool Prefix(SuperSnowGatling __instance, ref BulletType __result)
    {
        if (CustomCore.CustomPlantsSkinActive.ContainsKey(__instance.thePlantType))
        {
            Dictionary<int, int> bulletDic = CustomPlantMonoBehaviour.BulletTypes[__instance.thePlantType];
            List<int> bulletTypes = [.. bulletDic.Keys];
            BulletType bulletType = (BulletType)bulletTypes[new Random().Next(0, bulletTypes.Count)];
            __result = bulletType;
            return false;
        }
        return true;
    }
}*/

/// <summary>
/// made by @likefengzi
/// </summary>
[RegisterTypeInIl2Cpp]
public class CustomPlantMonoBehaviour : MonoBehaviour
{
    /// <summary>
    /// 自定义恢复
    /// </summary>
    public void CustomAnimHeal(float num)
    {
        if (ThisPlant.thePlantHealth < ThisPlant.thePlantMaxHealth)
        {
            try
            {
                ThisPlant.Recover((int)(ThisPlant.thePlantMaxHealth * num));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    /// <summary>
    /// 自定义射击
    /// </summary>
    public void CustomAnimShoot()
    {
        Dictionary<int, int> bulletDic = BulletTypes[ThisPlant.thePlantType];
        List<int> bulletTypes = [.. bulletDic.Keys];
        BulletType bulletType = (BulletType)bulletTypes[new Random().Next(0, bulletTypes.Count)];
        BulletMoveWay bulletMoveWay = (BulletMoveWay)bulletDic[(int)bulletType];
        Bullet bullet = Board.Instance.GetComponent<CreateBullet>().SetBullet(
            (float)(ThisPlant.shoot.position.x + 0.1f),
            ThisPlant.shoot.position.y,
            ThisPlant.thePlantRow,
            bulletType, bulletMoveWay);
        bullet.Damage = ThisPlant.attackDamage;
        //投射抛物线子弹
        if (bulletMoveWay == BulletMoveWay.Throw || bulletMoveWay == BulletMoveWay.Quick_throw)
        {
            bullet.targetPlant = ThisPlant;
            //搜索同一行僵尸
            //bullet.targetZombie = Plant.SearchZombie().GetComponent<Zombie>();
            bullet.targetZombie = this.SearchZombieInSameRow(ThisPlant);
            if (bullet.targetZombie != null)
            {
                Vector2 startPosition = new(ThisPlant.transform.position.x, ThisPlant.transform.position.y);
                float t1 = Time.time - 0.5f;
                Vector2 firstPlace = new(bullet.targetZombie.transform.position.x,
                    bullet.targetZombie.transform.position.y);
                float t2 = Time.time;
                Vector2 secondPlace = firstPlace;
                float flightTime = 1.5f;
                //计算抛物线
                float[] calculate = [.. Lawnf.CalculateProjectileParameters(startPosition, t1, firstPlace, t2, secondPlace, flightTime)];
                try
                {
                    bullet.Vx = calculate[1];
                    bullet.Vy = calculate[2];
                    bullet.detaVy = -calculate[3];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                bullet.Die();
            }
        }

        //加农炮发射方式，但是有BUG，修不好
        if (bulletMoveWay == BulletMoveWay.Cannon)
        {
            bullet.targetZombie = this.SearchZombieInSameRow(ThisPlant);
            if (bullet.targetZombie != null)
            {
                bullet.cannonPos = bullet.targetZombie.transform.position;
            }
            else
            {
                bullet.Die();
            }
        }
    }

    public void CustomAnimSuperShoot()
    {
    }

    public Zombie? SearchZombieInSameRow(Plant plant)
    {
        foreach (Zombie zombie in Board.Instance.zombieArray)
        {
            if (zombie != null && zombie.gameObject.activeInHierarchy)
            {
                if (zombie.theZombieRow == plant.thePlantRow)
                {
                    if (plant.vision > zombie.transform.position.x)
                    {
                        if (zombie.transform.position.x > plant.transform.position.x &&
                            plant.SearchUniqueZombie(zombie) && !zombie.isMindControlled)
                        {
                            return zombie;
                        }
                    }
                }
            }
        }

        return null;
    }

    public Plant ThisPlant => gameObject.GetComponent<Plant>();

    //public BulletType bulletType;
    public static Dictionary<PlantType, Dictionary<int, int>> BulletTypes = [];
}

// 跟踪自定义卡牌状态
[RegisterTypeInIl2Cpp]
public class CheckCardState : MonoBehaviour
{
    public GameObject? card = null;
    public PlantType cardType = PlantType.Nothing;
    public GameObject? movingCardUI = null;

    public void Start()
    {
        CustomCore.checkBehaviours.Add(this);
    }

    public void CheckState()
    {
        List<PlantType> cardsOnSeedBank = new List<PlantType>();
        GameObject seedGroup = InGameUI.Instance.SeedBank.transform.GetChild(0).gameObject;
        for (int i = 0; i < seedGroup.transform.childCount; i++)
        {
            GameObject seed = seedGroup.transform.GetChild(i).gameObject;
            if (seed.transform.childCount > 0)
                cardsOnSeedBank.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType);
        }

        if (card != movingCardUI && card.transform.childCount >= 2 && card.transform.GetChild(1).GetComponent<CardUI>().thePlantType == cardType)
        {
            if (cardsOnSeedBank.Contains(cardType))
                card.transform.GetChild(1).gameObject.SetActive(false);
            else
                card.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}