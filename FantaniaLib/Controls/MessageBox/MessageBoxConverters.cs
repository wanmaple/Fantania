using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using FantaniaLib;

namespace Fantania.Controls;

public class MessageBoxIcon2VisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        MessageBoxIcons icon = (MessageBoxIcons)value;
        return icon != MessageBoxIcons.None;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MessageBoxIcon2ImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        MessageBoxIcons icon = (MessageBoxIcons)value;
        switch (icon)
        {
            case MessageBoxIcons.Warning:
                using (var res = AvaloniaHelper.ReadAssetStream("avares://Fantania/Assets/msgbox/warning.png"))
                    return new Bitmap(res);
            case MessageBoxIcons.Error:
                using (var res = AvaloniaHelper.ReadAssetStream("avares://Fantania/Assets/msgbox/error.png"))
                    return new Bitmap(res);
            default:
                break;
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ButtonEnum2ItemsSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        ButtonEnums enm = (ButtonEnums)value;
        IList<ButtonResults> ret = new List<ButtonResults>();
        var rslts = Enum.GetValues<ButtonResults>();
        foreach (var rslt in rslts)
        {
            uint bits1 = (uint)enm;
            uint bits2 = (uint)rslt;
            if ((bits1 & bits2) > 0)
            {
                ret.Add(rslt);
            }
        }
        return ret;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}