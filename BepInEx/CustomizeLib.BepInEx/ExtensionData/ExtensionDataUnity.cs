using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace CustomizeLib.BepInEx.ExtensionData.Unity
{
    public static class ExtensionDataUnity
    {
        public static ExtDataRef<T> GetData<T>(this UnityEngine.Object obj, string name)
        {
            if (obj is GameObject go) return GetData<T>(go, name);
            if (obj is Component comp) return GetData<T>(comp, name);
            return null;
        }

        public static ExtDataRef<T> GetData<T>(this GameObject obj, string name)
        {
            var dataComp = obj.GetOrAddComponent<DataComponent>();
            return new ExtDataRef<T>(obj, name);
        }

        public static ExtDataRef<T> GetData<T>(this Component obj, string name)
        {
            var dataComp = obj.gameObject.GetOrAddComponent<DataComponent>();
            return new ExtDataRef<T>(obj, name);
        }

        public static void SetData(this UnityEngine.Object obj, string name, object value)
        {
            if (obj is GameObject go) SetData(go, name, value);
            if (obj is Component comp) SetData(comp, name, value);
        }

        public static void SetData(this GameObject obj, string name, object value)
        {
            var dataComp = obj.GetOrAddComponent<DataComponent>();
            dataComp.SetData(name, value);
        }

        public static void SetData(this Component obj, string name, object value)
        {
            var dataComp = obj.gameObject.GetOrAddComponent<DataComponent>();
            dataComp.SetData(name, value);
        }
    }

    public class DataComponent : MonoBehaviour
    {
        public Dictionary<string, object> datas = new();

        public object? GetData(string name)
        {
            if (!datas.ContainsKey(name)) datas.Add(name, null);
            return datas[name];
        }

        public void SetData(string name, object value)
        {
            datas[name] = value;
        }
    }

    public class ExtDataRef<T>
    {
        public T? val
        {
            get => (T)(parent.GetOrAddComponent<DataComponent>().GetData(name) == null ? 
                default(T) : parent.GetOrAddComponent<DataComponent>().GetData(name));
            set => parent.GetOrAddComponent<DataComponent>().SetData(name, value);
        }
        public string name = "";
        public UnityEngine.Object? parent = null;

        public ExtDataRef(UnityEngine.Object parent, string name)
        {
            this.parent = parent;
            this.name = name;
        }
    }
}
