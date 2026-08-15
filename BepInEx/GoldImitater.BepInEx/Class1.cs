using BepInEx;
using BepInEx.Unity.IL2CPP;
using Core;
using CustomizeLib.BepInEx;
using CustomizeLib.BepInEx.ExtensionData.Basic;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace GoldImitater.BepInEx
{
    [BepInPlugin("salmon.goldimitater", "GoldImitater", "1.0")]
    public class Core : BasePlugin//960
    {
        public override void Load()
        {
            Tools.InitMod();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<GoldImitater>();
            ClassInjector.RegisterTypeInIl2Cpp<ClearCold>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "goldimitater");
            CustomCore.RegisterCustomPlant<Imitater, GoldImitater>(GoldImitater.PlantID, ab.GetAsset<GameObject>("GoldImitaterPrefab"),
                ab.GetAsset<GameObject>("GoldImitaterPreview"), [], 0f, 0f, 0, 300, 15, 50);
            CustomCore.RegisterCustomPlantSkin<Imitater, GoldImitater>(GoldImitater.PlantID, ab.GetAsset<GameObject>("GoldImitaterPrefabNewYear"),
                ab.GetAsset<GameObject>("GoldImitaterPreviewNewYear"), [], 0f, 0f, 0, 300, 15, 50);
            CustomCore.AddPlantAlmanacStrings(GoldImitater.PlantID, $"黄金模仿者({GoldImitater.PlantID})",
                "或许是宝藏呢？\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>特点：</color><color=red>出场1.5秒后，生成随机植物，僵尸和特殊产出</color>\n" +
                "<color=#3D1400>概率明细：</color>\n" +
                "<color=red>各类普通植物（40%）；究极植物（20%），各类普通僵尸（20%）；究极僵尸（10%）；领袖及Boss僵尸（5%）；其他事件（5%）。生成的僵尸随机具有0.8/1/1.2倍韧性</color>\n" +
                "<color=#3D1400>事件明细：</color>\n" +
                "<color=red>①生成10张卡片：黄金模仿者（50%）；钻石模仿者（50%）\n" +
                "②获得一个词条：植物词条（90%）；僵尸词条（10%）\n" +
                "③获得1000阳光</color>\n" +
                "<color=#3D1400>特殊强化：</color><color=red>①<Boss>僵王博士：血量x100，免疫寒冷，免疫冻结\n" +
                "②<Boss>黄金僵王博士：血量x100，免疫寒冷，免疫冻结\n" +
                "③<Boss>黑橄榄大帅：血量x15\n" +
                "④<Boss>黑橄榄将军：血量x15\n" +
                "④<Boss>舞台巡演车：血量x15</color>\n" +
                "<color=#3D1400>词条1：</color><color=red>孤注一掷：黄金模仿者出现究极植物与究极僵尸的概率大幅提高\n" +
                "*概率明细：\n" +
                "各类普通植物（10%）；究极植物（35%），各类普通僵尸（5%）；究极僵尸（30%）；领袖及Boss僵尸（15%）；其他事件（5%）。</color>\n\n" +
                "<color=#3D1400>“茄本无相，吾有万象。”黄金模仿者侃侃而谈：“就像你看到的，我可以是任何植物，任何僵尸，在后院奋战的豌豆是我，高举旗帜冲锋的僵尸也是我，沉入土壤的尸体是我，哇哇啼哭的孩童也是我。但是'我'与'我'之间的性格是不同的，记忆是不同的，童年成长都是不同的，他们各司其职，他们努力生活。“那拥有不同记忆不同性格的你还是你么”黄金模仿者似乎陷入了沉思……不再说话。</color>\n\n" +
                "<color=#955300>价格：</color><color=red>50</color>\n" +
                "<color=#955300>冷却：</color><color=red>15秒</color>");
            CustomCore.RegisterCustomCardToColorfulCards((PlantType)GoldImitater.PlantID, 14);
            GoldImitater.buff = CustomCore.RegisterCustomBuff("孤注一掷：黄金模仿者出现究极植物与究极僵尸的概率大幅提高", BuffType.AdvancedBuff, () => true, 5000, plantType: (PlantType)GoldImitater.PlantID);
        }
    }

    public class GoldImitater : MonoBehaviour
    {
        public static BuffID buff = -1;
        public static int PlantID = 1931;

        public void Start()
        {
            int total = 0;
            var config = GameAPP.config;
            if (config == null) return;
            if (config.levelZombieInRandom) total += 2;
            if (config.strongUltiZombieInRandom) total += 2;
            if (config.leaderInRandom) total += 6;
            if (GameAPP.theGameStatus == GameStatus.InGame && plant != null && UnityEngine.Random.Range(1, 101) <= total && plant.board != null &&  (plant.board.boardTag.isSuperRandom || plant.board.boardTag.isIZ))
            {
                plant.StarUp();
                plant.starUp = true;
                plant.UpdateStarIcon();
            }
        }

        public void AnimSpawn()
        {
            ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, plant.axis.transform.position, plant.thePlantRow, lim: true);
            int row = plant.thePlantRow;
            int column = plant.thePlantColumn;
            var axis = plant.axis.transform.position;
            plant.Die();
            var v = UnityEngine.Random.Range(1, 101);
            if (!Lawnf.TravelAdvanced(buff) && !plant.starUp)
            {
                if (v <= 60)
                {
                    RandomPlant(v, 40, 20, column, row);
                }
                else if (v <= 95)
                {
                    RandomZombie(v, 80, 10, 5, axis, row);
                }
                else
                {
                    v = UnityEngine.Random.Range(1, 4);
                    TriggerEvent(v, axis);
                }
            }
            else
            {
                if (v <= 45)
                {
                    RandomPlant(v, 10, 35, column, row);
                }
                else if (v <= 95)
                {
                    RandomZombie(v, 50, 30, 15, axis, row);
                }
                else
                {
                    v = UnityEngine.Random.Range(1, 4);
                    TriggerEvent(v, axis);
                }
            }
        }

        public void SetRandomPlant(PlantType plantType, int column, int row)
        {
            if (plant.board.GetBoxType(column, row) == BoxType.Water)
                CreatePlant.Instance.SetPlant(column, row, PlantType.LilyPad, isFreeSet: true);
            if (plant.board.GetBoxType(column, row) == BoxType.Roof)
                CreatePlant.Instance.SetPlant(column, row, PlantType.Pot, isFreeSet: true);
            TryStarUpPlant(CreatePlant.Instance.SetPlant(column, row, plantType, isFreeSet: true).GetComponent<Plant>());
            if (TypeMgr.IsPuff(plantType))
            {
                TryStarUpPlant(CreatePlant.Instance.SetPlant(column, row, plantType, isFreeSet: true).GetComponent<Plant>());
                TryStarUpPlant(CreatePlant.Instance.SetPlant(column, row, plantType, isFreeSet: true).GetComponent<Plant>());
            }
        }

        public void TryStarUpPlant(Plant plant)
        {
            if (!plant.OnStarUp() || !this.plant.starUp) return;
            plant.StarUp();
            plant.starUp = true;
            plant.UpdateStarIcon();
        }

        public void SetRandomZombie(ZombieType zombieType, float x, int row)
        {
            int nR = row;
            if (zombieType == ZombieType.ZombieBoss || zombieType == ZombieType.ZombieBoss2)
                nR = 0;
            if (zombieType == ZombieType.ZombieBoss)
                GameAPP.Instance.PlayMusic(MusicType.Boss);
            if (zombieType == ZombieType.ZombieBoss2)
                GameAPP.Instance.PlayMusic(MusicType.Boss2);
            var tmp = Board.Instance.isEveStarted;
            Board.Instance.isEveStarted = true;
            try
            {
                var zombie = CreateZombie.Instance.SetZombie(nR, zombieType, x).GetComponent<Zombie>();
                Board.Instance.isEveStarted = tmp;
                if (zombie == null) return;
                if (zombie.theZombieType == ZombieType.ZombieBoss || zombie.theZombieType == ZombieType.ZombieBoss2)
                    zombie.AddComponent<ClearCold>().zombie = zombie;
                float timer = 1f;
                switch (UnityEngine.Random.Range(0, 3))
                {
                    case 0:
                        timer = 0.8f;
                        break;
                    case 1:
                        timer = 1.2f;
                        break;
                }
                int bossMulti = 100;
                bool flag = zombieType == ZombieType.ZombieBoss || zombieType == ZombieType.ZombieBoss2;
                if (flag)
                    if (plant.board.boardTag.isSuperRandom)
                        bossMulti = 10;
                if (plant.starUp) bossMulti *= 10;
                if (flag)
                {
                    zombie.theHealth *= bossMulti;
                    zombie.theMaxHealth *= bossMulti;
                    zombie.SetData("GoldImitater_SpawnByGold", true);
                }
                if (zombieType == ZombieType.HorseBoss || zombieType == ZombieType.FootballBoss || zombieType == ZombieType.JacksonDriverBoss)
                {
                    zombie.theHealth *= 15;
                    zombie.theMaxHealth *= 15;
                }
                zombie.theHealth = (int)(zombie.theHealth * timer);
                zombie.theMaxHealth = (int)(zombie.theMaxHealth * timer);
                zombie.theFirstArmorHealth = (int)(zombie.theFirstArmorHealth * timer);
                zombie.theFirstArmorMaxHealth = (int)(zombie.theFirstArmorMaxHealth * timer);
                zombie.theSecondArmorHealth = (int)(zombie.theSecondArmorHealth * timer);
                zombie.theSecondArmorMaxHealth = (int)(zombie.theSecondArmorMaxHealth * timer);
            }
            catch (Exception)
            { }
        }

        public void RandomPlant(int v, int normal, int ulti, int column, int row)
        {
            var list = GetPlantsList();
            if (v <= normal)
            {
                list = list.Where(type => !Lawnf.IsUltiPlant(type)).ToList();
                SetRandomPlant(list[UnityEngine.Random.Range(0, list.Count)], column, row);
            }
            else if (v <= (normal + ulti))
            {
                list = list.Where(type => Lawnf.IsUltiPlant(type)).ToList();
                SetRandomPlant(list[UnityEngine.Random.Range(0, list.Count)], column, row);
            }
        }

        public void RandomZombie(int v, int normal, int ulti, int boss, Vector2 axis, int row)
        {
            var list = GetZombiesList();
            if (v <= normal)
            {
                list = list.Where(type => !TypeMgr.UltimateZombie(type) && !TypeMgr.IsBossZombie(type) && type != ZombieType.Nothing).ToList();
                SetRandomZombie(list[UnityEngine.Random.Range(0, list.Count)], axis.x, row);
            }
            else if (v <= (normal + ulti))
            {
                list = list.Where(type => TypeMgr.UltimateZombie(type) && !TypeMgr.IsBossZombie(type) && type != ZombieType.Nothing).ToList();
                SetRandomZombie(list[UnityEngine.Random.Range(0, list.Count)], axis.x, row);
            }
            else if (v <= (normal + ulti + boss))
            {
                list = list.Where(type => TypeMgr.IsBossZombie(type) && type != ZombieType.Nothing).ToList();
                SetRandomZombie(list[UnityEngine.Random.Range(0, list.Count)], axis.x, row);
            }
        }

        public void TriggerEvent(int v, Vector2 axis)
        {
            switch (v)
            {
                case 1:
                    {
                        for (int i = 1; i <= 10; i++)
                            Lawnf.SetDroppedCard(axis, UnityEngine.Random.Range(0, 2) == 0 ? (PlantType)PlantID : PlantType.DiamondImitater);
                        InGameText.Instance.ShowText("模仿十连抽！", 5f);
                    }
                    break;
                case 2:
                    {
                        var mgr = GameAPP.Instance.GetOrAddComponent<TravelMgr>();
                        int value = UnityEngine.Random.Range(1, 101);
                        var data = mgr.data;
                        if (value <= 90)
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
                case 3:
                    {
                        for (int i = 1; i <= 20; i++)
                            CreateItem.Instance.SetCoin(0, 0, 1, 0, axis);
                        InGameText.Instance.ShowText("大量阳光！", 5f);
                    }
                    break;
            }
        }

        public List<PlantType> GetPlantsList()
        {
            return GameAPP.resourcesManager.allPlants.ToArray().ToList().Where(x => x != PlantType.Nothing && x != PlantType.MagnetBox &&
                            x != PlantType.MagnetInterface && x != PlantType.Pit && x != PlantType.Refrash && x != PlantType.Extract_single &&
                            x != PlantType.Extract_ten && x != PlantType.VectorPlant).ToList();
        }

        public List<ZombieType> GetZombiesList()
        {
            return GameAPP.resourcesManager.allZombieTypes.ToArray().ToList().Where(x => x != ZombieType.Nothing && x != ZombieType.TrainingDummy &&
                        x != ZombieType.ProjectileZombie).ToList();
        }

        public Imitater plant => gameObject.GetComponent<Imitater>();
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

    /*[HarmonyPatch(typeof(CardUI), nameof(CardUI.Start))]
    public class CardUI_Start_Patch
    {
        public static int loopCount = 0;
        [HarmonyPrefix]
        public static void Postfix(CardUI __instance)
        {
            if (__instance.thePlantType == (PlantType)GoldImitater.PlantID && loopCount < 14)
            {
                GameObject go = GameObject.Instantiate(__instance.gameObject, __instance.transform.parent);
                go.transform.position = __instance.transform.position;
                __instance.CD = __instance.fullCD;
                var cardui = go.GetComponent<CardUI>();
                cardui.CD = cardui.fullCD;
                cardui.parent = __instance.gameObject;
                loopCount++;
            }
        }
    }*/

    /*[HarmonyPatch(typeof(Board), nameof(Board.Start))]
    public class Board_Start_Patch
    {
        [HarmonyPostfix]
        public static void Postfix() => CardUI_Start_Patch.loopCount = 0;
    }*/

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
        //    if (__instance.GetData<bool>("GoldImitater_SpawnByGold"))
        //    {
        //        __result = Mathf.Min(__result, 5000);
        //    }
        //}
    }

    [HarmonyPatch(typeof(GameAPP), nameof(GameAPP.Awake))]
    public static class GameAPPPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GameAPP __instance)
        {
            __instance.SetData("GoldImitater_set", true);
            __instance.SetData("GoldImitater_buffID", GoldImitater.buff);
        }
    }
}
