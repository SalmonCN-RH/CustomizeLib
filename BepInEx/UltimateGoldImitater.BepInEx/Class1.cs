using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UltimateGoldImitater.BepInEx.UltimateGoldImitater;
using Core;

namespace UltimateGoldImitater.BepInEx
{
    [BepInPlugin("salmon.ultimategoldimitater", "UltimateGoldImitater", "1.0")]
    public class Core : CorePlugin
    {
        public override void OnStart()
        {
            var ab = CustomCore.GetAssetBundle(Tools.GetAssembly(), "ultimategoldimitater");
            CustomCore.RegisterCustomPlant<Imitater, UltimateGoldImitater>(UltimateGoldImitater.PlantID, ab.GetAsset<GameObject>("UltimateGoldImitaterPrefab"),
                ab.GetAsset<GameObject>("UltimateGoldImitaterPreview"), new List<(PlantType, PlantType)> { }.ToIntegerList(), 0f, 0f, 0, 300, 15f, 50);
            CustomCore.AddPlantAlmanacStrings(UltimateGoldImitater.PlantID, $"究极黄金模仿者",
                $"孤注一掷，遍历死地而后生！\n" +
                $"<color=blue>黄金模仿者的限定形态</color>\n\n" +
                $"<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                $"<color=#3D1400>特点：</color><color=red>①不可被铲除\n" +
                $"②不会被僵尸索敌\n" +
                $"③出场1.5秒后，生成随机植物，僵尸和各种产出，概率均为随机值\n" +
                $"④使用钻石模仿者，黄金模仿者或究极黄金模仿者时，重置此次各类事件的概率\n" +
                $"⑤使用僵尸模仿者时，消耗700阳光，重置此次各类事件的概率，并使随机两个事件的概率大幅提高\n" +
                $"⑥使用各类模仿者重置五次时，使随机一个事件的概率提升至100%，此时不能再通过使用模仿者使其重置概率</color>\n" +
                $"<color=#3D1400>概率明细：</color>\n" +
                $"<color=red>各类普通植物（？%）\n" +
                $"究极植物（？%）\n" +
                $"各类普通僵尸（？%）\n" +
                $"究极僵尸（？%）\n" +
                $"领袖和Boss僵尸（？%）\n" +
                $"其他事件（？%）</color>\n" +
                $"<color=#3D1400>事件明细：</color><color=red>①生成十张卡片：究极黄金模仿者（？%）；黄金模仿者（？%）；钻石模仿者（？%）\n" +
                $"②获得一个词条：植物词条（？%）；僵尸词条（？%）\n" +
                $"③获得？阳光</color>\n" +
                $"<color=#3D1400>特殊强化：</color><color=red>①<Boss>僵王博士：血量x？，免疫寒冷，免疫冻结\n" +
                $"②<Boss>黄金僵王博士：血量x？，免疫寒冷，免疫冻结\n" +
                $"③<Boss>黑橄榄大帅：血量x？\n" +
                $"④<Boss>黑橄榄将军：血量x？</color>\n" +
                $"<color=#3D1400>词条1:</color><color=red>孤注一掷：黄金模仿者和究极黄金模仿者随机究极的概率大幅提升</color>\n\n" +
                $"<color=#3D1400>“欲戴其冠，必承其重”\n" +
                $"那枚头冠，从出生起，就戴在他的头上，人们都说他是天选，这是他的宿命。在他很小的时候，他的父母带他到那尊巨大面前，幼小的他看着雕像上巨大的头冠，在对比自己的，自己的头冠更像是一枚精巧的戒指，落在他的小脑袋上，他不懂那意味着什么，只是指着头冠“像～”又指了指雕像。\n" +
                $"那尊巨大的雕像，曾是带来希望和财富的象征，再有象征性的事物，在经过历史的长河时，总会丢失些什么，而这座雕像丢失的，这枚头冠丢失的，正是希望。人们指望这个带着头冠小孩儿为他们创造财富，日子一天一天过去，孩子一天一天长大，人们从期待逐渐变得怀疑，直到最后变得愤怒！“那个孩子，他不能带给我们财富，那就是祸害！我们会变成这样，我们变得平庸，我们没有富贵，都是因为他！抓住他！把他烧了！”门口的人越聚越多，就像是挤在蜂巢的蜜蜂……\n" +
                $"“后来呢。”\n" +
                $"“后来，我逃出来了，我一个人逃出来了。”</color>\n\n" +
                $"<color=#955300>花费：</color><color=red>50</color>\n" +
                $"<color=#955300>冷却：</color><color=red>15秒</color>");
            CustomCore.RegisterCustomClickCardOnPlantEvent(UltimateGoldImitater.PlantID, PlantType.DiamondImitater,
                (p) => p.GetComponent<UltimateGoldImitater>().FeedPlant(PlantType.DiamondImitater), (p) => !p.GetComponent<UltimateGoldImitater>().isLock,
                new CustomClickCardOnPlant()
                {
                    Trigger = CustomClickCardOnPlant.TriggerType.CardOnly
                });
            CustomCore.RegisterCustomClickCardOnPlantEvent(UltimateGoldImitater.PlantID, (PlantType)1931,
                (p) => p.GetComponent<UltimateGoldImitater>().FeedPlant((PlantType)1931), (p) => !p.GetComponent<UltimateGoldImitater>().isLock,
                new CustomClickCardOnPlant()
                {
                    Trigger = CustomClickCardOnPlant.TriggerType.CardOnly
                });
            CustomCore.RegisterCustomClickCardOnPlantEvent(UltimateGoldImitater.PlantID, UltimateGoldImitater.PlantID,
                (p) => p.GetComponent<UltimateGoldImitater>().FeedPlant(UltimateGoldImitater.PlantID), (p) => !p.GetComponent<UltimateGoldImitater>().isLock,
                new CustomClickCardOnPlant()
                {
                    Trigger = CustomClickCardOnPlant.TriggerType.CardOnly
                });
            CustomCore.RegisterCustomClickCardOnPlantEvent(UltimateGoldImitater.PlantID, (PlantType)1960,
                (p) =>
                {
                    p.GetComponent<UltimateGoldImitater>().FeedPlant((PlantType)1960);
                    Board.Instance.UseSun(700f);
                }, (p) =>
                {
                    if (Board.Instance.theSun < 700)
                        InGameText.Instance.ShowText("需要消耗700阳光", 3f);
                    return !p.GetComponent<UltimateGoldImitater>().isLock && Board.Instance.theSun >= 700;
                },
                new CustomClickCardOnPlant()
                {
                    Trigger = CustomClickCardOnPlant.TriggerType.CardOnly
                });
            CustomCore.RegisterCustomCardToColorfulCards(PlantID, 14);
        }

