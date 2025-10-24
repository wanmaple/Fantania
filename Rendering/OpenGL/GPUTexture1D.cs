using Avalonia.OpenGL;

namespace Fantania;

public class GPUTexture1D
{
    public int ID => _texId;
    public int Size => _size;
    public int Wrap => _wrap;

    public GPUTexture1D(int size, int wrap)
    {
        _size = size;
        _wrap = wrap;
    }

    public void Prepare(GlInterface gl)
    {
        _texId = OpenGLHelper.CreateTexture1DRGBA(gl, _size, GlConsts.GL_LINEAR, GlConsts.GL_LINEAR, _wrap, 0);
    }

    public void Dispose(GlInterface gl)
    {
        gl.DeleteTexture(_texId);
        _texId = -1;
    }

    public unsafe void SetData(GlInterface gl, void* data)
    {
        Bind(gl);
        OpenGLApiEx.TexImage1D(gl, OpenGLApiEx.GL_TEXTURE_1D, 0, GlConsts.GL_RGBA8, Size, GlConsts.GL_RGBA, GlConsts.GL_UNSIGNED_BYTE, (nint)data);
    }

    public void Bind(GlInterface gl)
    {
        gl.BindTexture(OpenGLApiEx.GL_TEXTURE_1D, _texId);
    }

    int _texId = -1;
    int _size;
    int _wrap;
}