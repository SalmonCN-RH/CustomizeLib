using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
#pragma warning disable
namespace CustomizeLib.BepInEx
{
    public static class Tools
    {
        public static Assembly GetAssembly() => Assembly.GetCallingAssembly();
        public static Assembly Assembly
        {
            get
            {
                return Assembly.GetCallingAssembly();
            }
        }

        public static void InitMod(bool skipRegister = false) => InitMod(Assembly.GetCallingAssembly(), skipRegister);

        public static void InitMod(Assembly assembly, bool skipRegister = false)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (!skipRegister)
            {
                var types = GetAllMonoBehaviourTypes(assembly);
                foreach (var type in types)
                    if (!ClassInjector.IsTypeRegisteredInIl2Cpp(type))
                        ClassInjector.RegisterTypeInIl2Cpp(type);
            }
            Harmony.CreateAndPatchAll(assembly);
        }

        public static Type[] GetAllMonoBehaviourTypes(Assembly assembly)
        {
            try
            {
                // 获取所有类型
                Type[] allTypes = assembly.GetTypes();

                // 筛选继承自MonoBehaviour的类型
                return allTypes
                    .Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) &&
                                  !type.IsAbstract &&
                                  !type.IsInterface)
                    .ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 处理类型加载异常
                return ex.Types
                    .Where(type => type != null &&
                                  typeof(MonoBehaviour).IsAssignableFrom(type) &&
                                  !type.IsAbstract &&
                                  !type.IsInterface)
                    .ToArray();
            }
        }
    }
    public struct ID
    {
        public int id = 0;
        public ID(int id) { this.id = id; }
        public ID(PlantType id) { this.id = (int)id; }
        public ID(ZombieType id) { this.id = (int)id; }
        public ID(ParticleType id) { this.id = (int)id; }
        public ID(BulletType id) { this.id = (int)id; }
        public ID(CherryBombType id) { this.id = (int)id; }
        public ID(MusicType id) { this.id = (int)id; }
        public ID(EffectType id) { this.id = (int)id; }
        public ID(Zombie.ZombieColor id) {  this.id = (int)id; }
        public static implicit operator int(ID id) => id.id;
        public static implicit operator PlantType(ID id) => (PlantType)id.id;
        public static implicit operator ZombieType(ID id) => (ZombieType)id.id;
        public static implicit operator ParticleType(ID id) => (ParticleType)id.id;
        public static implicit operator BulletType(ID id) => (BulletType)id.id;
        public static implicit operator CherryBombType(ID id) => (CherryBombType)id.id;
        public static implicit operator MusicType(ID id) => (MusicType)id.id;
        public static implicit operator EffectType(ID id) => (EffectType)id.id;
        public static implicit operator Zombie.ZombieColor(ID id) => (Zombie.ZombieColor)id.id;

        public static implicit operator ID(int i) => new ID(i);
        public static implicit operator ID(PlantType id) => new ID(id);
        public static implicit operator ID(ZombieType id) => new ID(id);
        public static implicit operator ID(ParticleType id) => new ID(id);
        public static implicit operator ID(BulletType id) => new ID(id);
        public static implicit operator ID(CherryBombType id) => new ID(id);
        public static implicit operator ID(MusicType id) => new ID(id);
        public static implicit operator ID(EffectType id) => new ID(id);
        public static implicit operator ID(Zombie.ZombieColor id) => new ID(id);

        public override string ToString()
        {
            return id.ToString();
        }
    }

    public struct BuffID
    {
        public int id = 0;
        public BuffID(int id) { this.id = id; }
        public BuffID(AdvBuff id) { this.id = (int)id; }
        public BuffID(UltiBuff id) { this.id = (int)id; }
        public BuffID(TravelDebuff id) { this.id = (int)id; }
        public BuffID(TravelUnlocks id) { this.id = (int)id; }

        public static implicit operator AdvBuff(BuffID id) => (AdvBuff)id.id;
        public static implicit operator UltiBuff(BuffID id) => (UltiBuff)id.id;
        public static implicit operator TravelDebuff(BuffID id) => (TravelDebuff)id.id;
        public static implicit operator TravelUnlocks(BuffID id) => (TravelUnlocks)id.id;
        public static implicit operator int(BuffID id) => id.id;
        public static implicit operator BuffID(AdvBuff i) => new BuffID(i);
        public static implicit operator BuffID(UltiBuff id) => new BuffID(id);
        public static implicit operator BuffID(TravelDebuff id) => new BuffID(id);
        public static implicit operator BuffID(TravelUnlocks id) => new BuffID(id);
        public static implicit operator BuffID(int id) => new BuffID(id);
    }

    public class CorePlugin : BasePlugin
    {
        public static List<Action> OnGameInitAction = new();

        public ManualLogSource Logger;

        public override void Load()
        {
            Logger = base.Log;
            Tools.InitMod(GetType().Assembly);
            OnStart();
            OnGameInitAction.Add(OnGameInit);
        }

        public virtual void OnStart() { }

        public virtual void OnGameInit() { }
    }

    public class EmptyDie : MonoBehaviour
    {
        public void Die() => Destroy(gameObject);
    }

    public static class CoreTools
    {
        public static float ColumnX
        {
            get
            {
                if (Mouse.Instance != null && !Mouse.Instance.IsDestroyed())
                    return Mouse.Instance.GetBoxXFromColumn(1) - Mouse.Instance.GetBoxXFromColumn(0);
                return 1.35f;
            }
        }
        public static float RowY
        {
            get
            {
                if (Mouse.Instance != null && !Mouse.Instance.IsDestroyed())
                    return Mouse.Instance.GetBoxYFromRow(1) - Mouse.Instance.GetBoxYFromRow(0);
                return 1.67f;
            }
        }

        public static Dictionary<string, AdvBuff> AdvBuffPair = new();
        public static Dictionary<string, UltiBuff> UltiBuffPair = new();
        public static Dictionary<string, TravelDebuff> DebuffBuffPair = new();

        public static void InitBuffDic()
        {
            TravelDictionary.advancedBuffsText = new();
            TravelDictionary.AdvBuffPlantPairs = new();
            TravelDictionary.allStrongUltimtePlant = new();
            TravelDictionary.CurseBuffs = new();
            TravelDictionary.debuffData = new();
            TravelDictionary.PlantInfo = new();
            TravelDictionary.RandomBuffs = new();
            TravelDictionary.RogueBuffs = new();
            TravelDictionary.ultimateBuffsText = new();
            TravelDictionary.unlocksText = new();
        }

        public static IEnumerator Init()
        {
            while (TravelDictionary.advancedBuffsText.Count < Enum.GetValues<AdvBuff>().Length) yield return new WaitForSeconds(1f);
            foreach (var (buff, str) in TravelDictionary.advancedBuffsText)
            {
                int index = str.IndexOf('：');
                if (index == -1)
                    index = str.IndexOf(":");
                if (index != -1)
                {
                    if (!AdvBuffPair.ContainsKey(str.Substring(0, index)))
                        AdvBuffPair.Add(str.Substring(0, index), buff);
                }
            }

            while (TravelDictionary.ultimateBuffsText.Count < Enum.GetValues<UltiBuff>().Length) yield return new WaitForSeconds(1f);
            foreach (var (buff, str) in TravelDictionary.ultimateBuffsText)
            {
                int index = str.IndexOf('：');
                if (index == -1)
                    index = str.IndexOf(":");
                if (index != -1)
                {
                    if (!UltiBuffPair.ContainsKey(str.Substring(0, index)))
                        UltiBuffPair.Add(str.Substring(0, index), buff);
                }
            }

            while (TravelDictionary.ultimateBuffsText.Count < Enum.GetValues<TravelDebuff>().Length) yield return new WaitForSeconds(1f);
            foreach (var (buff, tuple) in TravelDictionary.debuffData)
            {
                var str = tuple.Item1;
                int index = str.IndexOf('：');
                if (index == -1)
                    index = str.IndexOf(":");
                if (index != -1)
                {
                    if (!DebuffBuffPair.ContainsKey(str.Substring(0, index)))
                        DebuffBuffPair.Add(str.Substring(0, index), buff);
                }
            }
            yield break;
        }

        /// <summary>
        /// 根据词条名称获取普通词条,请在GameAPP实例化后调用
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AdvBuff GetAdvBuffByString(string name)
        {
            if (AdvBuffPair.ContainsKey(name))
                return AdvBuffPair[name];
            return (AdvBuff)(-1);
        }

        /// <summary>
        /// 根据词条名称获取究极词条,请在GameAPP实例化后调用
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static UltiBuff GetUltiBuffByString(string name)
        {
            if (UltiBuffPair.ContainsKey(name))
                return UltiBuffPair[name];
            return (UltiBuff)(-1);
        }


        public static TravelDebuff GetTravelDebuffByString(string name)
        {
            if (DebuffBuffPair.ContainsKey(name))
                return DebuffBuffPair[name];
            return (TravelDebuff)(-1);
        }

        public static TravelUnlocks GetTravelUnlocksByString(string name)
        {
            var id = -1;
            #region 映射
            switch (name)
            {
                case "UltimateChomper":
                    id = 0;
                    break;
                case "UltimateGatling":
                    id = 1;
                    break;
                case "UltimateFume":
                    id = 2;
                    break;
                case "UltimateTorch":
                    id = 3;
                    break;
                case "UltimateStar":
                    id = 4;
                    break;
                case "UltimateGloom":
                    id = 5;
                    break;
                case "UltimateMelon":
                    id = 6;
                    break;
                case "UltimateCannon":
                    id = 7;
                    break;
                case "UltimateTallNut":
                    id = 8;
                    break;
                case "UltimateHypno":
                    id = 9;
                    break;
                case "UltimateBigGatling":
                    id = 10;
                    break;
                case "UltimateCabbage":
                    id = 11;
                    break;
                case "UltimatePumpkin":
                    id = 12;
                    break;
                case "UltimateSpring":
                    id = 13;
                    break;
                case "UltimateKelp":
                    id = 14;
                    break;
                case "UltimateCorn":
                    id = 15;
                    break;
                case "UltimateSpruce":
                    id = 16;
                    break;
                case "UltimateBigChomper":
                    id = 17;
                    break;
                case "UltimateExplodeCannon":
                    id = 18;
                    break;
                case "UltimateSunflower":
                    id = 19;
                    break;
                case "UltimateWinterMelon":
                    id = 20;
                    break;
                case "UltimateCattail":
                    id = 21;
                    break;
                case "UltimateSeaShroom":
                    id = 22;
                    break;
                case "UltimateJalapeno":
                    id = 23;
                    break;
            }
            #endregion
            return (TravelUnlocks)id;
        }

        public static bool TravelAdvanced(string name) => Lawnf.TravelAdvanced(GetAdvBuffByString(name));

        public static bool TravelUltimate(string name) => Lawnf.TravelUltimate(GetUltiBuffByString(name));
        public static int TravelUltimateLevel(string name) => Lawnf.TravelUltimateLevel(GetUltiBuffByString(name));
        public static bool TravelDebuff(string name) => Lawnf.TravelDebuff(GetTravelDebuffByString(name));

        public static GameObject CreateCherryExplodeCustom(Vector2 v, int theRow, CherryBombType bombType = CherryBombType.Normal,
            int damage = 1800, PlantType fromType = PlantType.Nothing, Il2CppSystem.Action<Zombie> action = null, bool immediately = true,
            bool shake = true)
        {
            var particle = CreateParticle.SetParticle(CustomCore.CustomCherryStartID + (int)bombType, v, 11);
            if (shake)
                ScreenShake.TriggerShake(0.15f);
            GameAPP.PlaySound(40, 0.5f, 1.0f);

            BombCherry cherry = new BombCherry();
            cherry.board = Board.Instance;
            cherry.damageToZombie = damage;
            cherry.bombRow = theRow;
            cherry.bombType = bombType;
            cherry.zombieAction = action;
            cherry.bombPosition = v;
            cherry.fromType = fromType;
            cherry.targetPlant = null;

            if (immediately)
            {
                cherry.Explode(CustomDamageMaker.DamageMaker);
            }

            return particle;
        }

        /// <summary>
        /// 创建樱桃爆炸
        /// </summary>
        /// <param name="v">位置</param>
        /// <param name="theRow">行</param>
        /// <param name="bombType">樱桃爆炸类型</param>
        /// <param name="damage">伤害</param>
        /// <param name="fromType">来源</param>
        /// <param name="action">僵尸操作</param>
        /// <param name="immediately">是否立即引爆</param>
        /// <param name="shake">是否晃动屏幕</param>
        /// <param name="soundID">音效ID</param>
        /// <param name="volume">音量(子弹填0.2f)</param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public static (BombCherry, GameObject) CreateCherryExplode(Vector2 v, int theRow, CherryBombType bombType = CherryBombType.Normal,
            int damage = 1800, PlantType fromType = PlantType.Nothing, Il2CppSystem.Action<Zombie> action = null, bool immediately = true,
            bool shake = true, int soundID = 40, float volume = 0.5f, float pitch = 1f)
        {
            if (Board.Instance == null || Board.Instance.IsDestroyed()) return (null, null);
            GameObject particle = null;

            if (CustomCore.CustomCherrys.ContainsKey(bombType))
            {
                particle = CreateParticle.SetParticle(CustomCore.CustomCherryStartID + (int)bombType, v, 11);
                GameAPP.PlaySound(soundID, volume, pitch);
            }
            else
            {
                if (bombType == CherryBombType.Sun)
                {
                    particle = CreateParticle.SetParticle((int)ParticleType.SunBombCloud, v, 11).gameObject;
                }
                else if (bombType == CherryBombType.Bullet || bombType == CherryBombType.BulletAll)
                {
                    particle = CreateParticle.SetParticle((int)ParticleType.BombCloudSmall, v, 11).gameObject;
                }
                else if (bombType == CherryBombType.IceCharry)
                {
                    particle = CreateParticle.SetParticle((int)ParticleType.BombCloud_blue, v, 11).gameObject;
                }
                else
                {
                    particle = CreateParticle.SetParticle((int)ParticleType.BombCloud, v, 11).gameObject;
                }
                GameAPP.PlaySound(soundID, volume, pitch);
            }

            if (shake)
                ScreenShake.TriggerShake(0.15f);

            // 创建樱桃炸弹对象
            BombCherry cherryExplode = new BombCherry();

            // 设置炸弹属性
            cherryExplode.board = Board.Instance;
            cherryExplode.damageToZombie = damage;
            cherryExplode.bombRow = theRow;
            cherryExplode.bombType = bombType;
            cherryExplode.zombieAction = action;
            cherryExplode.bombPosition = v;
            cherryExplode.fromType = fromType;
            cherryExplode.targetPlant = null;

            // 如果设置为立即爆炸，则直接引爆
            if (immediately)
            {
                cherryExplode.Explode(CustomDamageMaker.DamageMaker);
            }

            return (cherryExplode, particle);
        }

        public static bool IsObjExist(this Component component) => !(component == null || component.IsDestroyed() || component.gameObject == null || component.gameObject.IsDestroyed());
        public static bool IsObjExist(this GameObject gameObject) => !(gameObject == null || gameObject.IsDestroyed());
        public static int GetStarProbability()
        {
            int total = 0;
            var config = GameAPP.config;
            if (config.levelZombieInRandom) total += 2;
            if (config.strongUltiZombieInRandom) total += 2;
            if (config.leaderInRandom) total += 6;
            return total;
        }
        public static string FormatAlmanac(string input) => StringFormatter.Format(input);

        public static Il2CppSystem.Collections.Generic.List<Zombie> GetAllZombies(bool mindControl)
        {
            if (mindControl) return Board.Instance?.zombieArray;
            else return Lawnf.GetAllZombies();
        }
    }

    public class StringFormatter
    {
        public static readonly HashSet<char> NumberCharset = new HashSet<char>
        {
            '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩'
        };  

        /// <summary>
        /// 格式化输入的字符串
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 按行分割，保留空行
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                bool isLastLine = (i == lines.Length - 1);

                if (i == 0)
                {
                    // 第一行不做任何操作
                    result.Append(line);
                }
                else if (isLastLine)
                {
                    // 最后一行特殊处理
                    result.Append(ProcessLastLine(line));
                }
                else
                {
                    // 中间行（第2行到倒数第2行）按常规规则处理
                    result.Append(ProcessNormalLine(line));
                }

                // 添加换行符（除了最后一行）
                if (i != lines.Length - 1)
                    result.Append(Environment.NewLine);
            }

            return result.ToString();
        }

        /// <summary>
        /// 处理普通行（不是第一行也不是最后一行）
        /// </summary>
        private static string ProcessNormalLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return line; // 空行保持不变

            // 1. 检查是否包含序号
            int indexOfNumber = FindFirstSerialNumber(line);
            if (indexOfNumber != -1)
            {
                // 序号规则：行首到序号（含序号）为棕色，序号后为红色，行尾闭合红色
                return $"<color=#3D1400>{line.Substring(0, indexOfNumber + 1)}</color><color=red>{line.Substring(indexOfNumber + 1)}</color>";
            }

            // 2. 检查中文冒号 或 特殊英文冒号（词条{integer}:）
            int colonIndex = FindFirstColon(line);
            if (colonIndex != -1)
            {
                // 冒号规则：行首到冒号（含冒号）为棕色，冒号后为红色，行尾闭合红色
                return $"<color=#3D1400>{line.Substring(0, colonIndex + 1)}</color><color=red>{line.Substring(colonIndex + 1)}</color>";
            }

            // 3. 无特殊标记，保持原样
            return line;
        }

        /// <summary>
        /// 处理最后一行
        /// </summary>
        private static string ProcessLastLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return line;

            // 最后一行：开头加<color=#3D1400>，末尾加</color>
            return $"<color=#3D1400>{line}</color>";
        }

        /// <summary>
        /// 查找第一个序号的位置
        /// </summary>
        private static int FindFirstSerialNumber(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (NumberCharset.Contains(line[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 查找第一个可处理的冒号（中文冒号，或符合“词条{integer}:”的英文冒号）
        /// </summary>
        /// <summary>
        /// 查找第一个有效冒号的位置（从左到右）
        /// </summary>
        private static int FindFirstColon(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '：') // 中文冒号直接匹配
                {
                    return i;
                }
                if (c == ':') // 英文冒号需要验证前置条件
                {
                    // 检查冒号前面是否正好是“词条{integer}”
                    string prefix = line.Substring(0, i);
                    if (Regex.IsMatch(prefix, @"词条\d+$"))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 判断英文冒号是否满足特殊条件（前面是“词条{integer}”）
        /// </summary>
        private static bool IsSpecialEnglishColon(string line, int colonIndex)
        {
            if (colonIndex <= 0)
                return false;

            string prefix = line.Substring(0, colonIndex);
            // 匹配模式：词条后跟一个或多个数字（允许数字前后无空格）
            return Regex.IsMatch(prefix, @"词条\d+$");
        }
    }
}
