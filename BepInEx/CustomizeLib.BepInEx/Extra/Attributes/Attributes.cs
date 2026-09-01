using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Extra.Attributes
{
    /// <summary>
    /// 方法额外返回值
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ResultAttribute : Attribute
    {
        public Type Type { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public GetterType Getter { get; set; } = GetterType.Property;
        public object? Instance { get; set; } = null!;
        public Access Access { get; set; } = Access.Static;

        public object? Result => AttributesTools.GetValue(Type, Name, Instance, Getter, Access);

        public ResultAttribute(TypeStruct.TypeEnum type, string name, GetterType getter = GetterType.Property) :
            this(GetTypeFromEnum(type), name, null, getter, Access.Static)
        { }

        /// <summary>
        /// 从静态对象中获取返回值
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名字</param>
        /// <param name="getter">获取的类型</param>
        public ResultAttribute(Type type, string name, GetterType getter = GetterType.Property) :
            this(type, name, null, getter, Access.Static)
        { }

        /// <summary>
        /// 从实例对象中获取返回值
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名字</param>
        /// <param name="obj">实例</param>
        /// <param name="getter">获取的类型</param>
        public ResultAttribute(Type type, string name, object? obj, GetterType getter = GetterType.Property) :
            this(type, name, obj, getter, Access.Instance)
        { }

        /// <summary>
        /// 通用构造函数
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="name">名字</param>
        /// <param name="obj">对象</param>
        /// <param name="getter">获取的类型</param>
        /// <param name="access">访问标志</param>
        public ResultAttribute(Type type, string name, object? obj, GetterType getter, Access access)
        {
            Type = type;
            Name = name;
            Getter = getter;
            Instance = obj;
            Access = access;
        }

        public static Type GetTypeFromEnum(TypeStruct.TypeEnum typeEnum) =>
            TypeEnumTools.GetTypeFromEnum(typeEnum);

        public static void AddOrUpdateEnumMap(TypeStruct.TypeEnum typeEnum, Type type) => TypeEnumTools.AddOrUpdateEnumMap(typeEnum, type);
    }

    /// <summary>
    /// 指定从哪里读取值
    /// </summary>
    public enum GetterType
    {
        /// <summary>
        /// 字段
        /// </summary>
        Field,
        /// <summary>
        /// 属性
        /// </summary>
        Property,
        /// <summary>
        /// 方法
        /// </summary>
        Method
    }

    /// <summary>
    /// 调用类型
    /// </summary>
    public enum Access
    {
        /// <summary>
        /// 实例
        /// </summary>
        Instance,
        /// <summary>
        /// 静态
        /// </summary>
        Static
    }
}
