using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomizeLib.BepInEx
{
    public static class Extensions
    {
        public static void SetLayer(this Transform transform, string layerName)
        {
            if (transform == null) return;
            if (transform.TryGetComponent<SortingGroup>(out var group))
                group.sortingLayerName = layerName;
            if (transform.TryGetComponent<SpriteRenderer>(out var sprite))
                sprite.sortingLayerName = layerName;
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).SetLayer(layerName);
        }

        public static T? GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject != null && gameObject.TryGetComponent<T>(out var component) && component != null)
                return component;
            else if (gameObject != null)
                return gameObject.AddComponent<T>();
            return null;
        }

        public static T? GetOrAddComponent<T>(this Transform gameObject) where T : Component
        {
            if (gameObject != null && gameObject.TryGetComponent<T>(out var component) && component != null)
                return component;
            else if (gameObject != null)
                return gameObject.AddComponent<T>();
            return null;
        }

        public static T? GetOrAddComponent<T>(this Component gameObject) where T : Component
        {
            if (gameObject != null && gameObject.TryGetComponent<T>(out var component) && component != null)
                return component;
            else if (gameObject != null)
                return gameObject.AddComponent<T>();
            return null;
        }

        public static Coroutine StartCoroutine(this MonoBehaviour self, IEnumerator routine)
        {
            return global::BepInEx.Unity.IL2CPP.Utils.MonoBehaviourExtensions.StartCoroutine(self, routine);
        }

        public static bool TryGetAsset<T>(this AssetBundle ab, string name, out T obj) where T : UnityEngine.Object
        {
            foreach (var ase in ab.LoadAllAssetsAsync().allAssets)
            {
                if (ase.TryCast<T>()?.name == name)
                {
                    obj = ase.Cast<T>();
                    return true;
                }
            }
            obj = null;
            return false;
        }

        public static T GetAsset<T>(this AssetBundle ab, string name) where T : UnityEngine.Object
        {
            foreach (var ase in ab.LoadAllAssetsAsync().allAssets)
            {
                if (ase.TryCast<T>()?.name == name)
                {
                    return ase.Cast<T>();
                }
            }
            throw new ArgumentException($"Could not find {name} from {ab.name}");
        }

        /// <summary>
        /// 将Texture2D转换为Sprite
        /// </summary>
        /// <param name="texture2D">Texture2D对象</param>
        /// <returns>Sprite对象</returns>
        public static Sprite ToSprite(this Texture2D texture2D) =>
            Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));

        public static List<string> GetAssetBundleAssetNames(this AssetBundle assetBundle)
        {
            if (assetBundle == null)
            {
                CustomCore.Instance.Value.Log.LogError("Failed to get AssetBundle!");
                return new List<string>();
            }

            List<string> assetNames = new List<string>();

            foreach (var asset in assetBundle.LoadAllAssets())
            {
                assetNames.Add(asset.name);
            }
            return assetNames;
        }
        public static void AddLayer(this Transform transform, int level)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).IsObjExist() && transform.GetChild(i).GetComponent<SpriteRenderer>() != null)
                {
                    transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder += level;
                    AddLayer(transform.GetChild(i), level);
                }
            }
        }
    }
}
