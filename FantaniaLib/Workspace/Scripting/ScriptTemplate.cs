using System.Collections.ObjectModel;
using MoonSharp.Interpreter;

namespace FantaniaLib;

public class ScriptTemplate : IPlacement
{
    public string ClassName
    {
        get
        {
            return _engine.GetInstanceMember(_obj, "__clsname").String;
        }
    }

    public string Group
    {
        get
        {
            _engine.CallInstanceFunction(_obj, "group", out DynValue val);
            return val.String;
        }
    }

    public string Name
    {
        get
        {
            _engine.CallInstanceFunction(_obj, "name", out DynValue val);
            return val.String;
        }
    }

    public string Tooltip
    {
        get
        {
            _engine.CallInstanceFunction(_obj, "tooltip", out DynValue val);
            return val.String;
        }
    }

    public IList<IPlacement> Children => _children;

    public ScriptTemplate(ScriptEngine engine, DynValue obj)
    {
        _engine = engine;
        _obj = obj;
    }

    ScriptEngine _engine;
    DynValue _obj;
    ObservableCollection<IPlacement> _children = new ObservableCollection<IPlacement>();
}