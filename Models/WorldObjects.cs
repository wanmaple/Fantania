using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fantania.Models;

public class WorldObjects : IEnumerable<WorldObject>
{
    public event Action<RenderLayers, bool> LayerVisibilityChanged;

    public WorldObjects()
    {
        var values = Enum.GetValues<RenderLayers>();
        _allObjects = new Dictionary<RenderLayers, ObservableCollection<WorldObject>>(values.Length);
        _layerVisibilities = new Dictionary<RenderLayers, bool>(values.Length);
        foreach (RenderLayers layer in values)
        {
            _allObjects.Add(layer, new ObservableCollection<WorldObject>());
            _layerVisibilities.Add(layer, true);
        }

    }
    public void AddObject(WorldObject obj)
    {
        _allObjects[obj.Template.Layer].Add(obj);
        obj.LayerChanged += OnWorldObjectLayerChanged;
    }

    public bool RemoveObject(WorldObject obj)
    {
        obj.LayerChanged -= OnWorldObjectLayerChanged;
        return _allObjects[obj.Template.Layer].RemoveFast(obj);
    }

    public bool IsLayerVisible(RenderLayers layer)
    {
        return _layerVisibilities[layer];
    }

    public void SetLayerVisible(RenderLayers layer, bool visible)
    {
        if (_layerVisibilities[layer] != visible)
        {
            _layerVisibilities[layer] = visible;
            foreach (var obj in _allObjects[layer])
            {
                obj.IsVisible = visible;
                obj.IsSelected = false;
            }
            LayerVisibilityChanged?.Invoke(layer, visible);
        }
    }

    public IEnumerator<WorldObject> GetEnumerator()
    {
        foreach (var pair in _allObjects)
        {
            foreach (var obj in pair.Value)
            {
                yield return obj;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    void OnWorldObjectLayerChanged(WorldObject obj, RenderLayers oldLayer, RenderLayers newLayer)
    {
        _allObjects[oldLayer].RemoveFast(obj);
        _allObjects[newLayer].Add(obj);
        obj.IsVisible = IsLayerVisible(newLayer);
    }

    Dictionary<RenderLayers, ObservableCollection<WorldObject>> _allObjects;
    Dictionary<RenderLayers, bool> _layerVisibilities;
}