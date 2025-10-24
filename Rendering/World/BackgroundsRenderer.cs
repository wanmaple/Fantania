using System;
using System.Collections.Generic;
using Avalonia.OpenGL;
using Fantania.Models;

namespace Fantania;

public class BackgroundsRenderer : IRenderer
{
    public event Action PriorityChanged;

    public int Priority { get; set; } = -1000;
    public bool IsEnabled { get; set; } = true;

    public BackgroundsRenderer(Stylegrounds sgs)
    {
        _stylegrounds = sgs;
    }

    public void Finalize(GlInterface gl)
    {
        _stylegrounds.BackgroundAdded -= OnBackgroundAdded;
        _stylegrounds.BackgroundRemoved -= OnBackgroundRemoved;
        if (_toDispose.Count > 0)
        {
            foreach (var renderer in _toDispose)
            {
                renderer.Finalize(gl);
            }
            _toDispose.Clear();
        }
        foreach (var pair in _sgRenderers)
        {
            pair.Value.Finalize(gl);
        }
        _sgRenderers.Clear();
    }

    public void Initialize(GlInterface gl)
    {
        foreach (var sg in _stylegrounds.Backgrounds)
        {
            OnBackgroundAdded(_stylegrounds, sg, null);
        }
        _stylegrounds.BackgroundAdded += OnBackgroundAdded;
        _stylegrounds.BackgroundRemoved += OnBackgroundRemoved;
    }

    public void Render(GlInterface gl)
    {
        if (_toDispose.Count > 0)
        {
            foreach (var renderer in _toDispose)
            {
                renderer.Finalize(gl);
            }
            _toDispose.Clear();
        }
        if (_toInit.Count > 0)
        {
            foreach (var renderer in _toInit)
            {
                renderer.Initialize(gl);
            }
            _toInit.Clear();
        }
        foreach (var sg in _stylegrounds.Backgrounds)
        {
            if (!sg.Visible) continue;
            IRenderer renderer = _sgRenderers[sg];
            sg.Template.UpdateRenderer(renderer);
            renderer.Render(gl);
        }
    }

    void OnBackgroundAdded(Stylegrounds sgs, Styleground sg, Styleground prev)
    {
        IRenderer renderer = sg.Template.CreateRenderer();
        _sgRenderers.Add(sg, renderer);
        _toInit.Add(renderer);
    }

    void OnBackgroundRemoved(Stylegrounds sgs, Styleground sg, Styleground prev)
    {
        IRenderer renderer = _sgRenderers[sg];
        _sgRenderers.Remove(sg);
        _toDispose.Add(renderer);
    }

    Stylegrounds _stylegrounds;
    Dictionary<Styleground, IRenderer> _sgRenderers = new Dictionary<Styleground, IRenderer>(16);
    HashSet<IRenderer> _toInit = new HashSet<IRenderer>(16);
    HashSet<IRenderer> _toDispose = new HashSet<IRenderer>(16);
}
