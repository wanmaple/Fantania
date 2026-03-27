using MoonSharp.Interpreter;

namespace FantaniaLib;

public class GameDataTemplate : ScriptTemplate
{
    public string DataGroup => GetOrCallMember("dataGroup").GetStringOrDefault(string.Empty);
    public string TypeName => ClassName.MakeFirstCharacterUpper();

    public GameDataTemplate(ScriptEngine engine, DynValue obj) : base(engine, obj)
    {
    }
}