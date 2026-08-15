using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateHypnoMagnet.BepInEx
{
    [BepInPlugin("salmon.ultimatehypnomagnet", "UltimateHypnoMagnet", "1.0")]
    public class Core : BasePlugin
    {
        // Token: 0x06000001 RID: 1 RVA: 0x000020C0 File Offset: 0x000002C0
        public override void Load()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            ClassInjector.RegisterTypeInIl2Cpp<UltimateHypnoMagnet>();
            Console.OutputEncoding = Encoding.UTF8;
            AssetBundle assetBundle = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "ultimatehypnomagnet");
            CustomCore.RegisterCustomPlant<UltimateMagnet, UltimateHypnoMagnet>(UltimateHypnoMagnet.PlantID, Extensions.GetAsset<GameObject>(assetBundle, "UltimateHypnoMagnetPrefab"), Extensions.GetAsset<GameObject>(assetBundle, "UltimateHypnoMagnetPreview"), new List<ValueTuple<int, int>>
            {
                new ValueTuple<int, int>(944, 8),
                new ValueTuple<int, int>(8, 944)
            }, 0.5f, 0f, 300, 300, 0f, 450);
            CustomCore.TypeMgrExtra.IsMagnetPlants.Add(UltimateHypnoMagnet.PlantID);
            CustomCore.AddFusion(944, UltimateHypnoMagnet.PlantID, 2);
            CustomCore.AddFusion(944, 2, UltimateHypnoMagnet.PlantID);
            int plantID = UltimateHypnoMagnet.PlantID;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
            defaultInterpolatedStringHandler.AppendLiteral("魅惑磁力菇王(");
            defaultInterpolatedStringHandler.AppendFormatted<int>(UltimateHypnoMagnet.PlantID);
            defaultInterpolatedStringHandler.AppendLiteral(")");
            CustomCore.AddPlantAlmanacStrings(plantID, defaultInterpolatedStringHandler.ToStringAndClear(), "磁性极强，快速吸引铁器并将其转化为对应的魅惑僵尸。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>转换配方：</color><color=red>樱桃炸弹←→魅惑菇</color>\n<color=#3D1400>特点：</color><color=red>磁力菇王亚种。拥有磁力菇王和魅惑磁力菇的特点，吸取间隔为0.5秒，吸取铁器3秒后转化为对应魅惑僵尸，每0.5秒额外伤害吸取范围内所有的橄榄三叉戟类僵尸或黑橄榄类僵尸。</color>\n<color=#3D1400>词条1：</color><color=red>电磁涡轮：吸取半径翻倍。</color>\n<color=#3D1400>词条2：</color><color=red>万磁王：一次性可吸3个铁器，且召唤的僵尸血量和啃咬伤害+300%。</color>\n<color=#3D1400>词条3：</color><color=red>精兵强将：魅惑磁力菇王将召唤随机究极僵尸。</color>\n\n<color=#3D1400>作为植物界的“大魔术师”，魅惑磁力菇王最精湛的演技就是将铁器变成僵尸，不过被召唤的僵尸都以奇怪的姿势瘫在了舞台上。</color>");
            IEnumerator enumerator = Enum.GetValues(typeof(BucketType)).GetEnumerator();
            {
                while (enumerator.MoveNext())
                {
                    BucketType bucketType = (BucketType)enumerator.Current;
                    CustomCore.RegisterCustomUseItemOnPlantEvent(UltimateHypnoMagnet.PlantID, bucketType, delegate (Plant plant)
                    {
                        bool flag = plant != null && plant.thePlantType == UltimateHypnoMagnet.PlantID;
                        if (flag)
                        {
                            UltimateHypnoMagnet component = plant.GetComponent<UltimateHypnoMagnet>();
                            component.SpawnZombie(bucketType, null);
                        }
                    });
                }
            }
        }
    }

    public class UltimateHypnoMagnet : MonoBehaviour
    {
        // Token: 0x06000005 RID: 5 RVA: 0x00002062 File Offset: 0x00000262
        public UltimateHypnoMagnet()
            : base(ClassInjector.DerivedConstructorPointer<UltimateHypnoMagnet>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002077 File Offset: 0x00000277
        public UltimateHypnoMagnet(IntPtr i)
            : base(i)
        {
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000022A8 File Offset: 0x000004A8
        public void Start()
        {
            bool flag = UltimateHypnoMagnet.types == null;
            if (flag)
            {
                UltimateHypnoMagnet.types = new List<ZombieType>();
                foreach (ZombieType zombieType in GameAPP.resourcesManager.allZombieTypes)
                {
                    bool flag2 = TypeMgr.UltimateZombie(zombieType);
                    if (flag2)
                    {
                        UltimateHypnoMagnet.types.Add(zombieType);
                    }
                }
                foreach (ZombieType zombieType2 in TypeMgr.UltiZombie_level_a)
                {
                    bool flag3 = !UltimateHypnoMagnet.types.Contains(zombieType2);
                    if (flag3)
                    {
                        UltimateHypnoMagnet.types.Add(zombieType2);
                    }
                }
                foreach (ZombieType zombieType3 in TypeMgr.UltiZombie_level_b)
                {
                    bool flag4 = !UltimateHypnoMagnet.types.Contains(zombieType3);
                    if (flag4)
                    {
                        UltimateHypnoMagnet.types.Add(zombieType3);
                    }
                }
                foreach (ZombieType zombieType4 in TypeMgr.UltiZombie_level_c)
                {
                    bool flag5 = !UltimateHypnoMagnet.types.Contains(zombieType4);
                    if (flag5)
                    {
                        UltimateHypnoMagnet.types.Add(zombieType4);
                    }
                }
                bool flag6 = UltimateHypnoMagnet.types.Contains((ZombieType)320);
                if (flag6)
                {
                    UltimateHypnoMagnet.types.Remove((ZombieType)320);
                }
                bool flag7 = UltimateHypnoMagnet.types.Contains((ZombieType)319);
                if (flag7)
                {
                    UltimateHypnoMagnet.types.Remove((ZombieType)319);
                }
                bool flag8 = UltimateHypnoMagnet.types.Contains((ZombieType)318);
                if (flag8)
                {
                    UltimateHypnoMagnet.types.Remove((ZombieType)318);
                }
                bool flag9 = UltimateHypnoMagnet.types.Contains((ZombieType)28);
                if (flag9)
                {
                    UltimateHypnoMagnet.types.Remove((ZombieType)28);
                }
                bool flag10 = UltimateHypnoMagnet.types.Contains((ZombieType)226);
                if (flag10)
                {
                    UltimateHypnoMagnet.types.Remove((ZombieType)226);
                }
                bool flag11 = UltimateHypnoMagnet.types.Contains((ZombieType)43);
                if (flag11)
                {
                    UltimateHypnoMagnet.types.Remove((ZombieType)43);
                }
                for (int i = 0; i < UltimateHypnoMagnet.types.Count; i++)
                {
                    bool flag12 = TypeMgr.IsBossZombie(UltimateHypnoMagnet.types[i]);
                    if (flag12)
                    {
                        UltimateHypnoMagnet.types.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x000024F0 File Offset: 0x000006F0
        public void SpawnZombie(BucketType bucket, UltimateMagnet.AttrackedBucket item = null)
        {
            if (!Lawnf.TravelAdvanced((AdvBuff)1))
            {
                Zombie zombie = null;
                switch ((int)bucket)
                {
                    case 0:
                        {
                            int num = Random.Range(0, 3);
                            if (num == 0)
                            {
                                zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)4, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                            }
                            else if (num == 1)
                            {
                                zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)106, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                            }
                            else
                            {
                                zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)114, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                            }
                            break;
                        }
                    case 1:
                        {
                            int num2 = Random.Range(0, 3);
                            if (num2 == 0)
                            {
                                zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)9, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                            }
                            else if (num2 == 1)
                            {
                                zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)109, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                            }
                            else
                            {
                                zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)118, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                            }
                            break;
                        }
                    case 2:
                        if (Random.Range(0, 2) == 0)
                        {
                            zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)24, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        }
                        else
                        {
                            zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)30, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        }
                        break;
                    case 3:
                        {
                            MinerZombie component = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)28, this.plant.axis.transform.position.x, false).GetComponent<MinerZombie>();
                            component.theStatus = (ZombieStatus)13;
                            component.Rise();
                            zombie = component;
                            break;
                        }
                    case 4:
                        zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)40, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        break;
                    case 5:
                        zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)210, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        break;
                    case 6:
                        {
                            PogoZombie component2 = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)32, this.plant.axis.transform.position.x, false).GetComponent<PogoZombie>();
                            component2.LoseJumper(0);
                            zombie = component2;
                            break;
                        }
                    case 7:
                        zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)33, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        break;
                    case 8:
                        zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)38, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        break;
                    case 9:
                        zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)39, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        break;
                    case 10:
                        if (Random.Range(0, 2) == 0)
                        {
                            zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)8, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        }
                        else
                        {
                            zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)114, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        }
                        break;
                    case 11:
                        zombie = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, (ZombieType)52, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                        break;
                }
                if (zombie != null && Lawnf.TravelAdvanced((AdvBuff)53))
                {
                    zombie.theAttackDamage *= 4;
                    zombie.theMaxHealth *= 4;
                    zombie.theHealth = zombie.theMaxHealth;
                    zombie.theFirstArmorMaxHealth *= 4;
                    zombie.theFirstArmorHealth = zombie.theFirstArmorMaxHealth;
                    zombie.theSecondArmorMaxHealth *= 4;
                    zombie.theSecondArmorHealth = zombie.theSecondArmorMaxHealth;
                    zombie.UpdateHealthText();
                }
            }
            else
            {
                ZombieType zombieType = UltimateHypnoMagnet.types[Random.Range(0, UltimateHypnoMagnet.types.Count)];
                Zombie component3 = CreateZombie.Instance.SetZombieWithMindControl(this.plant.thePlantRow, zombieType, this.plant.axis.transform.position.x, false).GetComponent<Zombie>();
                if (Lawnf.TravelAdvanced((AdvBuff)53))
                {
                    component3.theAttackDamage *= 4;
                    component3.theMaxHealth *= 4;
                    component3.theHealth = component3.theMaxHealth;
                    component3.theFirstArmorMaxHealth *= 4;
                    component3.theFirstArmorHealth = component3.theFirstArmorMaxHealth;
                    component3.theSecondArmorMaxHealth *= 4;
                    component3.theSecondArmorHealth = component3.theSecondArmorMaxHealth;
                    component3.UpdateHealthText();
                }
            }
            if (item != null)
            {
                item.die = true;
            }
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.SetParticle((ParticleType)11, this.plant.axis.transform.position, this.plant.thePlantRow, true, 0f);
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002082 File Offset: 0x00000282
        public void Awake()
        {
            this.plant.shoot = base.transform.FindChild("Shoot");
        }

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x0600000A RID: 10 RVA: 0x000020A1 File Offset: 0x000002A1
        public UltimateMagnet plant
        {
            get
            {
                return base.gameObject.GetComponent<UltimateMagnet>();
            }
        }

        // Token: 0x04000002 RID: 2
        public static List<ZombieType> types = null;

        // Token: 0x04000003 RID: 3
        public static ID PlantID = 1915;
    }

    [HarmonyPatch(typeof(UltimateMagnet))]
    public class UltimateMagnet_Shoot
    {
        // Token: 0x0600000C RID: 12 RVA: 0x00002B64 File Offset: 0x00000D64
        [HarmonyPatch("Shoot")]
        [HarmonyPrefix]
        public static bool Prefix(UltimateMagnet __instance, ref UltimateMagnet.AttrackedBucket bucket)
        {
            bool flag = __instance != null && __instance.thePlantType == UltimateHypnoMagnet.PlantID;
            bool flag2;
            if (flag)
            {
                UltimateHypnoMagnet component = __instance.GetComponent<UltimateHypnoMagnet>();
                component.SpawnZombie(bucket.bucket.theBucketType, bucket);
                flag2 = false;
            }
            else
            {
                flag2 = true;
            }
            return flag2;
        }
    }
}
