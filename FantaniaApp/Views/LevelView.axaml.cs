using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public class BoundingBox2CanvasRectConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not Rectf bounds || values[1] is not ILevelCanvas canvas) return AvaloniaProperty.UnsetValue;
        Vector2 tlWorld = bounds.TopLeft;
        Vector2 brWorld = bounds.BottomRight;
        Vector2 tlCanvas = canvas.WorldPositionToCanvasPosition(tlWorld);
        Vector2 brCanvas = canvas.WorldPositionToCanvasPosition(brWorld);
        return new Rect(tlCanvas.X, tlCanvas.Y, brCanvas.X - tlCanvas.X, brCanvas.Y - tlCanvas.Y);
    }
}

public class Selections2VisibleConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 3) return AvaloniaProperty.UnsetValue;
        if (values[0] is not IReadOnlyList<ISelectableItem> selections || values[1] is not TransformGizmoTypes mode || values[2] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        if (selections.Count > 0)
        {
            if (mode == TransformGizmoTypes.None)
                return false;
            if (mode == TransformGizmoTypes.Translation)
                return selections.All(s => s.CanTranslate(workspace));
            if (mode == TransformGizmoTypes.Rotation)
            {
                var groups = SelectionHelper.GroupSelections(selections);
                return groups.FullySelectedEntities.Count + groups.PartiallySelectedNodes.Sum(pair => pair.Value.Count) + groups.OtherSelectables.Count == 1;
            }
            if (mode == TransformGizmoTypes.Scale)
                return selections.All(s => s.CanScale(workspace));
        }
        return false;
    }
}

public class Selections2CenterXConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not IReadOnlyList<ISelectableItem> selections) return AvaloniaProperty.UnsetValue;
        if (selections.Count <= 0) return AvaloniaProperty.UnsetValue;
        if (values[1] is not ILevelCanvas canvas) return AvaloniaProperty.UnsetValue;
        var groups = SelectionHelper.GroupSelections(selections);
        Vector2 sum = Vector2.Zero;
        int num = 0;
        foreach (var multiNodeEntity in groups.FullySelectedEntities)
        {
            sum += multiNodeEntity.Position.ToVector2();
            num++;
        }
        foreach (var nodeList in groups.PartiallySelectedNodes.Values)
        {
            foreach (var node in nodeList)
            {
                sum += node.WorldPosition;
                num++;
            }
        }
        foreach (var other in groups.OtherSelectables)
        {
            sum += other.WorldPosition;
            num++;
        }
        Vector2 avg = sum / num;
        Vector2 cvs = canvas.WorldPositionToCanvasPosition(avg);
        return (double)cvs.X;
    }
}

public class Selections2CenterYConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not IReadOnlyList<ISelectableItem> selections) return AvaloniaProperty.UnsetValue;
        if (selections.Count <= 0) return AvaloniaProperty.UnsetValue;
        if (values[1] is not ILevelCanvas canvas) return AvaloniaProperty.UnsetValue;
        var groups = SelectionHelper.GroupSelections(selections);
        Vector2 sum = Vector2.Zero;
        int num = 0;
        foreach (var multiNodeEntity in groups.FullySelectedEntities)
        {
            sum += multiNodeEntity.Position.ToVector2();
            num++;
        }
        foreach (var nodeList in groups.PartiallySelectedNodes.Values)
        {
            foreach (var node in nodeList)
            {
                sum += node.WorldPosition;
                num++;
            }
        }
        foreach (var other in groups.OtherSelectables)
        {
            sum += other.WorldPosition;
            num++;
        }
        Vector2 avg = sum / num;
        Vector2 cvs = canvas.WorldPositionToCanvasPosition(avg);
        return (double)cvs.Y;
    }
}

public partial class LevelView : UserControl
{
    LevelViewModel ViewModel => (LevelViewModel)DataContext!;

