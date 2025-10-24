using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class DatabaseObjectSync
{
    private class ReferenceInfo
    {
        public object Object { get; set; }
        public PropertyInfo Property { get; set; }
        public Type DatabaseType { get; set; }
        public string CommandText { get; set; }
    }

    public static string DatabaseValue2CommandText(Type dbPropType, object val, bool addQuote = true)
    {
        if (dbPropType == typeof(DatabaseIntegerAttribute))
        {
            return ((int)val).ToString();
        }
        else if (dbPropType == typeof(DatabaseRealAttribute))
        {
            return ((double)val).ToString();
        }
        else if (dbPropType == typeof(DatabaseBooleanAttribute))
        {
            return ((bool)val) ? "1" : "0";
        }
        else if (dbPropType == typeof(DatabaseStringAttribute))
        {
            return addQuote ? $"'{val.ToString()}'" : val.ToString();
        }
        else if (dbPropType == typeof(DatabaseVector2Attribute))
        {
            Vector vec = (Vector)val;
            return addQuote ? $"'{vec.X},{vec.Y}'" : $"{vec.X},{vec.Y}";
        }
        else if (dbPropType == typeof(DatabaseVector3Attribute))
        {
            System.Numerics.Vector3 vec = (System.Numerics.Vector3)val;
            return addQuote ? $"'{vec.X},{vec.Y},{vec.Z}'" : $"{vec.X},{vec.Y},{vec.Z}";
        }
        else if (dbPropType == typeof(DatabaseVector4Attribute))
        {
            System.Numerics.Vector4 vec = (System.Numerics.Vector4)val;
            return addQuote ? $"'{vec.X},{vec.Y},{vec.Z},{vec.W}'" : $"{vec.X},{vec.Y},{vec.Z},{vec.W}";
        }
        else if (dbPropType == typeof(DatabaseCurveAttribute))
        {
            HermitCurve curve = (HermitCurve)val;
            var ary = new List<string>();
            foreach (HermitPivot pivot in curve.Pivots)
            {
                ary.Add($"{pivot.Index},{pivot.Value},{pivot.Tangent}");
            }
            return addQuote ? $"'{string.Join(';', ary)}'" : string.Join(';', ary);
        }
        else if (dbPropType == typeof(DatabaseGradient1DAttribute))
        {
            Gradient1D gradient = (Gradient1D)val;
            var ary = new List<string>();
            for (int i = 0; i < Gradient1D.SEGMENTS; i++)
            {
                if (gradient.HasStop(i))
                {
                    var color = gradient.ColorAt(i);
                    ary.Add($"{i},{color.X},{color.Y},{color.Z},{color.W}");
                }
            }
            return addQuote ? $"'{string.Join(';', ary)}'" : string.Join(';', ary);
        }
        else if (dbPropType == typeof(DatabaseGradient2DAttribute))
        {
            Gradient2D gradient = (Gradient2D)val;
            var ary = new List<string>();
            ary.Add($"{(int)gradient.Shape}");
            ary.Add($"{gradient.BeginAnchor.X},{gradient.BeginAnchor.Y}");
            ary.Add($"{gradient.EndAnchor.X},{gradient.EndAnchor.Y}");
            for (int i = 0; i < Gradient1D.SEGMENTS; i++)
            {
                if (gradient.Gradient.HasStop(i))
                {
                    var color = gradient.Gradient.ColorAt(i);
                    ary.Add($"{i},{color.X},{color.Y},{color.Z},{color.W}");
                }
            }
            return addQuote ? $"'{string.Join(';', ary)}'" : string.Join(';', ary);
        }
        else if (dbPropType == typeof(DatabaseNoise2DAttribute))
        {
            Noise2DLite noise = (Noise2DLite)val;
            var ary = new List<string>();
            ary.Add($"{(int)noise.NoiseType},{noise.Seed},{noise.Octaves},{noise.Frequency},{(noise.Inverted ? 1 : 0)},{(noise.Repeat ? 1 : 0)}");
            switch (noise.NoiseType)
            {
                case NoiseTypes.Cellular:
                    var argsCellular = noise.NoiseArguments as CellularNoiseParameters;
                    ary.Add($"{argsCellular.JitterModifier},{(int)argsCellular.DistanceFunction},{(int)argsCellular.ReturnType}");
                    break;
                case NoiseTypes.Perlin:
                    var argsPerlin = noise.NoiseArguments as PerlinNoiseParameters;
                    ary.Add($"{(int)argsPerlin.SmoothFunction}");
                    break;
                case NoiseTypes.Simplex:
                    var argsSimplex = noise.NoiseArguments as SimplexNoiseParameters;
                    ary.Add($"{(int)argsSimplex.Algorithm}");
                    break;
                case NoiseTypes.Value:
                    var argsValue = noise.NoiseArguments as ValueNoiseParameters;
                    ary.Add($"{(int)argsValue.Algorithm}");
                    break;
            }
            return addQuote ? $"'{string.Join(';', ary)}'" : string.Join(';', ary);
        }
        else if (dbPropType == typeof(DatabaseCurvedEdgeAttribute))
        {
            CurvedEdge edge = (CurvedEdge)val;
            var ary = new List<string>();
            string curveText = DatabaseValue2CommandText(typeof(DatabaseCurveAttribute), edge.Curve, false);
            ary.Add(curveText);
            ary.Add($"{edge.CurveAmplitude},{edge.NoiseSeed},{edge.NoiseAmplitude}");
            return addQuote ? $"'{string.Join('|', ary)}'" : string.Join('|', ary);
        }
        else if (dbPropType == typeof(DatabaseGroupReferenceAttribute))
        {
            DatabaseObject obj = val as DatabaseObject;
            int id = obj == null ? 0 : obj.ID;
            return addQuote ? $"'{obj.Group},{id}'" : $"{obj.Group},{id}";
        }
        else if (dbPropType == typeof(DatabaseTypeReferenceAttribute))
        {
            DatabaseObject obj = val as DatabaseObject;
            int id = obj == null ? 0 : obj.ID;
            return addQuote ? $"'{obj.GetType().Name},{id}'" : $"{obj.GetType().Name},{id}";
        }
        throw new ArgumentException($"dbPropType '{dbPropType.Name}' is not supported.");
    }

    public static object CommandText2DatabaseValue(Type dbPropType, string text, bool unescapeQuote = false)
    {
        if (dbPropType == typeof(DatabaseIntegerAttribute))
        {
            return int.Parse(text);
        }
        else if (dbPropType == typeof(DatabaseRealAttribute))
        {
            return double.Parse(text);
        }
        else if (dbPropType == typeof(DatabaseBooleanAttribute))
        {
            return int.Parse(text) == 1 ? true : false;
        }
        else if (dbPropType == typeof(DatabaseStringAttribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            return text;
        }
        else if (dbPropType == typeof(DatabaseVector2Attribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary = text.Split(',');
            return new Vector(double.Parse(ary[0]), double.Parse(ary[1]));
        }
        else if (dbPropType == typeof(DatabaseVector3Attribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary = text.Split(',');
            return new System.Numerics.Vector3(float.Parse(ary[0]), float.Parse(ary[1]), float.Parse(ary[2]));
        }
        else if (dbPropType == typeof(DatabaseVector4Attribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary = text.Split(',');
            return new System.Numerics.Vector4(float.Parse(ary[0]), float.Parse(ary[1]), float.Parse(ary[2]), float.Parse(ary[3]));
        }
        else if (dbPropType == typeof(DatabaseCurveAttribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary = text.Split(';');
            HermitCurve curve = new HermitCurve();
            for (int i = 0; i < ary.Length; i++)
            {
                var ary2 = ary[i].Split(',');
                int idx = int.Parse(ary2[0]);
                float val = float.Parse(ary2[1]);
                float tan = float.Parse(ary2[2]);
                if (idx == 0 || idx == HermitCurve.SEGMENTS - 1)
                {
                    curve.UpdatePivot(idx, val, tan);
                }
                else
                {
                    curve.AddPivot(idx, val, tan);
                }
            }
            return curve;
        }
        else if (dbPropType == typeof(DatabaseGradient1DAttribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary = text.Split(';');
            var gradient = new Gradient1D();
            for (int i = 0; i < ary.Length; i++)
            {
                var ary2 = ary[i].Split(',');
                int idx = int.Parse(ary2[0]);
                float r = float.Parse(ary2[1]);
                float g = float.Parse(ary2[2]);
                float b = float.Parse(ary2[3]);
                float a = float.Parse(ary2[4]);
                var color = new System.Numerics.Vector4(r, g, b, a);
                if (idx == 0 || idx == HermitCurve.SEGMENTS - 1)
                {
                    gradient.UpdateColor(idx, color);
                }
                else
                {
                    gradient.InsertColor(idx, color);
                }
            }
            return gradient;
        }
        else if (dbPropType == typeof(DatabaseGradient2DAttribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary = text.Split(';');
            GradientShapes shape = (GradientShapes)int.Parse(ary[0]);
            var beginAnchorAry = ary[1].Split(',');
            var endAnchorAry = ary[2].Split(',');
            System.Numerics.Vector2 beginAnchor = new System.Numerics.Vector2(float.Parse(beginAnchorAry[0]), float.Parse(beginAnchorAry[1]));
            System.Numerics.Vector2 endAnchor = new System.Numerics.Vector2(float.Parse(endAnchorAry[0]), float.Parse(endAnchorAry[1]));
            var gradient = new Gradient1D();
            for (int i = 3; i < ary.Length; i++)
            {
                var ary2 = ary[i].Split(',');
                int idx = int.Parse(ary2[0]);
                float r = float.Parse(ary2[1]);
                float g = float.Parse(ary2[2]);
                float b = float.Parse(ary2[3]);
                float a = float.Parse(ary2[4]);
                var color = new System.Numerics.Vector4(r, g, b, a);
                if (idx == 0 || idx == HermitCurve.SEGMENTS - 1)
                {
                    gradient.UpdateColor(idx, color);
                }
                else
                {
                    gradient.InsertColor(idx, color);
                }
            }
            return new Gradient2D(gradient, beginAnchor, endAnchor)
            {
                Shape = shape,
            };
        }
        else if (dbPropType == typeof(DatabaseNoise2DAttribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary1 = text.Split(';');
            var ary2 = ary1[0].Split(',');
            NoiseTypes type = (NoiseTypes)int.Parse(ary2[0]);
            int seed = int.Parse(ary2[1]);
            int octaves = int.Parse(ary2[2]);
            double freq = double.Parse(ary2[3]);
            bool inverted = int.Parse(ary2[4]) != 0;
            bool repeat = int.Parse(ary2[5]) != 0;
            var noise = new Noise2DLite
            {
                Seed = seed,
                Octaves = octaves,
                Frequency = freq,
                Inverted = inverted,
                Repeat = repeat,
            };
            noise.NoiseType = type;
            var ary3 = ary1[1].Split(',');
            switch (type)
            {
                case NoiseTypes.Cellular:
                    var argsCellular = noise.NoiseArguments as CellularNoiseParameters;
                    argsCellular.JitterModifier = double.Parse(ary3[0]);
                    argsCellular.DistanceFunction = (NoiseHelper.CellularDistanceFunctions)int.Parse(ary3[1]);
                    argsCellular.ReturnType = (NoiseHelper.CellularReturnTypes)int.Parse(ary3[2]);
                    break;
                case NoiseTypes.Perlin:
                    var argsPerlin = noise.NoiseArguments as PerlinNoiseParameters;
                    argsPerlin.SmoothFunction = (NoiseHelper.PerlinSmoothFunctions)int.Parse(ary3[0]);
                    break;
                case NoiseTypes.Simplex:
                    var argsSimplex = noise.NoiseArguments as SimplexNoiseParameters;
                    argsSimplex.Algorithm = (NoiseHelper.SimplexAlgorithms)int.Parse(ary3[0]);
                    break;
                case NoiseTypes.Value:
                    var argsValue = noise.NoiseArguments as ValueNoiseParameters;
                    argsValue.Algorithm = (NoiseHelper.ValueAlgorithms)int.Parse(ary3[0]);
                    break;
            }
            return noise;
        }
        else if (dbPropType == typeof(DatabaseCurvedEdgeAttribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary1 = text.Split('|');
            HermitCurve curve = (HermitCurve)CommandText2DatabaseValue(typeof(DatabaseCurveAttribute), ary1[0], false);
            var ary2 = ary1[1].Split(',');
            int curveAmplitude = int.Parse(ary2[0]);
            int noiseSeed = int.Parse(ary2[1]);
            double noiseAmplitude = double.Parse(ary2[2]);
            return new CurvedEdge
            {
                Curve = curve,
                CurveAmplitude = curveAmplitude,
                NoiseSeed = noiseSeed,
                NoiseAmplitude = noiseAmplitude,
            };
        }
        else if (dbPropType == typeof(DatabaseGroupReferenceAttribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary = text.Split(',');
            string group = ary[0];
            int id = int.Parse(ary[1]);
            Workspace workspace = WorkspaceViewModel.Current.Workspace;
            return workspace.MainDatabase.ObjectsOfGroup(group).FirstOrDefault(obj => obj.ID == id);
        }
        else if (dbPropType == typeof(DatabaseTypeReferenceAttribute))
        {
            if (unescapeQuote)
            {
                text = text.Substring(1, text.Length - 2);
            }
            var ary = text.Split(',');
            string type = ary[0];
            int id = int.Parse(ary[1]);
            Workspace workspace = WorkspaceViewModel.Current.Workspace;
            return workspace.MainDatabase.ObjectsOfType(type).FirstOrDefault(obj => (obj as DatabaseObject).ID == id);
        }
        throw new ArgumentException($"dbPropType '{dbPropType.Name}' is not supported.");
    }

    public DatabaseObjectSync()
    {
    }

    public bool CheckModified()
    {
        return _added.Count > 0 || _removed.Count > 0 || _modified.Count > 0;
    }

    public void AddObject(Workspace workspace, DatabaseObject obj)
    {
        workspace.MainDatabase.AddObject(obj, workspace);
        if (!_removed.Remove(obj))
        {
            _added.Add(obj);
        }
    }

    public void RemoveObject(Workspace workspace, DatabaseObject obj)
    {
        workspace.MainDatabase.RemoveObject(obj, workspace);
        if (!_added.Remove(obj))
        {
            _removed.Add(obj);
        }
    }

    public async Task SyncFrom(Workspace workspace, IEnumerable<IGrouping<string, GroupedType>> groupings)
    {
        await workspace.MainDatabase.OpenAsync();
        await workspace.EditorDatabase.OpenAsync();
        foreach (var group in groupings)
        {
            var groupArray = new ObservableCollection<DatabaseObject>();
            workspace.MainDatabase._groupedObjects.Add(group.Key, groupArray);
            foreach (GroupedType type in group)
            {
                var typedArray = new ObservableCollection<IPlacement>();
                workspace.MainDatabase._typedObjects.Add(type.Type.Name, typedArray);
                string sql = $"SELECT * FROM {type.Type.Name}";
                await workspace.MainDatabase.ExecuteQuery(sql, (reader, columns) =>
                {
                    var obj = Activator.CreateInstance(type.Type) as DatabaseObject;
                    for (int i = 0; i < columns.Count; i++)
                    {
                        string propName = columns[i].ColumnName;
                        string propValue = reader.GetString(i);
                        if (propName == "ID")
                        {
                            obj._id = int.Parse(propValue);
                        }
                        else
                        {
                            PropertyInfo property = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
                            Type dbPropType = GetDatabasePropertyType(type.Type, propName);
                            if (dbPropType is DatabaseGroupReferenceAttribute || dbPropType is DatabaseTypeReferenceAttribute)
                            {
                                _refs.Add(new ReferenceInfo
                                {
                                    Object = obj,
                                    Property = property,
                                    DatabaseType = dbPropType,
                                    CommandText = propValue,
                                });
                                continue;
                            }
                            property.SetValue(obj, CommandText2DatabaseValue(dbPropType, propValue));
                        }
                    }
                    typedArray.Add(obj);
                    groupArray.Add(obj);
                });
                string queryTableName = type.Group + "Query";
                foreach (DatabaseObject obj in typedArray)
                {
                    sql = $"SELECT [Name],[Comment] FROM {queryTableName} WHERE ID = {obj.ID}";
                    await workspace.EditorDatabase.ExecuteQuery(sql, (reader, columns) =>
                    {
                        string name = reader.GetString(0);
                        string comment = reader.GetString(1);
                        obj.Name = name;
                        obj.Comment = comment;
                    });
                }
            }
        }
        await workspace.MainDatabase.CloseAsync();
        await workspace.EditorDatabase.CloseAsync();
    }

    public async Task SyncTo(Workspace workspace)
    {
        WorkspaceViewModel vm = WorkspaceViewModel.Current;
        List<Task> allTasks = new List<Task>();
        await workspace.MainDatabase.OpenAsync();
        await workspace.EditorDatabase.OpenAsync();
        // Remove first.
        foreach (DatabaseObject obj in _removed)
        {
            string groupTableName = obj.Group + "Group";
            string queryTableName = obj.Group + "Query";
            string tableName = obj.GetType().Name;
            string sql = $"DELETE FROM {groupTableName} WHERE ID = {obj.ID}";
            allTasks.Add(vm.LogOptional(string.Format(Localization.Resources.LogExecutingSql, sql)));
            allTasks.Add(workspace.MainDatabase.ExecuteSql(sql));
            sql = $"DELETE FROM {tableName} WHERE ID = {obj.ID}";
            allTasks.Add(vm.LogOptional(string.Format(Localization.Resources.LogExecutingSql, sql)));
            allTasks.Add(workspace.MainDatabase.ExecuteSql(sql));
            sql = $"DELETE FROM {queryTableName} WHERE ID = {obj.ID}";
            allTasks.Add(vm.LogOptional(string.Format(Localization.Resources.LogExecutingSql, sql)));
            allTasks.Add(workspace.EditorDatabase.ExecuteSql(sql));
        }
        var sb = new StringBuilder();
        // Alter next.
        foreach (DatabaseObject obj in _modified.Keys)
        {
            // the object might be deleted.
            if (!workspace.MainDatabase.ObjectsOfType(obj.GetType().Name).Contains(obj))
                continue;
            var changes = _modified[obj].Where(info => info.PropertyName != "Name" && info.PropertyName != "Comment").ToArray();
            if (changes.Length > 0)
            {
                sb.Clear();
                sb.Append("UPDATE ").Append(obj.GetType().Name).Append(" SET ");
                for (int i = 0; i < changes.Length; i++)
                {
                    PropertyChangeInfo change = changes[i];
                    sb.Append(change.PropertyName).Append(" = ").Append(change.NewValue);
                    if (i != changes.Length - 1)
                        sb.Append(',');
                }
                sb.Append(" WHERE ID = ").Append(obj.ID);
                string sql = sb.ToString();
                allTasks.Add(vm.LogOptional(string.Format(Localization.Resources.LogExecutingSql, sql)));
                allTasks.Add(workspace.MainDatabase.ExecuteSql(sql));
            }

            changes = _modified[obj].Where(info => info.PropertyName == "Name" || info.
            PropertyName == "Comment").ToArray();
            if (changes.Length > 0)
            {
                sb.Clear();
                sb.Append("UPDATE ").Append(obj.Group + "Query").Append(" SET ");
                for (int i = 0; i < changes.Length; i++)
                {
                    PropertyChangeInfo change = changes[i];
                    sb.Append(change.PropertyName).Append(" = ").Append(change.NewValue);
                    if (i != changes.Length - 1)
                        sb.Append(',');
                }
                sb.Append(" WHERE ID = ").Append(obj.ID);
                string sql = sb.ToString();
                allTasks.Add(vm.LogOptional(string.Format(Localization.Resources.LogExecutingSql, sql)));
                allTasks.Add(workspace.EditorDatabase.ExecuteSql(sql));
            }
        }
        // Add last.
        var kvs = new List<KeyValuePair<string, object>>();
        foreach (DatabaseObject obj in _added)
        {
            string groupTableName = obj.Group + "Group";
            string queryTableName = obj.Group + "Query";
            string tableName = obj.GetType().Name;
            var dict = GetDatabaseProperties(obj.GetType());
            kvs.Clear();
            foreach (string propName in dict.Keys)
            {
                object val = obj.GetType().GetProperty(propName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(obj);
                kvs.Add(new KeyValuePair<string, object>(propName, val));
            }
            sb.Clear();
            sb.Append("INSERT INTO ").Append(tableName).Append("(ID,");
            for (int i = 0; i < kvs.Count; i++)
            {
                sb.Append(kvs[i].Key);
                if (i != kvs.Count - 1)
                    sb.Append(',');
            }
            sb.Append(") VALUES(").Append(obj.ID).Append(',');
            for (int i = 0; i < kvs.Count; i++)
            {
                string cmdText = DatabaseValue2CommandText(GetDatabasePropertyType(obj.GetType(), kvs[i].Key), kvs[i].Value);
                sb.Append(cmdText);
                if (i != kvs.Count - 1)
                    sb.Append(',');
            }
            sb.Append(")");
            string sql = sb.ToString();
            allTasks.Add(vm.LogOptional(string.Format(Localization.Resources.LogExecutingSql, sql)));
            allTasks.Add(workspace.MainDatabase.ExecuteSql(sql));
            sql = $"INSERT INTO {groupTableName}(ID, Type) VALUES({obj.ID}, '{tableName}')";
            allTasks.Add(vm.LogOptional(string.Format(Localization.Resources.LogExecutingSql, sql)));
            allTasks.Add(workspace.MainDatabase.ExecuteSql(sql));
            sql = $"INSERT INTO {queryTableName}(ID, Name, Comment) VALUES({obj.ID}, '{obj.Name}', '{obj.Comment}')";
            allTasks.Add(vm.LogOptional(string.Format(Localization.Resources.LogExecutingSql, sql)));
            allTasks.Add(workspace.EditorDatabase.ExecuteSql(sql));
        }
        foreach (Task task in allTasks)
        {
            await task;
        }
        await workspace.MainDatabase.CloseAsync();
        await workspace.EditorDatabase.CloseAsync();
        foreach (DatabaseObject obj in _added)
        {
            WatchPropertyChanged(obj);
        }
        _added.Clear();
        _removed.Clear();
        _modified.Clear();
    }

    public Type GetDatabasePropertyType(Type objType, string propName)
    {
        if (propName == "Name" || propName == "Comment")
            return typeof(DatabaseStringAttribute);
        if (!_cache.TryGetValue(objType, out var props))
        {
            props = ReflectionHelper.GetDatabaseProperties(objType);
            _cache.Add(objType, props);
        }
        return props[propName].DatabaseType;
    }

    public bool IsDatabasePropertyType(Type objType, string propName)
    {
        if (propName == "Name" || propName == "Comment")
            return true;
        if (!_cache.TryGetValue(objType, out var props))
        {
            props = ReflectionHelper.GetDatabaseProperties(objType);
            _cache.Add(objType, props);
        }
        return props.ContainsKey(propName);
    }

    public void LateInit(Workspace workspace)
    {
        foreach (var refInfo in _refs)
        {
            refInfo.Property.SetValue(refInfo.Object, CommandText2DatabaseValue(refInfo.DatabaseType, refInfo.CommandText));
        }
        _refs.Clear();
        foreach (var pair in workspace.MainDatabase._groupedObjects)
        {
            foreach (var obj in pair.Value)
            {
                WatchPropertyChanged(obj);
            }
        }
    }

    void OnDatabaseObjectPropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        DatabaseObject obj = sender as DatabaseObject;
        if (!IsDatabasePropertyType(obj.GetType(), e.PropertyName)) return;
        if (!_modified.TryGetValue(obj, out var changes))
        {
            changes = new ObservableCollection<PropertyChangeInfo>();
            _modified.Add(obj, changes);
        }
        if (!changes.Any(info => info.PropertyName == e.PropertyName))
        {
            changes.Add(new PropertyChangeInfo
            {
                PropertyName = e.PropertyName,
                OldValue = DatabaseValue2CommandText(GetDatabasePropertyType(obj.GetType(), e.PropertyName), obj.GetType().GetProperty(e.PropertyName, BindingFlags.Instance | BindingFlags.Public).GetValue(obj)),
            });
        }
        WorkspaceViewModel.Current.Workspace.CheckModified();
    }

    void OnDatabaseObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DatabaseObject obj = sender as DatabaseObject;
        if (!IsDatabasePropertyType(obj.GetType(), e.PropertyName)) return;
        if (_modified.TryGetValue(obj, out var changes))
        {
            var changeInfo = changes.FirstOrDefault(info => info.PropertyName == e.PropertyName);
            if (changeInfo != null)
            {
                string newValue = DatabaseValue2CommandText(GetDatabasePropertyType(obj.GetType(), e.PropertyName), obj.GetType().GetProperty(e.PropertyName, BindingFlags.Instance | BindingFlags.Public).GetValue(obj));
                if (newValue == changeInfo.OldValue as string)
                {
                    changes.Remove(changeInfo);
                    if (changes.Count <= 0)
                    {
                        _modified.Remove(obj);
                    }
                }
                else
                {
                    changeInfo.NewValue = newValue;
                }
            }
        }
        WorkspaceViewModel.Current.Workspace.CheckModified();
    }

    void WatchPropertyChanged(DatabaseObject obj)
    {
        obj.PropertyChanging += OnDatabaseObjectPropertyChanging;
        obj.PropertyChanged += OnDatabaseObjectPropertyChanged;
    }

    void UnwatchPropertyChanged(DatabaseObject obj)
    {
        obj.PropertyChanging -= OnDatabaseObjectPropertyChanging;
        obj.PropertyChanged -= OnDatabaseObjectPropertyChanged;
    }

    IReadOnlyDictionary<string, DatabasePropertyInfo> GetDatabaseProperties(Type objType)
    {
        if (!_cache.TryGetValue(objType, out var props))
        {
            props = ReflectionHelper.GetDatabaseProperties(objType);
            _cache.Add(objType, props);
        }
        return props;
    }

    HashSet<DatabaseObject> _added = new HashSet<DatabaseObject>(0);
    HashSet<DatabaseObject> _removed = new HashSet<DatabaseObject>(0);
    Dictionary<DatabaseObject, IList<PropertyChangeInfo>> _modified = new Dictionary<DatabaseObject, IList<PropertyChangeInfo>>(0);

    List<ReferenceInfo> _refs = new List<ReferenceInfo>();
    Dictionary<Type, IReadOnlyDictionary<string, DatabasePropertyInfo>> _cache = new Dictionary<Type, IReadOnlyDictionary<string, DatabasePropertyInfo>>(20);
}