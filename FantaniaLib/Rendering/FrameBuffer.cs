namespace FantaniaLib;

public enum FrameBufferDepthFormat
{
    None,
    Depth24Stencil8,
}

public class FrameBufferDescription
{
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public TextureFormats ColorFormat { get; set; } = TextureFormats.RGBA8;
    public FrameBufferDepthFormat DepthFormat { get; set; } = FrameBufferDepthFormat.Depth24Stencil8;
}

public class FrameBuffer : IRenderResource
{
    public int ID => _fbo;
    public int ColorAttachment => _colorAttachment;
    public int DepthAttachment => _depthAttachment;

    internal FrameBuffer(int fbo, int colorAttachment, int depthAttachment)
    {
        _fbo = fbo;
        _colorAttachment = colorAttachment;
        _depthAttachment = depthAttachment;
    }

    public void Dispose(IRenderDevice device)
    {
        device.DeleteTexture(_colorAttachment);
        device.DeleteRenderBuffer(_depthAttachment);
        device.DeleteFrameBuffer(_fbo);
        _fbo = _colorAttachment = _depthAttachment = 0;
    }

    int _fbo;
    int _colorAttachment, _depthAttachment;
}