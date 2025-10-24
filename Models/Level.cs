using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Fantania.ViewModels;

namespace Fantania.Models;

// Godot z-index range is [-4096, 4096].
public enum RenderLayers
{
    Background = 4000,
    BelowCharacters = 1000,
    Characters = 0,
    BelowPlatform = -1000,
    Platform = -2000,
    InFrontOfPlatform = -3000,
    // Editor only
    Selection = -9000,
}

public class Level : ObservableObject
{
    public const uint SERILIZATION_MARKER = 0xF748A593;

    public IReadOnlyList<string> LayerStrings => Enum.GetNames<RenderLayers>().Except(["Selection",]).ToArray();

    public event Action<LevelObject> ObjectAdded;
    public event Action<LevelObject> ObjectRemoved;
    public event Action RenderInitialized;

    public IEnumerable<LevelObject> AllObjects => _allObjects;
    public LevelEnvironment Environment => _envLv;
    public IEnumerable<LevelObject> SelectedObjects => _selections.Keys;
    public Rect Bounds => _bvh.Bounds;
    public LevelObject AddingObject => _addingObj;

    private string _name = string.Empty;
    public string Name => _name;

    private string _group = string.Empty;
    public string Group
    {
        get => _group;
        set => _group = value;
    }

    public Level(string name, string group)
    {
        _name = name;
        _group = group;
    }

    ~Level()
    {
    }

    public void OnInitialized()
    {
        RenderInitialized?.Invoke();
        _dirty = false;
    }

    public void OnDestroyed()
    {
        foreach (var obj in _allObjects.ToArray())
        {
            RemoveObject(obj);
        }
    }

    public async Task Load(Workspace workspace, string path)
    {
        await Task.Run(() =>
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    uint marker = br.ReadUInt32();
                    if (marker != SERILIZATION_MARKER)
                    {
                        return;
                    }
                    _group = br.ReadString();
                    int version = br.ReadInt32();
                    var serializer = new ObjectSerializer(version);
                    int objNum = br.ReadInt32();
                    for (int i = 0; i < objNum; i++)
                    {
                        LevelObject obj = serializer.Deserialize(br, workspace) as LevelObject;
                        AddObject(obj);
                    }
                }
            }
        });
    }

    public async Task Save(Workspace workspace)
    {
        const int version = 1;
        var serializer = new ObjectSerializer(version);
        await Task.Run(() =>
        {
            string path = Path.Combine(workspace.LevelsFolder, Name + ".bin");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(SERILIZATION_MARKER);
                    bw.Write(Group);
                    bw.Write(version);
                    bw.Write(_bvh.ItemCount);
                    foreach (var obj in _bvh.Enumerate())
                    {
                        serializer.Serialize(bw, obj);
                    }
                }
            }
        });
        _dirty = false;
    }

    public void AddObject(LevelObject obj, bool placed = true)
    {
        _allObjects.AddObject(obj);
        if (placed)
        {
            _bvh.AddItem(obj);
            obj.OnEnter(this);
        }
        obj.TransformChanged += OnObjectTransformChanged;
        obj.SelectStateChanged += OnObjectSelectStateChanged;
        ObjectAdded?.Invoke(obj);
    }

    public void RemoveObject(LevelObject obj)
    {
        if (_allObjects.RemoveObject(obj))
        {
            if (_bvh.RemoveItem(obj))
                obj.OnExit(this);
            obj.TransformChanged -= OnObjectTransformChanged;
            obj.SelectStateChanged -= OnObjectSelectStateChanged;
            ObjectRemoved?.Invoke(obj);
        }
    }

    public void ReadyAdd(LevelObject adding)
    {
        _addingObj = adding;
        _addingObj.OnReadyAdding(this);
    }

    public void CancelAdd()
    {
        if (_addingObj != null)
        {
            _addingObj.OnCancelAdding(this);
            _addingObj = null;
        }
    }

    public void PlaceAdding()
    {
        _bvh.AddItem(_addingObj);
        _addingObj.OnEnter(this);
        _addingObj.IsVisible = IsLayerVisible(_addingObj.Template.Layer);
        _addingObj.OnPlaceFromAdding(this);
        _addingObj = null;
    }

    public bool IsLayerVisible(RenderLayers layer)
    {
        return _allObjects.IsLayerVisible(layer);
    }

    public void SetLayerVisible(RenderLayers layer, bool visible)
    {
        _allObjects.SetLayerVisible(layer, visible);
    }

    public IReadOnlyList<LevelObject> PointTest(Point point, Predicate<LevelObject> filter = null)
    {
        var ret = new List<LevelObject>(16);
        _bvh.PointTest(point, ret, obj =>
        {
            if (!obj.PointTestInExactBounds(point))
                return false;
            return filter == null || filter.Invoke(obj);
        });
        return ret;
    }

    public void PointTest(Point point, IList<LevelObject> list, Predicate<LevelObject> filter = null)
    {
        _bvh.PointTest(point, list, obj =>
        {
            if (!obj.PointTestInExactBounds(point))
                return false;
            return filter == null || filter.Invoke(obj);
        });
    }

    public ISet<LevelObject> RectTest(Rect rect, Predicate<LevelObject> filter = null)
    {
        var ret = new HashSet<LevelObject>(16);
        _bvh.RectTest(rect, ret, filter);
        return ret;
    }

    public void RectTest(Rect rect, ISet<LevelObject> set, Predicate<LevelObject> filter = null)
    {
        _bvh.RectTest(rect, set, filter);
    }

    public void DeselectAll()
    {
        foreach (var pair in _selections)
        {
            RemoveObject(pair.Value);
            pair.Key._isSelected = false;
        }
        _selections.Clear();
        WorkspaceViewModel.Current.ClearSelections();
    }

    public bool CheckModified()
    {
        return _dirty;
    }

    public void MarkDirty()
    {
        _dirty = true;
    }

    void OnObjectTransformChanged(LevelObject obj)
    {
        _bvh.UpdateItem(obj);
    }

    void OnObjectSelectStateChanged(LevelObject obj, bool selected)
    {
        WorkspaceViewModel vm = WorkspaceViewModel.Current;
        if (selected)
        {
            var selection = ObjectPool<Selection>.Get(obj);
            if (obj is ISizeableObject)
                selection.VertexColor = new System.Numerics.Vector4(0.0f, 1.0f, 0.2f, 1.0f);
            else
                selection.VertexColor = new System.Numerics.Vector4(0.0f, 1.0f, 1.0f, 1.0f);
            AddObject(selection, false);
            _selections.Add(obj, selection);
            vm.AddSelectedObject(obj);
        }
        else
        {
            RemoveObject(_selections[obj]);
            _selections.Remove(obj);
            vm.RemoveSelectedObject(obj);
        }
    }

    internal BoundingVolumeHierarchy<LevelObject> _bvh = new BoundingVolumeHierarchy<LevelObject>();
    internal LevelObjects _allObjects = new LevelObjects();
    LevelEnvironment _envLv = new LevelEnvironment();
    LevelObject _addingObj;
    Dictionary<LevelObject, Selection> _selections = new Dictionary<LevelObject, Selection>(16);
    bool _dirty = false;
}