namespace FantaniaLib;

public class ExportSettings : ScriptObject
{
    private string _exportFolder = string.Empty;
    [SerializableField(FieldTypes.String), EditableField(EditControlType = typeof(FolderBox), TooltipKey = "TT_ExportSettings_ExportFolder", EditParameter = "title:H_ExportTo")]
    public string ExportFolder
    {
        get { return _exportFolder; }
        set
        {
            if (_exportFolder != value)
            {
                OnPropertyChanging(nameof(ExportFolder));
                _exportFolder = value;
                OnPropertyChanged(nameof(ExportFolder));
            }
        }
    }

    public ExportSettings(ScriptTemplate script) : base(script)
    {
        
    }
}