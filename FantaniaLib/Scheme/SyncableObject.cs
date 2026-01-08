using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class SyncableObject : ObservableObject
{
    protected IReadOnlyList<PropertyInfo> GetPropertiesWithAttribute<T>() where T : Attribute
    {
        var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var ret = new List<PropertyInfo>(props.Length);
        foreach (PropertyInfo prop in props)
        {
            var attr = prop.GetCustomAttribute<T>();
            if (attr != null)
            {
                ret.Add(prop);
            }
        }
        return ret;
    }
}