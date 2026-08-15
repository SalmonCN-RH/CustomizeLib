using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Core;

namespace FreezingCherry.BepInEx
{
    [BepInPlugin("salmon.freezingcherry", "FreezingCherry", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            Tools.InitMod();
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "freezingcherry");
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            {
                CustomCore.RegisterCustomPlant<IceCherry, FreezingCherry>(FreezingCherry.PlantID, ab.GetAsset<GameObject>("FreezingCherryPrefab"),
                    ab.GetAsset<GameObject>("FreezingCherryPreview"), new List<(int, int)>
                    {
                        ((int)PlantType.IceCherry, (int)PlantType.DoomShroom),
                        ((int)PlantType.DoomShroom, (int)PlantType.IceCherry),
                        ((int)PlantType.IceDoom, (int)PlantType.CherryBomb),
                        ((int)PlantType.CherryBomb, (int)PlantType.IceDoom),
                        ((int)PlantType.DoomCherry, (int)PlantType.IceShroom),
                        ((int)PlantType.IceShroom, (int)PlantType.DoomCherry)
                    }, 0f, 0f, 3600, 300, 7.5f, 350);
                CustomCore.AddPlantAlmanacStrings(FreezingCherry.PlantID, $"寒霜樱桃",
                    "特制的毁灭樱桃模具，吸收特定子弹来生产樱桃炸弹和毁灭菇。\n\n" +
                    "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                    "<color=#3D1400>融合配方：</color><color=red>樱桃炸弹+寒冰菇+毁灭菇</color>\n" +
                    "<color=#3D1400>伤害：</color><color=red>3600（灰烬）</color>\n" +
                    "<color=#3D1400>特点：</color><color=red>①不会自主爆炸\n" +
                    "②吸收特定子弹，回复能量\n" +
                    "每100点能量，产出一张樱桃炸弹卡片，每产生3张卡片，额外产出一张毁灭菇卡片\n" +
                    "*产出的卡片上限为8张，超出的部分会立即销毁</color>\n" +
                    "<color=#3D1400>蓄能能量：</color><color=red>①樱桃子弹：回复1点能量\n" +
                    "②火焰樱桃子弹，毁灭豌豆子弹，寄生樱桃子弹：回复2点能量\n" +
                    "③爆炸樱桃子弹，毁灭菇子弹及变种：回复10点能量\n" +
                    "④大毁灭菇子弹及变种：回复20点能量\n" +
                    "<color=#3D1400>词条1:</color><color=red>凝冰之力：寒霜樱桃（及其进阶）获得的能量x2</color>\n" +
                    "<color=#3D1400>词条2:</color><color=red>霜灭：寒霜樱桃（及其进阶）生产的卡片持续时间减半，卡片消失后，造成寒冰樱桃爆炸和寒冰毁灭菇爆炸</color>\n\n" +
                    "<color=#3D1400>作为一名战地记者，寒霜樱桃始终奔赴在前线，跟随大部队的步伐，穿梭于僵尸和植物之间，他们从不害怕，“越是危险的地方，越能看清事件的本质，我们想要知道僵尸需要脑子的根本原因，如这将是我们停止战争的关键！”</color>");
                CustomCore.TypeMgrExtra.IsIcePlant.Add(FreezingCherry.PlantID);
                FreezingCherry.buff1 = CustomCore.RegisterCustomBuff("凝冰之力：寒霜樱桃获得的能量x2，进阶形态共享此效果", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<FreezingCherry>() || Board.Instance.ObjectExist<UltimateFreezingCherry>(),
                    5000, FreezingCherry.PlantID, bg: BuffBgType.Night);
                FreezingCherry.buff2 = CustomCore.RegisterCustomBuff("霜灭：寒霜樱桃生产的卡片持续时间减半，卡片消失后，造成寒冰樱桃爆炸和寒冰毁灭菇爆炸，进阶形态共享此效果", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<FreezingCherry>() || Board.Instance.ObjectExist<UltimateFreezingCherry>(),
                    5000, FreezingCherry.PlantID, bg: BuffBgType.Night);
                ab.GetAsset<GameObject>("IceDoomNoSprite").AddComponent<IceDoomNoSprite>();
                CustomCore.RegisterCustomParticle(FreezingCherry.ParticleID, ab.GetAsset<GameObject>("IceDoomNoSprite"));
                CustomCore.AddUltimatePlant(FreezingCherry.PlantID);
            }
            {
                CustomCore.RegisterCustomPlant<IceCherry, UltimateFreezingCherry>(UltimateFreezingCherry.PlantID, ab.GetAsset<GameObject>("UltimateFreezingCherryPrefab"),
                    ab.GetAsset<GameObject>("UltimateFreezingCherryPreview"), new List<(int, int)>
                    {
                        (FreezingCherry.PlantID, (int)PlantType.IceShroom)
                    }, 0f, 0f, 3600, 300, 7.5f, 425);

                CustomCore.AddPlantAlmanacStrings(UltimateFreezingCherry.PlantID, $"究极寒霜樱桃",
                    "从樱桃和毁灭子弹积蓄，生产樱桃炸弹和毁灭菇，并对僵尸投射爆炸樱桃副产物。\n" +
                    "<color=#0000FF>寒霜樱桃的进阶形态</color>\n\n" +
                    "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                    "<color=#3D1400>获取条件：</color><color=red>集齐基础形态双词条</color>\n" +
                    "<color=#3D1400>融合配方：</color><color=red>寒霜樱桃+寒冰菇</color>\n" +
                    "<color=#3D1400>伤害：</color><color=red>3600（灰烬）</color>\n" +
                    "<color=#3D1400>特点：</color><color=red>①不会自主爆炸\n" +
                    "②吸收特定子弹，回复能量\n" +
                    "每100点能量，产出一张樱桃炸弹卡片，每产生3张卡片，额外产出一张毁灭菇卡片\n" +
                    "*产出的卡片上限为8张，超出的部分会立即销毁\n" +
                    "④每次生产卡片时，投射3颗寒霜核能樱桃，全屏索敌，命中施加5秒冻结\n" +
                    "⑤使用樱桃炸弹或毁灭菇卡片可立即释放寒冰樱桃和寒冰毁灭菇效果</color>\n" +
                    "<color=#3D1400>蓄能能量：</color><color=red>①樱桃子弹：回复1点能量\n" +
                    "②火焰樱桃子弹，毁灭豌豆子弹，寄生樱桃子弹：回复2点能量\n" +
                    "③爆炸樱桃子弹，毁灭菇子弹及变种：回复10点能量\n" +
                    "④大毁灭菇子弹及变种：回复20点能量\n" +
                    "<color=#3D1400>连携词条:</color><color=red>霜陨子母弹：手套抓取核爆樱桃及亚种点击究极寒霜樱桃时，双方将进行短暂融合裂变，对全屏僵尸造成毁灭性打击：对全场造成150倍究极寒霜樱桃的伤害并冻结10秒，对周围连续产生大量樱桃爆炸，爆出72颗一圈的寒霜核能樱桃子弹，并留下10秒的寒霜辐射区域，冻结10秒关卡</color>\n\n" +
                    "<color=#3D1400>究极寒霜樱桃毅然决然的投入到与僵尸的斗争中，只是一昧的搜集研究是无法找到解决方法的，究极寒霜樱桃始终顶在最前边，“我的身后不只是我的队友，更是万家灯火！”</color>");
                CustomCore.TypeMgrExtra.IsIcePlant.Add(UltimateFreezingCherry.PlantID);
                ab.GetAsset<GameObject>("Doom_nuclear2").AddComponent<IceDoomNoSprite>();
                CustomCore.RegisterCustomParticle(UltimateFreezingCherry.ParticleID, ab.GetAsset<GameObject>("Doom_nuclear2"));
                ab.GetAsset<GameObject>("Doom_nuclear").AddComponent<IceDoomNoSprite>();
                CustomCore.RegisterCustomParticle(UltimateFreezingCherry.DoomParticle, ab.GetAsset<GameObject>("Doom_nuclear"));
                ab.GetAsset<GameObject>("Radiation").AddComponent<IceRadiation>();
                UltimateFreezingCherry.IceRadiation = ab.GetAsset<GameObject>("Radiation");
                ab.GetAsset<GameObject>("IceDoomOnlySprite").AddComponent<IceDoomNoSprite>();
                CustomCore.RegisterCustomParticle(UltimateFreezingCherry.IceDoomSprite, ab.GetAsset<GameObject>("IceDoomOnlySprite"));
                CustomCore.RegisterCustomCherry(UltimateFreezingCherry.CherryID, ab.GetAsset<GameObject>("BombCloud_vision_Super"));
                CustomCore.RegisterCustomCherry(UltimateFreezingCherry.CherrySmallID, ab.GetAsset<GameObject>("BombCloudSmall"));
                CustomCore.RegisterCustomClickCardOnPlantEvent(UltimateFreezingCherry.PlantID, PlantType.CherryBomb, (p) =>
                {
                    p.board.boardAction.SetDoom(0, 0, false, true, p.axis.transform.position, 3600, fromType: p.thePlantType);
                    Action<Zombie> action = (z) =>
                    {
                        z.SetFreeze(10f);
                        z.SetCold(15f);
                        z.TakeDamage(DmgType.NormalAll, p.attackDamage / 2, p.thePlantType);
                    };
                    p.board.boardAction.CreateCherryExplode(p.axis.transform.position, p.thePlantRow, CherryBombType.IceCharry, p.attackDamage, p.thePlantType, action);
                }, onPlant: new CustomClickCardOnPlant
                {
                    Trigger = CustomClickCardOnPlant.TriggerType.CardOnly
                });
                CustomCore.RegisterCustomClickCardOnPlantEvent(UltimateFreezingCherry.PlantID, PlantType.DoomShroom, (p) =>
                {
                    p.board.boardAction.SetDoom(0, 0, false, true, p.axis.transform.position, 3600, fromType: p.thePlantType);
                    Action<Zombie> action = (z) =>
                    {
                        z.SetFreeze(10f);
                        z.SetCold(15f);
                        z.TakeDamage(DmgType.NormalAll, p.attackDamage / 2, p.thePlantType);
                    };
                    p.board.boardAction.CreateCherryExplode(p.axis.transform.position, p.thePlantRow, CherryBombType.IceCharry, p.attackDamage, p.thePlantType, action);
                }, onPlant: new CustomClickCardOnPlant
                {
                    Trigger = CustomClickCardOnPlant.TriggerType.CardOnly
                });
                CustomCore.RegisterCustomClickCardOnPlantEvent(UltimateFreezingCherry.PlantID, PlantType.NuclearDoomCherry, (p) => p.anim.SetTrigger("super_cherry"),
                    (p) => p.anim.GetInteger("cureentStatus") == 0 && Lawnf.TravelAdvanced(UltimateFreezingCherry.buff));
                CustomCore.RegisterCustomClickCardOnPlantEvent(UltimateFreezingCherry.PlantID, PlantType.NuclearSquash, (p) => p.anim.SetTrigger("super_squash"),
                    (p) => p.anim.GetInteger("cureentStatus") == 0 && Lawnf.TravelAdvanced(UltimateFreezingCherry.buff));
                UltimateFreezingCherry.buff = CustomCore.RegisterCustomBuff("霜陨子母弹：手套抓取核爆樱桃及亚种点击究极寒霜樱桃时，双方将进行短暂融合裂变，对全屏僵尸造成毁灭性打击",
                    BuffType.AdvancedBuff, () => TravelStore.Instance != null && Lawnf.TravelAdvanced((AdvBuff)30) && Lawnf.TravelAdvanced((AdvBuff)31) &&
                    Lawnf.TravelAdvanced(FreezingCherry.buff1) && Lawnf.TravelAdvanced(FreezingCherry.buff2), 15000, plantType: PlantType.EndoFlame);
                CustomCore.RegisterCustomBanMix(UltimateFreezingCherry.PlantID, () => (Lawnf.TravelAdvanced(FreezingCherry.buff1) && Lawnf.TravelAdvanced(FreezingCherry.buff2)) ||
                Board.Instance.boardTag.enableAllTravelPlant || Board.Instance.boardTag.isSuperRandom || Board.Instance.boardTag.isUltimateSuperRandom || GameAPP.developerMode,
                    null, () => InGameText.Instance.ShowText("该配方需要抽取", 3f));
                CustomCore.AddUltimatePlant(UltimateFreezingCherry.PlantID);
                CustomCore.RegisterCustomBullet<Bullet_nuclear>(UltimateFreezingCherry.BulletID, ab.GetAsset<GameObject>("Bullet_IceNuclear"));
            }
        }
    }

    public class FreezingCherry : MonoBehaviour
    {
        public static ID PlantID = 1957;
        public static ID ParticleID = 1957;
        public static BuffID buff1 = -1; // 凝冰之力
        public static BuffID buff2 = -1; // 霜灭

        public static GameObject? IceDoomPrefab = null;
        public static int maxCard = 8;

        public IceCherry plant => gameObject.GetComponent<IceCherry>();
        public int spawnTime = 0;
        public List<DroppedCard> cards = new();

        public virtual bool AddEnergy(Collider2D collision)
        {
            if (plant == null) return false;
            if (collision == null || !collision.TryGetComponent<Bullet>(out var bullet) || bullet == null)
                return false;
            if (bullet.Team == Team.AI)
                return false;
            int energy = 0;

            switch (bullet.theBulletType)
            {
                case BulletType.Bullet_cherry:
                    energy = 1;
                    break;
                case BulletType.Bullet_fireCherry:
                case BulletType.Bullet_pea_doom:
                case BulletType.Bullet_pea_doom_fire:
                case BulletType.Bullet_pea_threeCherry:
                case BulletType.Bullet_pea_bombCherry:
                    energy = 2;
                    break;
                case BulletType.Bullet_superCherry:
                case BulletType.Bullet_cherrySquash:
                    energy = 10;
                    break;
                case BulletType.Bullet_doom:
                case BulletType.Bullet_doom_ulti:
                    if (bullet.theStatus == BulletStatus.Doom_big)
                        energy = 20;
                    else
                        energy = 10;
                    break;
                case BulletType.Bullet_doom_big:
                case BulletType.Bullet_doom_big_ulti:
                    energy = 20;
                    break;
                case BulletType.Bullet_doom_fire:
                    if (bullet.theStatus == BulletStatus.Doom_big)
                        energy = 20;
                    else
                        energy = 10;
                    break;
                default:
                    return false;
            }
            CreateParticle.SetParticle((int)ParticleType.SnowPeaSplat, new Vector2(bullet.transform.position.x, bullet.transform.position.y), bullet.theBulletRow);
            if (Lawnf.TravelAdvanced(buff1))
                energy *= 2;
            plant.attributeCount += energy;
            GameAPP.PlaySound(68, 0.5f, 1.0f);
            if (plant.attributeCount >= 100)
            {
                cards = cards.Where(card => card != null).OrderByDescending(card => card.existTime).ToList();
                var cherry = Lawnf.SetDroppedCard(plant.axis.transform.position + new Vector3(0f, 1f, 0f), PlantType.CherryBomb);
                cherry.SetData("FreezingCherry_CreateByCherry", true);
                cherry.SetData("FreezingCherry_FromType", plant.thePlantType);
                if (cards.Count >= maxCard)
                    cherry.existTime = Lawnf.TravelAdvanced(buff2) ? 7.5f : 15f;
                else
                    cards.Add(cherry);
                spawnTime++;
                if (spawnTime >= 3)
                {
                    var doom = Lawnf.SetDroppedCard(plant.axis.transform.position + new Vector3(0f, 1f, 0f), PlantType.DoomShroom);
                    doom.SetData("FreezingCherry_CreateByCherry", true);
                    doom.SetData("FreezingCherry_FromType", plant.thePlantType);
                    if (cards.Count(card => card.thePlantType == PlantType.DoomShroom) < 2)
                    {
                        var list = cards.OrderByDescending(card => card.existTime).ToList();
                        if (list.Count > 0)
                        {
                            var cherryCard = list.First(card => card.thePlantType == PlantType.CherryBomb);
                            if (cherryCard != null)
                            {
                                cards[cards.IndexOf(cherryCard)] = doom;
                                cards.Add(cherryCard);
                            }
                            else
                                cards.Add(doom);
                        }
                        else
                            cards.Add(doom);
                    }
                    else
                        doom.existTime = Lawnf.TravelAdvanced(buff2) ? 7.5f : 15f;
                    spawnTime = 0;
                }
                if (cards.Count > maxCard)
                {
                    for (int i = maxCard; i < cards.Count; i++)
                    {
                        cards[i].existTime = Lawnf.TravelAdvanced(buff2) ? 7.5f : 15f;
                        cards.RemoveAt(i);
                    }
                }
                bullet.Die();
                plant.attributeCount -= 100;
                return true;
            }
            bullet.Die();
            plant.UpdateText();
            return false;
        }
    }

    public class UltimateFreezingCherry : FreezingCherry
    {
        public static new ID PlantID = 1958;
        public static ID BulletID = 1958;
        public static new ID ParticleID = 1958;
        public static ID DoomParticle = 1959;
        public static ID IceDoomSprite = 1960;
        public static BuffID buff = -1;
        public static GameObject? IceRadiation = null;
        public static ID CherryID = 1958;
        public static ID CherrySmallID = 1959;

        public new IceCherry plant => gameObject.GetComponent<IceCherry>();

        public void Awake()
        {
            plant.shoot = transform.FindChild("Body/Shoot");
        }

        public void Start()
        {
            if (plant == null) return;
            plant.gameObject.layer = LayerMask.NameToLayer("TorchWood");
        }

        public void Update()
        {
            if (plant == null) return;
            plant.attributeFloat += Time.deltaTime;
        }

        public void SuperStart()
        {
            plant.anim.SetInteger("cureentStatus", 1);
            plant.invincible = true;
            plant.GetComponent<SortingGroup>().sortingLayerName = "plant11";
        }

        public void ChangeState()
        {
            plant.anim.SetInteger("cureentStatus", 2);
            plant.board.boardAction.SetDoom(0, 0, false, true, plant.axis.transform.position, 18000, existParticle: false, fromType: plant.thePlantType);
            CreateParticle.SetParticle(IceDoomSprite, plant.axis.transform.position, 11);
            plant.StartCoroutine(CreateBomb());
        }

        public void SuperEnd()
        {
            CreateParticle.SetParticle(DoomParticle, plant.axis.transform.position, 11);
            foreach (var zombie in Lawnf.GetAllZombies())
            {
                if (zombie == null) continue;
                if (zombie.isMindControlled) continue;
                zombie.SetFreeze(10f);
                zombie.SetCold(15f);
                zombie.TakeDamage(DmgType.IceAll, plant.attackDamage * 150, plant.thePlantType);
            }
            Vector3 position = plant.axis.position;
            for (int angle = 0; angle < 360; angle += 5)
            {
                // 创建子弹
                Bullet bullet = CreateBullet.Instance.SetBullet(position.x, position.y, plant.thePlantRow, BulletID, BulletMoveWay.Free, false);
                if (bullet != null)
                {
                    bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
                    bullet.Damage = plant.attackDamage * 150;
                    bullet.fromType = UltimateFreezingCherry.PlantID;
                }
            }
            Instantiate(IceRadiation, plant.axis.transform.position, Quaternion.identity, plant.board.transform);
            plant.board.iceDoomFreezeTime += 10f;
            plant.board.StartCoroutine(CreateBomb());
            plant.Die(Plant.DieReason.BySelf);
        }

        public override bool AddEnergy(Collider2D collision)
        {
            if (base.AddEnergy(collision))
                plant.StartCoroutine(StartShoot());
            return true;
        }

        public void Shoot()
        {
            if (plant == null) return;
            if (plant.board == null) return;
            Vector2 shoot = plant.shoot.position;
            var zombies = Lawnf.GetAllZombies().ToSystemList().Where(z => z != null && !z.isMindControlled).ToList();
            float[] projectileParams = new float[4];
            Zombie? zombie = null;
            Vector2 position = default;
            if (zombies.Count > 0)
            {
                zombie = zombies[UnityEngine.Random.Range(0, zombies.Count)];
                projectileParams = global::Core.Lawnf.CalculateProjectileWithSpeed(shoot, zombie.Velocity, zombie.ColliderPosition, 1.5f);
            }
            else
            {
                return;
            }
            if (projectileParams == null || projectileParams.Length <= 1)
                return;
            Bullet? bullet = null;
            if (zombie != null)
            {
                bullet = CreateBullet.Instance.SetBullet(shoot.x, shoot.y, zombie.theZombieRow, BulletID, BulletMoveWay.Throw);
                bullet.ThrowTo(zombie, new(), 1.5f.GetNullable());
            }
            else
            {
                bullet = CreateBullet.Instance.SetBullet(shoot.x, shoot.y, Mouse.Instance.GetRowFromY(position.x, position.y), BulletID, BulletMoveWay.Throw);
                bullet.ThrowTo(position, new(), 1.5f.GetNullable());
            }
            bullet.Damage = plant.attackDamage;
            bullet.fromType = plant.thePlantType;
        }

        public IEnumerator StartShoot()
        {
            for (int i = 1; i <= 3; i++)
            {
                try
                {
                    Shoot();
                }
                catch (Exception) 
                {
                    yield break; 
                }
                yield return new WaitForSeconds(0.08f);
            }
            yield break;
        }
        public IEnumerator CreateBomb()
        {
            if (Board.Instance == null)
                yield break;

            int bombCount = 0;
            int maxBombs = 30 * 3; // 总共90个炸弹

            while (bombCount < maxBombs)
            {
                for (int j = 0; j < 3; j++)
                {
                    try
                    {
                        SetBomb();
                        bombCount++;
                    }
                    catch (Exception)
                    {
                        yield break;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void SetBomb()
        {
            try
            {
                Action<Zombie> action = (z) =>
                {
                    z.SetFreeze(10f);
                    z.SetCold(15f);
                    int damage = 7200;
                    if (z.GetAttrTimers().freezeTimer > 0f)
                        damage *= 4;
                    z.TakeDamage(DmgType.NormalAll, damage, PlantID);
                };
                int column = UnityEngine.Random.Range(0, Board.Instance.columnNum);
                int row = UnityEngine.Random.Range(0, Board.Instance.rowNum);
                var position = new Vector2(Mouse.Instance.GetBoxXFromColumn(column), Mouse.Instance.GetBoxYFromRow(row));
                Board.Instance.boardAction.CreateCherryExplode(position, row, CherryID, 7200, PlantID, action);
            }
            catch (Exception e)
            {
                return;
            }
        }
    }

    public class IceRadiation : MonoBehaviour
    {
        public static IceRadiation? Instance = null;

        public LayerMask zombieLayer = default;
        public float timer = 0.5f;
        public float lifeTimer = 10f;
        public int damage = 10800;
        public PlantType fromType = UltimateFreezingCherry.PlantID;

        public void Awake()
        {
            zombieLayer = LayerMask.GetMask("Zombie");

            if (Instance != null)
                Instance.Die();

            Instance = this;
        }

        public void Die()
        {
            Destroy(gameObject);
            Instance = null;
        }

        public void FixedUpdate()
        {
            timer -= Time.fixedDeltaTime;

            if (timer <= 0.0f)
            {
                timer = 0.5f;

                Vector3 position = transform.position;
                float radius = transform.localScale.x * 2.5f / 3.7f;

                Collider2D[] colliders = Physics2D.OverlapCircleAll(
                    new Vector2(position.x, position.y),
                    radius,
                    zombieLayer.value
                );

                foreach (Collider2D collider in colliders)
                {
                    if (collider.TryGetComponent<Zombie>(out var zombie) && zombie != null)
                    {
                        float damage = this.damage * lifeTimer;

                        if (zombie.theFirstArmor == null)
                        {
                            damage *= 3.0f;
                        }
                        zombie.AddfreezeLevel(100);
                        zombie.TakeDamage(DmgType.NormalAll, (int)damage, fromType);
                    }
                }
            }
        }

        public void Update()
        {
            lifeTimer -= Time.deltaTime;

            if (lifeTimer <= 0.0f)
            {
                Die();
            }
        }
    }

    public class IceDoomNoSprite : MonoBehaviour
    {
        public void Die() => Destroy(gameObject);
    }

    [HarmonyPatch(typeof(IceCherry))]
    public static class IceCherryPatch
    {
        [HarmonyPatch(nameof(IceCherry.OnTriggerEnter2D))]
        [HarmonyPrefix]
        public static bool PreOnTriggerEnter2D(IceCherry __instance, ref Collider2D collision)
        {
            if (__instance != null && __instance.thePlantType == FreezingCherry.PlantID)
            {
                if (__instance != null && __instance.TryGetComponent<FreezingCherry>(out var cherry) && cherry != null)
                    cherry.AddEnergy(collision);
                return false;
            }
            if (__instance != null && __instance.thePlantType == UltimateFreezingCherry.PlantID)
            {
                if (__instance != null && __instance.TryGetComponent<UltimateFreezingCherry>(out var cherry) && cherry != null)
                    cherry.AddEnergy(collision);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(IceCherry.DieEvent))]
        [HarmonyPrefix]
        public static bool PreDieEvent(IceCherry __instance, ref Plant.DieReason reason)
        {
            if (__instance != null && (__instance.thePlantType == FreezingCherry.PlantID || __instance.thePlantType == UltimateFreezingCherry.PlantID) && reason != Plant.DieReason.ByMix && reason != Plant.DieReason.BySelf)
            {
                Action<Zombie> action = (z) =>
                {
                    z.SetFreeze(10f);
                    z.SetCold(15f);
                    int damage = __instance.attackDamage;
                    if (z.GetAttrTimers().freezeTimer > 0f)
                        damage *= 4;
                    z.TakeDamage(DmgType.NormalAll, damage, __instance.thePlantType);
                };
                __instance.board.boardAction.CreateCherryExplode(__instance.axis.transform.position, __instance.thePlantRow, CherryBombType.IceCharry, __instance.attackDamage, __instance.thePlantType, action);
                __instance.board.boardAction.SetDoom(0, 0, false, true, __instance.axis.transform.position, 3600, fromType: __instance.thePlantType);
                if (__instance.TryGetComponent<UltimateFreezingCherry>(out var cherry) && cherry != null && !cherry.IsDestroyed())
                    __instance.StopAllCoroutines();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPatch(nameof(Plant.Die))]
        [HarmonyPrefix]
        public static bool Prefix(Plant __instance, ref Plant.DieReason reason)
        {
            if (__instance.thePlantType == UltimateFreezingCherry.PlantID && __instance.anim.GetInteger("cureentStatus") != 0 && reason != Plant.DieReason.BySelf)
            {
                __instance.thePlantHealth = __instance.thePlantMaxHealth;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Plant.Crashed))]
        [HarmonyPrefix]
        public static bool PreCrashed(Plant __instance, ref Zombie zombie)
        {
            if (__instance.thePlantType == UltimateFreezingCherry.PlantID && __instance.anim.GetInteger("cureentStatus") != 0)
            {
                __instance.thePlantHealth = __instance.thePlantMaxHealth;
                if (zombie != null)
                    zombie.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DroppedCard))]
    public static class DroppedCardPatch
    {
        [HarmonyPatch(nameof(DroppedCard.Update))]
        [HarmonyPrefix]
        public static void PreUpdate(DroppedCard __instance)
        {
            if (__instance.GetData<bool>("FreezingCherry_CreateByCherry") && Lawnf.TravelAdvanced(FreezingCherry.buff2))
            {
                if (__instance.existTime > 5f && __instance.existTime < 7.5f)
                    __instance.flashing = true;
                if (__instance.existTime + Time.deltaTime >= 7.5f && GameAPP.theGameStatus == GameStatus.InGame && Time.timeScale > 0f)
                {
                    Action<Zombie> action = (z) =>
                    {
                        z.SetFreeze(10f);
                        z.SetCold(15f);
                        int damage = 3600;
                        if (z.GetAttrTimers().freezeTimer > 0f)
                            damage *= 4;
                        z.TakeDamage(DmgType.NormalAll, damage, __instance.GetData<PlantType>("FreezingCherry_FromType"));
                    };
                    var bomb = CoreTools.CreateCherryExplode(__instance.transform.position, Mouse.Instance.GetRowFromY(__instance.transform.position.x, __instance.transform.position.y), CherryBombType.IceCharry,
                        3600, __instance.GetData<PlantType>("FreezingCherry_FromType"), action).Item2;
                    if (GameAPP.config.distablexplodeFlash)
                    {
                        bomb.GetComponent<ParticleSystem>().Simulate(0f, true);
                        bomb.GetComponent<ParticleSystemRenderer>().enabled = false;
                    }
                    __instance.board.boardAction.SetDoom(0, 0, false, true, __instance.transform.position - new Vector3(0f, 0.75f, 0f), 3600, existParticle: false, fromType: __instance.GetData<PlantType>("FreezingCherry_FromType"));
                    if (!GameAPP.config.distablexplodeFlash)
                        CreateParticle.SetParticle(FreezingCherry.ParticleID, __instance.transform.position, 11, true);
                    UnityEngine.Object.Destroy(__instance.gameObject);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Bullet_nuclear))]
    public static class Bullet_nuclearPatch
    {
        [HarmonyPatch(nameof(Bullet_nuclear.HitZombie))]
        [HarmonyPrefix]
        public static bool PreHitZombie(Bullet_nuclear __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == UltimateFreezingCherry.BulletID)
            {
                if (zombie != null)
                {
                    Action<Zombie> action = (z) =>
                    {
                        z.SetCold(10f);
                        z.AddfreezeLevel(25);
                        if (z.GetAttrTimers().freezeTimer > 0f)
                            z.TakeDamage(DmgType.NormalAll, __instance.Damage * 4, __instance.fromType);
                        else
                            z.TakeDamage(DmgType.NormalAll, __instance.Damage, __instance.fromType);
                    };
                    var cherry = CoreTools.CreateCherryExplode(__instance.transform.position, __instance.theBulletRow, UltimateFreezingCherry.CherrySmallID, __instance.Damage, action: action, volume: 0.2f).Item1;
                    cherry.bombRow = __instance.theBulletRow;
                    cherry.range = 1f;
                    cherry.fromType = __instance.fromType;
                    cherry.maxRow = 1;
                    __instance.board.boardAction.SetDoom(0, 0, false, true, __instance.transform.position, __instance.Damage, existParticle: false, fromType: __instance.GetData<PlantType>("FreezingCherry_FromType"));
                    var position = __instance.transform.position;
                    position.y -= 0.75f;
                    if (!GameAPP.config.distablexplodeFlash)
                        CreateParticle.SetParticle(UltimateFreezingCherry.ParticleID, position, 11, true);
                    __instance.Die();
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Bullet_nuclear.HitLand))]
        [HarmonyPrefix]
        public static bool PreLand(Bullet_nuclear __instance)
        {
            if (__instance.theBulletType == UltimateFreezingCherry.BulletID)
            {
                Action<Zombie> action = (z) =>
                {
                    z.SetCold(10f);
                    z.AddfreezeLevel(25);
                    if (z.GetAttrTimers().freezeTimer > 0f)
                        z.TakeDamage(DmgType.NormalAll, __instance.Damage * 4, __instance.fromType);
                    else
                        z.TakeDamage(DmgType.NormalAll, __instance.Damage, __instance.fromType);
                };
                var cherry = CoreTools.CreateCherryExplode(__instance.transform.position, __instance.theBulletRow, UltimateFreezingCherry.CherrySmallID, __instance.Damage, action: action, volume: 0.2f).Item1;
                cherry.bombRow = __instance.theBulletRow;
                cherry.range = 1f;
                cherry.fromType = __instance.fromType;
                cherry.maxRow = 1;
                cherry.zombieAction = action;
                __instance.board.boardAction.SetDoom(0, 0, false, true, __instance.transform.position, __instance.Damage, existParticle: false, fromType: __instance.GetData<PlantType>("FreezingCherry_FromType"));
                var position = __instance.transform.position;
                position.y -= 0.75f;
                if (!GameAPP.config.distablexplodeFlash)
                    CreateParticle.SetParticle(UltimateFreezingCherry.ParticleID, position, 11, true);
                __instance.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Mouse), nameof(Mouse.GetPlantsOnMouse))]
    public static class MousePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Il2CppSystem.Collections.Generic.List<Plant> __result)
        {
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                var plant = __result[i];
                if (plant == null) return;
                if (plant.thePlantType == UltimateFreezingCherry.PlantID && plant.anim.GetInteger("cureentStatus") != 0)
                    __result.Remove(plant);
            }
        }
    }
}
