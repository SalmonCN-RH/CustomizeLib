using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace InvisibleZombie.BepInEx
{
    [BepInPlugin("salmon.invisiblezombie", "InvisibleZombie", "1.0.0")]
    public class Core : BasePlugin
    {
        public static BuffID buff1 = -1;
        public static BuffID buff2 = -1;

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ClassInjector.RegisterTypeInIl2Cpp<Invisibler>();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            buff1 = CustomCore.RegisterCustomBuff(new BuffConfig()
            {
                backGround = BuffBgType.Night,
                type = BuffType.Debuff,
                iconZombie = ZombieType.NormalZombie,
                desc = "隐形食脑者I：所有僵尸的能见度大幅降低，僵尸接近地图左侧时会逐渐显现",
                unlock = () => true,
                almanac = AlmanacBuffType.Zombie
            });
            buff2 = CustomCore.RegisterCustomBuff(new BuffConfig()
            {
                backGround = BuffBgType.Night,
                type = BuffType.Debuff,
                iconZombie = ZombieType.NormalZombie,
                desc = "隐形食脑者II：所有僵尸将不再能见",
                unlock = () => true,
                almanac = AlmanacBuffType.Zombie
            });
        }
    }

    public class Invisibler : MonoBehaviour
    {
        public static bool buff1 => Lawnf.TravelDebuff(Core.buff1);
        public static bool buff2 => Lawnf.TravelDebuff(Core.buff2);

        public Zombie zombie = null!;
        public Vector2 position => zombie.axis.position;
        public List<(SpriteRenderer renderer, Color startColor)> renderers = new();
        public Board board => zombie.board;
        public float left => Mouse.Instance.GetBoxXFromColumn(0);
        public float right => Mouse.Instance.GetBoxXFromColumn(board.columnNum - 1);
        public float distance => position.x - left;

        public void Start()
        {
            if (zombie == null)
            {
                Destroy(this);
                return;
            }
            renderers = zombie.gameObject.GetComponentsInChildren<SpriteRenderer>(true).Select(r => (r, r.color)).ToList();
        }

        public void Update()
        {
            if (!zombie.IsObjExist())
            {
                Destroy(this);
                return;
            }
            if (buff2)
            {
                foreach (var (renderer, color) in renderers)
                {
                    if (!renderer.IsObjExist()) continue;
                    renderer.color = new(color.r, color.g, color.b, 0f);
                }
            }
            else if (buff1)
            {
                foreach (var (renderer, color) in renderers)
                {
                    if (!renderer.IsObjExist()) continue;
                    renderer.color = new(color.r, color.g, color.b, color.a * CalculateAlpha(position.x));
                }
            }
        }

        public float CalculateAlpha(float x)
        {
            var colX = Mouse.Instance.GetBoxXFromColumn(3);
            if (x < left) return 0.5f;
            if (x <= colX) // 如果在第四列左侧
                return (-0.35f / (colX - left)) * (x - left) + 0.5f;
            else
                return (-0.15f / (right - colX)) * (x - colX) + 0.15f;
        }
    }

    [HarmonyPatch(typeof(Zombie))]
    public static class ZombiePatch
    {
        [HarmonyPatch(nameof(Zombie.Start))]
        [HarmonyPostfix]
        public static void PostStart(Zombie __instance)
        {
            if (GameAPP.theGameStatus == GameStatus.InGame && __instance.IsObjExist())
            {
                if (__instance.theZombieType != ZombieType.FootballBoss && __instance.theZombieType != ZombieType.HorseBoss && 
                    __instance.theZombieType != ZombieType.ZombieBoss && __instance.theZombieType != ZombieType.ZombieBoss2)
                    __instance.GetOrAddComponent<Invisibler>()!.zombie = __instance;
            }
        }
    }
}
