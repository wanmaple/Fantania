using System.Numerics;
using Avalonia.OpenGL;

namespace Fantania;

public struct OpenGLUniform
{
    public enum UniformType
    {
        Float1,
        Float2,
        Float3,
        Float4,
        Matrix3x3,
    }

    public OpenGLUniform(UniformType type, object value)
    {
        _uniformType = type;
        _value = value;
    }

    public T GetValue<T>()
    {
        return (T)_value;
    }

    public void SetValue(object value)
    {
        _value = value;
    }

    public OpenGLUniform Clone()
    {
        return new OpenGLUniform(_uniformType, _value);
    }

    public void Sync(GlInterface gl, int location)
    {
        switch (_uniformType)
        {
            case UniformType.Float1:
                gl.Uniform1f(location, GetValue<float>());
                break;
            case UniformType.Float2:
                OpenGLApiEx.Uniform2f(gl, location, GetValue<Vector2>());
                break;
            case UniformType.Float3:
                OpenGLApiEx.Uniform3f(gl, location, GetValue<Vector3>());
                break;
            case UniformType.Float4:
                OpenGLApiEx.Uniform4f(gl, location, GetValue<Vector4>());
                break;
            case UniformType.Matrix3x3:
                OpenGLApiEx.UniformMatrix3fv(gl, location, GetValue<Matrix3x3>());
                break;
        }
    }

    UniformType _uniformType;
    object _value;
}