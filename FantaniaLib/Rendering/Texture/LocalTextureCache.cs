namespace FantaniaLib;

public class LocalTextureCache : IDisposable
{
    public LocalTextureCache(IRenderDevice device, TextureFilters defaultFilter)
    {
        _device = device;
        _defaultFilter = defaultFilter;
    }

    public unsafe int Acquire(ITexture2D texture)
    {
        if (_blacklist.Contains(texture.Identifier)) return 0;
        if (!_texIdMap.TryGetValue(texture.Identifier, out var counter))
        {
            if (!texture.TryDecode(_defaultFilter, out var desc, out var data))
            {
                _blacklist.Add(texture.Identifier);
                return 0;
            }
            fixed (void* ptr = data)
            {
                int texId = _device.CreateTexture2D(desc, (nint)ptr);
                counter = new ReferenceCounter<int>(texId);
            }
            _texIdMap.Add(texture.Identifier, counter);
        }
        else
            counter.Acquire();
        return counter.Item;
    }

    public void Release(ITexture2D texture)
    {
        if (_texIdMap.TryGetValue(texture.Identifier, out var counter))
        {
            counter.Release();
            // if (counter.IsFree)
            // {
            //     _device.DeleteTexture(counter.Item);
            //     _texIdMap.Remove(texture.Identifier);
            // }
        }
    }

    public void CleanFreedTextures()
    {
        if (_texIdMap.Count == 0)
            return;
        List<string>? keysToRemove = null;
        foreach (var pair in _texIdMap)
        {
            var counter = pair.Value;
            if (counter.IsFree)
            {
                _device.DeleteTexture(counter.Item);
                keysToRemove ??= new List<string>(4);
                keysToRemove.Add(pair.Key);
            }
        }
        if (keysToRemove != null)
        {
            foreach (var key in keysToRemove)
            {
                _texIdMap.Remove(key);
            }
        }
    }

    public void Dispose()
    {
        foreach (var counter in _texIdMap.Values)
        {
            _device.DeleteTexture(counter.Item);
        }
    }

    IRenderDevice _device;
    TextureFilters _defaultFilter;
    Dictionary<string, ReferenceCounter<int>> _texIdMap = new Dictionary<string, ReferenceCounter<int>>(128);
    HashSet<string> _blacklist = new HashSet<string>(16);
}