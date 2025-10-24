using System.Collections.Generic;
using Fantania.Models;

namespace Fantania;

public class DrawTemplateLayerComparer : IComparer<DrawTemplate>
{
    public static readonly DrawTemplateLayerComparer Default = new DrawTemplateLayerComparer();

    public int Compare(DrawTemplate? x, DrawTemplate? y)
    {
        return x.Layer.CompareTo(y.Layer);
    }
}