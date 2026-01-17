using System.Collections.ObjectModel;

namespace FantaniaLib;

public class LevelModule : WorkspaceModule
{
    private Level? _curLv = null;
    public Level? CurrentLevel
    {
        get { return _curLv; }
        private set
        {
            if (_curLv != value)
            {
                _curLv = value;
                _workspace.Solution.LatestEditingLevel = _curLv != null ? _curLv.Name : string.Empty;
                OnPropertyChanged(nameof(CurrentLevel));
            }
        }
    }

    public IReadOnlyList<LevelDescription> LevelDescriptions => _lvDescs;

    public LevelModule(IWorkspace workspace) : base(workspace)
    {
        LoadExistingLevels();
    }

    public async Task CreateLevel(LevelCreateConfig config)
    {
        string lvFolder = _workspace.GetAbsolutePath(Workspace.LEVELS_FOLDER);
        if (!Directory.Exists(lvFolder))
            Directory.CreateDirectory(lvFolder);
        string lvPath = _workspace.GetAbsolutePath(Workspace.LEVELS_FOLDER, config.Name + ".lv");
        Level lv = await Level.CreateNew(config, lvPath);
        _lvDescs.Add(new LevelDescription
        {
            Name = lv.Name,
        });
        CurrentLevel = lv;
    }

    public void LoadLevel(string lvName)
    {
        if (!_lvDescs.Any(lv => lv.Name == lvName)) return;
        string lvPath = _workspace.GetAbsolutePath(Workspace.LEVELS_FOLDER, lvName + ".lv");
        var lv = Level.OpenExist(lvPath);
        CurrentLevel = lv;
    }

    void LoadExistingLevels()
    {
        string lvFolder = _workspace.GetAbsolutePath(Workspace.LEVELS_FOLDER);
        if (Directory.Exists(lvFolder))
        {
            var di = new DirectoryInfo(lvFolder);
            foreach (var fi in di.GetFiles("*.lv", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileNameWithoutExtension(fi.Name);
                _lvDescs.Add(new LevelDescription
                {
                    Name = name,
                });
            }
        }
    }

    ObservableCollection<LevelDescription> _lvDescs = new ObservableCollection<LevelDescription>();
}