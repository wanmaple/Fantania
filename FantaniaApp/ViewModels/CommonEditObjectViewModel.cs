using FantaniaLib;

namespace Fantania.ViewModels;

public class CommonEditObjectViewModel : ViewModelBase
{
    public IWorkspace Workspace { get; private set; }
    public IEditableObject EditingObject { get; private set; }

    public CommonEditObjectViewModel(IWorkspace workspace, IEditableObject obj)
    {
        Workspace = workspace;
        EditingObject = obj;
    }
}