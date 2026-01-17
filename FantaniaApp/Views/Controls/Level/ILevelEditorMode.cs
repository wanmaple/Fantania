using FantaniaLib;

namespace Fantania.Views;

public interface ILevelEditorMode
{
    void OnMousePressed(LevelEditorContext context, ControlInputEventArgs e);
    void OnMouseReleased(LevelEditorContext context, ControlInputEventArgs e);
    void OnMouseMoved(LevelEditorContext context, ControlInputEventArgs e);
    void OnMouseWheelChanged(LevelEditorContext context, ControlInputEventArgs e);
    void OnKeyDown(LevelEditorContext context, ControlInputEventArgs e);
    void OnKeyUp(LevelEditorContext context, ControlInputEventArgs e);

    void OnEnter(LevelEditorContext context);
    void OnExit(LevelEditorContext context);
}