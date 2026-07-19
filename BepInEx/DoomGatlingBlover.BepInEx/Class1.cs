using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using Unity.VisualScripting;
using CustomizeLib.BepInEx;

namespace DoomGatlingBlover.BepInEx
{
    [BepInPlugin("salmon.doomgatlingblover", "DoomGatlingBlover", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnGameInit()
        {
            CustomCore.RegisterCustomBanMix(UltimateDoomGatlingBlover.PlantID,
                () => (Lawnf.TravelAdvanced(CoreTools.GetAdvBuffByString("枕戈待旦")) && Lawnf.TravelAdvanced(CoreTools.GetAdvBuffByString("核能威慑")) && Utils.EnableTravelPlant()) || Utils.IsCheat());
            UltimateDoomGatlingBlover.BuffID = CustomCore.RegisterCustomBuff("轰炸火力：究极浮空毁灭射手的子弹会寄生毁灭炸弹。究极樱桃射手的子弹附带究极浮空毁灭射手的追加效果，给予寄生毁灭炸弹的充能x5",
                BuffType.AdvancedBuff, () => CoreTools.TravelAdvanced("枕戈待旦") && CoreTools.TravelAdvanced("核能威慑") && CoreTools.TravelUltimate("力大砖飞") && CoreTools.TravelUltimate("快速填装") && TravelStore.Instance != null, 15000, PlantType.EndoFlame);
        }

        public override void OnStart()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ClassInjector.RegisterTypeInIl2Cpp<DoomGatlingBlover>();
            ClassInjector.RegisterTypeInIl2Cpp<UltimateDoomGatlingBlover>();
            ClassInjector.RegisterTypeInIl2Cpp<DoomBomb>();
            ClassInjector.RegisterTypeInIl2Cpp<DoomDoom>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "doomgatlingblover");
            {
                CustomCore.RegisterCustomPlant<UltimateGatlingBlover, DoomGatlingBlover>((int)DoomGatlingBlover.PlantID, ab.GetAsset<GameObject>("DoomGatlingBloverPrefab"),
                    ab.GetAsset<GameObject>("DoomGatlingBloverPreview"), new List<(int, int)>
                    {
                        ((int)PlantType.DoomGatling, (int)PlantType.Blover)
                    }, 1.5f, 0f, 300, 300, 7.5f, 525);
                CustomCore.AddPlantAlmanacStrings((int)DoomGatlingBlover.PlantID, $"浮空毁灭射手({(int)DoomGatlingBlover.PlantID})",
                    "毁灭爆破浮游炮，会与攻速更快的植物协同攻击提升火力\n" +
                    "<color=#0000FF>毁灭机枪射手同人亚种</color>\n\n" +
                    "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                    "<color=#3D1400>融合配方：</color><color=red>毁灭机枪射手（底座）+三叶草</color>\n" +
                    "<color=#3D1400>转化配方：</color><color=red>铲除←→三叶草</color>\n" +
                    "<color=#3D1400>伤害：</color><color=red>300/1.5秒</color>\n" +
                    "<color=#3D1400>特性：</color><color=red>飞行</color>\n" +
                    "<color=#3D1400>特点：</color><color=red>①下方植物攻击间隔低于1.5秒时，自身攻击改为与其一致。\n" +
                    "②每8发子弹改为大毁灭菇子弹。</color>\n" +
                    "<color=#3D1400>词条1:</color><color=red>枕戈待旦：攻击速度+100%</color>\n" +
                    "<color=#3D1400>词条2:</color><color=red>核能威慑：每4发必为大毁灭菇</color>\n\n" +
                    "<color=#3D1400>浮空毁灭射手的家里有很多花，他喜欢养花，奔放的杜鹃，高洁的百合，坚韧的腊梅，“如你所见，这些花朵都在盛开，同一时间，就在我的家里，我喜欢他们盛开的样子，我喜欢花香充满鼻腔的感觉，我没有强制控制它们盛开，只是给了它们一个选择，它们选择为我而开，我也喜欢它们绽放自我，不论是花朵还是植物，都要有一颗勇敢坚强的心，不惧风雪，不畏挫折。”</color>");
                CustomCore.TypeMgrExtra.FlyingPlants.Add(DoomGatlingBlover.PlantID);
            }
            {
                CustomCore.RegisterCustomPlant<UltimateGatlingBlover, UltimateDoomGatlingBlover>((int)UltimateDoomGatlingBlover.PlantID, ab.GetAsset<GameObject>("UltimateDoomGatlingBloverPrefab"),
                    ab.GetAsset<GameObject>("UltimateDoomGatlingBloverPreview"), new List<(int, int)>
                    {
                        ((int)PlantType.UltimateDoomGatling, (int)PlantType.Blover)
                    }, 1.5f, 0f, 300, 300, 7.5f, 650);
                CustomCore.AddPlantAlmanacStrings((int)UltimateDoomGatlingBlover.PlantID, $"究极浮空毁灭射手({(int)UltimateDoomGatlingBlover.PlantID})",
                    "毁灭爆破浮游炮，发射的余烬子弹会根据异常状态追加效果\n" +
                    "<color=#0000FF>浮空毁灭射手进阶形态</color>\n\n" +
                    "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                    "<color=#3D1400>使用条件：</color><color=red>集齐基础形态双词条</color>\n" +
                    "<color=#3D1400>融合配方：</color><color=red>究极毁灭机枪射手（底座）+三叶草</color>\n" +
                    "<color=#3D1400>转化配方：</color><color=red>铲除←→三叶草</color>\n" +
                    "<color=#3D1400>伤害：</color><color=red>300/1.5秒</color>\n" +
                    "<color=#3D1400>特性：</color><color=red>飞行</color>\n" +
                    "<color=#3D1400>特点：</color><color=red>①下方植物攻击间隔低于1.5秒时，自身攻击改为与其一致。\n" +
                    "②发射余烬毁灭菇子弹，每8发子弹变为6倍伤害的余烬大毁灭菇子弹\n" +
                    "③子弹会强化余烬状态对特定状态的增益：\n" +
                    "寒冷状态：施加1秒冻结，使本次伤害x4\n" +
                    "红温状态：改为引发一次等量伤害的红温爆炸\n" +
                    "中毒状态：造成伤害前，追加（子弹攻击力x0.5x蒜值）的伤害</color>\n" +
                    "<color=#3D1400>余烬毁灭菇子弹：</color><color=red>施加余烬状态。能穿透二类防具。大型子弹会造成2次半径3格的范围伤害，仅第2次触发追击效果</color>\n" +
                    "<color=#3D1400>词条1:</color><color=red>枕戈待旦：攻击速度+100%</color>\n" +
                    "<color=#3D1400>词条2:</color><color=red>核能威慑：每4发必为大毁灭菇</color>\n" +
                    "<color=#3D1400>连携词条：</color><color=red>轰炸火力：究极浮空毁灭射手直击目标会挂上寄生毁灭炸弹。究极樱桃射手的子弹附带究极浮空毁灭射手的追加效果，给予寄生毁灭炸弹的充能x5</color>\n" +
                    "<color=#3D1400>寄生毁灭炸弹：</color><color=red>持续5秒，期间受到究极樱桃射手和究极浮空毁灭射手的伤害将等额储存在内，随后引爆，对半径3.5格的目标造成伤害并施加余烬效果，若目标死亡时将提前引爆。双方的子弹击中时携带寄生毁灭炸弹的目标会产生半径0.5格基于10%毁灭炸弹的伤害</color>\n\n" +
                    "<color=#3D1400>究极浮空毁灭射手是植物界的大师，他一生都在研究“炁”，“不管是植物还是僵尸，他们的身上都有不同的‘炁’，我们可以根据‘炁’所散发的颜色来判断他们的状态，他们的心情，他们的过去和未来”究极浮空毁灭射手闭上了眼睛“我们透过眼睛只能看到事物的表面，真正的世界需要我们用心去看，去观察，有的时候，闭上眼睛，才能看到事物的本相”</color>");
                CustomCore.TypeMgrExtra.FlyingPlants.Add(UltimateDoomGatlingBlover.PlantID);
                DoomBomb.bomb = ab.GetAsset<GameObject>("DoomBomb");
                DoomBomb.bomb.AddComponent<DoomBomb>();
                ab.GetAsset<GameObject>("Doom").AddComponent<DoomDoom>();
                CustomCore.RegisterCustomParticle(DoomBomb.doomID, ab.GetAsset<GameObject>("Doom"));
                CustomCore.RegisterCustomParticle(DoomBomb.doomSplatID, ab.GetAsset<GameObject>("DoomSplat"));
            }
        }
    }

    public class DoomGatlingBlover : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1950;

        public UltimateGatlingBlover plant => gameObject.GetComponent<UltimateGatlingBlover>();

        public void Start()
        {
            plant.shoot = gameObject.transform.FindChild("PeaShooter_Head/Shoot");
        }

        public void Update()
        {
            if (plant != null && GameAPP.theGameStatus == GameStatus.InGame)
            {
                if (Lawnf.TravelAdvanced(CoreTools.GetAdvBuffByString("枕戈待旦")))
                    plant.thePlantAttackCountDown -= Time.deltaTime;
                plant.coordination = 0f;
            }
        }

        public bool IsBigShoot() => plant.attributeCount % (CoreTools.TravelAdvanced("核能威慑") ? 4 : 8) == 0;

        public BulletType GetBulletType()
        {
            if (IsBigShoot())
                return GetBigBulletType();
            return GetNormalBulletType();
        }

        public virtual BulletType GetNormalBulletType() => BulletType.Bullet_doom;
        public virtual BulletType GetBigBulletType() => BulletType.Bullet_doom_big;
    }

    public class UltimateDoomGatlingBlover : DoomGatlingBlover
    {
        public static new PlantType PlantID = (PlantType)1963;
        public static BuffID BuffID = -1;

        public override BulletType GetNormalBulletType() => BulletType.Bullet_doom_ulti;
        public override BulletType GetBigBulletType() => BulletType.Bullet_doom_big_ulti;
    }

    public class DoomBomb : MonoBehaviour
    {
        public static GameObject bomb;
        public static ID doomID = 1963;
        public static ID doomSplatID = 1964;

        public int damage = 0;
        public float deathTime = 5f;
        public Board? board;
        public float columnX = 3.5f;

        public void AddDamage(int damage) => this.damage += damage;

        public void Start()
        {
            board = Board.Instance;
            if (board == null)
            {
                Destroy(gameObject);
                return;
            };
            columnX = (Mouse.Instance.GetBoxXFromColumn(board.columnNum - 1) - Mouse.Instance.GetBoxXFromColumn(0)) / board.columnNum; 
            
            var parentScale = transform.parent.localScale;
            var localScale = transform.localScale;

            transform.localScale = new Vector3(
                localScale.x * 0.3f / parentScale.x,
                localScale.y * 0.3f / parentScale.y,
                localScale.z * 0.3f / parentScale.z
            );
        }

        public void Update()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame)
            {
                deathTime -= Time.deltaTime;
                if (deathTime <= 0f)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        public void SmallBomb()
        {
            var zombies = Physics2D.OverlapCircleAll(transform.position, columnX * 0.5f, LayerMask.GetMask("Zombie"));
            foreach (var collider in zombies)
            {
                if (collider == null || collider.IsDestroyed() || collider.gameObject == null || collider.gameObject.IsDestroyed()) continue;
                if (!collider.gameObject.TryGetComponent<Zombie>(out var zombie) || zombie == null || zombie.IsDestroyed()) continue;
                zombie.TakeDamage(DmgType.Carred, (int)(damage * 0.1f) + 1, UltimateDoomGatlingBlover.PlantID);
            }
            CreateParticle.SetParticle(doomSplatID, transform.position, 11);
            GameAPP.PlaySound(70, 0.5f, 1.0f);
        }

        public void Bomb()
        {
            var zombies = Physics2D.OverlapCircleAll(transform.position, columnX * 3.5f, LayerMask.GetMask("Zombie"));
            foreach (var collider in zombies)
            {
                if (collider == null || collider.IsDestroyed() || collider.gameObject == null || collider.gameObject.IsDestroyed()) continue;
                if (!collider.gameObject.TryGetComponent<Zombie>(out var zombie) || zombie == null || zombie.IsDestroyed()) continue;
                zombie.TakeDamage(DmgType.Carred, damage, UltimateDoomGatlingBlover.PlantID);
            }
            if (!GameAPP.config.distablexplodeFlash)
                CreateParticle.SetParticle(doomID, transform.position, 11);
            else
                CreateParticle.SetParticle(doomSplatID, transform.position, 11);
            GameAPP.PlaySound(41, 0.5f, 1.0f);
        }

        public void OnDestroy()
        {
            Bomb();
        }

        public static bool HasBomb(Zombie zombie) => zombie.GetComponentsInChildren<DoomBomb>().Count > 0;

        public static DoomBomb? TryAddBomb(Zombie zombie)
        {
            if (zombie.gameObject == null || zombie == null || zombie.IsDestroyed()) return null;
            var component = zombie.GetComponentInChildren<DoomBomb>();
            if (component != null) 
                return component;
            var result = Instantiate(bomb, zombie.axis.transform.position + new Vector3(0f, 0.95f, 0f), Quaternion.identity, zombie.transform);
            return result.GetComponent<DoomBomb>();
        }
    }

    public class DoomDoom : MonoBehaviour
    {
        public void Update()
        {
            if (transform.parent == null) Destroy(gameObject);
        }

        public void Die() => Destroy(gameObject);
    }

    [HarmonyPatch(typeof(UltimateGatlingBlover))]
    public static class UltimateGatlingBloverPatch
    {
        [HarmonyPatch(nameof(UltimateGatlingBlover.AttributeEvent))]
        [HarmonyPrefix]
        public static bool PreAttributeEvent(UltimateGatlingBlover __instance)
        {
            if (__instance != null && (__instance.thePlantType == DoomGatlingBlover.PlantID || __instance.thePlantType == UltimateDoomGatlingBlover.PlantID))
            {
                __instance.attributeCount++;
                var shoot = __instance.shoot.position;
                var blover = __instance.GetComponent<DoomGatlingBlover>();
                var bulletType = blover.GetBulletType();

                var bullet = CreateBullet.Instance.SetBullet(shoot.x, shoot.y, __instance.thePlantRow, bulletType, BulletMoveWay.MoveRight);
                if (bullet == null) return false;

                bullet.Damage = __instance.attackDamage;
                bullet.fromType = __instance.thePlantType;

                if (blover.IsBigShoot())
                {
                    bullet.theStatus = BulletStatus.Doom_big;
                    bullet.Damage *= 6;
                    __instance.attributeCount = 0;
                }

                GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(UltimateGatlingBlover.DieEventMustExecute))]
        [HarmonyPrefix]
        public static bool DieEventMustExecute(UltimateGatlingBlover __instance, ref Plant.DieReason reason)
        {
            if (__instance != null && reason == Plant.DieReason.ByShovel)
            {
                if (__instance.thePlantType == DoomGatlingBlover.PlantID)
                {
                    Lawnf.SetDroppedCard(__instance.shoot.position, PlantType.DoomGatling);
                    return false;
                }
                else if (__instance.thePlantType == UltimateDoomGatlingBlover.PlantID)
                {
                    Lawnf.SetDroppedCard(__instance.shoot.position, PlantType.UltimateDoomGatling);
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_superCherry))]
    public static class Bullet_superCherryPatch
    {
        [HarmonyPatch(nameof(Bullet_superCherry.HitZombie))]
        [HarmonyPrefix]
        public static void Prefix(Bullet_superCherry __instance, ref Zombie zombie)
        {
            if (Lawnf.TravelAdvanced(UltimateDoomGatlingBlover.BuffID) && __instance.fromType == PlantType.UltimateGatling)
            {
                __instance.Damage *= 6;
                if (zombie.GetAttrTimers().coldTimer > 0f)
                {
                    if (zombie.freezeSpeed != 0f)
                        zombie.SetFreeze(1f);
                    __instance.Damage *= 4;
                }

                if (zombie.GetAttrTimers().isJalaed)
                    zombie.JalaedExplode(true, __instance.Damage);

                if (zombie.GetAttrTimers().poisonTimer > 0f)
                    zombie.DamagedByPoison(__instance._damage / 40f);
                if (DoomBomb.HasBomb(zombie))
                {
                    var bomb = DoomBomb.TryAddBomb(zombie);
                    if (bomb == null) return;
                    bomb.AddDamage(__instance.Damage * 5);
                    bomb.SmallBomb();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_cherrySquash))]
    public static class Bullet_cherrySquashPatch
    {
        [HarmonyPatch(nameof(Bullet_cherrySquash.HitZombie))]
        [HarmonyPrefix]
        public static void Prefix(Bullet_cherrySquash __instance, ref Zombie zombie)
        {
            if (Lawnf.TravelAdvanced(UltimateDoomGatlingBlover.BuffID) && __instance.fromType == PlantType.UltimateGatling)
            {
                if (zombie.GetAttrTimers().coldTimer > 0f)
                {
                    if (zombie.freezeSpeed != 0f)
                        zombie.SetFreeze(1f);
                    __instance.Damage *= 4;
                }

                if (zombie.GetAttrTimers().isJalaed)
                    zombie.JalaedExplode(true, __instance.Damage);

                if (zombie.GetAttrTimers().poisonTimer > 0f)
                    zombie.DamagedByPoison(__instance._damage / 40f);
                if (DoomBomb.HasBomb(zombie))
                {
                    var bomb = DoomBomb.TryAddBomb(zombie);
                    if (bomb == null) return;
                    bomb.AddDamage(__instance.Damage * 5);
                    bomb.SmallBomb();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_doom_ulti))]
    public static class Bullet_doom_ultiPatch
    {
        [HarmonyPatch(nameof(Bullet_doom_ulti.HitZombie))]
        [HarmonyPrefix]
        public static void PreHitZombie(Bullet_doom_ulti __instance, ref Zombie zombie)
        {
            if (Lawnf.TravelAdvanced(UltimateDoomGatlingBlover.BuffID) && __instance.fromType == UltimateDoomGatlingBlover.PlantID)
            {
                var bomb = DoomBomb.TryAddBomb(zombie);
                if (bomb == null) return;
                bomb.AddDamage(__instance.Damage);
                bomb.SmallBomb();
            }
        }
    }
}