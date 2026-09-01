using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;
using Unity.VisualScripting;
using System.Collections;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using UnityEngine.Rendering;

namespace ElectricSuperGatling_BepInEx
{
    [BepInPlugin("salmon.electricsupergatling", "ElectricSuperGatling", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_electricSuperGatlingPea>();
            ClassInjector.RegisterTypeInIl2Cpp<ElectricSuperGatling>();
            ClassInjector.RegisterTypeInIl2Cpp<ElectricLine>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "electricsupergatling");
            CustomCore.RegisterCustomBullet<Bullet_pea, Bullet_electricSuperGatlingPea>((BulletType)Bullet_electricSuperGatlingPea.BulletID, 
                ab.GetAsset<GameObject>("ElectricPea"));
            CustomCore.RegisterCustomPlant<SuperGatling, ElectricSuperGatling>(
                ElectricSuperGatling.PlantID,
                ab.GetAsset<GameObject>("ElectricSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("ElectricSuperGatlingPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.ElectricOnion)
                },
                1.5f, 0f, 30, 300, 7.5f, 825
            );
            CustomCore.RegisterCustomPlantSkin<SuperGatling, ElectricSuperGatling>(
                ElectricSuperGatling.PlantID,
                ab.GetAsset<GameObject>("ElectricSuperGatlingSkinPrefab"),
                ab.GetAsset<GameObject>("ElectricSuperGatlingSkinPreview"),
                new()
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.ElectricOnion)
                },
                1.5f, 0f, 30, 300, 7.5f, 825, new List<(BulletType, List<GameObject?>)>()
                {
                    (Bullet_electricSuperGatlingPea.BulletID, new() { ab.GetAsset<GameObject>("ElectricPeaSkin") })
                }
            );
            CustomCore.RegisterCustomPlantSkin<SuperGatling, ElectricSuperGatling>(
                ElectricSuperGatling.PlantID,
                ab.GetAsset<GameObject>("ElectricSuperGatlingSkin2Prefab"),
                ab.GetAsset<GameObject>("ElectricSuperGatlingSkin2Preview"),
                new()
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.ElectricOnion)
                },
                1.5f, 0f, 30, 300, 7.5f, 825, new List<(BulletType, List<GameObject?>)>()
                {
                    (Bullet_electricSuperGatlingPea.BulletID, new() { ab.GetAsset<GameObject>("ElectricPeaSkin2") })
                }
            );
            CustomCore.AddUltimatePlant((PlantType)ElectricSuperGatling.PlantID);
            CustomCore.AddPlantAlmanacStrings(ElectricSuperGatling.PlantID,
                $"电能超级机枪射手({ElectricSuperGatling.PlantID})",
                "一次发射六颗电能豌豆，有概率一次性发射大量电能豌豆\n\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin、@白鱼余余丶</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>30x6/1.5秒</color>\n" +
                "<color=#3D1400>子弹伤害：</color><color=red>30/0.15秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>每次攻击有2%概率出发大招，5秒内，每0.02秒散射3发电能豌豆</color>\n" +
                "<color=#3D1400>电能豌豆：</color><color=red><color=#3D1400>①</color>无限穿透，子弹会向3x3范围持续造成伤害\n" +
                "<color=#3D1400>②</color>子弹前三次直击目标时，索敌半径3.7格的非直击目标释放一次闪电链，造成一次半径1.5格，伤害为10倍攻击力的灰烬伤害\n" +
                "<color=#3D1400>③</color>受到伤害的目标有1%概率陷入0.5秒的定身效果</color>\n" +
                "<color=#3D1400>词条1:</color><color=red>五阶升级：电能超级机枪射手的攻击力x10，子弹的攻击频率x3，闪电链击中目标时会减少1点护甲系数</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>超级机枪射手+闪电洋葱</color>\n\n" +
                "<color=#3D1400>电能超级机枪射手以强大的实力在植物界声名远扬，有他在的地方，观众们都会呼喊“▇▇▇▇▇”，这段话成为了植物们对他的第一印象。不过在来到了这片土地后，电能超级机枪射手受到了奇怪的影响…</color>"
            );
            Bullet_electricSuperGatlingPea.ElectricLine = ab.GetAsset<GameObject>("PrismLine");
            Bullet_electricSuperGatlingPea.ElectricLineSkin = ab.GetAsset<GameObject>("PrismLineSkin");
            CustomCore.RegisterCustomParticle(Bullet_electricSuperGatlingPea.OnHitParticle, 
                ab.GetAsset<GameObject>("Bullet  ElectricSplatyellow"));
            CustomCore.RegisterCustomParticle(Bullet_electricSuperGatlingPea.OnHitParticleSkin, 
                ab.GetAsset<GameObject>("Bullet ElectricSplatblue"));
            CustomCore.RegisterCustomParticle(Bullet_electricSuperGatlingPea.LineHit, ab.GetAsset<GameObject>("ElectricSplat"));
            CustomCore.RegisterCustomParticle(Bullet_electricSuperGatlingPea.LineHitSkin, ab.GetAsset<GameObject>("ElectricSplatSkin"));
        }
    }

    public class Bullet_electricSuperGatlingPea : MonoBehaviour
    {
        public static GameObject ElectricLine = null!;
        public static GameObject ElectricLineSkin = null!;
        public static ID OnHitParticle = 1906;
        public static ID OnHitParticleSkin = 1907;
        public static ID LineHit = 1908;
        public static ID LineHitSkin = 1909;
        public static ID BulletID = 1906;
        public static List<Zombie> ZombiesInTimer = new();
        public static bool buff => CoreTools.TravelAdvanced("五阶升级"); // 可以省掉读buff的消耗

        public float attackCountDown = 0f;
        public int hitTimes = 0;
        public GameObject line = null!;
        public bool skin => bullet.gameObject.name.StartsWith("ElectricPeaSkin");

        public void Start()
        {
            // 设置闪电链gameobject
            if (skin) line = ElectricLineSkin;
            else line = ElectricLine;
            hitTimes = 0;
        }

        public void OnHitZombie(Zombie z)
        {
            hitTimes++;
            Destroy(CreateParticle.SetParticle(skin ? OnHitParticleSkin : OnHitParticle, z.axis.position + new Vector3(0f, 0.8f), z.theZombieRow), 1f);
            GameAPP.PlaySound(SoundType.Laser, 0.25f, 1f);
            if (buff) z.theArmor -= 1;
            if (hitTimes <= 3) // 如果是前3次直击
            {
                var zombiesInRange = Physics2D.OverlapCircleAll(bullet.gameObject.transform.position, CoreTools.ColumnX * 3.7f, GameInfo.zombieLayer).
                    Where(col => col.IsObjExist() && col.TryGetComponent<Zombie>(out var zombie) && zombie.IsObjExist() && zombie != z). // 找到所有存在zombie组件的碰撞体
                    Select(zombie => zombie.GetComponent<Zombie>()).ToList(); // 已经把空判断做了，不用再做一次了
                if (zombiesInRange.Count <= 0)
                    return;
                zombiesInRange.Remove(z); // 移除直击的僵尸
                var target = zombiesInRange.GetRandomItem(); // 获取随机的一个僵尸
                if (buff) target.theArmor -= 1;
                var row = Mathf.Max(bullet.theBulletRow, target.theZombieRow);
                var end = target.axis.position + new Vector3(0f, 0.8f);
                Destroy(CreateParticle.SetParticle(skin ? LineHitSkin : LineHit, end, row), 1f);
                CreateLine(z.axis.position + new Vector3(0f, 0.8f), end, row);
                foreach (var col in Physics2D.OverlapCircleAll(target.axis.position, CoreTools.ColumnX * 1.5f, GameInfo.zombieLayer))
                {
                    // 判空
                    if (!col.IsObjExist()) continue;
                    if (!col.TryGetComponent<Zombie>(out var zombie)) continue;
                    if (!zombie.IsObjExist()) continue;
                    zombie.TakeDamage(bullet.Damage * 10, bullet, DamageType.Carred, bullet.fromType);
                    zombie.StartCoroutine(SetTimer(zombie));
                }
            }
        }

        public void FixedUpdate()
        {
            if (GameAPP.theGameStatus != GameStatus.InGame) return;
            attackCountDown -= Time.deltaTime * (buff ? 3 : 1);
            if (attackCountDown <= 0f)
            {
                var columnX = CoreTools.ColumnX;
                foreach (var col in Physics2D.OverlapBoxAll(bullet.gameObject.transform.position, new(columnX * 3f, columnX * 3f), 0f, GameInfo.zombieLayer))
                {
                    // 对象判空
                    if (!col.IsObjExist()) continue;
                    if (!col.TryGetComponent<Zombie>(out var zombie)) continue;
                    if (!zombie.IsObjExist()) continue;

                    zombie.TakeDamage(bullet.Damage, bullet, DamageType.Carred, bullet.fromType);
                    zombie.StartCoroutine(SetTimer(zombie));
                }
                attackCountDown = 0.15f;
            }
        }

        public void OnEnable()
        {
            hitTimes = 0;
        }

        public GameObject CreateLine(Vector2 start, Vector2 end, int row)
        {
            var newLine = Instantiate(line, bullet.board.transform);
            var renderer = newLine.transform.GetChild(0).GetComponent<LineRenderer>();
            // 添加配置颜色组件
            renderer.gameObject.AddComponent<ElectricLine>();
            // 设置起点终点
            renderer.SetPosition(0, start);
            renderer.SetPosition(1, end);
            renderer.startWidth += 0.25f;
            renderer.endWidth += 0.25f;
            newLine.AddComponent<SortingGroup>().sortingLayerName = $"particle{row}";
            Destroy(newLine, 0.5f);
            return newLine;
        }

        public static IEnumerator SetTimer(Zombie zombie)
        {
            if (UnityEngine.Random.Range(1, 101) != 1) yield break;
            if (!zombie.IsObjExist()) yield break;
            if (zombie.timers.TryGetValue((ZombieTimer)(int)BulletID, out var time) && time > 0f) yield break;
            // 如果是二爷掉报纸或者阿尔法假死
            if (zombie.theStatus == ZombieStatus.Paper_losePaper || zombie.theStatus == ZombieStatus.Legion_fall) yield break;
            ZombiesInTimer = ZombiesInTimer.Where(z => z.IsObjExist()).ToList(); // 去除Null元素
            if (ZombiesInTimer.Contains(zombie)) yield break;
            ZombiesInTimer.Add(zombie);
            zombie.timers[(ZombieTimer)(int)BulletID] = 0.5f;
            var origin = 0f; // 实际要设置的值，在交换后就变成了原来的速度
            (origin, zombie.theOriginSpeed) = (zombie.theOriginSpeed, origin);
            yield return new WaitForSeconds(0.5f);
            if (!zombie.IsObjExist()) yield break;
            (origin, zombie.theOriginSpeed) = (zombie.theOriginSpeed, origin);
            zombie.timers[(ZombieTimer)(int)BulletID] = 0f;
            ZombiesInTimer.Remove(zombie);
            yield break;
        }
        public Bullet_pea bullet => gameObject.GetComponent<Bullet_pea>();
    }

    public class ElectricLine : MonoBehaviour
    {
        private float live = 0f;
        private Color startColor = new();
        private LineRenderer renderer;

        public void Awake()
        {
            renderer = gameObject.GetComponent<LineRenderer>();
            startColor = renderer.startColor;
        }

        public void Update()
        {
            live += Time.deltaTime;
            if (live > 0.2f)
            {
                Color color = startColor;
                color.a -= Time.deltaTime * 5f;
                renderer.startColor = color;
                renderer.endColor = color;
                startColor = color;
            }
        }
    }

    public class ElectricSuperGatling : MonoBehaviour
    {
        public static int PlantID = 1906;

        public SuperGatling plant => gameObject.GetComponent<SuperGatling>();

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.FindChild("GatlingPea_head/Shoot");
        }
    }

    [HarmonyPatch(typeof(SuperGatling), nameof(SuperGatling.GetBulletType))]
    public class SuperGatling_GetBulletType
    {
        public static bool Prefix(SuperGatling __instance, ref BulletType __result)
        {
            if ((int)__instance.thePlantType == ElectricSuperGatling.PlantID)
            {
                __result = (BulletType)Bullet_electricSuperGatlingPea.BulletID;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_pea), nameof(Bullet_pea.HitZombie))]
    public static class Bullet_peaPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Bullet_pea __instance, ref Zombie zombie)
        {
            if ((int)__instance.theBulletType == Bullet_electricSuperGatlingPea.BulletID)
            {
                __instance.GetComponent<Bullet_electricSuperGatlingPea>().OnHitZombie(zombie);
                __instance.hitCount = int.MinValue; // 重置是否击中，不然后续都无法判定直击
                return false;
            }
            return true;
        }
    }
}