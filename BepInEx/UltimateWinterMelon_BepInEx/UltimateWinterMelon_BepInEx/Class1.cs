using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;
using TMPro;
using Unity.VisualScripting;

namespace UltimateWinterMelonBepInEx
{
    [BepInPlugin("salmon.ultimatewintermelon", "UltimateWinterMelon", "1.0")]
    public class Core : BasePlugin//304
    {
        public static GameObject ultimateWinterMelonParticlePrefab = null;
        public override void Load()
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
                var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "ultimatewintermelon");
                ultimateWinterMelonParticlePrefab = ab.GetAsset<GameObject>("UltimateWinterMelonParicle");

                ClassInjector.RegisterTypeInIl2Cpp<UltimateWinterMelon>();
                ClassInjector.RegisterTypeInIl2Cpp<Bullet_ultimateWinterMelon>();
                ClassInjector.RegisterTypeInIl2Cpp<ZombieIceExtension>();

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
                    "极寒与时空之力，僵尸越多，僵尸越少。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>300*2/3秒</color>\n<color=#3D1400>融合配方：</color><color=red>超时空毁灭菇+冰瓜</color>\n<color=#3D1400>特点：</color><color=red>铁植物。子弹对一类防具的僵尸造成3倍伤害，攻击施加寒冷状态，对冻结状态的僵尸伤害x4。命中的僵尸有10%概率击退三格或者传送到本行最右侧。每次攻击有概率开大，开大时将子弹投掷到天上，在本行的每个僵尸降下1～3个超时空西瓜，初始概率为10%，每次攻击增加5%概率，开大后重置为10%，如果开大时本行没有僵尸则不重置开大概率。</color>\n<color=#3D1400>词条1：</color><color=red>凛冬将至：究极超时空西瓜投手每次攻击都会对攻击范围内的僵尸造成1点极冻值。</color>\n<color=#3D1400>词条2：</color><color=red>白洞：开大时有10%概率让本行所有僵尸移到本行最右侧，大招的子弹伤害×2。</color>\n<color=#3D1400>极冻值：</color><color=red>极冻值上限为75层。每拥有1层极冻值，僵尸减速5%，最低速度为30%。当僵尸拥有50层极冻值时，超时空西瓜将额外造成僵尸当前所有生命值的5%的伤害。当僵尸拥有75层极冻值时，超时空西瓜将额外造成僵尸当前所有生命值的15%的伤害。当僵尸减速效果消失时，极冻值归0。</color>\n\n<color=#3D1400>他诞生于宇宙的终焉，踏着时间的逆流溯游而行。他目睹了常人无法想象的奇景——文明的余烬重燃为璀璨的星火；散落的梦境碎片重新拼凑成完整的画卷；末日的灰烬倒卷回创世的黎明。而此刻，他将以这趟逆行之旅，将终焉的轨迹引向命定的彼岸。</color>"
                );
                CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)UltimateWinterMelon.PlantID);
                CustomCore.TypeMgrExtra.IsMagnetPlants.Add((PlantType)UltimateWinterMelon.PlantID);
                UltimateWinterMelon.Buff1 = CustomCore.RegisterCustomBuff("凛冬将至：究极超时空西瓜对命中的僵尸附带1层极冻值。", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<UltimateWinterMelon>(), 5000, "#DA64FF", (PlantType)UltimateWinterMelon.PlantID);
                UltimateWinterMelon.Buff2 = CustomCore.RegisterCustomBuff("白洞：开大时有10%概率让本行所有僵尸移到本行最右侧，大招的子弹伤害×2。", BuffType.AdvancedBuff, () => Lawnf.TravelAdvanced(UltimateWinterMelon.Buff1), 5000, "#DA64FF", (PlantType)UltimateWinterMelon.PlantID);
            }
            catch (Exception) { }
        }
    }

    public class UltimateWinterMelon : MonoBehaviour
    {
        public static int PlantID = 991;
        public static int Buff1 = -1;
        public static int Buff2 = -1;

        public int superShoot = 10;
        public TextMeshPro superShootText = null;
        public TextMeshPro superShootTextShadow = null;
        public GameObject Tground = null;
        // public float rotateCountdown = 0.02f; //旋转间隔

        public WinterMelon plant => gameObject.GetComponent<WinterMelon>();

        public void Update() //每帧执行（不受deltaTime影响）
        {
            try
            {
                if (plant != null) //空值判断
                    plant.thePlantAttackInterval = 2f;
                UpdateText(); //更新血条
                              // rotateCountdown -= Time.fixedUnscaledDeltaTime; // 旋转间隔减少不受timeScale影响的deltaTime
                if (Tground != null)
                {
                    // 每秒旋转90度，按帧时间缩放
                    Tground.transform.Rotate(0, 0, -90f * Time.deltaTime);
                }
                if (GameAPP.theGameStatus == GameStatus.Almanac && Tground != null && Time.timeScale == 0) // 如果黑洞不为空且当前游戏为图鉴状态且时间速率为0
                {
                    Tground.transform.Rotate(0, 0, -90f * Time.unscaledDeltaTime);
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
            catch (Exception) { }
        }

        public void Awake()
        {
            try
            {
                plant.shoot = plant.gameObject.transform.FindChild("Shoot");
                plant.textHead = plant.gameObject.transform.FindChild("TextHead").gameObject;
                InitText();
                Tground = plant.gameObject.transform.FindChild("body").FindChild("Tground").gameObject;
            }
            catch (Exception) { }
        }

        public void InitText()
        {
            try
            {
                Transform textHead = plant.textHead.transform;
                textHead.position = new Vector3(textHead.position.x, textHead.position.y + 0.3f, 0f);
                if (superShootText == null)
                    superShootText = plant.SetPlantText("大招概率", Color.cyan, new Vector2(0f, -0.4f), textHead, $"大招概率:{superShoot}%", 20);
                if (superShootTextShadow == null)
                    superShootTextShadow = plant.SetPlantText("大招概率", Color.black, new Vector2(0.01f, -0.41f), textHead, $"大招概率:{superShoot}%", 19);
            }
            catch (Exception) { }
        }

        public void UpdateText()
        {
            try
            {
                if (superShootText == null || superShootTextShadow == null)
                    InitText();
                superShootText.text = $"大招概率:{superShoot}%";
                superShootTextShadow.text = $"大招概率:{superShoot}%";
                superShootText.fontSize = 2.3f;
                superShootTextShadow.fontSize = 2.3f;
            }
            catch (Exception) { }
        }
        public void SuperShoot()
        {
            try
            {
                GameAPP.PlaySound(4, 1.0f);
                int attackCount = 10;
                bool attack = false;
                int tp = UnityEngine.Random.Range(0, 10);
                foreach (Zombie z in GameAPP.board.GetComponent<Board>().zombieArray)
                {
                    if (tp == 0 && Lawnf.TravelAdvanced(UltimateWinterMelon.Buff2) && z.theZombieType != ZombieType.UltimateSnowZombie && z != null && z.theZombieType != ZombieType.UltimateKirovZombie && z.theStatus != ZombieStatus.Dying && !z.beforeDying && z.theZombieRow == plant.thePlantRow && z.axis.transform.position.x > plant.axis.transform.position.x - 0.5f && !z.isMindControlled)
                    {
                        ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, z.axis.transform.position, z.theZombieRow);
                        Vector3 pos = z.axis.transform.position;
                        pos.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                        Vector3 pos_trans = z.transform.position;
                        pos_trans.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                        z.transform.position = pos_trans;
                        z.axis.transform.position = pos;
                    }
                    int r = UnityEngine.Random.Range(0, 2);
                    if (r == 0 || attackCount > 0)
                    {
                        if (z != null && z.theStatus != ZombieStatus.Dying && !z.beforeDying && z.theZombieRow == plant.thePlantRow && z.axis.transform.position.x > plant.axis.transform.position.x - 0.5f && !z.isMindControlled)
                        {
                            int random = UnityEngine.Random.Range(1, 4);
                            for (int i = 0; i < random; i++)
                            {
                                var bullet = CreateBullet.Instance.SetBullet(plant.shoot.transform.position.x, plant.shoot.transform.position.y, plant.thePlantRow, (BulletType)Bullet_ultimateWinterMelon.Bullet_ID, (int)BulletMoveWay.Cannon);
                                var pos2 = bullet.cannonPos;
                                pos2.x = z.axis.transform.position.x - 0.15f;
                                pos2.y = z.axis.transform.position.y;
                                bullet.cannonPos = pos2;
                                // bullet.theStatus = BulletStatus.Melon_cannon;
                                bullet.Damage = Lawnf.TravelAdvanced(Buff2) ? plant.attackDamage * 2 : plant.attackDamage;
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
            catch (Exception) { }
        }
    }

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
            try
            {
                if (__instance != null && (int)__instance.theBulletType == Bullet_ultimateWinterMelon.Bullet_ID)
                {
                    Vector3 position = __instance.transform.position;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(__instance.transform.position, 3f, zombie.zombieLayer);
                    foreach (Collider2D collider in colliders)
                    {
                        Zombie z = null;
                        if (collider is not null && !collider.IsDestroyed() && collider.TryGetComponent<Zombie>(out z) && z is not null && !z.isMindControlled && !z.IsDestroyed() &&
                            (__instance.theBulletRow == z.theZombieRow || z.theZombieRow == __instance.theBulletRow + 1 || z.theZombieRow == __instance.theBulletRow - 1))
                        {
                            int damage = __instance.Damage;
                            if (z.freezeTimer > 0)
                                damage *= 4;
                            if (z.theFirstArmorHealth > 0)
                                damage *= 3;
                            z.AddfreezeLevel(75);
                            z.SetCold(10f, int.MaxValue);

                            ZombieIceExtension component = z.GetComponent<ZombieIceExtension>();
                            if (component.iceLevel < 75 && Lawnf.TravelAdvanced(UltimateWinterMelon.Buff1))
                                component.iceLevel++; // 极冻值<75层才加，保证极冻值总是<=75层
                            int iceLevel = component.iceLevel;

                            if (Lawnf.TravelAdvanced(UltimateWinterMelon.Buff1) && iceLevel >= 50)
                            {
                                z.coldSpeed = (float)Math.Pow(0.95, iceLevel);
                                z.coldSpeed = z.coldSpeed < 0.3f ? 0.3f : z.coldSpeed;
                                if (z.theSpeed < 0.5)
                                    damage += (int)((z.theHealth + z.theFirstArmorHealth + z.theSecondArmorHealth) * 0.05f); // 极冻值的额外增伤从总最大血量的7.5%改成当前血量的5%了
                            }

                            if (Lawnf.TravelAdvanced(UltimateWinterMelon.Buff1) && iceLevel >= 75)
                                damage += (int)((z.theHealth + z.theFirstArmorHealth + z.theSecondArmorHealth) * 0.15f);

                            z.TakeDamage(DmgType.IceAll, damage);

                            int knockback = UnityEngine.Random.Range(0, 5);
                            if (knockback == 0)
                            {
                                ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, z.axis.transform.position, z.theZombieRow);
                                z.KnockBack(z.axis.transform.position.x + 3f);
                            }

                            int tp = UnityEngine.Random.Range(0, 10);
                            if (tp == 0 && z.theZombieType != ZombieType.UltimateSnowZombie && z.theZombieType != ZombieType.UltimateKirovZombie)
                            {
                                ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, z.axis.transform.position, z.theZombieRow);
                                Vector3 pos = z.axis.transform.position;
                                pos.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                                Vector3 pos_trans = z.transform.position;
                                pos_trans.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                                z.transform.position = pos_trans;
                                z.axis.transform.position = pos;
                            }
                        }
                    }

                    // 究冰瓜瓜粒子
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
            }
            catch (Exception) { }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet_winterMelon), nameof(Bullet_winterMelon.HitLand))]
    public class Bullet_winterMelon_HitLand
    {
        [HarmonyPrefix]
        public static bool Prefix(Bullet_winterMelon __instance)
        {
            try
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
                            z.AddfreezeLevel(75);
                            z.SetCold(10f, int.MaxValue);

                            ZombieIceExtension component = z.GetComponent<ZombieIceExtension>();
                            if (component.iceLevel < 75 && Lawnf.TravelAdvanced(UltimateWinterMelon.Buff1))
                                component.iceLevel++; // 极冻值<75层才加，保证极冻值总是<=75层
                            int iceLevel = component.iceLevel;

                            if (Lawnf.TravelAdvanced(UltimateWinterMelon.Buff1) && iceLevel >= 50)
                            {
                                z.coldSpeed = (float)Math.Pow(0.95, iceLevel);
                                z.coldSpeed = z.coldSpeed < 0.3f ? 0.3f : z.coldSpeed;
                                if (z.theSpeed < 0.5)
                                    damage += (int)((z.theHealth + z.theFirstArmorHealth + z.theSecondArmorHealth) * 0.05f); // 极冻值的额外增伤从总最大血量的7.5%改成当前血量的5%了
                            }

                            if (Lawnf.TravelAdvanced(UltimateWinterMelon.Buff1) && iceLevel >= 75)
                                damage += (int)((z.theHealth + z.theFirstArmorHealth + z.theSecondArmorHealth) * 0.15f);

                            if (((z.theHealth + z.theFirstArmorHealth + z.theSecondArmorHealth) < z.portaledTimer * 30) && Lawnf.TravelAdvanced(UltimateWinterMelon.Buff2))
                            {
                                if (!z.TryGetComponent<LegionZombie>(out var legion) && !TypeMgr.IsBossZombie(z.theZombieType))
                                {
                                    z.portaledTimer = 0f;
                                    z.isStopped = false;
                                    z.UnPortaled();
                                    z.Die();
                                }
                                else
                                    damage *= 4;
                            }

                            // 如果拥有白洞词条且极冻值>=75则对领袖僵尸造成4倍增伤，普通僵尸增加伤害/5的传送时间
                            if (Lawnf.TravelAdvanced(UltimateWinterMelon.Buff2) && iceLevel >= 75)
                            {
                                if (TypeMgr.IsBossZombie(z.theZombieType) || z.TryGetComponent<LegionZombie>(out var legion))
                                    damage *= 4;
                                else if (!z.isPortaled && z.theFirstArmorHealth <= 0)
                                    z.SetPortaled(__instance.Damage / 5);
                                else if (z.isPortaled && z.theFirstArmorHealth <= 0)
                                    z.portaledTimer += __instance.Damage / 5;
                            }

                            z.TakeDamage(DmgType.IceAll, damage);

                            int knockback = UnityEngine.Random.Range(0, 5);
                            if (knockback == 0)
                            {
                                ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, z.axis.transform.position, z.theZombieRow);
                                z.KnockBack(z.axis.transform.position.x + 3f);
                            }

                            int tp = UnityEngine.Random.Range(0, 10);
                            if (tp == 0 && z.theZombieType != ZombieType.UltimateSnowZombie && z.theZombieType != ZombieType.UltimateKirovZombie)
                            {
                                ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, z.axis.transform.position, z.theZombieRow);
                                Vector3 pos = z.axis.transform.position;
                                pos.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                                Vector3 pos_trans = z.transform.position;
                                pos_trans.x = Mouse.Instance.GetBoxXFromColumn(GameAPP.board.GetComponent<Board>().columnNum);
                                z.transform.position = pos_trans;
                                z.axis.transform.position = pos;
                            }
                        }
                    }

                    // 究冰瓜瓜粒子
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
            }
            catch (Exception) { }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bullet), nameof(Bullet.Update))]
    public class Bullet_Update
    {
        [HarmonyPrefix]
        public static bool Prefix(Bullet __instance)
        {
            try
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
            }
            catch (Exception) { }
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
            try
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
            catch (Exception) { }
        }

        [HarmonyPostfix]
        public static void Postfix(Shooter __instance)
        {
            try
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
            catch (Exception) { }
        }
    }

    [HarmonyPatch(typeof(Zombie), nameof(Zombie.Start))]
    public class Zombie_Start
    {
        [HarmonyPostfix]
        public static void Postfix(Zombie __instance)
        {
            try
            {
                ZombieIceExtension component = null;
                if (__instance != null)
                {
                    component = __instance.AddComponent<ZombieIceExtension>();
                }
                if (component != null)
                {
                    component.parent = __instance;
                }
            }
            catch (Exception) { }
        }
    }

    public class ZombieIceExtension : MonoBehaviour
    {
        public int iceLevel = 0;
        public Zombie parent = null;
        public float coldSpeed = -1f;

        public void Update()
        {
            try
            {
                if (parent != null && (parent.coldTimer > 0 || parent.freezeTimer > 0) && iceLevel > 0)
                {
                    coldSpeed = (float)Math.Pow(0.95, iceLevel);
                    coldSpeed = coldSpeed < 0.3f ? 0.3f : coldSpeed;
                    coldSpeed = coldSpeed > 0.5f ? 0.5f : coldSpeed;
                }
                else if (parent != null && (parent.coldTimer <= 0 || parent.freezeTimer <= 0))
                {
                    iceLevel = 0;
                }
                if (iceLevel > 75) // 极冻值最多75层
                    iceLevel = 75;
                if (parent != null && coldSpeed != -1)
                    parent.coldSpeed = coldSpeed;
            }
            catch (Exception) { }
        }
    }
}