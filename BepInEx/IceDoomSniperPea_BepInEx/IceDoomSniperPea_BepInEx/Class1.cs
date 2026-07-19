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

namespace IceDoomSniperPea.BepInEx
{
    [BepInPlugin("salmon.icedoomsniperpea", "IceDoomSniperPea", "1.0")]
    public class Core : BasePlugin//304
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
                ab.GetAsset<GameObject>("IceDoomSniperPeaPreviewSkin"), list, 6f, 0f, 600, 300, 7.5f, 800);
            IceDoomBomb = ab.GetAsset<GameObject>("IceDoomBomb");
            IceDoomBomb.gameObject.AddComponent<IceDoomBomb>();
            CustomCore.AddPlantAlmanacStrings(IceDoomSniperPea.PlantID, "冰毁狙击豌豆(" + IceDoomSniperPea.PlantID + ")",
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
            IceDoomSniperPea.buff1 = (AdvBuff)CustomCore.RegisterCustomBuff("精装炸弹：冰毁炸弹爆炸的伤害x3，冰毁炸弹引爆后会为附近的僵尸挂上冰毁炸弹。2级时，每次引爆都造成寒冰毁灭菇爆炸，引爆时有50%概率触发超级爆头，并附带10%韧性的伤害", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<IceDoomSniperPea>(), 5000, (PlantType)IceDoomSniperPea.PlantID, 2, BuffBgType.Night);
            IceDoomSniperPea.buff2 = (AdvBuff)CustomCore.RegisterCustomBuff("夜影暗袭：冰毁狙击射手每11发将锁定场上韧性上限最高的僵尸。每轮攻击有概率触发连狙", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<IceDoomSniperPea>(), 5000, (PlantType)IceDoomSniperPea.PlantID, bg: BuffBgType.Night);
            CustomCore.AddUltimatePlant((PlantType)IceDoomSniperPea.PlantID);
        }
    }

    public class IceDoomSniperPea : MonoBehaviour
    {
        public static int PlantID = 1900;
        public static AdvBuff buff1 = (AdvBuff)(-1);
        public static AdvBuff buff2 = (AdvBuff)(-1);

        public void AttackZombie(Zombie zombie, int damage)
        {
            try
            {
                if (zombie == null) return;

                zombie.TakeDamage(DmgType.Normal, damage);
                zombie.SetCold(10f);
                zombie.AddfreezeLevel(100);
                if (plant.attackCount % 2 == 1)
                {
                    var bomb = Instantiate(Core.IceDoomBomb, plant.ac.transform.position, Quaternion.identity);
                    bomb.transform.SetParent(zombie.transform, true);
                    bomb.GetComponent<IceDoomBomb>().zombie = zombie;
                    bomb.GetComponent<IceDoomBomb>().parent = true;
                }

                if (plant.attackCount % 2 == 0)
                {
                    var go = zombie.transform.FindChild("IceDoomBomb(Clone)").gameObject;
                    go.GetComponent<IceDoomBomb>().Bomb(plant.attackCount);
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

            if (targetZombie == null || !SearchUniqueZombie(targetZombie))
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
                    if (plant.targetZombie.isMindControlled)
                        SearchZombie();
                }
            }
            catch (Exception) { }
        }

        // 僵尸状态验证
        public bool SearchUniqueZombie(Zombie zombie)
        {
            if (zombie == null) return false;

            if (zombie.isMindControlled || zombie.beforeDying)
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

            if (plant.board == null)
                return null;
            foreach (var zombie in plant.board.zombieArray)
            {
                if (zombie == null) continue;
                if (zombie.transform == null) continue;
                if (plant.vision < zombie.transform.position.x) continue;
                if (plant.axis == null) continue;

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

            int maxHealth = int.MinValue;
            Zombie targetZombie = null;

            if (plant.board == null)
                return null;
            foreach (var zombie in plant.board.zombieArray)
            {
                if (zombie == null) continue;
                if (zombie.transform == null) continue;
                if (plant.vision < zombie.transform.position.x) continue;
                if (plant.axis == null) continue;

                if (zombie.transform.position.x > plant.axis.transform.position.x)
                {
                    if (!SearchUniqueZombie(zombie))
                        continue;

                    int totalHealth = zombie.theMaxHealth + zombie.theFirstArmorMaxHealth + zombie.theSecondArmorMaxHealth;

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

        public SniperPea plant => gameObject.GetComponent<SniperPea>();
    }

    public class IceDoomBomb : MonoBehaviour
    {
        public bool parent = true;
        public Zombie zombie = null;

        public void Die()
        {
            if (zombie != null)
            {
                zombie.SetData("HasIceDoomBomb", false);
                if (Board.Instance != null)
                {
                    int dmg = Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) ? 1800 : 600;
                    Action<Zombie> action = (z) =>
                    {
                        z.SetCold(10f);
                        z.AddfreezeLevel(50);
                        if (z.GetAttrTimers().freezeTimer > 0f)
                            z.TakeDamage(DmgType.Normal, dmg * 3);
                    };
                    Board.Instance.boardAction.CreateCherryExplode(transform.position, zombie.theZombieRow, CherryBombType.IceCharry, dmg, action: action, fromType: (PlantType)IceDoomSniperPea.PlantID);
                    int totalHealth = zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;
                    if (MultiLevelBuff.TravelCustomBuffLevel(BuffType.AdvancedBuff, (int)IceDoomSniperPea.buff1) == 2)
                    {
                        int damage = zombie.GetDamage((int)(totalHealth * 0.1f) + 1, DamageType.Normal, false);
                        if (zombie.theZombieType == ZombieType.TrainingDummy)
                        {
                            zombie.theHealth -= damage;
                            zombie.theFirstArmorHealth -= damage;
                            zombie.theSecondArmorHealth -= damage;
                            UpdateHealth(zombie);
                        }
                        else
                            zombie.TakeDamage(DmgType.IceAll, damage, (PlantType)IceDoomSniperPea.PlantID);
                    }
                    else
                        zombie.TakeDamage(DmgType.Normal, (int)(totalHealth * 0.05f) + 1, (PlantType)IceDoomSniperPea.PlantID);
                }
                if (Lawnf.TravelAdvanced(IceDoomSniperPea.buff1))
                    Diffusion();
            }
            Destroy(gameObject);
        }

        public void Start()
        {
            if (zombie == null)
                return;
            if (zombie.GetData("HasIceDoomBomb") is true)
            {
                Destroy(gameObject);
                return;
            }
            zombie.SetData("HasIceDoomBomb", true);
        }

        public void Diffusion()
        {
            if (!parent)
                return;

            foreach (var collider in Physics2D.OverlapCircleAll(transform.position, 1f, zombie.zombieLayer))
            {
                if (collider == null || collider.gameObject == null || collider.gameObject.IsDestroyed()) continue;
                if (!collider.gameObject.TryGetComponent<Zombie>(out var z)) continue;
                if (z == null || z.IsDestroyed()) continue;
                if (z == zombie) continue;

                var position = z.axis.transform.position;
                position.y += 0.9f;
                var bomb = Instantiate(Core.IceDoomBomb, position, Quaternion.identity);
                bomb.transform.SetParent(z.transform, true);
                bomb.GetComponent<IceDoomBomb>().zombie = z;
                bomb.GetComponent<IceDoomBomb>().parent = false;
            }
        }

        public void Bomb(int attackCount = 0)
        {
            int dmg = Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) ? 1800 : 600;
            Action<Zombie> action = (z) =>
            {
                z.SetCold(10f);
                z.AddfreezeLevel(50);
                if (z.GetAttrTimers().freezeTimer > 0f)
                    z.TakeDamage(DmgType.Normal, dmg * 3, (PlantType)IceDoomSniperPea.PlantID);
            };

            Board board = Board.Instance;
            if (board == null)
            {
                Destroy(gameObject);
                return;
            }

            board.boardAction.CreateCherryExplode(gameObject.transform.position, zombie.theZombieRow, CherryBombType.IceCharry, dmg, action: action);
            int totalHealth = zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;
            if (MultiLevelBuff.TravelCustomBuffLevel(BuffType.AdvancedBuff, (int)IceDoomSniperPea.buff1) == 2)
            {
                int damage = zombie.GetDamage((int)(totalHealth * 0.1f) + 1, DamageType.Normal, false);
                if (zombie.theZombieType == ZombieType.TrainingDummy)
                {
                    zombie.theHealth -= damage;
                    zombie.theFirstArmorHealth -= damage;
                    zombie.theSecondArmorHealth -= damage;
                    UpdateHealth(zombie);
                }
                else
                    zombie.TakeDamage(DmgType.IceAll, damage, (PlantType)IceDoomSniperPea.PlantID);
            }
            else
                zombie.TakeDamage(DmgType.Normal, (int)(totalHealth * 0.05f) + 1, (PlantType)IceDoomSniperPea.PlantID);

            bool iceDoom = false;

            if (attackCount % 12 == 0)
            {
                if (TypeMgr.IsBossZombie(zombie.theZombieType))
                {
                    zombie.theHealth -= 10000;
                    zombie.theFirstArmorHealth -= 10000;
                    zombie.theSecondArmorHealth -= 10000;
                    UpdateHealth(zombie);
                }
                else
                    zombie.TakeDamage(DmgType.MaxDamage, int.MaxValue);
                board.boardAction.SetDoom(0, 0, false, true, zombie.axis.transform.position, (Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) ? 21600 : 7200), fromType: (PlantType)IceDoomSniperPea.PlantID);
                iceDoom = true;
            }

            if (MultiLevelBuff.TravelCustomBuffLevel(BuffType.AdvancedBuff, (int)IceDoomSniperPea.buff1) == 2)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
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
                board.boardAction.SetDoom(0, 0, false, true, zombie.axis.transform.position, (Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) ? 21600 : 7200), fromType: (PlantType)IceDoomSniperPea.PlantID);
                iceDoom = true;
            }

            if (iceDoom && Lawnf.TravelAdvanced(IceDoomSniperPea.buff1))
            {
                foreach (var z in board.zombieArray)
                {
                    if (z == null || z.IsDestroyed()) continue;
                    if (z == zombie) continue;

                    var position = z.axis.transform.position;
                    position.y += 0.9f;
                    var bomb = Instantiate(Core.IceDoomBomb, position, Quaternion.identity);
                    bomb.transform.SetParent(z.transform, true);
                    bomb.GetComponent<IceDoomBomb>().zombie = z;
                    bomb.GetComponent<IceDoomBomb>().parent = false;
                }
            }

            if (Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) && !iceDoom)
            {
                Diffusion();
            }

            Destroy(gameObject);
        }

        public void OnDestroy()
        {
            try
            {
                if (gameObject != null)
                    Destroy(gameObject);
                if (zombie != null)
                    zombie.SetData("HasIceDoomBomb", false);
            }
            catch (Exception e) { }
        }

        public static void UpdateHealth(Zombie z)
        {
            if (z.theFirstArmorHealth < 0)
                z.theFirstArmorHealth = 0;
            if (z.theSecondArmorHealth < 0)
                z.theSecondArmorHealth = 0;
            z.UpdateHealthText();
        }
    }
}