using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantania;

public abstract class DatabaseSync()
{
    protected string CommandCreateTableIfNotExists(TableConfig config)
    {
        var sb = new StringBuilder();
        sb.Append("CREATE TABLE IF NOT EXISTS ");
        sb.Append(config.Name);
        sb.Append("(");
        sb.Append("ID INTEGER PRIMARY KEY");
        foreach (string columnName in config.ColumnNames)
        {
            sb.Append(',');
            sb.Append(columnName);
            sb.Append(' ');
            sb.Append(config.GetSqliteType(columnName));
            sb.Append(" DEFAULT ");
            sb.Append(config.GetDefault(columnName));
        }
        sb.Append(")");
        string sql = sb.ToString();
        return sql;
    }

    protected string CommandAddColumnToTable(string table, string columnName, ColumnType type)
    {
        string sql = $"ALTER TABLE {table} ADD COLUMN {columnName} {TableConfig.ColumnTypeToSqliteType(type)} DEFAULT {TableConfig.ColumnType2DefaultValue(type)}";
        return sql;
    }

    protected string CommandRenameColumnOfTable(string table, string oldName, string newName)
    {
        string sql = $"ALTER TABLE {table} RENAME COLUMN {oldName} TO {newName}";
        return sql;
    }
}

public class MainDatabaseSync : DatabaseSync
{
    public async Task SyncDatabase(SqliteDatabase database, IGrouping<string, GroupedType> group)
    {
        string groupName = group.Key;
        string groupTableName = groupName + "Group";
        TableConfig groupTableCfg = new TableConfig(groupTableName);
        groupTableCfg.Add("Type", ColumnType.String);
        string sql = CommandCreateTableIfNotExists(groupTableCfg);
        await database.ExecuteSql(sql);
        foreach (GroupedType type in group)
        {
            Type objType = type.Type;
            TableConfig config = ReflectionHelper.GenerateTableConfig(objType);
            sql = CommandCreateTableIfNotExists(config);
            await database.ExecuteSql(sql);
            // check if there are new columns to add.
            foreach (string columnName in config.ColumnNames)
            {
                sql = $"SELECT 1 FROM PRAGMA_TABLE_INFO('{config.Name}') WHERE name='{columnName}'";
                string result = await database.ExecuteScalar(sql);
                if (string.IsNullOrEmpty(result))
                {
                    // check if need to rename
                    RenameColumnAttribute attr = ReflectionHelper.GetPropertyAttribute<RenameColumnAttribute>(objType, columnName);
                    if (attr != null)
                    {
                        sql = CommandRenameColumnOfTable(config.Name, attr.OldName, columnName);
                        await database.ExecuteSql(sql);
                    }
                    else
                    {
                        sql = CommandAddColumnToTable(config.Name, columnName, config[columnName]);
                        await database.ExecuteSql(sql);
                    }
                }
            }
        }
    }
}

public class EditorDatabaseSync : DatabaseSync
{
    public async Task SyncDatabase(SqliteDatabase database, IGrouping<string, GroupedType> group)
    {
        string groupName = group.Key;
        string tableName = groupName + "Query";
        string sql = $"CREATE TABLE IF NOT EXISTS {tableName} (ID INTEGER PRIMARY KEY, Name TEXT NOT NULL UNIQUE, Comment TEXT NOT NULL)";
        await database.ExecuteSql(sql);
    }
}