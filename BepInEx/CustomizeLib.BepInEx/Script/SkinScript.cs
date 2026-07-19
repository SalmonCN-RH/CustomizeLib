using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CustomizeLib.BepInEx.Script
{
    public static class SkinScript
    {
        private static uint token = 0;
        private static ManualLogSource Logger = new("CuLibScript");
        private static List<string[]> DllPaths =
        [
            ["dotnet"],
            ["BepInEx", "core"],
            ["BepInEx", "interop"],
            ["BepInEx", "plugins"],
        ];

        public static void RunCSharpScript(string content)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(content);
            var references = new List<MetadataReference>();

            foreach (var arr in DllPaths)
            {
                var path = Path.Combine(arr);
                foreach (var dll in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    references.Add(MetadataReference.CreateFromFile(dll)); // 递归添加所有dll
                }
            }

            var compilation = CSharpCompilation.Create(
                $"CustomizeLibDynamicScript{token}",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            token++;

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                Logger.LogError("Compile error:");
                foreach (var diagnostic in result.Diagnostics
                             .Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    Logger.LogError($"  {diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                return;
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            try
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray()!;
                }

                bool found = false;
                foreach (var type in types)
                {
                    var method = type.GetMethod("Entry", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (method != null && method.ReturnType == typeof(void))
                    {
                        method.Invoke(null, null);
                        found = true;
                        break;
                    }
                }
                if (!found) Logger.LogWarning("Not found entry method in script! (The signature of the Entry method should be public static void Entry())");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to call the Entry() method: {ex.Message}");
            }
        }
    }
}