        public override void OnGameInit()
        {
            foreach (var pt in GameAPP.resourcesManager.allPlants)
            {
                if (pt == PlantType.VectorPlant) continue;
                if (Lawnf.IsUltiPlant(pt)) UltimateGoldImitater.UltiPlants.Add(pt);
                else UltimateGoldImitater.NormalPlants.Add(pt);
            }
            foreach (var zt in GameAPP.resourcesManager.allZombieTypes)
            {
                if (zt == ZombieType.TrainingDummy)
                    continue;
                if (TypeMgr.UltimateZombie(zt) && !TypeMgr.IsBossZombie(zt)) UltimateGoldImitater.UltiZombies.Add(zt);
                else if (TypeMgr.IsBossZombie(zt)) UltimateGoldImitater.BossZombies.Add(zt);
                else UltimateGoldImitater.NormalZombies.Add(zt);
            }
        }
    }

    public class UltimateGoldImitater : MonoBehaviour
    {
        public static ID PlantID = 1968;

        public const int BaseNoBuff = 5 + 5 + 5 + 5 + 1 + 1; // 无词条保底概率
        public static Dictionary<Probability, int> NoBuffProbability = new Dictionary<Probability, int>()
    {
        { Probability.NormalPlant, 5 },
        { Probability.UltimatePlant, 5 },
        { Probability.NormalZombie, 5 },
        { Probability.UltimateZombie, 5 },
        { Probability.Boss, 5 },
        { Probability.Event, 5 }
    };
        public const int BaseBuff = 2 + 15 + 2 + 15 + 3 + 1; // 有词条保底概率
        public static Dictionary<Probability, int> BuffProbability = new Dictionary<Probability, int>()
    {
        { Probability.NormalPlant, 2 },
        { Probability.UltimatePlant, 15 },
        { Probability.NormalZombie, 2 },
        { Probability.UltimateZombie, 15 },
        { Probability.Boss, 3 },
        { Probability.Event, 1 }
    };
        public static List<PlantType> NormalPlants = new();
        public static List<PlantType> UltiPlants = new();
        public static List<ZombieType> NormalZombies = new();
        public static List<ZombieType> UltiZombies = new();
        public static List<ZombieType> BossZombies = new();
        public static BuffID buff = -1;

