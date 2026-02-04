namespace FantaniaLib;

[BindingScript]
public class TextureManager : IDisposable
{
    public int FallbackTextureID => _fallbackTextureId;

    public unsafe TextureManager(IRenderDevice device)
    {
        _cacheSingleImage = new LocalTextureCache(device);
        _cacheAtlas = new LocalTextureCache(device);
        var fallback = new LocalTexture2D("avares://Fantania/Assets/textures/white4x4.png");
        if (fallback.TryDecode(out var desc, out var data))
        {
            fixed (void* ptr = data)
            {
                _fallbackTextureId = device.CreateTexture2D(desc, (nint)ptr);
            }
        }
    }

    public int AcquireTextureID(ITexture2D texture)
    {
        if (texture.Category == TextureCategory.Local)
            return _cacheSingleImage.Acquire(texture);
        else if (texture.Category == TextureCategory.Atlas)
            return _cacheAtlas.Acquire(texture);
        return 0;
    }

    public void ReleaseTexture(ITexture2D texture)
    {
        if (texture.Category == TextureCategory.Local)
            _cacheSingleImage.Release(texture);
        else if (texture.Category == TextureCategory.Atlas)
            _cacheAtlas.Release(texture);
    }

    public void Tick()
    {
        // 之所以不在ReleaseTexture时直接删除，是因为Canvas可能还在绘制上一帧的数据，而上一帧的数据可能还用到了这些纹理。
        _cacheSingleImage.CleanFreedTextures();
        _cacheAtlas.CleanFreedTextures();
    }

    public void Dispose()
    {
        _cacheSingleImage.Dispose();
        _cacheAtlas.Dispose();
    }

    LocalTextureCache _cacheSingleImage;
    LocalTextureCache _cacheAtlas;
    int _fallbackTextureId = 0;
}