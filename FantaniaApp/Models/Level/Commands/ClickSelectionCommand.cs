using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class ClickSelectionCommand : ICanvasCommand
{
    public Vector2 Point { get; private set; }
    public bool KeepOld { get; private set; }

    public ClickSelectionCommand(Vector2 pt, bool keepOld)
    {
        Point = pt;
        KeepOld = keepOld;
    }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        var bvh = context.SpaceHierarchy;
        _cacheResults.Clear();
        bvh.PointTest(Point, _cacheResults);
        _cacheResults.StableSort(RenderableDepthComparer.Instance);
        if (KeepOld)
        {
            // 复数选中的话，就默认多选取第一个当前未被选中的对象
            var target = _cacheResults.FirstOrDefault(r => !context.Workspace.EditorModule.SelectedObjects.Contains(r));
            if (target != null)
            {
                _cacheResults.Clear();
                _cacheResults.Add(target);
            }
        }
        else if (_cacheResults.Count > 1)
        {
            var curSelections = context.Workspace.EditorModule.SelectedObjects;
            if (curSelections.Count(o => _cacheResults.Contains(o)) > 1 || curSelections.All(o => !_cacheResults.Contains(o)))
            {
                // 多个包含或者全部不包含，那么就默认选中第一个。
                var first = _cacheResults[0];
                _cacheResults.Clear();
                _cacheResults.Add(first);
            }
            else 
            {
                // 只有单个包含，就选取之后的那个
                var sel = (IRenderable)curSelections[0];
                int index = _cacheResults.IndexOf(sel);
                index = (index + 1) % _cacheResults.Count;
                var next = _cacheResults[index];
                _cacheResults.Clear();
                _cacheResults.Add(next);
            }
        }
        context.SelectionContext.UpdateSelectedObjects(_cacheResults, KeepOld);
    }

    static List<IRenderable> _cacheResults = new List<IRenderable>(0);
}