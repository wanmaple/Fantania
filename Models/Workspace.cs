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
    public const string LEVELS_FOLDER = "levels";
    public const string TEXTURE_FOLDER = "textures";
    public const string ATLAS_FOLDER = "atlas";
    public const string MESS_FOLDER = ".fantania";
    public const string TEMP_FOLDER = ".temp";

    public event Action<Level> LevelChanged;

    public string RootFolder { get; private set; } = string.Empty;
    public string LevelsFolder { get; private set; } = string.Empty;
    public string MessFolder { get; private set; } = string.Empty;
    public string TempFolder { get; private set; } = string.Empty;
    public string TextureFolder { get; private set; } = string.Empty;
    public string AtlasFolder { get; private set; } = string.Empty;
    public SqliteDatabase MainDatabase => _dbMain;
    public SqliteDatabase EditorDatabase => _dbEditor;
    public DatabaseObjectSync DatabaseSync => _dbSync;
    public LevelGrouping LevelGrouping => _lvGrouping;
    public Stylegrounds CurrentStylegrounds
    {
        get
        {
            var ret = _stylegrounds.GetStylegrounds(CurrentLevel.Group, out bool isNew);
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

    private Level _currentLv;
    public Level CurrentLevel
    {
        get { return _currentLv; }
        set
        {
            if (_currentLv != value)
            {
                _currentLv = value;
                OnPropertyChanged(nameof(CurrentLevel));
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
        // levels
        string sgFile = GetAbsolutePath(STYLEGROUNDS_NAME);
        if (File.Exists(sgFile))
            File.Delete(sgFile);
        if (Directory.Exists(LevelsFolder))
            Directory.Delete(LevelsFolder, true);
        // folders
        if (!Directory.Exists(LevelsFolder))
            Directory.CreateDirectory(LevelsFolder);
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
        var lvDI = new DirectoryInfo(LevelsFolder);
        foreach (FileInfo lvFI in lvDI.GetFiles("*.bin"))
        {
            string fullPath = lvFI.FullName;
            // check valid
            string group = string.Empty;
            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    uint marker = br.ReadUInt32();
                    if (marker != Level.SERILIZATION_MARKER)
                    {
                        continue;
                    }
                    group = br.ReadString();
                }
            }
            _lvGrouping.AddLevelInfo(Path.GetFileNameWithoutExtension(fullPath), group);
        }
        if (!_lvGrouping.IsValid)
            throw new Exception("Invalid workspace without any levels.");
        await SyncLevel(_lvGrouping.Groups[0].LevelNames[0]);
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
        if (_currentLv.CheckModified())
            await _currentLv.Save(this);
        if (_stylegrounds.CheckModified())
            await _stylegrounds.Save(this);
        CheckModified();
    }

    public void CheckModified()
    {
        IsModified = _currentLv.CheckModified() || _dbSync.CheckModified() || _stylegrounds.CheckModified();
    }

    public async Task CreateLevelAsync(string lvName, string groupName)
    {
        string oldGroup = null;
        if (_currentLv != null)
        {
            oldGroup = _currentLv.Group;
            _currentLv.RenderInitialized -= OnLevelRenderInitialized;
            UnwatchBVHItemChanged();
            UnwatchLayerVisibilityChange();
        }
        CurrentLevel = new Level(lvName, groupName);
        await CurrentLevel.Save(this);
        _lvGrouping.AddLevelInfo(lvName, groupName);
        string newGroup = CurrentLevel.Group;
        if (oldGroup != newGroup)
        {
            OnPropertyChanged(nameof(CurrentStylegrounds));
        }
        CurrentLevel.RenderInitialized += OnLevelRenderInitialized;
        LevelChanged?.Invoke(CurrentLevel);
        CheckModified();
    }

    public async Task SwitchLevelAsync(string lvName)
    {
        string oldGroup = _currentLv.Group;
        _currentLv.RenderInitialized -= OnLevelRenderInitialized;
        UnwatchBVHItemChanged();
        UnwatchLayerVisibilityChange();
        CurrentLevel = new Level(lvName, string.Empty);
        string lvPath = Path.Combine(LevelsFolder, lvName + ".bin");
        await CurrentLevel.Load(this, lvPath);
        string newGroup = CurrentLevel.Group;
        if (oldGroup != newGroup)
        {
            OnPropertyChanged(nameof(CurrentStylegrounds));
        }
        CurrentLevel.RenderInitialized += OnLevelRenderInitialized;
        LevelChanged?.Invoke(_currentLv);
        CheckModified();
    }

    public async Task DeleteLevel(string lvName)
    {
        await Task.Run(() =>
        {
            string lvPath = Path.Combine(LevelsFolder, lvName + ".bin");
            if (File.Exists(lvPath))
            {
                File.Delete(lvPath);
            }
            string groupName = LevelGrouping.GetGroupName(lvName);
            LevelGrouping.RemoveLevelInfo(lvName, groupName);
        });
    }

    public async Task ChangeLevelGroup(string lvName, string oldGroup, string newGroup)
    {
        await Task.Run(() =>
        {
            string lvPath = Path.Combine(LevelsFolder, lvName + ".bin");
            if (File.Exists(lvPath))
            {
                byte[] bytes = null;
                using (var fs = new FileStream(lvPath, FileMode.Open, FileAccess.Read))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        br.ReadUInt32();
                        br.ReadString();
                        bytes = br.ReadBytes((int)(fs.Length - fs.Position));
                    }
                }
                using (var fs = new FileStream(lvPath, FileMode.Create, FileAccess.Write))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        bw.Write(Level.SERILIZATION_MARKER);
                        bw.Write(newGroup);
                        bw.Write(bytes);
                    }
                }
            }
            LevelGrouping.RemoveLevelInfo(lvName, oldGroup);
            LevelGrouping.AddLevelInfo(lvName, newGroup);
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
            _cacheLvObjModifying = new PropertyChangeInfo
            {
                PropertyName = e.PropertyName,
                OldValue = DatabaseObjectSync.DatabaseValue2CommandText(_dbSync.GetDatabasePropertyType(obj.GetType(), e.PropertyName), obj.GetType().GetProperty(e.PropertyName, BindingFlags.Instance | BindingFlags.Public).GetValue(obj)),
            };
        }
    }

    void OnWatchDatabaseObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_cacheLvObjModifying == null) return;
        DatabaseObject obj = sender as DatabaseObject;
        string newValue = DatabaseObjectSync.DatabaseValue2CommandText(_dbSync.GetDatabasePropertyType(obj.GetType(), e.PropertyName), obj.GetType().GetProperty(e.PropertyName, BindingFlags.Instance | BindingFlags.Public).GetValue(obj));
        _undoStack.AddOperation(new ModifyDatabaseObjectOperation(obj, new PropertyChangeInfo
        {
            PropertyName = _cacheLvObjModifying.PropertyName,
            OldValue = _cacheLvObjModifying.OldValue,
            NewValue = newValue,
        }));
        _cacheLvObjModifying = null;
    }

    void OnLevelRenderInitialized()
    {
        _currentLv.RenderInitialized -= OnLevelRenderInitialized;
        foreach (var obj in _currentLv._bvh.Enumerate())
        {
            WatchLevelObjectPropertyChange(obj);
        }
        WatchBVHItemChanged();
        WatchLayerVisibilityChange();
    }

    void OnLevelObjectAdded(LevelObject obj)
    {
        WatchLevelObjectPropertyChange(obj);
        _undoStack.AddOperation(new NewLevelObjectOperation(obj));
        _currentLv.MarkDirty();
        CheckModified();
    }

    void OnLevelObjectRemoved(LevelObject obj)
    {
        UnwatchLevelObjectPropertyChange(obj);
        _undoStack.AddOperation(new DeleteLevelObjectOperation(obj));
        _currentLv.MarkDirty();
        CheckModified();
    }

    internal void WatchBVHItemChanged()
    {
        _currentLv._bvh.ItemAdded += OnLevelObjectAdded;
        _currentLv._bvh.ItemRemoved += OnLevelObjectRemoved;
    }

    internal void UnwatchBVHItemChanged()
    {
        _currentLv._bvh.ItemAdded -= OnLevelObjectAdded;
        _currentLv._bvh.ItemRemoved -= OnLevelObjectRemoved;
    }

    void OnLayerVisibilityChanged(RenderLayers layer, bool visible)
    {
        _undoStack.AddOperation(new ChangeLayerVisibilityOperation(layer, visible));
    }

    internal void WatchLayerVisibilityChange()
    {
        _currentLv._allObjects.LayerVisibilityChanged += OnLayerVisibilityChanged;
    }

    internal void UnwatchLayerVisibilityChange()
    {
        _currentLv._allObjects.LayerVisibilityChanged -= OnLayerVisibilityChanged;
    }

    internal void WatchLevelObjectPropertyChange(LevelObject obj)
    {
        obj.PropertyChanging += OnLevelObjectPropertyChanging;
        obj.PropertyChanged += OnLevelObjectPropertyChanged;
    }

    internal void UnwatchLevelObjectPropertyChange(LevelObject obj)
    {
        obj.PropertyChanging -= OnLevelObjectPropertyChanging;
        obj.PropertyChanged -= OnLevelObjectPropertyChanged;
    }

    void OnLevelObjectPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        LevelObject obj = sender as LevelObject;
        var props = SerializationHelper.GetSerializableProperties(obj.GetType());
        var property = props.FirstOrDefault(prop => prop.PropertyInfo.Name == e.PropertyName);
        if (property != null)
        {
            object oldValue = SerializationHelper.StoreSerializableProperty(property.PropertyInfo, property.PropertyInfo.GetValue(obj));
            _cacheLvObjModifying = new PropertyChangeInfo
            {
                PropertyName = e.PropertyName,
                OldValue = oldValue,
            };
        }
    }

    void OnLevelObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        LevelObject obj = sender as LevelObject;
        var props = SerializationHelper.GetSerializableProperties(obj.GetType());
        var property = props.FirstOrDefault(prop => prop.PropertyInfo.Name == e.PropertyName);
        if (property != null)
        {
            object newValue = SerializationHelper.StoreSerializableProperty(property.PropertyInfo, property.PropertyInfo.GetValue(obj));
            _undoStack.AddOperation(new ModifyLevelObjectOperation(obj, new PropertyChangeInfo
            {
                PropertyName = _cacheLvObjModifying.PropertyName,
                OldValue = _cacheLvObjModifying.OldValue,
                NewValue = newValue,
            }));
            _currentLv.MarkDirty();
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

    async Task SyncLevel(string lvName)
    {
        if (_currentLv != null)
        {
            _currentLv.RenderInitialized -= OnLevelRenderInitialized;
        }
        _currentLv = new Level(lvName, string.Empty);
        string lvPath = Path.Combine(LevelsFolder, lvName + ".bin");
        if (File.Exists(lvPath))
        {
            await _currentLv.Load(this, lvPath);
        }
        _currentLv.RenderInitialized += OnLevelRenderInitialized;
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
            LevelsFolder = GetAbsolutePath(LEVELS_FOLDER);
            if (!Directory.Exists(LevelsFolder))
                Directory.CreateDirectory(LevelsFolder);
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
    LevelGrouping _lvGrouping = new LevelGrouping();
    GroupedStylegrounds _stylegrounds = new GroupedStylegrounds();

    // UI related.
    PanelStates _panelStates = new PanelStates();

    // For undoables and not effect DBSync logic.
    PropertyChangeInfo _cacheLvObjModifying;
    PropertyChangeInfo _cacheStylegroundModifying;

    // Background jobs.
    BackgroundJobManager _bgJobMgr = new BackgroundJobManager();
}