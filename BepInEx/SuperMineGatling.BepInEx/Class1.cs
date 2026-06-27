using BepInEx;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Unity;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;

namespace SuperMineGatling.BepInEx
{
    [BepInPlugin("salmon.superminegatling", "SuperMineGatling", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "superminegatling");
            CustomCore.RegisterCustomPlant<PeaMine, SuperMineGatling>(SuperMineGatling.PlantID, ab.GetAsset<GameObject>("SuperMineGatlingPrefab"),
                ab.GetAsset<GameObject>("SuperMineGatlingPreview"), 
                new List<(PlantType, PlantType)>()
                {
                    (PlantType.PotatoMine, PlantType.SuperGatling),
                    (PlantType.SuperGatling, PlantType.PotatoMine)
                }.ToIntegerList(), 1.5f, 0f, 40, 300, 7.5f, 625);
            CustomCore.AddPlantAlmanacStrings(SuperMineGatling.PlantID, $"土豆超级机枪射手",
                "一次发射六颗土豆豌豆，有一定概率散射具有爆炸的土豆豌豆。\n\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>40x6/1.5秒</color>\n" +
                "<color=#3D1400>特性：</color><color=red>低矮</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①距离前方僵尸越近攻击间隔越短，最低0.5秒\n" +
                "②普通攻击下，每发子弹有2%概率开大，5秒内，每0.02秒散射3发土豆豌豆，命中时造成一次10倍攻击力的范围爆炸伤害\n" +
                "③接触僵尸时，造成7200的范围爆炸伤害并入土，随后等待15秒后重新出土</color>\n" +
                "<color=#3D1400>融合配方：</color><color=red>超级机枪射手+土豆雷</color>\n\n" +
                "<color=#3D1400>土豆超级机枪射手直播无果后，受到了土豆地雷的邀请，加入了土豆爆破小队。情绪本就不稳定的他，在某天情绪激动，在基地到处炸，这一段连续又持久的爆炸声把基地炸的满目疮痍，有土豆从爆炸中跑出来了？是三线土豆地雷！</color>");
            CustomCore.AddUltimatePlant(SuperMineGatling.PlantID);
            CustomCore.TypeMgrExtra.IsPotatoMine.Add(SuperMineGatling.PlantID);
        }
    }

    public class SuperMineGatling : MonoBehaviour
    {
        public static ID PlantID = 1938;

        public float timer = 0f;
        public float shoot = 0f;
        public float origin = 1f;
        public bool inv = false;

        public void Awake()
        {
            plant.shoot = transform.FindChild("PotatoMine_light1").FindChild("Shoot");
            plant.attributeCountdown = 15f * 2;
            plant.isShort = true;
        }

        public void Update()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame && plant != null)
            {
                if (shoot > 0f && timer > 0f)
                {
                    shoot -= Time.deltaTime;
                    if (shoot <= 0f)
                        ShootEvent();
                }

                if (timer > 0f)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                    {
                        timer = 0f;
                        shoot = 0f;
                        plant.anim.SetBool("shooting", false);
                        plant.invincible = inv;
                    }
                }
            }
        }

        public void Shoot1()
        {
            var superShoot = CoreTools.TravelUltimate("极速爆发") ? 6 : 2;
            if (UnityEngine.Random.Range(0, 100) < superShoot)
            {
                timer = 5f;
                plant.flashCountDown = 5f;
                ShootEvent();
                plant.anim.SetBool("shooting", true);
                plant.Recover(plant.thePlantMaxHealth);
                inv = plant.invincible;
                plant.invincible = true;
                ResetSpeed();
            }
            else
            {
                var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow,
                    BulletType.Bullet_potato, BulletMoveWay.MoveRight);
                bullet.Damage = plant.attackDamage;
                bullet.fromType = plant.thePlantType;
                GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);
            }
        }

        public void ShootEvent()
        {
            int damage = CoreTools.TravelUltimate("精准射击") ? (2 * plant.attackDamage) : plant.attackDamage;
            damage *= 10;
            var moveWay = CoreTools.TravelUltimate("精准射击") ? BulletMoveWay.MoveRight : BulletMoveWay.Right_free;
            float x = plant.shoot.position.x;
            float y = plant.shoot.position.y;

            SuperShoot(x, y, moveWay, damage);
            SuperShoot(x, y, moveWay, damage);
            SuperShoot(x, y, moveWay, damage);

            GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);

            if (timer <= 0f)
            {
                shoot = 0f;
                plant.anim.SetBool("shooting", false);
            }
            else
                shoot = 0.02f;
        }

        public void SuperShoot(float basex, float basey, BulletMoveWay moveWay, int dmg)
        {
            var bullet = CreateBullet.Instance.SetBullet(basex + UnityEngine.Random.Range(-0.1f, 0.1f), basey + UnityEngine.Random.Range(-0.1f, 0.1f),
                plant.thePlantRow, BulletType.Bullet_potato_explode, moveWay);
            bullet.Damage = dmg;
            bullet.transform.Rotate(0f, 0f, UnityEngine.Random.Range(-15f, 15f));
            bullet.normalSpeed = UnityEngine.Random.Range(12f, 14f);
            bullet.fromType = plant.thePlantType;
        }

        public void AnimDown()
        {
            plant.attributeCountdown = 15f * 2;
            plant.anim.ResetTrigger("rise");
        }

        public void SetSpeed()
        {
            // 不丢伤害
            origin = plant.anim.GetFloat("shootSpeed");
            plant.anim.SetFloat("shootSpeed", 4f / (3f * (plant.thePlantAttackInterval - 0.1f)));
        }

        public void ResetSpeed()
        {
            plant.anim.SetFloat("shootSpeed", origin);
        }

        public PeaMine plant => gameObject.GetComponent<PeaMine>();
    }

    [HarmonyPatch(typeof(PeaMine))]
    public static class PeaMinePatch
    {
        [HarmonyPatch(nameof(PeaMine.AnimShoot))]
        [HarmonyPrefix]
        public static bool PreAnimShoot(PeaMine __instance)
        {
            if (__instance.thePlantType == SuperMineGatling.PlantID)
            {
                __instance.GetComponent<SuperMineGatling>().Shoot1();
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(PeaMine.BombEffect))]
        [HarmonyPrefix]
        public static bool PreBombEffect(PeaMine __instance)
        {
            if (__instance.thePlantType == SuperMineGatling.PlantID)
            {
                AoeDamage.SmallBombPotato(__instance.axis.position, 1.0f, __instance.zombieLayer, __instance.thePlantRow, 4 * 60 * __instance.attackDamage,
                    __instance.thePlantType);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PotatoMine))]
    public static class PotatoMinePatch
    {
        [HarmonyPatch(nameof(PotatoMine.Explode))]
        [HarmonyPrefix]
        public static bool PreAnimShoot(PotatoMine __instance)
        {
            if (__instance.thePlantType == SuperMineGatling.PlantID)
            {
                __instance.BombEffect();
                __instance.exploded = false;
                __instance.isAready = false;
                __instance.anim.SetTrigger("down");

                ParticleManager.Instance.SetParticle(ParticleType.PotaoExplode, __instance.axis.position, __instance.thePlantRow);
                GameAPP.PlaySound(47, 0.5f, 1.0f);
                ScreenShake.TriggerShake(0.15f);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch(nameof(Plant.Crashed))]
        [HarmonyPrefix]
        public static bool PreCrashed(Plant __instance)
        {
            if (__instance.thePlantType == SuperMineGatling.PlantID && __instance.GetComponent<SuperMineGatling>().timer > 0f)
            {
                return false;
            }
            return true;
        }
    }
}
