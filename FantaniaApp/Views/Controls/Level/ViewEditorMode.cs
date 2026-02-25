using System;
using System.Collections.Generic;
using System.Linq;
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
        ResetResizeState(context);
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
        if (_resizePending)
        {
            Vector2 worldPos = context.CanvasToWorld(e.MouseState.Position.ToVector2());
            context.AddCommand(new UpdateResizeCommand(worldPos));
        }
        else if (_selecting)
        {
            var selection = context.Workspace.EditorModule.Selection;
            selection.Current = context.CanvasToWorld(e.MouseState.Position.ToVector2());
            if (!selection.IsZero)
            {
                Rectf range = new Rectf(new Vector2(selection.Left, selection.Top), new Vector2(selection.Width, selection.Height));
                context.AddCommand(new RangeSelectionCommand(range, SelectionModeFromKeyModifiers(e.KeyState.KeyModifiers)));
            }
        }
        e.Handled = true;
    }

    public void OnMousePressed(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (e.MouseState.IsLeftButtonPressed)
        {
            Vector2 worldPos = context.CanvasToWorld(e.MouseState.Position.ToVector2());
            if (CanStartResize(context, context.Workspace.EditorModule.SelectedObjects, e.MouseState.Position.ToVector2(), out var handle))
            {
                _resizePending = true;
                context.AddCommand(new StartResizeCommand(worldPos, e.MouseState.Position.ToVector2(), handle));
            }
            else
            {
                Vector2 toWorld = context.CanvasToWorld(e.MouseState.Position.ToVector2());
                context.Workspace.EditorModule.Selection.Origin = toWorld;
                context.Workspace.EditorModule.Selection.Current = toWorld;
                _selecting = true;
                context.AddCommand(new SetupSelectionCommand(SelectionSetups.Begin));
            }
            e.Handled = true;
        }
    }

    public void OnMouseReleased(LevelEditorContext context, ControlInputEventArgs e)
    {
        if (e.MouseState.IsLeftButtonJustReleased)
        {
            if (_resizePending)
            {
                context.AddCommand(new EndResizeCommand());
                _resizePending = false;
            }
            else
            {
                var selection = context.Workspace.EditorModule.Selection;
                if (selection.IsZero)
                {
                    Vector2 worldPos = context.CanvasToWorld(e.MouseState.Position.ToVector2());
                    if (e.KeyState.KeyModifiers.HasFlag(KeyModifiers.Alt))
                        context.AddCommand(new MultiNodeSelectionCommand(worldPos));
                    else
                        context.AddCommand(new ClickSelectionCommand(worldPos, SelectionModeFromKeyModifiers(e.KeyState.KeyModifiers)));
                }
                ResetSelectionStates(context);
            }
            e.Handled = true;
        }
        else if (e.MouseState.IsRightButtonJustReleased)
        {
            var selections = context.Workspace.EditorModule.SelectedObjects;
            Vector2 worldPos = context.CanvasToWorld(e.MouseState.Position.ToVector2());
            if (selections.Any(s => s.BoundingBox.Contains(worldPos)))
            {
                var groups = SelectionHelper.GroupSelections(selections);
                foreach (var entity in groups.FullySelectedEntities)
                {
                    entity.ResetRotationAndScale();
                }
                foreach (var (container, nodes) in groups.PartiallySelectedNodes)
                {
                    foreach (var node in nodes)
                    {
                        if (node.Rotation != 0.0f || node.Scale != Vector2.One)
                        {
                            var snapshotBefore = node.CreateSnapshot();
                            node.ResetRotationAndScale();
                            var op = new ModifyEntityNodeOperation(context.Workspace, node, snapshotBefore, node.CreateSnapshot());
                            context.Workspace.UndoStack.AddOperation(op);
                        }
                    }
                }
                foreach (var sel in groups.OtherSelectables)
                {
                    sel.ResetRotationAndScale();
                }
                context.Workspace.EditorModule.Notify();
            }
        }
    }

    public void OnMouseWheelChanged(LevelEditorContext context, ControlInputEventArgs e)
    {
    }

    void ResetSelectionStates(LevelEditorContext context)
    {
        if (_selecting)
        {
            _selecting = false;
            context.AddCommand(new SetupSelectionCommand(SelectionSetups.End));
        }
    }

    void ResetResizeState(LevelEditorContext context)
    {
        if (_resizePending)
        {
            _resizePending = false;
            context.AddCommand(new EndResizeCommand());
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

    void DeleteSelections(IReadOnlyList<ISelectableItem> selections, LevelEditorContext context)
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

    bool CanStartResize(LevelEditorContext context, IReadOnlyList<ISelectableItem> selections, Vector2 canvasPos, out ResizeHandleTypes handle)
    {
        if (!TryGetResizeHandle(context, selections, canvasPos, out handle))
            return false;
        return true;
    }

    bool TryGetResizeHandle(LevelEditorContext context, IReadOnlyList<ISelectableItem> selections, Vector2 canvasPos, out ResizeHandleTypes handle)
    {
        handle = ResizeHandleTypes.None;
        if (selections.Count <= 0)
            return false;
        foreach (var sel in selections)
        {
            if (sel is not ISizeableEntity)
                return false;
        }
        float minDistanceSq = float.MaxValue;
        foreach (var sel in selections)
        {
            if (TryHitResizeHandle(context, sel.BoundingBox, canvasPos, out var candidate, out float distanceSq) && distanceSq < minDistanceSq)
            {
                minDistanceSq = distanceSq;
                handle = candidate;
            }
        }
        return handle != ResizeHandleTypes.None;
    }

    bool TryHitResizeHandle(LevelEditorContext context, Rectf bound, Vector2 canvasPos, out ResizeHandleTypes handle, out float distanceSq)
    {
        handle = ResizeHandleTypes.None;
        distanceSq = float.MaxValue;
        if (bound.IsZero)
            return false;
        Vector2 leftTop = context.WorldToCanvas(bound.TopLeft);
        Vector2 rightTop = context.WorldToCanvas(bound.TopRight);
        Vector2 leftBottom = context.WorldToCanvas(bound.BottomLeft);
        Vector2 rightBottom = context.WorldToCanvas(bound.BottomRight);
        float left = MathF.Min(leftTop.X, leftBottom.X);
        float right = MathF.Max(rightTop.X, rightBottom.X);
        float top = MathF.Min(leftTop.Y, rightTop.Y);
        float bottom = MathF.Max(leftBottom.Y, rightBottom.Y);
        const float CORNER_RADIUS = 10.0f;
        const float EDGE_THICKNESS = 8.0f;
        float cornerRadiusSq = CORNER_RADIUS * CORNER_RADIUS;
        float dTopLeft = (canvasPos - new Vector2(left, top)).LengthSquared();
        float dTopRight = (canvasPos - new Vector2(right, top)).LengthSquared();
        float dBottomLeft = (canvasPos - new Vector2(left, bottom)).LengthSquared();
        float dBottomRight = (canvasPos - new Vector2(right, bottom)).LengthSquared();
        bool cornerHit = false;
        if (dTopLeft <= cornerRadiusSq)
        {
            cornerHit = true;
            handle = ResizeHandleTypes.Left | ResizeHandleTypes.Top;
            distanceSq = dTopLeft;
        }
        if (dTopRight <= cornerRadiusSq && dTopRight < distanceSq)
        {
            cornerHit = true;
            handle = ResizeHandleTypes.Right | ResizeHandleTypes.Top;
            distanceSq = dTopRight;
        }
        if (dBottomLeft <= cornerRadiusSq && dBottomLeft < distanceSq)
        {
            cornerHit = true;
            handle = ResizeHandleTypes.Left | ResizeHandleTypes.Bottom;
            distanceSq = dBottomLeft;
        }
        if (dBottomRight <= cornerRadiusSq && dBottomRight < distanceSq)
        {
            cornerHit = true;
            handle = ResizeHandleTypes.Right | ResizeHandleTypes.Bottom;
            distanceSq = dBottomRight;
        }
        if (cornerHit)
            return true;
        if (canvasPos.X >= left - EDGE_THICKNESS && canvasPos.X <= left + EDGE_THICKNESS && canvasPos.Y >= top - EDGE_THICKNESS && canvasPos.Y <= bottom + EDGE_THICKNESS)
        {
            float candidateDistanceSq = MathF.Abs(canvasPos.X - left) * MathF.Abs(canvasPos.X - left);
            if (candidateDistanceSq < distanceSq)
            {
                handle = ResizeHandleTypes.Left;
                distanceSq = candidateDistanceSq;
            }
        }
        if (canvasPos.X >= right - EDGE_THICKNESS && canvasPos.X <= right + EDGE_THICKNESS && canvasPos.Y >= top - EDGE_THICKNESS && canvasPos.Y <= bottom + EDGE_THICKNESS)
        {
            float candidateDistanceSq = MathF.Abs(canvasPos.X - right) * MathF.Abs(canvasPos.X - right);
            if (candidateDistanceSq < distanceSq)
            {
                handle = ResizeHandleTypes.Right;
                distanceSq = candidateDistanceSq;
            }
        }
        if (canvasPos.Y >= top - EDGE_THICKNESS && canvasPos.Y <= top + EDGE_THICKNESS && canvasPos.X >= left - EDGE_THICKNESS && canvasPos.X <= right + EDGE_THICKNESS)
        {
            float candidateDistanceSq = MathF.Abs(canvasPos.Y - top) * MathF.Abs(canvasPos.Y - top);
            if (candidateDistanceSq < distanceSq)
            {
                handle = ResizeHandleTypes.Top;
                distanceSq = candidateDistanceSq;
            }
        }
        if (canvasPos.Y >= bottom - EDGE_THICKNESS && canvasPos.Y <= bottom + EDGE_THICKNESS && canvasPos.X >= left - EDGE_THICKNESS && canvasPos.X <= right + EDGE_THICKNESS)
        {
            float candidateDistanceSq = MathF.Abs(canvasPos.Y - bottom) * MathF.Abs(canvasPos.Y - bottom);
            if (candidateDistanceSq < distanceSq)
            {
                handle = ResizeHandleTypes.Bottom;
                distanceSq = candidateDistanceSq;
            }
        }
        return handle != ResizeHandleTypes.None;
    }

    bool _selecting = false;
    bool _resizePending = false;
}