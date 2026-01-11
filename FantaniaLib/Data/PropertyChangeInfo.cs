using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class PropertyChangeInfo : ObservableObject
{
    private string _propName = string.Empty;
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

    private string _oldValue = string.Empty;
    public string OldValue
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
    
    private string _newValue = string.Empty;
    public string NewValue
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