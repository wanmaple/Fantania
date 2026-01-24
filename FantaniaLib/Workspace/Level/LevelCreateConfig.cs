namespace FantaniaLib;

public class LevelCreateConfig : SimpleEditable
{
    private string _name = string.Empty;
    [EditableField(TooltipKey = "TT_LevelName", FieldValidatorType = typeof(LevelNameValidator))]
    public string Name
    {
        get { return _name; }
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
}