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
                return (selections.Count == 1 && selections[0].CanRotate(workspace)) || selections.All(s => s is SingleNodeEntity && s.CanRotate(workspace));
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
        Rectf box = Rectf.Zero;
        foreach (var r in selections)
        {
            box = box.Merge(r.BoundingBox);
        }
        Vector2 tlCvs = canvas.WorldPositionToCanvasPosition(box.TopLeft);
        Vector2 brCvs = canvas.WorldPositionToCanvasPosition(box.BottomRight);
        return (double)(tlCvs.X + brCvs.X) * 0.5;
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
        Rectf box = Rectf.Zero;
        foreach (var r in selections)
        {
            box = box.Merge(r.BoundingBox);
        }
        Vector2 tlCvs = canvas.WorldPositionToCanvasPosition(box.TopLeft);
        Vector2 brCvs = canvas.WorldPositionToCanvasPosition(box.BottomRight);
        return (double)(tlCvs.Y + brCvs.Y) * 0.5;
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

    Vector2 _startWorldPos;
}