using System.Text.Json;

namespace FantaniaLib;

public class Workspace : SyncableObject, IWorkspace
{
    public class TickData
    {
        public ulong Frame = 0ul;
        public float Time = 0.0f;

        public int FPS => Math.Min(60, MathHelper.RoundToInt(_dtQueue.Average()));

        public void Enqueue(float fps)
        {
            _dtQueue.Enqueue(fps);
            if (_dtQueue.Count > 60)
            {
                _dtQueue.Dequeue();
            }
        }

        private Queue<float> _dtQueue = new Queue<float>();
    }

    public const string GENERATED_FOLDER = ".fantania";
    public const string TEXTURE_FOLDER = "textures";
    public const string SCRIPTS_FOLDER = "scripts";
    public const string ENTITIES_FOLDER = "entities";

    public string RootFolder { get; private set; }
    public bool IsModified { get; private set; }
    public UndoStack UndoStack => _undoStack;
    public ulong FrameCount => _tickData.Frame;
    public float Time => _tickData.Time;

    public bool IsValid => _valid;

    public DatabaseModule DatabaseModule => _dbModule;
    public PlacementModule PlacementModule => _placeModule;
    public EditorModule EditorModule => _editorModule;
    public LogModule LogModule => _logModule;
    public ScriptingModule ScriptingModule => _scriptModule;

    public Workspace(string rootFolder)
    {
        if (!Directory.Exists(rootFolder))
            throw new WorkspaceException("Invalid workspace folder");
        RootFolder = rootFolder.ToStandardPath();
        _dbModule = new DatabaseModule(this);
        _placeModule = new PlacementModule(this);
        _editorModule = new EditorModule(this);
        _logModule = new LogModule(this);
        _scriptModule = new ScriptingModule(this);
        Validate().GetAwaiter().GetResult();
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
        string dbFilePath = GetAbsolutePath(DatabaseModule.DATABASE_FILENAME);
        if (File.Exists(dbFilePath))
            File.Delete(dbFilePath);
        InitializeRequired();
        string texFolder = GetAbsolutePath(TEXTURE_FOLDER);
        string scriptFolder = GetAbsolutePath(SCRIPTS_FOLDER);
        string genFolder = GetAbsolutePath(GENERATED_FOLDER);
        if (!Directory.Exists(texFolder))
            Directory.CreateDirectory(texFolder);
        if (!Directory.Exists(scriptFolder))
            Directory.CreateDirectory(scriptFolder);
        if (!Directory.Exists(genFolder))
            Directory.CreateDirectory(genFolder);
    }

    public async Task Open()
    {
        InitializeRequired();
        await _dbModule.SyncFromDatabase();
        _placeModule.Sync();
    }

    public async Task Save()
    {
        await _dbModule.SyncToDatabase();
    }

    public void Tick(TimeSpan elapsed)
    {
        unchecked
        {
            ++_tickData.Frame;
        }
        float dt = (float)elapsed.TotalSeconds - _tickData.Time;
        _tickData.Enqueue(1.0f / dt);
        EditorModule.FPS = _tickData.FPS;
        _tickData.Time = (float)elapsed.TotalSeconds;
        bool modified = _dbModule.HasChange;
        if (modified != IsModified)
        {
            IsModified = modified;
            OnPropertyChanged(nameof(IsModified));
        }
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
            string dbFilePath = GetAbsolutePath(DatabaseModule.DATABASE_FILENAME);
            if (File.Exists(dbFilePath))
            {
                var conn = await SqliteHelper.OpenDatabase(dbFilePath);
                await conn!.OpenAsync();
                await conn!.CloseAsync();
            }
            _scriptModule.ScriptEngine.SetGlobal("Workspace", new WorkspaceProxy(this));
        }
        catch (Exception)
        {
            _valid = false;
            return;
        }
    }

    void InitializeRequired()
    {
        _scriptModule.LoadBuiltinScripts();
    }

    const string SOLUTION_FILENAME = "workspace.json";

    bool _valid = false;
    WorkspaceSolution? _solution;
    DatabaseModule _dbModule;
    PlacementModule _placeModule;
    EditorModule _editorModule;
    LogModule _logModule;
    ScriptingModule _scriptModule;
    UndoStack _undoStack = new UndoStack();
    TickData _tickData = new TickData();
}