        public bool isLock = false;
        public Probability lockedProbability = Probability.NormalPlant;

        // 使用数组存储概率，索引对应枚举值
        public int[] ActualProbabilities = new int[6]; // 存储实际概率值，总和应为100
        public int[] CumulativeProbabilities = new int[6]; // 存储累积概率值

        public void Awake()
        {
            GenProbability();
            foreach (var collider in gameObject.GetComponents<BoxCollider2D>())
                collider.contactCaptureLayers = 0;
        }

        public void Start()
        {
            if (plant == null) return;
            int total = 0;
            var config = GameAPP.config;
            if (config.levelZombieInRandom) total += 2;
            if (config.strongUltiZombieInRandom) total += 2;
            if (config.leaderInRandom) total += 6;
            if (GameAPP.theGameStatus == GameStatus.InGame && plant != null && UnityEngine.Random.Range(1, 101) <= total && plant.board != null && (plant.board.boardTag.isSuperRandom || plant.board.boardTag.isIZ))
            {
                plant.StarUp();
                plant.starUp = true;
                plant.UpdateStarIcon();
            }
        }

        public void Update()
        {
            if (plant == null) return;
            plant.TryBeActive();
        }

        public void AnimSpawn()
        {
            var random = UnityEngine.Random.Range(1, 101);
            var type = Probability.Event;

            // 使用累积概率数组进行判断
            for (int i = 0; i < 6; i++)
            {
                if (random <= CumulativeProbabilities[i])
                {
                    type = (Probability)i;
                    break;
                }
            }

            switch (type)
            {
                case Probability.NormalPlant:
                case Probability.UltimatePlant:
                    SetPlant(type);
                    break;
                case Probability.NormalZombie:
                case Probability.UltimateZombie:
                case Probability.Boss:
                    SetZombie(type);
                    break;
                case Probability.Event:
                    GetEvent();
                    break;
            }
            CreateParticle.SetParticle((int)ParticleType.RandomCloud, plant.axis.position + new Vector3(0f, 0.5f, 0f), 11);
            plant.Die(Plant.DieReason.BySelf);
        }

        public void SetPlant(Probability probability)
        {
            Plant? p = null;
            switch (probability)
            {
                case Probability.NormalPlant:
                    {
                        var pt = NormalPlants[UnityEngine.Random.Range(0, NormalPlants.Count)];
                        for (int i = 0; i < (TypeMgr.IsPuff(pt) ? 3 : 1); i++)
                            p = CreatePlant.Instance.SetPlant(plant.thePlantColumn, plant.thePlantRow, pt, isFreeSet: true);
                    }
                    break;
                case Probability.UltimatePlant:
                    {
                        var pt = UltiPlants[UnityEngine.Random.Range(0, UltiPlants.Count)];
                        for (int i = 0; i < (TypeMgr.IsPuff(pt) ? 3 : 1); i++)
                            p = CreatePlant.Instance.SetPlant(plant.thePlantColumn, plant.thePlantRow, pt, isFreeSet: true);
                    }
                    break;
            }
            if (p != null && StarUp() && UnityEngine.Random.Range(0, 10) == 0 && p.OnStarUp())
            {
                p.starUp = true;
                p.StarUp();
                p.UpdateStarIcon();
            }
            if (p != null)
                ReinforcePlant(p, 0.2f, 6.0f, 0.2f, 6.0f);
        }

