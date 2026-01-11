using System;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace Fantania;

public static class FastTest
{
    public static void Test()
    {
        TestWithSKCodec();
    }

    public static void TestWithSKCodec()
    {
        Console.WriteLine("=== 使用 SKCodec 测试 ===");

        var testFiles = new[]
        {
            "/Users/winder/Downloads/curved.png",
            "/Users/winder/Downloads/Image_fx (1).jpg",
        };

        foreach (var file in testFiles)
        {
            if (!File.Exists(file)) continue;

            Console.WriteLine($"\n分析: {file}");

            using var stream = File.OpenRead(file);
            using var codec = SKCodec.Create(stream);

            if (codec != null)
            {
                var info = codec.Info;
                Console.WriteLine($"  尺寸: {info.Width}x{info.Height}");
                Console.WriteLine($"  颜色类型: {info.ColorType}");
                Console.WriteLine($"  Alpha类型: {info.AlphaType}");
                Console.WriteLine($"  颜色空间: {(info.ColorSpace == null ? "null" : "存在")}");

                // 关键：分析颜色空间对象
                if (info.ColorSpace != null)
                {
                    AnalyzeColorSpaceMethods(info.ColorSpace);
                }
            }
            else
            {
                Console.WriteLine("  无法创建 SKCodec");
            }
        }
    }

    private static void AnalyzeColorSpaceMethods(SKColorSpace colorSpace)
    {
        Console.WriteLine("  === SKColorSpace 方法分析 ===");

        // 获取类型名
        var type = colorSpace.GetType();
        Console.WriteLine($"    类型: {type.FullName}");

        // 检查方法名
        var methods = type.GetMethods();

        Console.WriteLine("\n    可能相关的方法:");
        var relevantMethods = methods.Where(m =>
            m.Name.Contains("Gamma", StringComparison.OrdinalIgnoreCase) ||
            m.Name.Contains("Transfer", StringComparison.OrdinalIgnoreCase) ||
            m.Name.Contains("Srgb", StringComparison.OrdinalIgnoreCase) ||
            m.Name.Contains("Linear", StringComparison.OrdinalIgnoreCase) ||
            m.Name.Contains("Equal", StringComparison.OrdinalIgnoreCase) ||
            m.Name.Contains("ToXyz", StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (relevantMethods.Any())
        {
            foreach (var method in relevantMethods)
            {
                var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"    {method.ReturnType.Name} {method.Name}({parameters})");
            }
        }
        else
        {
            Console.WriteLine("    没有找到明显相关的方法");

            // 列出所有公共方法
            Console.WriteLine("\n    所有公共方法:");
            foreach (var method in methods.OrderBy(m => m.Name))
            {
                if (method.DeclaringType == typeof(object)) continue;
                Console.WriteLine($"    {method.Name}");
            }
        }
    }
}