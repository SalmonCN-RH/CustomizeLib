using CustomizeLib.MelonLoader;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Unity.VisualScripting;
using UnityEngine;
using static MelonLoader.MelonLogger;

[assembly: MelonInfo(typeof(SolarSpruce.Core), "SolarSpruce", "1.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace SolarSpruce
{
    [RegisterTypeInIl2Cpp]
    public class Bullet_shulkSolarSpruce : MonoBehaviour
    {
        public static int BulletID = 1901;
        public Bullet_shulkSolarSpruce() : base(ClassInjector.DerivedConstructorPointer<Bullet_shulkSolarSpruce>()) => ClassInjector.DerivedConstructorBody(this);

        public Bullet_shulkSolarSpruce(IntPtr i) : base(i)
        {
        }
        public void Start()
        {
            bullet.penetrationTimes = 2147483647;
        }

        public Bullet_shulkLeaf_ultimate bullet => gameObject.GetComponent<Bullet_shulkLeaf_ultimate>();
    }

    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatesolarspruce");
            CustomCore.RegisterCustomBullet<Bullet_shulkLeaf_ultimate, Bullet_shulkSolarSpruce>((BulletType)Bullet_shulkSolarSpruce.BulletID, ab.GetAsset<GameObject>("SolarSpruceBulletPrefab"));
            CustomCore.RegisterCustomPlant<UltimateSpruce, SolarSpruce>(SolarSpruce.PlantID, ab.GetAsset<GameObject>("SolarSprucePrefab"),
                ab.GetAsset<GameObject>("SolarSprucePreview"), [((int)PlantType.UltimateCabbage, (int)PlantType.SpruceShooter), ((int)PlantType.SpruceShooter, (int)PlantType.UltimateCabbage)], 1.5f, 0, 60, 300, 0f, 550);
            SolarSpruce.RegisterSuperSkill();
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)SolarSpruce.PlantID);
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, (int)PlantType.Cabbagepult, SolarSpruce.PlantID);
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, SolarSpruce.PlantID, (int)PlantType.Cabbagepult);
            CustomCore.AddPlantAlmanacStrings(SolarSpruce.PlantID, $"究极太阳神云杉({SolarSpruce.PlantID})", "冰与火之舞？或许吧。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>转换配方：</color><color=red>卷心菜投手←→云杉弓手</color>\n<color=#3D1400>伤害：</color><color=red>60/0.02s</color>\n<color=#3D1400>特点：</color><color=red>究极太阳神卷心菜亚种。免疫沉默和冻结，不会自主召唤太阳。每1.5秒发射1发子弹，该子弹每命中15次掉落5点阳光。如果阳光大于15000，则下一次攻击消耗200阳光使该子弹伤害x3。</color>\n<color=#3D1400>词条1：</color><color=red>金光闪闪：每发子弹会消耗超过15000阳光部分0.3%的阳光，使该子弹增加（5x消耗阳光）的伤害。</color>\n<color=#3D1400>词条2：</color><color=red>人造太阳：子弹掉落阳光数增加至15。</color>\n<color=#3D1400>连携词条：</color><color=red>星神合一：究极杨桃大帝和究极太阳神卷心菜或究极太阳神云杉的数量均不小于10时：究极太阳神云杉子弹为范围伤害，太阳持续时间无限且伤害x5，固定每3秒召唤太阳神流星，伤害为（1800+1800x大帝数量）x（1+0.5x太阳神和太阳神云杉数量），并分裂180个400伤害的子弹，并掉落1250阳光。</color>\n<color=#3D1400>大招：</color><color=red>消耗1000金钱。如果场上不存在太阳，则生成一个持续时间30秒的太阳，如果存在太阳，则为太阳增加15秒持续时间。</color>\n\n<color=#3D1400>究极太阳神云杉曾被问道是如何控制体内的冰与火之力的，他是这么说的：“只要经常玩《冰与火之舞》，你也可以获得这种力量！”后来这期节目因为《冰与火之舞》不是甲方所以没有播出。</color>");
        }
    }

    [RegisterTypeInIl2Cpp]
    public class SolarSpruce : MonoBehaviour
    {
        public static int PlantID = 1904;
        public GameObject back = null;
        public SolarSpruce() : base(ClassInjector.DerivedConstructorPointer<SolarSpruce>()) => ClassInjector.DerivedConstructorBody(this);

        public SolarSpruce(IntPtr i) : base(i)
        {
        }

        public Bullet AnimShoot_SolarSpruce()
        {
            Vector3 shootPos = plant.shoot.position;
            // 空值检查：确保plant、shoot和CreateBullet实例存在
            
            // 创建特殊子弹
            Bullet bullet = CreateBullet.Instance.SetBullet(
                x: shootPos.x + 0.1f,
                y: shootPos.y,
                theRow: plant.thePlantRow,
                theBulletType: (BulletType)Bullet_shulkSolarSpruce.BulletID,
                theMovingWay: 16,
                isZombieBullet: false
            );

            int damage = plant.attackDamage;
            if (GameAPP.board.GetComponent<Board>().theSun > 15000)
            {
                damage *= 3;
                GameAPP.board.GetComponent<Board>().UseSun(200);
            }
            if (Lawnf.TravelUltimate(22) && GameAPP.board.GetComponent<Board>().theSun > 15000)
            {
                int outSun = GameAPP.board.GetComponent<Board>().theSun - 15000; //求出超过15000的阳光数量
                outSun = (int)(outSun * 0.003); //超出部分*3%
                if (outSun > 0)
                {
                    GameAPP.board.GetComponent<Board>().UseSun(outSun); //使用超出部分*3%的阳光
                    damage += (5 * outSun); //伤害增加超出部分*3%
                }
            }

            // 配置子弹属性
            bullet.Damage = damage;
            bullet.from = plant;  // 设置发射源

            int soundId = UnityEngine.Random.Range(3, 5);
            GameAPP.PlaySound(
                theSoundID: soundId,
                theVolume: 0.5f,
                pitch: 1.0f
            );

            return bullet;  // 原始方法固定返回null
        }

        public void Start()
        {
            plant.shoot = plant.gameObject.transform.FindChild("body").FindChild("zi_dan").FindChild("Shoot");
            if (plant.energyText != null)
                plant.energyText.gameObject.SetActive(false);
            if (plant.energyTextShadow != null)
                plant.energyTextShadow.gameObject.SetActive(false);
            back = plant.gameObject.transform.FindChild("body/t1/t1").gameObject;
            // MelonLogger.Msg(Solar.Instance.deathTime);
        }

        public void Update()
        {
            if (back != null)
            {
                if (GameAPP.theGameStatus != GameStatus.Almanac && plant != null && !plant.isCrashed)
                    back.transform.Rotate(0, 0, Time.deltaTime * -90f);
                else
                    back.transform.Rotate(0, 0, Time.unscaledDeltaTime * -90f);
            }
        }

        public static void RegisterSuperSkill()
        {
            CustomCore.RegisterSuperSkill(SolarSpruce.PlantID, (plant) => 1000, (plant) =>
            {
                plant.Recover((float)plant.thePlantMaxHealth);
                plant.flashCountDown = 3f;
                if (Solar.Instance == null)
                {
                    GameObject solarPrefab = GameAPP.itemPrefab[46]; // 索引46

                    // 设置生成位置
                    Vector3 spawnPos = new Vector3(-25f, 33f, 0f);

                    // 实例化对象
                    GameObject solarObj = GameObject.Instantiate(
                        solarPrefab,
                        spawnPos,
                        Quaternion.identity,
                        GameAPP.board.GetComponent<Board>().transform
                    );

                    // 获取组件并初始化
                    Solar solar = solarObj.GetComponent<Solar>();
                    solar?.SetDamage();

                    // 播放音效
                    GameAPP.PlaySound(
                        theSoundID: 95,
                        theVolume: 0.5f,
                        pitch: 1.0f
                    );
                    Solar.Instance = solar;
                    solar.deathTime = 30;
                }
                else
                {
                    Solar.Instance.deathTime += 15;
                }
            });
        }

        public UltimateSpruce plant => gameObject.GetComponent<UltimateSpruce>();
    }

    [HarmonyLib.HarmonyPatch(typeof(Bullet_shulkLeaf_ultimate), nameof(Bullet_shulkLeaf_ultimate.HitZombie))]
    public static class Bullet_shulkLeaf_ultimate_HitZombie_Patch
    {
        public static bool Prefix(Bullet_shulkLeaf_ultimate __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == (BulletType)Bullet_shulkSolarSpruce.BulletID)
            {
                if (Lawnf.TravelAdvanced(45) &&
                Lawnf.GetPlantCount(PlantType.UltimateStar, Board.Instance) + Lawnf.GetPlantCount(PlantType.UltimateBlover, Board.Instance) >= 10 &&
                Lawnf.GetPlantCount(PlantType.UltimateCabbage, Board.Instance) >= 10)
                {
                    foreach (RaycastHit2D hit in Physics2D.RaycastAll(__instance.transform.position, Vector2.zero))
                    {
                        if (hit.collider.gameObject.TryGetComponent<Zombie>(out var z) && z != null && !z.IsDestroyed() && !TypeMgr.IsAirZombie(z.theZombieType) && z.theStatus != ZombieStatus.Miner_digging && !z.isMindControlled)
                        {
                            // 造成伤害
                            z.TakeDamage(
                                theDamageType: DmgType.Shieldless,
                                theDamage: __instance.Damage,
                                false
                            );
                            __instance.hitTimes++;
                        }
                    }
                }
                else
                {
                    zombie.TakeDamage(
                        theDamageType: DmgType.Shieldless,
                        theDamage: __instance.Damage,
                        false
                    );
                    __instance.hitTimes++;
                }

                if (zombie != null)
                    __instance.PlaySound(zombie);

                if (__instance.hitTimes >= 15)
                {
                    __instance.hitTimes = 0;
                    // MelonLogger.Msg(Lawnf.TravelUltimate(23));
                    if (Lawnf.TravelUltimate(23))
                    {
                        CreateItem.Instance.SetCoin(0, 0, 2, 0, zombie.axis.transform.position, false);
                        // sun.GetComponent<CoinSun>().sunPrice = 15;
                        // MelonLogger.Msg(sun.GetComponent<CoinSun>().sunPrice);
                    }
                    else
                    {
                        CreateItem.Instance.SetCoin(0, 0, 13, 0, zombie.axis.transform.position, false);
                    }
                }

                return false;
            }
            return true;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Lawnf), nameof(Lawnf.GetPlantCount), new Type[] { typeof(PlantType), typeof(Board) })]
    public static class Lawnf_GetPlantCount_Patch
    {
        [HarmonyLib.HarmonyPostfix]
        public static void Postfix(ref PlantType theSeedType, ref int __result)
        {
            if (theSeedType == PlantType.UltimateCabbage)
            {
                __result += Lawnf.GetPlantCount((PlantType)SolarSpruce.PlantID, Board.Instance);
            }
        }
    }

    /*[HarmonyLib.HarmonyPatch(typeof(Plant), nameof(Plant.SearchZombie))]
    public class Plant_SearchZombie_Patch
    {
        public static bool Prefix(Plant __instance, ref GameObject __result)
        {
            if ((int)__instance.thePlantType == SolarSpruce.PlantID)
            {
                List<Zombie> zombies = __instance.board?.zombieArray.ToArray().ToList<Zombie>();
                if (zombies == null) __result = null;

                // 遍历所有僵尸
                foreach (Zombie zombie in zombies)
                {
                    if (zombie == null) continue;

                    // 获取僵尸位置
                    Vector3 zombiePos = zombie.axis?.position ?? Vector3.zero;

                    // 位置条件检查
                    if (zombiePos.x < __instance.vision)
                    {
                        // 特殊僵尸类型检查
                        if (__instance.SearchUniqueZombie(zombie))
                        {
                            __result = zombie.gameObject;
                            return false;
                        }
                    }
                }

                __result = null;
                return false;
            }
            return true;
        }
    }*/
}