using CustomizeLib.BepInEx.ExtensionData.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    public static class MultiLevelBuff
    {
        /// <summary>
        /// 自定义词条是否是多级词条
        /// </summary>
        /// <param name="buffType">词条类型</param>
        /// <param name="returnID">对应的数组的ID（索引），即注册词条是返回的ID</param>
        /// <returns>(是否是多级词条, 在数据字典/数组中的索引)</returns>
        public static (bool, int) IsMultiLevelBuff(BuffType buffType, int returnID)
        {
            var data = GetBuffDataInDic(buffType, returnID);
            if (data != (-1, -1))
            {
                var index = data.Item1;
                return (data.Item2 > 0, index);
            }
            return (false, -1);
        }

        /// <summary>
        /// 获取词条等级
        /// </summary>
        public static int GetBuffCurrentLevel(BuffType buffType, int id)
        {
            var result = IsMultiLevelBuff(buffType, id);
            if (result.Item1)
            {
                if (TravelMgr.Instance is null)
                    return 0;
                var array = GetBuffArray();
                if (array is null) return 0;
                return array[result.Item2];
            }
            return 0;
        }

        /// <summary>
        /// 获取词条最大等级
        /// </summary>
        public static int GetBuffMaxLevel(BuffType buffType, int id) => GetBuffDataInDic(buffType, id).Item2;

        /// <summary>
        /// 获取词条在词条数据字典中的数据
        /// </summary>
        /// <returns>(列表的Index, 最大等级)</returns>
        public static (int, int) GetBuffDataInDic(BuffType buffType, int id)
        {
            if (CustomCore.CustomBuffsLevel.ContainsKey((buffType, id))) return CustomCore.CustomBuffsLevel[(buffType, id)];
            return (-1, -1);
        }

        /// <summary>
        /// 获取词条数组
        /// </summary>
        public static int[] GetBuffArray() => (int[])TravelMgr.Instance.GetData(LevelBuffData.LEVEL_BUFF_ARR);

        public static void AddBuffLevel(BuffType buffType, int buffID, int value = 1) =>
            SetBuffLevel(buffType, buffID, GetBuffCurrentLevel(buffType, buffID) + value);

        /// <summary>
        /// 设置词条等级
        /// </summary>
        public static void SetBuffLevel(BuffType buffType, int buffID, int value)
        {
            int level = value % (GetBuffMaxLevel(buffType, buffID) + 1);
            var data = TravelMgr.Instance.data;
            var array = GetBuffArray();
            var id = new BuffID(buffID); // 转换类型便于处理
            int index = IsMultiLevelBuff(buffType, buffID).Item2;
            array[index] = level;
            if (array[index] > GetBuffMaxLevel(buffType, buffID)) array[index] = 0;
            if (array[index] == 0) // 移除词条
                switch (buffType)
                {
                    case BuffType.AdvancedBuff:
                        if (data.advBuffs.Contains(id)) TravelMgr.Instance.data.advBuffs.Remove(id);
                        break;
                    case BuffType.UltimateBuff:
                        if (data.ultiBuffs.Contains(id)) data.ultiBuffs.Remove(id);
                        if (data.ultiBuffs_lv2.Contains(id)) data.ultiBuffs_lv2.Remove(id);
                        break;
                    case BuffType.Debuff:
                        if (data.travelDebuffs.Contains(id)) data.travelDebuffs.Add(id);
                        break;
                    case BuffType.UnlockPlant:
                        if (data.unlockedPlants.Contains(id)) data.unlockedPlants.Add(id);
                        break;
                }
            else // 设置词条
                switch (buffType)
                {
                    case BuffType.AdvancedBuff:
                        if (!data.advBuffs.Contains(id)) data.advBuffs.Add(id);
                        break;
                    case BuffType.UltimateBuff:
                        if (!data.ultiBuffs.Contains(id)) data.ultiBuffs.Add(id);
                        if (!data.ultiBuffs_lv2.Contains(id)) data.ultiBuffs_lv2.Add(id);
                        break;
                    case BuffType.Debuff:
                        if (!data.travelDebuffs.Contains(id)) data.travelDebuffs.Add(id);
                        break;
                    case BuffType.UnlockPlant:
                        if (!data.unlockedPlants.Contains(id)) data.unlockedPlants.Add(id);
                        break;
                    default:
                        break;
                }
            TravelMgr.Instance.SetData(LevelBuffData.LEVEL_BUFF_ARR, array);
        }

        /// <summary>
        /// 获取词条等级
        /// </summary>
        public static int TravelCustomBuffLevel(BuffType buffType, int returnID) => GetBuffCurrentLevel(buffType, returnID);

        /// <summary>
        /// 设置开关栏的文本
        /// </summary>
        public static void SetToolText(UIButton self, BuffType buffType, int id, bool have)
        {
            var text = self.GetComponentInChildren<TextMeshProUGUI>();
            int level = GetBuffCurrentLevel(buffType, id);
            if (text == null) return;
            if (have)
            {
                text.text = $"已开启（{level}级）";
                text.color = Color.green;
            }
            else
            {
                text.text = "已关闭";
                text.color = Color.white;
            }
        }
    }

    public static class LevelBuffData
    {
        public const string LEVEL_BUFF_ARR = "CustomBuffsLevel";
    }
}
