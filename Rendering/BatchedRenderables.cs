using System.Collections.Generic;
using System.Numerics;
using Avalonia.OpenGL;
using Fantania.Models;

namespace Fantania;

public class BatchedRenderables : IDisposableGL
{
    public bool Dirty { get; private set; } = false;
    public int ObjectCount => _renderables.Count;

    public BatchedRenderables(DrawTemplate template)
    {
        _template = template;
        _template.RenderLayerChanged += OnTemplateLayerChanged;
        _passes = new RenderPass[_template.RenderPassCount];
        for (int i = 0; i < _passes.Length; i++)
        {
            _passes[i] = new RenderPass(_template, i);
        }
    }

    public unsafe void Render(GlInterface gl, Matrix3x3 viewMatrix, Vector4 resolution)
    {
        foreach (var pass in _passes)
        {
            pass.Prepare(gl);
        }
        if (Dirty)
        {
            if (ObjectCount > 0)
            {
                for (int i = 0; i < _passes.Length; i++)
                {
                    _passes[i].UpdateVertices(gl);
                }
            }
            Dirty = false;
        }
        if (ObjectCount > 0)
        {
            foreach (var pass in _passes)
            {
                pass.Render(gl, viewMatrix, resolution);
            }
        }
    }

    public bool HasRenderable(IRenderable renderable)
    {
        return _renderables.Contains(renderable);
    }

    public void AddRenderable(IRenderable renderable)
    {
        foreach (var pass in _passes)
        {
            _template.OnBatchAdded(pass, renderable);
        }
        _renderables.Add(renderable);
        Dirty = true;
    }

    public void RemoveRenderable(IRenderable renderable)
    {
        _renderables.Remove(renderable);
        foreach (var pass in _passes)
        {
            _template.OnBatchRemoved(pass, renderable);
        }
        Dirty = true;
    }

    public void UpdateRenderable(IRenderable renderable)
    {
        foreach (var pass in _passes)
        {
            _template.OnBatchUpdated(pass, renderable);
        }
        Dirty = true;
    }

    public void Dispose(GlInterface gl)
    {
        foreach (var pass in _passes)
        {
            pass.Dispose(gl);
        }
        _template.RenderLayerChanged -= OnTemplateLayerChanged;
    }

    void OnTemplateLayerChanged(RenderLayers oldLayer, RenderLayers newLayer)
    {
        foreach (var pass in _passes)
        {
            pass.UpdateAllRenderables();
        }
        Dirty = true;
    }

    DrawTemplate _template;
    RenderPass[] _passes;
    HashSet<IRenderable> _renderables = new HashSet<IRenderable>(64);
}