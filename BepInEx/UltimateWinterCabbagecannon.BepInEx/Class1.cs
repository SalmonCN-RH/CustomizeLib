using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWinterCabbagecannon.BepInEx
{
    [BepInPlugin("salmon.ultimatewintercabbagecannon", "UltimateWinterCabbagecannon", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<UltimateWinterCabbagecannon>();
            ClassInjector.RegisterTypeInIl2Cpp<SubCabbage>();
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "ultimatewintercabbagecannon");
            CustomCore.RegisterCustomBullet<Bullet_cabbage>(UltimateWinterCabbagecannon.BulletID, ab.GetAsset<GameObject>("Bullet_iceDoomCabbage"));
            CustomCore.RegisterCustomPlant<CabbageCannon, UltimateWinterCabbagecannon>((int)UltimateWinterCabbagecannon.PlantID, ab.GetAsset<GameObject>("UltimateWinterCabbagecannonPrefab"),
                ab.GetAsset<GameObject>("UltimateWinterCabbagecannonPreview"), new List<(int, int)>
                {
                    ((int)PlantType.UltimateCannon, (int)PlantType.Cabbagepult)
                }, 0.75f, 0f, 300, 300, 7.5f, 575);
            CustomCore.RegisterCustomPlantSkin<CabbageCannon, UltimateWinterCabbagecannon>((int)UltimateWinterCabbagecannon.PlantID, ab.GetAsset<GameObject>("UltimateWinterCabbagecannonSkinPrefab"),
                ab.GetAsset<GameObject>("UltimateWinterCabbagecannonSkinPreview"), new List<(int, int)>
                {
                    ((int)PlantType.UltimateCannon, (int)PlantType.Cabbagepult)
                }, 0.75f, 0f, 300, 300, 7.5f, 575);
            CustomCore.AddPlantAlmanacStrings((int)UltimateWinterCabbagecannon.PlantID,
                $"究极冷寂迫击炮",
                "抛投卷心菜造成冷寂杀伤，炮口启动僚机后，会进行覆盖打击。\n" +
                "<color=#0000FF>究级冷寂加农炮同人亚种</color>\n\n" +
                "<color=#3D1400>贴图作者：@屑红leong、@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转换配方：</color><color=red>玉米投手←→卷心菜投手\n" +
                "*转化为究极冷寂加农炮时，装填时间重置为35秒</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300/0.75秒</color>\n" +
                "<color=#3D1400>索敌范围：</color><color=red>附近三行前方区域</color>\n" +
                "<color=#3D1400>特性：</color><color=red>巨型</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①攻击时，向索敌范围内每只僵尸各投掷一发冷寂卷心菜\n" +
                "②攻击时有5%概率启动僚机，发射2发攻击力6倍的集束炮弹，随后降落18枚子炮弹，二者命中均施加5秒冻结状态，对于免疫冻结状态的僵尸伤害x8</color>\n" +
                "<color=#3D1400>冷寂卷心菜：</color><color=red>①命中时先施加寒冷状态，赋予25点冻结值，然后造成半径1格爆炸伤害，对冻结单位伤害x4\n" +
                "②本行前方的卷心菜保护伞或绿宝石伞，可以承接本应向其右侧范围投掷的爆破卷心菜并弹射</color>\n" +
                "<color=#3D1400>词条1：</color><color=red>兵贵神速：攻击速度+100%</color>\n" +
                "<color=#3D1400>词条2：</color><color=red>中心爆破：子弹和炮弹的伤害x4</color>\n\n" +
                "<color=#3D1400>“我一直都在尝试接近那个临界点，绝对零度。”究极冷寂迫击炮一生都在研究如何让温度降到最低，他尝试过多种方法却总是失败，“似乎有什么力量在阻止我接近它，它就像一株荷花静静的矗立在那里，只可远观不可亵玩。”究极冷寂迫击炮知道，自己越接近它，也会越害怕，“我不知道靠近它甚至是融合它会有怎样的结果，但是我愿意承担这个结果，我不会放弃，也不会马虎。”</color>"
            );
            CustomCore.AddFusion((int)PlantType.UltimateCannon, (int)UltimateWinterCabbagecannon.PlantID, (int)PlantType.Cornpult);
            CustomCore.RegisterCustomCherry(UltimateWinterCabbagecannon.BombID, ab.GetAsset<GameObject>("IceDoomCabbageBomb"));
            CustomCore.TypeMgrExtra.IsIcePlant.Add(UltimateWinterCabbagecannon.PlantID);
            CustomCore.TypeMgrExtra.DoubleBoxPlants.Add(UltimateWinterCabbagecannon.PlantID);
            CustomCore.AddUltimatePlant(UltimateWinterCabbagecannon.PlantID);
            CustomCore.RegisterCustomOnMixEvent(UltimateWinterCabbagecannon.PlantID, PlantType.Cornpult, (p) =>
            {
                p.GetComponent<UltimateCannon>().firstLoad = false;
            });
            UltimateWinterCabbagecannon.sub = ab.GetAsset<GameObject>("Bullet_subIceDoomCabbage");
            UltimateWinterCabbagecannon.sub.AddComponent<SubCabbage>();
        }
    }

    public class UltimateWinterCabbagecannon : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1956;
        public static BulletType BulletID = (BulletType)1956;
        public static CherryBombType BombID = (CherryBombType)1956;
        public static GameObject? sub = null;

        public CabbageCannon plant => gameObject.GetComponent<CabbageCannon>();
        public float originSpeed = 1f;

        public void Awake()
        {
            plant.shoot = transform.FindChild("Cabbagepult_cabbage _2");
        }

        public void Update()
        {
            if (plant != null && GameAPP.theGameStatus == GameStatus.InGame)
            {
                if (Lawnf.TravelUltimate((UltiBuff)15))
                    plant.thePlantAttackCountDown -= Time.deltaTime;
            }
        }

        public void AnimSuper_IceDoomCabbagecannon()
        {
            var zombies = Lawnf.GetAllZombies().ToSystemList().Where(z => z != null && !z.isMindControlled).ToList();
            for (int i = 1; i <= 20; i++)
            {
                if (sub == null) continue;
                var cabbage = Instantiate(sub, Vector3.zero, Quaternion.identity, plant.board.transform).GetComponent<SubCabbage>();
                if (cabbage != null)
                {
                    int damage = plant.attackDamage;
                    cabbage.board = plant.board;
                    cabbage.plantType = plant.thePlantType;
                    if (i % 10 == 0)
                        cabbage.iceDoom = true;
                }
            }
        }

        public void SuperStart()
        {
            plant.attributeCountdown = 0.666f;
        }

        public void ShootStart()
        {
            originSpeed = plant.anim.speed;
            if (Lawnf.TravelUltimate((UltiBuff)15))
            {
                plant.anim.speed = originSpeed * 2;
            }
        }

        public void ShootEnd()
        {
            plant.anim.speed = originSpeed;
        }
    }

    public class SubCabbage : MonoBehaviour
    {
        public int damage = 300;
        public Board? board = null;
        public Vector3 targetPosition;
        public PlantType plantType = UltimateWinterCabbagecannon.PlantID;
        public bool iceDoom = false;
        public float speed = 10f;
        public int row = 0;

        public void Start()
        {
            if (board == null) Destroy(gameObject);

            if (Lawnf.TravelUltimate((UltiBuff)14))
                damage *= 4;
            SetPosition();
        }

        public void SetPosition()
        {
            var list = Lawnf.GetAllZombies().ToSystemList().Where(z => z != null && !z.isMindControlled).ToList();
            Vector3 position;
            if (list.Count > 0)
            {
                var zombie = list[UnityEngine.Random.Range(0, list.Count)];
                position = zombie.axis.transform.position;
                targetPosition = zombie.axis.transform.position;
            }
            else
            {
                targetPosition = new Vector3(UnityEngine.Random.Range(board.boardMinX, board.boardMaxX), UnityEngine.Random.Range(board.boardMinY, board.boardMaxY));
                position = new Vector3(UnityEngine.Random.Range(board.boardMinX, board.boardMaxX) + UnityEngine.Random.Range(-1.1f, 1.1f), board.boardMaxY + UnityEngine.Random.Range(1.8f, 2.7f));
            }
            transform.position = position + new Vector3(0f, UnityEngine.Random.Range(5.2f, 6.7f), 0f) + new Vector3(UnityEngine.Random.Range(-3.0f, 3.0f), UnityEngine.Random.Range(0.0f, 6.0f));
            row = Mouse.Instance.GetRowFromY(position.x, position.y);
        }

        public void Update()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;

            if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                Vector2 direction = targetPosition - transform.position;
                if (direction.magnitude > 0.1f)
                {
                    // 计算需要旋转的角度（使右侧指向目标）
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    // 应用旋转（只绕Z轴旋转，适合2D游戏）
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                Die();
        }

        public void Die()
        {
            if (iceDoom)
            {
                board?.boardAction.SetDoom(0, 0, false, true, transform.position, 1800, fromType: plantType);
            }
            else
            {
                Action<Zombie> action = (z) =>
                {
                    z.SetFreeze(5f);
                    z.SetCold(10f);
                    z.AddfreezeLevel(25);
                    if (z.GetAttrTimers().freezeTimer > 0f)
                        z.TakeDamage(DmgType.Carred, damage * 4, plantType);
                    else
                        z.TakeDamage(DmgType.Carred, damage * 8, plantType);
                };
                var cherry = board?.boardAction.CreateCherryExplode(transform.position, row, CherryBombType.IceCharry, fromType: plantType, action: action, immediately: false);
                if (cherry == null)
                {
                    Destroy(gameObject);
                    return;
                };
                cherry.range = 1.5f;
                cherry.fromType = plantType;
                cherry.damageToZombie = damage;
                cherry.maxRow = 1;
                cherry.Explode();
            }
            Destroy(gameObject);
        }
    }

    [HarmonyPatch(typeof(CabbageCannon))]
    public static class CabbageCannonPatch
    {
        [HarmonyPatch(nameof(CabbageCannon.Shoot1))]
        [HarmonyPrefix]
        public static bool PreShoot1(CabbageCannon __instance, ref Bullet __result)
        {
            try
            {
                if (__instance.thePlantType != UltimateWinterCabbagecannon.PlantID)
                    return true;

                var zombies = new List<Zombie>();

                foreach (Zombie zombie in __instance.board.zombieArray)
                {
                    
                    if (Math.Abs(zombie.theZombieRow - __instance.thePlantRow) <= 1)
                    {
                        if (Thrower.ThrowSearchZombie(zombie) && zombie.axis.position.x > __instance.axis.position.x)
                            zombies.Add(zombie);
                    }
                }

                var umbrella = __instance.FindUmbrella(__instance.shoot.position);

                float flightTime = __instance.flightTime;
                int damage = __instance.attackDamage;
                var plantType = __instance.thePlantType;

                foreach (var zombie in zombies)
                {
                    int row = zombie.theZombieRow;
                    if (umbrella != null && zombie.axis.position.x > __instance.axis.position.x)
                        row = umbrella.thePlantRow;
                    var bullet = CreateBullet.Instance.SetBullet(__instance.shoot.position.x, __instance.shoot.position.y, row,
                        UltimateWinterCabbagecannon.BulletID, BulletMoveWay.Throw);

                    if (umbrella != null && zombie.axis.position.x > __instance.axis.position.x)
                        bullet.ThrowTo(umbrella, new(), __instance.flightTime.GetNullable());
                    else
                        bullet.ThrowTo(zombie, new(), flightTime.GetNullable());
                    bullet.targetZombie = zombie;

                    bullet.Damage = damage;
                    bullet.fromType = plantType;
                }
                if (UnityEngine.Random.Range(0, 100) < 5 && __instance.attributeCountdown <= 0f)
                    __instance.anim.SetTrigger("super");
                GameAPP.PlaySound(145, 0.5f, UnityEngine.Random.Range(0.9f, 1.2f));

                return false;
            }
            catch
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_cabbage))]
    public static class Bullet_cabbagePatch
    {
        [HarmonyPatch(nameof(Bullet_cabbage.HitZombie))]
        [HarmonyPrefix]
        public static bool PreZombie(Bullet_cabbage __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == UltimateWinterCabbagecannon.BulletID)
            {
                if (zombie != null)
                {
                    var cherry = CoreTools.CreateCherryExplode(__instance.transform.position, __instance.theBulletRow, UltimateWinterCabbagecannon.BombID, immediately: false, shake: false, volume: 0.2f).Item1;
                    int damage = __instance.Damage;
                    if (Lawnf.TravelUltimate((UltiBuff)14))
                        damage *= 4;
                    cherry.damageToZombie = damage;
                    cherry.bombRow = __instance.theBulletRow;
                    cherry.bombType = CherryBombType.Custom;
                    cherry.range = 1f;
                    cherry.fromType = __instance.fromType;
                    cherry.maxRow = 1;
                    Action<Zombie> action = (z) =>
                    {
                        z.SetCold(10f);
                        z.AddfreezeLevel(25);
                        if (z.GetAttrTimers().freezeTimer > 0f)
                        {
                            z.TakeDamage(DmgType.NormalAll, damage * 4, __instance.fromType);
                        }
                        else
                        {
                            z.TakeDamage(DmgType.NormalAll, damage, __instance.fromType);
                        }
                    };
                    cherry.zombieAction = action;
                    cherry.Explode();
                    __instance.Die();
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Bullet_cabbage.HitLand))]
        [HarmonyPrefix]
        public static bool PreLand(Bullet_cabbage __instance)
        {
            if (__instance.theBulletType == UltimateWinterCabbagecannon.BulletID)
            {
                var cherry = CoreTools.CreateCherryExplode(__instance.transform.position, __instance.theBulletRow, UltimateWinterCabbagecannon.BombID, immediately: false, shake: false, volume: 0.2f).Item1;
                int damage = __instance.Damage;
                if (Lawnf.TravelUltimate((UltiBuff)14))
                    damage *= 4;
                cherry.damageToZombie = damage;
                cherry.bombRow = __instance.theBulletRow;
                cherry.bombType = CherryBombType.IceCharry;
                cherry.range = 1f;
                cherry.fromType = __instance.fromType;
                cherry.maxRow = 1;
                Action<Zombie> action = (z) =>
                {
                    z.SetCold(10f);
                    z.AddfreezeLevel(25);
                    if (z.GetAttrTimers().freezeTimer > 0f)
                        z.TakeDamage(DmgType.NormalAll, damage * 4, __instance.fromType);
                    else
                        z.TakeDamage(DmgType.NormalAll, damage, __instance.fromType);
                };
                cherry.zombieAction = action;
                cherry.Explode();
                __instance.Die();
                return false;
            }
            return true;
        }
    }
}
