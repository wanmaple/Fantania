namespace FantaniaLib;

[BindingScript]
public enum SizerTypes
{
    None,
    Texture,
    Fixed,
}

public interface IRenderableSizer
{
    public Vector2Int SizeOfRenderable(IWorkspace workspace);
}

public class FallbackSizer : IRenderableSizer
{
    public static readonly IRenderableSizer Fallback = new FallbackSizer();

    private FallbackSizer()
    {}

    public Vector2Int SizeOfRenderable(IWorkspace workspace)
    {
        return Vector2Int.Zero;
    }
}

public class TextureSizer : IRenderableSizer
{
    public TextureDefinition TextureDefinition { get; private set; }

    public TextureSizer(TextureDefinition def)
    {
        TextureDefinition = def;
    }
    
    public Vector2Int SizeOfRenderable(IWorkspace workspace)
    {
        ITexture2D? texture = TextureDefinition.ToTexture(workspace.RootFolder);
        if (texture != null)
            return new Vector2Int(texture.TextureRect.Width, texture.TextureRect.Height);
        return Vector2Int.Zero;
    }
}

public class FixedSizer : IRenderableSizer
{
    public Vector2Int FixedSize { get; private set; }

    public FixedSizer(Vector2Int size)
    {
        FixedSize = size;
    }

    public Vector2Int SizeOfRenderable(IWorkspace workspace)
    {
        return FixedSize;
    }
}