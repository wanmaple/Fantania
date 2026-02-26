namespace FantaniaLib;

public class RenderableDepthComparer : IComparer<IRenderable>
{
    public static readonly RenderableDepthComparer Instance = new RenderableDepthComparer();

    private RenderableDepthComparer()
    {}

    public int Compare(IRenderable? x, IRenderable? y)
    {
        IRenderable lhs = x!, rhs = y!;
        if (lhs.Depth == rhs.Depth)
        {
            if (lhs.EntityOrder == rhs.EntityOrder)
            {
                return lhs.LocalOrder.CompareTo(rhs.LocalOrder);
            }
            return lhs.EntityOrder.CompareTo(rhs.EntityOrder);
        }
        return x!.Depth.CompareTo(y!.Depth);
    }
}

public class RenderableInverseDepthComparer : IComparer<IRenderable>
{
    public static readonly RenderableInverseDepthComparer Instance = new RenderableInverseDepthComparer();

    private RenderableInverseDepthComparer()
    {}

    public int Compare(IRenderable? x, IRenderable? y)
    {
        IRenderable lhs = x!, rhs = y!;
        if (lhs.Depth == rhs.Depth)
        {
            if (lhs.EntityOrder == rhs.EntityOrder)
            {
                return lhs.LocalOrder.CompareTo(rhs.LocalOrder);
            }
            return lhs.EntityOrder.CompareTo(rhs.EntityOrder);
        }
        return y!.Depth.CompareTo(x!.Depth);
    }
}