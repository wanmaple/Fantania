using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace FantaniaLib;

public class SqliteSyncer
{
    public bool HasChange => _added.Count > 0 || _removed.Count > 0 || _modified.Count > 0;

    public SqliteSyncer(SqliteDatabase db, SerializationRule rule)
    {
        _db = db;
        _rule = rule;
    }

    public async Task SyncFromDatabase(IReadOnlyList<PlacementTemplate> placements, IReadOnlyList<Type> commonTypes)
    {
        await _db.OpenAsync();
        var kvs = new List<KeyValuePair<string, string>>(16);
        foreach (PlacementTemplate template in placements)
        {
            if (!await CheckTableExists(template.ClassName)) continue;
            await _db.ExecuteQuery(SqlQueryTable(template.ClassName), (reader, cols) =>
            {
                kvs.Clear();
                for (int i = 0; i < cols.Count; i++)
                {
                    string propName = cols[i].ColumnName;
                    string propVal = reader.GetString(i);
                    kvs.Add(new KeyValuePair<string, string>(propName, propVal));
                }
                int id = int.Parse(kvs.First(pair => pair.Key == "ID")!.Value);
                var placement = new UserPlacement(template, id);
                var fields = placement.SerializableFields;
                foreach (var kv in kvs)
                {
                    if (kv.Key == "ID") continue;
                    FieldInfo? field = fields.FirstOrDefault(f => f.FieldName == kv.Key);
                    if (field == null) continue;
                    placement.SetFieldValue(kv.Key, _rule.CastFrom(field.FieldType, kv.Value, placement));
                }
                WatchPropertyChange(placement);
                _db.AddObject(placement);
            });
        }
        await _db.CloseAsync();
    }

    public async Task SyncToDatabase()
    {
        await _db.OpenAsync();
        // delete rows at first.
        foreach (var obj in _removed)
        {
            if (!string.IsNullOrEmpty(obj.GroupName))
            {
                string groupTableName = obj.GroupName + "Group";
                await _db.ExecuteSql(SqlDeleteRow(groupTableName, obj.ID));
            }
            string tableName = obj.TypeName;
            await _db.ExecuteSql(SqlDeleteRow(tableName, obj.ID));
        }
        // alter rows next.
        foreach (DatabaseObject obj in _modified.Keys)
        {
            // might be deleted.
            if (!_db.ObjectsOfType(obj.TypeName).Contains(obj)) continue;
            await SyncTableScheme(obj.TypeName, obj.SerializableFields);
            var changes = _modified[obj];
            await _db.ExecuteSql(SqlUpdateRow(obj.TypeName, obj.ID, changes, obj.SerializableFields));
        }
        // insert rows at last.
        var kvs = new List<KeyValuePair<string, object>>();
        foreach (DatabaseObject obj in _added)
        {
            if (!string.IsNullOrEmpty(obj.GroupName))
            {
                string groupTableName = obj.GroupName + "Group";
                FieldInfo[] fields = [
                    new FieldInfo { FieldName = "Type", FieldType = FieldTypes.String, }
                ];
                await SyncTableScheme(groupTableName, fields);
                kvs.Clear();
                kvs.Add(new KeyValuePair<string, object>("Type", obj.TypeName));
                await _db.ExecuteSql(SqlInsertRow(groupTableName, obj.ID, kvs, fields, obj));
            }
            await SyncTableScheme(obj.TypeName, obj.SerializableFields);
            kvs.Clear();
            foreach (var field in obj.SerializableFields)
            {
                kvs.Add(new KeyValuePair<string, object>(field.FieldName, obj.GetFieldValue(field.FieldName)!));
            }
            await _db.ExecuteSql(SqlInsertRow(obj.TypeName, obj.ID, kvs, obj.SerializableFields, obj));
        }
        await _db.CloseAsync();
        foreach (var obj in _added)
        {
            WatchPropertyChange(obj);
        }
        foreach (var obj in _removed)
        {
            UnwatchPropertyChange(obj);
        }
        _added.Clear();
        _removed.Clear();
        _modified.Clear();
    }

    public void AddObject(DatabaseObject obj)
    {
        _db.AddObject(obj);
        if (!_removed.Remove(obj))
        {
            _added.Add(obj);
        }
    }

    public void RemoveObject(DatabaseObject obj)
    {
        _db.RemoveObject(obj);
        if (!_added.Remove(obj))
        {
            _removed.Add(obj);
        }
    }

    async Task<bool> CheckTableExists(string tableName)
    {
        string sql = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        string? text = await _db.ExecuteScalar(sql);
        int cnt = int.Parse(text!);
        return cnt > 0;
    }

    async Task SyncTableScheme(string tableName, IReadOnlyList<FieldInfo> fields)
    {
        if (!await CheckTableExists(tableName))
        {
            await _db.ExecuteSql(SqlCreateTable(tableName, fields));
        }
        else
        {
            foreach (FieldInfo field in fields)
            {
                string sql = $"SELECT 1 FROM PRAGMA_TABLE_INFO('{tableName}') WHERE name='{field.FieldName}'";
                string? text = await _db.ExecuteScalar(sql);
                if (string.IsNullOrEmpty(text))
                {
                    await _db.ExecuteSql(SqlAddColumnOfTable(tableName, field));
                }
            }
        }
    }

    string SqlCreateTable(string tableName, IReadOnlyList<FieldInfo> fields)
    {
        var sb = new StringBuilder();
        sb.Append("CREATE TABLE IF NOT EXISTS ");
        sb.Append(tableName);
        sb.Append("(");
        sb.Append("ID INTEGER PRIMARY KEY");
        foreach (FieldInfo field in fields)
        {
            sb.Append(',');
            sb.Append(field.FieldName);
            sb.Append(' ');
            sb.Append(SqliteTypeFromFieldType(field.FieldType));
            sb.Append(" DEFAULT ");
            sb.Append(DefaultValueFromFieldType(field.FieldType));
        }
        sb.Append(")");
        return sb.ToString();
    }

