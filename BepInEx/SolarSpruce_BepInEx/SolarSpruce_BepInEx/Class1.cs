using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace SolarSpruce
{
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

    [BepInPlugin("salmon.solarspruce", "SolarSpruce", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "ultimatesolarspruce");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_shulkSolarSpruce>();
            ClassInjector.RegisterTypeInIl2Cpp<SolarSpruce>();
            CustomCore.RegisterCustomBullet<Bullet_shulkLeaf_ultimate, Bullet_shulkSolarSpruce>((BulletType)Bullet_shulkSolarSpruce.BulletID, ab.GetAsset<GameObject>("SolarSpruceBulletPrefab"));
            CustomCore.RegisterCustomPlant<UltimateSpruce, SolarSpruce>(SolarSpruce.PlantID, ab.GetAsset<GameObject>("SolarSprucePrefab"),
                ab.GetAsset<GameObject>("SolarSprucePreview"), new List<(int, int)> { ((int)PlantType.UltimateCabbage, (int)PlantType.SpruceShooter), ((int)PlantType.SpruceShooter, (int)PlantType.UltimateCabbage) }, 1.5f, 0, 40, 300, 0f, 550);
            SolarSpruce.RegisterSuperSkill();
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)SolarSpruce.PlantID);
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, (int)PlantType.Cabbagepult, SolarSpruce.PlantID);
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, SolarSpruce.PlantID, (int)PlantType.Cabbagepult);
            CustomCore.AddPlantAlmanacStrings(SolarSpruce.PlantID, $"究极太阳神云杉({SolarSpruce.PlantID})", "冰与火之舞？或许吧。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>转换配方：</color><color=red>卷心菜投手←→云杉弓手</color>\n<color=#3D1400>伤害：</color><color=red>40/0.02s</color>\n<color=#3D1400>特点：</color><color=red>究极太阳神卷心菜亚种，免疫沉默和冻结，不会主动召唤太阳，每1.5秒发射1发子弹，该子弹每命中僵尸15次掉落5点阳光。每次攻击如果阳光大于15000，则消耗200阳光为这发子弹伤害变为原伤害的3倍。增伤时先计算伤害×3，再计算金光闪闪词条效果。</color>\n<color=#3D1400>词条1：</color><color=red>金光闪闪：每发子弹会消耗超过15000阳光部分0.3%的阳光，使该子弹增加（2x消耗阳光）的伤害。</color>\n<color=#3D1400>词条2：</color><color=red>人造太阳：子弹掉落阳光数增加至15。</color>\n<color=#3D1400>大招：</color><color=red>消耗1000金钱。如果场上不存在太阳，则生成一个持续时间30秒的太阳，如果存在太阳，则为太阳增加15秒持续时间。</color>\n\n<color=#3D1400>究极太阳神云杉曾被问道是如何控制体内的冰与火之力的，他是这么说的：“只要经常玩《冰与火之舞》，你也可以获得这种力量！”后来这期节目因为《冰与火之舞》不是甲方所以没有播出。</color>");
        }
    }

    public class SolarSpruce : MonoBehaviour
    {
        public static int PlantID = 1904;
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
                    damage += (2 * outSun); //伤害增加超出部分*3%
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
            // MelonLogger.Msg(Solar.Instance.deathTime);
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

    [HarmonyLib.HarmonyPatch(typeof(Bullet_shulkLeaf_ultimate), "HitZombie")]
    public static class Bullet_shulkLeaf_ultimate_HitZombie_Patch
    {
        public static bool Prefix(Bullet_shulkLeaf_ultimate __instance, Zombie zombie)
        {
            if (__instance.theBulletType == (BulletType)Bullet_shulkSolarSpruce.BulletID)
            {
                if (zombie == null) return false;

                // 造成伤害
                zombie.TakeDamage(
                    theDamageType: DmgType.Shieldless,
                    theDamage: __instance.Damage,
                    false
                );

                __instance.PlaySound(zombie);
                __instance.hitTimes++;

                if (__instance.hitTimes % 15 == 0)
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
}