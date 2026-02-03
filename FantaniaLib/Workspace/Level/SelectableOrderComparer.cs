namespace FantaniaLib;

public class SelectableOrderComparer : IComparer<ISelectableItem>
{
    public static readonly IComparer<ISelectableItem> Instance = new SelectableOrderComparer();

    public int Compare(ISelectableItem? x, ISelectableItem? y)
    {
        ISelectableItem lhs = x!, rhs = y!;
        if (lhs.Depth == rhs.Depth)
        {
            if (lhs.EntityOrder == rhs.EntityOrder)
            {
                return lhs.LocalOrder.CompareTo(rhs.LocalOrder);
            }
            return lhs.EntityOrder.CompareTo(rhs.EntityOrder);
        }
        return x!.Depth.CompareTo(y!.Depth);
    }
}