using System.Numerics;
using Avalonia.Input;
using Fantania.Models;
using FantaniaLib;

namespace Fantania.Views;

public class ViewEditorMode : ILevelEditorMode
{
    public void OnEnter(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnExit(LevelEditorContext context, ControlInputEventArgs e)
    {
        ResetSelectionStates(context);
    }

    public void OnKeyDown(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    public void OnKeyUp(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (e.KeyState.JustReleased == Key.Back || e.KeyState.JustReleased == Key.Delete)
        {
            var selections = context.Workspace.EditorModule.SelectedObjects;
            while (selections.Count > 0)
            {
                var s = selections[0];
                s.OnDelete(context.Workspace);
                selections.RemoveAtFast(0);
            }
            context.Workspace.EditorModule.Notify();
        }
    }

    public void OnMouseMoved(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (_selecting)
        {
            var selection = context.Workspace.EditorModule.Selection;
            selection.Current = e.MouseState.Position.ToVector2();
            if (!selection.IsZero)
            {
                Vector2 origWorld = context.CanvasToWorld(new Vector2(selection.Left, selection.Top));
                Vector2 curWorld = context.CanvasToWorld(new Vector2(selection.Right, selection.Bottom));
                Rectf range = new Rectf(origWorld, curWorld - origWorld);
                context.AddCommand(new RangeSelectionCommand(range, SelectionModeFromKeyModifiers(e.KeyState.KeyModifiers)));
            }
            e.Handled = true;
        }
    }

    public void OnMousePressed(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (e.MouseState.IsLeftButtonPressed)
        {
            Vector2 toCanvas = e.MouseState.Position.ToVector2();
            context.Workspace.EditorModule.Selection.Origin = toCanvas;
            context.Workspace.EditorModule.Selection.Current = toCanvas;
            _selecting = true;
            context.FixCamera = true;
            context.AddCommand(new SetupSelectionCommand(SelectionSetups.Begin));
            e.Handled = true;
        }
    }

    public void OnMouseReleased(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (e.MouseState.IsLeftButtonJustReleased)
        {
            var selection = context.Workspace.EditorModule.Selection;
            if (selection.IsZero)
            {
                Vector2 worldPos = context.CanvasToWorld(e.MouseState.Position.ToVector2());
                context.AddCommand(new ClickSelectionCommand(worldPos, SelectionModeFromKeyModifiers(e.KeyState.KeyModifiers)));
            }
            ResetSelectionStates(context);
            e.Handled = true;
        }
    }

    public void OnMouseWheelChanged(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    void ResetSelectionStates(LevelEditorContext context)
    {
        if (_selecting)
        {
            var selection = context.Workspace.EditorModule.Selection;
            selection.Reset();
            _selecting = false;
            context.FixCamera = false;
            context.AddCommand(new SetupSelectionCommand(SelectionSetups.End));
        }
    }

    SelectionModes SelectionModeFromKeyModifiers(KeyModifiers modifiers)
    {
        if (modifiers.HasFlag(KeyModifiers.Control) && modifiers.HasFlag(KeyModifiers.Shift))
            return SelectionModes.Remove;
        if (modifiers.HasFlag(KeyModifiers.Control))
            return SelectionModes.Add;
        return SelectionModes.Replace;
    }

    bool _selecting = false;
}