using System;
using System.Collections.Generic;
using Avalonia.OpenGL;

namespace Fantania;

public class RendererComparer : IComparer<IRenderer>
{
    public static readonly RendererComparer Default = new RendererComparer();

    public int Compare(IRenderer? x, IRenderer? y)
    {
        return x.Priority.CompareTo(y.Priority);
    }
}

public interface IRenderer
{
    event Action PriorityChanged;

    int Priority { get; set; }
    bool IsEnabled { get; set; }

    void Initialize(GlInterface gl);
    void Render(GlInterface gl);
    void Finalize(GlInterface gl);
}