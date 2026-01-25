using System.Collections.ObjectModel;

namespace FantaniaLib;

public class Level : IReadonlyLevel
{
    public const int SERIALIZATION_VERSION = 1;

    public string Name { get; private set; }
    public IReadOnlyList<LevelEntity> Entities => _entities;

    internal IList<LevelEntity> WritableEntities => _entities;

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

    public string NewGUID()
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

    ObservableCollection<LevelEntity> _entities = new ObservableCollection<LevelEntity>();
    HashSet<string> _usedGUIDs = new HashSet<string>();
}