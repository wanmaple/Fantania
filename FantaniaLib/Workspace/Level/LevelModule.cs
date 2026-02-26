using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FantaniaLib;

public class LevelModule : WorkspaceModule
{
    public const int MAX_LAYER = 39;
    public const int MIN_LAYER = -40;

    public event Action<LevelEntity>? EntityAdded;
    public event Action<LevelEntity>? EntityRemoved;

    private Level? _curLv = null;
    public IReadonlyLevel? CurrentLevel => _curLv;
    public LayerManager LayerManager => _layerMgr;
    public SpecialPropertyObserver SpecialPropertyObserver => _specPropOb;

    public IReadOnlyList<LevelDescription> LevelDescriptions => _lvDescs;
    public bool HasChange => (_syncer != null ? _syncer.HasChange : false) || _specPropOb.HasChange;

    public LevelModule(IWorkspace workspace) : base(workspace)
    {
        LoadExistingLevels();
    }

    public async Task CreateLevel(LevelCreateConfig config)
    {
        string lvFolder = _workspace.GetAbsolutePath(Workspace.LEVELS_FOLDER);
        if (!Directory.Exists(lvFolder))
            Directory.CreateDirectory(lvFolder);
        string lvPath = GetLevelFilePath(config.Name);
        Level lv = Level.CreateNew(config);
        _lvDescs.Add(new LevelDescription
        {
            Name = lv.Name,
        });
        _lvDescs.StableSort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        _syncer = new BinaryDataSyncer<LevelEntity>(lv.MutableEntities, SerializationRule.Default);
        await _syncer.SyncToFile(lvPath);
        SetCurrentLevel(lv);
    }

    public async Task LoadLevel(string lvName)
    {
        if (!_lvDescs.Any(lv => lv.Name == lvName)) return;
        string lvPath = GetLevelFilePath(lvName);
        var lv = Level.OpenExist(lvPath);
        _syncer = new BinaryDataSyncer<LevelEntity>(lv.MutableEntities, SerializationRule.Default);
        await _syncer.SyncFromFile(lvPath);
        SetCurrentLevel(lv);
        lv.OnLevelLoaded(_workspace);
    }

    public async Task DeleteLevel(string lvName)
    {
        await Task.Run(() =>
        {
            var desc = _lvDescs.FirstOrDefault(lv => lv.Name == lvName);
            if (desc != null)
            {
                string path = GetLevelFilePath(desc.Name);
                File.Delete(path);
                _lvDescs.Remove(desc);
                if (_curLv != null && _curLv.Name == desc.Name)
                {
                    _syncer = null;
                    SetCurrentLevel(null!);
                }
            }
        });
    }

    public void DeleteAllLevels()
    {
        _lvDescs.Clear();
        foreach (var desc in _lvDescs)
        {
            string path = GetLevelFilePath(desc.Name);
            File.Delete(path);
        }
    }

    public async Task SyncCurrentLevel()
    {
        if (_curLv != null)
        {
            string lvPath = GetLevelFilePath(_curLv.Name);
            await _syncer!.SyncToFile(lvPath);
            _specPropOb.Reset();
            foreach (var entity in _curLv.Entities)
            {
                if (entity is MultiNodesEntity e)
                {
                    _specPropOb.Observe(e);
                }
            }
        }
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
            _lvDescs.StableSort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        }
    }

    void SetCurrentLevel(Level lv)
    {
        if (_curLv != lv)
        {
            _workspace.EditorModule.SelectedObjects.Clear();
            _specPropOb.Reset();
            OnPropertyChanging(nameof(CurrentLevel));
            if (_curLv != null)
            {
                foreach (var entity in _curLv.Entities)
                {
                    UnwatchPropertyChange(entity);
                }
            }
            _curLv = lv;
            if (_curLv != null)
            {
                foreach (var entity in _curLv.Entities)
                {
                    WatchPropertyChange(entity);
                }
            }
            _workspace.UserTemporary.LatestEditingLevel = _curLv != null ? _curLv.Name : string.Empty;
            OnPropertyChanged(nameof(CurrentLevel));
        }
    }

    public void PlaceEntity(LevelEntity entity)
    {
        AddEntity(entity);
        var op = new NewLevelEntityOperation(_workspace, entity);
        _workspace.UndoStack.AddOperation(op);
    }

    public void DeleteEntity(LevelEntity entity)
    {
        RemoveEntity(entity);
        var op = new DelLevelEntityOperation(_workspace, entity);
        _workspace.UndoStack.AddOperation(op);
    }

    internal void AddEntity(LevelEntity entity)
    {
        if (_curLv != null)
        {
            _syncer!.AddObject(entity);
            entity.OnEnter(_workspace);
            EntityAdded?.Invoke(entity);
            WatchPropertyChange(entity);
        }
    }

    internal void RemoveEntity(LevelEntity entity)
    {
        if (_curLv != null)
        {
            UnwatchPropertyChange(entity);
            entity.OnExit(_workspace);
            _syncer!.RemoveObject(entity);
            EntityRemoved?.Invoke(entity);
        }
    }

    public void WatchPropertyChange(LevelEntity obj)
    {
        obj.PropertyChanging += OnLevelEntityPropertyChanging;
        obj.PropertyChanged += OnLevelEntityPropertyChanged;
    }

    public void UnwatchPropertyChange(LevelEntity obj)
    {
        obj.PropertyChanging -= OnLevelEntityPropertyChanging;
        obj.PropertyChanged -= OnLevelEntityPropertyChanged;
    }

    void OnLevelEntityPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        LevelEntity entity = (LevelEntity)sender!;
        FieldInfo? fieldInfo = entity.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            _tempChange = new PropertyChangeInfo
            {
                PropertyName = fieldInfo.FieldName,
                OldValue = _rule.CastTo(fieldInfo.FieldType, entity.GetFieldValue(fieldInfo.FieldName), entity),
            };
        }
    }

    void OnLevelEntityPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        LevelEntity entity = (LevelEntity)sender!;
        FieldInfo? fieldInfo = entity.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            _tempChange!.NewValue = _rule.CastTo(fieldInfo.FieldType, entity.GetFieldValue(fieldInfo.FieldName), entity);
            var op = new ModifyLevelEntityOperation(_workspace, entity, _tempChange);
            _workspace.UndoStack.AddOperation(op);
            _tempChange = null;
        }
    }

    string GetLevelFilePath(string lvName)
    {
        return _workspace.GetAbsolutePath(Workspace.LEVELS_FOLDER, lvName + LEVEL_EXTENSION);
    }

    const string LEVEL_EXTENSION = ".lv";

    ObservableCollection<LevelDescription> _lvDescs = new ObservableCollection<LevelDescription>();
    BinaryDataSyncer<LevelEntity>? _syncer;
    LayerManager _layerMgr = new LayerManager();
    SpecialPropertyObserver _specPropOb = new SpecialPropertyObserver();

    PropertyChangeInfo? _tempChange;
    SerializationRule _rule = SerializationRule.Default;
}