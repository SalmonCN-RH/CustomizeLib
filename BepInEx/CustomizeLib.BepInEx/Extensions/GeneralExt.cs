using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomizeLib.BepInEx
{
    public static class GeneralExtensions
    {
        public static void Swap<T>(ref T a, ref T b) =>
            (b, a) = (a, b);

        public static T GetRandomItem<T>(this IList<T> list) => list[UnityEngine.Random.RandomRangeInt(0, list.Count)];

        public static List<(T1, T2)> ToEnumList<T1, T2>(this List<(int, int)> list) where T1 : Enum where T2 : Enum
        {
            var result = new List<(T1, T2)>();
            foreach (var (v1, v2) in list)
                result.Add((v1.ToEnumVal<T1>(), v2.ToEnumVal<T2>()));
            return result;
        }

        public static List<(int, int)> ToIntegerList<T1, T2>(this List<(T1, T2)> list) where T1 : Enum where T2 : Enum =>
            [.. list.Select(tuple => (tuple.Item1.ToIntVal(), tuple.Item2.ToIntVal()))];

        public static T ToEnumVal<T>(this int value) where T : Enum => (T)Enum.ToObject(typeof(T), value);
        public static int ToIntVal<T>(this T value) where T : Enum => (int)Enum.ToObject(typeof(T), value);
        public static void AddOrUpdateValue<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic == null) return;
            if (dic.ContainsKey(key)) dic[key] = value;
            else dic.Add(key, value);
        }
        public static int SafeToInt(this object value)
        {
            if (value is int || value is long)
            {
                var val = Convert.ToInt64(value);
                if (val < int.MinValue) return int.MinValue;
                else if (val > int.MaxValue) return int.MaxValue;
                return (int)val;
            }
            throw new ArgumentException("value must be int or long");
        }
    }
}
