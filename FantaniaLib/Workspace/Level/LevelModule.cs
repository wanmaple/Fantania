using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Text.Json;
using Avalonia.Input.Platform;

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
    public LevelMetadata? Metadata => _curLv?.Metadata;
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
        string metaPath = GetLevelMetaPath(config.Name);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        _metaSyncer = new JsonDataSyncer<LevelMetadata>(lv.Metadata, SerializationRule.Default);
        File.WriteAllText(metaPath, _metaSyncer.SyncToJson());
        SetCurrentLevel(lv);
    }

    public async Task LoadLevel(string lvName)
    {
        if (!_lvDescs.Any(lv => lv.Name == lvName)) return;
        string lvPath = GetLevelFilePath(lvName);
        var lv = Level.OpenExist(lvPath);
        _syncer = new BinaryDataSyncer<LevelEntity>(lv.MutableEntities, SerializationRule.Default);
        await _syncer.SyncFromFile(lvPath);
        string metaPath = GetLevelMetaPath(lvName);
        if (File.Exists(metaPath))
        {
            try
            {
                string metaContent = File.ReadAllText(metaPath);
                _metaSyncer = new JsonDataSyncer<LevelMetadata>(lv.Metadata, SerializationRule.Default);
                _metaSyncer.SyncFromJson(metaContent);
            }
            catch (Exception)
            {
                // 解析失败，说明.meta文件内容不合法，直接忽略
            }
        }
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
                string metaPath = GetLevelMetaPath(desc.Name);
                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
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
            string metaPath = GetLevelMetaPath(desc.Name);
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }
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
                    UnwatchEntityPropertyChange(entity);
                }
                UnwatchMetaPropertyChange(_curLv.Metadata);
            }
            _curLv = lv;
            if (_curLv != null)
            {
                foreach (var entity in _curLv.Entities)
                {
                    WatchEntityPropertyChange(entity);
                }
                WatchMetaPropertyChange(_curLv.Metadata);
            }
            _workspace.UserTemporary.LatestEditingLevel = _curLv != null ? _curLv.Name : string.Empty;
            OnPropertyChanged(nameof(CurrentLevel));
            OnPropertyChanged(nameof(Metadata));
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
            WatchEntityPropertyChange(entity);
        }
    }

    internal void RemoveEntity(LevelEntity entity)
    {
        if (_curLv != null)
        {
            UnwatchEntityPropertyChange(entity);
            entity.OnExit(_workspace);
            _syncer!.RemoveObject(entity);
            EntityRemoved?.Invoke(entity);
        }
    }

    public async Task CutEntities(IReadOnlyList<LevelEntity> entities)
    {
        List<string> data = new List<string>(entities.Count);
        var clipboard = AvaloniaHelper.GetClipboard();
        foreach (var entity in entities)
        {
            data.Add(entity.OnCopy(_workspace));
            DeleteEntity(entity);
        }
        JsonSerializerOptions option = new JsonSerializerOptions
        {
            WriteIndented = false,
        };
        string serializedData = JsonSerializer.Serialize(data, option);
        await clipboard.SetTextAsync(serializedData);
    }

    public async Task CopyEntities(IReadOnlyList<LevelEntity> entities)
    {
        List<string> data = new List<string>(entities.Count);
        var clipboard = AvaloniaHelper.GetClipboard();
        foreach (var entity in entities)
        {
            data.Add(entity.OnCopy(_workspace));
        }
        JsonSerializerOptions option = new JsonSerializerOptions
        {
            WriteIndented = false,
        };
        string serializedData = JsonSerializer.Serialize(data, option);
        await clipboard.SetTextAsync(serializedData);
    }

    public async Task PasteEntities(Vector2Int worldPos)
    {
        var clipboard = AvaloniaHelper.GetClipboard();
        string serializedData = await clipboard.TryGetTextAsync() ?? string.Empty;
        if (string.IsNullOrEmpty(serializedData)) return;
        try
        {
            var ary = JsonSerializer.Deserialize<List<string>>(serializedData);
            Vector2 center = Vector2.Zero;
            List<LevelEntity> entities = new List<LevelEntity>(ary!.Count);
            foreach (string entityData in ary!)
            {
                Dictionary<string, object?> dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(entityData)!;
                string typename = dict["Type"]!.ToString()!;
                LevelEntity entity = (LevelEntity)Activator.CreateInstance(Type.GetType(typename)!, true)!;
                entity.OnPaste(_workspace, dict["Data"]!.ToString()!);
                entities.Add(entity);
                center += entity.Position.ToVector2();
            }
            center /= entities.Count;
            Vector2Int offset = worldPos - center.ToVector2i();
            foreach (var entity in entities)
            {
                entity.Position += offset;
                entity.Order = CurrentLevel!.NewOrder();
                PlaceEntity(entity);
            }
            _workspace.EditorModule.CancelSelection();
            foreach (var entity in entities)
            {
                if (entity is MultiNodesEntity multiNodeEntity)
                {
                    foreach (var node in multiNodeEntity.AllNodes)
                    {
                        _workspace.EditorModule.SelectedObjects.Add(node);
                    }
                }
                else
                {
                    _workspace.EditorModule.SelectedObjects.Add((ISelectableItem)entity);
                }
            }
            _workspace.EditorModule.Notify();
        }
        catch (Exception)
        {
            // 解析失败，说明剪贴板内容不是合法的实体数据，直接忽略
            await clipboard.SetTextAsync(null);
        }
    }

    public void WatchEntityPropertyChange(LevelEntity obj)
    {
        obj.PropertyChanging += OnLevelEntityPropertyChanging;
        obj.PropertyChanged += OnLevelEntityPropertyChanged;
    }

    public void UnwatchEntityPropertyChange(LevelEntity obj)
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

    public void WatchMetaPropertyChange(LevelMetadata meta)
    {
        meta.PropertyChanging += OnLevelMetaPropertyChanging;
        meta.PropertyChanged += OnLevelMetaPropertyChanged;
    }

    public void UnwatchMetaPropertyChange(LevelMetadata meta)
    {
        meta.PropertyChanging -= OnLevelMetaPropertyChanging;
        meta.PropertyChanged -= OnLevelMetaPropertyChanged;
    }

    void OnLevelMetaPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        LevelMetadata meta = (LevelMetadata)sender!;
        FieldInfo? fieldInfo = meta.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            _tempChange = new PropertyChangeInfo
            {
                PropertyName = fieldInfo.FieldName,
                OldValue = _rule.CastTo(fieldInfo.FieldType, meta.GetFieldValue(fieldInfo.FieldName), meta),
            };
        }
    }

    void OnLevelMetaPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        LevelMetadata meta = (LevelMetadata)sender!;
        FieldInfo? fieldInfo = meta.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            _tempChange!.NewValue = _rule.CastTo(fieldInfo.FieldType, meta.GetFieldValue(fieldInfo.FieldName), meta);
            var op = new ModifyLevelMetaOperation(_workspace, meta, _tempChange);
            _workspace.UndoStack.AddOperation(op);
            _tempChange = null;
        }
    }

    string GetLevelFilePath(string lvName)
    {
        return _workspace.GetAbsolutePath(Workspace.LEVELS_FOLDER, lvName + LEVEL_EXTENSION);
    }

    string GetLevelMetaPath(string lvName)
    {
        return _workspace.GetAbsolutePath(Workspace.LEVELS_FOLDER, lvName + LEVEL_META_EXTENSION);
    }

    const string LEVEL_EXTENSION = ".lv";
    const string LEVEL_META_EXTENSION = ".meta";

    ObservableCollection<LevelDescription> _lvDescs = new ObservableCollection<LevelDescription>();
    BinaryDataSyncer<LevelEntity>? _syncer;
    JsonDataSyncer<LevelMetadata>? _metaSyncer;
    LayerManager _layerMgr = new LayerManager();
    SpecialPropertyObserver _specPropOb = new SpecialPropertyObserver();

    PropertyChangeInfo? _tempChange;
    SerializationRule _rule = SerializationRule.Default;
}