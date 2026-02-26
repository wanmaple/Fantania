using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class ClickSelectionCommand : ICanvasCommand
{
    public Vector2 Point { get; private set; }
    public SelectionModes Mode { get; set; }

    public ClickSelectionCommand(Vector2 pt, SelectionModes mode)
    {
        Point = pt;
        Mode = mode;
    }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        var bvh = context.SelectableHierarchy;
        bvh.PointTest(Point, _cache, r => context.Workspace.LevelModule.LayerManager.IsLayerVisible(MathHelper.FloorToInt((float)r.Depth / LevelEntity.LAYER_RANGE)));
        if (Mode == SelectionModes.Add)
        {
            // 复数选中的话，就默认多选取第一个当前未被选中的对象
            var target = _cache.FirstOrDefault(r => !context.Workspace.EditorModule.SelectedObjects.Contains(r));
            if (target != null)
            {
                _cache.Clear();
                _cache.Add(target);
            }
        }
        else if (Mode == SelectionModes.Remove)
        {
            // 取消选中第一个已经选中的对象
            var target = _cache.FirstOrDefault(r => context.Workspace.EditorModule.SelectedObjects.Contains(r));
            if (target != null)
            {
                _cache.Clear();
                _cache.Add(target);
            }
        }
        else if (_cache.Count > 1)
        {
            var curSelections = context.Workspace.EditorModule.SelectedObjects;
            if (curSelections.Count(o => _cache.Contains(o)) > 1 || curSelections.All(o => !_cache.Contains(o)))
            {
                // 多个包含或者全部不包含，那么就默认选中第一个。
                var first = _cache.First();
                _cache.Clear();
                _cache.Add(first);
            }
            else 
            {
                // 只有单个包含，就选取之后的那个
                var sel = curSelections[0];
                var list = _cache.ToList();
                int index = list.IndexOf(sel);
                index = (index + 1) % list.Count;
                var next = list[index];
                _cache.Clear();
                _cache.Add(next);
            }
        }
        context.SelectionContext.UpdateSelectedObjects(_cache, Mode);
        _cache.Clear();
    }

    static SortedSet<ISelectableItem> _cache = new SortedSet<ISelectableItem>(SelectableOrderComparer.Instance);
}