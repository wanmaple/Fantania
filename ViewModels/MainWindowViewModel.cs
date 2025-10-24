using CommunityToolkit.Mvvm.Input;
using Fantania.Models;

namespace Fantania.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private Workspace _currentWorkspace;
    public Workspace CurrentWorkspace
    {
        get { return _currentWorkspace; }
        set
        {
            if (_currentWorkspace != value)
            {
                _currentWorkspace = value;
                OnPropertyChanged(nameof(CurrentWorkspace));
            }
        }
    }

    public MainWindowViewModel()
    {
    }

    [RelayCommand]
    void Undo()
    {
        if (_currentWorkspace != null)
        {
            _currentWorkspace.UndoStack.Undo();
        }
    }

    [RelayCommand]
    void Redo()
    {
        if (_currentWorkspace != null)
        {
            _currentWorkspace.UndoStack.Redo();
        }
    }
}
