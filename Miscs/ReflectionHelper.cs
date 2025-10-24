using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fantania;

public static class ReflectionHelper
{
    public static IEnumerable<IGrouping<string, GroupedType>> GetAllGroupedInfo(Assembly asm)
    {
        var allGrouped = new List<GroupedType>();
        var allTypes = asm.GetTypes();
        foreach (Type type in allTypes)
        {
            if (type == typeof(DatabaseObject) || !type.IsAssignableTo(typeof(DatabaseObject)) || type.IsAbstract) continue;
            IgnoreDatabaseAttribute ignore = type.GetCustomAttribute<IgnoreDatabaseAttribute>();
            if (ignore != null) continue;
            DataGroupAttribute attr = type.GetCustomAttribute<DataGroupAttribute>();
            if (attr == null)
                throw new DatabaseException(type, $"Forgot to setup DataGroup to Type {type.FullName}.");
            var group = new GroupedType
            {
                Type = type,
                Group = attr.Group,
            };
            allGrouped.Add(group);
        }
        return allGrouped.GroupBy(item => item.Group);
    }

    public static IReadOnlyDictionary<string, DatabasePropertyInfo> GetDatabaseProperties(Type type)
    {
        if (!type.IsAssignableTo(typeof(DatabaseObject))) return null;
        var ret = new Dictionary<string, DatabasePropertyInfo>();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (PropertyInfo propInfo in properties)
        {
            DatabaseAttribute attr = propInfo.GetCustomAttribute<DatabaseAttribute>();
            if (attr == null) continue;
            ret.Add(propInfo.Name, new DatabasePropertyInfo
            {
                Name = propInfo.Name,
                DatabaseType = attr.GetType(),
            });
        }
        return ret;
    }

    public static TableConfig GenerateTableConfig(Type type)
    {
        string typename = type.Name;
        var ret = new TableConfig(typename);
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (PropertyInfo propInfo in properties)
        {
            DatabaseAttribute attr = propInfo.GetCustomAttribute<DatabaseAttribute>();
            if (attr == null) continue;
            if (IsArrayType(propInfo.PropertyType))
                ret.Add(propInfo.Name, ColumnType.Array);
            else
                ret.Add(propInfo.Name, DATABASE_TYPE_TO_COLUMN_TYPE[attr.GetType()]);
        }
        return ret;
    }

    public static T GetPropertyAttribute<T>(Type type, string propName) where T : Attribute
    {
        var propInfo = type.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
        if (propInfo == null) return null;
        var attr = propInfo.GetCustomAttribute<T>();
        return attr;
    }

    public static bool IsArrayType(Type type)
    {
        return type != typeof(string) && type.IsAssignableTo(typeof(IEnumerable));
    }

    private static readonly Dictionary<Type, ColumnType> DATABASE_TYPE_TO_COLUMN_TYPE = new Dictionary<Type, ColumnType>
    {
        { typeof(DatabaseBooleanAttribute), ColumnType.Boolean },
        { typeof(DatabaseIntegerAttribute), ColumnType.Integer },
        { typeof(DatabaseRealAttribute), ColumnType.Real },
        { typeof(DatabaseStringAttribute), ColumnType.String },
        { typeof(DatabaseVector2Attribute), ColumnType.Vector2 },
        { typeof(DatabaseVector3Attribute), ColumnType.Vector3 },
        { typeof(DatabaseVector4Attribute), ColumnType.Vector4 },
        { typeof(DatabaseCurveAttribute), ColumnType.Curve },
        { typeof(DatabaseGradient1DAttribute), ColumnType.Gradient1D },
        { typeof(DatabaseGradient2DAttribute), ColumnType.Gradient2D },
        { typeof(DatabaseNoise2DAttribute), ColumnType.Noise2D },
        { typeof(DatabaseCurvedEdgeAttribute), ColumnType.CurvedEdge },
    };
}