using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Ext.SystemExt
{
    public static partial class SystemExtensions
    {
        public static bool AddErrorIfDup<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, string message)
        {
            if (dictionary.ContainsKey(key))
            {
                CustomCore.CLogger.LogError(message);
                return false;
            }
            dictionary.Add(key, value);
            return true;
        }
    }
}