        public void ReinforcePlant(Plant plant, float healthMin, float healthMax, float speedMin, float speedMax)
        {
            float healthMultiplier = UnityEngine.Random.Range(healthMin, healthMax);
            plant.ModifyHealth((PlantHealthAdder)5, healthMultiplier, false);
            plant.ModifyDamage((PlantDamageAdder)44, healthMultiplier, false, float.MaxValue.GetNullable());
            float speedMultiplier = UnityEngine.Random.Range(speedMin, speedMax);
            plant.attributeSpeed = speedMultiplier;
            plant.attackSpeedAdder = speedMultiplier - 1f;
            plant.thePlantSpeed = speedMultiplier * plant.thePlantSpeed;
            plant.thePlantProduceInterval = (1f / speedMultiplier) * plant.thePlantProduceInterval;
        }

        public void SetZombie(Probability probability)
        {
            Zombie? z = null;
            var zt = ZombieType.Nothing;
            switch (probability)
            {
                case Probability.NormalZombie:
                    {
                        zt = NormalZombies[UnityEngine.Random.Range(0, NormalZombies.Count)];
                    }
                    break;
                case Probability.UltimateZombie:
                    {
                        zt = UltiZombies[UnityEngine.Random.Range(0, UltiZombies.Count)];
                    }
                    break;
                case Probability.Boss:
                    {
                        zt = BossZombies[UnityEngine.Random.Range(0, BossZombies.Count)];
                    }
                    break;
            }
            var row = plant.thePlantRow;
            if (zt == ZombieType.ZombieBoss)
            {
                row = 0;
                GameAPP.Instance.PlayMusic(MusicType.Boss);
            }
            if (zt == ZombieType.ZombieBoss2)
            {
                row = 0;
                GameAPP.Instance.PlayMusic(MusicType.Boss2);
            }
            z = CreateZombie.Instance.SetZombie(row, zt, plant.axis.position.x).GetComponent<Zombie>();
            if (z == null) return;
            var multi = 1f;
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    multi = 0.8f;
                    break;
                case 1:
                    multi = 1.2f;
                    break;
            }
            z.theHealth = (int)(multi * z.theHealth);
            z.theMaxHealth = (int)(multi * z.theMaxHealth);
            z.theFirstArmorHealth = (int)(multi * z.theFirstArmorHealth);
            z.theFirstArmorMaxHealth = (int)(multi * z.theFirstArmorMaxHealth);
            z.theSecondArmorHealth = (int)(multi * z.theSecondArmorHealth);
            z.theSecondArmorMaxHealth = (int)(multi * z.theSecondArmorMaxHealth);
            switch (z.theZombieType)
            {
                case ZombieType.ZombieBoss:
                case ZombieType.ZombieBoss2:
                    {
                        var v = UnityEngine.Random.Range(10f, 500f);
                        if (plant.starUp)
                            v = UnityEngine.Random.Range(100f, 500f);
                        v = (float)Math.Round(v, 1, MidpointRounding.AwayFromZero);
                        z.theHealth = (int)(v * z.theHealth);
                        z.theMaxHealth = (int)(v * z.theMaxHealth);
                        z.SetData("UltimateGoldImitater_SpawnByGold", true);
                        if (plant.board.boardTag.isSuperRandom)
                        {
                            z.theHealth /= 10;
                            z.theMaxHealth /= 10;
                        }
                        z.AddComponent<ClearCold>().zombie = z;
                    }
                    break;
                case ZombieType.HorseBoss:
                case ZombieType.FootballBoss:
                    {
                        var v = UnityEngine.Random.Range(10f, 50f);
                        if (plant.starUp)
                            v = UnityEngine.Random.Range(25f, 100f);
                        v = (float)Math.Round(v, 1, MidpointRounding.AwayFromZero);
                        z.theHealth = (int)(v * z.theHealth);
                        z.theMaxHealth = (int)(v * z.theMaxHealth);
                    }
                    break;
            }

