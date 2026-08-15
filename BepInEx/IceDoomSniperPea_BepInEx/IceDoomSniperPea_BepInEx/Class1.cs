using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameLevel.RogueShooting;
using CustomizeLib.BepInEx.UnmanagedTools;
using UI;
using NewTravel;
using System.Collections;

namespace IceDoomSniperPea.BepInEx
{
    [BepInPlugin("salmon.icedoomsniperpea", "IceDoomSniperPea", "1.0")]
    public class Core : BasePlugin
    {
        public static GameObject IceDoomBomb = null;

        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<IceDoomSniperPea>();
            ClassInjector.RegisterTypeInIl2Cpp<IceDoomBomb>();

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "icedoomsniperpea");
            var list = new List<(int, int)>
            {
                ((int)PlantType.SniperPea, (int)PlantType.IceDoom),
                ((int)PlantType.IceDoom, (int)PlantType.SniperPea),
                ((int)PlantType.DoomSniper, (int)PlantType.IceShroom),
                ((int)PlantType.IceShroom, (int)PlantType.DoomSniper),
                (1902, (int)PlantType.DoomShroom),
                ((int)PlantType.DoomShroom, 1902)
            };
            CustomCore.RegisterCustomPlant<SniperPea, IceDoomSniperPea>(IceDoomSniperPea.PlantID, ab.GetAsset<GameObject>("IceDoomSniperPeaPrefab"),
                ab.GetAsset<GameObject>("IceDoomSniperPeaPreview"), list, 3f, 0f, 600, 300, 7.5f, 800);
            CustomCore.RegisterCustomPlantSkin<SniperPea, IceDoomSniperPea>(IceDoomSniperPea.PlantID, ab.GetAsset<GameObject>("IceDoomSniperPeaPrefabSkin"),
                ab.GetAsset<GameObject>("IceDoomSn" +
                "iperPeaPreviewSkin"), list, 6f, 0f, 600, 300, 7.5f, 800);
            IceDoomBomb = ab.GetAsset<GameObject>("IceDoomBomb");
            IceDoomBomb.gameObject.AddComponent<IceDoomBomb>();
            CustomCore.AddPlantAlmanacStrings(IceDoomSniperPea.PlantID, "冷寂狙击射手(" + IceDoomSniperPea.PlantID + ")",
                "静寂无声中发射附带炸药的狙击，同时第二发将其引爆，造成范围杀伤效果。\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>伤害：600x2/3秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①特点同狙击射手，每次攻击赋予100冻结值，对冻结的僵尸伤害x4\n" +
                "②每轮发射中第1发安装冰毁炸弹，第2发命中拥有冰毁炸弹的僵尸会将其引爆，造成伤害为1800点的寒冰爆炸效果，若未引爆，3秒种后自动引爆并造成600点伤害\n" +
                "③每11发安装超级冰毁炸弹，第12发引爆射击造成超级爆头造成21亿伤害（对于领袖将造成10000点伤害），随后释放7200点的寒冰毁灭菇爆炸\n" +
                "<color=#3D1400>融合配方：</color><color=red>狙击射手+寒冰菇+毁灭菇</color>\n" +
                "<color=#3D1400>词条1：</color><color=red>精装炸弹：冰毁炸弹爆炸的伤害x3，冰毁炸弹引爆后会为附近的僵尸挂上冰毁炸弹。2级时，每次引爆都造成寒冰毁灭菇爆炸，引爆时有50%概率触发超级爆头，并附带10%韧性的伤害</color>\n" +
                "<color=#3D1400>词条2：</color><color=red>夜影暗袭：冰毁狙击射手每11发将锁定场上韧性上限最高的僵尸。每轮攻击有概率触发连狙\n\n" +
                "<color=#3D1400>远方的僵尸缓缓逼近，声势浩大，所过之处寸草不生，乌鸦吱吱哇哇的叫着，像是在宣告僵尸大军的到来。冰毁狙击豌豆对准了最大的巨人僵尸，一发毙命，随后对其他植物说道“它们不是不可战胜的，我们同仇敌忾，我们团结一心，我们全力以赴，我们会前赴后继的拿下胜利！僵尸并不可怕，可怕的是我们会退缩，会害怕，但是兄弟们，一旦我们在这里畏手畏脚，在这里退缩在这里倒下，他们就会冲进庭院进行杀戮撕咬，为了我们的未来！为了守护的院落！绝不后退一步！”</color>");
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)IceDoomSniperPea.PlantID);
            CustomCore.TypeMgrExtra.LevelPlants.Add((PlantType)IceDoomSniperPea.PlantID, CardLevel.Gold);
            IceDoomSniperPea.buff1 = (AdvBuff)CustomCore.RegisterCustomBuff(new BuffConfig()
            {
                almanac = AlmanacBuffType.WeakUltimate,
                backGround = BuffBgType.Night,
                cost = 5000,
                desc = "精装炸弹：冰毁炸弹爆炸的伤害x3，冰毁炸弹引爆后会为附近的僵尸挂上冰毁炸弹。2级时，每次引爆都造成寒冰毁灭菇爆炸，引爆时有50%概率触发超级爆头，并附带10%韧性的伤害",
                iconPlant = (PlantType)IceDoomSniperPea.PlantID,
                maxLevel = 2,
                unlock = () => Board.Instance.ObjectExist<IceDoomSniperPea>(),
                type = BuffType.AdvancedBuff,
                probably = true
            });
            IceDoomSniperPea.buff2 = (AdvBuff)CustomCore.RegisterCustomBuff(new BuffConfig()
            {
                almanac = AlmanacBuffType.WeakUltimate,
                backGround = BuffBgType.Night,
                cost = 5000,
                desc = "夜影暗袭：冰毁狙击射手每11发将锁定场上韧性上限最高的僵尸。每轮攻击有概率触发连狙",
                iconPlant = (PlantType)IceDoomSniperPea.PlantID,
                unlock = () => Board.Instance.ObjectExist<IceDoomSniperPea>(),
                type = BuffType.AdvancedBuff,
                probably = true
            });
            CustomCore.AddUltimatePlant((PlantType)IceDoomSniperPea.PlantID);
            #region 诸神
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IceDoomSniperPea>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IceDoomSniperPea.UniqueUpgrade>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IceDoomSniperPea.BombBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IceDoomSniperPea.SuperShotBuff>();
            ClassInjector.RegisterTypeInIl2Cpp<Shooting_IceDoomSniperPea.SniperBuff>();
            #endregion
        }
    }

    public class IceDoomSniperPea : MonoBehaviour
    {
        public static int PlantID = 1900;
        // 精装炸弹
        public static AdvBuff buff1 = (AdvBuff)(-1);
        // 夜影暗袭
        public static AdvBuff buff2 = (AdvBuff)(-1);

        public bool isRogueShooting => plant.board != null ? plant.board.boardTag.rogueShooting : false;
        /// <summary>
        /// 质变-狙击
        /// </summary>
        public bool superShot = false;
        /// <summary>
        /// 炸弹加伤害
        /// </summary>
        public int bomb = 0;
        /// <summary>
        /// 额外僵尸造成血量
        /// </summary>
        public int health = 0;

        public void AddBomb() => bomb++;
        public void AddHealth() => health++;

        public void AttackZombie(Zombie zombie, int damage)
        {
            try
            {
                if (!zombie.IsObjExist()) return;

                // 冰毁狙本体狙击
                zombie.TakeDamage(DmgType.IceAll, damage + Shooting_IceDoomSniperPea.ExtraDamage(zombie), (PlantType)PlantID);
                zombie.SetCold(10f);
                zombie.AddfreezeLevel(100 / (isRogueShooting ? 2 : 1)); // 如果是诸神，冻结值/2
                if (plant.attackCount % 2 == 1)
                {
                    var bomb = Instantiate(Core.IceDoomBomb, plant.ac.transform.position, Quaternion.identity);
                    bomb.transform.SetParent(zombie.transform, true);
                    bomb.GetComponent<IceDoomBomb>().zombie = zombie;
                    bomb.GetComponent<IceDoomBomb>().parent = true;
                    bomb.GetComponent<IceDoomBomb>().plant = plant;
                    bomb.GetComponent<IceDoomBomb>().damage = plant.attackDamage;
                }

                if (plant.attackCount % 2 == 0)
                {
                    var go = zombie.transform.FindChild("IceDoomBomb(Clone)").gameObject;
                    go.GetComponent<IceDoomBomb>().Bomb(plant.attackCount);
                    plant.board.StartCoroutine(CreateBomb(bomb, zombie));
                }

                ParticleManager.Instance.SetParticle(ParticleType.IceDoomSplat, plant.ac.transform.position, plant.targetZombie.theZombieRow, true);
            }
            catch (Exception e)
            {
            }
        }

        public void AnimShoot_IceDoom()
        {
            GameAPP.PlaySound(40, 0.2f, 1.0f);

            var targetZombie = plant.targetZombie;

            if (!targetZombie.IsObjExist() || !SearchUniqueZombie(targetZombie))
                return;

            plant.attackCount++;

            int damage = plant.attackDamage;

            if (targetZombie.GetAttrTimers().freezeTimer > 0)
                damage *= 4;

            AttackZombie(targetZombie, damage);
            if (plant.attackCount % 10 == 0 && Lawnf.TravelAdvanced(buff2))
            {
                SearchMaxHealthZombie();
                plant.AcPositionUpdate();
            }

            if (plant.attackCount % 2 == 0 && Lawnf.TravelAdvanced(buff2) && UnityEngine.Random.Range(1, 10) <= 3)
            {
                plant.thePlantAttackCountDown = 0.1f;
            }

            if (targetZombie.theStatus != ZombieStatus.Dying && !targetZombie.beforeDying)
                return;

            plant.targetZombie = null;
            return;
        }

        public void FixedUpdate()
        {
            try
            {
                if (plant.targetZombie != null)
                {
                    if (plant.targetZombie.isMindControlled || plant.targetZombie.beforeDying || 
                        plant.targetZombie.GetTotalHealth() <= 0 || plant.targetZombie.theStatus == ZombieStatus.Paper_losePaper)
                        SearchZombie();
                }
            }
            catch (Exception) { }
        }

        // 僵尸状态验证
        public bool SearchUniqueZombie(Zombie zombie)
        {
            if (!zombie.IsObjExist()) return false;

            if (zombie.isMindControlled || zombie.beforeDying || zombie.GetTotalHealth() <= 0)
                return false;

            int status = (int)zombie.theStatus;

            if (status <= 7)
            {
                if (status == 1 || status == 7)
                    return false;
            }
            else if (status == 12 || (status >= 20 && status <= 24))
            {
                return false;
            }

            return true;
        }

        // 目标搜索方法
        public Zombie SearchZombie()
        {
            plant.zombieList.Clear();

            float minDistance = float.MaxValue;
            Zombie targetZombie = null;

            if (!plant.board.IsObjExist())
                return null;
            foreach (var zombie in plant.board.zombieArray)
            {
                if (!zombie.IsObjExist()) continue;
                if (!zombie.transform.IsObjExist()) continue;
                if (plant.vision < zombie.transform.position.x) continue;
                if (!plant.axis.IsObjExist()) continue;

                if (zombie.transform.position.x > plant.axis.transform.position.x)
                {
                    if (!SearchUniqueZombie(zombie))
                        continue;
                    float distance = Vector3.Distance(zombie.transform.position, plant.axis.transform.position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        targetZombie = zombie;
                    }
                }
            }

            if (targetZombie != null)
            {
                plant.targetZombie = targetZombie;
                return targetZombie;
            }

            return null;
        }

        public Zombie SearchMaxHealthZombie()
        {
            plant.zombieList.Clear();

            var maxHealth = long.MinValue;
            Zombie targetZombie = null;

            if (!plant.board.IsObjExist())
                return null;
            foreach (var zombie in plant.board.zombieArray)
            {
                if (!zombie.IsObjExist()) continue;
                if (!zombie.transform.IsObjExist()) continue;
                if (plant.vision < zombie.transform.position.x) continue;
                if (!plant.axis.IsObjExist()) continue;

                if (zombie.transform.position.x > plant.axis.transform.position.x)
                {
                    if (!SearchUniqueZombie(zombie))
                        continue;

                    var totalHealth = zombie.theMaxHealth + zombie.theFirstArmorMaxHealth + zombie.theSecondArmorMaxHealth;

                    if (totalHealth > maxHealth)
                    {
                        maxHealth = totalHealth;
                        targetZombie = zombie;
                    }
                }
            }

            if (targetZombie != null)
            {
                plant.targetZombie = targetZombie;
                return targetZombie;
            }

            return null;
        }

        public IEnumerator CreateBomb(int count, Zombie z)
        {
            if (!(Board.Instance != null && Board.Instance.boardTag.rogueShooting)) yield break; // 如果不是诸神
            var zombie = z;
            var visited = new HashSet<Zombie>() { z };
            for (int i = 0; i < count; ++i)
            {
                yield return new WaitForSeconds(0.15f);
                var list = Lawnf.GetAllZombies().ToArray().
                            Where(z => z.IsObjExist()).
                            Where(z => (z.GetData("HasIceDoomBomb") == null || !(bool)z.GetData("HasIceDoomBomb")) && z.Alive).
                            OrderBy(z => visited.Contains(z)).
                            ThenBy(_ => Guid.NewGuid()).ToList();
                if (list.Count > 0) zombie = list[0];
                else continue;
                if (plant == null) continue;
                var position = zombie.axis.position;
                position.y += 0.9f;
                var bomb = Instantiate(Core.IceDoomBomb, position, Quaternion.identity);
                if (!bomb.IsObjExist()) continue;
                bomb.transform.SetParent(plant.board.transform, true);
                bomb.GetComponent<IceDoomBomb>().zombie = zombie;
                bomb.GetComponent<IceDoomBomb>().parent = true;
                bomb.GetComponent<IceDoomBomb>().plant = plant;
                bomb.GetComponent<IceDoomBomb>().damage = plant.attackDamage / 10;
                bomb.GetComponent<IceDoomBomb>().Bomb();
                zombie.SetData("HasIceDoomBomb", true);
                visited.Add(zombie);
            }
        }

        public SniperPea plant => gameObject.GetComponent<SniperPea>();
    }

    public class IceDoomBomb : MonoBehaviour
    {
        public static bool isRogueShooting => Board.Instance != null ? Board.Instance.boardTag.rogueShooting : false;
        public static int bombCount
        {
            get
            {
                if (!isRogueShooting) return 0;
                if (ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var plant))
                    return plant.GetComponent<IceDoomSniperPea>().bomb;
                return 0;
            }
        }

        public int damage = 600;
        public bool parent = true;
        public Zombie zombie = null;
        public Plant plant = null;
        public bool destroy = false;

        public static int GetDamage(int origin) => origin * (1 + bombCount);

        public void Die()
        {
            try
            {
                if (zombie.IsObjExist())
                {
                    zombie.SetData("HasIceDoomBomb", false);
                    if (Board.Instance.IsObjExist())
                    {
                        int dmg = Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) ? damage * 3 : damage;
                        Action<Zombie> action = (z) =>
                        {
                            z.SetCold(10f);
                            z.AddfreezeLevel(50);
                            // 冰樱伤害
                            int inDmg = dmg;
                            if (z.GetAttrTimers().freezeTimer > 0f)
                                inDmg *= 4;
                            z.TakeDamage(DmgType.Normal, GetDamage(inDmg + Shooting_IceDoomSniperPea.ExtraDamage(zombie)), (PlantType)IceDoomSniperPea.PlantID);
                        };
                        var totalHealth = zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;
                        if (MultiLevelBuff.TravelCustomBuffLevel(BuffType.AdvancedBuff, (int)IceDoomSniperPea.buff1) == 2)
                        {
                            var damage = zombie.GetDamage((int)(totalHealth * 0.1f) + 1, DamageType.Normal, false);
                            if (zombie.theZombieType != ZombieType.TrainingDummy)
                            {
                                zombie.theHealth -= damage.SafeToInt();
                                zombie.theFirstArmorHealth -= damage.SafeToInt();
                                zombie.theSecondArmorHealth -= damage.SafeToInt();
                                UpdateHealth(zombie);
                            }
                            else // 假人造成伤害
                                zombie.TakeDamage(DmgType.IceAll, GetDamage(damage.SafeToInt() + Shooting_IceDoomSniperPea.ExtraDamage(zombie)).SafeToInt(), (PlantType)IceDoomSniperPea.PlantID);
                        }
                        else // 不拥有二级词条时造成伤害
                        {
                            IEnumerator MakeDamage()
                            {
                                yield return null;
                                if (zombie.IsObjExist())
                                {
                                    foreach (var child in zombie.transform.GetComponentsInChildren<Transform>(true))
                                        if (!child.IsObjExist()) yield break;
                                }
                                zombie.TakeDamage(DmgType.IceAll, GetDamage((int)(totalHealth * 0.05f) + 1 + Shooting_IceDoomSniperPea.ExtraDamage(zombie)), (PlantType)IceDoomSniperPea.PlantID);
                                yield break;
                            }
                            if (zombie.IsObjExist() && zombie.isActiveAndEnabled && zombie.gameObject.active)
                                zombie.StartCoroutine(MakeDamage());
                        }
                        Board.Instance.boardAction.CreateCherryExplode(transform.position, zombie.theZombieRow, CherryBombType.IceCharry, dmg, action: action, fromType: (PlantType)IceDoomSniperPea.PlantID);
                    }
                    if (Lawnf.TravelAdvanced(IceDoomSniperPea.buff1))
                        Diffusion();
                }
                Destroy(gameObject);
            }
            catch (Exception) { }
        }

        public void Start()
        {
            try
            {
                if (!zombie.IsObjExist())
                    return;
                if (zombie.GetData("HasIceDoomBomb") is true)
                {
                    Destroy(gameObject);
                    return;
                }
                zombie.SetData("HasIceDoomBomb", true);
            }
            catch (Exception) { }
        }

        public void Diffusion()
        {
            try
            {
                if (!parent)
                    return;

                foreach (var collider in Physics2D.OverlapCircleAll(transform.position, 1f, zombie.zombieLayer))
                {
                    if (!collider.IsObjExist() || !collider.gameObject.IsObjExist() || collider.gameObject.IsDestroyed()) continue;
                    if (!collider.gameObject.TryGetComponent<Zombie>(out var z)) continue;
                    if (!z.IsObjExist() || z.IsDestroyed()) continue;
                    if (z == zombie) continue;

                    var position = z.axis.transform.position;
                    position.y += 0.9f;
                    var bomb = Instantiate(Core.IceDoomBomb, position, Quaternion.identity);
                    bomb.transform.SetParent(z.transform, true);
                    bomb.GetComponent<IceDoomBomb>().zombie = z;
                    bomb.GetComponent<IceDoomBomb>().parent = false;
                    bomb.GetComponent<IceDoomBomb>().plant = plant;
                    bomb.GetComponent<IceDoomBomb>().damage = damage;
                }
            }
            catch (Exception) { }
        }

        public void Bomb(int attackCount = 0)
        {
            try
            {
                int dmg = Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) ? damage * 9 : damage * 3;
                Action<Zombie> action = (z) =>
                {
                    z.SetCold(10f);
                    z.AddfreezeLevel(50 / (isRogueShooting ? 2 : 1)); // 诸神下冻结值/2
                    int inDmg = dmg;
                    if (z.GetAttrTimers().freezeTimer > 0f)
                        inDmg *= 4;
                    // 冰樱伤害
                    z.TakeDamage(DmgType.Normal, GetDamage(inDmg + Shooting_IceDoomSniperPea.ExtraDamage(z)), (PlantType)IceDoomSniperPea.PlantID);
                };
                if (!zombie.IsObjExist())
                {
                    Destroy(gameObject);
                    return;
                }
                var board = Board.Instance;
                if (!board.IsObjExist())
                {
                    Destroy(gameObject);
                    return;
                }
                board.boardAction.CreateCherryExplode(gameObject.transform.position, zombie.theZombieRow,
                    CherryBombType.IceCharry, dmg, action: action);
                var totalHealth = zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;
                if (MultiLevelBuff.TravelCustomBuffLevel(BuffType.AdvancedBuff, (int)IceDoomSniperPea.buff1) == 2)
                {
                    var damage = zombie.GetDamage((int)(totalHealth * 0.1f) + 1, DamageType.IceAll, false);
                    if (zombie.theZombieType != ZombieType.TrainingDummy)
                    {
                        zombie.theHealth -= damage;
                        zombie.theFirstArmorHealth -= damage.SafeToInt();
                        zombie.theSecondArmorHealth -= damage.SafeToInt();
                        UpdateHealth(zombie);
                    }
                    else // 二级词条 && 非木偶
                        zombie.TakeDamage(DmgType.IceAll, GetDamage((damage.SafeToInt() + Shooting_IceDoomSniperPea.ExtraDamage(zombie))).SafeToInt(), (PlantType)IceDoomSniperPea.PlantID);
                }
                else // 不拥有二级词条
                    zombie.TakeDamage(DmgType.IceAll, GetDamage((int)(totalHealth * 0.05f) + 1 + Shooting_IceDoomSniperPea.ExtraDamage(zombie)), (PlantType)IceDoomSniperPea.PlantID);

                bool iceDoom = false;

                #region 超级爆头
                // 12发的超级爆头
                var shoot = isRogueShooting ? 24 : 12;
                if (ShootingManager.Instance != null)
                    if (ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var sniper))
                        shoot -= sniper.GetComponent<IceDoomSniperPea>().bomb * 2;
                // var shoot = 12;
                if (attackCount % shoot == 0)
                {
                    if (!board.boardTag.rogueShooting)
                    {
                        if (TypeMgr.IsBossZombie(zombie.theZombieType))
                        {
                            zombie.theHealth -= 10000;
                            zombie.theFirstArmorHealth -= 10000;
                            zombie.theSecondArmorHealth -= 10000;
                            UpdateHealth(zombie);
                        }
                        else
                            zombie.TakeDamage(DmgType.MaxDamage, int.MaxValue, (PlantType)IceDoomSniperPea.PlantID);
                    }
                    else
                    {
                        Shooting_IceDoomSniperPea.RogueSuperShoot(zombie);
                    }
                    SetIceDoom(ref iceDoom);
                }

                // 2级词条的超级爆头
                if (MultiLevelBuff.TravelCustomBuffLevel(BuffType.AdvancedBuff, (int)IceDoomSniperPea.buff1) == 2)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        if (!board.boardTag.rogueShooting)
                        {
                            if (TypeMgr.IsBossZombie(zombie.theZombieType))
                            {
                                zombie.theHealth -= 10000;
                                zombie.theFirstArmorHealth -= 10000;
                                zombie.theSecondArmorHealth -= 10000;
                                UpdateHealth(zombie);
                            }
                            else
                                zombie.TakeDamage(DmgType.MaxDamage, int.MaxValue, (PlantType)IceDoomSniperPea.PlantID);
                        }
                        else
                        {
                            Shooting_IceDoomSniperPea.RogueSuperShoot(zombie);
                        }
                    }
                    SetIceDoom(ref iceDoom);
                }

                // 诸神选词条的超级爆头
                if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var p) &&
                    p.GetComponent<IceDoomSniperPea>().superShot) // 检查flag
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        Shooting_IceDoomSniperPea.RogueSuperShoot(zombie);
                    }
                    SetIceDoom(ref iceDoom);
                }
                #endregion

                if (iceDoom && Lawnf.TravelAdvanced(IceDoomSniperPea.buff1))
                {
                    foreach (var z in board.zombieArray)
                    {
                        if (!z.IsObjExist() || z.IsDestroyed()) continue;
                        if (z == zombie) continue;
                        var position = z.axis.transform.position;
                        position.y += 0.9f;
                        var bomb = Instantiate(Core.IceDoomBomb, position, Quaternion.identity);
                        bomb.transform.SetParent(z.transform, true);
                        bomb.GetComponent<IceDoomBomb>().zombie = z;
                        bomb.GetComponent<IceDoomBomb>().parent = false;
                        bomb.GetComponent<IceDoomBomb>().plant = plant;
                        bomb.GetComponent<IceDoomBomb>().damage = damage;
                    }
                }
                if (Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) && !iceDoom)
                {
                    Diffusion();
                }
                destroy = true;
                Destroy(gameObject);
            }
            catch (Exception) { }
        }

        public void OnDestroy()
        {
            try
            {
                if (!destroy)
                {
                    destroy = true;
                    Die();
                }
                if (gameObject != null)
                    Destroy(gameObject);
                if (zombie != null)
                    zombie.SetData("HasIceDoomBomb", false);
            }
            catch (Exception e) { }
        }

        public void SetIceDoom(ref bool iceDoom)
        {
            try
            {
                if (!Board.Instance.IsObjExist()) return;
                if (isRogueShooting)
                {
                    if (ShootingManager.Instance != null &&
                        ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var plant))
                        if (!plant.GetComponent<IceDoomSniperPea>().superShot)
                        {
                            int dmg = Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) ? 1800 : 600;
                            Action<Zombie> action = (z) =>
                            {
                                z.SetCold(10f);
                                z.AddfreezeLevel(50 / (isRogueShooting ? 2 : 1)); // 诸神下冻结值/2
                                int inDmg = dmg;
                                if (z.GetAttrTimers().freezeTimer > 0f)
                                    inDmg *= 4;
                                // 冰樱伤害
                                z.TakeDamage(DmgType.Normal, GetDamage(inDmg + Shooting_IceDoomSniperPea.ExtraDamage(z)), (PlantType)IceDoomSniperPea.PlantID);
                            };

                            var board = Board.Instance;
                            if (!board.IsObjExist())
                            {
                                Destroy(gameObject);
                                return;
                            }

                            board.boardAction.CreateCherryExplode(gameObject.transform.position,
                                !zombie.IsObjExist() ? Mouse.Instance.GetRowFromY(transform.position.x, transform.position.y) : zombie.theZombieRow,
                                CherryBombType.IceCharry, dmg, action: action);
                            return; // 没有质变就提前返回
                        }
                }
                Board.Instance.boardAction.SetDoom(0, 0, false, true, zombie.axis.transform.position,
                    (Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) ? 21600 : 7200),
                    fromType: (PlantType)IceDoomSniperPea.PlantID);
                if (isRogueShooting && ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var p))
                {
                    var multi = (int)p.GetComponent<IceDoomSniperPea>().health * 0.005f;
                    foreach (var zombie in Lawnf.GetAllZombies())
                    {
                        zombie.TakeDamage(DamageType.IceAll, GetDamage((int)(zombie.GetTotalHealth() * multi)), (PlantType)IceDoomSniperPea.PlantID);
                    }
                }
                iceDoom = true;
            }
            catch (Exception) { }
        }

        public static void UpdateHealth(Zombie z)
        {
            try
            {
                if (z.theFirstArmorHealth < 0)
                    z.theFirstArmorHealth = 0;
                if (z.theSecondArmorHealth < 0)
                    z.theSecondArmorHealth = 0;
                z.UpdateHealthText();
            }
            catch (Exception) { }
        }
    }

    #region 诸神
    public class Shooting_IceDoomSniperPea : BaseConfig
    {
        // 实现il2cpp要求实现的方法
        public Shooting_IceDoomSniperPea(IntPtr ptr) : base(ptr) { }
        public Shooting_IceDoomSniperPea() : base(ClassInjector.DerivedConstructorPointer<Shooting_IceDoomSniperPea>()) =>
            ClassInjector.DerivedConstructorBody(this);
        // 实现抽象类的方法
        public override PlantType PlantType => (PlantType)IceDoomSniperPea.PlantID;
        public override string Role => "输出";
        public override Il2CppSystem.Collections.Generic.List<BaseBuff> Buffs
        {
            get
            {
                var result = new Il2CppSystem.Collections.Generic.List<BaseBuff>();
                foreach (var item in CustomBuffs) result.Add(item);
                return result;
            }
        }
        private List<BaseBuff> CustomBuffs = new()
        {
            new DamageBuff((PlantType)IceDoomSniperPea.PlantID),
            // new SpeedBuff((PlantType)IceDoomSniperPea.PlantID),
            new BombBuff(),
            new SuperShotBuff(),
            new UniqueUpgrade(),
            new SniperBuff()
        };

        public override void ReinforcePlant(Plant plant)
        {
            // plant.ModifyDamage(PlantDamageAdder.Shooting, 1f, false, new(float.MaxValue)); // 1倍增伤
            plant.attackDamage = 1200;
            plant.ModifySpeed(PlantSpeedAdder.Shooting, 2f);
        }

        public static void RogueSuperShoot(Zombie zombie)
        {
            if (ShootingManager.Instance != null && ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var plant))
            {
                if (!zombie.IsObjExist()) return;
                if (zombie.theZombieType == ZombieType.HorseBoss || zombie.theZombieType == ZombieType.ZombieBoss ||
                    zombie.theZombieType == ZombieType.ZombieBoss2)
                {
                    // 有爆头质变x0.1，否则x0.01
                    var healthDmg = (int)(zombie.theMaxHealth * (plant.GetComponent<IceDoomSniperPea>().superShot ? 0.1f : 0.01f)); 
                    zombie.TakeDamage(IceDoomBomb.GetDamage(10000 + healthDmg + ExtraDamage(zombie)), plant, DamageType.IceAll);
                }
                else
                {
                    if (plant.GetComponent<IceDoomSniperPea>().superShot)
                    {
                        zombie.theHealth = 0;
                        zombie.theFirstArmorHealth = 0;
                        zombie.theSecondArmorHealth = 0;
                        zombie.TakeDamage(DamageType.IceAll, IceDoomBomb.GetDamage(zombie.GetTotalMaxHealth()), (PlantType)IceDoomSniperPea.PlantID);
                    }
                    else
                        zombie.TakeDamage(IceDoomBomb.GetDamage(10_0000 + ExtraDamage(zombie)), plant, DamageType.IceAll);
                }
            }
        }

        public void ResetQuality()
        {
            CustomBuffs[0] = new DamageBuff((PlantType)IceDoomSniperPea.PlantID);
            // CustomBuffs[1].Cast<SpeedBuff>().randomQuality = ShootingManager.Instance.GetRandomQuality();
        }

        public static int ExtraDamage(Zombie zombie)
        {
            if (ShootingManager.Instance != null && 
                ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var plant))
            {
                return (int)(zombie.GetTotalHealth() * 0.05f * (int)plant.GetComponent<IceDoomSniperPea>().health);
            }
            return 0;
        }

        public class UniqueUpgrade : BaseBuff
        {
            // 实现il2cpp的方法
            public UniqueUpgrade(IntPtr ptr) : base(ptr) { }
            public UniqueUpgrade() : base(ClassInjector.DerivedConstructorPointer<UniqueUpgrade>()) =>
                ClassInjector.DerivedConstructorBody(this);
            // 实现抽象类的方法
            public override float AppearWeight => 0.167f;
            public override string Description => "引爆的炸弹附加僵尸血量的+0.5%";
            public override int MaxCount => 5;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => (PlantType)IceDoomSniperPea.PlantID;
            public override string Title => "强化：炸弹";
            public override void OnGet()
            {
                if (ShootingManager.Instance != null && 
                    ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var plant))
                {
                    Action<Plant> action = (p) => p.GetComponent<IceDoomSniperPea>().AddHealth();
                    SafeModify(action);
                }
            }
        }

        public class SniperBuff : BaseBuff
        {
            // 实现il2cpp的方法
            public SniperBuff(IntPtr ptr) : base(ptr) { }
            public SniperBuff() : base(ClassInjector.DerivedConstructorPointer<SniperBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            // 实现抽象类的方法
            public override float AppearWeight => 0.33f;
            public override string Description => "炸弹伤害+100%\n每次引爆炸弹会使随机一个僵尸额外引起一次10%伤害的爆炸，每次选择都会增加一次爆炸次数";
            public override int MaxCount => 10;
            public override Quality Rarity => Quality.gold;
            public override PlantType ShowType => (PlantType)IceDoomSniperPea.PlantID;
            public override string Title => "强化：子母弹";
            public override void OnGet()
            {
                if (ShootingManager.Instance != null &&
                    ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var plant))
                {
                    Action<Plant> action = (p) => p.GetComponent<IceDoomSniperPea>().AddBomb();
                    SafeModify(action);
                }
            }
        }

        public class BombBuff : BaseBuff
        {
            // 实现il2cpp的方法
            public BombBuff(IntPtr ptr) : base(ptr) { }
            public BombBuff() : base(ClassInjector.DerivedConstructorPointer<BombBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            // 实现抽象类的方法
            public override float AppearWeight => 0.05f;
            public override string Description => "获得词条：精装炸弹";
            public override int MaxCount => 1;
            public override Quality Rarity => Quality.diamond;
            public override PlantType ShowType => (PlantType)IceDoomSniperPea.PlantID;
            public override string Title => "质变：全副武装";
            public override void OnGet()
            {
                if (TravelMgr.Instance != null)
                {
                    TravelMgr.Instance.GetNormalBuff(IceDoomSniperPea.buff1); // 获取精装炸弹
                    // TravelMgr.Instance.GetNormalBuff(IceDoomSniperPea.buff2); // 获取夜影暗袭
                }
            }
        }
        public class SuperShotBuff : BaseBuff
        {
            // 实现il2cpp的方法
            public SuperShotBuff(IntPtr ptr) : base(ptr) { }
            public SuperShotBuff() : base(ClassInjector.DerivedConstructorPointer<SuperShotBuff>()) =>
                ClassInjector.DerivedConstructorBody(this);
            // 实现抽象类的方法
            public override float AppearWeight => 0.05f;
            public override string Description => "每次引爆炸弹有50%概率触发更强的爆炸，同时造成超级爆头\n超级爆头的伤害大幅提升";
            public override int MaxCount => 1;
            public override Quality Rarity => Quality.diamond;
            public override PlantType ShowType => (PlantType)IceDoomSniperPea.PlantID;
            public override string Title => "质变：超级狙击";
            public override void OnGet()
            {
                if (ShootingManager.Instance != null &&
                    ShootingManager.Instance.TryGetPlant((PlantType)IceDoomSniperPea.PlantID, out var plant))
                {
                    plant.GetComponent<IceDoomSniperPea>().superShot = true; // flag = 1
                }
            }
        }

    }

    [HarmonyPatch(typeof(GameLevel.RogueShooting.SniperPea))]
    public static class ShootingSniperPeaPatch
    {
        [HarmonyPatch(nameof(GameLevel.RogueShooting.SniperPea.Buffs), MethodType.Getter)]
        [HarmonyPostfix]
        public static void PostGetBuffs(ref Il2CppSystem.Collections.Generic.List<BaseBuff> __result)
        {
            __result.Add(new UpgradeBuff(PlantType.SniperPea, (PlantType)IceDoomSniperPea.PlantID));
        }
    }

    [HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManagerPatch
    {
        [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
        [HarmonyPrefix]
        public static void PreShowBuff()
        {
            if (Config.configs != null)
            {
                if (!Config.configs.ContainsKey((PlantType)IceDoomSniperPea.PlantID))
                    Config.configs.Add((PlantType)IceDoomSniperPea.PlantID, new Shooting_IceDoomSniperPea());
                else
                    Config.configs[(PlantType)IceDoomSniperPea.PlantID].
                        Cast<Shooting_IceDoomSniperPea>().ResetQuality(); // 重置品质

            }
        }
    }

    [HarmonyPatch(typeof(Zombie))]
    public static class ZombiePatch
    {
        [HarmonyPatch(nameof(Zombie.AnimLoseActive))]
        [HarmonyPrefix]
        public static bool PreAnimLoseActive(Zombie __instance, ref Transform obj)
        {
            if (!obj.IsObjExist())
                return false;
            return true;
        }
    }
    #endregion
}