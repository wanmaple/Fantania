namespace FantaniaLib;

public interface ISizeableEntity
{
    Vector2Int Position { get; set; }
    Vector2Int Size { get; set; }

    Vector2Int GetUnitSize(IWorkspace workspace);
}