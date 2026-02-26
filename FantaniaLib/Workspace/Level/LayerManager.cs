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

    LayerVisibility[] _layerVisibilities = new LayerVisibility[LevelModule.MAX_LAYER - LevelModule.MIN_LAYER + 1];
}