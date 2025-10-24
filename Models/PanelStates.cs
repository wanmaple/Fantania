using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class PanelStates : ObservableObject
{
    private bool _showWorldPanel = false;
    public bool ShowWorldPanel
    {
        get { return _showWorldPanel; }
        set
        {
            if (_showWorldPanel != value)
            {
                _showWorldPanel = value;
                OnPropertyChanged(nameof(ShowWorldPanel));
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
        ShowWorldPanel = ShowStylegroundPanel = ShowLayerPanel = false;
    }
}