using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FantaniaLib;

public class BinaryDataSyncer<T> : IDisposable where T : BinaryObject
{
    private struct SerializationClassInfo
    {
        public required string ClassName;
        public required IReadOnlyList<FieldInfo> Fields;
    }

    public const int SERIALIZATION_VERSION = 1;

    public bool HasChange => _added.Count > 0 || _removed.Count > 0 || _modified.Count > 0;

    public BinaryDataSyncer(IList<T> source, SerializationRule rule)
    {
        _source = source;
        _rule = rule;
    }

    public async Task SyncFromFile(string path)
    {
        await Task.Run(async () =>
        {
            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs);
            int ver = br.ReadInt32();
            if (ver < SERIALIZATION_VERSION)
            {
                // upgrade data logic
            }
            else
            {
                BinaryHeaderV1 header = new BinaryHeaderV1();
                header.Version = ver;
                header.SerializedTypeCount = br.ReadInt32();
                header.SerializedEntityCount = br.ReadInt32();
                header.TypeSegmentOffset = br.ReadInt32();
                header.TypeSegmentSize = br.ReadInt32();
                header.EntitySegmentOffset = br.ReadInt32();
                header.EntitySegmentSize = br.ReadInt32();
                var serializations = new Dictionary<string, SerializationClassInfo>(header.SerializedTypeCount);
                for (int i = 0; i < header.SerializedTypeCount; i++)
                {
                    string className = br.ReadString();
                    int fieldNum = br.ReadInt32();
                    var fields = new FieldInfo[fieldNum];
                    var clsInfo = new SerializationClassInfo
                    {
                        ClassName = className,
                        Fields = fields,
                    };
                    for (int j = 0; j < fieldNum; j++)
                    {
                        string fieldName = br.ReadString();
                        FieldTypes fieldType = (FieldTypes)br.ReadInt32();
                        var fieldInfo = new FieldInfo
                        {
                            FieldName = fieldName,
                            FieldType = fieldType,
                        };
                        fields[j] = fieldInfo;
                    }
                    serializations.Add(className, clsInfo);
                }
                for (int i = 0; i < header.SerializedEntityCount; i++)
                {
                    string clsName = br.ReadString();
                    var clsInfo = serializations[clsName];
                    Type? type = Type.GetType(clsName);
                    if (type == null)
                    {
                        // 类名被改了，这个实体不加载，但是数据要接着读下去
                        foreach (var _ in clsInfo.Fields)
                        {
                            br.ReadString();
                        }
                    }
                    else
                    {
                        T obj = (T)Activator.CreateInstance(type, true)!;
                        foreach (var field in clsInfo.Fields)
                        {
                            string data = br.ReadString();
                            object? val = _rule.CastFrom(field.FieldType, data);
                            obj.SetFieldValue(field.FieldName, val);
                        }
                        _source.Add(obj);
                        WatchPropertyChange(obj);
                    }
                }
            }
            await fs.FlushAsync();
        });
    }

    public async Task SyncToFile(string path)
    {
        await Task.Run(async () =>
        {
            using var fs = File.OpenWrite(path);
            using var bw = new BinaryWriter(fs);
            BinaryHeaderV1 header = new BinaryHeaderV1();
            header.Version = SERIALIZATION_VERSION;
            var serializations = CollectSerializationClassInfo(_source);
            header.SerializedTypeCount = serializations.Count;
            header.SerializedEntityCount = _source.Count;
            header.TypeSegmentOffset = Marshal.SizeOf<BinaryHeaderV1>();
            fs.Seek(header.TypeSegmentOffset, SeekOrigin.Begin);
            foreach (var pair in serializations)
            {
                var ser = pair.Value;
                bw.Write(ser.ClassName);
                bw.Write(ser.Fields.Count);
                foreach (var field in ser.Fields)
                {
                    bw.Write(field.FieldName);
                    bw.Write((int)field.FieldType);
                }
            }
            int sizeOfTypes = (int)(fs.Position - header.TypeSegmentOffset);
            header.TypeSegmentSize = sizeOfTypes;
            header.EntitySegmentOffset = header.TypeSegmentOffset + header.TypeSegmentSize;
            foreach (var item in _source)
            {
                bw.Write(item.ClassName);
                var serInfo = serializations[item.ClassName];
                foreach (var field in serInfo.Fields)
                {
                    var val = item.GetFieldValue(field.FieldName);
                    string toWrite = _rule.CastTo(field.FieldType, val);
                    bw.Write(toWrite);
                }
            }
            header.EntitySegmentSize = (int)(fs.Position - header.EntitySegmentOffset);
            fs.Seek(0, SeekOrigin.Begin);
            bw.Write(header.Version);
            bw.Write(header.SerializedTypeCount);
            bw.Write(header.SerializedEntityCount);
            bw.Write(header.TypeSegmentOffset);
            bw.Write(header.TypeSegmentSize);
            bw.Write(header.EntitySegmentOffset);
            bw.Write(header.EntitySegmentSize);
            await fs.FlushAsync();
            foreach (var obj in _removed)
            {
                UnwatchPropertyChange(obj);
            }
            foreach (var obj in _added)
            {
                WatchPropertyChange(obj);
            }
            _added.Clear();
            _removed.Clear();
            _modified.Clear();
        });
    }

    public void AddObject(T obj)
    {
        _source.Add(obj);
        if (!_removed.Remove(obj))
        {
            _added.Add(obj);
        }
    }

    public void RemoveObject(T obj)
    {
        _source.RemoveFast(obj);
        if (!_added.Remove(obj))
        {
            _removed.Add(obj);
        }
    }

    IReadOnlyDictionary<string, SerializationClassInfo> CollectSerializationClassInfo(IEnumerable<T> objs)
    {
        var ret = new Dictionary<string, SerializationClassInfo>(64);
        var groups = objs.GroupBy(obj => obj.ClassName);
        foreach (var group in groups)
        {
            var obj = group.First();
            var classInfo = new SerializationClassInfo
            {
                ClassName = group.Key,
                Fields = obj.SerializableFields,
            };
            ret.Add(group.Key, classInfo);
        }
        return ret;
    }

    void WatchPropertyChange(T obj)
    {
        obj.PropertyChanging += OnObjectPropertyChanging;
        obj.PropertyChanged += OnObjectPropertyChanged;
    }

    void UnwatchPropertyChange(T obj)
    {
        obj.PropertyChanging -= OnObjectPropertyChanging;
        obj.PropertyChanged -= OnObjectPropertyChanged;
    }

    void OnObjectPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        T obj = (T)sender!;
        FieldInfo? fieldInfo = obj.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            if (!_modified.TryGetValue(obj, out var changes))
            {
                changes = new ObservableCollection<PropertyChangeInfo>();
                _modified.Add(obj, changes);
            }
            if (!changes.Any(change => change.PropertyName == e.PropertyName))
            {
                changes.Add(new PropertyChangeInfo
                {
                    PropertyName = fieldInfo.FieldName,
                    OldValue = _rule.CastTo(fieldInfo.FieldType, obj.GetFieldValue(fieldInfo.FieldName)),
                });
            }
        }
    }

    void OnObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        T obj = (T)sender!;
        FieldInfo? fieldInfo = obj.SerializableFields.FirstOrDefault(f => f.FieldName == e.PropertyName);
        if (fieldInfo != null)
        {
            IList<PropertyChangeInfo> changes = _modified[obj];
            var change = changes.First(c => c.PropertyName == e.PropertyName);
            string newVal = _rule.CastTo(fieldInfo.FieldType, obj.GetFieldValue(fieldInfo.FieldName));
            if (newVal == change.OldValue)
            {
                changes.RemoveFast(change);
                if (changes.Count <= 0)
                    _modified.Remove(obj);
            }
            else
            {
                change.NewValue = newVal;
            }
        }
    }

    public void Dispose()
    {
        foreach (var obj in _source)
        {
            UnwatchPropertyChange(obj);
        }
    }

    HashSet<T> _added = new HashSet<T>(0);
    HashSet<T> _removed = new HashSet<T>(0);
    Dictionary<T, IList<PropertyChangeInfo>> _modified = new Dictionary<T, IList<PropertyChangeInfo>>(0);

    IList<T> _source;
    SerializationRule _rule;
}