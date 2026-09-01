using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Extra.Attributes
{
    public static class AttributesTools
    {
        /// <summary>
        /// 获取方法上的所有 <see cref="ResultAttribute"/> 的返回值
        /// </summary>
        /// <param name="box">方法</param>
        /// <returns>所有返回值</returns>
        public static List<object?> GetMethodExtResults(Delegate box)
        {
            var method = box.Method;
            var result = new List<object?>();

            foreach (var attr in method.GetCustomAttributes(typeof(ResultAttribute), false))
                result.Add(((ResultAttribute)attr).Result);

            return result;
        }

        public static object? GetFieldValue(Type type, string name, object? obj)
        {
            var field = type.GetField(name);
            if (field == null) return null;
            return field.GetValue(obj);
        }

        public static object? GetPropertyValue(Type type, string name, object? obj)
        {
            var prop = type.GetProperty(name);
            if (prop == null) return null;
            if (!prop.CanRead) throw new InvalidOperationException($"Property {name} on type {type} is not readable");
            return prop.GetValue(obj);
        }

        public static object? GetMethodValue(Type type, string name, object? obj, Access access)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic;
            if (access == Access.Instance) flags |= BindingFlags.Instance;
            else if (access == Access.Static) flags |= BindingFlags.Static;
            var method = type.GetMethod(name, flags);
            if (method == null) throw new InvalidOperationException($"Can't find method {name} on type {type}");
            return method.Invoke(obj, []);
        }

        public static object? GetValue(Type type, string name, object? obj, GetterType getter, Access access)
        {
            return getter switch
            {
                GetterType.Field => GetFieldValue(type, name, obj),
                GetterType.Property => GetPropertyValue(type, name, obj),
                GetterType.Method => GetMethodValue(type, name, obj, access),
                _ => null
            };
        }
    }
}
