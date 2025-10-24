using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Fantania;

public class PropertyInfoValueEqualiter : IEqualityComparer<IEditableProperty>
{
    public static readonly PropertyInfoValueEqualiter Instance = new PropertyInfoValueEqualiter();

    PropertyInfoValueEqualiter() { }

    public bool Equals(IEditableProperty? x, IEditableProperty? y)
    {
        if (!x.PropertyInfo.Equals(y.PropertyInfo))
            return false;
        if (!IsValueEqual(x.EditValue, y.EditValue))
            return false;
        return true;
    }

    public int GetHashCode([DisallowNull] IEditableProperty obj)
    {
        int hash = obj.PropertyInfo.GetHashCode();
        hash = (hash * 397) ^ ValueHashCode(obj.EditValue);
        return hash;
    }

    bool IsValueEqual(object val1, object val2)
    {
        if (val1.GetType() != val2.GetType())
            return false;
        var type = val1.GetType();
        if (type is IEnumerable)
        {
            var collection1 = val1 as IEnumerable;
            var collection2 = val2 as IEnumerable;
            string str1 = string.Join(';', collection1);
            string str2 = string.Join(';', collection2);
            if (str1 != str2)
                return false;
        }
        else
        {
            if (!val1.Equals(val2))
                return false;
        }
        return true;
    }

    int ValueHashCode(object value)
    {
        int hash = 0;
        if (value is IEnumerable collection)
        {
            foreach (object item in collection)
            {
                hash = (hash * 397) ^ item.GetHashCode();
            }
        }
        else
            hash = value.GetHashCode();
        return hash;
    }
}