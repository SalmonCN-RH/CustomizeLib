using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx
{
    public static class Il2CppExtensions
    {
        public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this List<T> list)
        {
            var result = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var item in list) result.Add(item);
            return result;
        }

        public static List<T> ToSystemList<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            var result = new List<T>();
            foreach (var item in list) result.Add(item);
            return result;
        }

        public static Il2CppSystem.Collections.Generic.List<T> CreateNewList<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            var result = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (var item in list)
                result.Add(item);
            return result;
        }

        public static Il2CppSystem.Type ToIl2CppType(this Type type)
        {
            if (type == null) return null;
            return Il2CppType.From(type);
        }

        public static Il2CppSystem.Array ToArray<T>(this Il2CppReferenceArray<T> array) where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
            => array.Cast<Il2CppSystem.Array>();

        public static bool IsTypeOf(this Il2CppSystem.Object obj, Type type) => obj.GetIl2CppType() == Il2CppType.From(type);
        public static bool IsTypeOf<T>(this Il2CppSystem.Object obj) => obj.GetIl2CppType() == Il2CppType.From(typeof(T));
        public static Il2CppSystem.Object BoxEnumToIl2Object<TEnum>(object obj) where TEnum : Enum =>
            Il2CppSystem.Enum.ToObject(Il2CppType.From(typeof(TEnum)), (int)obj);
        public static Il2CppSystem.Object BoxEnumToIl2Object(object obj, Type enumType) =>
            Il2CppSystem.Enum.ToObject(Il2CppType.From(enumType), (int)obj);
        public static Il2CppSystem.Object BoxEnumToIl2Object(object obj, Il2CppSystem.Type enumType) =>
            Il2CppSystem.Enum.ToObject(enumType, (int)obj);
        public static Il2CppSystem.Type GetIl2CppType(this object obj) => Il2CppType.From(obj.GetType());
        public static Il2CppSystem.Nullable<T> GetNullable<T>(this T obj) where T : new() => new(obj);

        public static T GetRandom<T>(Il2CppSystem.Collections.Generic.HashSet<T> set, int count)
        {
            var skip = UnityEngine.Random.Range(0, count);
            var enumerator = set.GetEnumerator();
            while (skip-- > 0 && enumerator.MoveNext()) { }
            enumerator.MoveNext();
            return enumerator.Current;
        }
    }
}
