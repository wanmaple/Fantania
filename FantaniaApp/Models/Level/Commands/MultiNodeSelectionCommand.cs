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
        bvh.PointTest(Point, _cache, s => s is LevelEntityNode);
        // 这里和普通选中逻辑相同
        var target = _cache.FirstOrDefault(r => !context.Workspace.EditorModule.SelectedObjects.Contains(r));
        if (target == null)
        {
            target = _cache.FirstOrDefault();
        }
        if (target != null && target is LevelEntityNode node && node.Owner.AllNodes.Count > 1)
        {
            _cache.Clear();
            foreach (var n in node.Owner.AllNodes)
            {
                _cache.Add(n);
            }
        }
        context.SelectionContext.UpdateSelectedObjects(_cache, SelectionModes.Replace);
        _cache.Clear();
    }

    static SortedSet<ISelectableItem> _cache = new SortedSet<ISelectableItem>(SelectableOrderComparer.Instance);
}