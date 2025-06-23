using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using MelonLoader.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CustomizeLib.MelonLoader
{
    public static class TextureStore
    {
        public static void Init() => LoadTextures();

        public static Texture2D LoadImage(string path)//加载特定路径贴图
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The image file at path '" + path + "' does not exist.");
            }
            byte[] array = File.ReadAllBytes(path);
            Texture2D texture2D = new(2, 2, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);
            ImageConversion.LoadImage(texture2D, array);
            return texture2D;
        }

        public static void LoadTextures()//加载所有替换贴图
        {
            var path = Path.Combine(MelonEnvironment.ModsDirectory, "Textures");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                foreach (string text3 in Directory.EnumerateFiles(path, "*.png", SearchOption.AllDirectories))
                {
                    LoadImage(text3);
                    TextureDict[Path.GetFileNameWithoutExtension(text3)] = text3;
                }
            }
            catch (Exception ex2)
            {
                MelonLogger.Error("Error loading Texture.");
                MelonLogger.Error(ex2);
                return;
            }
            MelonLogger.Msg("Textures loaded successfully.");
        }

        public static void Reload()//重加载贴图
        {
            TextureDict.Clear();
            LoadTextures();
        }

        public static void ReplaceTextures()//换所有贴图
        {
            foreach (var texture2D in from Texture2D texture2D in (Texture2D[])Resources.FindObjectsOfTypeAll<Texture2D>()
                                      where !texture2D.name.StartsWith("replaced_")//避免重复替换
                                      select texture2D)
            {
                TryReplaceTexture2D(texture2D);
            }
        }

        //换贴图协程
        public static IEnumerator ReplaceTexturesCoroutine()
        {
            for (; ; )
            {
                ReplaceTextures();
                yield return new WaitForSeconds(0.5f);//每半秒换一次
            }
        }

        public static bool TryReplaceTexture2D(Texture2D ogTexture)
        {
            if (ogTexture is not null)
            {
                if (TextureDict.TryGetValue(ogTexture.name, out var text))
                {
                    try
                    {
                        ImageConversion.LoadImage(ogTexture, File.ReadAllBytes(text));
                        ogTexture.name = "replaced_" + ogTexture.name;//标记已替换的贴图
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error("Failed to replace texture: " + ogTexture.name + " at path: " + text);
                        MelonLogger.Error(ex);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 替换贴图列表，(贴图名称,贴图路径)
        /// </summary>
        public static Dictionary<string, string> TextureDict { get; set; } = [];
    }
}