using System.Numerics;
using Avalonia.OpenGL;
using static FantaniaLib.GLConstants;

namespace FantaniaLib;

public static class GLApiEx
{
    public unsafe static void BlendFunc(GlInterface gl, int src, int dst)
    {
        nint ptr = gl.GetProcAddress("glBlendFunc");
        var func = (delegate*<int, int, void>)ptr;
        func(src, dst);
    }

    public unsafe static void TexImage1D(GlInterface gl, int target, int level, int internalFormat, int length, int format, int type, nint data)
    {
        nint ptr = gl.GetProcAddress("glTexImage1D");
        var func = (delegate*<int, int, int, int, int, int, int, nint, void>)ptr;
        func(target, level, internalFormat, length, 0, format, type, data);
    }

    public unsafe static void Uniform1i(GlInterface gl, int location, int value)
    {
        nint ptr = gl.GetProcAddress("glUniform1i");
        var func = (delegate*<int, int, void>)ptr;
        func(location, value);
    }

    public unsafe static void Uniform1iv(GlInterface gl, int location, int[] values)
    {
        nint ptr = gl.GetProcAddress("glUniform1iv");
        var func = (delegate*<int, int, void*, void>)ptr;
        fixed (int* arrPtr = values)
        {
            func(location, values.Length, arrPtr);
        }
    }

    public unsafe static void Uniform1fv(GlInterface gl, int location, float[] values)
    {
        nint ptr = gl.GetProcAddress("glUniform1fv");
        var func = (delegate*<int, int, void*, void>)ptr;
        fixed (float* arrPtr = values)
        {
            func(location, values.Length, arrPtr);
        }
    }

    public unsafe static void Uniform2f(GlInterface gl, int location, Vector2 vec)
    {
        nint ptr = gl.GetProcAddress("glUniform2f");
        var func = (delegate*<int, float, float, void>)ptr;
        func(location, vec.X, vec.Y);
    }

    public unsafe static void Uniform2fv(GlInterface gl, int location, Vector2[] values)
    {
        nint ptr = gl.GetProcAddress("glUniform2fv");
        var func = (delegate*<int, int, void*, void>)ptr;
        fixed (Vector2* arrPtr = values)
        {
            func(location, values.Length, arrPtr);
        }
    }

    public unsafe static void Uniform3f(GlInterface gl, int location, Vector3 vec)
    {
        nint ptr = gl.GetProcAddress("glUniform3f");
        var func = (delegate*<int, float, float, float, void>)ptr;
        func(location, vec.X, vec.Y, vec.Z);
    }

    public unsafe static void Uniform3fv(GlInterface gl, int location, Vector3[] values)
    {
        nint ptr = gl.GetProcAddress("glUniform3fv");
        var func = (delegate*<int, int, void*, void>)ptr;
        fixed (Vector3* arrPtr = values)
        {
            func(location, values.Length, arrPtr);
        }
    }

    public unsafe static void Uniform4f(GlInterface gl, int location, Vector4 vec)
    {
        nint ptr = gl.GetProcAddress("glUniform4f");
        var func = (delegate*<int, float, float, float, float, void>)ptr;
        func(location, vec.X, vec.Y, vec.Z, vec.W);
    }

    public unsafe static void Uniform4fv(GlInterface gl, int location, Vector4[] values)
    {
        nint ptr = gl.GetProcAddress("glUniform4fv");
        var func = (delegate*<int, int, void*, void>)ptr;
        fixed (Vector4* arrPtr = values)
        {
            func(location, values.Length, arrPtr);
        }
    }

    public unsafe static void UniformMatrix3fv(GlInterface gl, int location, Matrix3x3 mat, bool transpose = false)
    {
        nint ptr = gl.GetProcAddress("glUniformMatrix3fv");
        var func = (delegate*<int, int, int, void*, void>)ptr;
        func(location, 1, transpose ? GL_TRUE : GL_FALSE, &mat);
    }

    public unsafe static void UniformMatrix3fv(GlInterface gl, int location, Matrix3x3[] values, bool transpose = false)
    {
        nint ptr = gl.GetProcAddress("glUniformMatrix3fv");
        var func = (delegate*<int, int, int, void*, void>)ptr;
        fixed (Matrix3x3* arrPtr = values)
        {
            func(location, values.Length, transpose ? GL_TRUE : GL_FALSE, arrPtr);
        }
    }

    public unsafe static void GenerateMipmap(GlInterface gl, int target)
    {
        nint ptr = gl.GetProcAddress("glGenerateMipmap");
        var func = (delegate*<int, void>)ptr;
        func(target);
    }

    public unsafe static void GenerateTextureMipmap(GlInterface gl, int texture)
    {
        nint ptr = gl.GetProcAddress("glGenerateTextureMipmap");
        var func = (delegate*<int, void>)ptr;
        func(texture);
    }

    public unsafe static void CullFace(GlInterface gl, int target)
    {
        nint ptr = gl.GetProcAddress("glCullFace");
        var func = (delegate*<int, void>)ptr;
        func(target);
    }

    public unsafe static void PixelStoref(GlInterface gl, int pname, float param)
    {
        nint ptr = gl.GetProcAddress("glPixelStoref");
        var func = (delegate*<int, float, void>)ptr;
        func(pname, param);
    }

    public unsafe static void PixelStorei(GlInterface gl, int pname, int param)
    {
        nint ptr = gl.GetProcAddress("glPixelStorei");
        var func = (delegate*<int, int, void>)ptr;
        func(pname, param);
    }

    public unsafe static bool IsVertexArray(GlInterface gl, int id)
    {
        nint ptr = gl.GetProcAddress("glIsVertexArray");
        var func = (delegate*<int, bool>)ptr;
        return func(id);
    }

    public unsafe static bool IsBuffer(GlInterface gl, int id)
    {
        nint ptr = gl.GetProcAddress("glIsBuffer");
        var func = (delegate*<int, bool>)ptr;
        return func(id);
    }

    public unsafe static bool IsTexture(GlInterface gl, int id)
    {
        nint ptr = gl.GetProcAddress("glIsTexture");
        var func = (delegate*<int, bool>)ptr;
        return func(id);
    }
}