            if (z != null)
                ReinforceZombie(z, 0.1f, 10.0f, 0.3f, 4.0f, 0.3f, 2.5f);
        }

        public void ReinforceZombie(Zombie zombie, float healthMin, float healthMax, float speedMin, float speedMax, float scaleMin, float scaleMax)
        {
            float healthMultiplier = UnityEngine.Random.Range(healthMin, healthMax);
            Lawnf.SetZombieHealth(zombie, healthMultiplier);

            zombie.theAttackDamage = (int)(zombie.theAttackDamage * healthMultiplier);
            zombie.UpdateHealthText();

            if (zombie.theZombieType != ZombieType.FootballBoss)
            {
                float speedMultiplier = UnityEngine.Random.Range(speedMin, speedMax);
                zombie.theOriginSpeed = zombie.theOriginSpeed * speedMultiplier;
            }

            if (zombie.theZombieType != ZombieType.ZombieBoss && zombie.theZombieType != ZombieType.ZombieBoss2 && 
                zombie.theZombieType != ZombieType.FootballBoss)
            {
                float scaleMultiplier = UnityEngine.Random.Range(scaleMin, scaleMax);

                Vector3 position = zombie.axis.position;
                zombie.transform.localScale = zombie.transform.localScale * scaleMultiplier;
                // 调整位置以适应新的缩放
                zombie.AdjustPosition(position);
            }
        }

