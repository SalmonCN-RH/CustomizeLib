using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using Unity.VisualScripting;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Basic;

namespace PuffUltimateGatling.BepInEx
{
    [BepInPlugin("salmon.puffultimategatling", "PuffUltimateGatling", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<PuffUltimateGatling>();
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_puffSuperCherry>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "puffultimategatling");
            CustomCore.RegisterCustomPlant<UltimateGatling, PuffUltimateGatling>((int)PuffUltimateGatling.PlantID, ab.GetAsset<GameObject>("PuffUltimateGatlingPrefab"),
                ab.GetAsset<GameObject>("PuffUltimateGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SmallPuff, (int)PlantType.UltimateGatling)
                }, 1.5f, 0f, 200, 300, 7.5f, 950);
            CustomCore.RegisterCustomPlantSkin<UltimateGatling, PuffUltimateGatling>((int)PuffUltimateGatling.PlantID, ab.GetAsset<GameObject>("PuffUltimateGatlingPrefabSkin"),
                ab.GetAsset<GameObject>("PuffUltimateGatlingPreviewSkin"), new List<(int, int)>
                {
                    ((int)PlantType.SmallPuff, (int)PlantType.UltimateGatling)
                }, 1.5f, 0f, 200, 300, 7.5f, 950);
            CustomCore.RegisterCustomBullet<Bullet_superCherry, Bullet_puffSuperCherry>((BulletType)PuffUltimateGatling.PlantID, ab.GetAsset<GameObject>("Bullet_puffSuperCherry"));
            CustomCore.AddPlantAlmanacStrings((int)PuffUltimateGatling.PlantID, $"究极小樱桃射手({(int)PuffUltimateGatling.PlantID})",
                "每轮发射四颗小型爆炸樱桃子弹，对于缩小的僵尸威力加倍。\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+究极樱桃射手</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>200x4/1.5秒</color>\n" +
                "<color=#3D1400>特性：</color><color=red>低矮，可密植</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①免疫樱桃爆炸伤害。\n" +
                "②发射迷你爆炸樱桃，造成半径0.5格无衰减溅射，对缩小的僵尸伤害x2。</color>\n" +
                "<color=#3D1400>词条1:</color><color=red>力大砖飞：植物方的樱桃爆炸伤害x3（樱桃炸弹除外）</color>\n" +
                "<color=#3D1400>词条2:</color><color=red>快速填装：攻击间隔减半</color>\n" +
                "<color=#3D1400>连携词条:</color><color=red>火力全开：究极小樱桃射手攻击时发射三行等量子弹，上下子弹将以正余弦函数轨迹飞行，子弹直击或溅射会使僵尸的体型缩小</color>\n\n" +
                "<color=#3D1400>“我参加过冷笑话大赛，还拿到过第一名！”究极小樱桃射手喜欢讲冷笑话，他想逗其他人开心，但是没人听他讲话，他看起来就像一个暴躁的樱桃，浑身写着生人勿近，实际上他去参加的冷笑话大赛，没有观众，也没有给他颁奖的主持人……</color>");
            CustomCore.TypeMgrExtra.IsPuff.Add(PuffUltimateGatling.PlantID);
        }
    }

    public class PuffUltimateGatling : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1929;

        public UltimateGatling plant => gameObject.GetComponent<UltimateGatling>();

        public void Start()
        {
            plant.shoot = gameObject.transform.FindChild("Shoot");
            plant.isShort = true;
        }

        public void AnimShoot_PuffUltimateGatling()
        {
            if (plant.starUp && !plant.fastShoot)
            {
                plant.StartCoroutine(plant.Shooting());
            }
            else
            {
                // 创建子弹
                var bullet = CreateBullet.Instance.SetBullet(
                    plant.shoot.transform.position.x,
                    plant.shoot.transform.position.y,
                    plant.thePlantRow,
                    (BulletType)PlantID,
                    BulletMoveWay.MoveRight,
                    false);

                bullet.Damage = plant.attackDamage;
                bullet.SetData("PuffUltimateGatling_byPuff", true);

                GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);

                if (Lawnf.TravelAdvanced(CoreTools.GetAdvBuffByString("火力全开")))
                {
                    Bullet up = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow, (BulletType)PlantID, BulletMoveWay.Sin, false);
                    up.Damage = plant.attackDamage;
                    up.SetData("PuffUltimateGatling_byPuff", true);

                    Bullet down = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow, (BulletType)PlantID, BulletMoveWay.Sin, false);
                    down.Damage = plant.attackDamage;
                    down.SetData("PuffUltimateGatling_byPuff", true);
                    down.theExistTime = 0.5f;
                }

                if (Lawnf.TravelAdvanced(CoreTools.GetAdvBuffByString("Rogue_究极樱桃射手专精I")))
                {
                    bullet.rogueStatus = 1;
                }
                else if (Lawnf.TravelAdvanced(CoreTools.GetAdvBuffByString("Rogue_究极樱桃射手专精II")))
                {
                    bullet.rogueStatus = 2;
                }

                if (plant.attributeCount > 0)
                {
                    if ((plant.attributeCount & 1) != 0)
                    {
                        Transform bulletTransform = bullet.transform;
                        Vector3 bulletPos = bulletTransform.position;
                        bulletPos.x -= 0f;
                        bulletPos.y -= 0.25f;
                        bulletPos.z -= 0f;
                        bulletTransform.position = bulletPos;
                    }

                    for (int i = 1; i <= plant.attributeCount; i++)
                    {
                        float offsetY = (i % 2 == 1) ? ((i + 1) / 2) * 0.5f : (i / 2) * -0.5f;

                        // 创建子弹
                        var additionalBullet = CreateBullet.Instance.SetBullet(
                            plant.shoot.transform.position.x,
                            plant.shoot.transform.position.y,
                            plant.thePlantRow,
                            (BulletType)PlantID,
                            BulletMoveWay.MoveRight,
                            false);

                        bullet.Damage = plant.attackDamage;
                        bullet.SetData("PuffUltimateGatling_byPuff", true);

                        GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);
                        Transform additionalTransform = additionalBullet.transform;

                        Vector3 additionalPos = additionalTransform.position;
                        additionalPos.x += 0f;
                        additionalPos.y += offsetY;
                        additionalPos.z += 0f;
                        additionalTransform.position = additionalPos;

                        additionalBullet.rogueStatus = 2; // 原字段: LODWORD(v34[7].monitor) = 2

                        if ((plant.attributeCount & 1) != 0)
                        {
                            Transform transformToAdjust = additionalBullet.transform;
                            Vector3 adjustedPos = transformToAdjust.position;
                            adjustedPos.x -= 0f;
                            adjustedPos.y -= 0.25f;
                            adjustedPos.z -= 0f;
                            transformToAdjust.position = adjustedPos;
                        }
                    }
                }
            }

            int soundId = UnityEngine.Random.Range(3, 5);
            GameAPP.PlaySound(soundId, 0.5f, 1f);
        }
    }

    public class Bullet_puffSuperCherry : MonoBehaviour
    {
    }

    [HarmonyPatch(typeof(Bullet_superCherry), nameof(Bullet_superCherry.HitZombie))]
    public static class Bullet_superCherry_HitZombie_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Bullet_superCherry __instance, ref Zombie zombie)
        {
            if (__instance.GetData("PuffUltimateGatling_byPuff") is not null && __instance.GetData("PuffUltimateGatling_byPuff") is true && __instance.theBulletType == (BulletType)PuffUltimateGatling.PlantID)
            {
                if (zombie.isSmall)
                    __instance.Damage *= 2;
                if (Lawnf.TravelAdvanced(CoreTools.GetAdvBuffByString("火力全开")))
                {
                    var collider = Physics2D.OverlapCircleAll(__instance.transform.position, 1.5f, zombie.zombieLayer);
                    foreach (var c in collider)
                    {
                        if (c.gameObject.TryGetComponent<Zombie>(out var z) && z != null)
                        {
                            z.BeSmall();
                        }
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(BombCherry), nameof(BombCherry.PlantTakeDamage))]
    public static class BombCherry_PlantTakeDamage_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Plant plant)
        {
            if (plant != null && plant.thePlantType == PuffUltimateGatling.PlantID)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(UltimateGatling._Shooting_d__3), nameof(UltimateGatling._Shooting_d__3.MoveNext))]
    public static class StarPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(UltimateGatling._Shooting_d__3 __instance, ref bool __result)
        {
            if (__instance.__4__this.thePlantType == PuffUltimateGatling.PlantID)
            {
                switch (__instance.__1__state)
                {
                    case 0:
                        // 初始化状态
                        __instance.__1__state = -1;
                        if (__instance.__4__this == null || __instance.__4__this.shoot == null)
                        {
                            __result = false;
                            return false;
                        }

                        // 记录射击位置
                        Vector3 shootPos = __instance.__4__this.shoot.position;
                        __instance._pos_5__2 = shootPos;
                        break;

                    case 1:
                        // 恢复执行状态
                        __instance.__1__state = -1;
                        __instance._i_5__3++;
                        break;
                }

                // 总共执行5轮射击
                if (__instance._i_5__3 >= 5)
                {
                    __result = false;
                    return false;
                }

                // 每轮发射5颗子弹
                for (int i = 0; i < 5; i++)
                {
                    // 计算随机偏移
                    float xOffset = UnityEngine.Random.Range(-0.1f, 0.1f);
                    float yOffset = UnityEngine.Random.Range(-0.1f, 0.1f);

                    // 创建子弹位置
                    float bulletX = __instance._pos_5__2.x + xOffset;
                    float bulletY = __instance._pos_5__2.y + yOffset;

                    // 创建子弹
                    Bullet bullet = CreateBullet.Instance.SetBullet(
                        bulletX, bulletY, __instance.__4__this.thePlantRow, (BulletType)PuffUltimateGatling.PlantID, BulletMoveWay.Right_free, false);

                    if (bullet != null)
                    {
                        // 设置随机旋转
                        float zRotation = UnityEngine.Random.Range(-15f, 15f) * Mathf.Deg2Rad;
                        Quaternion rotation = Quaternion.Euler(0f, 0f, zRotation);
                        bullet.transform.rotation = rotation;

                        // 设置子弹属性
                        bullet.fromType = __instance.__4__this.thePlantType;
                        bullet.Damage = __instance.__4__this.attackDamage;
                        bullet.normalSpeed = UnityEngine.Random.Range(12f, 14f);
                    }
                }

                // 等待下一帧
                __instance.__2__current = new UnityEngine.WaitForFixedUpdate();
                __instance.__1__state = 1;
                __result = true;
                return false;
            }
            return true;
        }
    }
}