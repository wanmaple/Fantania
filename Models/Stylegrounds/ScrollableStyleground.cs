using Fantania.Models;

namespace Fantania;

public class ScrollableStyleground : Styleground
{
    public ScrollableStyleground()
    {
    }

    private double _scrollX = 0.0;
    [EditDecimal(0.0, 1.0), Tooltip("TooltipScrollX"), StandardSerialization(1)]
    public double ScrollX
    {
        get { return _scrollX; }
        set
        {
            if (_scrollX != value)
            {
                OnPropertyChanging(nameof(ScrollX));
                _scrollX = value;
                OnPropertyChanged(nameof(ScrollX));
            }
        }
    }

    private double _scrollY = 0.0;
    [EditDecimal(0.0, 1.0), Tooltip("TooltipScrollY"), StandardSerialization(1)]
    public double ScrollY
    {
        get { return _scrollY; }
        set
        {
            if (_scrollY != value)
            {
                OnPropertyChanging(nameof(ScrollY));
                _scrollY = value;
                OnPropertyChanged(nameof(ScrollY));
            }
        }
    }

    private double _speedX = 0.0;
    [EditDecimal, Tooltip("TooltipSpeedX"), StandardSerialization(1)]
    public double SpeedX
    {
        get { return _speedX; }
        set
        {
            if (_speedX != value)
            {
                OnPropertyChanging(nameof(SpeedX));
                _speedX = value;
                OnPropertyChanged(nameof(SpeedX));
            }
        }
    }

    private double _speedY = 0.0;
    [EditDecimal, Tooltip("TooltipSpeedY"), StandardSerialization(1)]
    public double SpeedY
    {
        get { return _speedY; }
        set
        {
            if (_speedY != value)
            {
                OnPropertyChanging(nameof(SpeedY));
                _speedY = value;
                OnPropertyChanged(nameof(SpeedY));
            }
        }
    }

    public ScrollableStyleground(StylegroundTemplate template)
    : base(template)
    { }
}