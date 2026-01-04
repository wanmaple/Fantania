using System.Collections.ObjectModel;

namespace FantaniaLib;

public class Workspace : SyncableObject
{
    public const string GENERATED_FOLDER = ".fantania";
    public const string TEXTURE_FOLDER = "textures";
    public const string SCRIPTS_FOLDER = "scripts";

    public string RootFolder { get; set; }

    public IReadOnlyList<IPlacement> LevelPlacements => _levelPlacements;

    public Workspace(string rootFolder)
    {
        rootFolder = AvaloniaHelper.ConvertAvaloniaUriToStandardUri(rootFolder);
        if (!Directory.Exists(rootFolder))
            throw new WorkspaceException("Invalid workspace folder");
        RootFolder = rootFolder.ToStandardPath();
        Validate(RootFolder);
    }

    public string GetAbsolutePath(params string[] pathes)
    {
        pathes = new string[] { RootFolder, }.Concat(pathes).ToArray();
        return Path.Combine(pathes).ToStandardPath();
    }

    void Validate(string rootFolder)
    {
        try
        {
            foreach (string scriptPath in AvaloniaHelper.EnumerateAssetFolder("avares://Fantania/Assets/scripts/templates"))
            {
                if (scriptPath.EndsWith(".lua"))
                {
                    string script = AvaloniaHelper.ReadAssetText(scriptPath);
                    var template = new ScriptTemplate(_scriptEngine, _scriptEngine.ExecuteString(script));
                    AddLevelTemplate(template);
                }
            }
        }
        finally
        {
            _valid = true;
        }
    }

    void AddLevelTemplate(ScriptTemplate template)
    {
        string group = template.Group;
        IPlacement placementGroup = _levelPlacements.FirstOrDefault(p => p.Name == group);
        if (placementGroup == null)
        {
            placementGroup = new PlacementGroup(group);
            _levelPlacements.Add(placementGroup);
        }
        placementGroup.Children.Add(template);
        _levelTemplateMap.Add(template.ClassName, template);
    }

    bool _valid = false;
    ScriptEngine _scriptEngine = new ScriptEngine();
    Dictionary<string, ScriptTemplate> _levelTemplateMap = new Dictionary<string, ScriptTemplate>(128);
    ObservableCollection<IPlacement> _levelPlacements = new ObservableCollection<IPlacement>();
}