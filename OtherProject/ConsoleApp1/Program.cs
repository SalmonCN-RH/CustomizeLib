using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class CleanOutputDlls
{
    static int totalFiles = 0;
    static long totalSize = 0;
    static List<string> errorFiles = new List<string>();

    static void Main(string[] args)
    {
        // 设置你的项目根目录（BepInEx 文件夹）
        string projectRoot = @"F:\vsrepos\Customizelib\BepInEx";

        // 如果命令行提供了参数，使用命令行参数
        if (args.Length > 0)
        {
            projectRoot = args[0];
        }

        bool whatIf = args.Contains("--whatif") || args.Contains("-w");

        Console.WriteLine("========================================");
        Console.WriteLine("清理输出目录中的 DLL 和 PDB 文件");
        Console.WriteLine("========================================\n");

        if (!Directory.Exists(projectRoot))
        {
            Console.WriteLine($"错误：路径不存在 - {projectRoot}");
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"扫描路径: {projectRoot}\n");

        if (whatIf)
        {
            Console.WriteLine("【预览模式】不会实际删除文件\n");
        }

        // 递归查找所有 bin 和 obj 目录
        List<string> directories = new List<string>();

        // 查找所有 bin 目录
        var binDirs = Directory.GetDirectories(projectRoot, "bin", SearchOption.AllDirectories);
        directories.AddRange(binDirs);

        // 查找所有 obj 目录
        var objDirs = Directory.GetDirectories(projectRoot, "obj", SearchOption.AllDirectories);
        directories.AddRange(objDirs);

        // 去重并排序
        directories = directories.Distinct().OrderBy(d => d).ToList();

        Console.WriteLine($"找到 {directories.Count} 个输出目录\n");

        if (directories.Count == 0)
        {
            Console.WriteLine("没有找到 bin 或 obj 目录");
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
            return;
        }

        foreach (var dir in directories)
        {
            if (Directory.Exists(dir))
            {
                Console.WriteLine($"检查目录: {dir}");
                DeleteFilesInDirectory(dir, whatIf);
                Console.WriteLine();
            }
        }

        // 显示统计信息
        Console.WriteLine("========================================");
        if (whatIf)
            Console.WriteLine("预览模式完成！");
        else
            Console.WriteLine("清理完成！");
        Console.WriteLine("========================================");
        Console.WriteLine($"删除文件数: {totalFiles}");
        Console.WriteLine($"释放空间: {FormatSize(totalSize)}");

        if (errorFiles.Count > 0)
        {
            Console.WriteLine($"\n错误文件数: {errorFiles.Count}");
            foreach (var error in errorFiles.Take(10))
            {
                Console.WriteLine($"  {error}");
            }
            if (errorFiles.Count > 10)
                Console.WriteLine($"  ... 还有 {errorFiles.Count - 10} 个错误");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    static void DeleteFilesInDirectory(string directory, bool whatIf)
    {
        try
        {
            // 获取所有 DLL 和 PDB 文件
            var dllFiles = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories);
            var pdbFiles = Directory.GetFiles(directory, "*.pdb", SearchOption.AllDirectories);
            var files = dllFiles.Concat(pdbFiles).ToArray();

            if (files.Length == 0)
            {
                Console.WriteLine("  没有找到 DLL/PDB 文件");
                return;
            }

            int fileCount = 0;
            long sizeSum = 0;

            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    sizeSum += fileInfo.Length;
                    fileCount++;

                    if (whatIf)
                    {
                        Console.WriteLine($"  [预览] 将删除: {fileInfo.Name} ({FormatSize(fileInfo.Length)})");
                    }
                    else
                    {
                        File.Delete(file);
                        Console.WriteLine($"  [删除] {fileInfo.Name} ({FormatSize(fileInfo.Length)})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [错误] 无法删除: {Path.GetFileName(file)} - {ex.Message}");
                    errorFiles.Add(file);
                }
            }

            totalFiles += fileCount;
            totalSize += sizeSum;
            Console.WriteLine($"  共找到 {fileCount} 个文件，大小 {FormatSize(sizeSum)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  错误：无法访问目录 - {ex.Message}");
        }
    }

    static string FormatSize(long bytes)
    {
        if (bytes >= 1024 * 1024 * 1024)
            return $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB";
        if (bytes >= 1024 * 1024)
            return $"{bytes / 1024.0 / 1024.0:F2} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024.0:F2} KB";
        return $"{bytes} B";
    }
}