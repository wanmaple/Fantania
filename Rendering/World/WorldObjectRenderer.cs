using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia;
using Avalonia.OpenGL;
using Fantania.Models;

namespace Fantania;

public class WorldObjectRenderer : IRenderer
{
    public event Action PriorityChanged;

    public int Priority { get; set; } = 0;
    public bool IsEnabled { get; set; } = true;

    public WorldObjectRenderer(WorldCanvas canvas)
    {
        _canvas = canvas;
    }

    public void Render(GlInterface gl)
    {
        Rect viewRect = _canvas.GetViewRect();
        HandleAddRemoveObjects(gl, viewRect);
        Vector4 resolution = new Vector4(_canvas.ColorBufferWidth, _canvas.ColorBufferHeight, 1.0f / _canvas.ColorBufferWidth, 1.0f / _canvas.ColorBufferHeight);
        foreach (var pair in _batched)
        {
            DrawTemplate template = pair.Key;
            if (template.Transparent)
            {
                _transparentTemplates.Add(template);
                continue;
            }
            foreach (var batch in pair.Value)
            {
                batch.Render(gl, _canvas.ViewMatrix, resolution);
            }
        }
        foreach (var template in _transparentTemplates)
        {
            foreach (var batch in _batched[template])
            {
                batch.Render(gl, _canvas.ViewMatrix, resolution);
            }
        }
        _transparentTemplates.Clear();
    }

    public void Initialize(GlInterface gl)
    {
        _canvas.World.ObjectAdded += OnWorldObjectAdded;
        _canvas.World.ObjectRemoved += OnWorldObjectRemoved;
        var viewRect = _canvas.GetViewRect();
        foreach (var obj in _canvas.World.AllObjects)
        {
            obj.OnEnterCanvas(gl);
            if (viewRect.Intersects(obj.BoundingBox))
            {
                _visibleObjects.Add(obj);
                ShowWorldObject(obj);
                obj.RenderInfoChanged += OnObjectRenderInfoChanged;
                obj.VisibilityChanged += OnObjectVisibilityChanged;
            }
        }
    }

    public void Finalize(GlInterface gl)
    {
        _canvas.World.ObjectAdded -= OnWorldObjectAdded;
        _canvas.World.ObjectRemoved -= OnWorldObjectRemoved;
        foreach (var obj in _canvas.World.AllObjects)
        {
            obj.RenderInfoChanged -= OnObjectRenderInfoChanged;
            obj.VisibilityChanged -= OnObjectVisibilityChanged;
        }
        foreach (var pair in _batched)
        {
            foreach (var batch in pair.Value)
            {
                batch.Dispose(gl);
            }
        }
        _batched.Clear();
    }

    public void OnViewChanged(Rect viewRect)
    {
        var visibleObjects = _canvas.World.RectTest(viewRect, obj => obj.IsVisible);
        foreach (var curVisible in visibleObjects)
        {
            if (!_visibleObjects.Contains(curVisible))
                OnWorldObjectAdded(curVisible);
        }
        foreach (var oldVisible in _visibleObjects)
        {
            if (oldVisible == _canvas.World.AddingObject) continue;
            if (!oldVisible.InSpacePartition) continue;
            if (!visibleObjects.Contains(oldVisible))
                OnWorldObjectRemoved(oldVisible);
        }
    }

    void OnWorldObjectAdded(WorldObject obj)
    {
        if (!_removing.Remove(obj))
            _adding.Add(obj);
    }

    void OnWorldObjectRemoved(WorldObject obj)
    {
        if (!_adding.Remove(obj))
            _removing.Add(obj);
    }

    void OnObjectRenderInfoChanged(WorldObject obj)
    {
        UpdateRenderable(obj);
    }

    void OnObjectVisibilityChanged(WorldObject obj)
    {
        if (obj.IsVisible)
        {
            Rect viewRect = _canvas.GetViewRect();
            if (viewRect.Intersects(obj.BoundingBox))
            {
                _visibleObjects.Add(obj);
                ShowWorldObject(obj);
            }
        }
        else
        {
            if (_visibleObjects.Remove(obj))
                HideWorldObject(obj);
        }
    }

    void UpdateRenderable(WorldObject obj)
    {
        var batches = _batched[obj.Template];
        foreach (var batch in batches)
        {
            if (batch.HasRenderable(obj))
            {
                batch.UpdateRenderable(obj);
                break;
            }
        }
    }

    void HandleAddRemoveObjects(GlInterface gl, Rect viewRect)
    {
        foreach (WorldObject obj in _removing)
        {
            if (!_visibleObjects.Remove(obj)) continue;
            if (HideWorldObject(obj))
            {
                obj.RenderInfoChanged -= OnObjectRenderInfoChanged;
                obj.VisibilityChanged -= OnObjectVisibilityChanged;
            }
        }
        _removing.Clear();
        foreach (WorldObject obj in _adding)
        {
            obj.OnEnterCanvas(gl);
            if (!obj.IsVisible) continue;
            // 视野裁剪
            if (viewRect.Intersects(obj.BoundingBox) || !obj.InSpacePartition)
            {
                _visibleObjects.Add(obj);
                ShowWorldObject(obj);
                obj.RenderInfoChanged += OnObjectRenderInfoChanged;
                obj.VisibilityChanged += OnObjectVisibilityChanged;
            }
        }
        _adding.Clear();
    }

    void ShowWorldObject(WorldObject obj)
    {
        if (!_batched.TryGetValue(obj.Template, out var batches))
        {
            batches = new List<BatchedRenderables>(1);
            var batch = new BatchedRenderables(obj.Template);
            batch.AddRenderable(obj);
            batches.Add(batch);
            _batched.Add(obj.Template, batches);
        }
        else
        {
            // find available batch to use
            bool found = false;
            foreach (var batch in batches)
            {
                if (batch.ObjectCount >= 512)
                    continue;
                batch.AddRenderable(obj);
                found = true;
                break;
            }
            if (!found)
            {
                var batch = new BatchedRenderables(obj.Template);
                batches.Add(batch);
                batch.AddRenderable(obj);
            }
        }
    }

    bool HideWorldObject(WorldObject obj)
    {
        var batches = _batched[obj.Template];
        foreach (var batch in batches)
        {
            if (batch.HasRenderable(obj))
            {
                batch.RemoveRenderable(obj);
                return true;
            }
        }
        return false;
    }

    HashSet<WorldObject> _visibleObjects = new HashSet<WorldObject>(1024);
    Dictionary<DrawTemplate, List<BatchedRenderables>> _batched = new Dictionary<DrawTemplate, List<BatchedRenderables>>(16);

    HashSet<WorldObject> _adding = new HashSet<WorldObject>(10);
    HashSet<WorldObject> _removing = new HashSet<WorldObject>(10);
    SortedSet<DrawTemplate> _transparentTemplates = new SortedSet<DrawTemplate>(DrawTemplateLayerComparer.Default);

    WorldCanvas _canvas;
}
