using BepInEx;
using BepInEx.Unity.Common;
using BepInEx.Unity.IL2CPP;
using Cpp2IL.Core;
using Cpp2IL.Core.Model.Contexts;
using Il2CppInterop.Runtime;
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
            byte[] machineCode = new byte[]
            {
                0x8D, 0x04, 0x11, // lea eax, [rcx+rdx]
                0x41, 0x03, 0xC0, // mov eax, r8d
                0x41, 0x03, 0xC1, // mov eax, r9d
                0xC3              // ret
            };
            var handle = AsmCore.AllocMemmory(machineCode);
            Console.WriteLine($"{handle.AddrOfPinnedObject():X}");
            foreach (var item in AsmCore.FindCodeOf((byte*)handle.AddrOfPinnedObject(), 2, 8, 
                (arr) => arr[0] == 0x41 && arr[1] == 0x03))
            {
                Console.WriteLine($"{item:X}");
            }
            handle.Free();

            var klass = Il2CppClassPointerStore<AbyssSwordStar>.NativeClassPtr;
            var method = IL2CPP.il2cpp_class_get_method_from_name(klass, "Awake", 0);
            var va = CppCore.GetRuntimeVA(method);
            var hook = InlineHook.Install(IntPtr.Add(va, 0x53), 15, new byte[]
            {
                0x58, // pop rax
                0x48, 0x83, 0xC4, 0x30, // add rsp, 30h
                0x5B, // pop rbx
                0xC3 // ret
            });
            //AsmCore.SetAssemblyCode((byte*)va, 0x53, new byte[]
            //{
            //    0x48, 0x83, 0xC4, 0x30, // add rsp, 30h
            //    0x5B, // pop rbx
            //    0xC3 // ret
            //});
            // hook.UnInstall();
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
