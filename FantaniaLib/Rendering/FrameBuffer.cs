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

public class FrameBufferColorDescription
{
    public static readonly FrameBufferColorDescription Default = new FrameBufferColorDescription();

    public TextureFormats Format { get; set; } = TextureFormats.RGBA8;
    public TextureMinFilters MinFilter { get; set; } = TextureMinFilters.Nearest;
    public TextureMagFilters MagFilter { get; set; } = TextureMagFilters.Nearest;
    public TextureWraps WrapS { get; set; } = TextureWraps.ClampToEdge;
    public TextureWraps WrapT { get; set; } = TextureWraps.ClampToEdge;
}

public class FrameBufferDescription
{
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public FrameBufferColorDescription ColorDescription { get; set; } = FrameBufferColorDescription.Default;
    public IReadOnlyList<FrameBufferColorDescription>? ColorDescriptions { get; set; } = null;
    public DepthFormats DepthFormat { get; set; } = DepthFormats.Depth24Stencil8;
}

public class FrameBuffer : IRenderResource
{
    public FrameBufferDescription Description { get; private set; }
    public int ID => _fbo;
    public int ColorAttachment => ColorAttachmentAt(0);
    public IReadOnlyList<int> ColorAttachments => _colorAttachments;
    public int DepthAttachment => _depthAttachment;

    internal FrameBuffer(FrameBufferDescription fbDesc, int fbo, IReadOnlyList<int> colorAttachments, int depthAttachment)
    {
        Description = fbDesc;
        _fbo = fbo;
        _colorAttachments = [.. colorAttachments];
        _depthAttachment = depthAttachment;
    }

    public int ColorAttachmentAt(int index)
    {
        if (index < 0 || index >= _colorAttachments.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _colorAttachments[index];
    }

    public void Dispose(IRenderDevice device)
    {
        foreach (var colorAttachment in _colorAttachments)
        {
            device.DeleteTexture(colorAttachment);
        }
        device.DeleteRenderBuffer(_depthAttachment);
        device.DeleteFrameBuffer(_fbo);
        _fbo = _depthAttachment = 0;
        _colorAttachments = [];
    }

    int _fbo;
    int[] _colorAttachments = [];
    int _depthAttachment;
}