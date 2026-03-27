namespace FantaniaLib;

public struct ExportEntity
{
    public string EntityType;
    public IReadOnlyList<ExportProperty> EntityProperties;
    public IReadOnlyList<ExportProperty> TemplateProperties;
}

public struct ExportGameData
{
    public string TypeName;
    public string GroupName;
    public IReadOnlyList<ExportProperty> Properties;
}