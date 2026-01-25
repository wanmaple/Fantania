using System.Collections.ObjectModel;

namespace FantaniaLib;

public class EditorModule : WorkspaceModule
{
    private bool _notifyFlag = false;
    /// <summary>
    /// 这是一个专门用于各种情况的Binding触发器，如果你的Binding由于某些原因不会触发PropertyChanged，你可以用MultiBinding把这个属性绑定上去，然后在需要触发的时机，调用Notify方法就可以触发Binding的改变了。
    /// </summary>
    public bool NotifyFlag
    {
        get { return _notifyFlag; }
        set
        {
            if (_notifyFlag != value)
            {
                _notifyFlag = value;
                OnPropertyChanged(nameof(NotifyFlag));
            }
        }
    }

    private EntityPlacementModes _curPlaceMode = EntityPlacementModes.Select;
    public EntityPlacementModes CurrentPlacementMode
    {
        get { return _curPlaceMode; }
        set
        {
            if (_curPlaceMode != value)
            {
                _curPlaceMode = value;
                OnPropertyChanged(nameof(CurrentPlacementMode));
            }
        }
    }

    private Vector2Int _mouseWorldPos = Vector2Int.Zero;
    public Vector2Int MouseWorldPosition
    {
        get { return _mouseWorldPos; }
        set
        {
            if (_mouseWorldPos != value)
            {
                _mouseWorldPos = value;
                OnPropertyChanged(nameof(MouseWorldPosition));
            }
        }
    }

    private SelectionBox _selection = new SelectionBox();
    public SelectionBox Selection
    {
        get { return _selection; }
        set
        {
            if (_selection != value)
            {
                _selection = value;
                OnPropertyChanged(nameof(Selection));
            }
        }
    }

    private ObservableCollection<IRenderable> _selectedObjs = new ObservableCollection<IRenderable>();
    public IList<IRenderable> SelectedObjects => _selectedObjs;

    private int _fps = 0;
    public int FPS
    {
        get { return _fps; }
        set
        {
            if (_fps != value)
            {
                _fps = value;
                OnPropertyChanged(nameof(FPS));
            }
        }
    }

    public EditorModule(IWorkspace workspace) : base(workspace)
    {}

    public void Notify()
    {
        NotifyFlag = !NotifyFlag;
    }
}