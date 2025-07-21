using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace PuffFireSniperPeaBepInEx
{
    [BepInPlugin("salmon.pufffiresniperpea", "PuffFireSniperPea", "1.0")]
    public class Core : BasePlugin//304
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<PuffFireSniperPea>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "pufffiresniperpea");
            CustomCore.RegisterCustomPlant<FireSniper, PuffFireSniperPea>(PuffFireSniperPea.PlantID, ab.GetAsset<GameObject>("PuffFireSniperPeaPrefab"),
                ab.GetAsset<GameObject>("PuffFireSniperPeaPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.SmallPuff, (int)PlantType.FireSniper),
                    (1908, (int)PlantType.Jalapeno)
                }, 3f, 0f, 300, 300, 0f, 725);
            CustomCore.AddPlantAlmanacStrings(PuffFireSniperPea.PlantID, "火焰小喷菇狙击豌豆(" + PuffFireSniperPea.PlantID + ")", "定期狙击一只僵尸，造成高额伤害并施加红温状态。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>300/3秒</color>\n<color=#3D1400>特点：</color><color=red>同火焰狙击射手。</color>\n<color=#3D1400>融合配方：</color><color=red>小喷菇（底座）+火焰狙击射手</color>\n\n<color=#3D1400>冰原最重要的是什么，那当然是火了！可惜隔壁那位冒失的家伙一不小心就把营地整的一团糟，这时候得请一位专业人士来帮忙了。“你是说，我的狙击任务就只是帮忙点营地的火堆？”火焰狙击小喷菇气愤的问道。</color>");
            CustomCore.TypeMgrExtra.IsPuff.Add((PlantType)PuffFireSniperPea.PlantID);
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)PuffFireSniperPea.PlantID);
            CustomCore.AddFusion(PuffFireSniperPea.PlantID, 1912, (int)PlantType.Peashooter);
        }
    }

    public class PuffFireSniperPea : MonoBehaviour
    {
        public static int PlantID = 1911;

        public PuffFireSniperPea() : base(ClassInjector.DerivedConstructorPointer<PuffFireSniperPea>()) => ClassInjector.DerivedConstructorBody(this);

        public PuffFireSniperPea(IntPtr i) : base(i)
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
            zombie.TakeDamage(DmgType.Normal, damage);
            zombie.SetJalaed();
            zombie.JalaedExplode();

            // 计算生成位置
            /*Vector3 spawnPos = plant.ac.transform.position;

            // 获取父级变换组件

            var particlePrefab = GameAPP.particlePrefab[0];
            var acTransform = plant.ac.transform;
            var position = acTransform.position;

            var particle = UnityEngine.Object.Instantiate(
                particlePrefab,
                position,
                Quaternion.identity,
                plant.board.transform
            );*/
        }

        public void AnimShoot_PuffFireSniperPea()
        {
            // Debug.Log("called");
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

        public void Awake()
        {
            plant.isShort = true;
        }

        public FireSniper plant => gameObject.GetComponent<FireSniper>();
    }
}