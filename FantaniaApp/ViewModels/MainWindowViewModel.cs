using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private Workspace _workspace = null;
    public Workspace Workspace
    {
        get { return _workspace; }
        set
        {
            if (_workspace != value)
            {
                _workspace = value;
                OnPropertyChanged(nameof(Workspace));
            }
        }
    }

    public MainWindowViewModel()
    {
    }

    [RelayCommand]
    public async Task NewWorkspace()
    {
        var top = TopLevel.GetTopLevel(AvaloniaHelper.GetTopWindow());
        var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Localization.Resources.MI_File_NewWorkspace,
            AllowMultiple = false,
        });
        if (folders.Count > 0)
        {
            string folder = folders[0].Path.AbsoluteUri;
            Workspace workspace = new Workspace(folder);
            if (workspace.IsValid)
            {
                if (!await MessageBoxHelper.PopupMessageYesNo(AvaloniaHelper.GetTopWindow(), string.Empty))
                    return;
            }
            await workspace.CreateNew();
            Workspace = workspace;
        }
    }

    [RelayCommand]
    public async Task OpenWorkspace()
    {
    }

    [RelayCommand]
    public async Task CloseWorkspace()
    {
        Workspace = null;
    }

    [RelayCommand]
    public async Task ExitApplication()
    {
        Environment.Exit(0);
    }
}
