using System.Numerics;
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

    public DynValue ExecuteFile(string luaPath, Table globalContext = null)
    {
        return _env.DoFile(luaPath, globalContext);
    }

    public DynValue ExecuteString(string luaCode, Table globalContext = null)
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

    public void BindClassToLua<T>(string name = null) where T : class
    {
        BindClassToLua(typeof(T), name);
    }

    public void BindClassToLua(Type type, string name = null)
    {
        UserData.RegisterType(type);
        if (type.IsAbstract || type.IsInterface || type.IsStaticClass()) return;
        if (string.IsNullOrEmpty(name))
            name = type.Name;
        var tbl = new Table(_env);
        tbl["__name"] = name;
        tbl["new"] = DynValue.NewCallback((context, args) =>
        {
            try
            {
                var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
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
        SetGlobal(name, tbl);
    }

    public void BindEnumToLua<T>(string name = null) where T : Enum
    {
        BindEnumToLua(typeof(T), name);
    }

    public void BindEnumToLua(Type type, string name = null)
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
            object memberValue = memberValues.GetValue(i);
            tbl[memberName] = DynValue.FromObject(_env, memberValue);
        }
        SetGlobal(name, tbl);
    }
    
    void InitializeRequirements()
    {
        ExecuteFile("avares://Fantania/Assets/scripts/init.lua");
        BindAssemblyToLua(Assembly.GetExecutingAssembly());
    }

    void BindAssemblyToLua(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            var attrBinding = type.GetCustomAttribute<BindingScriptAttribute>();
            if (attrBinding != null)
            {
                if (type.IsClass || type.IsInterface)
                {
                    BindClassToLua(type, attrBinding.CustomName);
                }
                else if (type.IsEnum)
                {
                    BindEnumToLua(type, attrBinding.CustomName);
                }
            }
        }
    }

    static void AutoConversions()
    {
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector2>((env, v) =>
        {
            float x = v.X;
            float y = v.Y;
            DynValue ret = DynValue.NewTable(env);
            ret.Table.Set("x", DynValue.FromObject(env, x));
            ret.Table.Set("y", DynValue.FromObject(env, y));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector2), v =>
        {
            float x = (float)v.Table.Get("x").Number;
            float y = (float)v.Table.Get("y").Number;
            return new Vector2(x, y);
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector3>((env, v) =>
        {
            float x = v.X;
            float y = v.Y;
            float z = v.Z;
            DynValue ret = DynValue.NewTable(env);
            ret.Table.Set("x", DynValue.FromObject(env, x));
            ret.Table.Set("y", DynValue.FromObject(env, y));
            ret.Table.Set("z", DynValue.FromObject(env, z));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector3), v =>
        {
            float x = (float)v.Table.Get("x").Number;
            float y = (float)v.Table.Get("y").Number;
            float z = (float)v.Table.Get("z").Number;
            return new Vector3(x, y, z);
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector4>((env, v) =>
        {
            float x = v.X;
            float y = v.Y;
            float z = v.Z;
            float w = v.W;
            DynValue ret = DynValue.NewTable(env);
            ret.Table.Set("r", DynValue.FromObject(env, x));
            ret.Table.Set("g", DynValue.FromObject(env, y));
            ret.Table.Set("b", DynValue.FromObject(env, z));
            ret.Table.Set("a", DynValue.FromObject(env, w));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector4), v =>
        {
            float x = (float)v.Table.Get("r").Number;
            float y = (float)v.Table.Get("g").Number;
            float z = (float)v.Table.Get("b").Number;
            float w = (float)v.Table.Get("a").Number;
            return new Vector4(x, y, z, w);
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector2Int>((env, v) =>
        {
            int x = v.x;
            int y = v.y;
            DynValue ret = DynValue.NewTable(env);
            ret.Table.Set("x", DynValue.FromObject(env, x));
            ret.Table.Set("y", DynValue.FromObject(env, y));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector2Int), v =>
        {
            int x = (int)v.Table.Get("x").Number;
            int y = (int)v.Table.Get("y").Number;
            return new Vector2Int(x, y);
        });
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Matrix3x3>((env, v) =>
        {
            float m00 = v.m00;
            float m01 = v.m01;
            float m02 = v.m02;
            float m10 = v.m10;
            float m11 = v.m11;
            float m12 = v.m12;
            float m20 = v.m20;
            float m21 = v.m21;
            float m22 = v.m22;
            DynValue ret = DynValue.NewTable(env);
            ret.Table.Set("m00", DynValue.FromObject(env, m00));
            ret.Table.Set("m01", DynValue.FromObject(env, m01));
            ret.Table.Set("m02", DynValue.FromObject(env, m02));
            ret.Table.Set("m10", DynValue.FromObject(env, m10));
            ret.Table.Set("m11", DynValue.FromObject(env, m11));
            ret.Table.Set("m12", DynValue.FromObject(env, m12));
            ret.Table.Set("m20", DynValue.FromObject(env, m20));
            ret.Table.Set("m21", DynValue.FromObject(env, m21));
            ret.Table.Set("m22", DynValue.FromObject(env, m22));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Matrix3x3), v =>
        {
            float m00 = (float)v.Table.Get("m00").Number;
            float m01 = (float)v.Table.Get("m01").Number;
            float m02 = (float)v.Table.Get("m02").Number;
            float m10 = (float)v.Table.Get("m10").Number;
            float m11 = (float)v.Table.Get("m11").Number;
            float m12 = (float)v.Table.Get("m12").Number;
            float m20 = (float)v.Table.Get("m20").Number;
            float m21 = (float)v.Table.Get("m21").Number;
            float m22 = (float)v.Table.Get("m22").Number;
            return new Matrix3x3(m00, m01, m02, m10, m11, m12, m20, m21, m22);
        });
    }

    Script _env = new Script();
}