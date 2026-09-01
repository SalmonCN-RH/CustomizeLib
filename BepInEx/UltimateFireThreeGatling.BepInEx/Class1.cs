using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.Extra.Attributes;
using CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace UltimateFireThreeGatling.BepInEx
{
    [BepInPlugin("salmon.ultimatefiresupergatling", "UltimateFireThreeGatling", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<UltimateFireThreeGatling>();
            ClassInjector.RegisterTypeInIl2Cpp<AshThreeGatling>();
            ClassInjector.RegisterTypeInIl2Cpp<UltimateFireThreeGatling_sp>();

            #region 浴火三大哥
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "ultimatefiresupergatling");
            CustomCore.RegisterCustomPlant<SuperThreeGatling, UltimateFireThreeGatling>(UltimateFireThreeGatling.PlantID,
                ab.GetAsset<GameObject>("UltimateFireThreeGatlingPrefab"),
                ab.GetAsset<GameObject>("UltimateFireThreeGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.SuperThreePeater),
                    (1901, (int)PlantType.AshThreePeater),
                    (AshThreeGatling.PlantID, (int)PlantType.Jalapeno)
                }, 1.5f, 0f, 160, 300, 0f, 1250);
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)UltimateFireThreeGatling.PlantID);
            CustomCore.AddPlantAlmanacStrings(UltimateFireThreeGatling.PlantID, $"究极浴火机枪射手",
                "介绍。\n\n" +
                "<color=#3D1400>融合配方：</color><color=red>浴火三线射手+超级机枪射手</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>（160x6）x3/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>普通攻击时，每发子弹有2%概率触发大招：回复1倍韧性血量，5秒内免疫伤害和碾压，每0.02秒向三行各发射1个3倍伤害的浴火豌豆</color>\n\n" +
                "<color=#3D1400>“我们来自未来，也来自过去，并存在于现在”浴火三线超级机枪射手的三个脑袋分别代表过去现在和未来，过去放眼未来，现在不被过去约束，未来不再重蹈覆辙，浴火三线超级机枪射手穿越到过去和未来，“我们找遍了所有的未来，只有一个未来是和平的，温暖的，就是我们正在进行，将要到达的未来”，他们异口同声，“就算是没有我们的未来，也要保持一颗热忱的心，就像火焰一样，温暖”</color>");
            CustomCore.TypeMgrExtra.LevelPlants.Add((PlantType)UltimateFireThreeGatling.PlantID, CardLevel.Red);
            CustomCore.AddUltimatePlant((PlantType)UltimateFireThreeGatling.PlantID);
            #endregion

            #region sp浴火三大哥
            var ab_sp = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "ultimatefiresupergatling_sp");
            CustomCore.RegisterCustomPlant<SuperThreeGatling, UltimateFireThreeGatling_sp>(UltimateFireThreeGatling_sp.PlantID,
                ab_sp.GetAsset<GameObject>("UltimateFireThreeGatling_spPrefab"),
                ab_sp.GetAsset<GameObject>("UltimateFireThreeGatling_spPreview"), new List<(PlantType, PlantType)>
                {
                    (PlantType.SuperGatling, PlantType.SuperThreePeater_sp),
                    ((PlantType)1901, PlantType.SuperThreePeater),
                    (UltimateFireThreeGatling.PlantID, PlantType.Jalapeno)
                }.ToIntegerList(), 1.5f, 0f, 180, 300, 7.5f, 0);
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)UltimateFireThreeGatling_sp.PlantID);
            CustomCore.AddPlantAlmanacStrings(UltimateFireThreeGatling_sp.PlantID, $"究极浴火机枪射手",
                "介绍。\n" +
                "<color=#0000FF>究极浴火机枪射手的进阶形态</color>\n\n" +
                "<color=#3D1400>融合配方：</color><color=red>究极浴火机枪射手+火爆辣椒</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>（180x6）/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=#3D1400>①</color><color=red>每轮向全场每行发射六发浴火豌豆，可穿透2次，伤害后施加红温状态</color>\n" +
                "<color=#3D1400>②</color><color=red>普通攻击时，每发子弹有2%概率触发大招：回复1倍韧性血量，5秒内免疫伤害和碾压，每0.02秒向全场每行各发射1个3倍伤害的浴火豌豆</color>\n" +
                "<color=#3D1400>③</color><color=red>出场或死亡时，在全场每行：释放火爆辣椒效果6次并发射浴火豌豆6发</color>\n\n" +
                "<color=#3D1400>请输入文本</color>");
            CustomCore.TypeMgrExtra.LevelPlants.Add((PlantType)UltimateFireThreeGatling_sp.PlantID, CardLevel.Red);
            CustomCore.AddUltimatePlant((PlantType)UltimateFireThreeGatling_sp.PlantID);
            #endregion

            #region 灰三大哥
            var ab_ash = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "ashthreegaling");
            CustomCore.RegisterCustomPlant<Plant, AshThreeGatling>(AshThreeGatling.PlantID,
                ab_ash.GetAsset<GameObject>("AshThreeGatlingPrefab"),
                ab_ash.GetAsset<GameObject>("AshThreeGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.AshThreePeater),
                    (1901, (int)PlantType.DarkThreePeater),
                    (1921, (int)PlantType.Jalapeno)
                }, 0f, 0f, 0, 300, 0f, 1125);
            CustomCore.AddPlantAlmanacStrings(AshThreeGatling.PlantID, $"灰烬超级机枪射手",
                "被彻底烧焦的三线超级机枪射手，无法攻击。\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>三线超级机枪射手（底座）+火爆辣椒+火爆辣椒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>失去攻击能力</color>\n\n" +
                "<color=#3D1400>“我们曾无限接近死亡，生命无法一直保持濒死状态，他们只有生或死两个状态，”灰烬三线超级机枪射手长舒一口气，对着面前的植物说道。“与其说我们是灰烬，不如说灰烬创造了我们，我们是灰烬的孩子，就像大家喜欢阳光一样，我们喜欢如同灰烬一般的环境。”灰烬三线超级机枪射手知道他们体内又多大的能量，他们必须要学会控制并抑制这股力量，“我们不想让世界变成我们梦中那样，就像你们看到的那样，我们不具备任何攻击能力。”他们一直保持濒死的状态，自己身体剩余的能量只能拿来修复一直损坏的细胞，“我们喜欢这个世界。”他们在说完这句话之后，就沉沉的睡去了</color>");
            CustomCore.TypeMgrExtra.LevelPlants.Add((PlantType)AshThreeGatling.PlantID, CardLevel.Purple);
            CustomCore.AddUltimatePlant((PlantType)AshThreeGatling.PlantID);
            #endregion
        }
    }

    public class UltimateFireThreeGatling : MonoBehaviour
    {
        public static ID BulletID = BulletType.Bullet_firePea_super;
        public static ID PlantID = 1923;

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("headPos2/ThreePeater_head2/ThreePeater_mouth/Shoot");
            if (Board.Instance != null && GameAPP.theGameStatus == GameStatus.InGame)
                for (int i = 0; i < Board.Instance.rowNum; i++)
                    Board.Instance.boardAction.CreateFireLine(i, 1800, false, false, true, null);
        }

        public SuperThreeGatling plant => gameObject.GetComponent<SuperThreeGatling>();
    }

    public class UltimateFireThreeGatling_sp : MonoBehaviour, IAsyncPlantEvent
    {
        public static ID BulletID = BulletType.Bullet_firePea_super;
        public static ID PlantID = 1941;
        public static OnClickData OnClickData => new(true, false);

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("headPos2/ThreePeater_head2/ThreePeater_mouth/Shoot");
            this.AddToList();
        }

        public void Start()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame || Time.timeScale <= 0f) return;
            _ = StartEvent();
        }

        [TriggerOnce]
        public async Task DieEvent(Plant.DieReason reason, TriggerType type)
        {
            float x = plant.axis.position.x;
            int damage = plant.attackDamage;
            var plantType = plant.thePlantType;
            var board = plant.board;
            foreach (var _ in Enumerable.Range(0, 6))
            {
                for (int i = 0; i < board.rowNum; i++)
                {
                    board.boardAction.CreateFireLine(i, damage * 10, fromType: plantType);
                    var bullet = CreateBullet.Instance.SetBullet(x, Mouse.Instance.GetLandY(x, i), i, BulletID, BulletMoveWay.MoveRight_threePeater);
                    bullet.Damage = damage;
                    bullet.fromType = plantType;
                }
                await UniTask.Delay(167, cancellationToken: board.GetCancellationTokenOnDestroy());
            }
        }

        public async Task StartEvent()
        {
            plant.anim.SetTrigger("shoot2");
            foreach (var _ in Enumerable.Range(0, 6))
            {
                for (int i = 0; i < plant.board.rowNum; i++)
                    plant.board.boardAction.CreateFireLine(i, plant.attackDamage * 10, fromType: PlantID);
                await UniTask.Delay(167, cancellationToken: plant.board.GetCancellationTokenOnDestroy());
            }
        }

        // [TriggerOnce]
        async Task IAsyncPlantEvent.OnUpdate(TriggerType trigger)
        {
            Console.WriteLine($"on update, {trigger}");
        }

        [TriggerOnce]
        async Task IAsyncPlantEvent.OnFixedUpdate(TriggerType trigger)
        {
            Console.WriteLine($"on fixedupdate");
        }

        public void SpShoot()
        {
            for (int i = 0; i < plant.board.rowNum; i++)
            {
                var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, i, BulletID, BulletMoveWay.MoveRight_threePeater);
                bullet.Damage = plant.attackDamage;
                bullet.fromType = plant.thePlantType;
            }
        }

        public void SpSuperShoot()
        {
            var shoot = plant.shoot.position;
            for (int i = 0; i < plant.board.rowNum; i++)
            {
                var bullet = CreateBullet.Instance.SetBullet(shoot.x + UnityEngine.Random.Range(-0.1f, 0.1f), shoot.y + UnityEngine.Random.Range(-0.2f, 0.2f),
                    i, BulletID, BulletMoveWay.MoveRight_threePeater);
                bullet.Damage = plant.attackDamage * 3;
                bullet.fromType = plant.thePlantType;
                bullet.normalSpeed = UnityEngine.Random.Range(12f, 14f);
            }

            GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);

            if (plant.timer <= 0f && !plant.keepShooting)
            {
                plant.AttributeCountdown = 0f;
                plant.anim.SetBool("shooting", false);
            }
            else
            {
                plant.AttributeCountdown = 0.02f;
            }
        }

        public GameObject SearchZombie()
        {
            foreach (var zombie in Lawnf.GetAllZombies())
            {
                if (!zombie.IsObjExist()) continue;
                if (zombie.axis.position.x <= plant.axis.position.x) continue;
                if (!plant.SearchUniqueZombie(zombie)) continue;
                return zombie.gameObject;
            }
            return null!;
        }

        public SuperThreeGatling plant => gameObject.GetComponent<SuperThreeGatling>();
    }

    public class AshThreeGatling : MonoBehaviour
    {
        public static ID PlantID = 1924;

        public Plant plant => gameObject.GetComponent<Plant>();
    }

    #region 浴火三大哥shoot
    [HarmonyPatch(typeof(ThreePeater))]
    public static class ThreePeaterPatch
    {
        [HarmonyPatch(nameof(ThreePeater.Shoot1))]
        [HarmonyPrefix]
        public static bool Prefix(ThreePeater __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
            {
                if (__instance.shoot == null) return false;

                var bullet = CreateBullet.Instance.SetBullet(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow,
                    UltimateFireThreeGatling.BulletID, BulletMoveWay.MoveRight);

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;

                GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1f);

                // 三线射手特殊逻辑：根据所在行发射额外子弹
                if (__instance.thePlantRow == 0)
                    __instance.ShootLower(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow + 1);
                else if (__instance.thePlantRow == __instance.board.rowNum - 1)
                    __instance.ShootUpper(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow - 1);
                else
                {
                    __instance.ShootLower(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow + 1);
                    __instance.ShootUpper(__instance.shoot.position.x, __instance.shoot.position.y, __instance.thePlantRow - 1);
                    return false;
                }

                __instance.Invoke("ExtraBullet", 0.2f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ThreePeater.ExtraBullet))]
        [HarmonyPrefix]
        public static bool PreExtraBullet(ThreePeater __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
            {
                var bullet = CreateBullet.Instance.SetBullet(__instance.shoot.position.x, __instance.shoot.position.y,
                    __instance.thePlantRow, UltimateFireThreeGatling.BulletID, BulletMoveWay.MoveRight);

                if (bullet == null) return false;

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ThreePeater.ShootLower))]
        [HarmonyPrefix]
        public static bool PreShootLower(ThreePeater __instance, float X, float Y, int row)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
            {
                var bullet = CreateBullet.Instance.SetBullet(X, Y, row, UltimateFireThreeGatling.BulletID, BulletMoveWay.MoveRight_threePeater);

                if (bullet == null) return false;

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ThreePeater.ShootUpper))]
        [HarmonyPrefix]
        public static bool PreShootUpper(ThreePeater __instance, float X, float Y, int row)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
            {
                var bullet = CreateBullet.Instance.SetBullet(X, Y, row, UltimateFireThreeGatling.BulletID, BulletMoveWay.MoveRight_threePeater);

                if (bullet == null) return false;

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;
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
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
            {
                var bullet = CreateBullet.Instance.SetBullet(x, y, row, UltimateFireThreeGatling.BulletID, bulletMoveWay, false);
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
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
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
    #endregion

    #region sp浴火三大哥shoot
    [HarmonyPatch(typeof(ThreePeater))]
    public static class ThreePeaterPatch_sp
    {
        [HarmonyPatch(nameof(ThreePeater.Shoot1))]
        [HarmonyPrefix]
        public static bool Prefix(ThreePeater __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling_sp.PlantID)
            {
                if (__instance.shoot == null) return false;
                __instance.GetComponent<UltimateFireThreeGatling_sp>().SpShoot();
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ThreePeater.SearchZombie))]
        [HarmonyPrefix]
        public static bool PreSearchZombie(ThreePeater __instance, ref GameObject __result)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling_sp.PlantID)
            {
                if (__instance.shoot == null) return false;
                __result = __instance.GetComponent<UltimateFireThreeGatling_sp>().SearchZombie();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SuperThreeGatling))]
    public class SuperThreeGatling_SuperShoot_sp
    {
        [HarmonyPatch(nameof(SuperThreeGatling.AttributeEvent))]
        [HarmonyPrefix]
        public static bool PreAttributeEvent(SuperThreeGatling __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling_sp.PlantID)
            {
                __instance.GetComponent<UltimateFireThreeGatling_sp>().SpSuperShoot();
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(SuperThreeGatling.Update))]
        [HarmonyPrefix]
        public static void Prefix_Update(SuperThreeGatling __instance, out bool __state)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling_sp.PlantID)
            {
                if (__instance.timer > 0 && __instance.timer - Time.deltaTime * __instance.attributeSpeed <= 0f)
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
    #endregion

    [HarmonyPatch(typeof(Plant), nameof(Plant.Die))]
    public class Plant_Die
    {
        [HarmonyPostfix]
        public static void Prefix(Plant __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateFireThreeGatling.PlantID)
                if (Board.Instance != null && GameAPP.theGameStatus == GameStatus.InGame)
                    for (int i = 0; i < Board.Instance.rowNum; i++)
                        Board.Instance.boardAction.CreateFireLine(i, 1800);
        }
    }
}