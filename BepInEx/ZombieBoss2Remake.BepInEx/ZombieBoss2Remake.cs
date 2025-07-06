using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static ZombieBoss;

namespace ZombieBoss2Remake.BepInEx
{
    public class ZombieBoss2Remake : MonoBehaviour
    {
        public ZombieBoss2Remake() : base(ClassInjector.DerivedConstructorPointer<ZombieBoss2Remake>()) => ClassInjector.DerivedConstructorBody(this);

        public ZombieBoss2Remake(IntPtr i) : base(i)
        {
        }

        [HideFromIl2Cpp]
        public static int GetRandomFalseIndex(IList<bool> list)
        {
            List<int> falseIndices = [];
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i])
                {
                    falseIndices.Add(i);
                }
            }

            if (falseIndices.Count is 0)
            {
                return -1;
            }

            int randomIndex = UnityEngine.Random.RandomRangeInt(0, falseIndices.Count);
            return falseIndices[randomIndex];
        }

        [HideFromIl2Cpp]
        public static int GetRandomFalseIndex(IList<int> list)
        {
            List<int> falseIndices = [];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is 0)
                {
                    falseIndices.Add(i);
                }
            }

            if (falseIndices.Count is 0)
            {
                return -1;
            }

            int randomIndex = UnityEngine.Random.RandomRangeInt(0, falseIndices.Count);
            return falseIndices[randomIndex];
        }

        public void AnimBlackHole()
        {
            if (BlackHole is not null)
            {
                var hole = Instantiate(BlackHole, gameObject.transform);
                hole.AddComponent<BlackHole>();
            }
        }

        public void AnimFreeze()
        {
            var ballPos = zombie.ballPosition.position;
            var effectPos = new Vector3(ballPos.x - 0.5f, ballPos.y + 0.1f, ballPos.z);
            CreateParticle.SetParticle(77, effectPos, zombie.targetRow, false);
            var snow = Instantiate(Core.Snow2, board.transform);

            var p = snow!.transform.position;
            p.y = 4.8f;
            snow.transform.position = p;
            HashSet<Vector2Int> boxes = [];
            // 填充所有网格位置
            for (int x = 0; x < board.columnNum; x++)
            {
                for (int y = 0; y < board.rowNum; y++)
                {
                    boxes.Add(new Vector2Int(x, y));
                }
            }

            // 移除不需要冻结的位置
            Remove(ref boxes);

            // 处理需要冻结的位置
            foreach (Vector2Int position in boxes)
            {
                bool createdFreezedPlant = false;

                foreach (Plant plant in Lawnf.Get1x1Plants(position.x, position.y))
                {
                    // 跳过特定类型的植物
                    if (plant.plantTag.flyingPlant ||
                        plant.plantTag.potPlant ||
                        plant.plantTag.pumpkinPlant ||
                        plant.plantTag.icePlant ||
                        plant.plantTag.firePlant ||
                        plant.plantTag.doubleBoxPlant ||
                        TypeMgr.IsFirePlant(plant.thePlantType) ||
                        TypeMgr.IsIcePlant(plant.thePlantType))
                    {
                        continue;
                    }

                    // 销毁植物并创建冻结效果
                    plant.Die(Plant.DieReason.ByFreeze);

                    if (!createdFreezedPlant)
                    {
                        FreezedPlant freezedPlant = GridItem.SetGridItem(
                            position.x, position.y,
                            GridItemType.IceBlock).Cast<FreezedPlant>();

                        freezedPlant.InitFreezedPlant(plant.thePlantType);
                        freezedPlant.savedData = new SavePlantData(plant);
                        freezedPlant.saved = true;

                        // 播放冻结特效
                        Vector3 effect = plant.transform.position;
                        effect.y += 0.5f;
                        CreateParticle.SetParticle(11, effect, plant.thePlantRow);
                        createdFreezedPlant = true;
                    }
                }
            }
        }

        public void AnimGatling()
        {
            var b1 = CreateBullet.Instance.SetBullet(GatlingShoot!.position.x, GatlingShoot.position.y, zombie.targetRow - 1, BulletType.Bullet_superCherry, BulletMoveWay.Three_up, true);
            b1.normalSpeed = -9;
            b1.Damage = 300;
            var b2 = CreateBullet.Instance.SetBullet(GatlingShoot!.position.x, GatlingShoot.position.y, zombie.targetRow, BulletType.Bullet_superCherry, BulletMoveWay.MoveRight, true);
            b2.Damage = 300;
            b2.normalSpeed = -9;
            var b3 = CreateBullet.Instance.SetBullet(GatlingShoot!.position.x, GatlingShoot.position.y, zombie.targetRow + 1, BulletType.Bullet_superCherry, BulletMoveWay.Three_down, true);
            b3.Damage = 300;
            b3.normalSpeed = -9;
        }

        public void AnimLeave()
        {
        }

        public void AnimPutBall()
        {
            // 播放音效
            GameAPP.PlaySound(121);

            // 获取球体位置并计算落点
            Vector3 ballPos = zombie.ballPosition.position;

            // 计算着陆Y坐标
            float landX = ballPos.x - 1.0f;
            float landY = Mouse.Instance.GetLandY(landX, zombie.targetRow);

            // 根据球类型生成不同粒子效果
            Vector3 effectPos;
            GameObject prefabToSpawn;

            if (zombie.ballType == 1)
            {
                effectPos = new Vector3(
                    ballPos.x - 0.5f,
                    ballPos.y + 0.1f,
                    ballPos.z
                );
                CreateParticle.SetParticle(77, effectPos, zombie.targetRow, false);
                prefabToSpawn = GameAPP.itemPrefab[33]; // 需要根据实际项目结构调整
            }
            else//冰
            {
                effectPos = new Vector3(
                    ballPos.x - 0.5f,
                    ballPos.y + 0.1f,
                    ballPos.z
                );
                CreateParticle.SetParticle(76, effectPos, zombie.targetRow, false);
                prefabToSpawn = GameAPP.itemPrefab[32]; // 需要根据实际项目结构调整
            }
            // 实例化球体对象
            Transform boardTransform = zombie.board.transform;
            Quaternion identity = Quaternion.identity;
            Vector3 spawnPos = new(landX, landY, 0);

            GameObject newBall = Instantiate(
                prefabToSpawn,
                spawnPos,
                identity,
                boardTransform
            );

            // 配置僵尸球组件
            ZombieBall zombieBall = newBall.GetComponent<ZombieBall>() ?? throw new NullReferenceException();
            zombieBall.theBallRow = zombie.targetRow;
            zombieBall.boss = true;
            zombieBall.StartBigger();

            // 配置排序组
            SortingGroup ballSortingGroup = newBall.GetComponent<SortingGroup>();
            if (zombie.sortingGroup != null && ballSortingGroup != null)
            {
                ballSortingGroup.sortingOrder = zombie.sortingGroup.sortingOrder + 1;
                ballSortingGroup.sortingLayerID = zombie.sortingGroup.sortingLayerID;
            }
            SummonMoney();
        }

        public void AnimSetHeadIdle()
        {
            zombie.bossStatus = BossStatus.head_idle;
        }

        public void AnimSetIdle()
        {
            zombie.bossStatus = BossStatus.idle;
        }

        public void AnimSlotMachine() => RefreshSlotMachine(Slots);

        public void AnimSlotMachine_1() => RefreshSlotMachine(Slots_1);

        public void AnimSlotMachineFinal()
        {
            int i = UnityEngine.Random.RandomRangeInt(0, 9);
            foreach (var sl in Slots)
            {
                foreach (var s in sl)
                {
                    s.active = false;
                }
                sl[i].active = true;
            }
            var r = Slots.GetRandomItem();
            foreach (var rr in r)
            {
                rr.active = false;
            }
            r.GetRandomItem().active = true;
        }

        public void AnimSlotMachineSummon()
        {
            CreateParticle.SetParticle((int)ParticleType.SilverCoinSplat, SlotMachine!.transform.position, 0);
            List<int> results = [];
            foreach (var sl in Slots)
            {
                for (int index = 0; index < sl.Count; index++)
                {
                    if (sl[index].active)
                    {
                        results.Add(index);
                        break;
                    }
                }
            }
            var query = results.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .Select(y => new { Element = y.Key, Counter = y.Count() })
              .ToList();
            ZombieType zty = ZombieType.Nothing;
            string text = "";
            if (query.Count > 0)
            {
                (zty, text) = query.First().Element switch
                {
                    0 => (ZombieType.UltimateGargantuar, "究极黑曜石巨人！"),
                    1 => (ZombieType.UltimateFootballZombie, "究极黑橄榄将军！"),
                    2 => (ZombieType.UltimatePaperZombie, "究极Z教授！"),
                    3 => (ZombieType.UltimateJacksonDriver, "究极舞台巡演车！"),
                    4 => (ZombieType.UltimateKirovZombie, "究极鱼型裂空机甲！"),
                    5 => (ZombieType.UltimateFootballDrown, "究极渊海三叉戟！"),
                    6 => (ZombieType.UltimateMachineNutZombie, "究极机械保龄球！"),
                    7 => (ZombieType.UltimateJackboxZombie, "究极玩偶匣皇后！"),
                    8 => (ZombieType.LegionZombie, "阿尔法僵尸小队！"),
                    _ => (ZombieType.Nothing, "什么也没有！")
                };
            }
            InGameText.Instance.ShowText(text, 5);
            if (zty is not ZombieType.Nothing || zty is not ZombieType.LegionZombie)
            {
                Instantiate(GameAPP.particlePrefab[11], CreateZombie.Instance.SetZombie(UnityEngine.Random.RandomRangeInt(0, board.rowNum), zty).transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity, zombie.board.transform);
            }
            for (int r = 0; r < board.rowNum; r++)
            {
                switch (zty)
                {
                    case ZombieType.UltimateGargantuar:
                        {
                            CreateZombie.Instance.SetZombie(r, ZombieType.SuperGargantuar);
                            break;
                        }
                    case ZombieType.UltimateFootballZombie:
                        {
                            CreateZombie.Instance.SetZombie(r, ZombieType.BlackFootball);
                            break;
                        }
                    case ZombieType.UltimatePaperZombie:
                        {
                            CreateZombie.Instance.SetZombie(r, ZombieType.CherryPaperZ95);
                            break;
                        }
                    case ZombieType.UltimateJacksonDriver:
                        {
                            CreateZombie.Instance.SetZombie(r, ZombieType.JacksonDriver);
                            break;
                        }
                    case ZombieType.UltimateKirovZombie:
                        {
                            CreateZombie.Instance.SetZombie(r, ZombieType.SuperKirov);
                            break;
                        }
                    case ZombieType.UltimateFootballDrown:
                        {
                            CreateZombie.Instance.SetZombie(r, ZombieType.FootballDrown);
                            break;
                        }
                    case ZombieType.UltimateMachineNutZombie:
                        {
                            CreateZombie.Instance.SetZombie(r, ZombieType.SuperMachineNutZombie);
                            break;
                        }
                    case ZombieType.UltimateJackboxZombie:
                        {
                            CreateZombie.Instance.SetZombie(r, ZombieType.JackboxJumpZombie);
                            break;
                        }
                    case ZombieType.LegionZombie:
                        {
                            Instantiate(GameAPP.particlePrefab[11], CreateZombie.Instance.SetZombie(r, zty).transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity, zombie.board.transform);
                            CreateZombie.Instance.SetZombie(r, zty);
                            CreateZombie.Instance.SetZombie(r, zty);
                            CreateZombie.Instance.SetZombie(r, zty);
                            CreateZombie.Instance.SetZombie(r, zty);
                            break;
                        }
                }
            }
        }

        public void AnimTakeAway()
        {
            Glove.Instance.gameObject.SetActive(false);
            HammerMgr.Instance.gameObject.SetActive(false);
        }

        public void AnimVase()
        {
            Instantiate(Core.VaseParticle, InnerHand)!.transform.localPosition = new(0, 0);
            var vases = Board.Instance.gameObject.GetComponentsInChildren<ScaryPot>();
            for (int i = vases.Count - 1; i >= 0; i--)
            {
                vases[i].OnHitted();
            }
            for (int c = board.columnNum - 4; c < board.columnNum; c++)
            {
                for (int r = 0; r < board.rowNum; r++)
                {
                    if (UnityEngine.Random.RandomRangeInt(0, 20) is 1)
                    {
                        var v = GridItem.SetGridItem(c, r, GridItemType.ScaryPot_plant);
                        v.transform.GetComponent<ScaryPot>().thePlantType = UltimatePlants.GetRandomItem();
                    }
                    else
                    {
                        var v = GridItem.SetGridItem(c, r, GridItemType.ScaryPot);
                        v.transform.GetComponent<ScaryPot>().theZombieType = UltimateZombies.GetRandomItem();
                    }
                    CreateParticle.SetParticle((int)ParticleType.RandomCloud, new(Mouse.Instance.GetBoxXFromColumn(c), Mouse.Instance.GetBoxYFromRow(r)), r);
                }
            }
            SummonMoney();
        }

        public void Awake()
        {
            zombie.spawnCount = 5;
            zombie.summonTime = 5.0f;
            zombie.ballCountDown = 3.0f;

            zombie.theStatus = ZombieStatus.Boss;
            zombie.bossStatus = BossStatus.enter;
            zombie.col.enabled = false;
        }

        public void BanGoldMelon()
        {
            foreach (var p in zombie.board.plantArray)
            {
                if (p is not null && p.thePlantType is PlantType.GoldMelon)
                {
                    p.Die();
                    SummonMoney(false);
                    InGameText.Instance.ShowText("你受到了金瓜的诅咒...", 3);
                }
            }
        }

        public void BodyTakeDamage(int theDamage)
        {
            if (theDamage > DamageLimit) theDamage = DamageLimit;
            if (ThirdStage)
            {
                if (AttackCountDown > 0) return;
                AttackCountDown = AttackInterval;
                zombie.theHealth -= theDamage;
                if (Glove.Instance is not null && HammerMgr.Instance is not null && (!Glove.Instance.gameObject.activeSelf || !HammerMgr.Instance.gameObject.activeSelf))
                {
                    InGameText.Instance.ShowText("锤子、手套已解锁", 3f);
                    Glove.Instance.gameObject.SetActive(true);
                    HammerMgr.Instance.gameObject.SetActive(true);
                }
                InGameUI.Instance.LevProgress.SetActive(false);
                return;
            }
            zombie.theHealth -= theDamage;
            if (zombie.theHealth >= zombie.theMaxHealth / 3f && zombie.theHealth <= 2f * zombie.theMaxHealth / 3f)
            {
                // 处理头部部件
                SetChildActive(zombie.head, 0, false);
                SetChildActive(zombie.head, 1, true);
                SetChildActive(zombie.head, 2, false);

                // 处理拇指部件
                ToggleComponent<Renderer>(zombie.thumb, false);
                SetChildActive(zombie.thumb, 0, true);
                SetChildActive(zombie.thumb, 1, false);

                // 处理下巴部件
                ToggleComponent<Renderer>(zombie.jaw, false);
                SetChildActive(zombie.jaw, 0, true);
                SetChildActive(zombie.jaw, 1, false);

                // 处理手部部件
                ToggleComponent<Renderer>(zombie.hand, false);
                SetChildActive(zombie.hand, 0, true);
                SetChildActive(zombie.hand, 1, false);

                // 处理脚部部件
                SetChildActive(zombie.foot_inner, 0, false);
                SetChildActive(zombie.foot_inner, 1, true);
            }
            else if (zombie.theHealth <= (float)(zombie.theMaxHealth / 3f) && (float)(zombie.theMaxHealth / 6f) < zombie.theHealth)
            {
                // 处理头部部件
                SetChildActive(zombie.head, 0, false);
                SetChildActive(zombie.head, 1, false);
                SetChildActive(zombie.head, 2, true);
                SetChildActive(zombie.head, 3, true);
                SetChildActive(zombie.head, 4, true);

                // 处理拇指部件
                ToggleComponent<Renderer>(zombie.thumb, false);
                SetChildActive(zombie.thumb, 0, false);
                SetChildActive(zombie.thumb, 1, true);

                // 处理下巴部件
                ToggleComponent<Renderer>(zombie.jaw, false);
                SetChildActive(zombie.jaw, 0, false);
                SetChildActive(zombie.jaw, 1, true);

                // 处理手部部件
                ToggleComponent<Renderer>(zombie.hand, false);
                SetChildActive(zombie.hand, 0, false);
                SetChildActive(zombie.hand, 1, true);

                // 处理外部脚部
                ToggleComponent<Renderer>(zombie.foot_outter, false);
                SetChildActive(zombie.foot_outter, 0, true);

                if (Glove.Instance is not null && HammerMgr.Instance is not null && (!Glove.Instance.gameObject.activeSelf || !HammerMgr.Instance.gameObject.activeSelf))
                {
                    InGameText.Instance.ShowText("锤子、手套已解锁", 3f);
                    Glove.Instance.gameObject.SetActive(true);
                    HammerMgr.Instance.gameObject.SetActive(true);
                }
            }

            return;
        }

        public void EnterThirdStage()
        {
            if (!ThirdStage)
            {
                ThirdStage = true;
                InGameUI.Instance.MoneyBank.active = true;
                Money.Instance.textMesh.color = Color.red;
                zombie.theMaxHealth = ThirdStageHealth;
                zombie.theHealth = ThirdStageHealth;
            }
        }

        public ZombieType GetZombieType()
        {
            // 创建候选僵尸类型列表
            int[] commonTypes = [9, 10, 15, 16, 18, 21, 30, 31, 33, 35, 36, 38, 39, 50, 51, 52, 108, 112, 110, 114, 118, 119];

            return !ThirdStage && UnityEngine.Random.RandomRangeInt(0, zombie.theMaxHealth) <= zombie.theHealth
                ? (ZombieType)commonTypes.GetRandomItem()
                : UltimateZombies.GetRandomItem();
        }

        public void HeadLeaveUpdate()
        {
            HeadLeaveTime -= Time.deltaTime;
            if (HeadLeaveTime <= 0)
            {
                zombie.anim?.SetTrigger($"head_leave");
                zombie.bossStatus = BossStatus.head_leave;
                HeadLeaveTime = 5f;
                zombie.spawnCount = UnityEngine.Random.RandomRangeInt(4, 7);
            }
        }

        public void HeadUpdate()
        {
            zombie.ballCountDown -= Time.deltaTime;
            if (zombie.ballCountDown <= 0f)
            {
                zombie.summonBallCount++;
                if (ThirdStage)
                {
                    switch (UltiSkillOrder[zombie.summonBallCount % 4])
                    {
                        case 0:
                            {
                                zombie.targetRow = UnityEngine.Random.Range(0, rowCount);
                                zombie.ball = true;
                                zombie.anim?.SetTrigger($"head_ulti");
                                zombie.animDriver?.SetTrigger("drive");
                                GameAPP.PlaySound(119);
                                zombie.ballCountDown = 5f;
                                break;
                            }
                        case 1:
                            {
                                zombie.targetRow = UnityEngine.Random.Range(0, rowCount);
                                zombie.ball = true;
                                zombie.anim?.SetTrigger($"attackslotmachine");
                                zombie.animDriver?.SetTrigger("drive");
                                GameAPP.PlaySound(119);
                                zombie.ballCountDown = 8f;

                                break;
                            }
                        case 2:
                            {
                                zombie.targetRow = UnityEngine.Random.Range(1, rowCount - 1);
                                zombie.ball = true;
                                zombie.anim?.SetTrigger($"gatling{zombie.targetRow + 1}");
                                zombie.animDriver?.SetTrigger("drive");
                                GameAPP.PlaySound(119);
                                zombie.ballCountDown = 8f;
                                break;
                            }
                        case 3:
                            {
                                zombie.targetRow = UnityEngine.Random.Range(1, rowCount - 1);
                                zombie.ball = true;
                                zombie.anim?.SetTrigger($"freeze");
                                zombie.animDriver?.SetTrigger("drive");
                                GameAPP.PlaySound(119);
                                zombie.ballCountDown = 5f;
                                break;
                            }
                    }
                }
                else
                {
                    switch (UnityEngine.Random.RandomRangeInt(0, 2))
                    {
                        case 0:
                            {
                                zombie.targetRow = UnityEngine.Random.Range(0, rowCount);
                                zombie.ball = true;
                                zombie.anim?.SetTrigger($"attack{zombie.targetRow + 1}");
                                zombie.animDriver?.SetTrigger("drive");
                                zombie.ballType = UnityEngine.Random.Range(0, 2);
                                UpdateEyeVisualization();
                                GameAPP.PlaySound(119);
                                zombie.ballCountDown = 5f;
                                break;
                            }
                        case 1:
                            {
                                zombie.targetRow = UnityEngine.Random.Range(1, rowCount - 1);
                                zombie.ball = true;
                                zombie.anim?.SetTrigger($"freeze");
                                zombie.animDriver?.SetTrigger("drive");
                                GameAPP.PlaySound(119);
                                zombie.ballCountDown = 5f;
                                break;
                            }
                    }
                }
            }
        }

        [HideFromIl2Cpp]
        public void RefreshSlotMachine(List<List<GameObject>> slots)
        {
            foreach (var sl in slots)
            {
                foreach (var s in sl)
                {
                    s.active = false;
                }
                sl.GetRandomItem().active = true;
            }
        }

        [HideFromIl2Cpp]
        public void Remove(ref HashSet<Vector2Int> boxes)
        {
            if (board == null || board.plantHead == null)
                throw new NullReferenceException();

            // 获取或创建植物筛选委托
            Func<Plant, bool> predicate = (p) => p is AdvancedFurnuce;

            // 筛选符合条件的植物
            Il2CppSystem.Collections.Generic.List<Plant> filteredPlants = board.plantHead.FindAll(predicate);

            // 遍历所有植物
            foreach (Plant plant in filteredPlants)
            {
                if (plant is null)
                    continue;

                Vector2Int plantPos = new(plant.thePlantColumn, plant.thePlantRow);

                // 移除植物周围3x3区域的方块
                for (int x = plantPos.x - 1; x <= plantPos.x + 1; x++)
                {
                    for (int y = plantPos.y - 1; y <= plantPos.y + 1; y++)
                    {
                        boxes.Remove(new Vector2Int(x, y));
                    }
                }
            }
        }

        public bool Skill()
        {
            if (!ThirdStage)
            {
                if (zombie.theHealth <= zombie.theMaxHealth / 2f && UnityEngine.Random.RandomRangeInt(0, 3) is 0)
                {
                    SkillRv();
                    return true;
                }
                else if (zombie.theHealth <= zombie.theMaxHealth / 1.25f && zombie.summonCount >= 10)
                {
                    SkillBungi();
                    return true;
                }
                return false;
            }
            else
            {
                switch (UnityEngine.Random.RandomRangeInt(0, 7))
                {
                    case 0:
                    case 6:
                        {
                            SkillRvEx();
                            break;
                        }
                    case 1:
                    case 2:
                    case 3:
                        {
                            SkillBungi();
                            break;
                        }
                    case 4:
                    case 5:
                        {
                            SkillVase();
                            break;
                        }
                }
            }
            return true;
        }

        public void SkillBungi()
        {
            zombie.anim.SetTrigger("bungi");
            zombie.bossStatus = BossStatus.bungi;
        }

        public void SkillRv()
        {
            zombie.anim.SetTrigger("rv");
            zombie.bossStatus = BossStatus.rv;

            // 随机生成目标位置
            zombie.targetRow = UnityEngine.Random.Range(0, rowCount - 1);
            zombie.targetColumn = UnityEngine.Random.Range(0, 3);

            Vector2 targetPos = new(Mouse.Instance.GetBoxXFromColumn(zombie.targetColumn), Mouse.Instance.GetBoxYFromRow(zombie.targetRow));

            Transform targetNode = zombie.animRV.transform.Find("Shadow");
            // 计算位置偏移
            Vector3 offset = new(targetPos.x - targetNode.position.x, targetPos.y - targetNode.position.y + 1.5f, 0);

            // 应用新位置
            Rv!.transform.position += offset;
            Rv!.active = true;
        }

        public void SkillRvEx()
        {
            SkillRv();
            for (int i = 0; i < board.rowNum; i++)
            {
                CreateZombie.Instance.SetZombie(i, ZombieType.DiamondRandomZombie);
                CreateZombie.Instance.SetZombie(i, ZombieType.DiamondRandomZombie);
            }
        }

        public void SkillSlotMachine()
        {
            zombie.anim.SetTrigger("slotstart");
            zombie.bossStatus = BossStatus.bungi;
        }

        public void SkillVase()
        {
            zombie.anim.SetTrigger("vase");
            zombie.bossStatus = BossStatus.bungi;
        }

        public void SpawnUpdate()
        {
            zombie.summonTime -= Time.deltaTime;

            if (zombie.summonTime < 0f)
            {
                zombie.summonTime = 4f;
                GameAPP.PlaySound(119);
                if (zombie.summonCount is 6)
                {
                    zombie.anim.SetTrigger("takeaway");
                    zombie.summonCount++;
                    return;
                }
                if (UnityEngine.Random.Range(0, 8) == 0 && Skill()) return;

                if (UnityEngine.Random.Range(0, 4) == 2)
                {
                    foreach (var plant in Board.Instance.plantArray)
                    {
                        if (plant is not null)
                        {
                            if (plant.thePlantColumn > 5 && !plant.invincible)
                            {
                                zombie.FootCrash(plant.thePlantRow);
                                return;
                            }
                        }
                    }
                }
                zombie.targetRow = UnityEngine.Random.Range(0, rowCount);
                zombie.anim.SetTrigger($"spawn{zombie.targetRow + 1}");
                zombie.bossStatus = BossStatus.spawn;
            }
        }

        public void Start()
        {
            zombie.revived = true;
            zombie.animDriver = gameObject.transform.FindChild("Zombie_boss_head").FindChild("Zombie_boss_driver").GetComponent<Animator>();
            zombie.animRV = gameObject.transform.FindChild("BossRv").GetComponent<Animator>();
            zombie.ballPosition = gameObject.transform.FindChild("Zombie_boss_jaw").transform;
            zombie.eye = gameObject.transform.FindChild("Zombie_boss_eyeglow").GetComponent<SpriteRenderer>();
            zombie.eyes = gameObject.transform.FindChild("Zombie_boss_eyeglow").gameObject;
            zombie.foot_inner = gameObject.transform.FindChild("Zombie_boss_foot_1").gameObject;
            zombie.foot_outter = gameObject.transform.FindChild("Zombie_boss_foot").gameObject;
            zombie.hand = gameObject.transform.FindChild("Zombie_boss_outerarm_hand").gameObject;
            zombie.head = gameObject.transform.FindChild("Zombie_boss_head").gameObject;
            zombie.jaw = gameObject.transform.FindChild("Zombie_boss_jaw").gameObject;
            zombie.spawnPosition = gameObject.transform.FindChild("Zombie_boss_outerarm_finger1_2");
            zombie.thumb = gameObject.transform.FindChild("Zombie_boss_outerarm_thumb2").gameObject;
            BlackHole = gameObject.transform.FindChild("BlackHole").gameObject;
            Rv = gameObject.transform.FindChild("BossRv").gameObject;
            Rv.AddComponent<Rv>();
            InnerHand = gameObject.transform.FindChild("Zombie_boss_innerarm_hand");
            GatlingShoot = gameObject.transform.FindChild("Zombie_boss_head").FindChild("Gatling").FindChild("GatlingShoot");
            foreach (var p in GameAPP.resourcesManager.allPlants)
            {
                if ((int)p >= 900 && (int)p <= 999 && p is not PlantType.GoldHypnoDoom)
                {
                    UltimatePlants.Add(p);
                }
            }

            foreach (var z in GameAPP.resourcesManager.allZombieTypes)
            {
                if ((int)z >= 200 && !TypeMgr.WaterZombie(z) && !BanList.Contains((int)z))
                {
                    if (TypeMgr.IsBossZombie(z) && z is not ZombieType.UltimateMachineNutZombie)
                    {
                        BossZombies.Add(z);
                    }
                    else if (z is not ZombieType.SuperSubmarine && z is not ZombieType.DolphinGatlingZombie && z is not ZombieType.UltimateMachineNutZombie)
                    {
                        UltimateZombies.Add(z);
                    }
                }
            }

            SlotMachine = gameObject.transform.FindChild("SlotMachine").gameObject;
            for (int c = 0; c < 9; c++)
            {
                Slot1.Add(SlotMachine.transform.GetChild(6).GetChild(1).GetChild(c).gameObject);
                Slot2.Add(SlotMachine.transform.GetChild(5).GetChild(1).GetChild(c).gameObject);
                Slot3.Add(SlotMachine.transform.GetChild(4).GetChild(1).GetChild(c).gameObject);
                Slot1_1.Add(SlotMachine.transform.GetChild(6).GetChild(2).GetChild(c).gameObject);
                Slot2_1.Add(SlotMachine.transform.GetChild(5).GetChild(2).GetChild(c).gameObject);
                Slot3_1.Add(SlotMachine.transform.GetChild(4).GetChild(2).GetChild(c).gameObject);
            }
            Slots = [Slot1, Slot2, Slot3];
            Slots_1 = [Slot1_1, Slot2_1, Slot3_1];

            foreach (var b in board.zombieBalls)
            {
                if (b is not null && b is ZombieBall ball && ball.boss)
                {
                    b.Die();
                }
            }
            System.Random rand = new();
            UltiSkillOrder = [.. from item in UltiSkillOrder orderby rand.Next() select item];
        }

        public void SummonMoney(bool buff = true)
        {
            for (int i = 0; i < 8; i++)
            {
                CreateItem.Instance.SetCoin(Mouse.Instance.GetColumnFromX(zombie.transform.position.x), zombie.theZombieRow, (int)ItemType.DiamondCoin, 1);
            }
            if (buff) UnlockBuff();
        }

        public void UnlockBuff()
        {
            var travel = GameAPP.gameAPP.GetComponent<TravelMgr>();
            var ulti = GetRandomFalseIndex(travel.ultimateUpgrades);
            var adv = GetRandomFalseIndex(travel.advancedUpgrades);
            int[] ban = [55, 56, 57, 58, 59];
            if (UnityEngine.Random.RandomRangeInt(0, 3) is 1 && ulti >= 0)
            {
                GameAPP.gameAPP.GetComponent<TravelMgr>().ultimateUpgrades[ulti] = 1;
            }
            else if (adv >= 0 && !ban.Contains(adv))
            {
                GameAPP.gameAPP.GetComponent<TravelMgr>().advancedUpgrades[adv] = true;
            }
        }

        public unsafe void UpdateEx()
        {
            if (zombie is null) return;
            if (!Alive)
            {
                board.theMoney = 0;
                return;
            }
            IL2CPP.Il2CppObjectBaseToPtrNotNull(zombie);
            Unsafe.SkipInit(out IntPtr exc);
            IL2CPP.il2cpp_runtime_invoke((IntPtr)typeof(Zombie).GetField("NativeMethodInfoPtr_Update_Protected_Virtual_New_Void_0", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!, IL2CPP.Il2CppObjectBaseToPtrNotNull(zombie), (void**)(IntPtr*)null, ref exc);
            Il2CppException.RaiseExceptionIfNecessary(exc);
            if (AttackCountDown > 0) AttackCountDown -= Time.deltaTime;
            if (AttackCountDown < -1) AttackCountDown = 0;
            zombie.UpdateHealthText();
            zombie.RemoveDeBuff();
            BanGoldMelon();
            if (ThirdStage)
            {
                if (Board.Instance.theMoney > zombie.theHealth)
                {
                    Board.Instance.theMoney = zombie.theHealth;
                }
            }
            else if (zombie.theHealth <= zombie.theMaxHealth / 6)
            {
                EnterThirdStage();
            }
            if (zombie.bossStatus is BossStatus.idle)
            {
                if (zombie.spawnCount > 0)
                {
                    SpawnUpdate();
                }
                else
                {
                    zombie.anim.SetTrigger("head_enter");
                    zombie.ball = false;
                    zombie.bossStatus = BossStatus.head_enter;
                    GameAPP.PlaySound(119, 0.5f, 1f);
                }
            }
            try
            {
                if (zombie.col is not null)
                    zombie.col.enabled = zombie.bossStatus is BossStatus.head_idle;
            }
            catch { }
            zombie.theStatus = zombie.bossStatus is BossStatus.head_idle ? ZombieStatus.Default : ZombieStatus.Boss;
            if (zombie.bossStatus is BossStatus.head_idle && zombie.freezeSpeed > 0 && !zombie.ball)
            {
                HeadUpdate();
            }
            if (zombie.bossStatus is BossStatus.head_idle && zombie.freezeSpeed > 0 && zombie.ball)
            {
                HeadLeaveUpdate();
            }
        }

        public void UpdateEyeVisualization()
        {
            if (zombie.eye is null) return;
            foreach (var child in zombie.eye.transform.GetComponentsInChildren<Transform>())
            {
                if (zombie.eye.transform == child) continue;
                child.gameObject.SetActive(false);
            }
            if (!ThirdStage)
            {
                zombie.eye.transform.GetChild(zombie.ballType is 1 ? 1 : 0).gameObject.SetActive(true);
            }
        }

        private static void SetChildActive(GameObject parent, int childIndex, bool active)
        {
            if (parent == null) return;
            Transform child = parent.transform.GetChild(childIndex);
            child?.gameObject.SetActive(active);
        }

        private static void ToggleComponent<T>(GameObject obj, bool enable) where T : Renderer
        {
            if (obj == null) return;
            T component = obj.GetComponent<T>();
            if (component != null)
                component.enabled = enable;
        }

        public bool Alive { get; set; } = true;
        public float AttackCountDown { get; set; } = 0;
        public float AttackInterval { get; set; } = 0.1f;
        public int[] BanList { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [302, 305, 306, 309, 317, 320, 323, 326, 329, 332, 335];
        public GameObject? BlackHole { get; set; }
        public Board board => Board.Instance;
        public List<ZombieType> BossZombies { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public int DamageLimit { get; set; } = 1000;
        public Transform? GatlingShoot { get; set; }
        public float HeadLeaveTime { get; set; } = 5f;
        public Transform? InnerHand { get; set; }
        public int rowCount => board.rowNum >= 6 ? 6 : 5;
        public GameObject? Rv { get; set; }
        public List<GameObject> Slot1 { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public List<GameObject> Slot1_1 { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public List<GameObject> Slot2 { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public List<GameObject> Slot2_1 { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public List<GameObject> Slot3 { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public List<GameObject> Slot3_1 { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public GameObject? SlotMachine { get; set; }
        public List<List<GameObject>> Slots { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public List<List<GameObject>> Slots_1 { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public bool ThirdStage { get; set; } = false;
        public int ThirdStageHealth { get; set; } = 100000;
        public List<PlantType> UltimatePlants { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public List<ZombieType> UltimateZombies { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [];
        public int[] UltiSkillOrder { [HideFromIl2Cpp] get; [HideFromIl2Cpp] set; } = [1, 2, 3, 0];
        public ZombieBoss2 zombie => gameObject.GetComponent<ZombieBoss2>();
    }
}