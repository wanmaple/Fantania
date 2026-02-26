using System.Collections.ObjectModel;

namespace FantaniaLib;

public class Level : IReadonlyLevel
{
    public const int SERIALIZATION_VERSION = 1;

    public string Name { get; private set; }
    public IReadOnlyList<LevelEntity> Entities => _entities;
    public TiledEntityManager TiledEntityManager => _tileMgr;

    internal IList<LevelEntity> MutableEntities => _entities;

    public static Level CreateNew(LevelCreateConfig config)
    {
        var lv = new Level(config.Name);
        return lv;
    }

    public static Level OpenExist(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        var lv = new Level(name);
        return lv;
    }

    internal Level(string name)
    {
        Name = name;
    }

    public void OnLevelLoaded(IWorkspace workspace)
    {
        RefreshGUIDs();
        foreach (var entity in _entities)
        {
            entity.OnLoaded(workspace, this);
        }
    }

    public string ObtainGUID()
    {
        string guid = string.Empty;
        do
        {
            guid = Guid.NewGuid().ToString();
            if (_usedGUIDs.Add(guid))
                break;
        } while (true);
        return guid;
    }

    public void ReleaseGUID(string guid)
    {
        _usedGUIDs.Remove(guid);
    }

    public int NewOrder()
    {
        return _entities.Count > 0 ? _entities.Max(e => e.Order) + 1 : 0;
    }

    void RefreshGUIDs()
    {
        _usedGUIDs.Clear();
        foreach (var entity in _entities)
        {
            _usedGUIDs.Add(entity.GUID);
        }
    }

    ObservableCollection<LevelEntity> _entities = new ObservableCollection<LevelEntity>();
    HashSet<string> _usedGUIDs = new HashSet<string>();
    TiledEntityManager _tileMgr = new TiledEntityManager();
}