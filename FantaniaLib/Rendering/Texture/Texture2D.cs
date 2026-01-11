namespace FantaniaLib;

public class Texture2D : ITexture2D
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public ColorSpaces ColorSpace { get; private set; } = ColorSpaces.Unknown;

    public bool IsValid { get; private set; } = false;

    public Texture2D(Stream stream)
    {
        LoadFromStream(stream);
    }

    void LoadFromStream(Stream stream)
    {
        IsValid = CodecAnalysis.AnalysisStream(stream, out var result);
        if (IsValid)
        {
            Width = result.Width;
            Height = result.Height;
            ColorSpace = result.ColorSpace;
        }
    }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, $"Size: ({Width}, {Height})", $"Color Space: {ColorSpace}");
    }
}