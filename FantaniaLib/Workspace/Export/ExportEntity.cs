namespace FantaniaLib;

public struct ExportEntity
{
    public string EntityType;
    public IReadOnlyList<ExportProperty> EntityProperties;
    public IReadOnlyList<ExportProperty> TemplateProperties;
}