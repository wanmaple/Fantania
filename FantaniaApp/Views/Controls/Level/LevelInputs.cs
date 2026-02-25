using System;
using System.ComponentModel;
using System.Numerics;
using Avalonia.Input;
using FantaniaLib;

namespace Fantania.Views;

public class LevelInputs : IDisposable
{
    public LevelEditConfig EditConfig => _context.EditConfig;

    public LevelInputs(ILevelCanvas canvas, LevelEditConfig config)
    {
        _inputTracker = new ControlInputTracker(canvas.Control);
        _inputTracker.MouseEntered += OnMouseEntered;
        _inputTracker.MouseExited += OnMouseExited;
        _inputTracker.MousePressed += OnMousePressed;
        _inputTracker.MouseReleased += OnMouseReleased;
        _inputTracker.MouseMoved += OnMouseMoved;
        _inputTracker.MouseWheelChanged += OnMouseWheelChanged;
        _inputTracker.MouseCaptureLost += OnMouseCaptureLost;
        _inputTracker.KeyDown += OnKeyDown;
        _inputTracker.KeyUp += OnKeyUp;
        _context = new LevelEditorContext(canvas, config);

        var currentLv = _context.Workspace.LevelModule.CurrentLevel;
        if (currentLv != null)
        {
            foreach (var entity in currentLv.Entities)
            {
                OnEntityAdded(entity);
            }
        }
        _context.Workspace.EditorModule.PropertyChanged += OnEditorModulePropertyChanged;
        _context.Workspace.LevelModule.EntityAdded += OnEntityAdded;
        _context.Workspace.LevelModule.EntityRemoved += OnEntityRemoved;
    }

    void OnMouseEntered(object? sender, ControlInputEventArgs e)
    {
        ChangeModeDependsOnPlacementMode(e);
        _context.Focus();
        _inArea = true;
    }

    void OnMouseExited(object? sender, ControlInputEventArgs e)
    {
        _manager.SetEditorMode(LevelEditorModes.None, _context, e);
        _inArea = false;
    }

    void OnMouseCaptureLost(object? sender, ControlInputEventArgs e)
    {
        _manager.SetEditorMode(LevelEditorModes.None, _context, e);
        _inArea = false;
    }

    void OnMousePressed(object? sender, ControlInputEventArgs e)
    {
        if (!_inArea) return;
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnMousePressed(_context, e);
    }

    void OnMouseReleased(object? sender, ControlInputEventArgs e)
    {
        if (!_inArea) return;
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnMouseReleased(_context, e);
    }

    void OnMouseMoved(object? sender, ControlInputEventArgs e)
    {
        if (!_inArea) return;
        Vector2 posToCanvas = e.MouseState.Position.ToVector2();
        Vector2 posToWorld = _context.CanvasToWorld(posToCanvas);
        _context.Workspace.EditorModule.MouseWorldPosition = posToWorld.ToGridSpace(_context.EditConfig.GridAlign);
        if (e.MouseState.IsMiddleButtonPressed && !_context.FixCamera)
        {
            Vector2 movement = e.MouseState.Movement.ToVector2();
            Vector2 newPos = e.MouseState.Position.ToVector2();
            Vector2 oldPos = newPos - movement;
            Vector2 newWorldPos = _context.CanvasToWorld(newPos);
            Vector2 oldWorldPos = _context.CanvasToWorld(oldPos);
            Vector2 movementWorld = newWorldPos - oldWorldPos;
            _context.Camera.Translate(-movementWorld);
            _context.Workspace.EditorModule.Notify();
            _context.Workspace.UserTemporary.CameraPosition = _context.Camera.Position;
        }
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnMouseMoved(_context, e);
    }

    void OnMouseWheelChanged(object? sender, ControlInputEventArgs e)
    {
        if (!_inArea) return;
        if (!_context.FixCamera)
        {
            Vector2 mouseWorldPos = _context.CanvasToWorld(e.MouseState.Position.ToVector2());
            _context.Camera.ZoomAt((float)e.MouseState.WheelDelta.Y * _context.EditConfig.ZoomSensitivity, mouseWorldPos);
            _context.Workspace.EditorModule.Notify();
            _context.Workspace.UserTemporary.CameraZoom = _context.Camera.Zoom;
        }
        ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnMouseWheelChanged(_context, e);
    }

