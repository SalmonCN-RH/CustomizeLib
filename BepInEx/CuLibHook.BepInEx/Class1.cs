using BepInEx;
using BepInEx.Unity.Common;
using BepInEx.Unity.IL2CPP;
using Cpp2IL.Core;
using Cpp2IL.Core.Model.Contexts;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Runtime;
using LibCpp2IL;
using LibCpp2IL.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace CuLibHook.BepInEx
{
    internal static class StringInfo
    {
        public const string DataPostfix = "_Data";
    }

    [BepInPlugin("culib", "CuLib", "1.0.0")]
    public class Core : BasePlugin
    {
        private static class GameInfo
        {
            public static string DataPath = "";
            public static string MetadataPath = "";
            public static string GameName = "";
            public static string GamePath = "";
            public static string GameAsmPath = "";
        }


        public override unsafe void Load()
        {
            InitGameInfo();
        }

        private static void InitGameInfo()
        {
            GameInfo.DataPath = Directory.EnumerateDirectories(Environment.CurrentDirectory, $"*{StringInfo.DataPostfix}",
               SearchOption.TopDirectoryOnly).FirstOrDefault()!;
            GameInfo.MetadataPath = Path.Combine(GameInfo.DataPath, "il2cpp_data", "Metadata", "global-metadata.dat");
            GameInfo.GameName = Path.GetFileName(GameInfo.DataPath)![..^StringInfo.DataPostfix.Length];
            GameInfo.GamePath = Directory.EnumerateFiles(Environment.CurrentDirectory, $"{GameInfo.GameName}.exe").
                FirstOrDefault()!;
            GameInfo.GameAsmPath = Path.Combine(Environment.CurrentDirectory, "GameAssembly.dll");
        }
    }
}
