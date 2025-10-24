using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fantania.Models;

namespace Fantania;

public static class SerializationHelper
{
    public static IReadOnlyList<SerializablePropertyInfo> GetSerializableProperties(Type objType)
    {
        if (!_cache.TryGetValue(objType, out var list))
        {
            List<SerializablePropertyInfo> proplist = new List<SerializablePropertyInfo>(16);
            foreach (var propInfo in objType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var attr = propInfo.GetCustomAttribute<CustomSerializationAttribute>();
                if (attr != null)
                {
                    int version = attr.Version;
                    var propSerializer = PROPERTY_SERIALIZERS[(attr.GetType(), version)];
                    proplist.Add(new SerializablePropertyInfo
                    {
                        Version = version,
                        PropertyInfo = propInfo,
                        PropertySerializer = propSerializer,
                    });
                }
            }
            proplist.Sort((lhs, rhs) =>
            {
                return lhs.PropertyInfo.Name.CompareTo(rhs.PropertyInfo.Name);
            });
            list = proplist;
            _cache.Add(objType, list);
        }
        return list;
    }

    public static object StoreSerializableProperty(PropertyInfo propInfo, object value)
    {
        Type propType = propInfo.PropertyType;
        if (!propType.IsClass)
        {
            return value;
        }
        else if (propType == typeof(string))
        {
            return value;
        }
        else if (propType.IsAssignableTo(typeof(DatabaseObject)))
        {
            DatabaseObject dbObj = value as DatabaseObject;
            return (dbObj.ID, dbObj.GetType().Name);
        }
        throw new NotImplementedException("The property type is not implemented.");
    }

    public static object TakeSerializableProperty(PropertyInfo propInfo, object value, Workspace workspace)
    {
        Type propType = propInfo.PropertyType;
        if (!propType.IsClass)
        {
            return value;
        }
        else if (propType == typeof(string))
        {
            return value;
        }
        else if (propType.IsAssignableTo(typeof(DatabaseObject)))
        {
            (int, string) dbInfo = (ValueTuple<int, string>)value;
            return workspace.MainDatabase.ObjectsOfType(dbInfo.Item2).First(obj => (obj as DatabaseObject).ID == dbInfo.Item1);
        }
        throw new NotImplementedException("The property type is not implemented.");
    }

    static Dictionary<Type, IReadOnlyList<SerializablePropertyInfo>> _cache = new Dictionary<Type, IReadOnlyList<SerializablePropertyInfo>>(16);

    static readonly Dictionary<(Type, int), IPropertySerializer> PROPERTY_SERIALIZERS = new Dictionary<(Type, int), IPropertySerializer>
    {
        { (typeof(StandardSerializationAttribute), 1), new StandardPropertySerializerV1() },
    };
}