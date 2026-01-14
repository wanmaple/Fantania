using System.Runtime.InteropServices;
using MoonSharp.Interpreter;

namespace FantaniaLib;

public abstract class ScriptableObject
{
    protected ScriptableObject(DynValue table)
    {
        nint ptr = new nint((long)table.Table.Get("__env").Number);
        GCHandle handle = GCHandle.FromIntPtr(ptr);
        _engine = (ScriptEngine)handle.Target!;
        _table = table;
    }

    protected DynValue GetMember(string member)
    {
        return _engine.GetInstanceMember(_table, member);
    }

    protected DynValue Call(string function, params object[] args)
    {
        if (_engine.CallInstanceFunction(_table, function, out var ret, args))
            return ret;
        return DynValue.Nil;
    }
    
    ScriptEngine _engine;
    DynValue _table;
}