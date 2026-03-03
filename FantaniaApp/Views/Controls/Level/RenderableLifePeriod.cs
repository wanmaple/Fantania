using FantaniaLib;

namespace Fantania.Views;

public class RenderableLifePeriod
{
    public RenderableLifePeriod(IWorkspace workspace, IRenderContext context)
    {
        _workspace = workspace;
        _context = context;
    }

    public void Register(BoundingVolumeHierarchy<IRenderable> bvh)
    {
        bvh.ItemAdded += OnRenderableEnter;
        bvh.ItemRemoved += OnRenderableExit;
    }

    public void Unregister(BoundingVolumeHierarchy<IRenderable> bvh)
    {
        bvh.ItemAdded -= OnRenderableEnter;
        bvh.ItemRemoved -= OnRenderableExit;
    }

    void OnRenderableEnter(IRenderable renderable)
    {
        renderable.OnEnter(_workspace, _context);
    }

    void OnRenderableExit(IRenderable renderable)
    {
        renderable.OnExit(_workspace, _context);
    }

    IWorkspace _workspace;
    IRenderContext _context;
}