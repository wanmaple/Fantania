using System.Collections.Generic;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class RangeSelectionCommand : ICanvasCommand
{
    public Rectf Range { get; private set; }
    public bool KeepOld { get; private set; }

    public RangeSelectionCommand(Rectf range, bool keepOld)
    {
        Range = range;
        KeepOld = keepOld;
    }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        var bvh = context.SpaceHierarchy;
        _cacheResults.Clear();
        bvh.RectTest(Range, _cacheResults);
        context.SelectionContext.UpdateSelectedObjects(_cacheResults, KeepOld);
    }

    static List<IRenderable> _cacheResults = new List<IRenderable>(0);
}