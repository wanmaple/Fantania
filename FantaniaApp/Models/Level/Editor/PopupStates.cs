using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class PopupStates : ObservableObject
{
    private bool _popupLvIsOpen = false;
    public bool LevelPopupIsOpen
    {
        get { return _popupLvIsOpen; }
        set
        {
            if (_popupLvIsOpen != value)
            {
                _popupLvIsOpen = value;
                if (_popupLvIsOpen)
                {
                    LayerPopupIsOpen = false;
                }
                OnPropertyChanged(nameof(LevelPopupIsOpen));
            }
        }
    }

    private bool _popupLayerIsOpen = false;
    public bool LayerPopupIsOpen
    {
        get { return _popupLayerIsOpen; }
        set
        {
            if (_popupLayerIsOpen != value)
            {
                _popupLayerIsOpen = value;
                if (_popupLayerIsOpen)
                {
                    LevelPopupIsOpen = false;
                }
                OnPropertyChanged(nameof(LayerPopupIsOpen));
            }
        }
    }

    private bool _popupRenderStatisticsIsOpen = true;
    public bool RenderStatisticsPopupIsOpen
    {
        get { return _popupRenderStatisticsIsOpen; }
        set
        {
            if (_popupRenderStatisticsIsOpen != value)
            {
                _popupRenderStatisticsIsOpen = value;
                OnPropertyChanged(nameof(RenderStatisticsPopupIsOpen));
            }
        }
    }
}