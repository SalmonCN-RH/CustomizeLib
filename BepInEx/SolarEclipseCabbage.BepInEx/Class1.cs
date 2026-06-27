using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace SolarEclipseCabbage.BepInEx
{
    [BepInPlugin("salmon.solareclipsecabbage", "SolarEclipseCabbage", "1.0")]
    public class Core : BasePlugin
    {
        public static GameObject SolarBomb = null;
        public static GameObject SolarEclipseStar = null;
        public static GameObject SolarEclipse = null;

        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<SolarEclipse>();
            ClassInjector.RegisterTypeInIl2Cpp<SolarEclipseCabbage>();
            ClassInjector.RegisterTypeInIl2Cpp<SolarEclipseBomb>();
            ClassInjector.RegisterTypeInIl2Cpp<SolarEclipseDoom>();
            ClassInjector.RegisterTypeInIl2Cpp<SolarEclipseStar>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "solareclipsecabbage");
            CustomCore.RegisterCustomPlant<SolarCabbage, SolarEclipseCabbage>((int)SolarEclipseCabbage.PlantID, ab.GetAsset<GameObject>("SolarEclipseCabbagePrefab").SetSaveMaterial(),
                ab.GetAsset<GameObject>("SolarEclipseCabbagePreview").SetSaveMaterial(), [], 2f, 0f, 300, 300, 90f, 850);
            CustomCore.RegisterCustomPlantSkin<SolarCabbage, SolarEclipseCabbage>((int)SolarEclipseCabbage.PlantID, ab.GetAsset<GameObject>("SolarEclipseCabbagePrefabSkin").SetSaveMaterial(),
                ab.GetAsset<GameObject>("SolarEclipseCabbagePreviewSkin").SetSaveMaterial(), [], 2f, 0f, 300, 300, 90f, 850);
            CustomCore.AddPlantAlmanacStrings((int)SolarEclipseCabbage.PlantID, $"究级蚀日神卷心菜",
                "昼临异象，墨噬金轮。日食下，灼射的植物将迎来异象强化。\n" +
                "<color=#0000FF>究级太阳神卷心菜的限定形态</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>使用条件：</color><color=red>①融合或转化太阳神卷心菜时有2%概率变异\n" +
                "②神秘模式\n" +
                "*可使用向日葵切回究级太阳神卷心菜</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300x5/2秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①拥有阳光卷心菜的特点\n" +
                "②在场时，召唤的太阳变为日食</color>\n" +
                "<color=#3D1400>日食：</color><color=red>①存在机制同太阳\n" +
                "②日食在场时，在场的所有植物增加50%攻击力，阳光高于15000时，消耗200阳光使攻击力x3</color>\n" +
                "<color=#3D1400>大招：</color><color=red>消耗1000金钱，召唤日食</color>\n" +
                "<color=#3D1400>连携词条：</color><color=red>昼晦宵赤：当究级蚀日神卷心菜和究级血月神卷心菜同时在场时；日食的加成x3，所有魅惑僵尸大幅加强，且日食和血月的持续时间无限。每秒在场上陨落4～8颗伤害为7200的小陨星，同时伤害本体和防具，每株蚀日神和血月神为小陨星伤害增加50%。每10秒陨落赤晦陨星，造成1800x（1+0.5x太阳神数量和月亮神数量）x（100x蚀日神和血月神数量），并分裂180个伤害400的子弹，赤晦陨星陨落时额外携带10个小陨星一起落下</color>\n\n" +
                "<color=#3D1400>当黑日蚀尽天光，乌云密布，声势浩大，世间万物被黑暗笼罩，诸般生灵寂静无声。他高悬黑日之上，宣告光明与秩序的死刑。他是悖逆天理之神，以黑暗重新定义光明。</color>");
            SolarEclipseCabbage.RegisterSuperSkill();
            CustomCore.RegisterCustomBullet<Bullet_sunCabbage>(SolarEclipseCabbage.BulletID, ab.GetAsset<GameObject>("Bullet_solarEclipseCabbage"));
            CustomCore.RegisterCustomBullet<Bullet_sunCabbage>(SolarEclipseCabbage.BulletSkinID, ab.GetAsset<GameObject>("Bullet_solarEclipseCabbageSkin"));
            SolarBomb = ab.GetAsset<GameObject>("SolarEclipseBomb");
            SolarBomb.AddComponent<SolarEclipseBomb>();
            SolarBomb.GetComponent<SortingGroup>().sortingLayerName = "fog";
            SolarEclipse = ab.GetAsset<GameObject>("SolarEclipse");
            SolarEclipse.AddComponent<SolarEclipse>();
            SolarEclipse.GetComponent<SortingGroup>().sortingLayerName = "fog";
            SolarEclipseStar = ab.GetAsset<GameObject>("SolarEclipseStar");
            SolarEclipseStar.AddComponent<SolarEclipseStar>();
            SolarEclipseStar.GetComponent<SortingGroup>().sortingLayerName = "fog";
            ab.GetAsset<GameObject>("EclipseDoomParticle").AddComponent<SolarEclipseDoom>();
            CustomCore.RegisterCustomParticle(SolarEclipseCabbage.ParticleType, ab.GetAsset<GameObject>("EclipseDoomParticle"));
            CustomCore.RegisterCustomParticle(SolarEclipseBomb.bombType, ab.GetAsset<GameObject>("EclipseBombParticle"));
            SolarEclipseCabbage.BuffID = CustomCore.RegisterCustomBuff("昼晦宵赤：当究级蚀日神卷心菜和究级血月神卷心菜同时在场时；日食的加成x3，所有魅惑僵尸属性大幅增强，且日食和血月的持续时间无限，持续召唤陨星", BuffType.AdvancedBuff, () =>
            Board.Instance.ObjectExist<SolarEclipseCabbage>() && Board.Instance.ObjectExist<RedLunarCabbage>() && Lawnf.TravelUltimate((UltiBuff)22) && Lawnf.TravelUltimate((UltiBuff)23) && TravelStore.Instance != null, 15000, PlantType.EndoFlame);
            CustomCore.TypeMgrExtra.LevelPlants.Add(SolarEclipseCabbage.PlantID, CardLevel.Red);
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, (int)SolarEclipseCabbage.PlantID, (int)PlantType.SunFlower);
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, (int)PlantType.SunFlower, (int)SolarEclipseCabbage.PlantID);
            CustomCore.AddUltimatePlant((PlantType)SolarEclipseCabbage.PlantID);
        }
    }

    public class SolarEclipseCabbage : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1944;
        public static BulletType BulletID = (BulletType)1944;
        public static BulletType BulletSkinID = (BulletType)1945;
        public static ParticleType ParticleType = (ParticleType)1944;
        public static BuffID BuffID = -1;

        public bool byTimer = false;
        public SolarCabbage plant => gameObject.GetComponent<SolarCabbage>();

        public void Update()
        {
            if (plant == null)
                return;
            if (plant.board == null || plant.anim == null)
                return;

            if (Lawnf.TravelAdvanced(BuffID) && Lawnf.GetPlantCount(PlantType.UltimateRedLunar, plant.board) > 0 && Lawnf.GetPlantCount(PlantID, plant.board) > 0 && Lunar.Instance != null &&
                Lunar.Instance.lifeTimer <= 7.0f && Lunar.Instance.red)
                Lunar.Instance.lifeTimer += 15.0f;
        }

        public void SolarEclipseCabbage_SuperSkill()
        {
            if (!byTimer)
            {
                plant.Recover((float)plant.thePlantMaxHealth);
                plant.flashCountDown = 1f;
            }
            if (byTimer && SolarEclipse.Instance != null)
            {
                byTimer = false;
                return;
            }
            byTimer = false;
            if (Solar.Instance != null)
            {
                Solar.Instance.Awake();
            }
            else
            {
                if (SolarEclipse.Instance == null)
                {
                    if (plant.board == null)
                        return;
                    var solarEclipse = Instantiate(Core.SolarEclipse, new Vector3(-25f, 33f, 0f), Quaternion.identity, plant.board.transform).GetComponent<SolarEclipse>();
                    solarEclipse.targetPosition = new Vector3(-9.5f, 6.5f);
                    GameAPP.PlaySound(95);
                }
                else
                {
                    SolarEclipse.Instance.deathTime += 15f;
                }
            }
        }

        public static void RegisterSuperSkill()
        {
            CustomCore.RegisterSuperSkill((int)PlantID, (plant) => 1000, (plant) =>
            {
                plant.anim.SetTrigger("super");
            }, 1000);
        }
    }

    public class SolarEclipse : MonoBehaviour
    {
        public static SolarEclipse Instance = null;

        public Board board = null;
        public float deathTime = 31.0f;
        public float timer = 0.5f;
        public float starTimer = 10f;
        public float bombTimer = 0.5f;
        public float godTimer = 3f;
        public bool arrived = false;
        public Vector3 targetPosition = default;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                Instance.deathTime += 15f;
                return;
            }

            // 设置静态实例
            Instance = this;

            board = Board.Instance;

            if (board == null)
                Destroy(gameObject);

            if (GameAPP.config.disableSolarStarEffect)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(2).gameObject.SetActive(false);
                transform.GetChild(3).gameObject.SetActive(true);
            }

            if (Solar.Instance != null)
                Destroy(Solar.Instance.gameObject);
        }

        public void Update()
        {
            Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, Mathf.Clamp(deathTime * 5.0f, 0.0f, 1.0f));
            newPos.z = 0.0f;
            transform.position = newPos;
            if (Vector3.Distance(transform.position, new Vector3(targetPosition.x, targetPosition.y, 0.0f)) < 0.5f)
                arrived = true;

            if (arrived && GameAPP.theGameStatus == GameStatus.InGame)
            {
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    timer = 0.5f;

                    bool any = false;
                    bool buffAdd = false;
                    if (Lawnf.TravelUltimate((UltiBuff)23))
                        buffAdd = true;
                    foreach (var plant in board.boardEntity.plantArray)
                    {
                        var multi = 1f;
                        if (plant == null) continue;

                        bool trigger = false;
                        if (plant.GetData("SolarEclipse_addedDamage_base") is null || (plant.GetData("SolarEclipse_addedDamage_base") is not null && plant.GetData("SolarEclipse_addedDamage_base") is false))
                        {
                            multi += 0.5f;
                            trigger = true;
                            plant.SetData("SolarEclipse_addedDamage_base", true);
                        }

                        if (board.theSun > 15000)
                        {
                            if (plant.GetData("SolarEclipse_addedDamage_extra") is null || (plant.GetData("SolarEclipse_addedDamage_extra") is not null && plant.GetData("SolarEclipse_addedDamage_extra") is false))
                            {
                                multi += 2f;
                                trigger = true;
                                plant.SetData("SolarEclipse_addedDamage_extra", true);
                                any = true;
                            }
                        }
                        if (buffAdd)
                        {
                            if (plant.GetData("SolarEclipse_addedDamage_buff") is null || (plant.GetData("SolarEclipse_addedDamage_buff") is not null && plant.GetData("SolarEclipse_addedDamage_buff") is false))
                            {
                                multi += 2f;
                                trigger = true;
                                plant.SetData("SolarEclipse_addedDamage_buff", true);
                            }
                        }
                        if (Lawnf.GetPlantCount(PlantType.UltimateRedLunar, board) > 0 && Lawnf.GetPlantCount(SolarEclipseCabbage.PlantID, board) > 0 &&
                            Lawnf.TravelAdvanced((AdvBuff)SolarEclipseCabbage.BuffID) && trigger)
                            multi += 2f;
                        if (multi != 1f)
                            plant.ModifyDamage((PlantDamageAdder)1944, multi, false, float.MaxValue.GetNullable());
                    }
                    if (any && board.theSun > 15000)
                    {
                        board.UseSun(200f);
                    }
                }

                deathTime -= Time.deltaTime;
                if (deathTime < 7.0f)
                {
                    if (Lawnf.GetPlantCount(PlantType.UltimateRedLunar, board) > 0 && Lawnf.GetPlantCount(SolarEclipseCabbage.PlantID, board) > 0 &&
                        Lawnf.TravelAdvanced((AdvBuff)SolarEclipseCabbage.BuffID))
                    {
                        deathTime += 15f;
                    }
                    if (deathTime < 0.0f)
                    {
                        Destroy(gameObject);
                        Instance = null;
                    }
                }

                if (Lawnf.TravelAdvanced((AdvBuff)SolarEclipseCabbage.BuffID) && Lawnf.GetPlantCount(PlantType.UltimateRedLunar, board) > 0 && Lawnf.GetPlantCount(SolarEclipseCabbage.PlantID, board) > 0)
                {
                    starTimer -= Time.deltaTime;
                    if (starTimer <= 0f)
                    {
                        SolarEclipseStar.SetStar();
                        starTimer = 10f;
                        for (int i = 0; i < 10; i++)
                            SolarEclipseBomb.SetBomb();
                    }

                    bombTimer -= Time.deltaTime;
                    if (bombTimer <= 0f)
                    {
                        for (int i = 0; i < UnityEngine.Random.Range(2, 5); i++)
                            SolarEclipseBomb.SetBomb();
                        bombTimer = 0.5f;
                    }
                }

                if (Solar.Instance != null)
                    Destroy(Solar.Instance.gameObject);

                if (Instance != this)
                    Destroy(gameObject);

                if (GameAPP.theGameStatus == GameStatus.InGame)
                    godTimer -= Time.deltaTime;
                if (godTimer <= 0f)
                {
                    if (!Lawnf.TravelAdvanced((AdvBuff)45))
                        return;

                    if (board == null) return;

                    if ((Lawnf.GetPlantCount(PlantType.UltimateStar, board) + Lawnf.GetPlantCount(PlantType.UltimateBlover, board) >= 10) && (Lawnf.GetPlantCount(PlantType.UltimateCabbage, board) >= 10))
                    {
                        GameObject solarStar = Instantiate(GameAPP.itemPrefab[47], board.transform);
                    }
                    godTimer = 3f;
                }
            }
        }
    }

    public class SolarEclipseStar : MonoBehaviour
    {
        public float g = -9.8f;
        public bool isLand = false;
        public float minY = 0.6f;
        public Board board = null;
        public float speedX = 7f;
        public float speedY = 0.0f;
        public Transform axis = null;

        public void Awake()
        {
            if (GameAPP.config.disableSolarStarEffect)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }
            else
                transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().Simulate(10f);
        }

        public void Start()
        {
            board = Board.Instance;
            if (board == null)
                Destroy(gameObject);

            // 播放音效
            GameAPP.PlaySound(95, 1.0f, 1.0f);

            // 计算最小Y位置
            if (board.rowNum % 2 == 0)
                minY = Mouse.Instance.GetBoxYFromRow(board.rowNum / 2 - 1) + 1f;
            else
                minY = Mouse.Instance.GetBoxYFromRow(board.rowNum / 2) + 1f;
            axis = gameObject.transform.FindChild("SolarStar/axis");
        }

        public void Update()
        {
            if (!isLand)
            {
                // 重力模拟
                speedY += g * Time.deltaTime;

                // 位置更新
                Vector3 position = transform.position;
                position.x += speedX * Time.deltaTime;
                position.y += speedY * Time.deltaTime;
                transform.position = position;

                // 旋转效果
                float rotationDirection = speedX < 0.0f ? -1.0f : 1.0f;
                transform.Rotate(0, 0, rotationDirection * -180.0f * Time.deltaTime);

                // 检查是否落地
                if (axis.position.y < minY)
                {
                    isLand = true;
                    Crash();
                }
            }
        }

        public static void SetStar()
        {
            if (Board.Instance == null) return;
            var go = Instantiate(Core.SolarEclipseStar, Board.Instance.transform);
            go.transform.position = new Vector3(-9f, 13f, 0f);
        }

        public void Crash()
        {
            int starDamage = 0;
            {
                int totalCount1 = Lawnf.GetPlantCount(PlantType.UltimateCabbage, board) + Lawnf.GetPlantCount(PlantType.UltimateLunarCabbage, board);
                int totalCount2 = Lawnf.GetPlantCount(SolarEclipseCabbage.PlantID, board) + Lawnf.GetPlantCount(PlantType.UltimateRedLunar, board);
                starDamage = 1800 * ((int)(1 + 0.5 * totalCount1) + 1) * (100 * totalCount2);
            }

            for (int i = board.zombieArray.Count - 1; i >= 0; i--)
            {
                if (board.zombieArray[i] == null) continue;
                if (board.zombieArray[i].isMindControlled || board.zombieArray[i].beforeDying) continue;
                board.zombieArray[i].TakeDamage(DmgType.NormalAll, starDamage);
            }

            // 播放音效
            GameAPP.PlaySound(41, 1.0f, 1.0f);

            CreateParticle.SetParticle((int)SolarEclipseCabbage.ParticleType, gameObject.transform.FindChild("SolarStar/axis").position, 11);

            // 屏幕震动
            ScreenShake.TriggerShake(0.15f);

            // 创建星星子弹
            CreateStars();

            Destroy(gameObject);
        }

        public void CreateStars()
        {
            for (int ring = 0; ring < 5; ring++)
            {
                float radius = (ring + 1) * 0.5f;
                for (int angle = 0; angle < 360; angle += 10)
                {
                    Vector3 position = axis.position;
                    float rad = angle * Mathf.Deg2Rad;
                    position.x += Mathf.Cos(rad) * radius;
                    position.y += Mathf.Sin(rad) * radius;

                    // 创建子弹
                    Bullet bullet = CreateBullet.Instance.SetBullet(position.x, position.y, 2, BulletType.Bullet_seaStar, (BulletMoveWay)2, false);
                    if (bullet != null)
                    {
                        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
                        bullet.Damage = 400;
                    }
                }
            }
        }
    }

    public class SolarEclipseBomb : MonoBehaviour
    {
        public static ParticleType bombType = (ParticleType)1945;

        public Vector2 target = Vector2.zero;
        public Vector2 start = Vector2.zero;
        public Board board;
        public float maxTime = 0.8f;
        public float timer = 0f;
        public bool crash = false;
        public int damage = 7200;

        public void Start()
        {
            board = Board.Instance;
            if (board == null)
            {
                Destroy(gameObject);
                return;
            }
            SetStartPosition();

            maxTime = UnityEngine.Random.Range(1.8f, 2.1f);
            damage += (Lawnf.GetPlantCount(PlantType.UltimateRedLunar, board) + Lawnf.GetPlantCount(SolarEclipseCabbage.PlantID, board)) * 3600;
        }

        public void Update()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame)
                timer += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, timer / maxTime);
            transform.Rotate(0, 0, -180 * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f && !crash)
            {
                crash = true;
                Bomb();
            }
        }

        public void SetStartPosition()
        {
            var list = board.zombieArray.ToArray().ToList().Where(z => z != null && !z.isMindControlled).ToList();
            if (list.Count <= 0)
            {
                target = new Vector2(UnityEngine.Random.Range(-5f, 7f), UnityEngine.Random.Range(2.7f, -4f));
            }
            else
            {
                int max = -1;
                Zombie zombie = null;
                foreach (var z in list)
                {
                    if (z.theMaxHealth + z.theFirstArmorMaxHealth + z.theSecondArmorMaxHealth > max)
                    {
                        zombie = z;
                        max = z.theMaxHealth + z.theFirstArmorMaxHealth + z.theSecondArmorMaxHealth;
                    }
                }
                zombie = UnityEngine.Random.Range(1, 101) <= 80 ? zombie : list[UnityEngine.Random.Range(0, list.Count)];
                target = new Vector2(zombie.axis.transform.position.x, zombie.axis.transform.position.y + 0.5f);
            }

            float angleRad = UnityEngine.Random.Range(63f, 75f) * Mathf.Deg2Rad; // 27~35度随机偏转

            Vector2 direction = new Vector2(-Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            float minTime = 1.8f;    // 最小时间
            float maxTime = 2.1f;    // 最大时间
            float maxSpeedX = 4.7f;  // x轴最大速度
            float maxSpeedY = 7.5f;  // y轴最大速度

            float maxLength = Mathf.Min(maxSpeedX * maxTime / Mathf.Abs(direction.x), maxSpeedY * maxTime / Mathf.Abs(direction.y));
            float minLength = Mathf.Min(maxSpeedX * minTime / Mathf.Abs(direction.x), maxSpeedY * minTime / Mathf.Abs(direction.y));

            float displacementLength = (minLength + maxLength) * 0.5f;

            start = target + direction * displacementLength;
            transform.position = start;
        }

        public void Bomb()
        {
            var colliders = Physics2D.OverlapCircleAll
                (new Vector2(transform.position.x, transform.position.y),
                2.5f,
                LayerMask.GetMask("Zombie"));

            for (int i = colliders.Count - 1; i >= 0; i--)
            {
                var collider = colliders[i];
                if (collider != null && !collider.IsDestroyed() && collider.TryGetComponent<Zombie>(out var zombie) && zombie != null &&
                    !zombie.IsDestroyed() && !zombie.beforeDying && !zombie.isMindControlled)
                {
                    zombie.TakeDamage(DmgType.RealDamage, damage);
                }
            }

            GameAPP.PlaySound(41, 0.5f, 1.0f);
            CreateParticle.SetParticle((int)bombType, transform.position, 11);

            Destroy(gameObject);
        }

        public static void SetBomb()
        {
            if (Board.Instance == null) return;
            var go = Instantiate(Core.SolarBomb, Board.Instance.transform);
        }
    }

    public class SolarEclipseDoom : MonoBehaviour
    {
        public void Die() => Destroy(gameObject);
    }

    [HarmonyPatch(typeof(SolarCabbage))]
    public static class SolarCabbagePatch
    {
        [HarmonyPatch(nameof(SolarCabbage.GetBulletType))]
        [HarmonyPrefix]
        public static bool GetBulletType(SolarCabbage __instance, ref BulletType __result)
        {
            if (__instance.thePlantType == SolarEclipseCabbage.PlantID)
            {
                if (__instance.gameObject.name.ToLower().Contains("skin"))
                    __result = SolarEclipseCabbage.BulletSkinID;
                else
                    __result = SolarEclipseCabbage.BulletID;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(SolarCabbage.SuperStart))]
        [HarmonyPrefix]
        public static bool SuperStart(SolarCabbage __instance)
        {
            if (__instance.thePlantType == SolarEclipseCabbage.PlantID)
            {
                if (__instance.TryGetComponent<SolarEclipseCabbage>(out var plant) && plant != null)
                    plant.SolarEclipseCabbage_SuperSkill();
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(SolarCabbage.SuperEnd))]
        [HarmonyPrefix]
        public static bool SuperEnd(SolarCabbage __instance)
        {
            if (__instance.thePlantType == SolarEclipseCabbage.PlantID)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Solar), nameof(Solar.Awake))]
    public static class Solar_Awake_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Solar __instance)
        {
            if (Board.Instance == null)
                return;
            foreach (var plant in Board.Instance.boardEntity.plantArray)
                if ((plant != null && plant.thePlantType == SolarEclipseCabbage.PlantID) || SolarEclipse.Instance != null)
                {
                    if (SolarEclipse.Instance == null)
                    {
                        var solarEclipse = UnityEngine.Object.Instantiate(Core.SolarEclipse, new Vector3(-25f, 33f, 0f), Quaternion.identity, plant.board.transform).GetComponent<SolarEclipse>();
                        solarEclipse.targetPosition = new Vector3(-9.5f, 6.5f);
                    }
                    UnityEngine.Object.Destroy(__instance.gameObject);
                    return;
                }
        }
    }

    [HarmonyPatch(typeof(CreatePlant), nameof(CreatePlant.CheckMix))]
    public static class CreatePlant_CheckMix_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(CreatePlant __instance, ref Plant __result)
        {
            if (__result != null && __result.GetComponent<Plant>().thePlantType == PlantType.UltimateCabbage && UnityEngine.Random.Range(0, 100) <= 1)
            {
                var plant = __result.GetComponent<Plant>();
                __instance.SetPlant(plant.thePlantColumn, plant.thePlantRow, SolarEclipseCabbage.PlantID, null, default, true);
                plant.Die();
            }
        }
    }

    [HarmonyPatch(typeof(Lunar), nameof(Lunar.Update))]
    public static class Lunar_Update_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Lunar __instance)
        {
            if (__instance.red)
            {
                if (__instance.board == null)
                    return;
                if (Lawnf.GetPlantCount(PlantType.UltimateRedLunar, __instance.board) > 0 && Lawnf.GetPlantCount(SolarEclipseCabbage.PlantID, __instance.board) > 0 &&
                Lawnf.TravelAdvanced((AdvBuff)SolarEclipseCabbage.BuffID))
                {
                    var list = __instance.board.zombieArray.ToArray().Where(z => z != null && z.isMindControlled).ToList();
                    foreach (var z in list)
                    {
                        if (z != null && !z.IsDestroyed() && (z.GetData("SolarEclipse_added_zombie") is null || (z.GetData("SolarEclipse_added_zombie") is not null && z.GetData("SolarEclipse_added_zombie") is false)))
                        {
                            int health = CalculateAddValue(z.theHealth);
                            health = z.theFirstArmorHealth + health < 0 ? int.MaxValue - health : health;
                            z.theHealth += health;
                            z.theMaxHealth += health;
                            int firstArmor = CalculateAddValue(z.theFirstArmorHealth);
                            firstArmor = z.theFirstArmorHealth + firstArmor < 0 ? int.MaxValue - firstArmor : firstArmor;
                            z.theFirstArmorHealth += firstArmor;
                            z.theFirstArmorMaxHealth += firstArmor;
                            int secondArmor = CalculateAddValue(z.theFirstArmorHealth);
                            secondArmor = z.theFirstArmorHealth + secondArmor < 0 ? int.MaxValue - secondArmor : secondArmor;
                            z.theSecondArmorHealth += secondArmor;
                            z.theSecondArmorMaxHealth += secondArmor;
                            z.theAttackDamage *= 50;
                            z.SetData("SolarEclipse_added_zombie", true);
                            z.UpdateHealthText();
                        }
                    }
                }
            }
        }
        public static int CalculateAddValue(int health)
        {
            double x = health / 10000;
            double denominator = 1 + Math.Exp(0.2 * (x - 35));
            double y = x + 10.0 / denominator;
            return (int)y * 10000;
        }
    }

    [HarmonyPatch(typeof(Board), nameof(Board.Update))]
    public static class BoardPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Board __instance)
        {
            if (__instance == null)
                return;
            if (__instance.GetData("SolarEclipse_superTime") == null)
                __instance.SetData("SolarEclipse_superTime", __instance.solarMaxTime);
            else
            {
                if (GameAPP.theGameStatus == GameStatus.InGame)
                    __instance.SetData("SolarEclipse_superTime", __instance.GetData<float>("SolarEclipse_superTime") - Time.deltaTime);
                if (__instance.GetData<float>("SolarEclipse_superTime") <= 0f)
                {
                    if (SolarEclipse.Instance == null)
                    {
                        foreach (var plant in __instance.boardEntity.plantArray)
                        {
                            if (plant == null) continue;
                            if (plant.thePlantType != SolarEclipseCabbage.PlantID) continue;
                            if (plant.TryGetComponent<SolarEclipseCabbage>(out var cabbage) && cabbage != null)
                            {
                                cabbage.byTimer = true;
                                plant.anim.SetTrigger("super");
                            }
                        }
                    }
                    else if (__instance.ObjectExist<SolarEclipseCabbage>())
                        SolarEclipse.Instance.deathTime += 15f;

                    __instance.SetData("SolarEclipse_superTime", __instance.solarMaxTime);
                }
            }
        }
    }
}