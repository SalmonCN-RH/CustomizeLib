using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using Unity.VisualScripting;
using UnityEngine;
using static Il2CppSystem.Uri;

[assembly: MelonInfo(typeof(UltimateWinterMelon.Core), "UltimateWinterMelon", "1.0.0", "Salmon")]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
namespace UltimateWinterMelon;
public class Core : MelonMod
{
    public static GameObject ultimateWinterMelonParticlePrefab = null;

    public override void OnInitializeMelon()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatewintermelon");
        ultimateWinterMelonParticlePrefab = ab.GetAsset<GameObject>("UltimateWinterMelonParicle");
        CustomCore.RegisterCustomBullet<Bullet_winterMelon, Bullet_ultimateWinterMelon>((BulletType)Bullet_ultimateWinterMelon.Bullet_ID, ab.GetAsset<GameObject>("Bullet_ultimateWinterMelonPrefab"));
        CustomCore.RegisterCustomPlant<WinterMelon, UltimateWinterMelon>(
            UltimateWinterMelon.PlantID,
            ab.GetAsset<GameObject>("UltimateWinterMelonPrefab"),
            ab.GetAsset<GameObject>("UltimateWinterMelonPreview"),
            new List<(int, int)>
            {
                ((int)PlantType.WinterMelon, (int)PlantType.PortalDoom),
                ((int)PlantType.PortalDoom, (int)PlantType.WinterMelon),
            },
            3f, 0f, 300, 300, 0, 700
        );
        CustomCore.AddPlantAlmanacStrings(UltimateWinterMelon.PlantID,
            $"究极超时空西瓜投手({UltimateWinterMelon.PlantID})",
            "极寒与时空之力，僵尸越多，僵尸越少。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>300*2/3秒</color>\n<color=#3D1400>特点：</color><color=red>铁植物。子弹对一类防具的僵尸造成3倍伤害，攻击施加寒冷状态（等级21亿），施加更强的减速效果并赋予75冻结值，对冻结的僵尸伤害x4，命中的僵尸有10%概率击退3格，有10%概率传送在本行最右侧。每次攻击有概率开大，开大时投掷到天上，使本行头顶召唤1～3个超时空西瓜，初始概率为10%，每次攻击增加5%，开大后重置到10%，如果没有僵尸则不重置概率。</color>\n<color=#3D1400>融合配方：</color><color=red>超时空毁灭菇+冰瓜</color>\n\n<color=#3D1400>他诞生于宇宙的终焉，踏着时间的逆流溯游而行。他目睹了常人无法想象的奇景——文明的余烬重燃为璀璨的星火；散落的梦境碎片重新拼凑成完整的画卷；末日的灰烬倒卷回创世的黎明。而此刻，他将以这趟逆行之旅，将终焉的轨迹引向命定的彼岸。</color>"
        );
        CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)UltimateWinterMelon.PlantID);
        CustomCore.TypeMgrExtra.IsMagnetPlants.Add((PlantType)UltimateWinterMelon.PlantID);
        // CustomCore.TypeMgrExtra.IsPuff.Contains(PlantType.SunGatlingPuff);\
    }
}

[RegisterTypeInIl2Cpp]
public class UltimateWinterMelon : MonoBehaviour
{
    public static int PlantID = 991;

    public int superShoot = 10;
    public TextMeshPro superShootText = null;
    public TextMeshPro superShootTextShadow = null;
    public GameObject Tground = null;
    public float rotateCountdown = 0f;

    public WinterMelon plant => gameObject.GetComponent<WinterMelon>();

