using System.Collections.Generic;
using System.Numerics;
using FantaniaLib;

namespace Fantania.Views;

public class LevelSpaceContext
{
    public BoundingVolumeHierarchy<IRenderable> RenderableHierarchy => _bvhRenderables;
    public BoundingVolumeHierarchy<ISelectableItem> SelectableHierarchy => _bvhSelectables;
    public Workspace Workspace => _canvas.Workspace!;
    public EntityRenderableManager EntityManager => _entityMgr;
    public ResizeContext ResizeContext { get; private set; }

    public LevelEntity? GhostEntity { get; set; }
    public SelectionContext SelectionContext { get; private set; }

    public LevelSpaceContext(ILevelCanvas canvas)
    {
        _canvas = canvas;
        SelectionContext = new SelectionContext(Workspace);
        ResizeContext = new ResizeContext();
    }

    public IEnumerable<IRenderable> CollectRenderables()
    {
        Vector2 tl = _canvas.CanvasPositionToWorldPosition(Vector2.Zero);
        Vector2 br = _canvas.CanvasPositionToWorldPosition(_canvas.ControlSize);
        Rectf visibleRect = new Rectf(tl, br - tl);
        _collector.Clear();
        _bvhRenderables.RectTest(visibleRect, _collector);
        return _collector;
    }

    ILevelCanvas _canvas;
    BoundingVolumeHierarchy<IRenderable> _bvhRenderables = new BoundingVolumeHierarchy<IRenderable>();
    BoundingVolumeHierarchy<ISelectableItem> _bvhSelectables = new BoundingVolumeHierarchy<ISelectableItem>();
    List<IRenderable> _collector = new List<IRenderable>(256);
    EntityRenderableManager _entityMgr = new EntityRenderableManager();
}