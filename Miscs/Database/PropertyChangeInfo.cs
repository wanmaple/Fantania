using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class PropertyChangeInfo : ObservableObject
{
    private string _propName;
    public string PropertyName
    {
        get { return _propName; }
        set
        {
            if (_propName != value)
            {
                _propName = value;
                OnPropertyChanged(nameof(PropertyName));
            }
        }
    }

    private object _oldValue;
    public object OldValue
    {
        get { return _oldValue; }
        set
        {
            if (_oldValue != value)
            {
                _oldValue = value;
                OnPropertyChanged(nameof(OldValue));
            }
        }
    }
    
    private object _newValue;
    public object NewValue
    {
        get { return _newValue; }
        set
        {
            if (_newValue != value)
            {
                _newValue = value;
                OnPropertyChanged(nameof(NewValue));
            }
        }
    }
}