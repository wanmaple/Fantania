using System.Collections.Generic;
using Avalonia;
using Fantania.Models;

namespace Fantania;

public class SelectionScope
{
    public bool IsEnabled { get; set; } = false;

    public void Apply(World world, Rect range)
    {
        if (range.Width == 0.0 && range.Height == 0.0)
        {
            _selected.Clear();
            return;
        }
        _cache.Clear();
        _toRm.Clear();
        world.RectTest(range, _cache, obj => obj.IsVisible);
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

    HashSet<WorldObject> _selected = new HashSet<WorldObject>(256);
    HashSet<WorldObject> _cache = new HashSet<WorldObject>(256);
    List<WorldObject> _toRm = new List<WorldObject>(256);
}