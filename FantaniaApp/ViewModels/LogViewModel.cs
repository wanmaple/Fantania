using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class LogViewModel : ViewModelBase
{
    public Workspace Workspace => _workspace;

    public LogViewModel(Workspace workspace)
    {
        _workspace = workspace;
    }

    [RelayCommand]
    public void ClearLog()
    {
        _workspace.LogModule.Clear();
    }

    [RelayCommand]
    public async Task CopyLog()
    {
        string currentLog = string.Join(Environment.NewLine, _workspace.LogModule.Logs.Select(log => log.Content));
        var topLevel = TopLevel.GetTopLevel(AvaloniaHelper.GetTopWindow())!;
        var clipboard = topLevel.Clipboard;
        await clipboard!.SetTextAsync(currentLog);
    }

    Workspace _workspace;
}