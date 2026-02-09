namespace FantaniaLib;

public interface IReadonlyLevel
{
    string Name { get; }
    IReadOnlyList<LevelEntity> Entities { get; }
    TiledEntityManager TiledEntityManager { get; }

    string ObtainGUID();
    void ReleaseGUID(string guid);
    int NewOrder();
}