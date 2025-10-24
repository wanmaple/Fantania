using System.Collections.Generic;
using Avalonia;
using Fantania.Models;

namespace Fantania;

public class SelectionScope
{
    public bool IsEnabled { get; set; } = false;

    public void Apply(Level lv, Rect range)
    {
        if (range.Width == 0.0 && range.Height == 0.0)
        {
            _selected.Clear();
            return;
        }
        _cache.Clear();
        _toRm.Clear();
        lv.RectTest(range, _cache, obj => obj.IsVisible);
        foreach (var selected in _selected)
        {
            if (!_cache.Remove(selected))
            {
                _toRm.Add(selected);
            }
        }
        foreach (var rm in _toRm)
        {
            rm.IsSelected = false;
            _selected.Remove(rm);
        }
        foreach (var obj in _cache)
            {
                if (_selected.Add(obj))
                {
                    obj.IsSelected = true;
                }
            }
    }

    public void Reset()
    {
        _selected.Clear();
        _cache.Clear();
    }

    HashSet<LevelObject> _selected = new HashSet<LevelObject>(256);
    HashSet<LevelObject> _cache = new HashSet<LevelObject>(256);
    List<LevelObject> _toRm = new List<LevelObject>(256);
}