using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    public static class GamePrefabs
    {
        public static CachePrefab<GameObject> IceRoad = new("background/IceRoad");
    }

    public class CachePrefab<T> where T : UnityEngine.Object
    {
        public T? value;
        public string path = "";

        public T GetValue()
        {
            if (value == null)
                value = Resources.Load<T>(path);
            return value;
        }

        /// <summary>
        /// 获取缓存Prefab实例
        /// </summary>
        /// <param name="path">相对于Resources文件夹的路径（不含Resources）</param>
        public CachePrefab(string path)
        {
            this.path = path;
        }

        public static implicit operator T(CachePrefab<T> cache) => cache.GetValue();
    }
}
