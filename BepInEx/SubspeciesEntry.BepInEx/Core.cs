using AlmanacData;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace SubspeciesEntry.BepInEx
{
    [BepInPlugin("salmon.subspeciesentry", "Subspecies Entry", "1.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }
    }

    public static class Core
    {
        public static void Init()
        {
            InitCustomBuff();
            CoreTools.Init();
            Load();
            TypeInit.Init();
            CustomBehaviours.Init();
        }

        public static void Load()
        {
            InitBuffText();
            InitAlmanacText();
            InitCardClick();
            InitDataExtra();
            InitLevelBuff();
        }

        public static void InitBuffText()
        {
            #region 大帝伴侣
            {
                // 流星雨
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("流星雨"),
                    "流星雨：究极杨桃大帝的攻击间隔降低至0.5秒", "流星雨：究极杨桃大帝的攻击间隔降低至0.5秒；亚种五叶草回旋加速降至0.5秒，吸引范围+50%");
                // 众星之力
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("众星之力"),
                    "众星之力：究极杨桃大帝的子弹伤害x2（2级时x3），但发射时不会超过3000（2级时无上限）",
                    "众星之力：究极杨桃大帝的子弹伤害x2，伤害上限增至3000；亚种五叶草子弹伤害x2。2级时，究极杨桃大帝伤害x3，取消伤害上限；亚种五叶草伤害x3，取消储存上限");
                // 斗转星移
                ReplaceText.ReplaceBuff(BuffType.AdvancedBuff, (int)CoreTools.GetAdvBuffByString("斗转星移"),
                    "斗转星移：所有的流星冷却缩短为原来的1/2，且场上每多一个究极杨桃大帝则提升300基础伤害",
                    "斗转星移：所有流星冷却缩短为原来的1/2，所有五叶草储存上限+50%，且场上每多一个究极杨桃大帝则提升300基础伤害，五叶草增加10储存上限");
            }
            #endregion
            #region 金蛋
            {
                // 无尽贪婪
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("无尽贪婪"),
                    "无尽贪婪：究极超时空玉米的黑洞可以吸引一切子弹",
                    "无尽贪婪：究极超时空玉米的黑洞可以吸引一切子弹；亚种超时空坚果每30秒立即回溯一次并回复2000韧性");
                // 万劫不复
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("万劫不复"),
                    "万劫不复：究极超时空玉米的黑洞吸引子弹的范围大幅增加",
                    "万劫不复：究极超时空玉米的黑洞吸引子弹的范围大幅增加；亚种超时空坚果每次回溯都会净化自身状态");
            }
            #endregion
            #region 血月÷子
            {
                // 金光闪闪
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("金光闪闪"),
                    "金光闪闪：太阳神发射子弹时，消耗超过15000部分的阳光的0.5%，使子弹增加消耗阳光数20倍的伤害，亚种月亮神子弹的光照等级增伤×3",
                    "金光闪闪：太阳神发射子弹时，消耗超过15000部分的阳光的0.5%，使子弹增加消耗阳光数20倍的伤害，亚种月亮神子弹的光照等级增伤×3；变种血月神的子弹的光照等级增伤x3，前20级光照等级，每级血月额外提供僵尸的增益提升50%");
                // 人造太阳
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("人造太阳"),
                    "人造太阳：太阳神卷心菜召唤的小太阳伤害x3，亚种月亮神卷心菜召唤的明月治疗量×3",
                    "人造太阳：太阳神卷心菜召唤的小太阳伤害x3，亚种月亮神卷心菜召唤的明月治疗量×3；变种血月神卷心菜召唤的血月持续时间x3，且召唤时间减至5秒");
            }
            #endregion
            #region 曾哥&牢灯
            {
                // 万籁俱寂
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("万籁俱寂"),
                    "万籁俱寂：究极忧郁菇亡语伤害增加到100万，亚种究极路灯花亡语提供的光照等级×3",
                    "万籁俱寂：究极忧郁菇死亡时或对其使用毁灭菇卡片时，造成1000万伤害并额外造成0.5倍韧性的伤害；亚种路灯花为全场提供的光照x3，并使全场目标的冻结值上限降至100");
                // 以爆制爆
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("以爆制爆"),
                    "以爆制爆：究极忧郁菇及亚种免疫非物理爆炸，每吸收10000点爆炸伤害将释放毁灭菇爆炸",
                    "以爆制爆：究极忧郁菇及亚种免疫非物理爆炸，每吸收或受到10000点伤害，释放一次（累计吸收伤害/3.8）伤害的毁灭菇效果，不留坑洞");
            }
            #endregion
            #region 鮟鱇鱼
            {
                // 深渊巨口
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("深渊巨口"),
                    "深渊巨口：提高究极大嘴花的攻击距离",
                    "深渊巨口：究极灯笼大嘴花对于不可吞食的僵尸，对其造成僵尸韧性的（25+65x范围僵尸总血量/ 范围僵尸总血量+10^6）%无视90%护甲系数的伤害，该效果会使最终消化时间改为90秒");
                // 光芒四射
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("光芒四射"),
                    "光芒四射：究极灯笼巨嘴咀嚼期间为5x5范围提供2级光照",
                    "光芒四射：究极灯笼大嘴花吞食后，为自身半径3.7格的植物提供（1+此次吞食僵尸数量）的光照等级，至多10点");
            }
            #endregion
            #region 珞
            //{
            //    // 我是梦珞
            //    ReplaceText.ReplaceBuff(BuffType.AdvancedBuff, (int)CoreTools.GetAdvBuffByString("我是梦珞"),
            //        "我是梦珞：魔法浮游炮伤害×3。魔法兔耳葱协助召唤的浮游炮会发射60发平射子弹",
            //        "我是梦珞：魔法浮游炮伤害增加（击杀僵尸韧性x10%）点。魔法兔耳葱协助召唤的浮游炮会发射15发子弹");
            //    // 我也是梦珞
            //    ReplaceText.ReplaceBuff(BuffType.AdvancedBuff, (int)CoreTools.GetAdvBuffByString("我也是梦珞"),
            //        "我也是梦珞：解锁亚种魔法兔耳葱。魔法猫尾草的追踪子弹伤害×5。魔法兔耳葱造成的伤害×3",
            //        "我也是梦珞：解锁亚种魔法兔耳葱。击杀场上僵尸需要的僵尸数降低至40。魔法兔耳葱造成的伤害x3");
            //}
            #endregion
            #region 牢樱
            {
                // 力大砖飞
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("力大砖飞"),
                    "力大砖飞：植物方造成的樱桃爆炸伤害x3",
                    "力大砖飞：植物方造成的樱桃爆炸效果x3，二级时植物方造成的樱桃爆炸效果x10，且伤害不低于300");
            }
            #endregion
            #region 黑高
            {
                // 世纪之盾
                ReplaceText.ReplaceBuff(BuffType.UltimateBuff, (int)CoreTools.GetUltiBuffByString("世纪之盾"),
                    "世纪之盾：究极黑曜石高坚果及亚种的韧性×3",
                    "世纪之盾：究极黑曜石高坚果及亚种的韧性×3，二级时×9");
            }
            #endregion
        }

        public static void InitAlmanacText()
        {
            #region 大帝伴侣
            {
                ReplaceText.ReplaceAlmanac(PlantType.UltimateStar,
                    "子弹伤害×2，伤害上限增至3000。2级，子弹伤害×3，取消伤害上限",
                    "子弹伤害x2，伤害上限增至3000；亚种五叶草子弹伤害x2。2级时，伤害x3，取消伤害上限；亚种五叶草伤害x3，取消储存上限");
                ReplaceText.ReplaceAlmanac(PlantType.UltimateStar,
                    "攻击间隔降低至0.5秒",
                    "攻击间隔降至0.5秒；亚种五叶草回旋加速降至0.5秒，吸引范围+50%");
            }
            #endregion
            #region 金蛋
            {
                // 无尽贪婪
                ReplaceText.ReplaceAlmanac(PlantType.UltimateCorn, "究极黑洞可以吸收绝大多数子弹",
                    "究极黑洞可以吸收绝大多数子弹；亚种超时空坚果每30秒立即回溯一次并回复2000韧性");
                // 万劫不复
                ReplaceText.ReplaceAlmanac(PlantType.UltimateCorn, "究极黑洞吸引子弹的半径翻倍",
                    "究极黑洞吸引子弹的半径翻倍；亚种超时空坚果每次回溯都会净化自身状态");
            }
            #endregion
            #region 血月÷子
            {
                ReplaceText.ReplaceAlmanac(PlantType.UltimateCabbage,
                    "太阳神的子弹会消耗超过15000阳光部分0.5%阳光，使该子弹增加(20×消耗阳光)的伤害；亚种月亮神的子弹的光照等级增伤×3",
                    "太阳神的子弹会消耗超过15000阳光部分0.5%阳光，使该子弹增加（20x消耗阳光）的伤害；亚种月亮神的子弹的光照等级增伤x3；变种血月神的子弹的光照等级增伤x3 ，前20级光照等级，每级血月额外提供僵尸的增益提升50%");
                ReplaceText.ReplaceAlmanac(PlantType.UltimateCabbage,
                    "太阳伤害×3\n月亮回血×3",
                    "太阳伤害x3\n月亮回血x3\n血月持续时间x3，且召唤时间减至5秒");
            }
            #endregion
            #region 曾哥&牢灯
            {
                // 万籁俱寂
                ReplaceText.ReplaceAlmanac(PlantType.UltimateGloom,
                    "究极忧郁菇死亡时造成的伤害增至100万，究极路灯花死亡时为全场提供的光照级数×3",
                    "究极忧郁菇死亡时或对其使用毁灭菇卡片时，造成1000万伤害并额外造成0.5倍韧性的伤害；亚种路灯花为全场提供的光照x3，并使全场目标的冻结值上限降至100");
                // 以爆制爆
                ReplaceText.ReplaceAlmanac(PlantType.UltimateGloom,
                    "免疫非物理爆炸，每吸收10000点伤害释放毁灭菇效果（不留坑洞）",
                    "究极忧郁菇及亚种免疫非物理爆炸，每吸收或受到10000点伤害，释放一次（累计吸收伤害/3.8）伤害的毁灭菇效果，不留坑洞");
            }
            #endregion
            #region 鮟鱇鱼
            {
                // 深渊巨口
                ReplaceText.ReplaceAlmanac(PlantType.UltimateBigChomper,
                    "深渊巨口：究极灯笼巨嘴花的攻击距离增加1.5格",
                    "深渊巨口：究极灯笼大嘴花对于不可吞食的僵尸，对其造成僵尸韧性的（25+65x范围僵尸总血量/ 范围僵尸总血量+10^6）%无视90%护甲系数的伤害，该效果会使最终消化时间改为90秒");
                // 光芒四射
                ReplaceText.ReplaceAlmanac(PlantType.UltimateBigChomper,
                    "光芒四射：消化时间内会为5x5范围提供2点光照等级",
                    "光芒四射：究极灯笼大嘴花吞食后，为自身半径3.7格的植物提供（1+此次吞食僵尸数量）的光照等级，至多10点");
            }
            #endregion
            #region 珞
            //{
            //    // 基础特性
            //    ReplaceText.ReplaceAlmanac(PlantType.CattailLour,
            //        "每击杀1个僵尸，召唤1台浮游炮。浮游炮在3秒内发射60发平射子弹（无法对空，160伤害），随后离场",
            //        "每击杀1个僵尸，召唤1台浮游炮。浮游炮在3秒内发射30发子弹（伤害为本体伤害2倍），可对空，随后离场，如果本体已有一个浮游炮，则改为为已存在的浮游炮增加子弹发射数量和伤害");
            //    ReplaceText.ReplaceAlmanac(PlantType.CattailLour,
            //       "<color=#3D1400>③</color><color=red>浮游炮击杀僵尸后也会召唤浮游炮</color>",
            //       "<color=#3D1400>③</color><color=red>当满足以下任意一个条件时，将召唤浮游炮：\n" +
            //       "①浮游炮击杀僵尸后\n" +
            //       "②场上击杀60个僵尸后\n" +
            //       "③出场及出场后每15秒</color>");
            //    // 我是梦珞
            //    ReplaceText.ReplaceAlmanac(PlantType.CattailLour,
            //        "我是梦珞：浮游炮伤害×3。魔法兔耳葱协助召唤的浮游炮会发射60发平射子弹",
            //        "我是梦珞：魔法浮游炮伤害增加（击杀僵尸韧性x10%）点。魔法兔耳葱协助召唤的浮游炮会发射12发子弹");
            //    // 我也是梦珞
            //    ReplaceText.ReplaceAlmanac(PlantType.CattailLour,
            //        "我也是梦珞：解锁亚种魔法兔耳葱。魔法猫尾草的追踪子弹伤害×5。魔法兔耳葱造成的伤害×3",
            //        "我也是梦珞：解锁亚种魔法兔耳葱。击杀场上僵尸需要的僵尸数降低至40。魔法兔耳葱造成的伤害x3");

            //    // 亚种图鉴修改
            //    ReplaceText.ReplaceAlmanac(PlantType.PinkOnion,
            //        "传递的电流击杀僵尸时，使场上随机一株魔法猫尾草召唤一台只会发射20发平射子弹的浮游炮",
            //        "传递的电流击杀僵尸时，使场上随机一株魔法猫尾草召唤一台只会发射12发平射子弹的浮游炮");
            //}
            #endregion
            #region 剑仙
            {
                ReplaceText.ReplaceAlmanac(PlantType.AbyssSwordStar,
                    "旋转仙剑扫荡群敌，并召唤十把从天而降的巨剑",
                    "旋转杨桃剑扫荡群敌，并召唤从天而降的巨剑");
                ReplaceText.ReplaceAlmanac(PlantType.AbyssSwordStar,
                    "<color=red>攻击时将仙剑绕身旋转2圈，对接触的僵尸每0.02秒造成1次伤害。然后召唤10把全场索敌的巨剑，落地时造成范围伤害，可对空</color>",
                    "<color=red>每6秒释放一次攻击，攻击时对僵尸：\n" +
                    "·每0.02秒对3x3范围内的僵尸造成一次伤害\n" +
                    "·召唤10次巨剑（每次1柄），全屏索敌，落地时造成5次范围伤害</color>\n" +
                    "<color=#3D1400>词条1:</color><color=red>血淬剑芒：究极剑仙杨桃攻击力x3，对无防具目标伤害x5</color>\n" +
                    "<color=#3D1400>词条2:</color><color=red>无限剑制：究极剑仙杨桃召唤飞剑时有10%概率对场上每个僵尸召唤飞剑</color>");
            }
            #endregion
            #region 牢樱
            {
                ReplaceText.ReplaceAlmanac(PlantType.UltimateGatling,
                    "植物方的樱桃爆炸伤害×3（樱桃炸弹除外）",
                    "植物方的樱桃爆炸伤害×3，二级时×10，且伤害不低于300（均不包括樱桃炸弹）");
            }
            #endregion
            #region 黑高
            {
                // 世纪之盾
                ReplaceText.ReplaceAlmanac(PlantType.UltimateTallNut, 
                    "韧性×3", 
                    "韧性×3，二级时×9");
            }
            #endregion
        }

        public static void InitCardClick()
        {
            #region 曾哥&牢灯
            {
                CardClickMgr.AddCardClickOnPlant(PlantType.DoomShroom, PlantType.UltimateGloom, (plant) =>
                {
                    plant.GetComponent<UltimateGloom>().DieEvent();
                }, () => CoreTools.TravelUltimate("万籁俱寂"));
                CardClickMgr.AddCardClickOnPlant(PlantType.DoomShroom, PlantType.UltimatePlantern, (plant) =>
                {
                    var lantern = plant.GetComponent<UltimatePlantern>();
                    var light = UnityEngine.Object.Instantiate(lantern.lanternLight, lantern.lanternLight.transform.position, 
                        Quaternion.identity, lantern.lanternLight.transform.parent);
                    lantern.DieEvent();
                    lantern.lanternLight = light;
                }, () => CoreTools.TravelUltimate("万籁俱寂"));
            }
            #endregion
        }

        public static void InitDataExtra()
        {
            #region 曾哥&牢灯
            {
                PlantDataMenuExtra.AddExtra(PlantType.UltimateGloom, "储存伤害：{0}", () => CoreTools.TravelUltimate("以爆制爆"),
                    (p) => p.GetOrAddComponent<DataSave>().GetData<int>("UltimateGloom_TotalDamage"));
                PlantDataMenuExtra.AddExtra(PlantType.UltimatePlantern, "储存伤害：{0}", () => CoreTools.TravelUltimate("以爆制爆"),
                    (p) => p.GetOrAddComponent<DataSave>().GetData<int>("UltimatePlantern_TotalDamage"));
            }
            #endregion
        }

        public static void InitLevelBuff()
        {
            #region 牢樱
            {
                // 力大砖飞
                BuffMgr.RegisterCustomLevelBuff(CoreTools.GetUltiBuffByString("力大砖飞"));
            }
            #endregion
            #region 黑高
            {
                // 世盾
                BuffMgr.RegisterCustomLevelBuff(CoreTools.GetUltiBuffByString("世纪之盾"));
            }
            #endregion
        }

        public static void InitCustomBuff()
        {
            #region 剑仙
            {
                BuffMgr.RegisterCustomAdvBuff("血淬剑芒：究极剑仙杨桃攻击力x3，对无防具目标伤害x5", () => CoreTools.TravelAdvanced("诸神黄昏"),
                    5000, PlantType.AbyssSwordStar);
                BuffMgr.RegisterCustomAdvBuff("无限剑制：究极剑仙杨桃召唤飞剑时有10%概率对场上每个僵尸召唤飞剑", () => CoreTools.TravelAdvanced("诸神黄昏"),
                    5000, PlantType.AbyssSwordStar);
            }
            #endregion
        }
    }

    #region 修改文本
    public static class ReplaceText
    {
        #region 字段
        public static bool loadAlmanac = false;
        public static bool loadBuff = false;

        public static Dictionary<PlantType, String> AlmanacStrings = new();
        public static List<(PlantType, String, String)> ReplaceAlmanacStrings = new();

        public static Dictionary<(BuffType, int), String> BuffStrings = new();
        public static List<((BuffType, int), String, String)> ReplaceBuffStrings = new();
        #endregion

        /// <summary>
        /// 替换图鉴文本
        /// </summary>
        /// <param name="almanacType">植物类型</param>
        /// <param name="origin">原来的文本</param>
        /// <param name="replace">替换后的文本</param>
        public static void ReplaceAlmanac(PlantType almanacType, String origin, String replace) =>
            ReplaceAlmanacStrings.Add((almanacType, origin, replace));

        /// <summary>
        /// 替换词条文本
        /// </summary>
        /// <param name="plantType">词条对应的植物类型</param>
        /// <param name="type">词条类型</param>
        /// <param name="index">词条ID</param>
        /// <param name="origin">原来的文本</param>
        /// <param name="replace">替换后的文本</param>
        public static void ReplaceBuff(BuffType type, int index, String origin, String replace) =>
            ReplaceBuffStrings.Add(((type, index), origin, replace));

        public static void InitAlmanacText()
        {
            foreach (var (almanacType, origin, replace) in ReplaceAlmanacStrings)
            {
                var almanac = AlmanacDataLoader.plantDatas[almanacType];
                var newStr = almanac.info.Replace(origin, replace);
                almanac.info = newStr;
                AlmanacDataLoader.plantDatas[almanacType] = almanac;
            }
        }

        public static void InitBuffText()
        {
            foreach (var ((type, index), origin, replace) in ReplaceBuffStrings)
            {
                var oldStr = "";
                switch (type)
                {
                    case BuffType.AdvancedBuff:
                        oldStr = TravelDictionary.advancedBuffsText[(AdvBuff)index];
                        break;
                    case BuffType.UltimateBuff:
                        oldStr = TravelDictionary.ultimateBuffsText[(UltiBuff)index];
                        break;
                }

                var newStr = oldStr.Replace(origin, replace);
                BuffStrings[(type, index)] = replace;
            }
        }

        public static void InitAlmanac()
        {
            if (!loadAlmanac)
            {
                InitAlmanacText();
                loadAlmanac = true;
            }
            foreach (var (type, str) in AlmanacStrings)
            {
                var almanac = AlmanacDataLoader.plantDatas[type];
                almanac.info = str;
                AlmanacDataLoader.plantDatas[type] = almanac;
            }
        }

        public static void InitBuff()
        {
            if (!loadBuff)
            {
                InitBuffText();
                loadBuff = true;
            }
            foreach (var ((type, index), str) in BuffStrings)
            {
                switch (type)
                {
                    case BuffType.AdvancedBuff:
                        TravelDictionary.advancedBuffsText[(AdvBuff)index] = str;
                        break;
                    case BuffType.UltimateBuff:
                        TravelDictionary.ultimateBuffsText[(UltiBuff)index] = str;
                        break;
                }
            }
        }
    }
    #endregion

    #region 卡片点击
    public static class CardClickMgr
    {
        public static Dictionary<(PlantType, PlantType), (Func<bool>, Action<Plant>)> OnCardClick { get; set; } = new();

        public static void AddCardClickOnPlant(PlantType card, PlantType plant, Action<Plant> action, Func<bool> can) =>
            OnCardClick.Add((card, plant), (can, action));
    }
    #endregion

    #region 详细信息额外显示
    public static class PlantDataMenuExtra
    {
        public static Dictionary<PlantType, (string, Func<bool>, Func<Plant, object>)> ExtraData { get; set; } = new();

        public static void AddExtra(PlantType plantType, string format, Func<bool> show, Func<Plant, object> datas) =>
            ExtraData.Add(plantType, (format, show, datas));
    }
    #endregion

    #region 词条
    public static class BuffMgr
    {
        public const int CustomBuffStartID = 25000;
        public static Dictionary<int, (string, Func<bool>, int, PlantType)> CustomAdvBuffs = new(); // (id, (text, unlock, cost, icon))
        public static List<UltiBuff> CustomLevelBuffs = new();

        public static void RegisterCustomLevelBuff(UltiBuff id) => CustomLevelBuffs.Add(id);

        public static int RegisterCustomAdvBuff(string text, Func<bool> unlock, int cost, PlantType icon)
        {
            var i = CustomBuffStartID + CustomAdvBuffs.Count;
            CustomAdvBuffs.Add(i, (text, unlock, cost, icon));
            TravelDictionary.advancedBuffsText.Add((AdvBuff)i, text);
            TravelDictionary.AdvBuffPlantPairs.Add((AdvBuff)i, icon);
            return i;
        }
    }
    #endregion

    #region 基础
    [HarmonyPatch(typeof(AlmanacDataLoader))]
    public static class AlmanacDataLoaderPatch
    {
        [HarmonyPatch(nameof(AlmanacDataLoader.LoadPlantData))]
        [HarmonyPostfix]
        public static void PostLoadPlantData()
        {
            ReplaceText.InitAlmanac();
        }
    }
    
    [HarmonyPatch(typeof(TravelLookMenu))]
    public static class TravelLookMenuPatch
    {
        [HarmonyPatch(nameof(TravelLookMenu.Start))]
        [HarmonyPostfix]
        public static void PostStart()
        {
            ReplaceText.InitBuff();
        }

        [HarmonyPatch(nameof(TravelLookMenu.GetAdvBuffs))]
        [HarmonyPostfix]
        public static void PostGetAdvBuffs(TravelLookMenu __instance, ref Il2CppSystem.Collections.Generic.List<AdvBuff> __result)
        {
            if (BuffMgr.CustomAdvBuffs.Count <= 0)
                return;
            foreach (var (id, _) in BuffMgr.CustomAdvBuffs)
                if (__instance.showAll)
                    __result.Add((AdvBuff)id);
        }
    }

    [HarmonyPatch(typeof(TravelMgr))]
    public static class TravelMgrPatch
    {
        [HarmonyPatch(nameof(TravelMgr.GetAdvancedBuffPool))]
        [HarmonyPostfix]
        public static void PostGetAdvancedBuffPool(ref Il2CppSystem.Collections.Generic.List<AdvBuff> __result)
        {
            foreach (var (key, value) in BuffMgr.CustomAdvBuffs)
            {
                if (value.Item2.Invoke() && !TravelMgr.Instance.data.advBuffs.Contains((AdvBuff)key))
                    __result.Add((AdvBuff)key);
            }
        }

        [HarmonyPatch(nameof(TravelMgr.GetText))]
        [HarmonyPostfix]
        public static void PostGetText(Il2CppSystem.Object buff, ref string __result)
        {
            var (type, id) = buff.DestructBuffObject();
            if (type == BuffType.AdvancedBuff && BuffMgr.CustomAdvBuffs.ContainsKey(id))
                __result = BuffMgr.CustomAdvBuffs[id].Item1;
        }
    }

    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPatch(nameof(Mouse.TryToSetPlantByCard))]
        [HarmonyPostfix]
        public static void PostTryToSetPlantByCard(Mouse __instance)
        {
            if (__instance.theCardOnMouse != null)
            {
                Plant? targetPlant = null;
                foreach (var plant in Lawnf.Get1x1Plants(__instance.theMouseColumn, __instance.theMouseRow))
                {
                    if (plant == null || plant.IsDestroyed() || plant.gameObject == null || plant.gameObject.IsDestroyed()) continue;
                    if (CardClickMgr.OnCardClick.ContainsKey((__instance.theCardOnMouse.thePlantType, plant.thePlantType)))
                    {
                        targetPlant = plant;
                        break;
                    }
                }
                if (targetPlant == null) return;
                if (CardClickMgr.OnCardClick[(__instance.theCardOnMouse.thePlantType, targetPlant.thePlantType)].Item1.Invoke())
                {
                    CardClickMgr.OnCardClick[(__instance.theCardOnMouse.thePlantType, targetPlant.thePlantType)].Item2.Invoke(targetPlant);
                    __instance.board.UseSun(__instance.theCardOnMouse.theSeedCost);

                    if (CoreTools.TravelAdvanced("贪婪诅咒"))
                        __instance.board.UseSun(__instance.board.theSun / 2);

                    __instance.theCardOnMouse.CD = 0f;
                    __instance.theCardOnMouse.isPickUp = false;

                    UnityEngine.Object.Destroy(__instance.theItemOnMouse);
                    __instance.ClearItemOnMouse(false);
                }
            }
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
                && PlantDataMenuExtra.ExtraData.ContainsKey(__instance.plant.thePlantType))
            {
                var value = PlantDataMenuExtra.ExtraData[__instance.plant.thePlantType];
                if (value.Item2.Invoke())
                {
                    var str = string.Format(value.Item1, value.Item3.Invoke(__instance.plant));
                    foreach (var text in __instance.infoText)
                        text.text += str;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameAPP))]
    public static class GameAPPPatch
    {
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPostfix]
        public static void PostStart(GameAPP __instance)
        {
            Core.Init();
            __instance.AddComponent<GameInfo>();
            ReplaceText.InitBuff();
        }
    }

    [HarmonyPatch(typeof(TravelLookBuff))]
    public static class TravelLookBuffPatch
    {
        [HarmonyPatch(nameof(TravelLookBuff.OnMouseUpAsButton))]
        [HarmonyPrefix]
        public static bool PreOnMouseUpAsButton(TravelLookBuff __instance)
        {
            if (TravelLookMenu.Instance != null && !TravelLookMenu.Instance.showAll)
            {
                if (__instance.buff.GetIl2CppType() == Il2CppType.From(typeof(UltiBuff)))
                {
                    var buff = __instance.buff.Unbox<UltiBuff>();
                    if (BuffMgr.CustomLevelBuffs.Contains(buff) && CoreTools.TravelAdvanced("升级") && __instance.manager.data.GetBuffLevel(buff) < 2)
                    {
                        __instance.manager.GetUltiBuff(buff, true);
                        __instance.manager.data.advBuffs.Remove(CoreTools.GetAdvBuffByString("升级"));
                        __instance.SetText("已满级");
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(TravelLookBuff.SetBuff))]
        [HarmonyPostfix]
        public static void PostSetBuff(TravelLookBuff __instance, Il2CppSystem.Object buff)
        {
            if (TravelLookMenu.Instance != null && !TravelLookMenu.Instance.showAll)
            {
                if (__instance.buff.GetIl2CppType() == Il2CppType.From(typeof(UltiBuff)))
                {
                    var id = __instance.buff.Unbox<UltiBuff>();
                    if (BuffMgr.CustomLevelBuffs.Contains(id))
                        if (__instance.manager.data.GetBuffLevel(id) < 2)
                            __instance.SetText($"{__instance.manager.data.GetBuffLevel(id)}级");
                        else
                            __instance.SetText("已满级");
                }
            }
        }
    }
    #endregion
}
