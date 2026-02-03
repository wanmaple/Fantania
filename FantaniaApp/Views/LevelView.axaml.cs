using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
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
        foreach (var selectable in lvCanvas.Workspace!.EditorModule.SelectedObjects)
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
        foreach (var selectable in lvCanvas.Workspace!.EditorModule.SelectedObjects)
        {
            selectable.OnTranslating(translated);
        }
    }

    void TransformGizmoOverlay_TranslationEnd(object? sender, GizmoEventArgs e)
    {
    }

    Vector2 _startWorldPos;
}