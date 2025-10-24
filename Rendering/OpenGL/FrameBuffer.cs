using Avalonia.OpenGL;

namespace Fantania;

public class FrameBuffer
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int ID => _fbo;
    public int ColorAttachment => _colorAttachment;
    public int DepthAttachment => _depthAttachment;

    public FrameBuffer(GlInterface gl, int width, int height)
    {
        Width = width;
        Height = height;
        OpenGLHelper.CreateFrameBufferColorDepthStencil(gl, width, height, out _fbo, out _colorAttachment, out _depthAttachment);
    }

    public void Bind(GlInterface gl)
    {
        gl.BindFramebuffer(GlConsts.GL_FRAMEBUFFER, _fbo);
    }

    public void Unbind(GlInterface gl)
    {
        gl.BindFramebuffer(GlConsts.GL_FRAMEBUFFER, 0);
    }

    public void BindForRead(GlInterface gl)
    {
        gl.BindFramebuffer(GlConsts.GL_READ_FRAMEBUFFER, _fbo);
    }

    public void BindForDraw(GlInterface gl)
    {
        gl.BindFramebuffer(GlConsts.GL_DRAW_FRAMEBUFFER, _fbo);
    }

    public void Resize(GlInterface gl, int width, int height)
    {
        OpenGLHelper.ResizeAttachments(gl, _colorAttachment, _depthAttachment, width, height);
        Width = width;
        Height = height;
    }

    public void Dispose(GlInterface gl)
    {
        gl.DeleteTexture(_colorAttachment);
        gl.DeleteRenderbuffer(_depthAttachment);
        gl.DeleteFramebuffer(_fbo);
    }

    int _fbo;
    int _colorAttachment;
    int _depthAttachment;
}