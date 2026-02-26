using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class LayerVisibility : ObservableObject
{
    public int Layer { get; set; }
    private bool _visible = true;
    public bool IsVisible
    {
        get { return _visible; }
        set
        {
            if (_visible != value)
            {
                _visible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }
}

public class LayerManager
{
    public event Action? LayerVisibilityChanged;

    public IReadOnlyList<LayerVisibility> LayerVisibilities => _layerVisibilities;

    public LayerManager()
    {
        for (int i = LevelModule.MAX_LAYER; i >= LevelModule.MIN_LAYER; i--)
        {
            _layerVisibilities[LevelModule.MAX_LAYER - i] = new LayerVisibility
            {
                Layer = i,
                IsVisible = true,
            };
            _layerVisibilities[LevelModule.MAX_LAYER - i].PropertyChanged += OnLayerVisibilityChanged;
        }
    }

    ~LayerManager()
    {
        foreach (var layerVis in _layerVisibilities)
        {
            layerVis.PropertyChanged -= OnLayerVisibilityChanged;
        }
    }

    public bool IsLayerVisible(int layer)
    {
        if (layer < LevelModule.MIN_LAYER || layer > LevelModule.MAX_LAYER) return false;
        return _layerVisibilities[LevelModule.MAX_LAYER - layer].IsVisible;
    }

    public void SetLayerVisible(int layer, bool visible)
    {
        if (layer < LevelModule.MIN_LAYER || layer > LevelModule.MAX_LAYER) return;
        _layerVisibilities[LevelModule.MAX_LAYER - layer].IsVisible = visible;
    }

    void OnLayerVisibilityChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LayerVisibility.IsVisible))
        {
            LayerVisibilityChanged?.Invoke();
        }
    }

    LayerVisibility[] _layerVisibilities = new LayerVisibility[LevelModule.MAX_LAYER - LevelModule.MIN_LAYER + 1];
}