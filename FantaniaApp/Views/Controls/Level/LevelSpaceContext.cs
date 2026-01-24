using System.Collections.Generic;
using System.Numerics;
using FantaniaLib;

namespace Fantania.Views;

public class LevelSpaceContext
{
    public BoundingVolumeHierarchy<IRenderable> SpaceHierarchy => _bvh;
    public Workspace Workspace => _canvas.Workspace!;
    public EntityRenderableManager EntityManager => _entityMgr;
    public LevelEntity? GhostEntity { get; set; }

    public LevelSpaceContext(ILevelCanvas canvas)
    {
        _canvas = canvas;
    }

    public IEnumerable<IRenderable> CollectRenderables()
    {
        Vector2 tl = _canvas.CanvasToWorld(Vector2.Zero);
        Vector2 br = _canvas.CanvasToWorld(_canvas.ControlSize);
        Rectf visibleRect = new Rectf(tl, br - tl);
        _collector.Clear();
        _bvh.RectTest(visibleRect, _collector);
        if (GhostEntity != null)
        {
        }
        return _collector;
    }

    ILevelCanvas _canvas;
    BoundingVolumeHierarchy<IRenderable> _bvh = new BoundingVolumeHierarchy<IRenderable>();
    List<IRenderable> _collector = new List<IRenderable>(256);
    List<IRenderable> _ghostRenderables = new List<IRenderable>(0);
    EntityRenderableManager _entityMgr = new EntityRenderableManager();
}