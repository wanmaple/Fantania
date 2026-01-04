using MoonSharp.Interpreter;

namespace FantaniaLib;

[BindingScript]
public class ScriptableObject
{
    public ScriptableObject(Script env)
    {
        _scriptInst = DynValue.NewTable(env);
    }

    DynValue _scriptInst;
}