    void OnKeyDown(object? sender, ControlInputEventArgs e)
    {
        if (_inArea)
            ChangeModeDependsOnPlacementMode(e);
        _manager.CurrentMode.OnKeyDown(_context, e);
    }

    void OnKeyUp(object? sender, ControlInputEventArgs e)
    {
        if (_inArea)
        {
            if (e.KeyState.JustReleased == Key.Space && !_context.FixCamera)
            {
                Vector2 mouseWorldPos = _context.CanvasToWorld(e.MouseState.Position.ToVector2());
                _context.Camera.SetZoomAt(1.0f, mouseWorldPos);
                _context.Workspace.EditorModule.Notify();
                _context.Workspace.UserTemporary.CameraZoom = _context.Camera.Zoom;
            }
            ChangeModeDependsOnPlacementMode(e);
        }
        _manager.CurrentMode.OnKeyUp(_context, e);
    }

    void ChangeModeDependsOnPlacementMode(ControlInputEventArgs e)
    {
        switch (_context.Workspace.EditorModule.CurrentPlacementMode)
        {
            case EntityPlacementModes.Select:
                _manager.SetEditorMode(LevelEditorModes.View, _context, e);
                break;
            case EntityPlacementModes.Place:
                _manager.SetEditorMode(LevelEditorModes.Place, _context, e);
                break;
        }
    }

    void OnEditorModulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorModule.CurrentPlacementMode))
        {
            if (_inArea)
                ChangeModeDependsOnPlacementMode(_inputTracker.CreateInputEventArgs());
            if (_context.Workspace.EditorModule.CurrentPlacementMode != EntityPlacementModes.Select)
                _context.Workspace.EditorModule.CancelSelection();
        }
    }

    void OnEntityAdded(LevelEntity entity)
    {
        if (entity is IMultiNodeContainer container)
        {
            container.NodeRemoved += OnEntityNodeRemoved;
        }
        if (entity is TiledEntity tiled)
        {
            tiled.PropertyChanged += OnTiledEntityPropertyChanged;
        }
    }

    void OnEntityRemoved(LevelEntity entity)
    {
        if (entity is IMultiNodeContainer container)
        {
            container.NodeRemoved -= OnEntityNodeRemoved;
            foreach (var node in container.AllNodes)
            {
                _context.Workspace.EditorModule.SelectedObjects.RemoveFast(node);
            }
        }
        else if (entity is ISelectableItem item)
            _context.Workspace.EditorModule.SelectedObjects.RemoveFast(item);
        if (entity is TiledEntity tiled)
        {
            tiled.PropertyChanged -= OnTiledEntityPropertyChanged;
        }
        _context.Workspace.EditorModule.Notify();
    }

    void OnEntityNodeRemoved(LevelEntityNode node)
    {
        _context.Workspace.EditorModule.SelectedObjects.RemoveFast(node);
        _context.Workspace.EditorModule.Notify();
    }

    void OnTiledEntityPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TiledEntity.Position) || e.PropertyName == nameof(TiledEntity.Size))
        {
            TiledEntity tiled = (TiledEntity)sender!;
            IReadonlyLevel lv = _context.Workspace.LevelModule.CurrentLevel!;
            var group = lv.TiledEntityManager.GetGroup(tiled);
            foreach (var entity in group.Entities)
                entity.RefreshSelf();
            lv.TiledEntityManager.RemoveEntity(_context.Workspace, tiled);
            lv.TiledEntityManager.AddEntity(_context.Workspace, tiled);
            group = lv.TiledEntityManager.GetGroup(tiled);
            foreach (var entity in group.Entities)
                entity.RefreshSelf();
        }
    }

    public void Dispose()
    {
        _inputTracker.Dispose();
    }

    ControlInputTracker _inputTracker;
    LevelEditorContext _context;
    LevelEditorModeManager _manager = new LevelEditorModeManager();
    bool _inArea = false;
}