using BepInEx.Logging;
using CustomizeLib.BepInEx.UnmanagedTools;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Internal
{
    internal static class HookCall
    {
        internal static bool load = false;

        internal static void SetBuffArr()
        {
            // advancedBuffsText
            var newAdvancedBuffsText = new Il2CppSystem.Collections.Generic.Dictionary<AdvBuff, string>();
            // 复制原来的值
            foreach (var item in TravelDictionary.advancedBuffsText)
                newAdvancedBuffsText[item.Key] = item.Value;
            // 复制新的值
            foreach (var item in CustomCore.CustomAdvancedBuffs)
                newAdvancedBuffsText[(AdvBuff)item.Key] = item.Value.Item2;
            // 复制引用
            TravelDictionary.advancedBuffsText = newAdvancedBuffsText;

            // AdvBuffPlantPairs
            var newAdvBuffPlantPairs = new Il2CppSystem.Collections.Generic.Dictionary<AdvBuff, PlantType>();
            foreach (var item in TravelDictionary.AdvBuffPlantPairs)
                newAdvBuffPlantPairs[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomAdvancedBuffs)
                newAdvBuffPlantPairs[(AdvBuff)item.Key] = item.Value.Item1;
            TravelDictionary.AdvBuffPlantPairs = newAdvBuffPlantPairs;

            // ultimateBuffsText
            var newUltimateBuffsText = new Il2CppSystem.Collections.Generic.Dictionary<UltiBuff, string>();
            foreach (var item in TravelDictionary.ultimateBuffsText)
                newUltimateBuffsText[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomUltimateBuffs)
                newUltimateBuffsText[(UltiBuff)item.Key] = item.Value.Item2;
            TravelDictionary.ultimateBuffsText = newUltimateBuffsText;

            // unlocksText
            var newUnlocksText = new Il2CppSystem.Collections.Generic.Dictionary<TravelUnlocks, string>();
            foreach (var item in TravelDictionary.unlocksText)
                newUnlocksText[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomUnlockBuffs)
                newUnlocksText[(TravelUnlocks)item.Key] = item.Value.Item2;
            TravelDictionary.unlocksText = newUnlocksText;

            // PlantToUnlock
            var newPlantToUnlock = new Il2CppSystem.Collections.Generic.Dictionary<PlantType, TravelUnlocks>();
            foreach (var item in TravelDictionary.PlantToUnlock)
                newPlantToUnlock[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomUnlockBuffs)
                newPlantToUnlock[item.Value.Item1] = (TravelUnlocks)item.Key;
            TravelDictionary.PlantToUnlock = newPlantToUnlock;

            // UnlockToPlant
            var newUnlockToPlant = new Il2CppSystem.Collections.Generic.Dictionary<TravelUnlocks, PlantType>();
            foreach (var item in TravelDictionary.UnlockToPlant)
                newUnlockToPlant[item.Key] = item.Value;
            foreach (var item in CustomCore.CustomUnlockBuffs)
                newUnlockToPlant[(TravelUnlocks)item.Key] = item.Value.Item1;
            TravelDictionary.UnlockToPlant = newUnlockToPlant;

            // debuffData
            var newDebuffData = new Il2CppSystem.Collections.Generic.Dictionary<TravelDebuff, Il2CppSystem.ValueTuple<string, ZombieType>>();
            foreach (var item in TravelDictionary.debuffData)
                newDebuffData.SetDictionaryItem(item.Key, new(item.Value.Pointer));
            foreach (var item in CustomCore.CustomDebuffs)
                newDebuffData.SetDictionaryItem((TravelDebuff)item.Key,
                    new Il2CppSystem.ValueTuple<string, ZombieType>(item.Value.Item1, item.Value.Item2));
            TravelDictionary.debuffData = newDebuffData;

            var newPlantInfo = new Il2CppSystem.Collections.Generic.Dictionary
                <PlantType, Il2CppSystem.ValueTuple<Il2CppSystem.Nullable<PlantType>, Il2CppSystem.Object, Il2CppSystem.Object, bool>>();
            foreach (var item in TravelDictionary.PlantInfo)
                newPlantInfo.SetDictionaryItem(item.Key, item.Value);
            foreach (var (key, value) in CustomCore.CustomPlantInfos)
            {
                Il2CppSystem.Nullable<PlantType> nullable = value.subType.HasValue ? new(value.subType.Value) : new();
                Il2CppSystem.Object buff1 = null!;
                if (value.buff1 != null) Il2CppExtensions.BoxEnumToIl2Object(value.buff1, value.buff1.GetType());
                Il2CppSystem.Object buff2 = null!;
                if (value.buff2 != null) Il2CppExtensions.BoxEnumToIl2Object(value.buff2, value.buff2.GetType());
                var strongUltimate = value.isStrongUltimate;
                var tuple = new Il2CppSystem.ValueTuple<Il2CppSystem.Nullable<PlantType>, Il2CppSystem.Object, Il2CppSystem.Object, bool>
                    (nullable, buff1, buff2, strongUltimate);
                newPlantInfo.SetDictionaryItem(key, tuple);
            }
        }

        internal static void RegisterTypes()
        {
            // 以备后用
        }
    }
}