    public LevelView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ViewModel.Workspace.EditorModule.PropertyChanged += OnEditorModulePropertyChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        ViewModel.Workspace.EditorModule.PropertyChanged -= OnEditorModulePropertyChanged;
    }

    void OnEditorModulePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorModule.SelectedObjects) || e.PropertyName == nameof(EditorModule.NotifyFlag))
        {
            RedrawNodeConnections();
        }
    }

    private void RedrawNodeConnections()
    {
        connectionOverlay.Children.Clear();
        var workspace = ViewModel.Workspace;
        if (workspace.LevelModule.CurrentLevel == null)
            return;
        var selections = workspace.EditorModule.SelectedObjects;
        var nodes = selections.OfType<LevelEntityNode>().GroupBy(n => n.Owner);
        foreach (var group in nodes)
        {
            if (group.Count() != group.Key.AllNodes.Count || group.Count() <= 1)
                continue;
            var ordered = group.OrderBy(n => n.LocalOrder).ToArray();
            Rectf bound = Rectf.Zero;
            for (int i = 0; i < ordered.Length - 1; i++)
            {
                Vector2 pt1 = group.Key.TransformAt(i) * Vector2.Zero;
                Vector2 pt2 = group.Key.TransformAt(i + 1) * Vector2.Zero;
                Vector2 p1 = lvCanvas.WorldPositionToCanvasPosition(pt1);
                Vector2 p2 = lvCanvas.WorldPositionToCanvasPosition(pt2);
                var line = new Polyline
                {
                    Stroke = Brushes.White,
                    StrokeThickness = 2.0,
                    Points = new Points { new(p1.X, p1.Y), new(p2.X, p2.Y) }
                };
                connectionOverlay.Children.Add(line);
                bound = bound.Merge(ordered[i].BoundingBox);
            }
            if (ordered.Length > 0)
                bound = bound.Merge(ordered[^1].BoundingBox);
            // Vector2 tl = lvCanvas.WorldPositionToCanvasPosition(bound.TopLeft);
            // Vector2 br = lvCanvas.WorldPositionToCanvasPosition(bound.BottomRight);
            // var rect = new Rectangle
            // {
            //     Stroke = Brushes.White,
            //     StrokeThickness = 1.0,
            //     Fill = null,
            //     Width = br.X - tl.X,
            //     Height = br.Y - tl.Y
            // };
            // Canvas.SetLeft(rect, tl.X);
            // Canvas.SetTop(rect, tl.Y);
            // connectionOverlay.Children.Add(rect);
            Vector2 parentWorldPos = group.Key.SelfTransform * Vector2.Zero;
            Vector2 parentCanvasPos = lvCanvas.WorldPositionToCanvasPosition(parentWorldPos);
            DrawParentAnchor(parentCanvasPos);
        }
    }

    void DrawParentAnchor(Vector2 pos)
    {
        double len = 8.0;
        var polygon = new Polygon
        {
            Fill = Brushes.White,
            Points = new Points
            {
                new(pos.X + len, pos.Y),
                new(pos.X, pos.Y + len),
                new(pos.X - len, pos.Y),
                new(pos.X, pos.Y - len),
            }
        };
        connectionOverlay.Children.Add(polygon);
    }

    void Grid_PointerMoved(object? sender, PointerEventArgs e)
    {
        Point pt = e.GetPosition(gizmo);
        overlay.IsHitTestVisible = gizmo.HitTest(pt);
        if (!overlay.IsHitTestVisible)
            gizmo.Reset();
    }

    void TransformGizmoOverlay_TranslationStart(object? sender, GizmoEventArgs e)
    {
        Point pt = e.PointerArgs.GetPosition(this);
        _startWorldPos = lvCanvas.CanvasPositionToWorldPosition(pt.ToVector2());
        var workspace = lvCanvas.Workspace!;
        var groups = SelectionHelper.GroupSelections(workspace.EditorModule.SelectedObjects);
        foreach (var entity in groups.FullySelectedEntities)
        {
            entity.OnTranslateBegin();
        }
        foreach (var nodeList in groups.PartiallySelectedNodes.Values)
        {
            foreach (var node in nodeList)
            {
                node.OnTranslateBegin();
            }
        }
        foreach (var selectable in groups.OtherSelectables)
        {
            selectable.OnTranslateBegin();
        }
    }

    void TransformGizmoOverlay_Translating(object? sender, GizmoEventArgs e)
    {
        Point pt = e.PointerArgs.GetPosition(this);
        Vector2 worldPos = lvCanvas.CanvasPositionToWorldPosition(pt.ToVector2());
        Vector2 movement = worldPos - _startWorldPos;
        if (e.Handle == TransformGizmoHandles.AxisX)
            movement = movement.WithX();
        else if (e.Handle == TransformGizmoHandles.AxisY)
            movement = movement.WithY();
        Vector2Int translated = movement.ToGridSpace(lvCanvas.EditConfig.GridAlign);
        var workspace = lvCanvas.Workspace!;
        var groups = SelectionHelper.GroupSelections(workspace.EditorModule.SelectedObjects);
        foreach (var entity in groups.FullySelectedEntities)
        {
            entity.OnTranslating(translated);
        }
        foreach (var nodeList in groups.PartiallySelectedNodes.Values)
        {
            foreach (var node in nodeList)
            {
                var snapshotBefore = node.CreateSnapshot();
                node.OnTranslating(translated);
                var op = new ModifyEntityNodeOperation(workspace, node, snapshotBefore, node.CreateSnapshot());
                workspace.UndoStack.AddOperation(op);
            }
        }
        foreach (var selectable in groups.OtherSelectables)
        {
            selectable.OnTranslating(translated);
        }
    }

    void TransformGizmoOverlay_TranslationEnd(object? sender, GizmoEventArgs e)
    {
    }

    void TransformGizmoOverlay_RotationStart(object? sender, GizmoEventArgs e)
    {
        Point pt = e.PointerArgs.GetPosition(this);
        _startWorldPos = lvCanvas.CanvasPositionToWorldPosition(pt.ToVector2());
        // 会进入这里是有个大前提的，这个并不要求像游戏引擎那样需要让多个节点围绕一个中心点旋转，这只是个编辑器，这种操作一般是不合理的，所以这里就可以简单处理，只需要考虑单个对象的旋转。
        var workspace = lvCanvas.Workspace!;
        var groups = SelectionHelper.GroupSelections(workspace.EditorModule.SelectedObjects);
        if (groups.FullySelectedEntities.Count > 0)
        {
            var entity = groups.FullySelectedEntities[0];
            _rotationCenter = entity.Position.ToVector2();
            entity.OnRotateBegin();
        }
        else
        {
            var selectable = workspace.EditorModule.SelectedObjects[0];
            _rotationCenter = selectable.WorldPosition;
            selectable.OnRotateBegin();
        }
        _gizmoRotationAngle = 0.0;
        gizmo.UpdateRotationVisual(_gizmoRotationAngle, e.PointerArgs.GetPosition(gizmo), true);
    }

    void TransformGizmoOverlay_Rotating(object? sender, GizmoEventArgs e)
    {
        Point pt = e.PointerArgs.GetPosition(this);
        Vector2 worldPos = lvCanvas.CanvasPositionToWorldPosition(pt.ToVector2());
        Vector2 dirStart = Vector2.Normalize(_startWorldPos - _rotationCenter);
        Vector2 dirNow = Vector2.Normalize(worldPos - _rotationCenter);
        float radianChange = dirNow.IsZero() ? 0.0f : (float)MathF.Asin(MathHelper.Clamp(dirStart.Cross(dirNow), -1.0f, 1.0f));
        var workspace = lvCanvas.Workspace!;
        var groups = SelectionHelper.GroupSelections(workspace.EditorModule.SelectedObjects);
        if (groups.FullySelectedEntities.Count > 0)
        {
            var entity = groups.FullySelectedEntities[0];
            entity.OnRotating(-radianChange);         
        }
        else
        {
            var selectable = workspace.EditorModule.SelectedObjects[0];
            if (selectable is LevelEntityNode node)
            {
                var snapshotBefore = node.CreateSnapshot();
                node.OnRotating(-radianChange);
                var op = new ModifyEntityNodeOperation(workspace, node, snapshotBefore, node.CreateSnapshot());
                workspace.UndoStack.AddOperation(op);
            }
            else
                selectable.OnRotating(-radianChange);
        }
        _startWorldPos = worldPos;
        _gizmoRotationAngle += radianChange;    // Avalonia的旋转方向和我自己定义的旋转方向是反的
        gizmo.UpdateRotationVisual(_gizmoRotationAngle, e.PointerArgs.GetPosition(gizmo), true);
    }

    void TransformGizmoOverlay_RotationEnd(object? sender, GizmoEventArgs e)
    {
        _gizmoRotationAngle = 0.0;
        gizmo.UpdateRotationVisual(0.0, default, false);
    }

    void TransformGizmoOverlay_ScaleStart(object? sender, GizmoEventArgs e)
    {
        Point pt = e.PointerArgs.GetPosition(this);
        _startWorldPos = lvCanvas.CanvasPositionToWorldPosition(pt.ToVector2());
        _scaleCenter = GetSelectionCenter(lvCanvas.Workspace!.EditorModule.SelectedObjects);
        _scaleCenterCanvas = lvCanvas.WorldPositionToCanvasPosition(_scaleCenter);
        _startCanvasPos = lvCanvas.WorldPositionToCanvasPosition(_startWorldPos);
        var workspace = lvCanvas.Workspace!;
        var groups = SelectionHelper.GroupSelections(workspace.EditorModule.SelectedObjects);
        foreach (var entity in groups.FullySelectedEntities)
        {
            entity.OnScaleBegin();
        }
        foreach (var nodeList in groups.PartiallySelectedNodes.Values)
        {
            foreach (var node in nodeList)
            {
                node.OnScaleBegin();
            }
        }
        foreach (var selectable in groups.OtherSelectables)
        {
            selectable.OnScaleBegin();
        }
        _scaleFactor = Vector2.One;
        gizmo.UpdateScaleVisual(_scaleFactor);
    }

    void TransformGizmoOverlay_Scaling(object? sender, GizmoEventArgs e)
    {
        Point pt = e.PointerArgs.GetPosition(lvCanvas);
        Vector2 startDelta = _startCanvasPos - _scaleCenterCanvas;
        Vector2 nowDelta = pt.ToVector2() - _scaleCenterCanvas;
        const float EPSILON = 0.0001f;
        const float MIN_START_LENGTH = 24.0f;
        float scaleX = 1.0f;
        float scaleY = 1.0f;
        if (e.Handle == TransformGizmoHandles.Center)
        {
            float startLen = startDelta.Length();
            float denom = MathF.Max(startLen, MIN_START_LENGTH);
            Vector2 dir = startLen > EPSILON ? Vector2.Normalize(startDelta) : Vector2.UnitX;
            float factor = Vector2.Dot(nowDelta, dir) / denom;
            scaleX = factor;
            scaleY = factor;
        }
        else if (e.Handle == TransformGizmoHandles.AxisX)
        {
            float denom = MathF.Abs(startDelta.X) > EPSILON ? startDelta.X : MathF.Sign(nowDelta.X == 0.0f ? 1.0f : nowDelta.X) * MIN_START_LENGTH;
            scaleX = nowDelta.X / denom;
        }
        else if (e.Handle == TransformGizmoHandles.AxisY)
        {
            float denom = MathF.Abs(startDelta.Y) > EPSILON ? startDelta.Y : MathF.Sign(nowDelta.Y == 0.0f ? 1.0f : nowDelta.Y) * MIN_START_LENGTH;
            scaleY = nowDelta.Y / denom;
        }
        _scaleFactor = new Vector2(scaleX, scaleY);
        var workspace = lvCanvas.Workspace!;
        var groups = SelectionHelper.GroupSelections(workspace.EditorModule.SelectedObjects);
        foreach (var entity in groups.FullySelectedEntities)
        {
            entity.OnScaling(_scaleFactor);
        }
        foreach (var nodeList in groups.PartiallySelectedNodes.Values)
        {
            foreach (var node in nodeList)
            {
                var snapshotBefore = node.CreateSnapshot();
                node.OnScaling(_scaleFactor);
                var op = new ModifyEntityNodeOperation(workspace, node, snapshotBefore, node.CreateSnapshot());
                workspace.UndoStack.AddOperation(op);
            }
        }
        foreach (var selectable in groups.OtherSelectables)
        {
            selectable.OnScaling(_scaleFactor);
        }
        gizmo.UpdateScaleVisual(_scaleFactor);
    }

    void TransformGizmoOverlay_ScaleEnd(object? sender, GizmoEventArgs e)
    {
        _scaleFactor = Vector2.One;
        gizmo.UpdateScaleVisual(_scaleFactor);
    }

    Vector2 GetSelectionCenter(IReadOnlyList<ISelectableItem> selections)
    {
        var groups = SelectionHelper.GroupSelections(selections);
        Vector2 sum = Vector2.Zero;
        int num = 0;
        foreach (var multiNodeEntity in groups.FullySelectedEntities)
        {
            sum += multiNodeEntity.Position.ToVector2();
            num++;
        }
        foreach (var nodeList in groups.PartiallySelectedNodes.Values)
        {
            foreach (var node in nodeList)
            {
                sum += node.WorldPosition;
                num++;
            }
        }
        foreach (var other in groups.OtherSelectables)
        {
            sum += other.WorldPosition;
            num++;
        }
        return num > 0 ? sum / num : Vector2.Zero;
    }

    Vector2 _startWorldPos;
    Vector2 _rotationCenter;
    double _gizmoRotationAngle;
    Vector2 _scaleCenter;
    Vector2 _scaleCenterCanvas;
    Vector2 _startCanvasPos;
    Vector2 _scaleFactor;
}