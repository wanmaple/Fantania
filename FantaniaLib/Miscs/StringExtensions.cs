using System.Numerics;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace FantaniaLib;

public static class StringExtensions
{
    public static string ToStandardPath(this string path)
    {
        return path.Replace('\\', '/');
    }

    public static string MakeFirstCharacterUpper(this string self)
    {
        if (string.IsNullOrEmpty(self)) return self;
        if (!char.IsUpper(self[0]))
        {
            return char.ToUpper(self[0]).ToString() + self.Substring(1);
        }
        return self;
    }

    public static Vector4 ToVector4(this string self)
    {
        return Color.Parse(self).ToVector4();
    }

    /// <summary>
    /// 解析 Windows 传统过滤器格式
    /// 格式规则：
    /// 1. 用 | 分隔不同的过滤器组
    /// 2. 每组包含两部分：显示文本 | 通配符模式
    /// 3. 示例："Text files (*.txt)|*.txt|All files|*.*"
    /// </summary>
    public static IReadOnlyList<FilePickerFileType> ParseWindowsFilter(this string self)
    {
        if (string.IsNullOrWhiteSpace(self))
        {
            return [FilePickerFileTypes.All];
        }
        var filters = new List<FilePickerFileType>();
        var parts = self.Split('|', StringSplitOptions.RemoveEmptyEntries);
        // 必须是偶数个部分（显示文本和模式成对出现）
        if (parts.Length % 2 != 0)
        {
            throw new FormatException(
                $"Windows Filter format error：'{self}'.");
        }
        for (int i = 0; i < parts.Length; i += 2)
        {
            var displayName = parts[i].Trim();
            var patternString = parts[i + 1].Trim();
            var patterns = ParsePatterns(patternString);
            if (patterns.Any())
            {
                filters.Add(new FilePickerFileType(displayName)
                {
                    Patterns = patterns.ToArray(),
                });
            }
        }
        if (!filters.Any())
        {
            filters.Add(FilePickerFileTypes.All);
        }
        return filters;
    }

    static IEnumerable<string> ParsePatterns(string patternString)
    {
        return patternString
            .Split(';', ',')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct();
    }
}