using System.Collections.Generic;

namespace Fantania;

public enum ColumnType
{
    Integer,
    Real,
    String,
    Boolean,
    Vector2,
    Vector3,
    Vector4,
    Curve,
    Gradient1D,
    Gradient2D,
    Noise2D,
    CurvedEdge,
    GroupReference,
    TypeReference,
    Array,
    Blob,
}

public class TableConfig
{
    public static string ColumnTypeToSqliteType(ColumnType columnType)
    {
        switch (columnType)
        {
            case ColumnType.Integer:
            case ColumnType.Boolean:
                return "INTEGER";
            case ColumnType.Real:
                return "REAL";
            case ColumnType.Blob:
                return "BLOB";
            default:
                return "TEXT";
        }
    }

    public static string ColumnType2DefaultValue(ColumnType columnType)
    {
        switch (columnType)
        {
            case ColumnType.Integer:
            case ColumnType.Boolean:
                return "0";
            case ColumnType.Real:
                return "0.0";
            case ColumnType.Vector2:
                return "\"0,0\"";
            case ColumnType.Vector3:
                return "\"0,0,0\"";
            case ColumnType.Vector4:
                return "\"0,0,0,0\"";
            case ColumnType.Curve:
                return $"\"0,1,0;{HermitCurve.SEGMENTS - 1},1,0\"";
            case ColumnType.Gradient1D:
                return $"\"0,0,0,0,1;{Gradient1D.SEGMENTS - 1},1,1,1,1\"";
            case ColumnType.Gradient2D:
                return $"\"0;0.5,0;0.5,1;0,0,0,0,1;{Gradient1D.SEGMENTS - 1},1,1,1,1\"";
            case ColumnType.Noise2D:
                return $"\"0,0,1,0.05,0,0;1.0,0,0\"";
            case ColumnType.CurvedEdge:
                return $"\"0,1,0;{HermitCurve.SEGMENTS - 1},1,0|0,0,0\"";
            case ColumnType.GroupReference:
            case ColumnType.TypeReference:
                return "0";
            case ColumnType.Array:
            default:
                return "\"\"";
        }
    }

    public string Name { get; set; }
    public Dictionary<string, ColumnType>.KeyCollection ColumnNames => _columns.Keys;

    public ColumnType this[string key]
    {
        get => _columns[key];
        set => _columns[key] = value;
    }

    public string GetSqliteType(string key)
    {
        ColumnType columnType = this[key];
        return ColumnTypeToSqliteType(columnType);
    }

    public string GetDefault(string key)
    {
        ColumnType columnType = this[key];
        return ColumnType2DefaultValue(columnType);
    }

    public TableConfig(string tableName)
    {
        Name = tableName;
    }

    public void Add(string columnName, ColumnType columnType)
    {
        _columns.Add(columnName, columnType);
    }

    Dictionary<string, ColumnType> _columns = new Dictionary<string, ColumnType>(16);
}