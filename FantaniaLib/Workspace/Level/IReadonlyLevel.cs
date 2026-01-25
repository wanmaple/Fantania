namespace FantaniaLib;

public interface IReadonlyLevel
{
    string Name { get; }
    IReadOnlyList<LevelEntity> Entities { get; }
}