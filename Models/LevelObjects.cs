using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fantania.Models;

public class LevelObjects : IEnumerable<LevelObject>
{
    public event Action<RenderLayers, bool> LayerVisibilityChanged;

    public LevelObjects()
    {
        var values = Enum.GetValues<RenderLayers>();
        _allObjects = new Dictionary<RenderLayers, ObservableCollection<LevelObject>>(values.Length);
        _layerVisibilities = new Dictionary<RenderLayers, bool>(values.Length);
        foreach (RenderLayers layer in values)
        {
            _allObjects.Add(layer, new ObservableCollection<LevelObject>());
            _layerVisibilities.Add(layer, true);
        }

    }
    public void AddObject(LevelObject obj)
    {
        _allObjects[obj.Template.Layer].Add(obj);
        obj.LayerChanged += OnLevelObjectLayerChanged;
    }

    public bool RemoveObject(LevelObject obj)
    {
        obj.LayerChanged -= OnLevelObjectLayerChanged;
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

    public IEnumerator<LevelObject> GetEnumerator()
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

    void OnLevelObjectLayerChanged(LevelObject obj, RenderLayers oldLayer, RenderLayers newLayer)
    {
        _allObjects[oldLayer].RemoveFast(obj);
        _allObjects[newLayer].Add(obj);
        obj.IsVisible = IsLayerVisible(newLayer);
    }

    Dictionary<RenderLayers, ObservableCollection<LevelObject>> _allObjects;
    Dictionary<RenderLayers, bool> _layerVisibilities;
}