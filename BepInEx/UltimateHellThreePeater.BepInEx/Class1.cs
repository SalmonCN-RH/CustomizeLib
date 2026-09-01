using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.Extra.ZombieExtra;
using CustomizeLib.BepInEx.UnmanagedTools;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;
using System.Reflection;
using UnityEngine;

namespace UltimateHellThreePeater.BepInEx
{
    [BepInPlugin("salmon.ultimatehellthreepeater", "UltimateHellThreePeater", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "ultimatehellthreepeater");
            CustomCore.RegisterCustomPlant<SuperThreePeater_sp, UltimateHellThreePeater>(UltimateHellThreePeater.PlantID,
                ab.GetAsset<GameObject>("UltimateHellThreePeaterPrefab"), ab.GetAsset<GameObject>("UltimateHellThreePeaterPreview"),
                new(), 1.5f, 0f, 180, 300, 50f, 775);
            CustomCore.RegisterCustomPlantSkin<SuperThreePeater_sp, UltimateHellThreePeater>(UltimateHellThreePeater.PlantID,
                ab.GetAsset<GameObject>("UltimateHellThreePeaterSkinPrefab"), ab.GetAsset<GameObject>("UltimateHellThreePeaterSkinPreview"),
                new(), 1.5f, 0f, 180, 300, 50f, 775, new List<(BulletType, List<GameObject?>)>
                {
                    (UltimateHellThreePeater.BulletID, new() { ab.GetAsset<GameObject>("Bullet_hellPea_skin") }),
                    (UltimateHellThreePeater.BulletFireID, new() { ab.GetAsset<GameObject>("Bullet_red_hellPea_skin") }),
                });
            CustomCore.RegisterCustomBullet<Bullet_firePea_super, Bullet_utlimateHellPea>(UltimateHellThreePeater.BulletID, ab.GetAsset<GameObject>("Bullet_hellPea"));
            CustomCore.RegisterCustomBullet<Bullet_firePea_super, Bullet_utlimateHellPea>(UltimateHellThreePeater.BulletFireID, ab.GetAsset<GameObject>("Bullet_red_hellPea"));
            CustomCore.RegisterCustomParticle(UltimateHellThreePeater.ParticleID, ab.GetAsset<GameObject>("FireFreeBlack"));
            CustomCore.RegisterCustomParticle(UltimateHellThreePeater.SkinParticleID, ab.GetAsset<GameObject>("FireFreeBlue"));

            CustomCore.AddPlantAlmanacStrings(UltimateHellThreePeater.PlantID, $"究极邪火射手",
                "凤凰已堕，黑焰骤起。\n" +
                "<color=#0000FF>究极浴火射手的特殊形态</color>\n\n" +
                "<color=#3D1400>使用条件：</color><color=#3D1400>①</color><color=red>种植究极浴火射手有2%概率变异</color>\n" +
                "<color=#3D1400>②</color><color=red>神秘模式</color>\n" +
                "<color=red>*可使用火爆辣椒切回究极浴火射手</color>\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>180x5/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=#3D1400>①</color><color=red>每轮向全场每行发射一发邪火子弹，可穿透2次，伤害后会同时施加红温状态和焚烧状态</color>\n" +
                "<color=#3D1400>②</color><color=red>在场时，所有火爆辣椒效果改为邪火爆辣椒效果：伤害后额外造成10%的真实伤害</color>\n" +
                "<color=#3D1400>③</color><color=red>出场或死亡时，在全场每行：释放邪火爆辣椒效果6次并发射邪火子弹6发</color>\n" +
                "<color=#3D1400>焚烧状态：</color><color=red>持续时间无限。处于该状态下，每1秒损失0.5%血量和韧性</color>\n\n" +
                "<color=#3D1400>null</color>");

            ClassInjector.RegisterTypeInIl2Cpp<BurnEffect>();
            CustomCore.RegisterCustomEffect(UltimateHellThreePeater.EffectID, (zombie, _, _) => new BurnEffect(zombie.Cast<Zombie>()).Construct());
            CustomCore.AddFusion(PlantType.SuperThreePeater_sp, UltimateHellThreePeater.PlantID, PlantType.Jalapeno);
            CustomCore.AddUltimatePlant(UltimateHellThreePeater.PlantID);
            CustomCore.TypeMgrExtra.LevelPlants.Add(UltimateHellThreePeater.PlantID, CardLevel.Red);
        }
    }

    public class UltimateHellThreePeater : MonoBehaviour
    {
        public static ID PlantID = 1940;
        public static ID BulletID = 1940;
        public static ID BulletFireID = 1941;
        public static ID EffectID = 1940;
        public static ID ParticleID = 1940;
        public static ID SkinParticleID = 1941;

        public void Awake()
        {
            plant.shoot = transform.FindChild("New/head2/mouth/mouth (1)");
        }

        public async Task DieEvent() // il2cpp下UniTask/UniTaskVoid不能作为返回值，所以直接使用System的Task
        {
            float x = plant.axis.position.x;
            int damage = plant.attackDamage;
            var plantType = plant.thePlantType;
            var board = plant.board;

            for (int cnt = 0; cnt < 6; cnt++)
            {
                for (int row = 0; row < board.rowNum; row++)
                {
                    board.boardAction.CreateFireLine(row, 10 * damage, fromType: plantType);
                    float y = Mouse.Instance.GetLandY(x, row);
                    Bullet bullet = CreateBullet.Instance.SetBullet(x, y + 0.7f, row, BulletID, BulletMoveWay.MoveRight_threePeater);
                    bullet.Damage = damage;
                    bullet.fromType = plantType;
                }

                // il2cpp下cancellationToken必须传参，如果不需要也得传None，否则会直接崩溃
                await UniTask.Delay(167, cancellationToken: board.GetCancellationTokenOnDestroy());
            }
        }

        public SuperThreePeater_sp plant => gameObject.GetComponent<SuperThreePeater_sp>();
    }

    public class BurnEffect : ZombieEffect
    {
        private IntPtr BaseType => Il2CppClassPointerStore<ZombieEffect>.NativeClassPtr;
        private static ID Color = 1940;

        #region 构造函数
        public BurnEffect(IntPtr ptr) : base(ptr) { }
        public BurnEffect() : base(ClassInjector.DerivedConstructorPointer<BurnEffect>()) =>
            ClassInjector.DerivedConstructorBody(this);
        public BurnEffect(Zombie zombie) : this() => this.zombie = zombie;
        #endregion
        private float attackTimer = 0f;

        public override EffectType EffectType => UltimateHellThreePeater.EffectID;

        internal BurnEffect Construct()
        {
            first = true;
            attackTimer = 0f;
            if (zombie.TryGetEffect<BurnEffect>(EffectType, out var exist))
            {
                attackTimer = Mathf.Max(attackTimer, exist.attackTimer);
                InheritFields(exist);
            }
            return this;
        }

        public override void OnStart()
        {
            zombie.AddColor(new(0.8f, 0f, 0f), Color);
            ClassTools.Call(IL2CPP.GetIl2CppMethod(BaseType, false, "OnStart", ClassTools.Void), Pointer);
        }

        public override void OnUpdate()
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= 1f)
            {
                var (health, first, second) = (zombie.theHealth, zombie.theFirstArmorHealth, zombie.theSecondArmorHealth);
                zombie.TakeDamage((int)(0.005f * zombie.TotalAllHealth), null, DamageType.NormalAll, UltimateHellThreePeater.PlantID, true);
                var (newHealth, newFirst, newSecond) = (zombie.theHealth, zombie.theFirstArmorHealth, zombie.theSecondArmorHealth);
                var (calHealth, calFirst, calSecond) = (health - newHealth, first - newFirst, second - newSecond);
                zombie.theMaxHealth -= calHealth;
                zombie.theFirstArmorMaxHealth -= calFirst;
                zombie.theSecondArmorMaxHealth -= calSecond;
                zombie.theMaxHealth = (long)Mathf.Max(zombie.theMaxHealth, 0);
                zombie.theFirstArmorMaxHealth = Mathf.Max(zombie.theFirstArmorMaxHealth, 0);
                zombie.theSecondArmorMaxHealth = Mathf.Max(zombie.theSecondArmorMaxHealth, 0);
                zombie.UpdateHealthText();
                attackTimer = 0f;
            }
            ClassTools.Call(IL2CPP.GetIl2CppMethod(BaseType, false, "OnUpdate", ClassTools.Void), Pointer);
        }

        public override void OnRemove()
        {
            zombie.RemoveColor(Color);
            ClassTools.Call(IL2CPP.GetIl2CppMethod(BaseType, false, "OnRemove", ClassTools.Void), Pointer);
        }
    }

    public class Bullet_utlimateHellPea : MonoBehaviour
    {
        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.IsObjExist()) return;
            if (!collision.TryGetComponent<UltimateTorch>(out var torch)) return;
            if (bullet.torchWood == torch) return;
            if (bullet.theBulletType == UltimateHellThreePeater.BulletFireID) return;
            bullet.board.boardAction.FirePeas(bullet, torch, bullet.Damage * 3 , UltimateHellThreePeater.BulletFireID);
            torch.fireTimes++;
        }

        public Bullet_firePea_super bullet => gameObject.GetComponent<Bullet_firePea_super>();
    }

    [HarmonyPatch(typeof(SuperThreePeater))]
    public static class SuperThreePeaterPatch
    {
        [HarmonyPatch(nameof(SuperThreePeater.GetBulletType))]
        [HarmonyPostfix]
        public static void PostGetBulletType(SuperThreePeater __instance, ref BulletType __result)
        {
            if (__instance.thePlantType == UltimateHellThreePeater.PlantID)
                __result = UltimateHellThreePeater.BulletID;
        }
    }

    [HarmonyPatch(typeof(SuperThreePeater_sp))]
    public static class SuperThreePeater_spPatch
    {
        [HarmonyPatch(nameof(SuperThreePeater_sp.DieEvent))]
        [HarmonyPrefix]
        public static bool PreDieEvent(SuperThreePeater_sp __instance)
        {
            if (__instance.thePlantType == UltimateHellThreePeater.PlantID)
            {
                _ = __instance.GetComponent<UltimateHellThreePeater>().DieEvent();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_firePea_super))]
    public static class Bullet_firePea_superPatch
    {
        [HarmonyPatch(nameof(Bullet_firePea_super.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_firePea_super __instance, ref Zombie zombie)
        {
            if (__instance.fromType == UltimateHellThreePeater.PlantID)
            {
                if (zombie == null) return false;
                zombie.TakeDamage(__instance.Damage, __instance.Cast<IDamageMaker>(), DamageType.Shieldless, __instance.fromType);
                zombie.SetJalaed();
                if (CoreTools.TravelAdvanced("百步穿杨"))
                {
                    zombie.JalaedExplode(damage: __instance._damage);
                }
                zombie.SetEffect(UltimateHellThreePeater.EffectID);
                GameAPP.PlaySound(UnityEngine.Random.Range(59, 61), 0.5f, 1f);
                var particle = CreateParticle.SetParticle(__instance.gameObject.name.Contains("skin") ? UltimateHellThreePeater.SkinParticleID : UltimateHellThreePeater.ParticleID, __instance.transform.position, __instance.theBulletRow);
                UnityEngine.Object.Destroy(particle, 0.5f);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BoardAction))]
    public static class BoardActionPatch
    {
        [HarmonyPatch(nameof(BoardAction.CreateFireLine))]
        [HarmonyPrefix]
        public static void PreCreateFireLine(BoardAction __instance, ref int damage, ref bool fromZombie, ref Il2CppSystem.Action<Zombie> action, 
            ref PlantType fromType)
        {
            if ((__instance.board.ObjectExist<UltimateHellThreePeater>() || fromType == UltimateHellThreePeater.PlantID) && !fromZombie)
            {
                int dmg = damage;
                Action<Zombie> add = (z) =>
                {
                    z.TakeDamage(dmg / 10, null, DamageType.NormalAll, UltimateHellThreePeater.PlantID, true);
                };
                action += add;
            }
        }
    }

    [HarmonyPatch(typeof(CreatePlant))]
    public static class CreatePlantPatch
    {
        [HarmonyPatch(nameof(CreatePlant.SetPlant))]
        [HarmonyPrefix]
        public static void PostSetPlant(ref PlantType theSeedType)
        {
            if (theSeedType == PlantType.SuperThreePeater_sp && GameAPP.theGameStatus == GameStatus.InGame)
            {
                var prop = 2 + (CoreTools.TravelAdvanced("怒火攻心") ? 4 : 0) + (CoreTools.TravelAdvanced("百步穿杨") ? 4 : 0);
                if (UnityEngine.Random.Range(0, 100) < prop)
                    theSeedType = UltimateHellThreePeater.PlantID;
            }
        }
    }
}
