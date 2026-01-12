using System.Reflection;
using MoonSharp.Interpreter;

namespace FantaniaLib;

public class ScriptEngine
{
    public Script Environment => _env;

    static ScriptEngine()
    {
        AutoConversions();
    }

    public ScriptEngine()
    {
        _env.Options.ScriptLoader = new FantaniaScriptLoader();
        InitializeRequirements();
    }

    public DynValue ExecuteFile(string luaPath, Table? globalContext = null)
    {
        return _env.DoFile(luaPath, globalContext);
    }

    public DynValue ExecuteString(string luaCode, Table? globalContext = null)
    {
        return _env.DoString(luaCode, globalContext);
    }

    public DynValue GetGlobal(string name)
    {
        return _env.Globals.Get(name);
    }

    public void SetGlobal(string name, object value)
    {
        _env.Globals[name] = DynValue.FromObject(_env, value);
    }

    public bool CallGlobalFunction(string functionName, out DynValue ret, params object[] args)
    {
        ret = DynValue.Nil;
        var function = _env.Globals.Get(functionName);
        if (function.Type == DataType.Function)
        {
            ret = _env.Call(function, args);
            return true;
        }
        return false;
    }

    public bool CallInstanceFunction(DynValue instance, string functionName, out DynValue ret, params object[] args)
    {
        ret = DynValue.Nil;
        var function = GetInstanceMember(instance, functionName);
        if (function.Type == DataType.Function)
        {
            var allArgs = new object[args.Length + 1];
            allArgs[0] = instance;
            args.CopyTo(allArgs, 1);
            ret = _env.Call(function, allArgs);
            return true;
        }
        return false;
    }

    public DynValue GetInstanceMember(DynValue instance, string memberName)
    {
        var member = instance.Table.Get(memberName);
        if (member.Type != DataType.Nil)
            return member;
        if (instance.Table.MetaTable != null)
        {
            member = instance.Table.MetaTable.Get(memberName);
            if (member.Type != DataType.Nil)
                return member;
            var indexer = instance.Table.MetaTable.Get("__index");
            if (indexer.Type == DataType.Table)
                return indexer.Table.Get(memberName);
            else if (indexer.Type == DataType.Function)
                return _env.Call(indexer, instance, DynValue.NewString(memberName));
        }
        return DynValue.Nil;
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
            var attrBinding = type.GetCustomAttribute<BindingScriptAttribute>();
            if (attrBinding != null)
            {
                if (type.IsClass || type.IsInterface)
                {
                    BindClassToLua(type, attrBinding.CustomName, attrBinding.CanInstantiate);
                }
                else if (type.IsEnum)
                {
                    BindEnumToLua(type, attrBinding.CustomName);
                }
            }
        }
    }

    public void BindClassToLua<T>(string? name = null, bool canInstantiate = true) where T : class
    {
        BindClassToLua(typeof(T), name, canInstantiate);
    }

    public void BindClassToLua(Type type, string? name = null, bool canInstantiate = true)
    {
        if (string.IsNullOrEmpty(name))
            name = type.Name;
        UserData.RegisterType(type);
        if (type.IsStaticClass())
        {
            SetGlobal(name, UserData.CreateStatic(type));
        }
        else
        {
            if (type.IsAbstract || type.IsInterface) return;
            var tbl = new Table(_env);
            tbl["__name"] = name;
            if (canInstantiate)
            {
                tbl["new"] = DynValue.NewCallback((context, args) =>
                {
                    try
                    {
                        var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                        object?[] clrArgs = new object[args.Count];
                        foreach (var ctor in ctors)
                        {
                            var parameters = ctor.GetParameters();
                            if (parameters.Length != args.Count)
                                continue;
                            bool match = true;
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                var paramType = parameters[i].ParameterType;
                                object? arg = args[i].ToObject(paramType);
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
            }
            SetGlobal(name, tbl);
        }
    }

    public void BindEnumToLua<T>(string? name = null) where T : Enum
    {
        BindEnumToLua(typeof(T), name);
    }

    public void BindEnumToLua(Type type, string? name = null)
    {
        if (!type.IsEnum)
            throw new ScriptRuntimeException($"{type} is not enum.");
        if (string.IsNullOrEmpty(name))
            name = type.Name;
        var tbl = new Table(_env);
        tbl["__name"] = name;
        string[] memberNames = type.GetEnumNames();
        Array memberValues = type.GetEnumValues();
        for (int i = 0; i < memberNames.Length; i++)
        {
            string memberName = memberNames[i];
            object memberValue = memberValues.GetValue(i)!;
            tbl[memberName] = DynValue.FromObject(_env, memberValue);
        }
        SetGlobal(name, tbl);
    }

    void InitializeRequirements()
    {
        BindAssemblyToLua(Assembly.GetExecutingAssembly());
        ExecuteFile("avares://Fantania/Assets/scripts/init.lua");
    }

    static void AutoConversions()
    {
        CommonConversions.AutoConversions();
        MathsConversions.AutoConversions();
    }

    Script _env = new Script();
}