    public void Update()
    {
        if (plant != null)
            plant.thePlantAttackInterval = 2f;
        UpdateText();
        rotateCountdown -= Time.fixedUnscaledDeltaTime;
        if (GameAPP.theGameStatus == GameStatus.Almanac && rotateCountdown <= 0f && Time.timeScale == 0f)
        {
            Tground.transform.Rotate(0, 0, -1.8f);
            rotateCountdown = 0.02f;
        }
        if (plant != null && plant.healthSlider != null && plant.healthSlider.healthText != null && plant.healthSlider.healthTextShadow != null)
        {
            superShootText.gameObject.SetActive(plant.healthSlider.gameObject.active);
            superShootTextShadow.gameObject.SetActive(plant.healthSlider.gameObject.active);
        }
        try
        {
            if (AlmanacMenu.Instance.currentShowCtrl.localShowPlant.name == this.plant.gameObject.name)
            {
                superShootText.gameObject.SetActive(false);
                superShootTextShadow.gameObject.SetActive(false);
            }
        }
        catch (Exception)
        {
        }
    }

    public void FixedUpdate()
    {
        if (Tground != null)
            Tground.transform.Rotate(0, 0, -1.8f);

    }

    public void Awake()
    {
        plant.shoot = plant.gameObject.transform.GetChild(0);
        plant.textHead = plant.gameObject.transform.GetChild(1).gameObject;
        InitText();
        Tground = plant.gameObject.transform.FindChild("body").FindChild("Tground").gameObject;
    }

    public void InitText()
    {
        Transform textHead = plant.textHead.transform;
        textHead.position = new Vector3(textHead.position.x, textHead.position.y + 0.3f, 0f);
        if (superShootText == null)
            superShootText = plant.SetPlantText("大招概率", Color.cyan, new Vector2(0f, -0.4f), textHead, $"大招概率:{superShoot}%", 20);
        if (superShootTextShadow == null)
            superShootTextShadow = plant.SetPlantText("大招概率", Color.black, new Vector2(0.01f, -0.41f), textHead, $"大招概率:{superShoot}%", 19);
    }

    public void UpdateText()
    {
        if (superShootText == null || superShootTextShadow == null)
            InitText();
        superShootText.text = $"大招概率:{superShoot}%";
        superShootTextShadow.text = $"大招概率:{superShoot}%";
        superShootText.fontSize = 2.3f;
        superShootTextShadow.fontSize = 2.3f;
    }
    public void SuperShoot()
    {
        GameAPP.PlaySound(4, 1.0f);
        int attackCount = 10;
        bool attack = false;
        foreach (Zombie z in GameAPP.board.GetComponent<Board>().zombieArray)
        {
            int r = UnityEngine.Random.Range(0, 2);
            if (r == 0 || attackCount > 0)
            {
                if (z != null && z.theStatus != ZombieStatus.Dying && !z.beforeDying && z.theZombieRow == plant.thePlantRow && z.axis.transform.position.x > plant.axis.transform.position.x - 0.5f && !z.isMindControlled)
                {
                    int random = UnityEngine.Random.Range(1, 4);
                    for (int i = 0; i < random; i++)
                    {
                        var RowFromY = Mouse.Instance.GetRowFromY(z.axis.transform.position.x, z.axis.transform.position.y);
                        var bullet = plant.board.GetComponent<CreateBullet>().SetBullet(plant.shoot.transform.position.x, plant.shoot.transform.position.y, RowFromY, (BulletType)Bullet_ultimateWinterMelon.Bullet_ID, (int)BulletMoveWay.Cannon);
                        var pos2 = bullet.cannonPos;
                        pos2.x = z.axis.transform.position.x - 0.15f;
                        pos2.y = z.axis.transform.position.y;
                        bullet.cannonPos = pos2;
                        // bullet.theStatus = BulletStatus.Melon_cannon;
                        bullet.Damage = plant.attackDamage;
                        bullet.from = plant;
                    }
                    attackCount--;
                    attack = true;
                }
            }
        }
        if (attack)
            superShoot = 10;
    }
}

[RegisterTypeInIl2Cpp]
public class Bullet_ultimateWinterMelon : MonoBehaviour
{
    public Bullet_winterMelon bullet => gameObject.GetComponent<Bullet_winterMelon>();

