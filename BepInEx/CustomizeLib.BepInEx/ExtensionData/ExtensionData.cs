using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx.ExtensionData.Basic
{
    public static class ExtensionData
    {
        public static Dictionary<Type, Dictionary<String, object>> staticData { get; set; } = [];
        public static Dictionary<Type, Dictionary<object, Dictionary<String, object>>> instanceData { get; set; } = [];

        public static object GetData(this Component component, String name)
        {
            if (component == null)
                return null;
            return component.gameObject.GetData(name);
        }
        public static T GetData<T>(this Component component, String name)
        {
            if (component == null)
                return default;
            try
            {
                if (component.gameObject.GetData(name) == null)
                    return default;
                return (T)component.gameObject.GetData(name);
            }
            catch (Exception e)
            {
                CustomCore.CLogger.LogInfo(
                    "Error on convert type (at Get Extension Data), \n" +
                    $"Message   : {e.Message}\n" +
                    $"StackTrace: {e.StackTrace}\n"
                    );
                return default;
            }
        }
        public static void SetData(this Component component, String name, object data)
        {
            if (component == null)
                return;
            component.gameObject.SetData(name, data);
        }

        public static object GetData(this GameObject gameObject, String name)
        {
            if (gameObject == null)
                return null;
            if (gameObject.TryGetComponent<ExtensionDataComponent>(out var edc))
                return edc.GetData(name);
            else
            {
                gameObject.AddComponent<ExtensionDataComponent>();
                return null;
            }
        }
        public static T GetData<T>(this GameObject gameObject, String name)
        {
            if (gameObject == null)
                return default;
            try
            {
                if (gameObject.GetData(name) == null)
                    return default;
                return (T)gameObject.GetData(name);
            }
            catch (Exception e)
            {
                CustomCore.CLogger.LogInfo(
                    "Error on convert type (at Get Extension Data), \n" +
                    $"Message   : {e.Message}\n" +
                    $"StackTrace: {e.StackTrace}\n"
                    );
                return default;
            }
        }
        public static void SetData(this GameObject gameObject, String name, object data)
        {
            if (gameObject == null)
                return;
            if (gameObject.TryGetComponent<ExtensionDataComponent>(out var edc))
                edc.SetData(name, data);
            else
            {
                var result = gameObject.AddComponent<ExtensionDataComponent>();
                result.SetData(name, data);
            }
        }

        public static object GetData<T>(String name) => GetData(typeof(T), name);
        public static TData GetData<TClass, TData>(String name) => GetData<TData>(typeof(TClass), name);
        public static object GetData(Type type, String name)
        {
            if (staticData.ContainsKey(type))
                if (staticData[type].ContainsKey(name))
                    return staticData[type][name];
                else
                    return null;
            else
                return null;
        }
        public static TData GetData<TData>(Type type, String name)
        {
            if (staticData.ContainsKey(type))
                if (staticData[type].ContainsKey(name))
                    return (TData)staticData[type][name];
                else
                    return default;
            else
                return default;
        }
        public static void SetData<T>(String name, object data) => SetData(typeof(T), name, data);
        public static void SetData(Type type, String name, object data)
        {
            if (staticData.ContainsKey(type))
                if (staticData[type].ContainsKey(name))
                    staticData[type][name] = data;
                else
                    staticData[type].Add(name, data);
            else
                staticData.Add(type, new Dictionary<String, object>() { { name, data } });
        }

        public static object GetData(this object obj, String name)
        {
            if (obj == null)
                return null;
            if (instanceData.ContainsKey(obj.GetType()))
                if (instanceData[obj.GetType()].ContainsKey(obj))
                    if (instanceData[obj.GetType()][obj].ContainsKey(name))
                        return instanceData[obj.GetType()][obj][name];
            return null;
        }
        public static T GetData<T>(this object obj, String name)
        {
            if (obj == null)
                return default;
            if (instanceData.ContainsKey(obj.GetType()))
                if (instanceData[obj.GetType()].ContainsKey(obj))
                    if (instanceData[obj.GetType()][obj].ContainsKey(name))
                        return (T)instanceData[obj.GetType()][obj][name];
            return default;
        }
        public static void SetData(this object obj, String name, object data)
        {
            if (obj == null)
                return;
            if (instanceData.ContainsKey(obj.GetType()))
                if (instanceData[obj.GetType()].ContainsKey(obj))
                    if (instanceData[obj.GetType()][obj].ContainsKey(name))
                        instanceData[obj.GetType()][obj][name] = data;
                    else
                        instanceData[obj.GetType()][obj].Add(name, data);
                else
                    instanceData[obj.GetType()].Add(obj, new Dictionary<String, object>() { { name, data } });
            else
                instanceData.Add(obj.GetType(), new Dictionary<object, Dictionary<String, object>>() { { obj, new Dictionary<String, object>() { { name, data } } } });
        }

        public static void WriteMethod<T>(this T comp, String name, Action<T> action) where T : Component => comp.SetData(name, action);
        public static void InvokeMethod<T>(this T comp, String name) where T : Component => comp.GetData<Action<T>>(name).Invoke(comp);
        public static void WriteMethod(this GameObject comp, String name, Action<GameObject> action) => comp.SetData(name, action);
        public static void InvokeMethod(this GameObject comp, String name) => comp.GetData<Action<GameObject>>(name).Invoke(comp);
    }

    public class ExtensionDataComponent : MonoBehaviour
    {
        public Dictionary<String, object> data { get; set; } = [];

        public object GetData(String name)
        {
            if (data.ContainsKey(name))
                return data[name];
            return null;
        }

        public void SetData(String name, object data)
        {
            if (this.data.ContainsKey(name))
                this.data[name] = data;
            else
                this.data.Add(name, data);
        }
    }
}