        public void GetEvent()
        {
            var eventid = UnityEngine.Random.Range(0, 3);
            switch (eventid)
            {
                case 0:
                    {
                        var pool = new List<PlantType>()
                    {
                        PlantType.DiamondImitater,
                        PlantID
                    };
                        if (GameAPP.resourcesManager.allPlants.Contains((PlantType)1931))
                            pool.Add((PlantType)1931);
                        for (int i = 0; i < UnityEngine.Random.Range(0, 100); i++)
                            pool.Add(pool[UnityEngine.Random.Range(0, pool.Count)]);
                        for (int i = 0; i < 10; i++)
                            Lawnf.SetDroppedCard(plant.axis.position + new Vector3(0f, 0.5f, 0f), pool[UnityEngine.Random.Range(0, pool.Count)]);
                    }
                    InGameText.Instance.ShowText("模仿十连抽！", 3f);
                    break;
                case 1:
                    {
                        var mgr = GameAPP.Instance.GetOrAddComponent<TravelMgr>();
                        int value = UnityEngine.Random.Range(1, 101);
                        var data = mgr.data;
                        if (value <= UnityEngine.Random.Range(0, 101))
                        {
                            switch (UnityEngine.Random.Range(0, 2))
                            {
                                case 0:
                                    {
                                        var list = new List<AdvBuff>();
                                        foreach (var (id, _) in TravelDictionary.advancedBuffsText)
                                            if (!data.advBuffs.Contains(id))
                                                list.Add(id);
                                        var advBuff = list[UnityEngine.Random.Range(0, list.Count)];
                                        TravelMgr.Instance.GetNormalBuff(advBuff);
                                        InGameText.Instance.ShowText($"抽到普通词条：{TravelDictionary.advancedBuffsText[advBuff]}", 5f);
                                    }
                                    break;
                                case 1:
                                    {
                                        var list = new List<UltiBuff>();
                                        foreach (var (id, _) in TravelDictionary.ultimateBuffsText)
                                            if (!data.ultiBuffs.Contains(id) && !data.ultiBuffs.Contains(id))
                                                list.Add(id);
                                        var ultiBuff = list[UnityEngine.Random.Range(0, list.Count)];
                                        TravelMgr.Instance.GetUltiBuff(ultiBuff);
                                        InGameText.Instance.ShowText($"抽到强究词条：{TravelDictionary.ultimateBuffsText[ultiBuff]}", 5f);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            var list = new List<TravelDebuff>();
                            foreach (var kvp in TravelDictionary.debuffData)
                            {
                                if (!data.travelDebuffs.Contains(kvp.Key))
                                    list.Add(kvp.Key);
                            }
                            var debuff = list[UnityEngine.Random.Range(0, list.Count)];
                            TravelMgr.Instance.GetDebuff(debuff);
                            InGameText.Instance.ShowText($"抽到僵尸词条：{TravelDictionary.debuffData[debuff].Item1}", 5f);
                        }
                    }
                    break;
                case 2:
                    {
                        for (int i = 0; i < UnityEngine.Random.Range(0, 4000); i++)
                        {
                            CreateItem.Instance.SetCoin(0, 0, (int)ItemType.NormalSun, 0, plant.axis.position + new Vector3(0f, 0.5f, 0f));
                        }
                        InGameText.Instance.ShowText("大量阳光！", 3f);
                    }
                    break;
            }
        }

        public void FeedPlant(PlantType pt)
        {
            ParticleManager.Instance.SetParticle(ParticleType.PointsSplat, plant.axis.position + new Vector3(0f, 0.5f, 0f), plant.thePlantRow);
            GameAPP.PlaySound(125, 0.5f, 1.0f);
            var newPlant = CreatePlant.Instance.SetPlant(plant.thePlantColumn, plant.thePlantRow, PlantID, isFreeSet: true).GetComponent<UltimateGoldImitater>();
            var arr = Enum.GetValues<Probability>();
            var prob = arr[UnityEngine.Random.Range(0, arr.Length)];
            if (plant.attributeCount >= 4)
            {
                newPlant.isLock = true;
                newPlant.lockedProbability = prob;
                newPlant.GenProbability();
                plant.Die();
                return;
            }
            newPlant.plant.attributeCount = plant.attributeCount + 1;
            plant.Die();
            // pt == 1960 的处理逻辑
            if (pt == (PlantType)1960)
            {
                // 获取所有概率类型
                var allProbabilities = Enum.GetValues<Probability>();

                // 随机选择两个不同的概率类型
                var selectedProbabilities = new List<Probability>();
                var tempList = allProbabilities.Cast<Probability>().ToList();
                for (int i = 0; i < 2 && tempList.Count > 0; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, tempList.Count);
                    selectedProbabilities.Add(tempList[randomIndex]);
                    tempList.RemoveAt(randomIndex);
                }

                // 确定是否有buff
                var hasBuff = Lawnf.TravelAdvanced(UltimateGoldImitater.buff) || plant.starUp;

                // 设置两个选中的概率为38%以上（随机38-100%）
                float targetPercentage1 = UnityEngine.Random.Range(38f, 100f);
                float targetPercentage2 = UnityEngine.Random.Range(38f, 100f);

                // 确保总概率不超过100%
                float totalTargetPercentage = targetPercentage1 + targetPercentage2;
                if (totalTargetPercentage > 100f)
                {
                    targetPercentage1 = 38f + (targetPercentage1 - 38f) * (62f / (totalTargetPercentage - 76f));
                    targetPercentage2 = 38f + (targetPercentage2 - 38f) * (62f / (totalTargetPercentage - 76f));
                    totalTargetPercentage = targetPercentage1 + targetPercentage2;
                }

                // 计算剩余概率
                float remainingPercentage = 100f - totalTargetPercentage;

                // 获取保底概率
                var remainingProbabilities = allProbabilities.Cast<Probability>().Where(p => !selectedProbabilities.Contains(p)).ToList();

                // 计算保底概率总和
                float totalBaseProbability = 0f;
                foreach (var probType in remainingProbabilities)
                {
                    float baseProb = hasBuff ? BuffProbability[probType] : NoBuffProbability[probType];
                    totalBaseProbability += baseProb;
                }

                // 如果剩余概率不足以满足保底，调整选中的概率
                if (remainingPercentage < totalBaseProbability)
                {
                    float needed = totalBaseProbability - remainingPercentage;
                    float reduce1 = Mathf.Min(targetPercentage1 - 38f, needed * (targetPercentage1 - 38f) / (totalTargetPercentage - 76f));
                    float reduce2 = needed - reduce1;

                    targetPercentage1 -= reduce1;
                    targetPercentage2 -= reduce2;
                    remainingPercentage = totalBaseProbability;
                }

                // 初始化实际概率数组
                for (int i = 0; i < 6; i++)
                {
                    newPlant.ActualProbabilities[i] = 0;
                }

                // 设置选中的两个概率
                newPlant.ActualProbabilities[(int)selectedProbabilities[0]] = Mathf.RoundToInt(targetPercentage1);
                newPlant.ActualProbabilities[(int)selectedProbabilities[1]] = Mathf.RoundToInt(targetPercentage2);

                // 分配剩余概率，确保不低于保底
                float remainingSum = remainingPercentage;
                foreach (var probType in remainingProbabilities)
                {
                    float baseProb = hasBuff ? BuffProbability[probType] : NoBuffProbability[probType];
                    float allocated = baseProb + (remainingSum - totalBaseProbability) * (baseProb / totalBaseProbability);
                    newPlant.ActualProbabilities[(int)probType] = Mathf.RoundToInt(allocated);
                }

                // 修正总和为100（处理四舍五入误差）
                int sum = newPlant.ActualProbabilities.Sum();
                if (sum != 100)
                {
                    int diff = 100 - sum;
                    newPlant.ActualProbabilities[5] += diff; // 调整最后一个
                }

                // 计算累积概率
                int cumulative = 0;
                for (int i = 0; i < 6; i++)
                {
                    cumulative += newPlant.ActualProbabilities[i];
                    newPlant.CumulativeProbabilities[i] = cumulative;
                }
            }
        }

        public bool StarUp() => plant.starUp;

        public void GenProbability()
        {
            if (isLock)
            {
                // 锁定模式：只有一个概率为100，其他为0
                for (int i = 0; i < 6; i++)
                {
                    if (i == (int)lockedProbability)
                    {
                        ActualProbabilities[i] = 100;
                        CumulativeProbabilities[i] = 100;
                    }
                    else
                    {
                        ActualProbabilities[i] = 0;
                        CumulativeProbabilities[i] = 0;
                    }
                }
                return;
            }

            // 正常模式：生成随机概率
            var hasBuff = Lawnf.TravelAdvanced(UltimateGoldImitater.buff) || plant.starUp;
            int remainingTotal = 100;

            // 生成前5项的实际概率值
            for (int i = 0; i < 5; i++)
            {
                var value = (Probability)i;
                int basep = hasBuff ? BuffProbability[value] : NoBuffProbability[value];

                // 计算剩余项的最低保底总和
                int remainingMinTotal = 0;
                for (int j = i + 1; j < 6; j++)
                {
                    var nextType = (Probability)j;
                    remainingMinTotal += hasBuff ? BuffProbability[nextType] : NoBuffProbability[nextType];
                }

                // 可分配的最大值 = 剩余总量 - 剩余项最低保底
                int maxValue = remainingTotal - remainingMinTotal;
                if (maxValue < basep) maxValue = basep;

                // 随机生成当前项的实际概率（在保底和最大值之间）
                ActualProbabilities[i] = UnityEngine.Random.Range(basep, maxValue + 1);
                remainingTotal -= ActualProbabilities[i];
            }

            // 最后一项取剩余值
            ActualProbabilities[5] = remainingTotal;

            // 计算累积概率
            int cumulative = 0;
            for (int i = 0; i < 6; i++)
            {
                cumulative += ActualProbabilities[i];
                CumulativeProbabilities[i] = cumulative;
            }

            // 确保最后一个累积概率为100（处理浮点误差）
            CumulativeProbabilities[5] = 100;
        }

        public enum Probability
        {
            NormalPlant,
            UltimatePlant,
            NormalZombie,
            UltimateZombie,
            Boss,
            Event
        }

        public Imitater plant => gameObject.GetComponent<Imitater>();
    }

