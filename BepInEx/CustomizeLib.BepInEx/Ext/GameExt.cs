using CustomizeLib.BepInEx.ExtensionData.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    public static class GameExtensions
    {
        public static void Explode(this BombCherry cherry) => cherry.Explode(CustomDamageMaker.DamageMaker);

        public static void FindCardUIAndChangeSprite(this Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                CardUI cardUI = parent.GetChild(i).GetComponent<CardUI>();
                if (cardUI != null)
                {
                    Mouse.Instance.ChangeCardSprite((PlantType)cardUI.theSeedType, cardUI);
                }

                // 递归查找子物体的子物体
                FindCardUIAndChangeSprite(parent.GetChild(i));
            }
        }
        public static bool ObjectExist<T>(this Board board) => board.GameObject().transform.GetComponentsInChildren<T>().Length > 0;
        public static BoardGrid GetGrid(this Board board, int column, int row) => board.gridSystem.GetGrid(column, row);
    }

    public static class TravelExtensions
    {
        public const string BUFF_TYPEDATA = "CustomizeLib_BuffOptionType";
        public const string BUFF_IDDATA = "CustomizeLib_BuffOptionID";

        /// <summary>
        /// 获取TravelBuffOptionButton的类型和ID信息
        /// </summary>
        /// <param name="option"></param>
        /// <returns>(类型, ID)</returns>
        /// <exception cref="InvalidOperationException">TravelBuffOptionButton实例未被设置类型及ID信息</exception>
        public static (BuffType, int) TryGetTypeAndID(this TravelBuffOptionButton option)
        {
            if (option.GetData(BUFF_TYPEDATA) != null && option.GetData(BUFF_IDDATA) != null)
                return (option.GetData<BuffType>(BUFF_TYPEDATA), option.GetData<int>(BUFF_IDDATA));
            throw new InvalidOperationException("Option data is not exist");
        }

        /// <summary>
        /// 设置TravelBuffOptionButton的类型和ID信息，仅为TryGetData提供数据
        /// </summary>
        /// <param name="option"></param>
        /// <param name="type">类型</param>
        /// <param name="id">ID</param>
        public static void SetTypeAndID(this TravelBuffOptionButton option, BuffType type, int id)
        {
            option.SetData(BUFF_TYPEDATA, type);
            option.SetData(BUFF_IDDATA, id);
        }

        /// <summary>
        /// 获取TravelStoreWindow的类型和ID信息
        /// </summary>
        /// <param name="option"></param>
        /// <returns>(类型, ID)</returns>
        /// <exception cref="InvalidOperationException">TravelStoreWindow实例未被设置类型及ID信息</exception>
        public static (BuffType, int) TryGetTypeAndID(this TravelStoreWindow option)
        {
            if (option.GetData(BUFF_TYPEDATA) != null && option.GetData(BUFF_IDDATA) != null)
                return (option.GetData<BuffType>(BUFF_TYPEDATA), option.GetData<int>(BUFF_IDDATA));
            throw new InvalidOperationException("Option data is not exist");
        }

        /// <summary>
        /// 设置TravelStoreWindow的类型和ID信息，仅为TryGetData提供数据
        /// </summary>
        /// <param name="option"></param>
        /// <param name="type">类型</param>
        /// <param name="id">ID</param>
        public static void SetTypeAndID(this TravelStoreWindow option, BuffType type, int id)
        {
            option.SetData(BUFF_TYPEDATA, type);
            option.SetData(BUFF_IDDATA, id);
        }

        public static (BuffType, int) GetTypeAndID(Il2CppSystem.Object buff)
        {
            BuffType buffType = (BuffType)(-1);
            int id = -1;
            if (buff.IsTypeOf<AdvBuff>())
            {
                buffType = BuffType.AdvancedBuff;
                id = (int)buff.Unbox<AdvBuff>();
            }
            else if (buff.IsTypeOf<UltiBuff>())
            {
                buffType = BuffType.UltimateBuff;
                id = (int)buff.Unbox<UltiBuff>();
            }
            else if (buff.IsTypeOf<TravelDebuff>())
            {
                buffType = BuffType.Debuff;
                id = (int)buff.Unbox<TravelDebuff>();
            }
            else if (buff.IsTypeOf<InvestBuff>())
            {
                buffType = BuffType.InvestmentBuff;
                id = (int)buff.Unbox<InvestBuff>();
            }
            else if (buff.IsTypeOf<TravelUnlocks>())
            {
                buffType = BuffType.UnlockPlant;
                id = (int)buff.Unbox<TravelUnlocks>();
            }
            return (buffType, id);
        }

        public static (BuffType, int) GeneralSet(this TravelBuffOptionButton option, Il2CppSystem.Object buff)
        {
            var tuple = GetTypeAndID(buff);
            option.SetTypeAndID(tuple.Item1, tuple.Item2);
            return tuple;
        }

        public static (BuffType, int) GeneralSet(this TravelStoreWindow window, Il2CppSystem.Object buff)
        {
            var tuple = GetTypeAndID(buff);
            window.SetTypeAndID(tuple.Item1, tuple.Item2);
            return tuple;
        }

        public static Type GetBuffType(BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.AdvancedBuff: return typeof(AdvBuff);
                case BuffType.UltimateBuff: return typeof(UltiBuff);
                case BuffType.Debuff: return typeof(TravelDebuff);
                case BuffType.InvestmentBuff: return typeof(InvestBuff);
                case BuffType.UnlockPlant: return typeof(TravelUnlocks);
            }
            return null;
        }

        public static GameObject SetSaveMaterial(this GameObject gameObject)
        {
            gameObject.AddComponent<SaveMaterial>();
            return gameObject;
        }
    }

    public class SaveMaterial : MonoBehaviour { }
}
