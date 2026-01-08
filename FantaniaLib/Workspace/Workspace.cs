using System.Text.Json;

namespace FantaniaLib;

public class Workspace : SyncableObject
{
    public const string GENERATED_FOLDER = ".fantania";
    public const string TEXTURE_FOLDER = "textures";
    public const string SCRIPTS_FOLDER = "scripts";

    public string RootFolder { get; set; }
    public bool IsValid => _valid;

    public ulong FrameCount => _frame;

    public LogModule LogModule => _logModule;
    public PlacementModule PlacementModule => _placeModule;

    public Workspace(string rootFolder)
    {
        rootFolder = AvaloniaHelper.ConvertAvaloniaUriToStandardUri(rootFolder);
        if (!Directory.Exists(rootFolder))
            throw new WorkspaceException("Invalid workspace folder");
        RootFolder = rootFolder.ToStandardPath();
        Validate();
    }

    public string GetAbsolutePath(params string[] pathes)
    {
        pathes = new string[] { RootFolder, }.Concat(pathes).ToArray();
        return Path.Combine(pathes).ToStandardPath();
    }

    public async Task CreateNew()
    {
        string slnFilePath = GetAbsolutePath(SOLUTION_FILENAME);
        _solution = new WorkspaceSolution();
        using (var fs = new FileStream(slnFilePath, FileMode.Create, FileAccess.Write))
        {
            JsonSerializer.Serialize(fs, _solution, new JsonSerializerOptions { WriteIndented = true, });
            await fs.FlushAsync();
        }
        string dbFilePath = GetAbsolutePath(DATABASE_FILENAME);
        if (File.Exists(dbFilePath))
            File.Delete(dbFilePath);

        InitializeRequired();
    }

    public void Tick(TimeSpan dt)
    {
        ++_frame;
    }

    async Task Validate()
    {
        _valid = true;
        string slnFilePath = GetAbsolutePath(SOLUTION_FILENAME);
        if (!File.Exists(slnFilePath))
        {
            _valid = false;
            return;
        }
        try
        {
            string slnJson = File.ReadAllText(slnFilePath);
            _solution = JsonSerializer.Deserialize<WorkspaceSolution>(slnJson);
            if (_solution == null)
            {
                _valid = false;
                return;
            }
        }
        catch (Exception)
        {
            _valid = false;
            return;
        }
        string dbFilePath = GetAbsolutePath(DATABASE_FILENAME);
        if (!File.Exists(dbFilePath))
        {
            _valid = false;
            return;
        }
        try
        {
            using (var conn = await SqliteHelper.OpenDatabase(dbFilePath))
            {
                await conn.CloseAsync();
            }
        }
        catch (Exception)
        {
            _valid = false;
            return;
        }
    }

    void InitializeRequired()
    {
        // Load built-in scripts
        foreach (string scriptPath in AvaloniaHelper.EnumerateAssetFolder("avares://Fantania/Assets/scripts/templates"))
        {
            if (scriptPath.EndsWith(".lua"))
            {
                string script = AvaloniaHelper.ReadAssetText(scriptPath);
                var template = new ScriptTemplate(_scriptEngine, _scriptEngine.ExecuteString(script));
                _placeModule.AddLevelTemplate(template);
            }
        }
    }

    const string SOLUTION_FILENAME = "workspace.json";
    const string DATABASE_FILENAME = "workspace.db";

    bool _valid = false;
    WorkspaceSolution? _solution;
    ScriptEngine _scriptEngine = new ScriptEngine();
    PlacementModule _placeModule = new PlacementModule();
    LogModule _logModule = new LogModule();
    ulong _frame = 0u;
}