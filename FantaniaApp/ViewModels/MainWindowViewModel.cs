using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Fantania.Localization;
using Fantania.Models;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private Workspace? _workspace = null;
    public Workspace? Workspace
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

    public RecentAccess Recents => _recents;

    public MainWindowViewModel()
    {
        LoadRecentAccess();
    }

    [RelayCommand]
    public async Task NewWorkspace()
    {
        if (!await CheckAndSaveWorkspaceChanges()) return;
        var top = TopLevel.GetTopLevel(AvaloniaHelper.GetTopWindow())!;
        var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Resources.MI_File_NewWorkspace,
            AllowMultiple = false,
        });
        if (folders.Count > 0)
        {
            string folder = folders[0].Path.AbsoluteUri;
            folder = AvaloniaHelper.ConvertAvaloniaUriToStandardUri(folder);
            Workspace workspace = new FantaniaWorkspace(folder);
            if (workspace.IsValid)
            {
                if (!await MessageBoxHelper.PopupWarningYesNo(AvaloniaHelper.GetTopWindow(), LocalizationHelper.GetLocalizedString("WARN_Replace_Exist_Workspace"), folder))
                    return;
            }
            await workspace.CreateNew();
            _recents.Access(workspace.RootFolder);
            SaveRecentAccess();
            Workspace = workspace;
        }
    }

    [RelayCommand]
    public async Task OpenWorkspace()
    {
        if (!await CheckAndSaveWorkspaceChanges()) return;
        var top = TopLevel.GetTopLevel(AvaloniaHelper.GetTopWindow())!;
        var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Resources.MI_File_OpenWorkspace,
            AllowMultiple = false,
        });
        if (folders.Count > 0)
        {
            string folder = folders[0].Path.AbsoluteUri;
            folder = AvaloniaHelper.ConvertAvaloniaUriToStandardUri(folder);
            await OpenWorkspace(folder);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOperateWorkspace))]
    public async Task SaveWorkspace()
    {
        await Workspace!.Save();
        await Workspace.LogAsync(LocalizationHelper.GetLocalizedString("MSG_WorkspaceSaved"));
    }

    [RelayCommand(CanExecute = nameof(CanOperateWorkspace))]
    public async Task CloseWorkspace()
    {
        if (!await CheckAndSaveWorkspaceChanges()) return;
        Workspace = null;
        GC.Collect();
    }

    [RelayCommand]
    public async Task OpenRecentWorkspace(string folder)
    {
        await Task.Yield();
        if (!Directory.Exists(folder))
        {
            await MessageBoxHelper.PopupErrorOkay(AvaloniaHelper.GetTopWindow(), LocalizationHelper.GetLocalizedString("ERR_NonExist_Workspace"), folder);
            return;
        }
        await OpenWorkspace(folder);
    }

    [RelayCommand(CanExecute = nameof(CanOperateWorkspace))]
    public async Task Export()
    {
        var settings = Workspace!.ScriptingModule.GetExportSettings();
        if (settings == null)
        {
            await MessageBoxHelper.PopupErrorOkay(AvaloniaHelper.GetTopWindow(), LocalizationHelper.GetLocalizedString("ERR_WorkspaceNotExportable"));
            return;
        }
        if (!await CheckAndSaveWorkspaceChanges()) return;
        var vExport = new ExportView();
        var vmExport = new ExportViewModel(Workspace!, settings);
        vExport.DataContext = vmExport;
        await vExport.ShowDialog(AvaloniaHelper.GetTopWindow());
    }

    bool CanOperateWorkspace()
    {
        return Workspace != null;
    }

    [RelayCommand]
    public void ExitApplication()
    {
        Environment.Exit(0);
    }

    [RelayCommand(CanExecute = nameof(CanCutCopy))]
    public async Task Cut()
    {
        var selections = Workspace!.EditorModule.SelectedObjects;
        var entities = SelectionHelper.GetSelectedEntities(selections);
        if (entities.Count > 0)
            await Workspace.LevelModule.CutEntities(entities);
    }

    [RelayCommand(CanExecute = nameof(CanCutCopy))]
    public async Task Copy()
    {
        var selections = Workspace!.EditorModule.SelectedObjects;
        var entities = SelectionHelper.GetSelectedEntities(selections);
        if (entities.Count > 0)
            await Workspace.LevelModule.CopyEntities(entities);
    }

    bool CanCutCopy()
    {
        if (Workspace != null)
        {
            return Workspace.EditorModule.SelectedObjects.Count > 0;
        }
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanPaste))]
    public async Task Paste()
    {
        Vector2Int worldPos = Workspace!.EditorModule.MouseWorldPosition;
        await Workspace.LevelModule.PasteEntities(worldPos);
    }

    bool CanPaste()
    {
        if (Workspace != null)
        {
            if (Workspace.LevelModule.CurrentLevel != null)
            {
                var clipboard = AvaloniaHelper.GetClipboard();
                return !string.IsNullOrEmpty(clipboard.TryGetTextAsync().GetAwaiter().GetResult());
            }
        }
        return false;
    }

    [RelayCommand(CanExecute = nameof(IsUndoable))]
    public void Undo()
    {
        if (Workspace != null)
        {
            Workspace.UndoStack.Undo();
        }
    }

    bool IsUndoable()
    {
        if (Workspace != null)
        {
            return Workspace.UndoStack.IsUndoable;
        }
        return false;
    }

    [RelayCommand(CanExecute = nameof(IsRedoable))]
    public void Redo()
    {
        if (Workspace != null)
        {
            Workspace.UndoStack.Redo();
        }
    }

    bool IsRedoable()
    {
        if (Workspace != null)
        {
            return Workspace.UndoStack.IsRedoable;
        }
        return false;
    }

    async Task OpenWorkspace(string folder)
    {
        if (Workspace != null)
        {
            if (Workspace.RootFolder == folder)
                return;
        }
        Workspace workspace = new FantaniaWorkspace(folder);
        if (!workspace.IsValid)
        {
            await MessageBoxHelper.PopupErrorOkay(AvaloniaHelper.GetTopWindow(), LocalizationHelper.GetLocalizedString("ERR_Invalid_Workspace"), folder);
            return;
        }
        await workspace.Open();
        _recents.Access(workspace.RootFolder);
        SaveRecentAccess();
        Workspace = workspace;
    }

    async Task<bool> CheckAndSaveWorkspaceChanges()
    {
        if (Workspace != null && Workspace.IsModified)
        {
            var result = await MessageBoxHelper.PopupWarningYesNoCancel(AvaloniaHelper.GetTopWindow(), LocalizationHelper.GetLocalizedString("WARN_ConfirmSaveWorkspace"));
            if (result == ButtonResults.Yes)
            {
                await SaveWorkspace();
                return true;
            }
            else if (result == ButtonResults.No)
                return true;
            return false;
        }
        return true;
    }

    void LoadRecentAccess()
    {
        string recentAccFile = Path.Combine(AppContext.BaseDirectory, RECENTS_FILENAME);
        if (File.Exists(recentAccFile))
        {
            try
            {
                using (var fs = new FileStream(recentAccFile, FileMode.Open, FileAccess.Read))
                {
                    _recents.Deserialize(fs);
                }
            }
            catch (Exception)
            {
                _recents.Clear();
            }
        }
    }

    void SaveRecentAccess()
    {
        string recentAccFile = Path.Combine(AppContext.BaseDirectory, RECENTS_FILENAME);
        using (var fs = new FileStream(recentAccFile, FileMode.Create, FileAccess.Write))
        {
            _recents.Serialize(fs);
        }
    }

    const string RECENTS_FILENAME = "recents";

    RecentAccess _recents = new RecentAccess();
}
