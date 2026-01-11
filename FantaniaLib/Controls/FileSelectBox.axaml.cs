using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace FantaniaLib;

public partial class FileSelectBox : UserControl
{
    public static readonly StyledProperty<string> PathProperty = AvaloniaProperty.Register<FileSelectBox, string>(nameof(Path), defaultValue: string.Empty);
    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<FileSelectBox, string>(nameof(Title), defaultValue: "Pick a file");
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> FilterProperty = AvaloniaProperty.Register<FileSelectBox, string>(nameof(Filter), defaultValue: string.Empty);
    public string Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public static readonly StyledProperty<string> RootFolderProperty = AvaloniaProperty.Register<FileSelectBox, string>(nameof(RootFolder), defaultValue: string.Empty);
    public string RootFolder
    {
        get => GetValue(RootFolderProperty);
        set => SetValue(RootFolderProperty, value);
    }

    public FileSelectBox()
    {
        InitializeComponent();
    }
    
    public async Task PickFile()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var options = new FilePickerOpenOptions
        {
            Title = Title,
            AllowMultiple = false,
            FileTypeFilter = Filter.ParseWindowsFilter(),
        };
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        if (files != null && files.Count > 0)
        {
            string path = files[0].Path.AbsolutePath.ToStandardPath();
            if (!string.IsNullOrEmpty(RootFolder) && !path.StartsWith(RootFolder))
            {
                await MessageBoxHelper.PopupErrorOkay(topLevel, $"The image must be inside '{RootFolder}'.");
                return;
            }
            if (!string.IsNullOrEmpty(RootFolder))
            {
                path = path.Substring(RootFolder.Length);
            }
            Path = path;
        }
    }
}