using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using MelonLoader;
using Unity.VisualScripting;
using UnityEngine;

[assembly: MelonInfo(typeof(UltimateMachineChomper.Core), "UltimateMachineChomper", "1.0.0", "Administrator", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace UltimateMachineChomper
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatemachinechomper");
            CustomCore.RegisterCustomBullet<Bullet_superCherry, Bullet_machineCherry>((BulletType)Bullet_machineCherry.BulletID, ab.GetAsset<GameObject>("Bullet_machineCherry"));
            CustomCore.RegisterCustomPlant<UltimateChomper, UltimateMachineChomper>(UltimateMachineChomper.PlantID, ab.GetAsset<GameObject>("UltimateMachineChomperPrefab"),
                ab.GetAsset<GameObject>("UltimateMachineChomperPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.SuperMachineNut, (int)PlantType.SuperChomper),
                    ((int)PlantType.SuperChomper, (int)PlantType.SuperMachineNut)
                }, 2f, 0f, 1000, 16000, 0f, 350);
            // prefab.AddComponent<SuperMachineNut>();
            CustomCore.TypeMgrExtra.IsTallNut.Add((PlantType)UltimateMachineChomper.PlantID);
            CustomCore.TypeMgrExtra.IsNut.Add((PlantType)UltimateMachineChomper.PlantID);
            foreach (BucketType bucketType in Enum.GetValues(typeof(BucketType)))
                CustomCore.RegisterCustomUseItemOnPlantEvent((PlantType)UltimateMachineChomper.PlantID, bucketType, (plant) =>
                {
                    plant.Recover(PlantDataLoader.plantData[UltimateMachineChomper.PlantID].field_Public_Int32_0 * 0.2f);
                });
            CustomCore.AddUltimatePlant((PlantType)UltimateMachineChomper.PlantID);
            CustomCore.AddPlantAlmanacStrings(UltimateMachineChomper.PlantID, $"究极机械战神({UltimateMachineChomper.PlantID})",
                "机鱼公司的终极产品。\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>超级大嘴花+超级机械坚果</color>\n" +
                "<color=#3D1400>韧性：</color><color=red>16000，限伤16000</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>1000/1.75秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①免疫碾压并击退，血量可以超过韧性，出场时血量为20倍韧性，基础减伤为10%。\n" +
                "②为3x3范围的植物承伤。消耗铁器回复0.2倍韧性的血量。场上铁器消失时，回复（0.2/全场超级机械坚果+究极机械战神的数量）倍韧性的血量。\n" +
                "③攻击会对范围内每只僵尸造成啃咬，再吐出爆炸机械子弹。啃咬附加自身血量1%的伤害，并回复0.4倍伤害的血量。\n" +
                "④每40秒一次，或者累计受伤2倍韧性时，下一次啃咬可以吞噬非领袖僵尸并回复1倍韧性的血量，同时减少150限伤和增加1%减伤，限伤和减伤上限为4000和90%。\n" +
                "⑤不死机制：受到致命伤害后，5秒内血量最低1，随后冷却10秒，出场时进入冷却。</color>\n" +
                "<color=#3D1400>词条1：</color><color=red>嗜血如命：回血量x3。</color>\n" +
                "<color=#3D1400>词条2：</color><color=red>极速吞噬：吞噬消化时间降为15秒，瞬间完成吞噬条件降至累计受伤1倍韧性。</color>\n\n" +
                "<color=#3D1400>“我到底是机械，还是植物”究极机械战神反问自己，他不清楚自己的力量是属于身上的机械，还是属于自己的植物部分。但是在他挡在其他植物身前时，在他努力保护其他植物时，所有人知道，他的力量来自于他的内心，他善良的内心。</color>");
        }
    }

    [RegisterTypeInIl2Cpp]
    public class Bullet_machineCherry : MonoBehaviour
    {
        public static int BulletID = 1907;

        public Bullet_machineCherry() : base(ClassInjector.DerivedConstructorPointer<Bullet_machineCherry>()) => ClassInjector.DerivedConstructorBody(this);

        public Bullet_machineCherry(IntPtr i) : base(i)
        {
        }

        public Bullet_pea bullet => gameObject.GetComponent<Bullet_pea>();
    }

    [RegisterTypeInIl2Cpp]
    public class UltimateMachineChomper : MonoBehaviour
    {
        public int maxDamage = 16000; // attributeCount
        public float damageTimes = 0.9f; // theLilyType
        public float health = 320000; // 不用动
        public int totalDamage = 0;
        public bool isInit = false; // 抽象bug之游戏开始前Awake、Start都不会调用
        public TextMeshPro extraText;
        public TextMeshPro extraTextShadow;
        public List<GameObject> sprites = new List<GameObject>();
        public Zombie landSubmarine = null;

        public void Start()
        {
            if (plant != null && GameAPP.theGameStatus == GameStatus.InGame)
            {
                plant.shoot = transform.FindChild("Shoot");
                plant.SetAttackRange();
                if (plant.thePlantHealth == 16000)
                    plant.thePlantHealth = 20 * plant.thePlantMaxHealth;
                health = plant.thePlantHealth;
                if (plant.attributeCount != 0)
                    maxDamage = plant.attributeCount;
                maxDamage = Mathf.Max(4000, maxDamage);
                damageTimes = 1f - (17500 - maxDamage) / 150 * 0.01f; // = 1 - (13500 - 4000) / 150 * 0.01f = 0.9
                damageTimes = Mathf.Max(0.1f, damageTimes);
                damageTimes = (float)Math.Round(damageTimes, 2);
                plant.jigsawType.Add(JigsawType.Instead);
                plant.jigsawType.Add(JigsawType.Uncrashable);
                if (plant.killingText != null && plant.killingTextShadow != null)
                {
                    plant.killingText.gameObject.SetActive(false);
                    plant.killingTextShadow.gameObject.SetActive(false);
                }
                InitText();
                UpdateText();
                plant.UpdateText();
                sprites.Add(plant.gameObject.transform.FindChild("body/face_upper/Chomper_face_upper").gameObject);
                sprites.Add(plant.gameObject.transform.FindChild("body/body/Chomper_body").gameObject);
                sprites.Add(plant.gameObject.transform.FindChild("body/back/armor_back").gameObject);
                sprites.Add(plant.gameObject.transform.FindChild("body/head/armor_head").gameObject);
                sprites.Add(plant.gameObject.transform.FindChild("body/front/armor_front").gameObject);
                plant.range = new Vector2(3.5f, 3.5f);
                Vector2 newPos = new Vector2(plant.axis.transform.position.x + 0.75f, plant.axis.transform.position.y);
                plant.pos = newPos;
                totalDamage = 0;
                landSubmarine = null;
                isInit = true;
            }
        }

        public void Update()
        {
            if (!isInit) Start();
            if (plant != null && GameAPP.theGameStatus == GameStatus.InGame)
            {
                if (plant.targetZombie == null && GameAPP.theGameStatus == GameStatus.InGame)
                    plant.ChomperSearchZombie();
                UpdateText();
            }
        }

        public void InitText()
        {
            if (plant == null) return;
            if (plant.textHead == null && GameAPP.theGameStatus == GameStatus.InGame) plant.textHead = plant.gameObject.transform.FindChild("textHead").gameObject;
            if (extraText == null)
            {
                GameObject extraTextGO = new GameObject("ExtraText");
                extraText = extraTextGO.AddComponent<TextMeshPro>();
                extraTextGO.transform.SetParent(plant.textHead.transform);
                extraTextGO.transform.localPosition = new Vector3(0f, -0.5f, 0);
                extraText.font = GameAPP.font;
                String status = ""; 
                if (plant.undeadTimer > 0)
                    status = "不死:" + ((Mathf.FloorToInt(plant.undeadTimer) + 1) + "秒");
                else if (plant.undeadCD > 0)
                    status = "不死:" + ((Mathf.FloorToInt(plant.undeadCD) + 1) + "秒");
                else if (plant.undead)
                    status = "不死:就绪";
                extraText.text = $"限伤:{maxDamage}\n" +
                        $"减伤:{Mathf.FloorToInt(100 - (damageTimes * 100))}%\n" +
                        status + "\n" +
                        "吞噬:" + ((plant.attributeCountdown <= 0) ? "就绪" : ((Mathf.FloorToInt(plant.attributeCountdown) + 1) + "秒"));
                extraText.color = Color.cyan;
                extraText.alignment = (TextAlignmentOptions)514;
                extraText.fontSize = 2.1f;
                extraText.GetComponent<RectTransform>().sizeDelta = new Vector2(2f, 1f);
                extraText.sortingOrder = 32;
            }
            if (extraTextShadow == null)
            {
                GameObject extraTextShadowGO = new GameObject("ExtraTextShadow");
                extraTextShadow = extraTextShadowGO.AddComponent<TextMeshPro>();
                extraTextShadowGO.transform.SetParent(plant.textHead.transform);
                extraTextShadowGO.transform.localPosition = new Vector3(0.01f, -0.51f, 0);
                extraTextShadow.font = GameAPP.font;
                extraTextShadow.text = extraText.text;
                extraTextShadow.color = Color.black;
                extraTextShadow.alignment = (TextAlignmentOptions)514;
                extraTextShadow.fontSize = 2.1f;
                extraTextShadow.GetComponent<RectTransform>().sizeDelta = new Vector2(2f, 1f);
                extraTextShadow.sortingOrder = 31;
            }
        }

        public void SetTextPosition()
        {
            if (extraText != null && extraTextShadow != null)
            {
                extraText.transform.localPosition = new Vector3(0f, -0.5f, 0);
                extraTextShadow.transform.localPosition = new Vector3(0.01f, -0.51f, 0);
            }
        }

        public void UpdateText()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame)
            {
                if (extraText == null || extraTextShadow == null)
                    InitText();
                SetTextPosition();
                if (plant == null) return;
                String status = "";
                if (plant.undeadTimer > 0)
                    status = "不死:" + ((Mathf.FloorToInt(plant.undeadTimer) + 1) + "秒");
                else if (plant.undeadCD > 0)
                    status = "不死:" + ((Mathf.FloorToInt(plant.undeadCD) + 1) + "秒");
                else if (plant.undead)
                    status = "不死:就绪";
                extraText.text = $"限伤:{maxDamage}\n" +
                        $"减伤:{Mathf.FloorToInt(100 - (damageTimes * 100))}%\n" +
                        status + "\n" +
                        "吞噬:" + ((plant.attributeCountdown <= 0) ? "就绪" : ((Mathf.FloorToInt(plant.attributeCountdown) + 1) + "秒"));
                extraTextShadow.text = extraText.text;
                extraText.fontSize = 2.1f;
                extraTextShadow.fontSize = 2.1f;
            }
        }

        public void ReplaceSprite()
        {
            if (plant.thePlantHealth > plant.thePlantMaxHealth * 2 / 3)
            {
                foreach (GameObject sprite in sprites)
                {
                    sprite.GetComponent<SpriteRenderer>().enabled = true;
                    sprite.transform.GetChild(0).gameObject.SetActive(false);
                    sprite.transform.GetChild(1).gameObject.SetActive(false);
                }
            }
            else if (plant.thePlantHealth > plant.thePlantMaxHealth / 3 && plant.thePlantHealth <= plant.thePlantMaxHealth * 2 / 3)
            {
                foreach (GameObject sprite in sprites)
                {
                    sprite.GetComponent<SpriteRenderer>().enabled = false;
                    sprite.transform.GetChild(0).gameObject.SetActive(true);
                    sprite.transform.GetChild(1).gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (GameObject sprite in sprites)
                {
                    sprite.GetComponent<SpriteRenderer>().enabled = false;
                    sprite.transform.GetChild(0).gameObject.SetActive(false);
                    sprite.transform.GetChild(1).gameObject.SetActive(true);
                }
            }
        }
        
        /*public void Summon()
        {
            plant.Recover(plant.thePlantMaxHealth);
            if (landSubmarine != null)
            {
                landSubmarine.Die();
                landSubmarine = null;
            }
            Board board = plant.board;
            if (board == null) return;

            if (board.GetBoxType(plant.thePlantColumn, plant.thePlantRow) == BoxType.Water)
                landSubmarine = CreateZombie.Instance.SetZombieWithMindControl(plant.thePlantRow, ZombieType.SuperSubmarine, theX: plant.axis.transform.position.x + 1.0f).GetComponent<Zombie>();
            else
                landSubmarine = CreateZombie.Instance.SetZombieWithMindControl(plant.thePlantRow, ZombieType.LandSubmarine, theX: plant.axis.transform.position.x + 1.0f).GetComponent<Zombie>();

            if (landSubmarine != null)
            {
                landSubmarine.theMaxHealth = plant.thePlantHealth / 3;
                landSubmarine.theHealth = landSubmarine.theMaxHealth;

                CreateParticle.SetParticle((int)ParticleType.RandomCloud, new Vector2(landSubmarine.axis.transform.position.x, landSubmarine.axis.transform.position.y + 0.5f), plant.thePlantRow);
            }
        }*/

        public static int PlantID = 1916;

        public UltimateMachineChomper() : base(ClassInjector.DerivedConstructorPointer<UltimateMachineChomper>()) => ClassInjector.DerivedConstructorBody(this);

        public UltimateMachineChomper(IntPtr i) : base(i)
        {
        }

        public UltimateChomper plant => gameObject.GetComponent<UltimateChomper>();
    }

    [HarmonyPatch(typeof(UltimateChomper))]
    public static class UltimateChomper_Patch
    {
        [HarmonyPatch(nameof(UltimateChomper.BiteEvent))]
        [HarmonyPrefix]
        public static bool Prefix(UltimateChomper __instance)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                if (__instance.targetZombie != null && !__instance.targetZombie.isMindControlled)
                {
                    // 对当前目标僵尸执行咬噬
                    __instance.Bite(__instance.targetZombie);
                }

                // 创建咬噬检测区域
                Vector2 biteCenter = new Vector2(__instance.pos.x, __instance.pos.y);
                Vector2 biteSize = new Vector2(__instance.range.x, __instance.range.y);
                int zombieLayerMask = __instance.zombieLayer;

                // 检测区域内所有僵尸
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                    biteCenter,
                    biteSize,
                    0f,
                    zombieLayerMask
                );

                // 遍历检测到的僵尸
                foreach (Collider2D collider in hitColliders)
                {
                    // 尝试获取僵尸组件
                    Zombie zombie = collider.GetComponent<Zombie>();
                    if (zombie == null) continue;

                    // 检查是否在同一行
                    if (zombie.theZombieRow != __instance.thePlantRow) continue;

                    // 排除当前目标僵尸
                    if (zombie == __instance.targetZombie) continue;

                    // 验证是否可被咬噬
                    if (__instance.CheckZombie(zombie))
                    {
                        // 执行咬噬动作
                        __instance.Bite(zombie);
                    }
                }

                // 清除当前目标
                __instance.targetZombie = null;

                float dynamicDamage = 1000 + __instance.thePlantHealth * 0.01f;
                dynamicDamage = Mathf.Max(__instance.attackDamage, dynamicDamage);

                // 旅行模式伤害加成
                if (Lawnf.TravelAdvanced(62))
                    dynamicDamage *= 1.5f;

                __instance.Recover(0.4f * dynamicDamage);

                // 播放咬噬音效
                GameAPP.PlaySound(49, 0.5f, 1.0f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(UltimateChomper.Bite))]
        [HarmonyPrefix]
        public static bool Prefix(UltimateChomper __instance, ref Zombie zombie)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                UltimateMachineChomper component = __instance.GetComponent<UltimateMachineChomper>();
                int value = Lawnf.TravelUltimate(1) ? __instance.thePlantMaxHealth : __instance.thePlantMaxHealth * 2;
                bool flag = component != null && component.totalDamage >= value;
                if (flag)
                {
                    component.maxDamage -= 150;
                    component.maxDamage = Mathf.Max(4000, component.maxDamage);
                    __instance.attributeCount = component.maxDamage;
                    component.damageTimes -= 0.01f;
                    component.damageTimes = Mathf.Max(0.1f, component.damageTimes);
                    component.damageTimes = (float)Math.Round(component.damageTimes, 2);
                    // __instance.theLilyType = (PlantType)((component.damageTimes * 100) + UltimateMachineChomper.addValueConst);
                    /*MelonLogger.Msg((int)__instance.theLilyType);
                    MelonLogger.Msg(component.damageTimes);*/
                    component.totalDamage = 0;
                    __instance.attributeCountdown = 0f;
                    __instance.UpdateText();
                }
                if ((__instance.attributeCountdown == 0f) && !TypeMgr.IsBossZombie(zombie.theZombieType))
                {
                    // 直接杀死僵尸
                    zombie.Die(2); // 2表示吞噬死亡

                    // 设置消化倒计时
                    __instance.attributeCountdown = Lawnf.TravelUltimate(1) ? 15f : 40f;

                    // 触发恢复动画

                    // 进入无敌状态
                    __instance.undead = true;
                    __instance.undeadTimer = 0f;
                    __instance.Recover((float)__instance.thePlantMaxHealth);

                    // 播放音效并返回
                    GameAPP.PlaySound(49, 0.5f, 1.0f);
                    return false;
                }

                // 计算动态伤害
                float dynamicDamage = 1000 + __instance.thePlantHealth * 0.01f;
                dynamicDamage = Mathf.Max(__instance.attackDamage, dynamicDamage);

                // 旅行模式伤害加成
                if (Lawnf.TravelAdvanced(62))
                    dynamicDamage *= 1.5f;

                // 对僵尸造成伤害
                zombie.TakeDamage(DmgType.NormalAll, (int)dynamicDamage, false);

                // 播放音效
                GameAPP.PlaySound(49, 0.5f, 1.0f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(UltimateChomper.Recover))]
        [HarmonyPostfix]
        public static void Postfix(UltimateChomper __instance, ref float health)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                UltimateMachineChomper component = __instance.GetComponent<UltimateMachineChomper>();
                if (component != null)
                {
                    if (Lawnf.TravelUltimate(0))
                        health *= 3f;
                    component.health += health;
                    component.health = Mathf.Clamp(component.health, 0, 1_000_000_000);
                    // MelonLogger.Msg(component.health);
                    __instance.thePlantHealth = (int)component.health;
                    __instance.UpdateText();
                    component.ReplaceSprite();
                }
            }
        }

        [HarmonyPatch(nameof(UltimateChomper.DecreaseHealth))]
        [HarmonyPrefix]
        public static bool Prefix_DecreaseHealth(UltimateChomper __instance)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                if (__instance.undeadCD > 0f)
                {
                    __instance.undeadCD -= Time.deltaTime;
                    if (__instance.undeadCD <= 0f)
                    {
                        // 冷却结束进入无敌状态
                        __instance.undeadTimer = 0f;
                        __instance.undead = true;
                    }
                }

                // 处理无敌持续时间
                if (__instance.undeadTimer > 0f)
                {
                    __instance.undeadTimer -= Time.deltaTime;
                    if (__instance.undeadTimer <= 0f)
                    {
                        // 无敌状态结束
                        __instance.undeadTimer = 0f;
                        __instance.undead = false;
                    }
                }
                __instance.UpdateText();
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(UltimateChomper.TakeDamage))]
        [HarmonyPrefix]
        public static void Prefix_TakeDamage(UltimateChomper __instance, ref int damage)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                UltimateMachineChomper component = __instance.GetComponent<UltimateMachineChomper>();
                damage = Mathf.Min(damage, component.maxDamage);
                float tmp = damage * component.damageTimes;
                damage = (int)tmp;
                component.health = __instance.thePlantHealth;
                component.totalDamage += damage;
            }
        }

        [HarmonyPatch(nameof(UltimateChomper.TakeDamage))]
        [HarmonyPostfix]
        public static void Postfix_TakeDamage(UltimateChomper __instance)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                UltimateMachineChomper component = __instance.GetComponent<UltimateMachineChomper>();
                if (component == null) return;
                component.ReplaceSprite();
            }
        }

        [HarmonyPatch(nameof(UltimateChomper.UpdateText))]
        [HarmonyPostfix]
        public static void Postfix_UpdateText(UltimateChomper __instance)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateMachineChomper.PlantID && GameAPP.theGameStatus == GameStatus.InGame && __instance.killingText != null && __instance.killingTextShadow != null)
            {
                __instance.killingText.gameObject.SetActive(false);
                __instance.killingTextShadow.gameObject.SetActive(false);
                UltimateMachineChomper component = __instance.GetComponent<UltimateMachineChomper>();
                if (component != null)
                {
                    component.UpdateText();
                    component.extraText.gameObject.SetActive(true);
                    component.extraTextShadow.gameObject.SetActive(true);
                }
            }
        }

        [HarmonyPatch(nameof(UltimateChomper.AnimShoot))]
        [HarmonyPrefix]
        public static bool Prefix_AnimShoot(UltimateChomper __instance)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                Transform shootPoint = __instance.shoot;
                if (shootPoint == null)
                    return false;

                // 获取射击点位置
                Vector3 shootPosition = shootPoint.position;

                // 创建子弹实例
                CreateBullet bulletCreator = CreateBullet.Instance;
                if (bulletCreator == null)
                    return false;

                // 创建特殊子弹
                Bullet bullet = bulletCreator.SetBullet(
                    shootPosition.x,
                    shootPosition.y,
                    __instance.thePlantRow,
                    (BulletType)Bullet_machineCherry.BulletID,
                    BulletMoveWay.MoveRight,
                    false
                );

                if (bullet == null)
                    return false;

                // 设置子弹伤害
                bullet.Damage = __instance.attackDamage;

                // 随机选择射击音效（3或4）
                int soundId = UnityEngine.Random.Range(3, 5);

                GameAPP.PlaySound(soundId, 0.5f, 1.0f);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SuperChomper), nameof(SuperChomper.ReplaceSprite))]
    public class SuperChomper_ReplaceSprite
    {
        [HarmonyPrefix]
        public static bool Prefix(SuperChomper __instance)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                UltimateMachineChomper component = __instance.GetComponent<UltimateMachineChomper>();
                if (component == null) return false;
                component.ReplaceSprite();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Chomper))]
    public class Chomper_SetAttackRange
    {
        [HarmonyPatch(nameof(Chomper.SetAttackRange))]
        [HarmonyPrefix]
        public static bool Prefix(Chomper __instance)
        {
            if ((int)__instance.thePlantType == UltimateMachineChomper.PlantID)
            {
                __instance.range = new Vector2(3.5f, 3.5f);
                Vector2 newPos = new Vector2(__instance.axis.transform.position.x + 0.75f, __instance.axis.transform.position.y);
                __instance.pos = newPos;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_superCherry), nameof(Bullet_superCherry.HitZombie))]
    public class Bullet_superCherry_HitZombie
    {
        [HarmonyPrefix]
        public static bool Prefix(Bullet_superCherry __instance, ref Zombie zombie)
        {
            if ((int)__instance.theBulletType == Bullet_machineCherry.BulletID)
            {
                if (zombie == null)
                {
                    return false;
                }

                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(__instance.transform.position, 2.5f, zombie.zombieLayer);

                int bulletColumn = Mouse.Instance.GetColumnFromX(__instance.transform.position.x) + 1;

                foreach (Collider2D collider in hitColliders)
                {
                    if (collider is not null && !collider.IsDestroyed() && collider.TryGetComponent<Zombie>(out var z) && z is not null && !z.isMindControlled && !z.IsDestroyed())
                    {

                        int zombieColumn = Mouse.Instance.GetColumnFromX(z.axis.transform.position.x);
                        // MelonLogger.Msg(bulletColumn - zombieColumn);
                        if (Mathf.Abs(bulletColumn - zombieColumn) <= 1 && Mathf.Abs(__instance.theBulletRow - z.theZombieRow) <= 1)
                        {
                            // 对僵尸造成伤害
                            z.TakeDamage(DmgType.NormalAll, __instance.Damage, false);
                        }
                    }
                }

                // 获取子弹位置
                Transform bulletTransform = __instance.transform;
                if (bulletTransform == null)
                    return false;

                Vector3 hitPosition = bulletTransform.position;

                // 创建命中粒子效果
                CreateParticle.SetParticle((int)ParticleType.BombKirov, hitPosition, __instance.theBulletRow, true);

                // 播放命中音效
                GameAPP.PlaySound(40, 0.2f, 1.0f);

                // 销毁子弹
                __instance.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bucket), nameof(Bucket.Die))]
    public class Bucket_Die
    {
        [HarmonyPrefix]
        public static void Prefix(Bucket __instance)
        {
            if (Board.Instance != null && Board.Instance.plantArray != null)
            {
                List<Plant> plants = new List<Plant>();
                int num = 0;
                for (int i = 0; i < Board.Instance.plantArray.Count; i++)
                {
                    Plant p = Board.Instance.plantArray[i];
                    if (p != null && (((int)p.thePlantType == UltimateMachineChomper.PlantID) || (p.thePlantType == PlantType.SuperMachineNut)))
                    {
                        num++;
                        if ((int)p.thePlantType == UltimateMachineChomper.PlantID)
                            plants.Add(p);
                    }
                }
                if (num > 0)
                {
                    for (int i = 0; i < plants.Count; i++)
                    {
                        Plant p = plants[i];
                        p?.Recover((PlantDataLoader.plantData[UltimateMachineChomper.PlantID].field_Public_Int32_0 * 0.2f) / num);
                    }
                }
            }
        }
    }
}