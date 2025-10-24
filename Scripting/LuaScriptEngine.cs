using System;
using System.Reflection;
using MoonSharp.Interpreter;

namespace Fantania.Scripting;

public class LuaScriptEngine
{
    public LuaScriptEngine()
    {
    }

    public DynValue ExecuteFile(string luaPath)
    {
        string scriptContent = IOHelper.ReadAllText(luaPath);
        return _env.DoString(scriptContent);
    }

    public DynValue ExecuteString(string luaCode)
    {
        return _env.DoString(luaCode);
    }

    public DynValue GetGlobal(string name)
    {
        return _env.Globals.Get(name);
    }

    public void SetGlobal(string name, object value)
    {
        _env.Globals[name] = DynValue.FromObject(_env, value);
    }

    public DynValue CallGlobalLuaFunction(string functionName, params object[] args)
    {
        var function = _env.Globals.Get(functionName);
        if (function.Type == DataType.Function)
        {
            return _env.Call(function, args);
        }
        throw new ScriptRuntimeException($"Lua function '{functionName}' not found.");
    }

    public DynValue CallInstanceLuaFunction(DynValue instance, string functionName, params object[] args)
    {
        var function = instance.Table.Get(functionName);
        if (function.Type == DataType.Function)
        {
            var allArgs = new object[args.Length + 1];
            allArgs[0] = instance;
            args.CopyTo(allArgs, 1);
            return _env.Call(function, allArgs);
        }
        throw new ScriptRuntimeException($"Lua function '{functionName}' not found in the given instance.");
    }

    public DynValue GetInstanceMember(DynValue instance, string memberName)
    {
        return instance.Table.Get(memberName);
    }

    public void SetInstanceMember(DynValue instance, string memberName, object value)
    {
        instance.Table.Set(memberName, DynValue.FromObject(_env, value));
    }

    public void BindAssemblyToLua(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (type.GetCustomAttribute<BindingLuaAttribute>() != null)
            {
                if (type.IsClass)
                {
                    BindClassToLua(type); 
                }
            }
        }
    }

    public void BindClassToLua<T>() where T : class
    {
        BindClassToLua(typeof(T));
    }
    
    public void BindClassToLua(Type type)
    {
        UserData.RegisterType(type);
        var tbl = new Table(_env);
        tbl["new"] = DynValue.NewCallback((context, args) =>
        {
            try
            {
                var ctors = type.GetConstructors();
                object[] clrArgs = new object[args.Count];
                ConstructorInfo targetCtor = null;
                foreach (var ctor in ctors)
                {
                    var parameters = ctor.GetParameters();
                    if (parameters.Length != args.Count)
                        continue;
                    bool match = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var paramType = parameters[i].ParameterType;
                        object arg = args[i].ToObject(paramType);
                        if (arg == null && args[i].Type != DataType.Nil)
                        {
                            match = false;
                            break;
                        }
                        clrArgs[i] = arg;
                    }
                    if (match)
                    {
                        var instance = ctor.Invoke(clrArgs);
                        return DynValue.FromObject(_env, instance);
                    }
                }
                throw new ScriptRuntimeException($"No matching constructor found for type '{type.FullName}'.");
            }
            catch (ScriptRuntimeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException($"Error creating instance of '{type.FullName}': {ex.Message}");
            }
        });
        SetGlobal(type.Name, tbl);
    }

    Script _env = new Script();
}