namespace FantaniaLib;

public class TextureManager
{
    public TextureManager(IRenderDevice device)
    {
        _cacheSingleImage = new LocalTextureCache(device);
        _cacheAtlas = new LocalTextureCache(device);
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

    LocalTextureCache _cacheSingleImage;
    LocalTextureCache _cacheAtlas;
}