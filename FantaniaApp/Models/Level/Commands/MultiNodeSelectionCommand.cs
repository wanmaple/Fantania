using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class MultiNodeSelectionCommand : ICanvasCommand
{
    public Vector2 Point { get; set; }

    public MultiNodeSelectionCommand(Vector2 pt)
    {
        Point = pt;
    }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        var bvh = context.SelectableHierarchy;
        bvh.PointTest(Point, _cache, s => s is LevelEntityNode || s is TiledEntity);
        // 这里和普通选中逻辑相同
        var target = _cache.FirstOrDefault(r => !context.Workspace.EditorModule.SelectedObjects.Contains(r));
        if (target == null)
        {
            target = _cache.FirstOrDefault();
        }
        if (target != null)
        {
            _cache.Clear();
            if (target is LevelEntityNode node && node.Owner.AllNodes.Count > 1)
            {
                foreach (var n in node.Owner.AllNodes)
                {
                    _cache.Add(n);
                }
            }
            else if (target is TiledEntity tiled)
            {
                var group = context.Workspace.LevelModule.CurrentLevel!.TiledEntityManager.GetGroup(tiled);
                foreach (var e in group.Entities)
                {
                    _cache.Add(e);
                }
            }
        }
        context.SelectionContext.UpdateSelectedObjects(_cache, SelectionModes.Replace);
        _cache.Clear();
    }

    static SortedSet<ISelectableItem> _cache = new SortedSet<ISelectableItem>(SelectableOrderComparer.Instance);
}