    public static int Bullet_ID = 1905;
}

[HarmonyPatch(typeof(Bullet_winterMelon), nameof(Bullet_winterMelon.HitZombie))]
public class Bullet_winterMelon_HitZombie
{
    [HarmonyPrefix]
    public static bool Prefix(Bullet_winterMelon __instance, ref Zombie zombie)
    {
        if (__instance != null && (int)__instance.theBulletType == Bullet_ultimateWinterMelon.Bullet_ID)
        {
            Vector3 position = __instance.transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(__instance.transform.position, 3f, zombie.zombieLayer);
            foreach (Collider2D collider in colliders)
            {
                if (collider is not null && !collider.IsDestroyed() && collider.TryGetComponent<Zombie>(out var z) && z is not null && !z.isMindControlled && !z.IsDestroyed() &&
                    (__instance.theBulletRow == z.theZombieRow || z.theZombieRow == __instance.theBulletRow + 1 || z.theZombieRow == __instance.theBulletRow - 1) )
                {
                    int damage = __instance.Damage;
                    if (z.freezeTimer > 0)
                        damage *= 4;
                    if (z.theFirstArmorHealth > 0)
                        damage *= 3;
                    z.TakeDamage(DmgType.IceAll, damage);
                    z.AddfreezeLevel(75);
                    z.SetCold(10, int.MaxValue);

                    z.coldSpeed = 0.05f;
                    z.coldSpeed *= 0.15f;

                    int knockback = UnityEngine.Random.Range(0, 5);
                    if (knockback == 0)
                    {
                        z.KnockBack(z.axis.transform.position.x + 3f);
                    }

                    int tp = UnityEngine.Random.Range(0, 10);
                    if (tp == 0 && z.theZombieType != ZombieType.UltimateSnowZombie)
                    {
                        Vector3 pos = z.axis.transform.position;
                        pos.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                        Vector3 pos_trans = z.transform.position;
                        pos_trans.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                        z.transform.position = pos_trans;
                        z.axis.transform.position = pos;
                    }
                }
            }
            if (Core.ultimateWinterMelonParticlePrefab != null)
            {
                Transform parent = Board.Instance.transform;

                // 实例化粒子对象
                GameObject particle = UnityEngine.Object.Instantiate(
                    Core.ultimateWinterMelonParticlePrefab,
                    position,
                    Quaternion.identity,
                    parent
                );

                CreateParticle.SetLayer(particle, zombie.theZombieRow);
            }
            int soundID = UnityEngine.Random.Range(104, 106);
            GameAPP.PlaySound(soundID, 0.5f, 1f);
            __instance.Die();
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Bullet_winterMelon), nameof(Bullet_winterMelon.HitLand))]
public class Bullet_winterMelon_HitLand
{
    [HarmonyPrefix]
    public static bool Prefix(Bullet_winterMelon __instance)
    {
        if (__instance != null && (int)__instance.theBulletType == Bullet_ultimateWinterMelon.Bullet_ID)
        {
            Vector3 position = __instance.transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(__instance.transform.position, 3f, __instance.zombieLayer);
            foreach (Collider2D collider in colliders)
            {
                if (collider is not null && !collider.IsDestroyed() && collider.TryGetComponent<Zombie>(out var z) && z is not null && !z.isMindControlled && !z.IsDestroyed() &&
                    (__instance.theBulletRow == z.theZombieRow || z.theZombieRow == __instance.theBulletRow + 1 || z.theZombieRow == __instance.theBulletRow - 1))
                {
                    int damage = __instance.Damage;
                    if (z.freezeTimer > 0)
                        damage *= 4;
                    if (z.theFirstArmorHealth > 0)
                        damage *= 3;
                    z.TakeDamage(DmgType.IceAll, damage);
                    z.AddfreezeLevel(75);
                    z.SetCold(10, int.MaxValue);


                    z.coldSpeed = 0.05f;
                    z.coldSpeed *= 0.15f;

                    int knockback = UnityEngine.Random.Range(0, 5);
                    if (knockback == 0)
                    {
                        z.KnockBack(z.axis.transform.position.x + 3f);
                    }

                    int tp = UnityEngine.Random.Range(0, 10);
                    if (tp == 0 && z.theZombieType != ZombieType.UltimateSnowZombie)
                    {
                        Vector3 pos = z.axis.transform.position;
                        pos.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                        Vector3 pos_ = z.transform.position;
                        pos_.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                        z.axis.transform.position = pos;
                        z.transform.position = pos_;
                    }
                }
            }
            if (Core.ultimateWinterMelonParticlePrefab != null)
            {
                Transform parent = Board.Instance.transform;

                // 实例化粒子对象
                GameObject particle = UnityEngine.Object.Instantiate(
                    Core.ultimateWinterMelonParticlePrefab,
                    position,
                    Quaternion.identity,
                    parent
                );

                CreateParticle.SetLayer(particle, __instance.theBulletRow);
            }
            int soundID = UnityEngine.Random.Range(104, 106);
            GameAPP.PlaySound(soundID, 0.5f, 1f);
            __instance.Die();
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Bullet), nameof(Bullet.Update))]
public class Bullet_Update
{
    [HarmonyPrefix]
    public static bool Prefix(Bullet __instance)
    {
        if (__instance != null && (int)__instance.theBulletType == Bullet_ultimateWinterMelon.Bullet_ID && __instance.theMovingWay == (int)BulletMoveWay.Cannon)
        {
            Vector3 position = __instance.transform.position;
            position.y -= Time.deltaTime * __instance.detaVy;
            __instance.transform.position = position;
            __instance.theExistTime += Time.deltaTime;
            if (position.y < (__instance.cannonPos.y + 0.5f) && __instance.theBulletRow == __instance.from.thePlantRow)
            {
                if (__instance.targetZombie == null)
                    __instance.HitLand();
                else
                    __instance.HitZombie(__instance.targetZombie);
            }
            Vector3 point = Camera.main.WorldToScreenPoint(position);
            if (point.y < 0)
                __instance.Die();
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(WinterMelon), nameof(WinterMelon.GetBulletType))]
public class WinterMelon_GetBulletType
{
    [HarmonyPostfix]
    public static void Postfix(WinterMelon __instance, ref BulletType __result)
    {
        if (__instance != null && (int)__instance.thePlantType == UltimateWinterMelon.PlantID)
        {
            __result = (BulletType)Bullet_ultimateWinterMelon.Bullet_ID;
        }
    }
}

[HarmonyPatch(typeof(Shooter), nameof(WinterMelon.AnimShoot))]
public class Shooter_AnimShoot
{
    [HarmonyPrefix]
    public static void Prefix(Shooter __instance)
    {
        if (__instance != null && (int)__instance.thePlantType == UltimateWinterMelon.PlantID)
        {
            PotEffects.MelonPotEffect(__instance, __instance.thePlantColumn, __instance.thePlantRow);
            if (__instance.melonSputter)
                __instance.attackDamage = 500;
            else if (__instance.attackDamage == 500)
                __instance.attackDamage = 300;
        }
    }

    [HarmonyPostfix]
    public static void Postfix(Shooter __instance)
    {
        if (__instance != null && (int)__instance.thePlantType == UltimateWinterMelon.PlantID)
        {
            UltimateWinterMelon plant = __instance.GetComponent<UltimateWinterMelon>();
            plant.superShoot += 5;
            plant.superShoot = plant.superShoot > 100 ? 100 : plant.superShoot;
            int random = UnityEngine.Random.Range(0, 100);
            if (plant.superShoot == 100)
                plant.SuperShoot();
            if (random <= plant.superShoot)
                plant.SuperShoot();
        }
    }
}