namespace Fantania.ViewModels;

public class DatabaseObjectViewModel : ViewModelBase
{
    public enum Functionalities
    {
        Adding,
        Editing,
    }

    private DatabaseObject _currentObj;
    public DatabaseObject CurrentObject
    {
        get { return _currentObj; }
        set
        {
            if (_currentObj != value)
            {
                _currentObj = value;
                OnPropertyChanged(nameof(CurrentObject));
            }
        }
    }

    public Functionalities Functionality { get; set; }

    public DatabaseObjectViewModel(DatabaseObject obj, Functionalities functionality)
    {
        CurrentObject = obj;
        Functionality = functionality;
    }
}