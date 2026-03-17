namespace FantaniaLib;

public class ExportSettings : ScriptObject
{
    private string _projFolder = string.Empty;
    [SerializableField(FieldTypes.String), EditableField(EditControlType = typeof(FolderBox), TooltipKey = "TT_ExportSettings_ExportFolder", EditParameter = "title:H_ExportTo")]
    public string ProjectFolder
    {
        get { return _projFolder; }
        set
        {
            if (_projFolder != value)
            {
                OnPropertyChanging(nameof(ProjectFolder));
                _projFolder = value;
                OnPropertyChanged(nameof(ProjectFolder));
            }
        }
    }

    private string _srcFolder = string.Empty;
    [SerializableField(FieldTypes.String), EditableField(EditControlType = typeof(FolderBox), TooltipKey = "TT_ExportSettings_SourceCodeFolder", EditParameter = "title:H_SelectSourceCodeFolder")]
    public string SourceCodeFolder
    {
        get { return _srcFolder; }
        set
        {
            if (_srcFolder != value)
            {
                OnPropertyChanging(nameof(SourceCodeFolder));
                _srcFolder = value;
                OnPropertyChanged(nameof(SourceCodeFolder));
            }
        }
    }

    public ExportSettings(ScriptTemplate script) : base(script)
    {
        
    }
}