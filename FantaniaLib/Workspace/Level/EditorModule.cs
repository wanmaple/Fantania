using System.Numerics;

namespace FantaniaLib;

public class EditorModule : WorkspaceModule
{
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
}