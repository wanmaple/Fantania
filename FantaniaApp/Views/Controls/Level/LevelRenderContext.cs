using System.Collections.Generic;
using System.Numerics;
using FantaniaLib;

namespace Fantania.Views;

public class LevelRenderContext
{
    public BoundingVolumeHierarchy<IRenderable> SpaceHierarchy => _bvh;
    public Workspace Workspace => _canvas.Workspace!;
    public LevelEntity? GhostEntity { get; set; }

    public LevelRenderContext(ILevelCanvas canvas)
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
        return _collector;
    }

    ILevelCanvas _canvas;
    BoundingVolumeHierarchy<IRenderable> _bvh = new BoundingVolumeHierarchy<IRenderable>();
    List<IRenderable> _collector = new List<IRenderable>(256);
}