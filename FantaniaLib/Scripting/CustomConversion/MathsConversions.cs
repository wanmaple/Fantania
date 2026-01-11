using System.Numerics;
using Avalonia.Media;
using MoonSharp.Interpreter;

namespace FantaniaLib;

public static class MathsConversions
{
    public static void AutoConversions()
    {
        // Vector2
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
        // Vector3
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
        // Vector4
        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector4>((env, v) =>
        {
            float x = v.X;
            float y = v.Y;
            float z = v.Z;
            float w = v.W;
            DynValue ret = DynValue.NewTable(env);
            ret.Table.Set("x", DynValue.FromObject(env, x));
            ret.Table.Set("y", DynValue.FromObject(env, y));
            ret.Table.Set("z", DynValue.FromObject(env, z));
            ret.Table.Set("w", DynValue.FromObject(env, w));
            ret.Table.Set("r", DynValue.FromObject(env, x));
            ret.Table.Set("g", DynValue.FromObject(env, y));
            ret.Table.Set("b", DynValue.FromObject(env, z));
            ret.Table.Set("a", DynValue.FromObject(env, w));
            return ret;
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector4), v =>
        {
            float x = (float)v.Table.Get("x").Number;
            float y = (float)v.Table.Get("y").Number;
            float z = (float)v.Table.Get("z").Number;
            float w = (float)v.Table.Get("w").Number;
            return new Vector4(x, y, z, w);
        });
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.String, typeof(Vector4), v =>
        {
            Color color = Color.Parse(v.String);
            return color.ToVector4();
        });
        // Vector2Int
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
        // Matrix3x3
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
}