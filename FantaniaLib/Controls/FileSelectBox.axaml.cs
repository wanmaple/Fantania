using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace FantaniaLib;

public class FileSelectedEventArgs : RoutedEventArgs
{
    public string FilePath { get; set; }

    public FileSelectedEventArgs(string filePath)
    {
        FilePath = filePath;
        RoutedEvent = FileSelectBox.FileSelectedEvent;
    }
}

public partial class FileSelectBox : UserControl
{
    public static readonly RoutedEvent<FileSelectedEventArgs> FileSelectedEvent = RoutedEvent.Register<FileSelectBox, FileSelectedEventArgs>(nameof(FileSelected), RoutingStrategies.Bubble);
    public event EventHandler<FileSelectedEventArgs> FileSelected
    {
        add => AddHandler(FileSelectedEvent, value);
        remove => RemoveHandler(FileSelectedEvent, value);
    }

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

    public static readonly StyledProperty<IWorkspace?> WorkspaceProperty = AvaloniaProperty.Register<FileSelectBox, IWorkspace?>(nameof(Workspace), defaultValue: null);
    public IWorkspace? Workspace
    {
        get => GetValue(WorkspaceProperty);
        set => SetValue(WorkspaceProperty, value);
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
                await MessageBoxHelper.PopupErrorOkay(topLevel, string.Format(Workspace!.LocalizeString("ERR_FileInsideWorkspace"), Workspace!.RootFolder));
                return;
            }
            if (!string.IsNullOrEmpty(RootFolder))
            {
                path = path.Substring(RootFolder.Length + (RootFolder.EndsWith('/') ? 0 : 1));
            }
            if (Path != path)
            {
                RaiseEvent(new FileSelectedEventArgs(path));
                Path = path;
            }
        }
    }
}