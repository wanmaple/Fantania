namespace FantaniaLib;

public class VertexStreamCache : IDisposable
{
    public VertexStreamCache(IRenderDevice device)
    {
        _device = device;
    }

    public VertexStream Acquire(VertexDescriptor desc)
    {
        if (!_cache.TryGetValue(desc, out var cache) || cache.Count == 0)
        {
            return _device.CreateVertexStream(desc);
        }
        return cache.Pop();
    }

    public void Recycle(VertexStream stream)
    {
        if (!_cache.TryGetValue(stream.Descriptor, out var cache))
        {
            cache = new Stack<VertexStream>(32);
            _cache.Add(stream.Descriptor, cache);
        }
        stream.Reset();
        cache.Push(stream);
    }

    public void Dispose()
    {
        foreach (var ary in _cache.Values)
        {
            foreach (var stream in ary)
            {
                stream.Dispose(_device);
            }
        }
        _cache.Clear();
    }

    Dictionary<VertexDescriptor, Stack<VertexStream>> _cache = new Dictionary<VertexDescriptor, Stack<VertexStream>>(8);
    IRenderDevice _device;
}