    string SqlAddColumnOfTable(string tableName, FieldInfo field)
    {
        return $"ALTER TABLE {tableName} ADD COLUMN {field.FieldName} {SqliteTypeFromFieldType(field.FieldType)} DEFAULT {DefaultValueFromFieldType(field.FieldType)}";
    }

    string SqlQueryTable(string tableName)
    {
        return $"SELECT * FROM {tableName}";
    }

    string SqlInsertRow(string tableName, int id, IReadOnlyList<KeyValuePair<string, object>> kvs, IReadOnlyList<FieldInfo> fields, object instance)
    {
        var sb = new StringBuilder();
        sb.Append("INSERT INTO ").Append(tableName).Append("(ID,");
        for (int i = 0; i < kvs.Count; i++)
        {
            sb.Append(kvs[i].Key);
            if (i != kvs.Count - 1)
                sb.Append(',');
        }
        sb.Append(") VALUES(").Append(id).Append(',');
        for (int i = 0; i < kvs.Count; i++)
        {
            FieldInfo field = fields.First(f => f.FieldName == kvs[i].Key);
            sb.Append(EscapeIfRequire(_rule.CastTo(field.FieldType, kvs[i].Value, instance), field));
            if (i != kvs.Count - 1)
                sb.Append(',');
        }
        sb.Append(')');
        return sb.ToString();
    }

    string SqlDeleteRow(string tableName, int id)
    {
        return $"DELETE FROM {tableName} WHERE ID = {id}";
    }

    string SqlUpdateRow(string tableName, int id, IList<PropertyChangeInfo> changes, IReadOnlyList<FieldInfo> fields)
    {
        var sb = new StringBuilder();
        sb.Append("UPDATE ").Append(tableName).Append(" SET ");
        for (int i = 0; i < changes.Count; i++)
        {
            PropertyChangeInfo change = changes[i];
            FieldInfo field = fields.First(f => f.FieldName == change.PropertyName);
            sb.Append(change.PropertyName).Append(" = ").Append(EscapeIfRequire(change.NewValue, field));
            if (i != changes.Count - 1)
                sb.Append(',');
        }
        sb.Append(" WHERE ID = ").Append(id);
        return sb.ToString();
    }

    string EscapeIfRequire(string content, FieldInfo field)
    {
        switch (field.FieldType)
        {
            case FieldTypes.Boolean:
            case FieldTypes.Integer:
            case FieldTypes.Float:
                return content;
            default:
                return $"'{content}'";
        }
    }

    string SqliteTypeFromFieldType(FieldTypes fieldType)
    {
        switch (fieldType)
        {
            case FieldTypes.Boolean:
            case FieldTypes.Integer:
                return "INTEGER";
            case FieldTypes.Float:
                return "REAL";
            default:
                return "TEXT";
        }
    }

    string DefaultValueFromFieldType(FieldTypes fieldType)
    {
        switch (fieldType)
        {
            case FieldTypes.Boolean:
            case FieldTypes.Integer:
                return "0";
            case FieldTypes.Float:
                return "0.0";
            case FieldTypes.Vector2:
                return "'0,0'";
            case FieldTypes.Color:
                return "'#FFFFFFFF'";
            default:
                return "''";
        }
    }

    void WatchPropertyChange(DatabaseObject obj)
    {
        obj.PropertyChanging += OnObjectPropertyChanging;
        obj.PropertyChanged += OnObjectPropertyChanged;
    }

    void UnwatchPropertyChange(DatabaseObject obj)
    {
        obj.PropertyChanging -= OnObjectPropertyChanging;
        obj.PropertyChanged -= OnObjectPropertyChanged;
    }

    void OnObjectPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        DatabaseObject obj = (DatabaseObject)sender!;
        FieldInfo? fieldInfo = obj.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            if (!_modified.TryGetValue(obj, out var changes))
            {
                changes = new ObservableCollection<PropertyChangeInfo>();
                _modified.Add(obj, changes);
            }
            if (!changes.Any(change => change.PropertyName == e.PropertyName))
            {
                changes.Add(new PropertyChangeInfo
                {
                    PropertyName = fieldInfo.FieldName,
                    OldValue = _rule.CastTo(fieldInfo.FieldType, obj.GetFieldValue(fieldInfo.FieldName), obj),
                });
            }
        }
    }

    void OnObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DatabaseObject obj = (DatabaseObject)sender!;
        FieldInfo? fieldInfo = obj.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            IList<PropertyChangeInfo> changes = _modified[obj];
            var change = changes.First(c => c.PropertyName == e.PropertyName);
            string newVal = _rule.CastTo(fieldInfo.FieldType, obj.GetFieldValue(fieldInfo.FieldName), obj);
            if (newVal == change.OldValue)
            {
                changes.RemoveFast(change);
                if (changes.Count <= 0)
                    _modified.Remove(obj);
            }
            else
            {
                change.NewValue = newVal;
            }
        }
    }

    HashSet<DatabaseObject> _added = new HashSet<DatabaseObject>(0);
    HashSet<DatabaseObject> _removed = new HashSet<DatabaseObject>(0);
    Dictionary<DatabaseObject, IList<PropertyChangeInfo>> _modified = new Dictionary<DatabaseObject, IList<PropertyChangeInfo>>(0);

    SqliteDatabase _db;
    SerializationRule _rule;
}