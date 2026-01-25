namespace FantaniaLib;

public class RenderableDepthComparer : IComparer<IRenderable>
{
    public static readonly RenderableDepthComparer Instance = new RenderableDepthComparer();

    private RenderableDepthComparer()
    {}

    public int Compare(IRenderable? x, IRenderable? y)
    {
        return x!.Depth.CompareTo(y!.Depth);
    }
}