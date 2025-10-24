using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Fantania.Models;
using Fantania.ViewModels;
using System;
using System.Threading.Tasks;

namespace Fantania.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (!_forceClosing)
        {
            e.Cancel = true;
            bool close = true;
            WorkspaceViewModel vm = WorkspaceViewModel.Current;
            if (vm != null)
            {
                Workspace workspace = vm.Workspace;
                if (workspace.IsModified)
                {
                    await MessageBoxHelper.PopupWarningYesNoCancel(this, "WarningConfirmSavingOnExit").ContinueWith(async task =>
                    {
                        if (task.Result == ButtonResults.Yes)
                        {
                            await workspace.Save();
                        }
                        else if (task.Result == ButtonResults.Cancel)
                        {
                            close = false;
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            if (close)
            {
                _forceClosing = true;
                this.Close();
            }
        }
        base.OnClosing(e);
    }

    public async Task NewWorkspace()
    {
        var topLevel = GetTopLevel(this);
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Localization.Resources.MainMenuFileNewWorkspace,
            AllowMultiple = false,
        });
        if (folders.Count > 0)
        {
            var folder = folders[0].Path.AbsoluteUri;
            var workspace = new Workspace(folder);
            if (workspace.IsValid())
            {
                if (!await MessageBoxHelper.PopupWarningYesNo(this, "MessageConfirmReinitializeWorkspace"))
                {
                    return;
                }
            }
            var newWorldView = new NewWorldView();
            newWorldView.DataContext = workspace;
            bool confirm = await newWorldView.ShowDialog<bool>(this);
            if (confirm)
            {
                await workspace.Initialize();
                await workspace.CurrentWorld.Save(workspace);
                var vmWorkspace = new WorkspaceViewModel(workspace);
                content.Content = new WorkspaceView
                {
                    DataContext = vmWorkspace,
                };
                ViewModel.CurrentWorkspace = workspace;
                WorkspaceViewModel.Current = vmWorkspace;
                workspace.OnInitialized();
                await vmWorkspace.Log(string.Format(Localization.Resources.LogWorkspaceInitialized, workspace.RootFolder));
            }
        }
    }

    public async Task OpenWorkspace()
    {
        var topLevel = GetTopLevel(this);
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Localization.Resources.MainMenuFileOpenWorkspace,
            AllowMultiple = false,
        });
        if (folders.Count > 0)
        {
            var folder = folders[0].Path.AbsoluteUri;
            var workspace = new Workspace(folder);
            if (ViewModel.CurrentWorkspace != null && ViewModel.CurrentWorkspace.RootFolder == workspace.RootFolder)
            {
                return;
            }
            if (ViewModel.CurrentWorkspace != null)
            {
                if (ViewModel.CurrentWorkspace.IsModified)
                {
                    var result = await MessageBoxHelper.PopupWarningYesNoCancel(this, "WarningConfirmSavingOnExit");
                    if (result == ButtonResults.Cancel)
                        return;
                    else if (result == ButtonResults.Yes)
                        await ViewModel.CurrentWorkspace.Save();
                }
            }
            
            if (!workspace.IsValid())
            {
                await MessageBoxHelper.PopupErrorOkay(this, "MessageInvalidWorkspace", folder, "Database files are not loaded.");
                return;
            }
            try
            {
#if DEBUG
                await workspace.SyncAllData(true);
#else
                await workspace.SyncAllData();
#endif
            }
            catch (Exception ex)
            {
                await MessageBoxHelper.PopupErrorOkay(this, "MessageInvalidWorkspace", folder, ex.Message);
                return;
            }
            var vmWorkspace = new WorkspaceViewModel(workspace);
            content.Content = new WorkspaceView
            {
                DataContext = vmWorkspace,
            };
            ViewModel.CurrentWorkspace = workspace;
            WorkspaceViewModel.Current = vmWorkspace;
            workspace.OnInitialized();
            await vmWorkspace.Log(string.Format(Localization.Resources.LogWorkspaceOpened, workspace.RootFolder));
        }
    }

    public async Task CloseWorkspace()
    {
        if (ViewModel.CurrentWorkspace != null)
        {
            if (ViewModel.CurrentWorkspace.IsModified)
            {
                var result = await MessageBoxHelper.PopupWarningYesNoCancel(this, "WarningConfirmSavingOnExit");
                if (result == ButtonResults.Cancel)
                    return;
                else if (result == ButtonResults.Yes)
                    await ViewModel.CurrentWorkspace.Save();
            }
        }
        content.Content = null;
        ViewModel.CurrentWorkspace.OnFinalized();
        ViewModel.CurrentWorkspace = null;
        WorkspaceViewModel.Current = null;
    }

    public async Task SaveWorkspace()
    {
        Workspace workspace = ViewModel.CurrentWorkspace;
        if (workspace.IsModified)
        {
            await workspace.Save();
        }
        await WorkspaceViewModel.Current.Log(string.Format(Localization.Resources.LogWorkspaceSaved, workspace.RootFolder));
    }

    public void Exit()
    {
        Environment.Exit(0);
    }

    bool _forceClosing = false;
}