using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace IceDoomGatlingBepInEx
{
    [BepInPlugin("salmon.icedoomgatling", "IceDoomGatling", "1.0")]
    public class Core : BasePlugin//304
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_iceDoomGatling_doom>();
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_iceDoomGatling_doomBig>();
            ClassInjector.RegisterTypeInIl2Cpp<IceDoomGatling>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "icedoomgatling");
            CustomCore.RegisterCustomBullet<Bullet_doom, Bullet_iceDoomGatling_doom>((BulletType)Bullet_iceDoomGatling_doom.Bullet_ID, ab.GetAsset<GameObject>("Bullet_iceDoomGatling_doom"));
            CustomCore.RegisterCustomBullet<Bullet_doom, Bullet_iceDoomGatling_doomBig>((BulletType)Bullet_iceDoomGatling_doomBig.Bullet_ID, ab.GetAsset<GameObject>("Bullet_iceDoomGatling_doomBig"));
            CustomCore.RegisterCustomPlant<DoomGatling, IceDoomGatling>(IceDoomGatling.PlantID, ab.GetAsset<GameObject>("IceDoomGatlingPrefab"),
                ab.GetAsset<GameObject>("IceDoomGatlingPreview"), new List<(int, int)>
                {
                    ((int)PlantType.DoomGatling, (int)PlantType.IceShroom),
                    ((int)PlantType.IceShroom, (int)PlantType.DoomGatling),
                    ((int)PlantType.SnowGatling, (int)PlantType.DoomShroom),
                    ((int)PlantType.DoomShroom, (int)PlantType.SnowGatling),
                    ((int)PlantType.SnowSplit, (int)PlantType.DoomPeashooter),
                    ((int)PlantType.DoomPeashooter, (int)PlantType.SnowSplit)
                },
                1.5f, 0f, 300, 300, 0, 600);

            CustomCore.RegisterCustomPlantSkin<DoomGatling, IceDoomGatling>(IceDoomGatling.PlantID, ab.GetAsset<GameObject>("IceDoomGatlingSkinPrefab"),
                ab.GetAsset<GameObject>("IceDoomGatlingSkinPreview"), new List<(int, int)>
                {
                    ((int)PlantType.DoomGatling, (int)PlantType.IceShroom),
                    ((int)PlantType.IceShroom, (int)PlantType.DoomGatling),
                    ((int)PlantType.SnowGatling, (int)PlantType.DoomShroom),
                    ((int)PlantType.DoomShroom, (int)PlantType.SnowGatling),
                    ((int)PlantType.SnowSplit, (int)PlantType.DoomPeashooter),
                    ((int)PlantType.DoomPeashooter, (int)PlantType.SnowSplit),
                    ((int)PlantType.GatlingPea, (int)PlantType.IceDoom),
                    ((int)PlantType.IceDoom, (int)PlantType.GatlingPea)
                },
                1.5f, 0f, 300, 300, 0, 600);

            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)IceDoomGatling.PlantID);
            CustomCore.AddPlantAlmanacStrings(IceDoomGatling.PlantID, $"寒冰毁灭菇机枪射手({IceDoomGatling.PlantID})", "如同死神一般的力量...\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>300×4/1.5秒，1800</color>\n<color=#3D1400>融合配方：</color><color=red>豌豆射手×4+寒冰菇+毁灭菇</color>\n<color=#3D1400>特点：</color><color=red>发射小寒冰毁灭菇子弹造成10点冻结值和10秒减速。第8发子弹改为大寒冰毁灭菇子弹，伤害1800并造成100点冻结值，半径3格无衰减溅射，对直击僵尸造成10秒减速，并在直击僵尸处生成伤害为1800的不冻结关卡的寒冰毁灭菇效果，然后休息3秒</color>\n<color=#3D1400>词条1：</color><color=red>枕戈待旦：发射完大寒冰毁灭菇子弹后不会休息。</color>\n<color=#3D1400>词条2：</color><color=red>核能威慑：每第4发子弹改为大寒冰毁灭菇子弹。</color>\n\n<color=#3D1400>“冰冻三尺非一日之寒”，寒冰毁灭菇机枪射手低调的外形下藏着一颗冰冷的心，对僵尸们来说就是“温柔杀手”，因为杀僵不见血。</color>");
        }
    }

    public class Bullet_iceDoomGatling_doom : MonoBehaviour
    {
        public static int Bullet_ID = 1902;

        public Bullet_doom bullet => gameObject.GetComponent<Bullet_doom>();
    }

    public class Bullet_iceDoomGatling_doomBig : MonoBehaviour
    {
        public static int Bullet_ID = 1903;

        public Bullet_doom bullet => gameObject.GetComponent<Bullet_doom>();
    }

    public class IceDoomGatling : MonoBehaviour
    {
        public static int PlantID = 990;
        public IceDoomGatling() : base(ClassInjector.DerivedConstructorPointer<IceDoomGatling>()) => ClassInjector.DerivedConstructorBody(this);

        public IceDoomGatling(IntPtr i) : base(i)
        {
        }

        public void Start()
        {
            plant.shoot = plant.transform.GetChild(0).FindChild("Shoot");
        }

        public Bullet AnimShoot_IceDoomGatling()
        {
            Vector3 firePosition = plant.shoot.position;

            // 检查旅行模式条件
            bool isAdvancedMode = Lawnf.TravelAdvanced(3);

            // 更新射击计数
            plant.doomTimes++;

            // 根据条件选择子弹类型
            Bullet bullet;
            if (plant.doomTimes < (isAdvancedMode ? 4 : 8))
            {
                // 发射普通子弹
                bullet = CreateBullet.Instance.SetBullet(
                    firePosition.x + 0.1f,
                    firePosition.y - 0.2f,
                    plant.thePlantRow,
                    (BulletType)Bullet_iceDoomGatling_doom.Bullet_ID, // 普通子弹ID
                    0, false);

                // 设置子弹伤害
                bullet.Damage = plant.attackDamage;
            }
            else
            {
                // 发射强力子弹
                bullet = CreateBullet.Instance.SetBullet(
                    firePosition.x + 0.1f,
                    firePosition.y - 0.2f,
                    plant.thePlantRow,
                    (BulletType)Bullet_iceDoomGatling_doomBig.Bullet_ID, // 强力子弹ID
                    0, false);
                // 设置强力子弹属性
                bullet.Damage = 6 * plant.attackDamage;
                bullet.theStatus = BulletStatus.Doom_big;

                // 重置射击计数
                plant.doomTimes = 0;

                // 调整攻击冷却时间
                if (!Lawnf.TravelAdvanced(2))
                {
                    plant.thePlantAttackCountDown = 3f;
                }
            }

            // 播放随机射击音效
            int soundID = UnityEngine.Random.Range(3, 5);
            GameAPP.PlaySound(soundID, 0.5f, 1f);

            return bullet;
        }

        public DoomGatling plant => gameObject.GetComponent<DoomGatling>();
    }

    [HarmonyLib.HarmonyPatch(typeof(Bullet_doom), nameof(Bullet_doom.HitZombie))]
    public class Bullet_doom_HitZombie
    {
        [HarmonyLib.HarmonyPrefix]
        public static bool Prefix(Bullet_doom __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == (BulletType)Bullet_iceDoomGatling_doom.Bullet_ID ||
                __instance.theBulletType == (BulletType)Bullet_iceDoomGatling_doomBig.Bullet_ID)
            {
                Transform bulletTransform = __instance.transform;
                Vector3 bulletPosition = bulletTransform.position;

                // 创建粒子效果
                GameObject particle = CreateParticle.SetParticle(28, bulletPosition, __instance.theBulletRow, true);

                // 特殊状态处理（状态6）
                if (__instance.theStatus == BulletStatus.Doom_big)
                {
                    // 放大粒子效果
                    if (particle != null)
                    {
                        Transform particleTransform = particle.transform;
                        Vector3 scale = particleTransform.localScale;
                        particleTransform.localScale = new Vector3(scale.x * 2f, scale.y * 2f, scale.z * 2f);

                        // 创建范围爆炸伤害
                        AoeDamage.BigBomb(
                            bulletPosition,
                            3.0f,
                            __instance.zombieLayer,
                            __instance.theBulletRow,
                            1800
                        );

                        GameAPP.board.GetComponent<Board>().SetDoom(Mouse.Instance.GetColumnFromX(zombie.axis.transform.position.x), zombie.theZombieRow, false, true, zombie.axis.position, 1800, 0);
                    }
                    zombie.AddfreezeLevel(100);

                    // 播放特殊音效
                    GameAPP.PlaySound(41, 0.5f, 1f);
                }
                else
                {
                    // 播放普通音效
                    GameAPP.PlaySound(70, 0.5f, 1f);
                    zombie.AddfreezeLevel(10);
                }
                zombie.SetCold(10f);

                // 旅行模式37特殊效果
                if (Lawnf.TravelAdvanced(37))
                {
                    // 获取僵尸位置
                    if (zombie != null && zombie.axis != null)
                    {
                        Vector3 zombiePosition = zombie.axis.position;

                        // 计算僵尸所在网格位置
                        int column = Mouse.Instance.GetColumnFromX(zombiePosition.x);
                        int row = zombie.theZombieRow;

                        // 设置末日效果
                        Board.Instance.SetDoom(
                            column,
                            row,
                            false,
                            true,
                            zombiePosition,
                            3600,
                            0
                        );
                    }
                }

                // 对僵尸造成伤害
                if (zombie != null && zombie.freezeTimer > 0)
                {
                    zombie.TakeDamage(DmgType.IceAll, __instance.Damage * 4);
                }
                else if (zombie != null)
                {
                    zombie.TakeDamage(DmgType.IceAll, __instance.Damage);
                }

                // 销毁子弹
                __instance.Die();
                return false;
            }
            return true;
        }
    }
}