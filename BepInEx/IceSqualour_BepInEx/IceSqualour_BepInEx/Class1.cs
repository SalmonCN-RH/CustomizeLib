using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace IceSqualourBepInEx
{
    [BepInPlugin("salmon.icesqualour", "IceSqualour", "1.0")]
    public class Core : BasePlugin//304
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<IceSqualour>();


            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "icesqualour");
            CustomCore.RegisterCustomPlant<Squalour, IceSqualour>(
                IceSqualour.PlantID,
                ab.GetAsset<GameObject>("IceSqualourPrefab"),
                ab.GetAsset<GameObject>("IceSqualourPreview"),
                new List<(int, int)>
                {
                ((int)PlantType.Squalour, (int)PlantType.IceShroom),
                ((int)PlantType.IceShroom, (int)PlantType.Squalour)
                },
                0f, 0f, 1800, 300, 0f, 200
            );
            CustomCore.RegisterCustomPlantSkin<Squalour, IceSqualour>(
                IceSqualour.PlantID,
                ab.GetAsset<GameObject>("IceSqualourSkinPrefab"),
                ab.GetAsset<GameObject>("IceSqualourSkinPreview"),
                new List<(int, int)>
                {
                ((int)PlantType.Squalour, (int)PlantType.IceShroom),
                ((int)PlantType.IceShroom, (int)PlantType.Squalour)
                },
                0f, 0f, 1800, 300, 0f, 200
            );
            CustomCore.AddPlantAlmanacStrings(IceSqualour.PlantID,
                $"寒冰猫瓜({IceSqualour.PlantID})",
                "蕴含寒冰力量的寒冰窝瓜，根据压到僵尸数量，生成窝瓜卡片及寒冰效果。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>1800</color>\n<color=#3D1400>特点：</color><color=red>拥有窝瓜的特点，压到1个僵尸时，生成1张窝瓜卡片及3x3范围内造成900伤害并冻结；压到两个僵尸时，生成2张窝瓜卡片及全屏造成900伤害并冻结；至少压到3个僵尸时，掉落3张窝瓜卡片和一张寒冰窝瓜卡片，同时对全屏僵尸造成1800伤害并冻结。对冻结的僵尸造成4倍伤害。</color>\n<color=#3D1400>融合配方：</color><color=red>猫瓜+寒冰菇</color>\n\n<color=#3D1400>想问她为何在雪原一带？获许是想如何学习极寒冰豆的奇妙魔法，或是讲冷笑话的诀窍。不过有一点需要注意；“不许叫我超威蓝猫！”寒冰猫瓜说。</color>"
            );
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)IceSqualour.PlantID);
        }
    }


    public class IceSqualour : MonoBehaviour
    {
        public static int PlantID = 1914;

        public Squalour plant => gameObject.GetComponent<Squalour>();
    }

    [HarmonyLib.HarmonyPatch(typeof(Squalour), nameof(Squalour.LourDie))]
    public class Squalour_LourDie_Patch
    {
        [HarmonyLib.HarmonyPrefix]
        public static bool Prefix(Squalour __instance)
        {
            if ((int)__instance.thePlantType == IceSqualour.PlantID)
            {
                // 获取位置并调整
                Vector3 position = __instance.axis.position;
                Vector2 adjustedPos = new Vector2(position.x, position.y - 0.5f);

                // 根据状态生成不同的掉落卡牌
                if (__instance.squashed)
                {

                    if (__instance.squashCount >= 3)
                    {
                        Lawnf.SetDroppedCard(adjustedPos, PlantType.IceSquash, 0);
                    }

                    int loopCount = Mathf.Min(__instance.squashCount, 3);
                    for (int i = 0; i < loopCount; i++)
                    {
                        Lawnf.SetDroppedCard(adjustedPos, PlantType.Squash, 0);
                    }
                }
                else
                {
                    Lawnf.SetDroppedCard(adjustedPos, (PlantType)IceSqualour.PlantID, 0);
                }

                // 调用死亡方法
                __instance.Die();
                return false;
            }
            return true;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Squash), nameof(Squash.AttackZombie))]
    public class Squash_AttackZombie_Patch
    {
        [HarmonyLib.HarmonyPrefix]
        public static bool Prefix(Squash __instance)
        {
            if ((int)__instance.thePlantType == IceSqualour.PlantID)
            {
                Vector3 position = __instance.axis.position;
                Vector2 centerPos = new Vector2(position.x, position.y);

                // 检测区域内的僵尸
                int zombieLayer = LayerMask.GetMask("Zombie");
                Collider2D[] colliders = Physics2D.OverlapBoxAll(
                    centerPos,
                    new Vector2(1f, 3f),
                    0f,
                    zombieLayer
                );

                if (colliders != null)
                {
                    foreach (var collider in colliders)
                    {
                        Zombie zombie;
                        if (collider.TryGetComponent<Zombie>(out zombie))
                        {
                            // 尝试攻击僵尸
                            bool attacked = __instance.AttackLandZombie(zombie);

                            // 检查是否符合特殊攻击条件
                            if (attacked && (zombie.theZombieRow == __instance.thePlantRow || zombie.theZombieType == ZombieType.DancePolZombie2))
                            {
                                __instance.ActionOnZombie(zombie);
                            }
                        }
                    }
                }

                // 检查当前位置的格子类型
                int col = Lawnf.GetColumnFromX(position.x);
                int row = __instance.thePlantRow;

                Collider2D[] zombies = Physics2D.OverlapCircleAll(__instance.axis.transform.position, 3f, LayerMask.GetMask("Zombie"));
                if (__instance.GetComponent<IceSqualour>().plant.squashCount == 1)
                {
                    foreach (Collider2D collider in zombies)
                    {
                        if (collider != null && collider.TryGetComponent<Zombie>(out var zombie) && zombie != null && zombie.axis != null)
                        {
                            int zombieCol = Mouse.Instance.GetColumnFromX(zombie.axis.transform.position.x);
                            int zombieRow = Mouse.Instance.GetRowFromY(zombie.axis.transform.position.x, zombie.axis.transform.position.y);
                            if ((zombieCol == col || zombieCol == col - 1 || zombieCol == col + 1) && (zombieRow == row || zombieRow == row + 1 || zombieRow == row - 1))
                            {
                                int damage = 900;
                                if (zombie.freezeTimer > 0f)
                                    damage *= 4;
                                zombie.TakeDamage(DmgType.IceAll, damage, false);
                                zombie.SetFreeze(4f);
                                zombie.SetCold(10f);
                            }
                        }
                    }
                    Vector3 particlePos = new Vector3(
                        position.x,
                        position.y - 0.7f,
                        0
                    );

                    // 创建冰霜粒子效果
                    CreateParticle.SetParticle(97, particlePos, __instance.thePlantRow, true);
                }
                else if (__instance.GetComponent<IceSqualour>().plant.squashCount > 1)
                {
                    Board board = GameAPP.board.GetComponent<Board>();
                    if (__instance.GetComponent<IceSqualour>().plant.squashCount == 2)
                    {
                        if (board != null)
                            foreach (Zombie zombie in board.zombieArray)
                            {
                                if (zombie != null)
                                {
                                    int damage = 900;
                                    if (zombie.freezeTimer > 0f)
                                        damage *= 4;
                                    zombie.TakeDamage(DmgType.IceAll, damage, false);
                                    zombie.SetFreeze(4f);
                                    zombie.SetCold(10f);
                                }
                            }
                    }
                    else
                    {
                        if (board != null)
                            foreach (Zombie zombie in board.zombieArray)
                            {
                                if (zombie != null)
                                {
                                    int damage = 1800;
                                    if (zombie.freezeTimer > 0f)
                                        damage *= 4;
                                    zombie.TakeDamage(DmgType.IceAll, damage, false);
                                    zombie.SetFreeze(4f);
                                    zombie.SetCold(10f);
                                }
                            }
                    }
                    Vector3 particlePos = new Vector3(
                        position.x,
                        position.y - 0.7f,
                        0
                    );

                    // 创建冰霜粒子效果
                    CreateParticle.SetParticle(97, particlePos, __instance.thePlantRow, true);
                }

                if (__instance.board.GetBoxType(col, row) != BoxType.Water)
                {
                    GameAPP.PlaySound(74, 0.5f, 1f);
                    ScreenShake.shakeDuration = 0.05f;
                }
                else // 正常格子
                {
                    // 创建特效
                    GameObject effectPrefab = Resources.Load<GameObject>("Particle/Anim/Water/WaterSplashPrefab");
                    Vector3 spawnPos = new Vector3(position.x, position.y - 1.75f, 0f);
                    Transform boardTransform = __instance.board.transform;
                    GameObject.Instantiate(effectPrefab, spawnPos, Quaternion.identity, boardTransform);

                    GameAPP.PlaySound(75, 0.5f, 1f);
                    __instance.Die();
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(SquashPumpkin), nameof(SquashPumpkin.AttributeEvent))]
    public class SquashPumpkinAttributeEventPatch
    {
        [HarmonyLib.HarmonyPrefix]
        public static bool Prefix(SquashPumpkin __instance)
        {
            List<Plant> plants = Lawnf.Get1x1Plants(__instance.thePlantColumn, __instance.thePlantRow).ToArray().ToList();
            bool find = false;
            Plant squalour = null;
            foreach (Plant plant in plants)
                if (plant.TryGetComponent<IceSqualour>(out var component))
                {
                    find = true;
                    squalour = plant;
                }
            if (find)
            {
                __instance.attributeCountdown = 3f;

                if (__instance.littleSquash == null)
                {
                    // 验证种植位置是否有效
                    BoxType gridType = __instance.board.GetBoxType(__instance.thePlantRow, __instance.thePlantColumn);
                    if (gridType != BoxType.Water) // 非水路格子
                    {
                        // 实例化预制体
                        GameObject instance = UnityEngine.Object.Instantiate(
                            __instance.squashPrefab_lour,
                            __instance.axis.position,
                            Quaternion.identity,
                            __instance.board.transform
                        );

                        // 获取LittleSquash组件
                        __instance.littleSquash = instance.GetComponent<LittleSquash>();

                        // 设置伤害值
                        if (__instance.littleSquash != null)
                        {
                            __instance.littleSquash.theDamage = squalour.attackDamage / 8;
                        }

                        // 设置小型Squash的位置
                        if (__instance.littleSquash != null)
                        {
                            __instance.littleSquash.theRow = __instance.thePlantRow;
                            Vector3 position = __instance.axis.position;

                            // 播放特效和音效
                            CreateParticle.SetParticle(1, position, __instance.thePlantRow, true);
                            GameAPP.PlaySound(22, 0.5f, 1.0f);
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }
}