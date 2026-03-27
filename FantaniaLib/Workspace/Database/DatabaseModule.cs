using System.ComponentModel;

namespace FantaniaLib;

public class DatabaseModule : WorkspaceModule
{
    public const string DATABASE_FILENAME = "main.db";

    public IList<string> GameDataGroups => _gamedataTemplateGroupMap.Keys.ToList();
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
        await _syncer.SyncFromDatabase(_workspace.PlacementModule.PlacementTemplateMap.Values.ToArray(), _gamedataTemplateMap.Values.ToArray());
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

    public void AddGameDataTemplate(GameDataTemplate template)
    {
        _gamedataTemplateMap.Add(template.ClassName, template);
        if (!_gamedataTemplateGroupMap.TryGetValue(template.DataGroup, out var list))
        {
            list = new List<GameDataTemplate>(4);
            _gamedataTemplateGroupMap.Add(template.DataGroup, list);
        }
        list.Add(template);
    }

    public IReadOnlyList<GameDataTemplate> GetGameDataTemplatesOfGroup(string group)
    {
        if (_gamedataTemplateGroupMap.TryGetValue(group, out var list))
            return list;
        return Array.Empty<GameDataTemplate>();
    }

    public UserGameData AddUserGameData(string templateType)
    {
        GameDataTemplate template = _gamedataTemplateMap[templateType];
        var groupObjs = GetObjectsOfGroup(template.DataGroup);
        int newId = groupObjs.Count <= 0 ? 1 : groupObjs.Max(d => d.ID) + 1;
        var data = new UserGameData(template, newId);
        AddObject(data);
        WatchPropertyChange(data);
        _workspace.UndoStack.AddOperation(new NewDatabaseObjectOperation(_workspace, data));
        return data;
    }

    public void RemoveUserGameData(UserGameData data)
    {
        RemoveObject(data);
        UnwatchPropertyChange(data);
        _workspace.UndoStack.AddOperation(new DelDatabaseObjectOperation(_workspace, data));
    }

    public IReadOnlyList<DatabaseObject> GetObjectsReferenced(DatabaseObject data)
    {
        var ret = new List<DatabaseObject>();
        foreach (var obj in _db.AllObjects)
        {
            var fields = obj.SerializableFields;
            foreach (var field in fields)
            {
                if (field.FieldType == FieldTypes.GroupReference)
                {
                    GroupReference groupRef = (GroupReference)obj.GetFieldValue(field.FieldName)!;
                    if (groupRef.ReferenceGroup == data.GroupName && groupRef.ReferenceID == data.ID)
                        ret.Add(obj);
                }
                else if (field.FieldType == FieldTypes.TypeReference)
                {
                    TypeReference typeRef = (TypeReference)obj.GetFieldValue(field.FieldName)!;
                    if (typeRef.ReferenceType == data.TypeName && typeRef.ReferenceID == data.ID)
                        ret.Add(obj);
                }
                else if (field.FieldType == FieldTypes.GroupReferenceArray)
                {
                    FantaniaArray<GroupReference> groupRefArray = (FantaniaArray<GroupReference>)obj.GetFieldValue(field.FieldName)!;
                    if (groupRefArray.Any(r => r.ReferenceGroup == data.GroupName && r.ReferenceID == data.ID))
                        ret.Add(obj);
                }
                else if (field.FieldType == FieldTypes.TypeReferenceArray)
                {
                    FantaniaArray<TypeReference> typeRefArray = (FantaniaArray<TypeReference>)obj.GetFieldValue(field.FieldName)!;
                    if (typeRefArray.Any(r => r.ReferenceType == data.TypeName && r.ReferenceID == data.ID))
                        ret.Add(obj);
                }
            }
        }
        return ret;
    }

    public void FixObjectsReferenced(IReadOnlyList<DatabaseObject> objs, DatabaseObject refered)
    {
        foreach (var obj in objs)
        {
            var fields = obj.SerializableFields;
            foreach (var field in fields)
            {
                if (field.FieldType == FieldTypes.GroupReference)
                {
                    GroupReference groupRef = (GroupReference)obj.GetFieldValue(field.FieldName)!;
                    if (groupRef.ReferenceGroup == refered.GroupName && groupRef.ReferenceID == refered.ID)
                        obj.SetFieldValue(field.FieldName, new GroupReference
                        {
                            ReferenceGroup = refered.GroupName,
                            ReferenceID = 0,
                        });
                }
                else if (field.FieldType == FieldTypes.TypeReference)
                {
                    TypeReference typeRef = (TypeReference)obj.GetFieldValue(field.FieldName)!;
                    if (typeRef.ReferenceType == refered.TypeName && typeRef.ReferenceID == refered.ID)
                        obj.SetFieldValue(field.FieldName, new TypeReference
                        {
                            ReferenceType = refered.TypeName,
                            ReferenceID = 0,
                        });
                }
                else if (field.FieldType == FieldTypes.GroupReferenceArray)
                {
                    FantaniaArray<GroupReference> groupRefArray = (FantaniaArray<GroupReference>)obj.GetFieldValue(field.FieldName)!;
                    for (int i = 0; i < groupRefArray.Count; i++)
                    {
                        GroupReference r = groupRefArray[i];
                        if (r.ReferenceGroup == refered.GroupName && r.ReferenceID == refered.ID)
                            groupRefArray[i] = new GroupReference
                            {
                                ReferenceGroup = refered.GroupName,
                                ReferenceID = 0,
                            };
                    }
                }
                else if (field.FieldType == FieldTypes.TypeReferenceArray)
                {
                    FantaniaArray<TypeReference> typeRefArray = (FantaniaArray<TypeReference>)obj.GetFieldValue(field.FieldName)!;
                    for (int i = 0; i < typeRefArray.Count; i++)
                    {
                        TypeReference r = typeRefArray[i];
                        if (r.ReferenceType == refered.TypeName && r.ReferenceID == refered.ID)
                            typeRefArray[i] = new TypeReference
                            {
                                ReferenceType = refered.TypeName,
                                ReferenceID = 0,
                            };
                    }
                }
            }
        }
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
                OldValue = _rule.CastTo(fieldInfo.FieldType, obj.GetFieldValue(fieldInfo.FieldName), obj),
            };
        }
    }

    void OnDatabaseObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DatabaseObject obj = (DatabaseObject)sender!;
        FieldInfo? fieldInfo = obj.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            _tempChange!.NewValue = _rule.CastTo(fieldInfo.FieldType, obj.GetFieldValue(fieldInfo.FieldName), obj);
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
    Dictionary<string, GameDataTemplate> _gamedataTemplateMap = new Dictionary<string, GameDataTemplate>(64);
    Dictionary<string, List<GameDataTemplate>> _gamedataTemplateGroupMap = new Dictionary<string, List<GameDataTemplate>>(16);

    PropertyChangeInfo? _tempChange;
}