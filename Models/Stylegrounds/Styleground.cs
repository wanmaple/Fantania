using System;
using System.Collections.Generic;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class Styleground : ObservableObject, IDataObject
{
    private StylegroundTemplate _template;
    [StandardSerialization(1)]
    public StylegroundTemplate Template
    {
        get => _template;
        set => _template = value;
    }

    private string _name = string.Empty;
    [EditString, Tooltip("TooltipStylegroundName"), StandardSerialization(1)]
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

    private bool _visible = true;
    public bool Visible
    {
        get { return _visible; }
        set
        {
            if (_visible != value)
            {
                OnPropertyChanging(nameof(Visible));
                _visible = value;
                OnPropertyChanged(nameof(Visible));
            }
        }
    }

    public bool ShouldReleaseForRendering => _renderRetainNum < 0;

    public Styleground()
    {
    }

    public Styleground(StylegroundTemplate template)
    {
        _template = template;
    }

    public IEnumerable<string> GetDataFormats()
    {
        return Array.Empty<string>();
    }

    public bool Contains(string dataFormat)
    {
        return false;
    }

    public object? Get(string dataFormat)
    {
        return null;
    }

    int _renderRetainNum = 0;
}