using FantaniaLib;

namespace Fantania.ViewModels;

public class ExportViewModel : ViewModelBase
{
    public IWorkspace Workspace => _workspace;
    public ExportContext Context => _context;

    public ExportViewModel(IWorkspace workspace, ExportSettings settings)
    {
        _workspace = workspace;
        _context = new ExportContext
        {
            ExportSettings = settings,
        };
    }

    IWorkspace _workspace;
    ExportContext _context;
}