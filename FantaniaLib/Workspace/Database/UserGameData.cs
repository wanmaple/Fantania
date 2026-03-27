namespace FantaniaLib;

public class UserGameData : ScriptDatabaseObject
{
    public override string TypeName => TemplateAs<GameDataTemplate>().TypeName;
    public override string GroupName => TemplateAs<GameDataTemplate>().DataGroup;

    public UserGameData(GameDataTemplate template, int id) : base(template, id)
    {
    }
}