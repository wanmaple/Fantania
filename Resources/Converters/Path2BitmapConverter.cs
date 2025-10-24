using System;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class Path2BitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string path = value as string;
        if (path.StartsWith("avares://"))
        {
            var uri = new Uri(path);
            using (var assets = AssetLoader.Open(uri))
                return new Bitmap(assets);
        }
        else if (File.Exists(path))
        {
            return new Bitmap(path);
        }
        else
        {
            Workspace workspace = WorkspaceViewModel.Current.Workspace;
            if (workspace != null)
            {
                string absolutePath = workspace.GetAbsolutePath(path);
                if (File.Exists(absolutePath))
                    return new Bitmap(absolutePath);
            }
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}