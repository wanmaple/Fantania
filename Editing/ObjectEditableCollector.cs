using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class EditablePropertyInfo
{
    public PropertyInfo propertyInfo;
    public EditAttribute editInfo;
}

public static class ObjectEditableCollector
{
    public static ObservableCollection<IEditableProperty> CollectEditableProperties(ObservableObject obj)
    {
        var ret = new ObservableCollection<IEditableProperty>();
        if (!_cache.TryGetValue(obj.GetType(), out var infolist))
        {
            infolist = new List<EditablePropertyInfo>(0);
            _cache.Add(obj.GetType(), infolist);
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var disableAttr = obj.GetType().GetCustomAttribute<DisableEditingAttribute>();
            var disabledProps = new HashSet<string>();
            if (disableAttr != null)
            {
                foreach (var disabledProp in disableAttr.DisabledPropertyNames)
                {
                    disabledProps.Add(disabledProp);
                }
            }
            foreach (PropertyInfo propInfo in properties)
            {
                EditAttribute editInfo = propInfo.GetCustomAttribute<EditAttribute>();
                if (editInfo != null)
                {
                    if (disabledProps.Contains(propInfo.Name))
                        continue;
                    infolist.Add(new EditablePropertyInfo
                    {
                        propertyInfo = propInfo,
                        editInfo = editInfo,
                    });
                    TooltipAttribute tooltipInfo = propInfo.GetCustomAttribute<TooltipAttribute>();
                    var property = new EditableProperty(obj, propInfo, editInfo, tooltipInfo == null ? string.Empty : string.Format(Localization.Resources.ResourceManager.GetString(tooltipInfo.TextKey, Localization.Resources.Culture) ?? string.Empty, tooltipInfo.Arguments));
                    ret.Add(property);
                }
            }
        }
        else
        {
            foreach (var info in infolist)
            {
                PropertyInfo propInfo = info.propertyInfo;
                EditAttribute editInfo = info.editInfo;
                TooltipAttribute tooltipInfo = propInfo.GetCustomAttribute<TooltipAttribute>();
                var property = new EditableProperty(obj, propInfo, editInfo, tooltipInfo == null ? string.Empty : string.Format(Localization.Resources.ResourceManager.GetString(tooltipInfo.TextKey, Localization.Resources.Culture) ?? string.Empty, tooltipInfo.Arguments));
                ret.Add(property);
            }
        }
        ret = new ObservableCollection<IEditableProperty>(ret.OrderBy(prop => prop.PropertyName));
        return ret;
    }

    public static ObservableCollection<IEditableProperty> CollectShareableEditableProperties(IEnumerable<ObservableObject> objects, IEqualityComparer<IEditableProperty> equaliter = null)
    {
        if (equaliter == null)
            equaliter = PropertyInfoValueEqualiter.Instance;
        var ret = new ObservableCollection<IEditableProperty>();
        HashSet<IEditableProperty> shareables = new HashSet<IEditableProperty>(equaliter);
        bool first = true;
        var toRm = new List<IEditableProperty>();
        foreach (ObservableObject obj in objects)
        {
            var props = CollectEditableProperties(obj);
            if (first)
            {
                foreach (var prop in props)
                {
                    shareables.Add(prop);
                }
                first = false;
            }
            else
            {
                toRm.Clear();
                foreach (var prop in shareables)
                {
                    bool contains = false;
                    foreach (var myProp in props)
                    {
                        if (equaliter.Equals(myProp, prop))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                    {
                        toRm.Add(prop);
                    }
                }
                foreach (var rm in toRm)
                {
                    shareables.Remove(rm);
                }
                if (shareables.Count == 0)
                    break;
            }
        }
        foreach (var prop in shareables)
        {
            ret.Add(new ShareableEditableProperty(prop as EditableProperty, objects));
        }
        return ret;
    }

    public static ObservableCollection<IEditableProperty> CollectTemporaryEditableProperties(object obj)
    {
        var ret = new ObservableCollection<IEditableProperty>();
        if (!_cache.TryGetValue(obj.GetType(), out var infolist))
        {
            infolist = new List<EditablePropertyInfo>(0);
            _cache.Add(obj.GetType(), infolist);
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var disableAttr = obj.GetType().GetCustomAttribute<DisableEditingAttribute>();
            var disabledProps = new HashSet<string>();
            if (disableAttr != null)
            {
                foreach (var disabledProp in disableAttr.DisabledPropertyNames)
                {
                    disabledProps.Add(disabledProp);
                }
            }
            foreach (PropertyInfo propInfo in properties)
            {
                EditAttribute editInfo = propInfo.GetCustomAttribute<EditAttribute>();
                if (editInfo != null)
                {
                    if (disabledProps.Contains(propInfo.Name))
                        continue;
                    infolist.Add(new EditablePropertyInfo
                    {
                        propertyInfo = propInfo,
                        editInfo = editInfo,
                    });
                    // TooltipAttribute tooltipInfo = propInfo.GetCustomAttribute<TooltipAttribute>();
                    var property = new TempEditableProperty(obj, propInfo, editInfo);
                    ret.Add(property);
                }
            }
        }
        else
        {
            foreach (var info in infolist)
            {
                PropertyInfo propInfo = info.propertyInfo;
                EditAttribute editInfo = info.editInfo;
                // TooltipAttribute tooltipInfo = propInfo.GetCustomAttribute<TooltipAttribute>();
                var property = new TempEditableProperty(obj, propInfo, editInfo);
                ret.Add(property);
            }
        }
        ret = new ObservableCollection<IEditableProperty>(ret.OrderBy(prop => prop.PropertyName));
        return ret;
    }

    static Dictionary<Type, List<EditablePropertyInfo>> _cache = new Dictionary<Type, List<EditablePropertyInfo>>(128);
}