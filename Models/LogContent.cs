using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public class LogContent : ObservableObject
{
    private string _content = string.Empty;
    public string Content
    {
        get { return _content; }
        set
        {
            if (_content != value)
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }
    }

    private FontWeight _fontWeight;
    public FontWeight FontWeight
    {
        get { return _fontWeight; }
        set
        {
            if (_fontWeight != value)
            {
                _fontWeight = value;
                OnPropertyChanged(nameof(FontWeight));
            }
        }
    }

    private int _fontSize;
    public int FontSize
    {
        get { return _fontSize; }
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }
    }

    private FontStyle _fontStyle;
    public FontStyle FontStyle
    {
        get { return _fontStyle; }
        set
        {
            if (_fontStyle != value)
            {
                _fontStyle = value;
                OnPropertyChanged(nameof(FontStyle));
            }
        }
    }

    private IBrush _color = Brushes.White;
    public IBrush Color
    {
        get { return _color; }
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
    }
}