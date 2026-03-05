namespace FantaniaLib;

public class FrameBufferPingPong
{
    public string PingName => _names[_currentIndex];
    public string PongName => _names[1 - _currentIndex];
    public FrameBuffer Ping => _buffers[_currentIndex];
    public FrameBuffer Pong => _buffers[1 - _currentIndex];

    public FrameBufferPingPong(string fb1Name, FrameBuffer fb1, string fb2Name, FrameBuffer fb2)
    {
        _names[0] = fb1Name;
        _names[1] = fb2Name;
        _buffers[0] = fb1;
        _buffers[1] = fb2;
    }

    public void Swap()
    {
        _currentIndex = 1 - _currentIndex;
    }

    FrameBuffer[] _buffers = new FrameBuffer[2];
    string[] _names = new string[2];
    int _currentIndex = 0;
}