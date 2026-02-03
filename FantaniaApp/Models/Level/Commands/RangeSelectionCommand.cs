using System.Collections.Generic;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class RangeSelectionCommand : ICanvasCommand
{
    public Rectf Range { get; private set; }
    public SelectionModes Mode { get; set; }

    public RangeSelectionCommand(Rectf range, SelectionModes mode)
    {
        Range = range;
        Mode = mode;
    }

    public void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        var bvh = context.SelectableHierarchy;
        bvh.RectTest(Range, _cache);
        context.SelectionContext.UpdateSelectedObjects(_cache, Mode);
        _cache.Clear();
    }

    static SortedSet<ISelectableItem> _cache = new SortedSet<ISelectableItem>(SelectableOrderComparer.Instance);
}