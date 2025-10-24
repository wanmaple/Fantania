using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public abstract class DatabaseObject : ObservableObject, IPlacement
{
    public int ID => _id;

    private string _name = string.Empty;
    [EditName, Tooltip("TooltipName")]
    public string Name
    {
        get { return _name; }
        set
        {
            if (_name != value)
            {
                OnPropertyChanging(nameof(Name));
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    private string _comment = string.Empty;
    [EditString, Tooltip("TooltipComment")]
    public string Comment
    {
        get { return _comment; }
        set
        {
            if (_comment != value)
            {
                OnPropertyChanging(nameof(Comment));
                _comment = value;
                OnPropertyChanged(nameof(Comment));
                OnPropertyChanged(nameof(Tooltip));
            }
        }
    }

    public string Group => _group;
    public virtual string IconPath => null;
    public string Tooltip => Comment;
    public virtual ObservableCollection<IPlacement> Children => null;
    public virtual Type GroupType => null;

    protected DatabaseObject()
    {
        Type myType = GetType();
        IgnoreDatabaseAttribute ignore = myType.GetCustomAttribute<IgnoreDatabaseAttribute>();
        if (ignore == null)
        {
            DataGroupAttribute? attr = myType.GetCustomAttribute<DataGroupAttribute>();
            if (attr == null)
                throw new DatabaseException(myType, $"Forgot to setup DataGroup to Type {myType.FullName}.");
            _group = attr.Group;
        }
    }

    public virtual void OnInitialized(Workspace workspace)
    {
        if (this is IRequireBackgroundJob job && job.ShouldQueueJob(workspace))
        {
            workspace.JobManager.AddJob(job);
        }    
    }
    public virtual void OnUnintialized(Workspace workspace)
    {
        if (this is IRequireBackgroundJob job)
        {
            job.OnDestroy(workspace);
            job.JobDirty = false;
        }
    }

    public virtual DatabaseObject Clone()
    {
        Type myType = GetType();
        var ret = Activator.CreateInstance(myType) as DatabaseObject;
        ret._id = WorkspaceViewModel.Current.GenerateID(Group);
        ret.Name = WorkspaceViewModel.Current.GenerateClonedName(Group, Name);
        var props = ReflectionHelper.GetDatabaseProperties(myType);
        foreach (var pair in props)
        {
            string propName = pair.Key;
            PropertyInfo propInfo = myType.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
            var propVal = propInfo.GetValue(this);
            if (propVal != null && ReflectionHelper.IsArrayType(propInfo.PropertyType))
            {
                var clonedArray = Activator.CreateInstance(propVal.GetType());
                var indexedProp = propVal.GetType().GetProperty("Item");
                IEnumerable ary = propVal as IEnumerable;
                int idx = 0;
                foreach (var item in ary)
                {
                    indexedProp.SetValue(clonedArray, indexedProp.GetValue(ary, [idx,]));
                    ++idx;
                }
                propVal = clonedArray;
            }
            propInfo.SetValue(ret, propVal);
        }
        return ret;
    }

    protected override void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        base.OnPropertyChanging(e);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
    }

    protected string GenerateUniqueName()
    {
        return $"{GetType().Name.ToLower()}_{ID}";
    }

    public override string ToString()
    {
        return ID.ToString();
    }

    string _group;

    internal int _id = -1;
}