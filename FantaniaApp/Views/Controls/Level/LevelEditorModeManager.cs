namespace Fantania.Views;

public enum LevelEditorModes
{
    None,
    View,   // 默认状态，可以进行视口操作以及实体选中操作。
    Place,  // 放置状态
    Count,
}

public class LevelEditorModeManager
{
    public ILevelEditorMode CurrentMode => _currentMode;

    public LevelEditorModeManager()
    {
        _allModes[(int)LevelEditorModes.None] = new EmptyLevelEditorMode();
        _allModes[(int)LevelEditorModes.View] = new ViewEditorMode();
        _allModes[(int)LevelEditorModes.Place] = new PlaceEditorMode();
        _currentMode = GetEditorMode(LevelEditorModes.None);
    }

    public void SetEditorMode(LevelEditorModes mode, LevelEditorContext context)
    {
        _currentMode.OnExit(context);
        _currentMode = GetEditorMode(mode);
        _currentMode.OnEnter(context);
    }

    ILevelEditorMode GetEditorMode(LevelEditorModes mode)
    {
        return _allModes[(int)mode];
    }

    ILevelEditorMode _currentMode;
    ILevelEditorMode[] _allModes = new ILevelEditorMode[(int)LevelEditorModes.Count];
}