    [HarmonyPatch(typeof(GameAPP), nameof(GameAPP.Start))]
    public static class GameAPPPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GameAPP __instance)
        {
            if (__instance.GetData<bool>("GoldImitater_set"))
                UltimateGoldImitater.buff = __instance.GetData<BuffID>("GoldImitater_buffID");
        }
    }

    [HarmonyPatch(typeof(ZombieBoss))]
    public static class ZombieBossStartPatch
    {
        [HarmonyPatch(nameof(ZombieBoss.Start))]
        [HarmonyPostfix]
        public static void Postfix(ZombieBoss __instance)
        {
            {
                var position = __instance.axis.transform.position;
                position.y -= Lawnf.GetAllZombies().ToSystemList().Where(z => z.theZombieType == ZombieType.ZombieBoss || z.theZombieType == ZombieType.ZombieBoss2)
                    .ToList().Count * 0.4f;
                __instance.healthText.transform.position = position;
            }
        }

        //[HarmonyPatch(nameof(ZombieBoss.GetDamage))]
        //[HarmonyPostfix]
        //public static void PostGetDamage(ZombieBoss __instance, ref int __result)
        //{
        //    if (__instance.GetData<bool>("UltimateGoldImitater_SpawnByGold"))
        //    {
        //        __result = Mathf.Min(__result, 5000);
        //    }
        //}
    }

    public class ClearCold : MonoBehaviour
    {
        public Zombie zombie;

        public void Update()
        {
            if (zombie != null && !zombie.IsDestroyed())
            {
                zombie.freezeLevel = 0;
                if (zombie.TryGetEffect<ColdEffect>(EffectType.Cold, out var cold) && cold.duration > 0f)
                {
                    cold.duration = 0f;
                }
                if (zombie.TryGetEffect<FreezeEffect>(EffectType.Freeze, out var freeze) && freeze.duration > 0f)
                {
                    freeze.duration = 0f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Die))]
    public static class PlantPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Plant __instance, ref Plant.DieReason reason)
        {
            if (__instance.thePlantType == UltimateGoldImitater.PlantID && reason == Plant.DieReason.ByShovel)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlantDataMenu))]
    public static class PlantDataMenuPatch
    {
        [HarmonyPatch(nameof(PlantDataMenu.Start))]
        [HarmonyPostfix]
        public static void PostStart(PlantDataMenu __instance)
        {
            if (__instance != null && __instance.gameObject != null && !__instance.IsDestroyed() && !__instance.gameObject.IsDestroyed() &&
                __instance.plant != null && __instance.plant.gameObject != null && !__instance.plant.IsDestroyed() && !__instance.plant.gameObject.IsDestroyed()
                && __instance.plant.thePlantType == UltimateGoldImitater.PlantID)
            {
                var plant = __instance.plant.GetComponent<UltimateGoldImitater>();
                var str = $"各类普通植物：{plant.ActualProbabilities[0]}%\n" +
                    $"究极植物：{plant.ActualProbabilities[1]}%\n" +
                    $"各类普通僵尸：{plant.ActualProbabilities[2]}%\n" +
                    $"究极僵尸：{plant.ActualProbabilities[3]}%\n" +
                    $"领袖和boss：{plant.ActualProbabilities[4]}%\n" +
                    $"随机事件：{plant.ActualProbabilities[5]}%";
                foreach (var text in __instance.infoText)
                    text.text += str;
            }
        }
    }
}
