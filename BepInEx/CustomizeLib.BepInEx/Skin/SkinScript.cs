using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;

namespace CustomizeLib.BepInEx.Script
{
    public static class SkinScript
    {
        private static uint token = 0;
        private static List<string[]> DllPaths =
        [
            ["dotnet"],
            ["BepInEx", "core"],
            ["BepInEx", "interop"],
            ["BepInEx", "plugins"],
        ];

        public static Assembly? GetCSharpScript(string content)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(content);
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            };
            foreach (var arr in DllPaths)
            {
                var path = Path.Combine([Environment.CurrentDirectory, .. arr]);
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    if (!file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) continue;
                    try
                    {
                        AssemblyName.GetAssemblyName(file);
                    }
                    catch (BadImageFormatException) { continue; }
                    catch (FileNotFoundException) { continue; }
                    catch (FileLoadException) { continue; }
                    references.Add(MetadataReference.CreateFromFile(file)); // 递归添加所有dll
                }
            }

            var compilation = CSharpCompilation.Create(
                $"CustomizeLibDynamicScript{Interlocked.Increment(ref token)}",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                CustomCore.CLogger.LogError("Compile error:");
                foreach (var diagnostic in result.Diagnostics
                             .Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    CustomCore.CLogger.LogError($"  {diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                return null;
            }

            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }

        public static void CallMethod(Assembly assembly, string name)
        {
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
                    var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (method != null && method.ReturnType == typeof(void))
                    {
                        method.Invoke(null, null);
                        found = true;
                        break;
                    }
                }
                if (!found) CustomCore.CLogger.LogWarning($"Not found entry method in script! (The signature of the {name} method should be public static void {name}())");
            }
            catch (Exception ex)
            {
                CustomCore.CLogger.LogError($"Failed to call the {name}() method: {ex.Message}");
            }
        }
    }
}
