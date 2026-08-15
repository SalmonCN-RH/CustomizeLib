using BepInEx;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Core;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimatePortalSpring.BepInEx
{
    [BepInPlugin("salmon.ultimateportalspring", "UltimatePortalSpring", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "ultimateportalspring");
            CustomCore.RegisterCustomPlant<UltimateSpring, UltimatePortalSpring>(UltimatePortalSpring.PlantID, ab.GetAsset<GameObject>("UltimatePortalSpringPrefab"),
                ab.GetAsset<GameObject>("UltimatePortalSpringPreview"), new List<(PlantType, PlantType)>
                {
                    (PlantType.UltimateSpring, PlantType.PortalPlant)
                }.ToIntegerList(), 0f, 0f, 300, 3000, 7.5f, 500);
            CustomCore.RegisterCustomPlantSkin<UltimateSpring, UltimatePortalSpring>(UltimatePortalSpring.PlantID, ab.GetAsset<GameObject>("UltimatePortalSpringSkinPrefab"),
                ab.GetAsset<GameObject>("UltimatePortalSpringSkinPreview"), new List<(PlantType, PlantType)>
                {
                    (PlantType.UltimateSpring, PlantType.PortalPlant)
                }.ToIntegerList(), 0f, 0f, 300, 3000, 7.5f, 500);
            CustomCore.AddPlantAlmanacStrings(UltimatePortalSpring.PlantID, $"究极火神黑洞菇",
                "投射蕴含时空力量的黑洞，能够持续吸引拉扯僵尸，并赋予传送状态。\n" +
                "<color=#0000FF>究极火神弹弹菇同人亚种</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转换配方：</color><color=red>橄榄帽←→超时空碎片</color>\n" +
                "<color=#3D1400>韧性：</color><color=red>3000</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300</color>\n" +
                "<color=#3D1400>索敌范围：</color><color=red>前方4.5格</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①可手动点击发射，伤害x2\n" +
                "②蓄力时，使究极黑洞的存在时间提升（1+持续时间x10%）倍，蓄力上限30秒\n" +
                "③发射后，黑洞大小会增长（1+蓄力时间x10%），最大400%\n" +
                "④发射会休息1.5秒，投射究极黑洞种子，命中后生成究极黑洞</color>\n" +
                "<color=#3D1400>究极黑洞：</color><color=red>①持续2.5秒，能吸引半径1.5格的僵尸减速25%-80%，并持续使僵尸移动到黑洞中心，僵尸与黑洞中心的距离越远幅度越高\n" +
                "②每1秒对范围内的僵尸施加0.5秒的传送状态，若为领袖僵尸时造成300伤害\n" +
                "③能吸收半径1.5格的究极超时空弹弹菇的子弹，每次吸收子弹会额外增长其10%的大小\n" +
                "④持续时间结束时坍缩，对半径1.5格范围造成7200的灰烬伤害，若已存在5秒则伤害和范围半径x5</color>\n" +
                "<color=#3D1400>词条1:</color><color=red>气定神闲：蓄力速度和蓄力上限x3</color>\n" +
                "<color=#3D1400>词条2:</color><color=red>僵尸试图在火海中游泳：黑洞的吸引效果翻倍</color>\n\n" +
                "<color=#3D1400>“听说您曾有一份工作，是什么原因离职的呢？”记者询问道，究极火神黑洞菇回答“其实呢，我很喜欢那份工作的，我喜欢园艺，喜欢睡在花圃中，和修理好的植物生活在一起，可是我无法控制头上的黑洞，如您所见，它无时不刻都在吸引周围的物件，它把我工作搞砸了！”记者不慌不忙，递上了一份申请表，“是这样的，我这里正好有一份工作，尽管不是你自己喜欢的，但是只有你能做。”之后，失物侦探所迎来了他的耶路撒冷。</color>");
            CustomCore.RegisterCustomBullet<Bullet_springMelon>(UltimatePortalSpring.BulletID, ab.GetAsset<GameObject>("Bullet_portalSpringMelon"));
            CustomCore.RegisterCustomParticle(UltimatePortalSpring.BombID, ab.GetAsset<GameObject>("PortalBombCloud"));
            CustomCore.RegisterCustomUseItemOnPlantEvent(PlantType.UltimateSpring, BucketType.PortalHeart, UltimatePortalSpring.PlantID);
            CustomCore.AddFusion((int)PlantType.UltimateSpring, UltimatePortalSpring.PlantID, (int)PlantType.HelmetPlant);
            CustomCore.RegisterCustomUseItemOnPlantEvent(UltimatePortalSpring.PlantID, BucketType.Helmet, PlantType.UltimateSpring);
            ab.GetAsset<GameObject>("Doom_portal").AddComponent<PortalDoom>();
            CustomCore.RegisterCustomParticle(UltimatePortalSpring.DoomID, ab.GetAsset<GameObject>("Doom_portal"));
            UltimatePortalSpring.target = ab.GetAsset<GameObject>("target_portal");
            UltimatePortalSpring.hole = ab.GetAsset<GameObject>("BlackHole_portal");
            UltimatePortalSpring.hole.AddComponent<PortalHole>();
            CustomCore.TypeMgrExtra.IsMagnetPlants.Add(UltimatePortalSpring.PlantID);
            CustomCore.TypeMgrExtra.IsFirePlant.Add(UltimatePortalSpring.PlantID);
            foreach (var item in Enum.GetValues<BucketType>())
                if (item != BucketType.Helmet)
                    CustomCore.RegisterCustomUseItemOnPlantEvent(UltimatePortalSpring.PlantID, item, (p) => p.Recover(500f));

            ClassInjector.RegisterTypeInIl2Cpp<UltimatePortalSpringConfig>(); // 把傻逼的诸神植物配置注册进沟槽的il2cpp
            ClassInjector.RegisterTypeInIl2Cpp<UniqueUpgrade>(); // 还有他该死的专属词条
        }

        public static void SetLayer(Transform transform, string layer)
        {
            var parsprite = transform.GetComponent<SpriteRenderer>();
            if (parsprite != null) parsprite.sortingLayerName = layer;
            var pargroup = transform.GetComponent<SortingGroup>();
            if (pargroup != null) pargroup.sortingLayerName = layer;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var sprite = child.GetComponent<SpriteRenderer>();
                if (sprite != null) sprite.sortingLayerName = layer;
                var group = child.GetComponent<SortingGroup>();
                if (group != null) group.sortingLayerName = layer;
                SetLayer(child, layer);
            }
        }
    }

    #region 诸神
    public class UltimatePortalSpringConfig : BaseConfig
    {
        // 实现IntPtr构造方法
        public UltimatePortalSpringConfig(IntPtr ptr) : base(ptr) { }
        public UltimatePortalSpringConfig() : base(ClassInjector.DerivedConstructorPointer<UltimatePortalSpringConfig>()) => ClassInjector.DerivedConstructorBody(this);
        // 实现抽象类的方法
        public PlantType PlantType => UltimatePortalSpring.PlantID;
        public Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }
        public override void ReinforcePlant(Plant plant)
        {
            plant.ModifyDamage(PlantDamageAdder.Shooting, 1f, false, new Il2CppSystem.Nullable<float>(float.MaxValue));
        }

        public override string Role => "不嘻嘻";

        // 自定义的方法
        private List<BaseBuff> CustomBuffs = new List<BaseBuff> { new UniqueUpgrade() };
    }

    public class UniqueUpgrade : BaseBuff
    {
        // 实现IntPtr构造方法
        public UniqueUpgrade(IntPtr ptr) : base(ptr) { }
        public UniqueUpgrade() : base(ClassInjector.DerivedConstructorPointer<UniqueUpgrade>()) => ClassInjector.DerivedConstructorBody(this);

        // 实现抽象类的方法
        public override PlantType ShowType => UltimatePortalSpring.PlantID;
        public override string Title => "强化：黑洞";
        public override string Description => "黑洞大小+20%\n蓄力速度和蓄力上限+40%";
        public override void OnGet()
        {
            if (ShootingManager.Instance.TryGetPlant(UltimatePortalSpring.PlantID, out var plant) && plant != null)
            {
                plant.attributeFloat = (int)plant.attributeFloat + 1;
            }
        }
        public override int MaxCount => 5;
        public override float AppearWeight => 0.167f; // 1/6
        public override Quality Rarity => Quality.gold;
    }

    // 把超时空火神加进专家列表
    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.Awake))]
        [HarmonyPostfix]
        public static void PostShootingManager(ShootingManager __instance)
        {
            __instance.ExpertPlants.Add(UltimatePortalSpring.PlantID);
        }

        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void ShowBuff()
        {
            if (Config.configs != null)
            {
                if (!Config.configs.ContainsKey(UltimatePortalSpring.PlantID))
                {
                    Config.configs.Add(UltimatePortalSpring.PlantID, new UltimatePortalSpringConfig());
                }
            }
        }

        // 特调
        //[HarmonyPatch(nameof(ShootingManager.RegisterExpertBuff))]
        //[HarmonyPrefix]
        //public static bool PreRegisterExpertBuff(ShootingManager __instance, ref MultipleChoiceMenu menu)
        //{
        //    // 计算专家植物出现概率：基于幸运值（0.3倍 + 1）再乘以0.01
        //    float probability = (__instance.Lucky * 8f + 1f) * 0.01f;
        //    if (UnityEngine.Random.value > probability) return false;
        //    if (__instance.YourPlants.Contains(UltimatePortalSpring.PlantID)) return false;
        //    // 随机选择一个专家植物
        //    PlantType selectedPlant = UltimatePortalSpring.PlantID;

        //    // 注册菜单选项
        //    string plantName = Lawnf.GetName(selectedPlant);
        //    string description = "获得新植物：" + plantName;

        //    menu.RegisterOption(
        //        "专家邀请",                    // 标题
        //        description,                  // 描述
        //        (Action)(() => __instance.GetNewPlant(selectedPlant)), // 点击回调
        //        selectedPlant,
        //        frameType: Quality.diamond
        //    );
        //    return false;
        //}
    }
    #endregion

    public class PortalDoom : MonoBehaviour
    {
        public void Die() => Destroy(gameObject);
    }

    public class UltimatePortalSpring : MonoBehaviour
    {
        public static ID PlantID = 1969;
        public static ID BulletID = 1969;
        public static ID BombID = 1969;
        public static ID DoomID = 1970;
        public static GameObject target = null;
        public static GameObject hole = null;

        public bool registered = false;
        public float shootCountDown = 0f;

        public void Awake()
        {
            plant.shoot = transform.FindChild("Shoot");
            Core.SetLayer(target.transform, "particle10");
            Core.SetLayer(hole.transform, "particle10");
        }

        public void Start()
        {
            if (plant == null || plant.board == null) return;
            if (TravelMgr.Instance == null) return;
            // board没初始化，只能在start调用
            // 如果是傻逼诸神
            if (plant.board.boardTag.rogueShooting)
            {
                TravelMgr.Instance.GetUltiBuff(CoreTools.GetUltiBuffByString("僵尸试图在火海中游泳")); // 拿词条
                InGameText.Instance.ShowText("在该模式下，究极火神黑洞菇蓄力完成后将自动发射", 3f);
            }
        }

        public void Update()
        {
            if (plant == null) return;
            if (plant.board == null) return;
            if (plant.board.boardTag.rogueShooting) // 如果是沟槽的诸神
            {
                // 把自动射击ban了
                plant.thePlantAttackInterval = 100_0000f;
                plant.thePlantAttackCountDown = 100_0000f;
            }
            if (!registered)
            {
                var func = () => $"{Math.Round(GetHoleTime(), 2, MidpointRounding.AwayFromZero)}";
                plant.ClearAllText();
                plant.RegisterText(Color.cyan, func);
                registered = true;
            }
            plant.UpdateText();
            if (shootCountDown > 0f)
            {
                shootCountDown -= Time.deltaTime;
                if (shootCountDown <= 0f)
                    shootCountDown = 0f;
                plant.anim.ResetTrigger("shoot2");
            }
            if (plant.ThrowerSearchZombie() == null) plant.anim.ResetTrigger("shoot");
        }

        public float GetHoleTime() => 2.5f * (1 + 0.1f * plant.timer);

        public void PortalShoot()
        {
            var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow, BulletID, BulletMoveWay.Throw);
            var umbrella = plant.FindUmbrella(plant.shoot.position);

            if (umbrella != null)
                bullet.ThrowTo(umbrella, new Il2CppSystem.Nullable<Vector2>(plant.shoot.position), new());
            else if (plant.targetZombie != null && plant.targetZombie.col != null)
                bullet.ThrowTo(plant.targetZombie, new Il2CppSystem.Nullable<Vector2>(plant.shoot.position), new Il2CppSystem.Nullable<float>(plant.flightTime));
            else
            {
                if (plant.board.gridSystem != null)
                {
                    var targetGrid = plant.board.gridSystem.GetGrid(plant.board.columnNum - 1, plant.thePlantRow);
                    if (targetGrid != null)
                        bullet.SetSpeed(plant.shoot.position, Vector2.zero, targetGrid.Position, plant.flightTime);
                }
            }

            bullet.targetPlant = umbrella;

            bullet.Damage = plant.attackDamage;
            bullet.fromType = plant.thePlantType;

            if (plant.melonSputter)
                bullet.melonSputter = true;

            if (plant.PumpkinType == PlantType.MelonPumpkin)
                plant.MelonShoot();

            bullet.SetData("UltimatePortalSpring_HoleTime", GetHoleTime());
            plant.timer = 0f;

            plant.targetZombie = null;
            Extensions.StartCoroutine(this, ResetCharge());

            GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);
        }

        public IEnumerator ResetCharge()
        {
            yield return new WaitForSeconds(1.5f);
            if (plant != null) plant.anim.SetTrigger("charge");
            yield break;
        }

        public void PortalShootByMouse()
        {
            var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, Mouse.Instance.GetRowFromY(plant.cannonTarget.x, plant.cannonTarget.y),
                BulletID, BulletMoveWay.Throw);
            if (bullet == null)
                return;

            bullet.SetSpeed(plant.shoot.position, Vector2.zero, plant.cannonTarget, 1.5f);

            bullet.melonSputter = plant.melonSputter;
            bullet.fromType = plant.thePlantType;

            bullet.SetData("UltimatePortalSpring_HoleTime", GetHoleTime());
            plant.timer = 0f;
            bullet.Damage = plant.attackDamage;
            bullet.Damage *= 2;
            Extensions.StartCoroutine(this, ResetCharge());
            if (plant.board.boardTag.rogueShooting)
                shootCountDown = 2f;
        }

        public void SetShootTarget(Vector2 position)
        {
            if (plant.theStatus == PlantStatus.Melonfume_charge)
            {
                plant.anim.SetTrigger("shoot2");

                plant.cannonTarget = position;
            }
        }

        public UltimateSpring plant => gameObject.GetComponent<UltimateSpring>();
    }

    public class PortalHole : MonoBehaviour
    {
        public static Dictionary<Zombie, float> affectedZombies = new Dictionary<Zombie, float>();
        public static Dictionary<Zombie, PortalHole> zombieControllerMap = new Dictionary<Zombie, PortalHole>(); // 新增：记录控制僵尸的黑洞

        public class AttractedBullet
        {
            public Bullet bullet;
            public float angle;
            public float radius;
            public float angularMomentum;   // 角动量（守恒）
            public float radialVelocity;    // 径向速度（向内为正）
            public float startAngle;
        }
        public List<AttractedBullet> attractedBullets = new List<AttractedBullet>();

        public float timer = 2.5f;
        public float live = 0f;
        public float attackCountDown = 1f;
        public Board board;
        public Dictionary<Zombie, float> myAffectedZombies = new();
        public bool isDying = false;
        public bool enterDieAnim = false;
        public float shrinkDuration = 1f;
        public float shrinkElapsed = 0f;
        public Vector3 originalScale;

        public int absorbedBulletCount = 0;
        public Vector3 baseScale;
        public float maxGrowScale = 4f;
        public float autoGrowTargetScale = 1f;
        public float UpdateCountDown = 0.5f;
        public Vector3 center => transform.position - new Vector3(0f, 0.5f, 0f);
        public float scaleMulti // 获取大小倍率
        {
            get
            {
                if (ShootingManager.Instance.TryGetPlant(UltimatePortalSpring.PlantID, out var plant) && plant != null)
                {
                    return 1 + plant.attributeFloat * 0.2f; // 每选一个+20%
                }
                return 1;
            }
        }

        public static bool IsPortalHoleInRange(Vector3 center, float radius)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<PortalHole>(out _))
                {
                    return true;
                }
            }
            return false;
        }

        public void Start()
        {
            board = Board.Instance;
            if (board == null)
            {
                Destroy(gameObject);
                return;
            }
            if (board.boardTag.rogueShooting)
                transform.localScale *= scaleMulti;
            baseScale = transform.localScale;
            originalScale = baseScale;
            autoGrowTargetScale = baseScale.x * maxGrowScale;
        }

        public void FixedUpdate()
        {
            if (isDying || enterDieAnim) return;
            if (Time.timeScale == 0f) return;
            //if (UpdateCountDown > 0f) // 0.5s计算一次
            //{
            //    UpdateCountDown -= Time.deltaTime;
            //    if (UpdateCountDown <= 0f)
            //    {
            //        UpdateCountDown = 0.5f;
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}
            CheckAndRestoreZombies();

            attackCountDown -= Time.deltaTime;
            var attack = false;
            if (attackCountDown <= 0f)
            {
                attackCountDown = 1f;
                attack = true;
            }

            if (transform.localScale.x < autoGrowTargetScale)
            {
                float growthFactor = timer / 2.5f; // 咔咔一顿消
                float targetScaleX = Mathf.Min(baseScale.x * growthFactor, autoGrowTargetScale);

                if (targetScaleX > transform.localScale.x)
                {
                    transform.localScale = new Vector3(targetScaleX, targetScaleX, transform.localScale.z);
                }
            }

            UpdateBullets();

            AttractBullets();

            float maxRadius = CoreTools.ColumnX * 1.5f * transform.localScale.x * (CoreTools.TravelUltimate("僵尸试图在火海中游泳") ? 1.25f : 1f);
            Vector3 attractCenter = center;
            myAffectedZombies = myAffectedZombies.Where(kvp => kvp.Key != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            foreach (var collider in Physics2D.OverlapCircleAll(attractCenter, maxRadius, LayerMask.GetMask("Zombie")))
            {
                if (!collider.IsObjExist()) continue;

                if (!collider.TryGetComponent<Zombie>(out var zombie) || !zombie.IsObjExist()) continue;
                if (zombie.theZombieType is ZombieType.HorseBoss or ZombieType.ZombieBoss or ZombieType.ZombieBoss2) continue;
                zombieControllerMap.TryGetValue(zombie, out var currentController);

                if (currentController != null && currentController != this) continue;

                bool isInMyAffected = myAffectedZombies.ContainsKey(zombie);
                bool isInAffected = affectedZombies.ContainsKey(zombie);

                if (isInMyAffected && !isInAffected) continue;

                float sqrDistance = (zombie.axis.transform.position - attractCenter).sqrMagnitude;
                float sqrMaxRadius = maxRadius * maxRadius;

                if (sqrDistance > sqrMaxRadius) continue;

                float distance = Mathf.Sqrt(sqrDistance);
                float normalizedDistance = Mathf.Max(distance / maxRadius, 0.1f);
                float effectStrength = Mathf.Min(1f / normalizedDistance, 5f);

                float speedMultiplier = Mathf.Lerp(0.3f, 1f, 1f - effectStrength / 5f);
                float attractionForce = Mathf.Lerp(0.5f, 5f, effectStrength / 5f);

                if (attack)
                {
                    if (!TypeMgr.IsBossZombie(zombie.theZombieType))
                    {
                        var timers = zombie.GetAttrTimers();
                        if (timers.portaledTimer > 0f)
                            timers.portaledTimer += 0.5f;
                        else
                            zombie.SetPortaled(0.5f);
                    }
                    else
                        zombie.TakeDamage(DmgType.Normal, 300, UltimatePortalSpring.PlantID);
                }

                // 优化点6：首次影响时，使用更高效的字典操作
                if (!isInMyAffected)
                {
                    // 使用 TryAdd 减少一次查询（.NET Core 3.0+ / .NET 5+）
                    if (!isInAffected)
                    {
                        affectedZombies[zombie] = zombie.theOriginSpeed;
                    }
                    myAffectedZombies[zombie] = zombie.theOriginSpeed;

                    // 直接赋值，避免多次查询
                    zombieControllerMap[zombie] = this;
                }

                // 优化点7：避免重复索引器查询
                float originalSpeed = myAffectedZombies[zombie];
                zombie.theOriginSpeed = originalSpeed * speedMultiplier;

                float moveDistance = attractionForce * Time.deltaTime * (CoreTools.TravelUltimate("僵尸试图在火海中游泳") ? 2f : 1f);
                Vector3 newPos = Vector3.MoveTowards(zombie.axis.position, attractCenter, moveDistance);
                zombie.SetPosition(newPos);
                zombie.theZombieRow = Mouse.Instance.GetRowFromY(newPos.x, newPos.y);
            }

            live += Time.deltaTime;
            if (live >= timer)
            {
                GetComponent<Animator>().SetTrigger("die");
                enterDieAnim = true;
            }
        }

        public void CheckAndRestoreZombies()
        {
            float maxRadius = CoreTools.ColumnX * 1.5f * transform.localScale.x;
            Vector3 attractCenter = center;
            List<Zombie> zombiesToRestore = new List<Zombie>();

            foreach (var kvp in myAffectedZombies)
            {
                Zombie zombie = kvp.Key;
                if (zombie == null || !zombie.IsObjExist())
                {
                    zombiesToRestore.Add(zombie);
                    continue;
                }

                float distance = Vector2.Distance(attractCenter, zombie.axis.transform.position);
                if (distance > maxRadius)
                {
                    zombiesToRestore.Add(zombie);
                }
            }

            foreach (var zombie in zombiesToRestore)
            {
                if (zombie != null && zombie.IsObjExist() && myAffectedZombies.ContainsKey(zombie))
                {
                    float originalSpeed = myAffectedZombies[zombie];
                    zombie.theOriginSpeed = originalSpeed;

                    if (zombie.GetAttrTimers().portaledTimer > 0f)
                    {
                        zombie.GetAttrTimers().portaledTimer = 0f;
                    }

                    myAffectedZombies.Remove(zombie);

                    // 移除控制权（如果当前黑洞还控制着这个僵尸）
                    if (zombieControllerMap.ContainsKey(zombie) && zombieControllerMap[zombie] == this)
                    {
                        // 检查是否有其他黑洞在控制这个僵尸
                        bool hasOtherController = false;
                        var allPortals = FindObjectsOfType<PortalHole>();
                        foreach (var portal in allPortals)
                        {
                            if (portal != this && portal.myAffectedZombies.ContainsKey(zombie))
                            {
                                hasOtherController = true;
                                zombieControllerMap[zombie] = portal;
                                break;
                            }
                        }

                        if (!hasOtherController)
                        {
                            zombieControllerMap.Remove(zombie);
                        }
                    }

                    if (affectedZombies.ContainsKey(zombie))
                    {
                        bool isAffectedByOther = false;
                        var allPortals = FindObjectsOfType<PortalHole>();
                        foreach (var portal in allPortals)
                        {
                            if (portal != this && portal.myAffectedZombies.ContainsKey(zombie))
                            {
                                isAffectedByOther = true;
                                break;
                            }
                        }

                        if (!isAffectedByOther)
                        {
                            affectedZombies.Remove(zombie);
                        }
                    }
                }
            }
        }

        private void AttractBullets()
        {
            // 扩大捕获范围：从原来的 1.7 倍提高到 3.5 倍
            float attractRadius = CoreTools.ColumnX * 3.5f * transform.localScale.x;

            foreach (var collider in Physics2D.OverlapCircleAll(transform.position, attractRadius, LayerMask.GetMask("Bullet")))
            {
                if (collider.IsObjExist() && collider.TryGetComponent<Bullet>(out var bullet))
                {
                    if (attractedBullets.Any(ab => ab.bullet == bullet))
                        continue;

                    if (bullet.theBulletType == UltimatePortalSpring.BulletID)
                    {
                        bullet.SetData("UltimatePortalSpring_BulletDieByHole", true);
                        foreach (var col in bullet.GetComponents<BoxCollider2D>())
                            col.enabled = false;

                        Vector2 dir = bullet.transform.position - transform.position;
                        float radius = dir.magnitude;
                        float angle = Mathf.Atan2(dir.y, dir.x);

                        // 获取子弹速度
                        Vector2 velocity = bullet.GetComponent<Rigidbody2D>()?.velocity ?? Vector2.zero;
                        float tangentialVel = Vector2.Dot(velocity, new Vector2(-dir.y, dir.x).normalized);

                        // 角动量 L = r * v_t
                        float angularMomentum = radius * tangentialVel;
                        // 径向速度（向内为正）
                        float radialVelocity = -Vector2.Dot(velocity, dir.normalized);

                        AttractedBullet attracted = new AttractedBullet
                        {
                            bullet = bullet,
                            angle = angle,
                            radius = radius,
                            angularMomentum = angularMomentum,
                            radialVelocity = radialVelocity,
                            startAngle = angle
                        };

                        attractedBullets.Add(attracted);
                    }
                }
            }
        }

        private void UpdateBullets()
        {
            float currentScale = transform.localScale.x;           // 当前黑洞大小
            float baseScale = 0.35f;                               // 初始大小

            // 引力强度随大小非线性增长
            float gravityMultiplier = Mathf.Pow(currentScale / baseScale, 2f);

            float baseG = 80f;
            float G = baseG * gravityMultiplier;

            float baseEscapeThreshold = 1.2f;
            float escapeThreshold = baseEscapeThreshold * (1f + gravityMultiplier * 0.5f);

            float escapeRadius = CoreTools.ColumnX * 1.0f * currentScale;

            for (int i = attractedBullets.Count - 1; i >= 0; i--)
            {
                AttractedBullet attracted = attractedBullets[i];

                if (attracted.bullet == null || !attracted.bullet.IsObjExist())
                {
                    attractedBullets.RemoveAt(i);
                    continue;
                }

                float r = attracted.radius;
                float v_r = attracted.radialVelocity;
                float L = attracted.angularMomentum;

                float gravitationalAcc = G / (r * r);
                float centrifugalAcc = (L * L) / (r * r * r);
                float netAcc = gravitationalAcc - centrifugalAcc;

                // 逃逸判定
                if (centrifugalAcc > gravitationalAcc * escapeThreshold && r > escapeRadius)
                {
                    foreach (var col in attracted.bullet.GetComponents<BoxCollider2D>())
                        col.enabled = true;
                    attracted.bullet.SetData("UltimatePortalSpring_BulletDieByHole", false);
                    attractedBullets.RemoveAt(i);
                    continue;
                }

                v_r += netAcc * Time.deltaTime;
                r -= v_r * Time.deltaTime;

                float angularSpeed = L / (r * r);
                attracted.angle += angularSpeed * Time.deltaTime;

                // 【修复】吸收半径：直接使用黑洞视觉大小乘以系数
                // 黑洞视觉半径大约是 localScale.x * 0.5（因为 scale 是直径），所以这里用 0.3 倍视觉半径
                float eventHorizon = 0.5f * CoreTools.ColumnX;

                // 【修复】直接判断，不做下限限制（或者下限设得极低）
                if (r < eventHorizon || v_r > 20f)
                {
                    AbsorbBullet(attracted.bullet);
                    attractedBullets.RemoveAt(i);
                    continue;
                }

                // 【修复】下限设为一个极小的值，不影响吸收判断
                r = Mathf.Max(r, eventHorizon * 0.5f);
                attracted.radius = r;
                attracted.radialVelocity = v_r;

                float x = transform.position.x + Mathf.Cos(attracted.angle) * r;
                float y = transform.position.y + Mathf.Sin(attracted.angle) * r;
                attracted.bullet.transform.position = new Vector3(x, y, attracted.bullet.transform.position.z);
            }
        }

        public void AbsorbBullet(Bullet bullet)
        {
            if (bullet == null || !bullet.IsObjExist())
                return;

            absorbedBulletCount++;

            float newScale = transform.localScale.x + 0.1f;
            transform.localScale = new Vector3(newScale, newScale, transform.localScale.z);

            bullet.Die();
        }

        public void Die()
        {
            if (isDying) return;
            isDying = true;

            foreach (var attracted in attractedBullets)
                if (attracted.bullet != null && attracted.bullet.IsObjExist())
                    attracted.bullet.Die();
            attractedBullets.Clear();
            myAffectedZombies = myAffectedZombies.Where(kvp => kvp.Key != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            foreach (var kvp in myAffectedZombies)
            {
                Zombie zombie = kvp.Key;
                if (zombie == null) continue;
                if (zombie != null && zombie.IsObjExist())
                {
                    zombie.theOriginSpeed = kvp.Value;

                    if (zombie.GetAttrTimers().portaledTimer > 0f)
                        zombie.GetAttrTimers().portaledTimer = 0f;
                }
                // 移除控制权
                if (zombieControllerMap.ContainsKey(zombie) && zombieControllerMap[zombie] == this)
                {
                    // 检查是否有其他黑洞在控制这个僵尸
                    bool hasOtherController = false;
                    var allPortals = FindObjectsOfType<PortalHole>();
                    foreach (var portal in allPortals)
                    {
                        if (portal != this && portal.myAffectedZombies.ContainsKey(zombie))
                        {
                            hasOtherController = true;
                            zombieControllerMap[zombie] = portal;
                            break;
                        }
                    }

                    if (!hasOtherController)
                    {
                        zombieControllerMap.Remove(zombie);
                    }
                }

                if (affectedZombies.ContainsKey(zombie))
                {
                    bool isAffectedByOther = false;
                    var allPortals = FindObjectsOfType<PortalHole>();
                    foreach (var portal in allPortals)
                    {
                        if (portal != this && portal.myAffectedZombies.ContainsKey(zombie))
                        {
                            isAffectedByOther = true;
                            break;
                        }
                    }

                    if (!isAffectedByOther)
                    {
                        affectedZombies.Remove(zombie);
                    }
                }
            }
            myAffectedZombies.Clear();

            var buff = live >= 5f ? 5 : 1;
            var damage = 7200 * buff;
            float explosionRadius = CoreTools.ColumnX * 1.5f * transform.localScale.x * buff;
            foreach (var collider in Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Zombie")))
            {
                if (collider.IsObjExist() && collider.TryGetComponent<Zombie>(out var zombie) && zombie.IsObjExist())
                {
                    zombie.TakeDamage(DmgType.Carred, damage, UltimatePortalSpring.PlantID);
                }
            }

            if (timer <= 5f)
            {
                CreateParticle.SetParticle(UltimatePortalSpring.BombID, transform.position, 11);
                GameAPP.PlaySound(40, 0.5f, 1.0f);
            }
            else
            {
                CreateParticle.SetParticle(UltimatePortalSpring.DoomID, transform.position, 11);
                GameAPP.PlaySound(SoundType.DoomShroom, 0.5f, 1.0f);
            }

            Destroy(gameObject);
        }
    }

    [HarmonyPatch(typeof(Bullet_springMelon))]
    public static class Bullet_springMelonPatch
    {
        [HarmonyPatch(nameof(Bullet_springMelon.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_springMelon __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == UltimatePortalSpring.BulletID)
            {
                if (!__instance.GetData<bool>("UltimatePortalSpring_BulletDieByHole") || !PortalHole.IsPortalHoleInRange(__instance.transform.position, 1f))
                {
                    zombie.TakeDamage(DmgType.Normal, __instance.Damage, __instance.fromType);
                    zombie.SetJalaed();
                    var effectPos = zombie.axis.position + new Vector3(0f, 0.5f, 0f);

                    var hole = UnityEngine.Object.Instantiate(UltimatePortalSpring.hole, effectPos, Quaternion.identity, __instance.board.transform)?.GetComponent<PortalHole>();
                    if (hole != null) hole.timer = __instance.GetData<float>("UltimatePortalSpring_HoleTime");

                    CreateParticle.SetParticle(UltimatePortalSpring.BombID, effectPos, 11);
                    __instance.AttackOtherZombie(zombie, MelonSputterType.RealJala);

                    GameAPP.PlaySound(40, 0.5f, 1.0f);
                    __instance.Die();
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Bullet_springMelon.HitLand))]
        [HarmonyPrefix]
        public static bool PreHitLand(Bullet_springMelon __instance)
        {
            if (__instance.theBulletType == UltimatePortalSpring.BulletID)
            {
                if (!__instance.GetData<bool>("UltimatePortalSpring_BulletDieByHole") || !PortalHole.IsPortalHoleInRange(__instance.transform.position, 1f))
                {
                    var effectPos = __instance.transform.position;

                    var hole = UnityEngine.Object.Instantiate(UltimatePortalSpring.hole, effectPos, Quaternion.identity, __instance.board.transform)?.GetComponent<PortalHole>();
                    if (hole != null) hole.timer = __instance.GetData<float>("UltimatePortalSpring_HoleTime");

                    CreateParticle.SetParticle(UltimatePortalSpring.BombID, effectPos, 11);

                    GameAPP.PlaySound(40, 0.5f, 1.0f);
                    __instance.Die();
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet))]
    public static class BulletPatch
    {
        [HarmonyPatch(nameof(Bullet.Die))]
        [HarmonyPostfix]
        public static void PostDie(Bullet __instance)
        {
            if (__instance.theBulletType == UltimatePortalSpring.BulletID)
            {
                foreach (var col in __instance.GetComponents<BoxCollider2D>())
                    col.enabled = true;
                __instance.SetData("UltimatePortalSpring_BulletDieByHole", false);
            }
        }
    }

    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPatch(nameof(Mouse.LeftClickWithSomeThing))]
        [HarmonyPostfix]
        public static void PostLeftClickWithSomeThing(Mouse __instance)
        {
            if (__instance.theItemOnMouse != null && __instance.cannonPlant != null && __instance.cannonPlant.thePlantType == UltimatePortalSpring.PlantID &&
                __instance.theItemOnMouse.name == "target_portal")
            {
                __instance.cannonPlant.GetComponent<UltimatePortalSpring>().SetShootTarget(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                __instance.ClearItemOnMouse(true);
            }
        }
    }

    [HarmonyPatch(typeof(UltimateSpring))]
    public static class UltimateSpringPatch
    {
        [HarmonyPatch(nameof(UltimateSpring.OnClicked))]
        [HarmonyPrefix]
        public static bool PreOnlicked(UltimateSpring __instance, ref Mouse mouse, ref bool __result)
        {
            if (__instance.thePlantType == UltimatePortalSpring.PlantID)
            {
                if (__instance.theStatus != PlantStatus.Melonfume_charge)
                {
                    __result = false;
                    return false;
                }
                mouse.cannonPlant = __instance;
                mouse.theItemOnMouse = UnityEngine.Object.Instantiate(UltimatePortalSpring.target, mouse.MousePosition,
                                Quaternion.identity, __instance.board.transform);
                mouse.theItemOnMouse.name = "target_portal";
                __result = true;
                return false;
            }
            return true;
        }

        // 给傻逼专属词条打的补丁
        [HarmonyPatch(nameof(UltimateSpring.TimerUpdate))]
        [HarmonyPrefix]
        public static bool PreTimerUpdate(UltimateSpring __instance)
        {
            if (__instance.thePlantType == UltimatePortalSpring.PlantID && __instance.board.boardTag.rogueShooting)
            {
                // ai抄的
                var multi = 1f;
                if (ShootingManager.Instance.TryGetPlant(UltimatePortalSpring.PlantID, out var plant) && plant != null)
                    multi += plant.attributeFloat * 0.4f;
                // 累加到计时器
                __instance.timer = Mathf.Min(__instance.timer + Time.deltaTime * multi, 30f * multi);
                if (__instance.timer >= 30f * multi)
                {
                    var x = Mouse.Instance.GetBoxXFromColumn(7);
                    var y = Mouse.Instance.GetBoxYFromRow(2);
                    __instance.cannonTarget = new(x, y);
                    __instance.anim.SetTrigger("shoot2");
                }
                return false;
            }
            return true;
        }
    }
}
