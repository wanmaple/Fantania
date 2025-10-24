using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class Workspace : ObservableObject
{
    public const int LATEST_VERSION = 1;
    public const string PROJECT_FILE_NAME = "project.json";
    public const string MAIN_DATABASE_NAME = "project.db";
    public const string EDITOR_DATABASE_NAME = "editor.db";
    public const string STYLEGROUNDS_NAME = "stylegrounds.bin";
    public const string WORLDS_FOLDER = "worlds";
    public const string TEXTURE_FOLDER = "textures";
    public const string ATLAS_FOLDER = "atlas";
    public const string MESS_FOLDER = ".fantania";
    public const string TEMP_FOLDER = ".temp";

    public event Action<World> WorldChanged;

    public string RootFolder { get; private set; } = string.Empty;
    public string WorldsFolder { get; private set; } = string.Empty;
    public string MessFolder { get; private set; } = string.Empty;
    public string TempFolder { get; private set; } = string.Empty;
    public string TextureFolder { get; private set; } = string.Empty;
    public string AtlasFolder { get; private set; } = string.Empty;
    public SqliteDatabase MainDatabase => _dbMain;
    public SqliteDatabase EditorDatabase => _dbEditor;
    public DatabaseObjectSync DatabaseSync => _dbSync;
    public WorldGrouping WorldGrouping => _worldGrouping;
    public Stylegrounds CurrentStylegrounds
    {
        get
        {
            var ret = _stylegrounds.GetStylegrounds(CurrentWorld.Group, out bool isNew);
            if (isNew)
            {
                WatchStylegroundAddedOrRemoved(ret);
            }
            return ret;
        }
    }

    public PanelStates PanelStates => _panelStates;

    public WorkspaceConfig? Config => _config;

    public BackgroundJobManager JobManager => _bgJobMgr;

    private bool _modified = false;
    public bool IsModified
    {
        get { return _modified; }
        set
        {
            if (_modified != value)
            {
                _modified = value;
                OnPropertyChanged(nameof(IsModified));
            }
        }
    }

    private World _currentWorld;
    public World CurrentWorld
    {
        get { return _currentWorld; }
        set
        {
            if (_currentWorld != value)
            {
                _currentWorld = value;
                OnPropertyChanged(nameof(CurrentWorld));
            }
        }
    }

    private UndoStack _undoStack;
    public UndoStack UndoStack
    {
        get { return _undoStack; }
        set
        {
            if (_undoStack != value)
            {
                _undoStack = value;
                OnPropertyChanged(nameof(UndoStack));
            }
        }
    }

    public Workspace(string rootDir)
    {
        RootFolder = OSHelper.ConvertAvaloniaUriToStandardUri(rootDir);
        RootFolder = RootFolder.Replace('\\', '/');
        UndoStack = new UndoStack();
        LoadStuffs();
    }

    ~Workspace()
    {
        if (_dbMain != null)
            _dbMain.Dispose();
        if (_dbEditor != null)
            _dbEditor.Dispose();
    }

    public bool IsValid()
    {
        if (_config == null) return false;
        if (_dbMain == null) return false;
        if (_dbEditor == null) return false;
        return true;
    }

    public void AddObject(DatabaseObject obj, bool record = true)
    {
        _dbSync.AddObject(this, obj);
        if (record)
        {
            UndoStack.AddOperation(new NewDatabaseObjectOperation(obj));
            WatchDatabaseObjectPropertyChange(obj);
        }
        CheckModified();
    }

    public void RemoveObject(DatabaseObject obj, bool record = true)
    {
        _dbSync.RemoveObject(this, obj);
        if (record)
        {
            UndoStack.AddOperation(new DeleteDatabaseObjectOperation(obj));
            UnwatchDatabaseObjectPropertyChange(obj);
        }
        CheckModified();
    }

    public async Task Initialize()
    {
        // create json file
        string projFile = GetAbsolutePath(PROJECT_FILE_NAME);
        _config = new WorkspaceConfig
        {
            Version = LATEST_VERSION,
            Modding = false,
        };
        using (var fs = new FileStream(projFile, FileMode.Create, FileAccess.Write))
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            JsonSerializer.Serialize(fs, _config, options);
        }
        // create database file.
        if (_dbMain != null)
            _dbMain.Dispose();
        string dbFile = GetAbsolutePath(MAIN_DATABASE_NAME);
        if (File.Exists(dbFile))
            File.Delete(dbFile);
        _dbMain = new SqliteDatabase(dbFile);
        if (_dbEditor != null)
            _dbEditor.Dispose();
        string editorDbFile = GetAbsolutePath(EDITOR_DATABASE_NAME);
        if (File.Exists(editorDbFile))
            File.Delete(editorDbFile);
        _dbEditor = new SqliteDatabase(editorDbFile);
        // worlds
        string sgFile = GetAbsolutePath(STYLEGROUNDS_NAME);
        if (File.Exists(sgFile))
            File.Delete(sgFile);
        if (Directory.Exists(WorldsFolder))
            Directory.Delete(WorldsFolder, true);
        // folders
        if (!Directory.Exists(WorldsFolder))
            Directory.CreateDirectory(WorldsFolder);
        if (!Directory.Exists(MessFolder))
            Directory.CreateDirectory(MessFolder);
        if (!Directory.Exists(TextureFolder))
            Directory.CreateDirectory(TextureFolder);
        if (!Directory.Exists(AtlasFolder))
            Directory.CreateDirectory(AtlasFolder);
        await SyncDatabase(true);
    }

    public async Task SyncAllData(bool syncDbScheme = false)
    {
        await SyncDatabase(syncDbScheme);
        var worldDI = new DirectoryInfo(WorldsFolder);
        foreach (FileInfo worldFI in worldDI.GetFiles("*.bin"))
        {
            string fullPath = worldFI.FullName;
            // check valid
            string group = string.Empty;
            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    uint marker = br.ReadUInt32();
                    if (marker != World.SERILIZATION_MARKER)
                    {
                        continue;
                    }
                    group = br.ReadString();
                }
            }
            _worldGrouping.AddWorldInfo(Path.GetFileNameWithoutExtension(fullPath), group);
        }
        if (!_worldGrouping.IsValid)
            throw new Exception("Invalid workspace without any worlds.");
        await SyncWorld(_worldGrouping.Groups[0].WorldNames[0]);
        string stylegroundsPath = GetAbsolutePath(STYLEGROUNDS_NAME);
        _stylegrounds = new GroupedStylegrounds();
        if (File.Exists(stylegroundsPath))
        {
            await SyncStylegrounds();
        }
    }

    public async Task Save()
    {
        await CreateBackup();
        if (_dbSync.CheckModified())
            await _dbSync.SyncTo(this);
        if (_currentWorld.CheckModified())
            await _currentWorld.Save(this);
        if (_stylegrounds.CheckModified())
            await _stylegrounds.Save(this);
        CheckModified();
    }

    public void CheckModified()
    {
        IsModified = _currentWorld.CheckModified() || _dbSync.CheckModified() || _stylegrounds.CheckModified();
    }

    public async Task CreateWorldAsync(string worldName, string groupName)
    {
        string oldGroup = null;
        if (_currentWorld != null)
        {
            oldGroup = _currentWorld.Group;
            _currentWorld.RenderInitialized -= OnWorldRenderInitialized;
            UnwatchBVHItemChanged();
            UnwatchLayerVisibilityChange();
        }
        CurrentWorld = new World(worldName, groupName);
        await CurrentWorld.Save(this);
        _worldGrouping.AddWorldInfo(worldName, groupName);
        string newGroup = CurrentWorld.Group;
        if (oldGroup != newGroup)
        {
            OnPropertyChanged(nameof(CurrentStylegrounds));
        }
        CurrentWorld.RenderInitialized += OnWorldRenderInitialized;
        WorldChanged?.Invoke(CurrentWorld);
        CheckModified();
    }

    public async Task SwitchWorldAsync(string worldName)
    {
        string oldGroup = _currentWorld.Group;
        _currentWorld.RenderInitialized -= OnWorldRenderInitialized;
        UnwatchBVHItemChanged();
        UnwatchLayerVisibilityChange();
        CurrentWorld = new World(worldName, string.Empty);
        string worldPath = Path.Combine(WorldsFolder, worldName + ".bin");
        await CurrentWorld.Load(this, worldPath);
        string newGroup = CurrentWorld.Group;
        if (oldGroup != newGroup)
        {
            OnPropertyChanged(nameof(CurrentStylegrounds));
        }
        CurrentWorld.RenderInitialized += OnWorldRenderInitialized;
        WorldChanged?.Invoke(_currentWorld);
        CheckModified();
    }

    public async Task DeleteWorld(string worldName)
    {
        await Task.Run(() =>
        {
            string worldPath = Path.Combine(WorldsFolder, worldName + ".bin");
            if (File.Exists(worldPath))
            {
                File.Delete(worldPath);
            }
            string groupName = WorldGrouping.GetGroupName(worldName);
            WorldGrouping.RemoveWorldInfo(worldName, groupName);
        });
    }

    public async Task ChangeWorldGroup(string worldName, string oldGroup, string newGroup)
    {
        await Task.Run(() =>
        {
            string worldPath = Path.Combine(WorldsFolder, worldName + ".bin");
            if (File.Exists(worldPath))
            {
                byte[] bytes = null;
                using (var fs = new FileStream(worldPath, FileMode.Open, FileAccess.Read))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        br.ReadUInt32();
                        br.ReadString();
                        bytes = br.ReadBytes((int)(fs.Length - fs.Position));
                    }
                }
                using (var fs = new FileStream(worldPath, FileMode.Create, FileAccess.Write))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        bw.Write(World.SERILIZATION_MARKER);
                        bw.Write(newGroup);
                        bw.Write(bytes);
                    }
                }
            }
            WorldGrouping.RemoveWorldInfo(worldName, oldGroup);
            WorldGrouping.AddWorldInfo(worldName, newGroup);
        });
    }

    public string GetAbsolutePath(string path)
    {
        path = path.Replace('\\', '/');
        if (!path.StartsWith(RootFolder))
        {
            path = Path.Combine(RootFolder, path).Replace('\\', '/');
        }
        return path;
    }

    public bool FileExists(string path)
    {
        path = GetAbsolutePath(path);
        return File.Exists(path);
    }

    public void OnInitialized()
    {
        _bgJobMgr.Initialize(this);
        _dbSync.LateInit(this);
        foreach (var pair in MainDatabase._groupedObjects)
        {
            foreach (var obj in pair.Value)
            {
                obj.OnInitialized(this);
                // register value changes to undostack
                WatchDatabaseObjectPropertyChange(obj);
            }
        }
    }

    public void OnFinalized()
    {
        foreach (var pair in MainDatabase._groupedObjects)
        {
            foreach (var obj in pair.Value)
            {
                obj.OnUnintialized(this);
            }
        }
        _bgJobMgr.Finalize();
    }

    internal void WatchDatabaseObjectPropertyChange(DatabaseObject obj)
    {
        obj.PropertyChanging += OnWatchDatabaseObjectPropertyChanging;
        obj.PropertyChanged += OnWatchDatabaseObjectPropertyChanged;
    }

    internal void UnwatchDatabaseObjectPropertyChange(DatabaseObject obj)
    {
        obj.PropertyChanging -= OnWatchDatabaseObjectPropertyChanging;
        obj.PropertyChanged -= OnWatchDatabaseObjectPropertyChanged;
    }

    void OnWatchDatabaseObjectPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        DatabaseObject obj = sender as DatabaseObject;
        if (_dbSync.IsDatabasePropertyType(obj.GetType(), e.PropertyName))
        {
            _cacheWorldObjModifying = new PropertyChangeInfo
            {
                PropertyName = e.PropertyName,
                OldValue = DatabaseObjectSync.DatabaseValue2CommandText(_dbSync.GetDatabasePropertyType(obj.GetType(), e.PropertyName), obj.GetType().GetProperty(e.PropertyName, BindingFlags.Instance | BindingFlags.Public).GetValue(obj)),
            };
        }
    }

    void OnWatchDatabaseObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_cacheWorldObjModifying == null) return;
        DatabaseObject obj = sender as DatabaseObject;
        string newValue = DatabaseObjectSync.DatabaseValue2CommandText(_dbSync.GetDatabasePropertyType(obj.GetType(), e.PropertyName), obj.GetType().GetProperty(e.PropertyName, BindingFlags.Instance | BindingFlags.Public).GetValue(obj));
        _undoStack.AddOperation(new ModifyDatabaseObjectOperation(obj, new PropertyChangeInfo
        {
            PropertyName = _cacheWorldObjModifying.PropertyName,
            OldValue = _cacheWorldObjModifying.OldValue,
            NewValue = newValue,
        }));
        _cacheWorldObjModifying = null;
    }

    void OnWorldRenderInitialized()
    {
        _currentWorld.RenderInitialized -= OnWorldRenderInitialized;
        foreach (var obj in _currentWorld._bvh.Enumerate())
        {
            WatchWorldObjectPropertyChange(obj);
        }
        WatchBVHItemChanged();
        WatchLayerVisibilityChange();
    }

    void OnWorldObjectAdded(WorldObject obj)
    {
        WatchWorldObjectPropertyChange(obj);
        _undoStack.AddOperation(new NewWorldObjectOperation(obj));
        _currentWorld.MarkDirty();
        CheckModified();
    }

    void OnWorldObjectRemoved(WorldObject obj)
    {
        UnwatchWorldObjectPropertyChange(obj);
        _undoStack.AddOperation(new DeleteWorldObjectOperation(obj));
        _currentWorld.MarkDirty();
        CheckModified();
    }

    internal void WatchBVHItemChanged()
    {
        _currentWorld._bvh.ItemAdded += OnWorldObjectAdded;
        _currentWorld._bvh.ItemRemoved += OnWorldObjectRemoved;
    }

    internal void UnwatchBVHItemChanged()
    {
        _currentWorld._bvh.ItemAdded -= OnWorldObjectAdded;
        _currentWorld._bvh.ItemRemoved -= OnWorldObjectRemoved;
    }

    void OnLayerVisibilityChanged(RenderLayers layer, bool visible)
    {
        _undoStack.AddOperation(new ChangeLayerVisibilityOperation(layer, visible));
    }

    internal void WatchLayerVisibilityChange()
    {
        _currentWorld._allObjects.LayerVisibilityChanged += OnLayerVisibilityChanged;
    }

    internal void UnwatchLayerVisibilityChange()
    {
        _currentWorld._allObjects.LayerVisibilityChanged -= OnLayerVisibilityChanged;
    }

    internal void WatchWorldObjectPropertyChange(WorldObject obj)
    {
        obj.PropertyChanging += OnWorldObjectPropertyChanging;
        obj.PropertyChanged += OnWorldObjectPropertyChanged;
    }

    internal void UnwatchWorldObjectPropertyChange(WorldObject obj)
    {
        obj.PropertyChanging -= OnWorldObjectPropertyChanging;
        obj.PropertyChanged -= OnWorldObjectPropertyChanged;
    }

    void OnWorldObjectPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        WorldObject obj = sender as WorldObject;
        var props = SerializationHelper.GetSerializableProperties(obj.GetType());
        var property = props.FirstOrDefault(prop => prop.PropertyInfo.Name == e.PropertyName);
        if (property != null)
        {
            object oldValue = SerializationHelper.StoreSerializableProperty(property.PropertyInfo, property.PropertyInfo.GetValue(obj));
            _cacheWorldObjModifying = new PropertyChangeInfo
            {
                PropertyName = e.PropertyName,
                OldValue = oldValue,
            };
        }
    }

    void OnWorldObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        WorldObject obj = sender as WorldObject;
        var props = SerializationHelper.GetSerializableProperties(obj.GetType());
        var property = props.FirstOrDefault(prop => prop.PropertyInfo.Name == e.PropertyName);
        if (property != null)
        {
            object newValue = SerializationHelper.StoreSerializableProperty(property.PropertyInfo, property.PropertyInfo.GetValue(obj));
            _undoStack.AddOperation(new ModifyWorldObjectOperation(obj, new PropertyChangeInfo
            {
                PropertyName = _cacheWorldObjModifying.PropertyName,
                OldValue = _cacheWorldObjModifying.OldValue,
                NewValue = newValue,
            }));
            _currentWorld.MarkDirty();
            CheckModified();
        }
    }

    void OnBackgroundAdded(Stylegrounds sgs, Styleground bg, Styleground prev)
    {
        var op = new NewBackgroundOperation(sgs, bg, prev);
        _undoStack.AddOperation(op);
        WatchStylegroundPropertyChange(bg);
        CheckModified();
    }

    void OnBackgroundRemoved(Stylegrounds sgs, Styleground bg, Styleground prev)
    {
        var op = new DeleteBackgroundOperation(sgs, bg, prev);
        _undoStack.AddOperation(op);
        UnwatchStylegroundPropertyChange(bg);
        CheckModified();
    }

    void OnBackgroundMoved(Stylegrounds sgs, Styleground bg, Styleground oldPrev, Styleground newPrev)
    {
        var op = new MoveBackgroundOperation(sgs, bg, oldPrev, newPrev);
        _undoStack.AddOperation(op);
        CheckModified();
    }

    void OnForegroundAdded(Stylegrounds sgs, Styleground fg, Styleground prev)
    {
        var op = new NewForegroundOperation(sgs, fg, prev);
        _undoStack.AddOperation(op);
        WatchStylegroundPropertyChange(fg);
        CheckModified();
    }

    void OnForegroundRemoved(Stylegrounds sgs, Styleground fg, Styleground prev)
    {
        var op = new DeleteForegroundOperation(sgs, fg, prev);
        _undoStack.AddOperation(op);
        UnwatchStylegroundPropertyChange(fg);
        CheckModified();
    }

    void OnForegroundMoved(Stylegrounds sgs, Styleground fg, Styleground oldPrev, Styleground newPrev)
    {
        var op = new MoveForegroundOperation(sgs, fg, oldPrev, newPrev);
        _undoStack.AddOperation(op);
        CheckModified();
    }

    internal void WatchStylegroundAddedOrRemoved(Stylegrounds sgs)
    {
        sgs.BackgroundAdded += OnBackgroundAdded;
        sgs.BackgroundRemoved += OnBackgroundRemoved;
        sgs.BackgroundMoved += OnBackgroundMoved;
        sgs.ForegroundAdded += OnForegroundAdded;
        sgs.ForegroundRemoved += OnForegroundRemoved;
        sgs.ForegroundMoved += OnForegroundMoved;
    }

    internal void UnwatchStylegroundAddedOrRemoved(Stylegrounds sgs)
    {
        sgs.BackgroundAdded -= OnBackgroundAdded;
        sgs.BackgroundRemoved -= OnBackgroundRemoved;
        sgs.BackgroundMoved -= OnBackgroundMoved;
        sgs.ForegroundAdded -= OnForegroundAdded;
        sgs.ForegroundRemoved -= OnForegroundRemoved;
        sgs.ForegroundMoved -= OnForegroundMoved;
    }

    void OnStylegroundPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        Styleground obj = sender as Styleground;
        var props = SerializationHelper.GetSerializableProperties(obj.GetType());
        var property = props.FirstOrDefault(prop => prop.PropertyInfo.Name == e.PropertyName);
        if (property != null)
        {
            object oldValue = SerializationHelper.StoreSerializableProperty(property.PropertyInfo, property.PropertyInfo.GetValue(obj));
            _cacheStylegroundModifying = new PropertyChangeInfo
            {
                PropertyName = e.PropertyName,
                OldValue = oldValue,
            };
        }
    }

    void OnStylegroundPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Styleground obj = sender as Styleground;
        var props = SerializationHelper.GetSerializableProperties(obj.GetType());
        var property = props.FirstOrDefault(prop => prop.PropertyInfo.Name == e.PropertyName);
        if (property != null)
        {
            object newValue = SerializationHelper.StoreSerializableProperty(property.PropertyInfo, property.PropertyInfo.GetValue(obj));
            _undoStack.AddOperation(new ModifyStylegroundOperation(obj, new PropertyChangeInfo
            {
                PropertyName = _cacheStylegroundModifying.PropertyName,
                OldValue = _cacheStylegroundModifying.OldValue,
                NewValue = newValue,
            }));
            CurrentStylegrounds.IsModified = true;
            CheckModified();
        }
    }

    internal void WatchStylegroundPropertyChange(Styleground sg)
    {
        sg.PropertyChanging += OnStylegroundPropertyChanging;
        sg.PropertyChanged += OnStylegroundPropertyChanged;
    }

    internal void UnwatchStylegroundPropertyChange(Styleground sg)
    {
        sg.PropertyChanging -= OnStylegroundPropertyChanging;
        sg.PropertyChanged -= OnStylegroundPropertyChanged;
    }

    async Task SyncDatabase(bool syncScheme)
    {
        var groups = ReflectionHelper.GetAllGroupedInfo(Assembly.GetExecutingAssembly());
        if (syncScheme)
        {
            await _dbMain.OpenAsync();
            await _dbEditor.OpenAsync();
            var mainSync = new MainDatabaseSync();
            var editorSync = new EditorDatabaseSync();
            foreach (var group in groups)
            {
                await mainSync.SyncDatabase(_dbMain, group);
                await editorSync.SyncDatabase(_dbEditor, group);
            }
            await _dbEditor.CloseAsync();
            await _dbMain.CloseAsync();
        }
        await _dbSync.SyncFrom(this, groups);
    }

    async Task SyncWorld(string worldName)
    {
        if (_currentWorld != null)
        {
            _currentWorld.RenderInitialized -= OnWorldRenderInitialized;
        }
        _currentWorld = new World(worldName, string.Empty);
        string worldPath = Path.Combine(WorldsFolder, worldName + ".bin");
        if (File.Exists(worldPath))
        {
            await _currentWorld.Load(this, worldPath);
        }
        _currentWorld.RenderInitialized += OnWorldRenderInitialized;
    }

    async Task SyncStylegrounds()
    {
        _stylegrounds = new GroupedStylegrounds();
        await _stylegrounds.Load(this);
        foreach (Stylegrounds sgs in _stylegrounds.AllStylegrounds)
        {
            WatchStylegroundAddedOrRemoved(sgs);
            foreach (var bg in sgs.Backgrounds)
            {
                WatchStylegroundPropertyChange(bg);
            }
            foreach (var fg in sgs.Foregrounds)
            {
                WatchStylegroundPropertyChange(fg);
            }
        }
    }

    async Task LoadStuffs()
    {
        try
        {
            string projFile = GetAbsolutePath(PROJECT_FILE_NAME);
            if (File.Exists(projFile))
            {
                string json = File.ReadAllText(projFile);
                _config = JsonSerializer.Deserialize<WorkspaceConfig>(json);
            }
            string dbFile = GetAbsolutePath(MAIN_DATABASE_NAME);
            _dbMain = new SqliteDatabase(dbFile);
            string dbEditor = GetAbsolutePath(EDITOR_DATABASE_NAME);
            _dbEditor = new SqliteDatabase(dbEditor);
            WorldsFolder = GetAbsolutePath(WORLDS_FOLDER);
            TextureFolder = GetAbsolutePath(TEXTURE_FOLDER);
            AtlasFolder = GetAbsolutePath(ATLAS_FOLDER);
            MessFolder = GetAbsolutePath(MESS_FOLDER);
            TempFolder = GetAbsolutePath(TEMP_FOLDER);
            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
        }
        catch (Exception)
        {
            _dbMain = null;
            _dbEditor = null;
        }
    }

    async Task CreateBackup()
    {

    }

    // Core data.
    WorkspaceConfig? _config = null;
    SqliteDatabase _dbMain;
    SqliteDatabase _dbEditor;
    DatabaseObjectSync _dbSync = new DatabaseObjectSync();
    WorldGrouping _worldGrouping = new WorldGrouping();
    GroupedStylegrounds _stylegrounds = new GroupedStylegrounds();

    // UI related.
    PanelStates _panelStates = new PanelStates();

    // For undoables and not effect DBSync logic.
    PropertyChangeInfo _cacheWorldObjModifying;
    PropertyChangeInfo _cacheStylegroundModifying;

    // Background jobs.
    BackgroundJobManager _bgJobMgr = new BackgroundJobManager();
}