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
                }
                OnPropertyChanged(nameof(LevelPopupIsOpen));
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