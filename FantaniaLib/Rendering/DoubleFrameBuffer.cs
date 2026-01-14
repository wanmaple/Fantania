namespace FantaniaLib;

public class DoubleFrameBuffer : IDisposable
{
    public FrameBuffer CurrentBuffer
    {
        get
        {
            lock (_mutexFb)
                return _fbs[_currentIdx];
        }
    }
    public FrameBuffer BackBuffer
    {
        get
        {
            lock (_mutexFb)
                return _fbs[(_currentIdx + 1) % 2];
        }
    }

    public DoubleFrameBuffer(IRenderDevice device, int width, int height)
    {
        _device = device;
        var desc = new FrameBufferDescription
        {
            ColorFormat = TextureFormats.RGBA8,
            DepthFormat = DepthFormats.Depth24Stencil8,
            Width = width,
            Height = height,
        };
        _fbs[0] = _device.CreateFrameBuffer(desc);
        _fbs[1] = _device.CreateFrameBuffer(desc);
    }

    public void Swap()
    {
        lock (_mutexFb)
            _currentIdx = (_currentIdx + 1) % _fbs.Length;
    }

    public void Dispose()
    {
        foreach (FrameBuffer fb in _fbs)
        {
            _device.DeleteResource(fb);
        }
    }

    IRenderDevice _device;
    FrameBuffer[] _fbs = new FrameBuffer[2];
    int _currentIdx = 0;

    object _mutexFb = new object();
}