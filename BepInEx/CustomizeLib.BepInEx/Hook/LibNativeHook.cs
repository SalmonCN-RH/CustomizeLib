using BepInEx.Unity.IL2CPP.Hook;
using CustomizeLib.BepInEx.UnmanagedTools;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Hook
{
    public static unsafe class LibNativeHook
    {
        private static List<INativeDetour> Detours { get; set; } = [];

        public static INativeDetour CreateAndApply<T>(nint from, T to, out T original) where T : Delegate
        {
            var detour = INativeDetour.CreateAndApply(from, to, out original);
            Detours.Add(detour);
            return detour;
        }

        /// <summary>
        /// 获取 Il2Cpp 方法结构体指针并初始化类
        /// </summary>
        /// <param name="asmName">程序集名</param>
        /// <param name="namespaze">命名空间</param>
        /// <param name="className">类名</param>
        /// <param name="isGeneric">泛型</param>
        /// <param name="methodName">方法名</param>
        /// <param name="returnTypeName">返回值类型名</param>
        /// <param name="argsTypes">参数列表</param>
        /// <returns>Il2Cpp 方法结构体指针</returns>
        public static IntPtr GetAndInitMethod(string asmName, string namespaze, string className, bool isGeneric, string methodName,
            string returnTypeName, params string[] argsTypes)
        {
            var clz = IL2CPP.GetIl2CppClass(asmName, namespaze, className);
            IL2CPP.il2cpp_init(clz);
            return IL2CPP.GetIl2CppMethod(clz, isGeneric, methodName, returnTypeName, argsTypes);
        }

        /// <summary>
        /// 获取 Il2Cpp 方法指针并初始化类
        /// </summary>
        /// <param name="asmName">程序集名</param>
        /// <param name="namespaze">命名空间</param>
        /// <param name="className">类名</param>
        /// <param name="isGeneric">泛型</param>
        /// <param name="methodName">方法名</param>
        /// <param name="returnTypeName">返回值类型名</param>
        /// <param name="argsTypes">参数列表</param>
        /// <returns>Il2Cpp 方法指针</returns>
        public static IntPtr GetAndInitMethodAddr(string asmName, string namespaze, string className, bool isGeneric, string methodName,
            string returnTypeName, params string[] argsTypes)
        {
            var strc = GetAndInitMethod(asmName, namespaze, className, isGeneric, methodName, returnTypeName, argsTypes);
            return GetMethodAddr(strc);
        }

        /// <summary>
        /// 获取 Il2Cpp 方法指针并初始化类
        /// </summary>
        /// <param name="target">类型</param>
        /// <param name="method">方法</param>
        /// <returns>Il2Cpp 方法指针</returns>
        public static IntPtr GetAndInitMethodAddr(Type target, MethodBase method)
        {
            var argTypes = new List<string>();
            foreach (var parameter in method.GetParameters())
            {
                bool addr = parameter.IsOut || parameter.ParameterType.IsByRef;
                var type = parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType() : parameter.ParameterType;
                argTypes.Add(GetTypeName(type!, addr));
            }
            var ret = method is MethodInfo info ? GetTypeName(info.ReturnType) : ClassTools.Void;
            return GetAndInitMethodAddr($"{target.Assembly.GetName().Name}.dll", target.Namespace ?? "", target.Name, method.IsGenericMethod, method.Name,
                ret, [.. argTypes]);
        }


        /// <summary>
        /// 获取 Il2Cpp 方法指针并初始化类
        /// </summary>
        /// <param name="target">类型</param>
        /// <param name="name">方法名</param>
        /// <returns>Il2Cpp 方法指针</returns>
        public static IntPtr GetAndInitMethodAddr(Type target, string name) =>
            GetAndInitMethodAddr(target, target.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!);

        /// <summary>
        /// 获取类型在 Il2Cpp 中的名称
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="addr">是否添加取址(ref / out 参数)</param>
        /// <returns>类型名</returns>
        public static string GetTypeName<T>(bool addr = false) => IL2CPP.RenderTypeName<T>(addr);

        /// <summary>
        /// 获取类型在 Il2Cpp 中的名称
        /// </summary>
        /// <param name="t">类型</param>
        /// <param name="addr">是否添加取址(ref / out 参数)</param>
        /// <returns>类型名</returns>
        public static string GetTypeName(Type t, bool addr = false) => IL2CPP.RenderTypeName(t, addr);

        /// <summary>
        /// 获取 Il2Cpp 方法地址
        /// </summary>
        /// <param name="methodInfo">Il2Cpp 方法结构体指针</param>
        /// <returns>方法地址</returns>
        public static IntPtr GetMethodAddr(Il2CppMethodInfo* methodInfo) => UnityVersionHandler.Wrap(methodInfo).MethodPointer;

        /// <summary>
        /// 获取 Il2Cpp 方法地址
        /// </summary>
        /// <param name="methodInfo">Il2Cpp 方法结构体指针</param>
        /// <returns>方法地址</returns>
        public static IntPtr GetMethodAddr(IntPtr methodInfo) => GetMethodAddr((Il2CppMethodInfo*)methodInfo);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ApplyNativeHookAttribute(string targetMethod = "ApplyHook") : Attribute
    {
        public string TargetMethod = targetMethod;
    }

    internal static class ApplyNativeHookTools
    {
        public static void RunAll()
        {
            foreach (var type in SystemTools.GetAllTypes())
            {
                foreach (var attr in type.GetCustomAttributes<ApplyNativeHookAttribute>())
                {
                    var method = type.GetMethod(attr.TargetMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (method == null)
                    {
                        CustomCore.CLogger.LogError($"Not found method {attr.TargetMethod} on type {type}");
                        continue;
                    }
                    method.Invoke(null, []);
                }
            }
        }
    }
}
