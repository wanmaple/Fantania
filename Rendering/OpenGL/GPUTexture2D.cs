using Avalonia.OpenGL;

namespace Fantania;

public class GPUTexture2D
{
    public int ID => _texId;
    public int Width => _width;
    public int Height => _height;
    public int Wrap => _wrap;

    public GPUTexture2D(int width, int height, int wrap)
    {
        _width = width;
        _height = height;
        _wrap = wrap;
    }

    public void Prepare(GlInterface gl)
    {
        _texId = OpenGLHelper.CreateTexture2DRGBA(gl, _width, _height, GlConsts.GL_LINEAR, GlConsts.GL_LINEAR, _wrap, _wrap);
    }

    public void Dispose(GlInterface gl)
    {
        gl.DeleteTexture(_texId);
        _texId = -1;
    }

    public unsafe void SetData(GlInterface gl, void* data)
    {
        Bind(gl);
        gl.TexImage2D(GlConsts.GL_TEXTURE_2D, 0, GlConsts.GL_RGBA8, _width, _height, 0, GlConsts.GL_RGBA, GlConsts.GL_UNSIGNED_BYTE, (nint)data);
    }

    public void Bind(GlInterface gl)
    {
        gl.BindTexture(GlConsts.GL_TEXTURE_2D, _texId);
    }

    int _texId = -1;
    int _width, _height;
    int _wrap;
}