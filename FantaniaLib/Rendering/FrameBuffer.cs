namespace FantaniaLib;

[Flags, BindingScript]
public enum BufferBits
{
    None = 0x0,
    Color = 0x1,
    Depth = 0x2,
    Stencil = 0x4,
}

[BindingScript]
public enum DepthFormats
{
    None,
    Depth24Stencil8,
}

public class FrameBufferDescription
{
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public TextureFormats ColorFormat { get; set; } = TextureFormats.RGBA8;
    public DepthFormats DepthFormat { get; set; } = DepthFormats.Depth24Stencil8;
}

public class FrameBuffer : IRenderResource
{
    public FrameBufferDescription Description { get; private set; }
    public int ID => _fbo;
    public int ColorAttachment => _colorAttachment;
    public int DepthAttachment => _depthAttachment;

    internal FrameBuffer(FrameBufferDescription fbDesc, int fbo, int colorAttachment, int depthAttachment)
    {
        Description = fbDesc;
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