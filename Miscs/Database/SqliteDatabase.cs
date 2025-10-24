using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Fantania.Models;
using Microsoft.Data.Sqlite;

namespace Fantania;

public class SqliteDatabase : IDisposable
{
    public SqliteDatabase(string path)
    {
        _conn = new SqliteConnection($"Data Source={path}");
    }

    ~SqliteDatabase()
    {
        Dispose();
    }

    public IReadOnlyList<DatabaseObject> ObjectsOfGroup(string group)
    {
        return _groupedObjects[group];
    }

    public IReadOnlyList<IPlacement> ObjectsOfType(string type)
    {
        return _typedObjects[type];
    }

    public void AddObject(DatabaseObject obj, Workspace workspace)
    {
        _groupedObjects[obj.Group].Add(obj);
        _typedObjects[obj.GetType().Name].Add(obj);
        obj.OnInitialized(workspace);
    }

    public void RemoveObject(DatabaseObject obj, Workspace workspace)
    {
        obj.OnUnintialized(workspace);
        _groupedObjects[obj.Group].RemoveFast(obj);
        _typedObjects[obj.GetType().Name].RemoveFast(obj);
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
        var command = _conn.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<string> ExecuteScalar(string sql)
    {
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

    SqliteConnection _conn;

    internal Dictionary<string, ObservableCollection<DatabaseObject>> _groupedObjects = new Dictionary<string, ObservableCollection<DatabaseObject>>();
    internal Dictionary<string, ObservableCollection<IPlacement>> _typedObjects = new Dictionary<string, ObservableCollection<IPlacement>>();
}