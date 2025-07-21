using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace IceSniperBepInEx
{
    [BepInPlugin("salmon.icesniperpea", "IceSniperPea", "1.0")]
    public class Core : BasePlugin//304
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<IceSniper>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "icesniper");
            List<ValueTuple<int, int>> list = new List<ValueTuple<int, int>>
            {
                ((int)PlantType.SniperPea, (int)PlantType.IceShroom), 
                ((int)PlantType.IceShroom, (int)PlantType.SniperPea), 
                ((int)PlantType.SuperSnowGatling, (int)PlantType.Peashooter), 
                ((int)PlantType.Peashooter, (int)PlantType.SuperSnowGatling)
            };
            CustomCore.RegisterCustomPlant<SniperPea, IceSniper>(IceSniper.PlantID, ab.GetAsset<GameObject>("IceSniperPrefab"),
                ab.GetAsset<GameObject>("IceSniperPreview"), list, 3f, 0f, 300, 300, 0, 675);
            CustomCore.AddPlantAlmanacStrings(IceSniper.PlantID, "寒冰狙击豌豆(" + IceSniper.PlantID + ")", "定期狙击一只僵尸，造成高额伤害并施加减速效果\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>300/3秒</color>\n<color=#3D1400>特点：</color><color=red>特点同狙击射手，每次攻击为目标僵尸累积75点冻结值和15秒减速，每6次狙击对目标僵尸造成100万伤害，可以与超级寒冰机枪射手相转化</color>\n<color=#3D1400>融合配方：</color><color=red>狙击射手+寒冰菇</color>\n<color=#3D1400>转换配方：</color><color=red>豌豆射手←→豌豆射手</color>\n\n<color=#3D1400>“时间热寂观测站”前首席计量员。它说10秒不是冻结，是给僵尸的怀表上了发条——往回调的！6次‘绝对零度收据’？嘘——别数，那是宇宙在它算盘上打了个寒颤！警告：请勿在易碎草坪、热血哲学家或试图计算自己熵值的僵尸面前种植… 毕竟，它射出的不是豌豆，是讣告——温度计当笔写的，且不接受体温复议（包括临终忏悔）。</color>");
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)IceSniper.PlantID);
            CustomCore.AddFusion((int)PlantType.SuperSnowGatling, IceSniper.PlantID, (int)PlantType.Peashooter);
            CustomCore.AddFusion((int)PlantType.SuperSnowGatling, (int)PlantType.Peashooter, IceSniper.PlantID);
        }
    }

    public class IceSniper : MonoBehaviour
    {
        public static int PlantID = 1902;

        public SniperPea plant => gameObject.GetComponent<SniperPea>();

        public IceSniper() : base(ClassInjector.DerivedConstructorPointer<IceSniper>()) => ClassInjector.DerivedConstructorBody(this);

        public IceSniper(IntPtr i) : base(i)
        {
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
        public void AttackZombie(Zombie zombie, int damage)
        {
            if (zombie == null) return;

            // 造成伤害
            if (zombie.freezeTimer > 0)
                zombie.TakeDamage(DmgType.Normal, damage * 4);
            else
                zombie.TakeDamage(DmgType.Normal, damage);
            zombie.SetCold(10f);
            zombie.AddfreezeLevel(75);

            // 计算生成位置
            Vector3 spawnPos = plant.ac.transform.position;

            // 获取父级变换组件
            Transform parentTransform = plant.board.transform;

            CreateParticle.SetParticle(
                theParticleType: 0x1C,
                position: spawnPos,
                row: plant.targetZombie.theZombieRow
            );
        }

        public void AnimShoot_IceSniper()
        {
            GameAPP.PlaySound(40, 0.2f, 1.0f);

            var targetZombie = plant.targetZombie;

            if (targetZombie == null || !SearchUniqueZombie(targetZombie))
                return;


            plant.attackCount++;

            int damage = plant.attackDamage;
            if (plant.attackCount % 6 == 0)
            {
                damage = 1000000;
            }

            AttackZombie(targetZombie, damage);

            if (targetZombie.theStatus != ZombieStatus.Dying && !targetZombie.beforeDying)
                return;

            plant.targetZombie = null;
            return;
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
    }
}