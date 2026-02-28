using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace FantaniaLib;

public class SqliteDatabase : IDisposable
{
    public event Action<string>? ExecutingNonQuery;

    public SqliteDatabase(string dbPath)
    {
        _conn = new SqliteConnection($"Data Source={dbPath}");
    }

    ~SqliteDatabase()
    {
        Dispose();
    }

    public IReadOnlyList<DatabaseObject> ObjectsOfGroup(string group)
    {
        if (_groupedObjs.TryGetValue(group, out var objs))
        {
            return objs;
        }
        return Array.Empty<DatabaseObject>();
    }

    public IReadOnlyList<DatabaseObject> ObjectsOfType(string type)
    {
        if (_typedObjs.TryGetValue(type, out var objs))
        {
            return objs;
        }
        return Array.Empty<DatabaseObject>();
    }

    public void AddObject(DatabaseObject obj)
    {
        ObservableCollection<DatabaseObject>? objs;
        if (!string.IsNullOrEmpty(obj.GroupName))
        {
            if (!_groupedObjs.TryGetValue(obj.GroupName, out objs))
            {
                objs = new ObservableCollection<DatabaseObject>();
                objs.Add(EmptyDatabaseObject.Instance);
                _groupedObjs.Add(obj.GroupName, objs);
            }
            objs.Add(obj);
        }
        if (!_typedObjs.TryGetValue(obj.TypeName, out objs))
        {
            objs = new ObservableCollection<DatabaseObject>();
            objs.Add(EmptyDatabaseObject.Instance);
            _typedObjs.Add(obj.TypeName, objs);
        }
        objs.Add(obj);
    }

    public void RemoveObject(DatabaseObject obj)
    {
        if (!string.IsNullOrEmpty(obj.GroupName))
        {
            _groupedObjs[obj.GroupName].RemoveFast(obj);
        }
        _typedObjs[obj.TypeName].RemoveFast(obj);
    }

    public async Task OpenAsync()
    {
        if (_conn != null)
        {
            await _conn.OpenAsync();
        }
    }

    public async Task CloseAsync()
    {
        if (_conn != null)
        {
            await _conn.CloseAsync();
        }
    }

    public async Task ExecuteSql(string sql)
    {
        if (_conn == null) return;
        var command = _conn.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        ExecutingNonQuery?.Invoke(sql);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<string?> ExecuteScalar(string sql)
    {
        if (_conn == null) return null;
        var command = _conn.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        using (var reader = await command.ExecuteReaderAsync())
        {
            if (reader.Read())
            {
                return reader.GetString(0);
            }
        }
        return null;
    }

    public async Task ExecuteQuery(string sql, Action<SqliteDataReader, ReadOnlyCollection<DbColumn>> action)
    {
        if (_conn == null) return;
        var command = _conn.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        using (var reader = await command.ExecuteReaderAsync())
        {
            var columns = await reader.GetColumnSchemaAsync();
            while (reader.Read())
            {
                action.Invoke(reader, columns);
            }
        }
    }

    public void Dispose()
    {
        if (_conn != null)
        {
            _conn.Dispose();
            _conn = null;
        }
    }

    Dictionary<string, ObservableCollection<DatabaseObject>> _groupedObjs = new Dictionary<string, ObservableCollection<DatabaseObject>>(0);
    Dictionary<string, ObservableCollection<DatabaseObject>> _typedObjs = new Dictionary<string, ObservableCollection<DatabaseObject>>(0);

    SqliteConnection? _conn;
}