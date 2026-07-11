using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx.UnmanagedTools
{
#pragma warning disable
    public static class ObjCopy
    {
        public static void CopyFieldAndProp(object source, object target, IEnumerable<Type>? skipTypes = null, IEnumerable<string>? skipStrs = null)
        {
            if (source == null || target == null)
                throw new ArgumentNullException("源对象和目标对象均不可为空。");
            if (skipTypes == null) skipTypes = new List<Type>();
            if (skipStrs == null) skipStrs = new List<string>();
            var sourceType = source.GetType();
            var targetType = target.GetType();

            var sourceFields = sourceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var sourceField in sourceFields)
            {
                var targetField = targetType.GetField(sourceField.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (targetField != null && targetField.FieldType == sourceField.FieldType &&
                    !skipTypes.Contains(targetField.FieldType) && !skipStrs.Contains(targetField.Name) &&
                    !skipTypes.Contains(sourceField.FieldType) && !skipStrs.Contains(sourceField.Name))
                {
                    object value = sourceField.GetValue(source);
                    targetField.SetValue(target, value);
                }
            }

            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var sourceProp in sourceProperties)
            {
                // 跳过索引器（带参数的属性）
                if (sourceProp.GetIndexParameters().Length > 0) continue;

                // 检查源属性是否可读
                if (!sourceProp.CanRead) continue;

                var targetProp = targetType.GetProperty(sourceProp.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                // 检查目标属性是否可写，且类型匹配
                if (targetProp != null && targetProp.CanWrite &&
                    targetProp.PropertyType == sourceProp.PropertyType && 
                    !skipTypes.Contains(targetProp.PropertyType) && !skipStrs.Contains(targetProp.Name) &&
                    !skipTypes.Contains(sourceProp.PropertyType) && !skipStrs.Contains(sourceProp.Name))
                {
                    object value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }
        }

        public static void CopyFieldAndPropTo(this object source, object target, IEnumerable<Type>? skipTypes = null, IEnumerable<string>? skipStrs = null) =>
            CopyFieldAndProp(source, target, skipTypes, skipStrs);
    }
}
