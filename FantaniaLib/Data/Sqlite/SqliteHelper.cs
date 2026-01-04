using Microsoft.Data.Sqlite;

namespace FantaniaLib;

public static class SqliteHelper
{
    public static async Task<SqliteConnection> CreateDatabase(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
        var conn = new SqliteConnection($"Data Source={path}");
        await conn.OpenAsync();
        return conn;
    }

    public static async Task<SqliteConnection> OpenDatabase(string path)
    {
        if (!File.Exists(path))
            return null;
        var conn = new SqliteConnection($"Data Source={path}");
        await conn.OpenAsync();
        return conn;
    }
}