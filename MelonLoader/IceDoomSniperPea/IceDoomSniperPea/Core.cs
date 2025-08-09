using MelonLoader;
using CustomizeLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using Il2Cpp;
using CustomizeLib.MelonLoader;

[assembly: MelonInfo(typeof(IceDoomSniperPea.Core), "IceDoomSniperPea", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace IceDoomSniperPea
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "icedoomsniperpea");
            CustomCore.RegisterCustomPlant<SniperPea, IceDoomSniperPea>(IceDoomSniperPea.PlantID, ab.GetAsset<GameObject>("IceDoomSniperPeaPrefab"),
                ab.GetAsset<GameObject>("IceDoomSniperPeaPreview"), [((int)PlantType.SniperPea, (int)PlantType.IceDoom), ((int)PlantType.IceDoom, (int)PlantType.SniperPea)], 6f, 0f, 600, 300, 0, 800);
            CustomCore.AddPlantAlmanacStrings(IceDoomSniperPea.PlantID, "冰毁狙击豌豆(" + IceDoomSniperPea.PlantID + ")", "定期狙击僵尸，造成冰毁爆炸和高额伤害\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>600/6秒</color>\n<color=#3D1400>特点：</color><color=red>特点同狙击射手，每次攻击为目标僵尸累积75点冻结值、15秒减速以及5秒冻结，每2次狙击在目标僵尸的位置释放不留坑洞不冻结关卡的寒冰毁灭菇效果，每6次狙击对目标僵尸造成21亿普通伤害</color>\n<color=#3D1400>融合配方：</color><color=red>狙击射手+寒冰毁灭菇</color>\n<color=#3D1400>词条1：</color><color=red>瞬狙：冰毁狙击的狙击间隔缩短为4秒，每次攻击有概率连狙</color>\n<color=#3D1400>词条2：</color><color=red>狙狙爆：冰毁狙击每次狙击都释放一次寒冰毁灭菇效果</color><color=#3D1400>\n词条3：</color><color=red>背起了行囊：冰毁狙击每次狙击都造成21亿伤害，该词条需要解锁词条1和词条2后才可获得</color>\n\n<color=#3D1400>“嘿，离我远点儿！”他时刻提防着要靠近他的植物和僵尸，这股力量太过强大太过危险，连自己都无法完全掌控，为此他远离其他植物，或许英雄都是孤独的.....（不合群的他有一股热枕的心，被他救下的植物甚至没有见到他，只见到了那股力量，如同神明，如同死神）</color>");
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)IceDoomSniperPea.PlantID);
            IceDoomSniperPea.buff1 = CustomCore.RegisterCustomBuff("瞬狙：冰毁狙击的狙击间隔缩短为4秒，每次攻击有概率连狙", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<IceDoomSniperPea>(), 5000, "#DA64FF", (PlantType)IceDoomSniperPea.PlantID);
            IceDoomSniperPea.buff2 = CustomCore.RegisterCustomBuff("狙狙爆：冰毁狙击每次狙击都释放一次寒冰毁灭菇效果", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<IceDoomSniperPea>(), 5000, "#DA64FF", (PlantType)IceDoomSniperPea.PlantID);
            IceDoomSniperPea.buff3 = CustomCore.RegisterCustomBuff("背起了行囊：冰毁狙击每次狙击都造成21亿伤害", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<IceDoomSniperPea>() && Lawnf.TravelAdvanced(IceDoomSniperPea.buff1) && Lawnf.TravelAdvanced(IceDoomSniperPea.buff2), 15000, "#DA64FF", (PlantType)IceDoomSniperPea.PlantID);

            CustomCore.AddFusion(IceDoomSniperPea.PlantID, 1902, (int)PlantType.DoomShroom);
            CustomCore.AddFusion(IceDoomSniperPea.PlantID, (int)PlantType.DoomShroom, 1902);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class IceDoomSniperPea : MonoBehaviour
    {
        public static int PlantID = 1900;
        public static int buff1 = -1;
        public static int buff2 = -1;
        public static int buff3 = -1;

        public IceDoomSniperPea() : base(ClassInjector.DerivedConstructorPointer<IceDoomSniperPea>()) => ClassInjector.DerivedConstructorBody(this);

        public IceDoomSniperPea(IntPtr i) : base(i)
        {
        }

        public void AttackZombie(Zombie zombie, int damage)
        {
            if (zombie == null) return;

            // 造成伤害
            zombie.TakeDamage(DmgType.Normal, damage);
            zombie.SetCold(15f);
            zombie.SetFreeze(5);
            zombie.AddfreezeLevel(75);

            if (plant.attackCount % 12 == 0)
            {
                int totalHealth = zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;
                int totalMaxHealth = zombie.theMaxHealth + zombie.theFirstArmorMaxHealth + zombie.theSecondArmorMaxHealth;
                totalHealth = totalHealth < totalMaxHealth ? totalHealth : totalMaxHealth;
                if (totalHealth < (((int)totalMaxHealth * 0.75) + 1))
                {
                    zombie.Die();
                }
            }
            int random = UnityEngine.Random.Range(0, 3); // 在0-2之间取随机数
            if (Lawnf.TravelAdvanced(buff1) && random == 1) //如果随机数不为0且开启顺狙词条
            {
                plant.thePlantAttackCountDown = 0.1f; //植物攻击间隔设为0.1秒（≈连狙）
            }

            // 计算生成位置
            Vector3 spawnPos = plant.ac.transform.position;

            // 获取父级变换组件
            Transform parentTransform = plant.board.transform;

            // CreateItem.Instance.SetCoin(0, 0, 13, 0, plant.targetZombie.axis.transform.position, false); //13为小阳光类型

            CreateParticle.SetParticle(
                theParticleType: 0x1C,
                position: spawnPos,
                row: plant.targetZombie.theZombieRow
            );
        }

        public void AnimShoot_IceDoom()
        {
            GameAPP.PlaySound(40, 0.2f, 1.0f);

            var targetZombie = plant.targetZombie;

            if (targetZombie == null || !SearchUniqueZombie(targetZombie))
                return;


            plant.attackCount++;

            int damage = plant.attackDamage;
            if (plant.attackCount % 6 == 0 || Lawnf.TravelAdvanced(buff3))
            {
                damage = 2147483647;
            }
            if (plant.attackCount % 2 == 0 || Lawnf.TravelAdvanced(buff2))
            {
                GameAPP.board.GetComponent<Board>().SetDoom(0, 0, false, true, targetZombie.axis.position, 3600);
            }

            AttackZombie(targetZombie, damage);

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
        public UnityEngine.GameObject SearchZombie()
        {
            plant.zombieList.Clear();

            float minDistance = float.MaxValue;
            UnityEngine.GameObject targetZombie = null;

            if (plant.board != null)
            {
                foreach (var zombie in plant.board.zombieArray)
                {
                    if (zombie == null) continue;

                    var zombieTransform = zombie.transform;
                    if (zombieTransform == null) continue;

                    if (plant.vision < zombieTransform.position.x) continue;

                    var axisTransform = plant.axis;
                    if (axisTransform == null) continue;

                    if (zombieTransform.position.x > axisTransform.position.x)
                    {
                        if (SearchUniqueZombie(zombie))
                        {
                            float distance = Vector3.Distance(zombieTransform.position, axisTransform.position);

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                targetZombie = zombie.gameObject;
                            }
                        }
                    }
                }
            }

            if (targetZombie != null)
            {
                plant.targetZombie = targetZombie.GetComponent<Zombie>();
                return targetZombie;
            }

            return null;
        }

        public SniperPea plant => gameObject.GetComponent<SniperPea>();
    }

    [HarmonyLib.HarmonyPatch(typeof(SniperPea), nameof(SniperPea.Update))]
    public class SniperPea_Update
    {
        [HarmonyLib.HarmonyPrefix]
        public static void Prefix(SniperPea __instance)
        {
            if (__instance == null) return;
            if ((int)__instance.thePlantType == IceDoomSniperPea.PlantID)
            {
                if (Lawnf.TravelAdvanced(IceDoomSniperPea.buff1))
                {
                    if (__instance.thePlantAttackCountDown > 4f)
                    {
                        __instance.thePlantAttackCountDown = 4f;
                    }
                }
            }
        }
    }
}