using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx
{
    public static class SystemTools
    {
        public static Type[] GetAllTypes() => GetAllTypes(AppDomain.CurrentDomain.GetAssemblies());

        public static Type[] GetAllTypes(Assembly[] asms)
        {
            var result = new List<Type>();
            foreach (var asm in asms)
                result.AddRange(GetAllTypes(asm));
            return [.. result];
        }

        public static Type[] GetAllTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (ex.Types != null)
                    return [.. ex.Types.Where(t => t != null).Select(t => t!)];
                return [];
            }
        }

        public static bool IsSubTypeOf(Type type, Type baseType, bool containBase = true)
        {
            if (type == null || baseType == null) return false;

            if (baseType.IsGenericTypeDefinition)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == baseType) return true;
                if (baseType.IsInterface) return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseType);

                var iter = type.BaseType;
                while (iter != null && iter != typeof(object))
                {
                    if (iter.IsGenericType && iter.GetGenericTypeDefinition() == baseType) return true;
                    iter = iter.BaseType;
                }
                return false;
            }

            if (containBase)
                return baseType.IsAssignableFrom(type);
            else
                return baseType.IsAssignableFrom(type) && type != baseType;
        }

        public static Type[] GetAllDerivedTypes(Assembly[] asms, Type baseType, bool containBase = true)
        {
            var allTypes = GetAllTypes(asms);
            return [.. allTypes.Where(t => IsSubTypeOf(t, baseType, containBase))];
        }

        public static Type[] GetAllDerivedTypes(Assembly asm, Type baseType, bool containBase = true) =>
            GetAllDerivedTypes([asm], baseType, containBase);

        public static Type[] GetAllDerivedTypes(Type type, bool containBase = true) => GetAllDerivedTypes(type.Assembly, type, containBase);

        public static Type[] GetAllDerivedTypes<TBase>(Assembly[] asms, bool containBase = true) => 
            GetAllDerivedTypes(asms, typeof(TBase), containBase);

        public static Type[] GetAllDerivedTypes<TBase>(Assembly asm, bool containBase = true) =>
            GetAllDerivedTypes([asm], typeof(TBase), containBase);

        public static Type[] GetAllDerivedTypes<TBase>(bool containBase = true) => GetAllDerivedTypes(typeof(TBase).Assembly, typeof(TBase), containBase);

        public static IEnumerable<MethodInfo> GetAllMethods(Type[] types, string name, BindingFlags flags)
        {
            return types.Where(t => TryGetMethod(t, name, flags) != null).Select(t => TryGetMethod(t, name, flags)).Select(info => info!);
        }

        public static MethodInfo? TryGetMethod(Type type, string name, BindingFlags flags)
        {
            return type.GetMethod(name, flags);
        }

        #region BindingFlags扩展
        public static BindingFlags AddPublic(this BindingFlags flag) => flag | BindingFlags.Public;
        public static BindingFlags AddNonPublic(this BindingFlags flags) => flags | BindingFlags.NonPublic;
        public static BindingFlags AddAllAccess(this BindingFlags flags) => flags | BindingFlags.Public | BindingFlags.NonPublic;
        public static BindingFlags AddInstance(this BindingFlags flags) => flags | BindingFlags.Instance;
        public static BindingFlags AddStatic(this BindingFlags flags) => flags | BindingFlags.Static;
        public static BindingFlags AddDeclaredOnly(this BindingFlags flags) => flags | BindingFlags.DeclaredOnly;

        public static BindingFlags AddFlag(this BindingFlags flag, FlagConfig config)
        {
            var res = flag;
            if (config.Public) res |= BindingFlags.Public;
            if (config.NonPublic) res |= BindingFlags.NonPublic;
            if (config.AllAccess) res |= BindingFlags.Public | BindingFlags.NonPublic;
            if (config.Instance) res |= BindingFlags.Instance;
            if (config.Static) res |= BindingFlags.Static;
            if (config.DeclaredOnly) res |= BindingFlags.DeclaredOnly;
            return res;
        }

        public struct FlagConfig
        {
            public bool Public { get; set; } = true;
            public bool NonPublic { get; set; } = true;
            public bool AllAccess
            {
                readonly get => Public && NonPublic;
                set => Public = NonPublic = value;
            }
            public bool Instance { get; set; } = true;
            public bool Static { get; set; } = false;
            public bool DeclaredOnly { get; set; } = true;

            public FlagConfig() { }
        }
        #endregion
    }
}
