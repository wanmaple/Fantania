using System;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Input;
using DynamicData;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class LevelCanvasInputHandler : ICanvasInputHandler
{
    private enum DragModes
    {
        None,
        Selecting,
        Translating,
        Sizing,
    }

    public bool IsEnabled { get; set; } = true;

    public LevelCanvasInputHandler(LevelCanvas canvas)
    {
        _canvas = canvas;
        _lv = canvas.Level;
    }

    public void Tick(TimeSpan dt)
    {
        if (!IsEnabled) return;
        if (_sizeTool.SizingObject != null)
        {
            if (_lastMousePosition != null)
            {
                const float MAX_SCROLL_DISTANCE = 40.0f;
                float mouseX = (float)_lastMousePosition.Value.X;
                float mouseY = (float)_lastMousePosition.Value.Y;
                Vector2 scroll = Vector2.Zero;
                float tx = 0.0f, ty = 0.0f;
                Vector2 unit = Vector2.Zero;
                if (mouseX <= MAX_SCROLL_DISTANCE)
                {
                    tx = mouseX / MAX_SCROLL_DISTANCE;
                    unit.X = 1.0f;
                }
                else if (mouseX >= _canvas.Bounds.Width - MAX_SCROLL_DISTANCE)
                {
                    tx = ((float)_canvas.Bounds.Width - mouseX) / MAX_SCROLL_DISTANCE;
                    unit.X = -1.0f;
                }
                if (mouseY <= MAX_SCROLL_DISTANCE)
                {
                    ty = mouseY / MAX_SCROLL_DISTANCE;
                    unit.Y = -1.0f;
                }
                else if (mouseY >= _canvas.Bounds.Height - MAX_SCROLL_DISTANCE)
                {
                    ty = ((float)_canvas.Bounds.Height - mouseY) / MAX_SCROLL_DISTANCE;
                    unit.Y = 1.0f;
                }
                tx = MathHelper.Clamp(tx, 0.5f, 1.0f);
                ty = MathHelper.Clamp(tx, 0.5f, 1.0f);
                if (!unit.IsZero())
                {
                    float speed = 160.0f / 60.0f;
                    _canvas.TranslateView(unit * speed / new Vector2(tx, ty));
                    Avalonia.Vector worldPos = CanvasPositionToAlignedWorldPosition(new Point(mouseX, mouseY));
                    _sizeTool.Apply(worldPos);
                    _canvas.PointerLevelPosition = worldPos;
                }
            }
        }
    }

    public bool MouseClick(Point relativePosition, MouseButton button, KeyModifiers modifiers)
    {
        if (!IsEnabled) return false;
        if (button == MouseButton.Left)
        {
            if (_lv.AddingObject != null)
            {
                if (_lv.AddingObject is not ISizeableObject)
                {
                    DrawTemplate template = _lv.AddingObject.Template;
                    _lv.PlaceAdding();
                    // continue adding.
                    Point posCanvas = relativePosition;
                    ReadyAdd(template, posCanvas);
                }
            }
            else
            {
                var toWorld = CanvasPositionToAlignedWorldPosition(relativePosition);
                Point worldPos = new Point(toWorld.X, -toWorld.Y);
                var insides = _lv.PointTest(worldPos, null);
                if (insides.Count > 0)
                {
                    var selections = insides.Where(obj => obj.IsSelected).ToArray();
                    if (!modifiers.IsModifierPressed(KeyModifiers.Control))
                    {
                        var vm = WorkspaceViewModel.Current;
                        for (int i = vm.SelectedObjects.Count - 1; i >= 0; i--)
                        {
                            var obj = vm.SelectedObjects[i] as LevelObject;
                            if (!insides.Contains(obj))
                                obj.IsSelected = false;
                        }
                        if (selections.Length == insides.Count)
                        {
                            for (int i = 1; i < insides.Count; i++)
                                insides[i].IsSelected = false;
                        }
                        else if (selections.Length > 1)
                        {
                            insides.OrderBy(obj => obj.RealDepth).First(obj => !obj.IsSelected).IsSelected = true;
                            foreach (var selection in selections)
                            {
                                selection.IsSelected = false;
                            }
                        }
                        else if (selections.Length == 1)
                        {
                            selections[0].IsSelected = false;
                            int index = insides.IndexOf(selections[0]);
                            insides[(index + 1) % insides.Count].IsSelected = true;
                        }
                        else
                        {
                            insides[0].IsSelected = true;
                        }
                    }
                    else
                    {
                        // 优先选中未选中的，其次根据深度值取消选中。
                        if (selections.Length != insides.Count)
                        {
                            insides.First(obj => !obj.IsSelected).IsSelected = true;
                        }
                        else
                        {
                            insides.OrderBy(obj => obj.RealDepth).First().IsSelected = false;
                        }
                    }
                }
                else
                {
                    var vm = WorkspaceViewModel.Current;
                    for (int i = vm.SelectedObjects.Count - 1; i >= 0; i--)
                    {
                        (vm.SelectedObjects[i] as LevelObject).IsSelected = false;
                    }
                }
            }
        }
        WorkspaceViewModel.Current.Workspace.UndoStack.ForceStopMerging();
        return false;
    }

    public bool MouseDragging(Point relativePosition, Point changed, MouseButton button, KeyModifiers modifiers)
    {
        if (!IsEnabled) return false;
        if (button == MouseButton.Left)
        {
            WorkspaceViewModel vm = WorkspaceViewModel.Current;
            SizeTool.SizeDirections sizeDir = SizeTool.SizeDirections.None;
            if (_dragMode == DragModes.None)
            {
                Avalonia.Vector worldPos = CanvasPositionToAlignedWorldPosition(relativePosition);
                if (_lv.AddingObject != null && _lv.AddingObject.CreateMode == CreateModes.Sizeable)
                {
                    _dragMode = DragModes.Sizing;
                }
                else if (vm.SelectedObjects.Count == 1 && vm.SelectedObjects[0] is ISizeableObject sizeable && ShouldResize(sizeable, CanvasPositionToWorldPosition(relativePosition - changed), out sizeDir))
                {
                    _dragMode = DragModes.Sizing;
                }
                else
                {
                    Point pt = new Point(worldPos.X, -worldPos.Y);
                    foreach (var obj in vm.SelectedObjects)
                    {
                        if (obj is not LevelObject worldObj) continue;
                        if (worldObj.BoundingBox.Contains(pt))
                        {
                            _dragMode = DragModes.Translating;
                            break;
                        }
                    }
                    if (_dragMode == DragModes.None)
                    {
                        _dragMode = DragModes.Selecting;
                    }
                }
            }

            Vector2 worldChanged = _canvas.CanvasMovementToViewMovement(changed).ToVector2();
            if (_dragMode == DragModes.Selecting)
            {
                if (_lv.AddingObject == null)
                {
                    // do selection here.
                    if (!_selection.IsEnabled)
                    {
                        if (!modifiers.IsModifierPressed(KeyModifiers.Control))
                        {
                            _lv.DeselectAll();
                        }
                        _selectionStart = relativePosition - changed;
                        _canvas.SelectionLeft = Math.Min(_selectionStart.X, relativePosition.X);
                        _canvas.SelectionTop = Math.Min(_selectionStart.Y, relativePosition.Y);
                        _canvas.SelectionWidth = Math.Abs(changed.X);
                        _canvas.SelectionHeight = Math.Abs(changed.Y);
                        _selection.IsEnabled = true;
                    }
                    else
                    {
                        if (relativePosition.X >= _selectionStart.X)
                        {
                            _canvas.SelectionLeft = _selectionStart.X;
                            _canvas.SelectionWidth = relativePosition.X - _selectionStart.X;
                        }
                        else
                        {
                            _canvas.SelectionLeft = relativePosition.X;
                            _canvas.SelectionWidth = _selectionStart.X - relativePosition.X;
                        }
                        if (relativePosition.Y >= _selectionStart.Y)
                        {
                            _canvas.SelectionTop = _selectionStart.Y;
                            _canvas.SelectionHeight = relativePosition.Y - _selectionStart.Y;
                        }
                        else
                        {
                            _canvas.SelectionTop = relativePosition.Y;
                            _canvas.SelectionHeight = _selectionStart.Y - relativePosition.Y;
                        }
                    }
                    if (_canvas.SelectionWidth != 0.0 || _canvas.SelectionHeight != 0.0)
                    {
                        Point bl = new Point(_canvas.SelectionLeft, _canvas.SelectionTop + _canvas.SelectionHeight);
                        Point tr = bl + new Point(_canvas.SelectionWidth, -_canvas.SelectionHeight);
                        Avalonia.Vector worldBL = _canvas.CanvasPositionToGLPosition(bl);
                        Avalonia.Vector worldTR = _canvas.CanvasPositionToGLPosition(tr);
                        Rect range = new Rect(new Point(worldBL.X, worldBL.Y), new Point(worldTR.X, worldTR.Y));
                        _selection.Apply(_lv, range);
                    }
                }
            }
            else if (_dragMode == DragModes.Translating)
            {
                _draggingAccumulation += worldChanged;
                Vector2 movement = Vector2.Zero;
                int align = Preferences.Singleton.LevelSettings.AlignSize;
                while (MathF.Abs(_draggingAccumulation.X) >= align)
                {
                    movement.X += align * MathF.Sign(_draggingAccumulation.X);
                    _draggingAccumulation.X -= MathF.Sign(_draggingAccumulation.X) * align;
                }
                while (MathF.Abs(_draggingAccumulation.Y) >= align)
                {
                    movement.Y += align * MathF.Sign(_draggingAccumulation.Y);
                    _draggingAccumulation.Y -= MathF.Sign(_draggingAccumulation.Y) * align;
                }
                if (movement != Vector2.Zero)
                {
                    foreach (var obj in vm.SelectedObjects)
                    {
                        if (obj is not LevelObject lvObj) continue;
                        lvObj.Position += new Vector2(movement.X, -movement.Y);
                    }
                }
            }
            else if (_dragMode == DragModes.Sizing)
            {
                ISizeableObject sizeable = _lv.AddingObject != null ? (_lv.AddingObject as ISizeableObject) : vm.SelectedObjects[0] as ISizeableObject;
                if (_sizeTool.SizingObject == null)
                {
                    Point start = relativePosition - changed;
                    if (_lv.AddingObject != null)
                    {
                        Avalonia.Vector worldStart = CanvasPositionToAlignedWorldPosition(start);
                        _lv.AddingObject.Position = worldStart;
                        sizeDir = SizeTool.SizeDirections.BottomRight;
                    }
                    _sizeTool.Prepare(sizeable, sizeDir);
                }
                else
                {
                    Avalonia.Vector worldCurrent = CanvasPositionToAlignedWorldPosition(relativePosition);
                    _sizeTool.Apply(worldCurrent);
                }
            }
        }
        else if (button == MouseButton.Middle)
        {
            Vector2 worldChanged = _canvas.CanvasMovementToViewMovement(changed).ToVector2();
            _canvas.TranslateView(worldChanged * _canvas.ViewScale);
        }
        return false;
    }

    public bool MouseDraggingEnd(Point relativePosition, MouseButton button)
    {
        if (button == MouseButton.Left)
        {
            if (_dragMode == DragModes.Sizing)
            {
                ISizeableObject sizeable = _lv.AddingObject as ISizeableObject;
                if (sizeable != null && sizeable.CustomSize.X != 0.0 && sizeable.CustomSize.Y != 0.0)
                {
                    DrawTemplate template = _lv.AddingObject.Template;
                    _lv.PlaceAdding();
                    // continue adding.
                    Point posCanvas = relativePosition;
                    ReadyAdd(template, posCanvas);
                }
                _sizeTool.Cancel();
            }
            _selection.Reset();
            _selection.IsEnabled = false;
            _canvas.SelectionWidth = _canvas.SelectionHeight = 0.0;
            _draggingAccumulation = Vector2.Zero;
            _dragMode = DragModes.None;
        }
        WorkspaceViewModel.Current.Workspace.UndoStack.ForceStopMerging();
        return false;
    }

    public bool MouseEnter(Point relativePosition)
    {
        if (!IsEnabled) return false;
        WorkspaceViewModel vm = WorkspaceViewModel.Current;
        if (vm.SelectedObjects.Count == 1 && vm.SelectedObjects[0] is DrawTemplate template)
        {
            ReadyAdd(template, relativePosition);
        }
        _canvas.Focus();
        return false;
    }

    public bool MouseExit()
    {
        if (!IsEnabled) return false;
        if (_lv.AddingObject != null)
        {
            _lv.CancelAdd();
        }
        _draggingAccumulation = Vector2.Zero;
        _selection.IsEnabled = false;
        _canvas.SelectionWidth = _canvas.SelectionHeight = 0.0;
        _sizeTool.Cancel();
        _lastMousePosition = null;
        return false;
    }

    public bool MouseMoving(Point relativePosition, KeyModifiers modifiers)
    {
        if (!IsEnabled) return false;
        Avalonia.Vector worldPos = CanvasPositionToAlignedWorldPosition(relativePosition);
        _canvas.PointerLevelPosition = worldPos;
        if (_lv.AddingObject != null && _lv.AddingObject.CreateMode != CreateModes.Sizeable)
        {
            _lv.AddingObject.Position = worldPos;
        }
        _lastMousePosition = relativePosition;
        return false;
    }

    public bool MouseScrolling(Point relativePosition, Avalonia.Vector delta, KeyModifiers modifiers)
    {
        if (!IsEnabled) return false;
        const float SCALE_SPEED = 0.5f;
        float currentScale = (float)Math.Clamp(1.0f + SCALE_SPEED * (float)delta.Y, 0.5, 2.0);
        // currentScale = Math.Clamp(currentScale, 0.5f, 1.5f);
        Avalonia.Vector pos1 = _canvas.CanvasPositionToWorldPosition(relativePosition);
        Avalonia.Vector pos2 = _canvas.CanvasPositionToWorldPosition(new Point(0.0, _canvas.Bounds.Height));
        Avalonia.Vector offset = pos2 - pos1;
        offset = new Avalonia.Vector(offset.X, -offset.Y) * _canvas.ViewScale;
        _canvas.TranslateView(offset.ToVector2());
        _canvas.ScaleView(currentScale);
        _canvas.TranslateView(-offset.ToVector2());
        return false;
    }

    public bool KeyReleased(Key key, KeyModifiers modifiers)
    {
        if (!IsEnabled) return false;
        if (key == Key.R)
        {
            _canvas.ResetScale();
        }
        else if (key == Key.Back || key == Key.Delete)
        {
            if (_lv.AddingObject == null)
            {
                foreach (LevelObject obj in _lv.SelectedObjects)
                {
                    obj.IsSelected = false;
                    _lv.RemoveObject(obj);
                }
            }
        }
        else if (key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right)
        {
            bool controlPressed = modifiers.IsModifierPressed(KeyModifiers.Control);
            double step = controlPressed ? 1.0 : Preferences.Singleton.LevelSettings.AlignSize;
            Avalonia.Vector unit = UnitVectorFromKey(key);
            LevelObject targetObj = _lv.AddingObject;
            if (targetObj == null)
            {
                foreach (LevelObject obj in _lv.SelectedObjects)
                {
                    obj.OnTranslate(unit * step);
                }
            }
        }
        else if (key == Key.Oem4 || key == Key.OemCloseBrackets)
        {
            bool controlPressed = modifiers.IsModifierPressed(KeyModifiers.Control);
            double step = controlPressed ? 1.0 : 3.0;
            double sign = key == Key.Oem4 ? -1.0 : 1.0;
            LevelObject targetObj = _lv.AddingObject;
            if (targetObj == null)
            {
                foreach (LevelObject obj in _lv.SelectedObjects)
                {
                    obj.OnRotate(step * sign);
                }
            }
            else
            {
                targetObj.OnRotate(step * sign);
            }
        }
        else if (key == Key.OemSemicolon || key == Key.OemQuotes)
        {
            bool controlPressed = modifiers.IsModifierPressed(KeyModifiers.Control);
            double step = controlPressed ? 0.02 : 0.2;
            double scale = key == Key.OemSemicolon ? -step : step;
            LevelObject targetObj = _lv.AddingObject;
            if (targetObj == null)
            {
                foreach (LevelObject obj in _lv.SelectedObjects)
                {
                    obj.OnScale(new Avalonia.Vector(scale, scale));
                }
            }
            else
            {
                targetObj.OnScale(new Avalonia.Vector(scale, scale));
            }
        }
        else if (key == Key.OemComma || key == Key.OemPeriod)
        {
            LevelObject targetObj = _lv.AddingObject;
            if (targetObj == null)
            {
                targetObj = _lv.SelectedObjects.FirstOrDefault();
            }
            if (targetObj != null)
            {
                if (key == Key.OemComma)
                {
                    var overlapped = _lv.RectTest(targetObj.BoundingBox, item => item != targetObj && item.Template.Layer == targetObj.Template.Layer && item.RelativeDepth <= targetObj.RelativeDepth).OrderByDescending(item => item.RelativeDepth);
                    var first = overlapped.FirstOrDefault();
                    if (first != null)
                    {
                        targetObj.RelativeDepth = Math.Max(first.RelativeDepth - 1, 0);
                    }
                }
                else if (key == Key.OemPeriod)
                {
                    var overlapped = _lv.RectTest(targetObj.BoundingBox, item => item != targetObj && item.Template.Layer == targetObj.Template.Layer && item.RelativeDepth >= targetObj.RelativeDepth).OrderBy(item => item.RelativeDepth);
                    var first = overlapped.FirstOrDefault();
                    if (first != null)
                    {
                        targetObj.RelativeDepth = Math.Min(first.RelativeDepth + 1, 999);
                    }
                }
            }
        }
        return false;
    }

    void ReadyAdd(DrawTemplate template, Point posCanvas)
    {
        WorkspaceViewModel vm = WorkspaceViewModel.Current;
        LevelObject toAdd = template.OnCreateLevelObject();
        toAdd.Position = CanvasPositionToAlignedWorldPosition(posCanvas);
        _lv.ReadyAdd(toAdd);
    }

    Avalonia.Vector CanvasPositionToWorldPosition(Point posToCanvas)
    {
        Avalonia.Vector worldPos = _canvas.CanvasPositionToWorldPosition(posToCanvas);
        int x = (int)worldPos.X;
        int y = (int)worldPos.Y;
        return new Avalonia.Vector(x, y);
    }

    Avalonia.Vector CanvasPositionToAlignedWorldPosition(Point posToCanvas)
    {
        Avalonia.Vector worldPos = CanvasPositionToWorldPosition(posToCanvas);
        int align = Preferences.Singleton.LevelSettings.AlignSize;
        double x = TurnToGridAligned((int)worldPos.X, align);
        double y = TurnToGridAligned((int)worldPos.Y, align);
        return new Avalonia.Vector(x, y);
    }

    bool ShouldResize(ISizeableObject sizeable, Avalonia.Vector mouseWorldPos, out SizeTool.SizeDirections dir)
    {
        dir = SizeTool.SizeDirections.None;
        double l = sizeable.Left;
        double r = sizeable.Right;
        double t = sizeable.Top;
        double b = sizeable.Bottom;
        const double THRESHOLD = 32.0;
        double threshold = THRESHOLD / _canvas.ViewScale;
        double dis2l = Math.Abs(mouseWorldPos.X - l);
        double dis2r = Math.Abs(mouseWorldPos.X - r);
        double dis2t = Math.Abs(mouseWorldPos.Y - t);
        double dis2b = Math.Abs(mouseWorldPos.Y - b);
        if (dis2l < dis2r && dis2l < threshold)
        {
            dir |= SizeTool.SizeDirections.Left;
        }
        else if (dis2r < threshold)
        {
            dir |= SizeTool.SizeDirections.Right;
        }
        if (dis2t < dis2b && dis2t < threshold)
        {
            dir |= SizeTool.SizeDirections.Top;
        }
        else if (dis2b < threshold)
        {
            dir |= SizeTool.SizeDirections.Bottom;
        }
        return dir != SizeTool.SizeDirections.None;
    }

    int TurnToGridAligned(int num, int gridSize)
    {
        int halfSize = gridSize / 2;
        return num % gridSize >= halfSize ? (num / gridSize * gridSize + gridSize) : (num / gridSize * gridSize);
    }

    Avalonia.Vector UnitVectorFromKey(Key key)
    {
        switch (key)
        {
            case Key.Up:
                return new Avalonia.Vector(0.0, -1.0);
            case Key.Down:
                return new Avalonia.Vector(0.0, 1.0);
            case Key.Left:
                return new Avalonia.Vector(-1.0, 0.0);
            case Key.Right:
                return new Avalonia.Vector(1.0, 0.0);
        }
        return Avalonia.Vector.Zero;
    }

    Level _lv;
    LevelCanvas _canvas;

    Vector2 _draggingAccumulation;
    SelectionScope _selection = new SelectionScope();
    Point _selectionStart;
    SizeTool _sizeTool = new SizeTool();
    DragModes _dragMode = DragModes.None;
    Avalonia.Vector? _lastMousePosition = null;
}