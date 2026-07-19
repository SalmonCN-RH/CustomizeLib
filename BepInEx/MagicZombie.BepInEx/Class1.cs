using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using Unity.VisualScripting;
using CustomizeLib.BepInEx;

namespace MagicZombie.BepInEx
{
    [BepInPlugin("salmon.magiczombie", "MagicZombie", "1.0")]
    public class Core : BasePlugin//960
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<MagicZombie>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "magiczombie");
            CustomCore.RegisterCustomZombie<NormalZombie, MagicZombie>((ZombieType)MagicZombie.ZombieID,
                ab.GetAsset<GameObject>("MagicZombie"), null, 50, 1350, 0, 0);
            CustomCore.AddZombieAlmanacStrings(MagicZombie.ZombieID, $"魔术僵尸({MagicZombie.ZombieID})", "魔术技巧！喜好对植物表演的大魔术师。\n\n<color=#3D1400>贴图作者：@林秋AutumnLin</color>\n<color=#3D1400>韧性：</color><color=red>1350</color>\n<color=#3D1400>特点：</color><color=red>每5秒对在场随机植物进行一次魔术融合，对于无法融合的植物会使魔术失效。小概率会使植物消失并生成小鬼僵尸，若在场无植物时则在自身处生成小鬼。</color>\n\n<color=#3D1400>“我喜欢魔术，更喜欢表演魔术，”魔术僵尸热情高涨，游走在舞台中央，“所有人都会喜欢我的魔术，包括植物！”</color>");
        }
    }

    public class MagicZombie : MonoBehaviour
    {
        public static int ZombieID = 500;
        public bool loseHead = false;
        public bool loseHand = false;
        public float magicCountDown = 5f;

        public void Awake()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame && zombie is not null)
            {
                zombie.butterHead = gameObject.transform.FindChild("Zombie_head/hat").gameObject;
            }
        }

        public void Update()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame && zombie is not null)
            {
                if (!loseHand && zombie.theHealth < zombie.theMaxHealth / 3 * 2)
                {
                    Destroy(zombie.transform.FindChild("Zombie_outerarm_lower").gameObject);
                    Destroy(zombie.transform.FindChild("Zombie_outerarm_hand").gameObject);
                    loseHand = true;
                }
                if (!loseHead && zombie.theHealth < zombie.theMaxHealth / 3)
                {
                    Destroy(zombie.transform.FindChild("Zombie_jaw").gameObject);
                    Destroy(zombie.transform.FindChild("Zombie_head").gameObject);
                    loseHead = true;
                }
                if (zombie.theSpeed != 0 && !loseHand && !zombie.isMindControlled && !loseHead)
                    magicCountDown -= Time.deltaTime * zombie.theSpeed;
                if (magicCountDown < 0 && !loseHand && !zombie.isMindControlled && !loseHead)
                {
                    zombie.anim.SetTrigger("magic");
                    magicCountDown = 5f;
                }
            }
        }

        public void AnimMagic()
        {
            if (Board.Instance is not null && !loseHand && zombie is not null)
            {
                if (Board.Instance.boardEntity.plantArray is null)
                {
                    CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.ImpZombie, zombie.axis.transform.position.x);
                    ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, zombie.axis.transform.position, lim: true);
                    return;
                }
                var list = Board.Instance.boardEntity.plantArray.ToArray().ToList().Where(p => p is not null).ToList();
                if (list.Count <= 0)
                {
                    CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.ImpZombie, zombie.axis.transform.position.x);
                    ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, zombie.axis.transform.position, lim: true);
                    return;
                }
                var index = UnityEngine.Random.Range(0, list.Count);
                var plant = list[index];
                if (plant is null)
                    return;
                if (UnityEngine.Random.Range(1, 101) < 95)
                {
                    var result = CreatePlant.Instance.SetPlant(plant.thePlantColumn, plant.thePlantRow, PlantType.Present);
                    ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, plant.axis.transform.position);
                    if (result is not null)
                        result.GetComponent<Present>().AnimEvent();
                }
                else
                {
                    plant.Die();
                    CreateZombie.Instance.SetZombie(plant.thePlantRow, ZombieType.ImpZombie, plant.axis.transform.position.x);
                    ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, plant.axis.transform.position);
                }
            }
        }

        public NormalZombie? zombie => gameObject.TryGetComponent<NormalZombie>(out var z) ? z : null;
    }

    [HarmonyPatch(typeof(CreateZombie), nameof(CreateZombie.SetZombie))]
    public static class CreateZombie_SetZombie_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref int theRow, ref ZombieType theZombieType, ref float theX)
        {
            if (UnityEngine.Random.Range(0, 100) <= 20 && theZombieType == ZombieType.RandomZombie && GameAPP.theGameStatus == GameStatus.InGame)
            {
                CreateZombie.Instance.SetZombie(theRow, (ZombieType)MagicZombie.ZombieID, theX);
                return false;
            }
            return true;
        }
    }
}