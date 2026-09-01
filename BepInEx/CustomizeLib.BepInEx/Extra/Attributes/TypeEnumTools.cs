using CustomizeLib.BepInEx.Extra.PlantExtra.IPlantEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Extra.Attributes
{
    public static class TypeEnumTools
    {
        public static Dictionary<TypeStruct.TypeEnum, Type> TypeMaps { get; set; } = new()
        {
            [TypeStruct.TypeEnum.OnClickData] = typeof(OnClickData),
            [TypeStruct.TypeEnum.SystemTypes] = typeof(SystemTypes)
        };

        public static Type GetTypeFromEnum(TypeStruct.TypeEnum typeEnum)
        {
            if (TypeMaps.TryGetValue(typeEnum, out var type)) return type;
            throw new KeyNotFoundException($"Not found type map of enum value {typeEnum}");
        }

        public static void AddOrUpdateEnumMap(TypeStruct.TypeEnum typeEnum, Type type) => TypeMaps[typeEnum] = type;
    }

    public static class SystemTypes
    {
        public static bool True => true;
        public static bool False => false;
    }

    public static class TypeStruct
    {
        public static class OnClickDataStruct
        {
            public const string Success = "Success";
            public const string NotSuccess = "NotSuccess";
        }

        public static class SystemTypes
        {
            public const string True = "True";
            public const string False = "False";
        }

        public enum TypeEnum
        {
            OnClickData,
            SystemTypes
        }
    }
}
