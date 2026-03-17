using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.ViewModels;

public partial class ExportViewModel : ViewModelBase
{
    public IWorkspace Workspace => _workspace;
    public ExportContext Context => _context;
    public Window View { get; private set; }

    public ExportViewModel(Window view, IWorkspace workspace, ExportSettings settings)
    {
        View = view;
        _workspace = workspace;
        _context = new ExportContext
        {
            ExportSettings = settings,
        };
    }

    [RelayCommand]
    public async Task Export()
    {
        try
        {
            await PendingView.ShowForAsync(View, Task.Run(()=>
            {
                _context.ExportTo(Workspace);
            }));
        }
        catch (Exception ex)
        {
            Workspace.LogError(ex.Message);
        }
    }

    IWorkspace _workspace;
    ExportContext _context;
}