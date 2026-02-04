using System.Collections.Generic;
using System.Numerics;
using Avalonia.Input;
using Fantania.Localization;
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
            DeleteSelections(selections, context);
        }
        else if (e.KeyState.JustReleased == Key.N)
        {
            var selections = context.Workspace.EditorModule.SelectedObjects;
            var multiNodeEntities = new HashSet<IMultiNodeContainer>(8);
            foreach (var sel in selections)
            {
                if (sel is LevelEntityNode node)
                {
                    if (multiNodeEntities.Add(node.Owner))
                        node.Owner.AppendNode(context.Workspace, context.Workspace.EditorModule.MouseWorldPosition.ToVector2());
                }
            }
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

    void DeleteSelections(IList<ISelectableItem> selections, LevelEditorContext context)
    {
        var groups = SelectionHelper.GroupSelections(selections);
        var entitiesToDelete = new HashSet<LevelEntity>(16);
        var nodesDelFailed = new List<ISelectableItem>(8);
        foreach (var entity in groups.FullySelectedEntities)
        {
            entitiesToDelete.Add(entity);
        }
        foreach (var sel in groups.OtherSelectables)
        {
            if (sel is LevelEntity entity)
                entitiesToDelete.Add(entity);
        }
        foreach (var entity in entitiesToDelete)
        {
            context.Workspace.LevelModule.DeleteEntity(entity);
        }
        bool hasDeletionFailure = false;
        foreach (var (container, nodesToRemove) in groups.PartiallySelectedNodes)
        {
            var snapshot = new EntityNodesSnapshot(nodesToRemove);
            if (!container.RemoveNodes(context.Workspace, snapshot))
            {
                hasDeletionFailure = true;
                nodesDelFailed.AddRange(snapshot.Nodes);
            }
        }
        if (hasDeletionFailure)
        {
            context.Workspace.LogWarning(LocalizationHelper.GetLocalizedString("Warn_DeleteNodesFailed"));
        }
        context.Workspace.EditorModule.Notify();
    }

    bool _selecting = false;
}