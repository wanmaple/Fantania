using System.Collections.ObjectModel;

namespace FantaniaLib;

public class Level
{
    public const int SERIALIZATION_VERSION = 1;

    public string Name { get; private set; }

    public static async Task<Level> CreateNew(LevelCreateConfig config, string path)
    {
        var lv = new Level(config.Name);
        await Task.Run(async () =>
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(SERIALIZATION_VERSION);
                    await fs.FlushAsync();
                }
            }
        });
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

    internal void Serialize(BinaryWriter writer)
    {
        writer.Write(SERIALIZATION_VERSION);
        writer.Write(_entities.Count);
    }

    ObservableCollection<LevelEntity> _entities = new ObservableCollection<LevelEntity>();
    HashSet<string> _usedGUIDs = new HashSet<string>();
}