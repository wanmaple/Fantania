using System.Collections.ObjectModel;

namespace Fantania.ViewModels;

public class PropertiesViewModel : ViewModelBase
{
    private ObservableCollection<IEditableProperty> _properites;
    public ObservableCollection<IEditableProperty> EditableProperties => _properites;

    public PropertiesViewModel(ObservableCollection<IEditableProperty> properties)
    {
        _properites = properties;
    }
}