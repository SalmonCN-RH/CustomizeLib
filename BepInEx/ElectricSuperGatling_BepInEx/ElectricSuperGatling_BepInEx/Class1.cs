using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;
using Unity.VisualScripting;

namespace ElectricSuperGatling_BepInEx
{
    [BepInPlugin("salmon.electricsupergatling", "ElectricSuperGatling", "1.0")]
    public class Core : BasePlugin//304
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<Bullet_electricSuperGatlingPea>();
            ClassInjector.RegisterTypeInIl2Cpp<ElectricSuperGatling>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "electricsupergatling");
            CustomCore.RegisterCustomBullet<Bullet, Bullet_electricSuperGatlingPea>((BulletType)Bullet_electricSuperGatlingPea.BulletID, ab.GetAsset<GameObject>("ElectricSuperGatlingBullet"));
            CustomCore.RegisterCustomPlant<SuperGatling, ElectricSuperGatling>(
                ElectricSuperGatling.PlantID,
                ab.GetAsset<GameObject>("ElectricSuperGatlingPrefab"),
                ab.GetAsset<GameObject>("ElectricSuperGatlingPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.SuperGatling, (int)PlantType.ElectricOnion),
                    ((int)PlantType.ElectricOnion, (int)PlantType.SuperGatling)
                },
                1.5f, 0f, 20, 300, 0f, 825
            );
            CustomCore.AddPlantAlmanacStrings(ElectricSuperGatling.PlantID,
                $"超级电能机枪射手({ElectricSuperGatling.PlantID})",
                "一次发射六颗电能豌豆，有概率一次性发射大量电能豌豆\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>20/0.14秒;\n20×3/0.14秒（大招）</color>\n<color=#3D1400>特点：</color><color=red>攻击有3%概率散射大量电能豌豆，持续5秒。子弹每攻击到僵尸49次对附近每有僵尸造成伤害为100点的爆炸伤害</color>\n<color=#3D1400>融合配方：</color><color=red>超级机枪射手+闪电洋葱</color>\n\n<color=#3D1400>“你是电，你是光，你是唯一的神话，我只爱你，You are my super star!”超级电能机枪豌豆总是把这句歌词挂在嘴边，电光火石间，僵尸们就因触碰114514伏的高压电流而死。</color>"
            );
        }
    }

    public class Bullet_electricSuperGatlingPea : MonoBehaviour
    {
        public static int BulletID = 1904;
        public int damageTimes = 0;
        public int attackedZombieCount = 0;

        public Bullet_electricSuperGatlingPea() : base(ClassInjector.DerivedConstructorPointer<Bullet_electricSuperGatlingPea>()) => ClassInjector.DerivedConstructorBody(this);

        public Bullet_electricSuperGatlingPea(IntPtr i) : base(i)
        {
        }

        public void Start()
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }

        public void FixedUpdate()
        {
            if (GameAPP.theGameStatus is (int)GameStatus.InGame)
            {
                damageTimes++;
                if (damageTimes >= 7)
                {
                    var pos = bullet.transform.position;
                    LayerMask layermask = bullet.zombieLayer.m_Mask;
                    var array = Physics2D.OverlapCircleAll(new(pos.x, pos.y), 1f);
                    foreach (var z in array)
                    {
                        if (attackedZombieCount < 49)
                        {
                            if (z is not null && !z.IsDestroyed() && z.TryGetComponent<Zombie>(out var zombie) && zombie is not null && !zombie.isMindControlled && !zombie.IsDestroyed() && zombie.theStatus != ZombieStatus.Miner_digging)
                            {
                                zombie.TakeDamage(DmgType.Explode, bullet.Damage);
                                GameAPP.PlaySound(UnityEngine.Random.RandomRange(0, 3));
                                attackedZombieCount++;
                            }
                        }
                        else
                        {
                            if (z is not null && !z.IsDestroyed() && z.TryGetComponent<Zombie>(out var zombie) && zombie is not null && !zombie.isMindControlled && !zombie.IsDestroyed() && zombie.theStatus != ZombieStatus.Miner_digging)
                            {
                                AoeDamage.BigBomb(zombie.axis.transform.position, 1.1f, bullet.zombieLayer, bullet.theBulletRow, 100);
                                // GameAPP.board.GetComponent<Board>().SetDoom(0, 0, false, false, zombie.axis.transform.position, 1800, 0, null);
                                GameAPP.PlaySound(UnityEngine.Random.RandomRange(0, 3));
                            }
                        }
                    }
                    if (attackedZombieCount >= 49)
                        attackedZombieCount = 0;
                    damageTimes = 0;
                }
                /*MelonLogger.Msg(damageTimes);
                MelonLogger.Msg(attackedZombieCount);*/
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
        }

        public Bullet bullet => gameObject.GetComponent<Bullet>();
    }

    public class ElectricSuperGatling : MonoBehaviour
    {
        public static int PlantID = 1906;

        public SuperGatling plant => gameObject.GetComponent<SuperGatling>();

        public void Awake()
        {
            plant.shoot = plant.gameObject.transform.GetChild(0).FindChild("Shoot");
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
}