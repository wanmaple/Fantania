namespace FantaniaLib;

public interface IPlacement
{
    string ClassName { get; }
    string Group { get; }
    string Name { get; }
    string Tooltip { get; }
    IReadOnlyList<IPlacement> Children { get; }
    IList<IPlacement> Source { get; }
}