using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class PanelStates : ObservableObject
{
    private bool _showLevelPanel = false;
    public bool ShowLevelPanel
    {
        get { return _showLevelPanel; }
        set
        {
            if (_showLevelPanel != value)
            {
                _showLevelPanel = value;
                OnPropertyChanged(nameof(ShowLevelPanel));
            }
        }
    }

    private bool _showStylegroundPanel = false;
    public bool ShowStylegroundPanel
    {
        get { return _showStylegroundPanel; }
        set
        {
            if (_showStylegroundPanel != value)
            {
                _showStylegroundPanel = value;
                OnPropertyChanged(nameof(ShowStylegroundPanel));
            }
        }
    }

    private bool _showLayerPanel = false;
    public bool ShowLayerPanel
    {
        get { return _showLayerPanel; }
        set
        {
            if (_showLayerPanel != value)
            {
                _showLayerPanel = value;
                OnPropertyChanged(nameof(ShowLayerPanel));
            }
        }
    }

    public void HideAll()
    {
        ShowLevelPanel = ShowStylegroundPanel = ShowLayerPanel = false;
    }
}