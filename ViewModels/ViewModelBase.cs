using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected void RaisePropertyChanged<T>(ref T field, string propertyName)
    {

    }
}
