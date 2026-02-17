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

    private TransformGizmoTypes _curTransMode = TransformGizmoTypes.None;
    public TransformGizmoTypes CurrentTransformMode
    {
        get { return _curTransMode; }
        set
        {
            if (_curTransMode != value)
            {
                _curTransMode = value;
                OnPropertyChanged(nameof(CurrentTransformMode));
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

    private ObservableCollection<ISelectableItem> _selectedObjs = new ObservableCollection<ISelectableItem>();
    public ObservableCollection<ISelectableItem> SelectedObjects => _selectedObjs;

    private ObservableCollection<ISnapPoint> _snapPts = new ObservableCollection<ISnapPoint>();
    public ObservableCollection<ISnapPoint> SnapPoints => _snapPts;

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

    private int _dcs = 0;
    public int DrawCalls
    {
        get { return _dcs; }
        set
        {
            if (_dcs != value)
            {
                _dcs = value;
                OnPropertyChanged(nameof(DrawCalls));
            }
        }
    }

    private int _tris = 0;
    public int Triangles
    {
        get { return _tris; }
        set
        {
            if (_tris != value)
            {
                _tris = value;
                OnPropertyChanged(nameof(Triangles));
            }
        }
    }

    public EditorModule(IWorkspace workspace) : base(workspace)
    {}

    public void CancelSelection()
    {
        _selectedObjs.Clear();
        Notify();
    }

    public void ClearSnapPoints()
    {
        _snapPts.Clear();
        Notify();
    }

    public void Notify()
    {
        NotifyFlag = !NotifyFlag;
    }
}