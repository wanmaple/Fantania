using System.ComponentModel;

namespace FantaniaLib;

public class DatabaseModule : WorkspaceModule
{
    public const string DATABASE_FILENAME = "main.db";

    public SerializationRule SerializationRule => _rule;
    public bool HasChange => _syncer.HasChange;

    public DatabaseModule(IWorkspace workspace) : base(workspace)
    {
        _db = new SqliteDatabase(_workspace.GetAbsolutePath(DATABASE_FILENAME));
        _syncer = new SqliteSyncer(_db, _rule);

        _db.ExecutingNonQuery += OnDatabaseExecutingSql;
    }

    public async Task SyncFromDatabase()
    {
        await _syncer.SyncFromDatabase(_workspace.PlacementModule.PlacementTemplateMap.Values.ToArray(), Array.Empty<Type>());
    }

    public async Task SyncToDatabase()
    {
        await _syncer.SyncToDatabase();
    }

    public IReadOnlyList<DatabaseObject> GetObjectsOfGroup(string group)
    {
        return _db.ObjectsOfGroup(group);
    }

    public IReadOnlyList<DatabaseObject> GetObjectsOfType(string type)
    {
        return _db.ObjectsOfType(type);
    }

    public T? GetGroupedObject<T>(string group, int id) where T : DatabaseObject
    {
        return GetObjectsOfGroup(group).FirstOrDefault(o => o.ID == id) as T;
    }

    public T? GetTypedObject<T>(string type, int id) where T : DatabaseObject
    {
        return GetObjectsOfType(type).FirstOrDefault(o => o.ID == id) as T;
    }

    public void AddObject(DatabaseObject obj)
    {
        _syncer.AddObject(obj);
    }

    public void RemoveObject(DatabaseObject obj)
    {
        _syncer.RemoveObject(obj);
    }

    internal void WatchPropertyChange(DatabaseObject obj)
    {
        obj.PropertyChanging += OnDatabaseObjectPropertyChanging;
        obj.PropertyChanged += OnDatabaseObjectPropertyChanged;
    }

    internal void UnwatchPropertyChange(DatabaseObject obj)
    {
        obj.PropertyChanging -= OnDatabaseObjectPropertyChanging;
        obj.PropertyChanged -= OnDatabaseObjectPropertyChanged;
    }

    void OnDatabaseObjectPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        DatabaseObject obj = (DatabaseObject)sender!;
        FieldInfo? fieldInfo = obj.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            _tempChange = new PropertyChangeInfo
            {
                PropertyName = fieldInfo.FieldName,
                OldValue = _rule.CastTo(fieldInfo.FieldType, obj.GetFieldValue(fieldInfo.FieldName)),
            };
        }
    }

    void OnDatabaseObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DatabaseObject obj = (DatabaseObject)sender!;
        FieldInfo? fieldInfo = obj.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            _tempChange!.NewValue = _rule.CastTo(fieldInfo.FieldType, obj.GetFieldValue(fieldInfo.FieldName));
            var op = new ModifyDatabaseObjectOperation(_workspace, obj, _tempChange);
            _workspace.UndoStack.AddOperation(op);
            _tempChange = null;
        }
    }

    void OnDatabaseExecutingSql(string sql)
    {
        _workspace.LogOptional($"Executing '{sql}'");
    }

    SqliteDatabase _db;
    SqliteSyncer _syncer;
    SerializationRule _rule = SerializationRule.Default;

    PropertyChangeInfo? _tempChange;
}