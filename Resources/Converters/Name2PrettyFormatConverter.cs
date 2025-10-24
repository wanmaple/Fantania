using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class Name2PrettyFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string str = value as string;
        List<int> splitStart = new List<int>();
        splitStart.Add(0);
        if (!char.IsUpper(str[0]))
        {
            str = char.ToUpper(str[0]).ToString() + str.Substring(1);
        }
        for (int i = 1; i < str.Length; i++)
        {
            char c = str[i];
            if (char.IsUpper(c))
                splitStart.Add(i);
        }
        // splitStart.Add(str.Length);
        int last = str.Length;
        int temp = -1;
        List<string> words = new List<string>();
        for (int i = splitStart.Count - 1; i >= 0; i--)
        {
            int idx = splitStart[i];
            if (last - idx == 1)
            {
                if (temp < 0) temp = last;
                last = idx;
                continue;
            }
            if (temp >= 0)
            {
                words.Add(str.Substring(last, temp - last));
                temp = -1;
            }
            words.Add(str.Substring(idx, last - idx));
            last = idx;
        }
        if (temp >= 0)
        {
            words.Add(str.Substring(last, temp - last));
        }
        words.Reverse();
        return string.Join(' ', words);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
