using SkiaSharp;

namespace FantaniaLib;

public enum ColorSpaces
{
    Unknown,
    Linear,
    Srgb,
    OtherSrgbLike,
    OtherLinear,
    WideGamut,
    Custom,
}

public static class CodecAnalysis
{
    public class AnalysisResult
    {
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public ColorSpaces ColorSpace { get; set; } = ColorSpaces.Unknown;
        public string Details { get; set; } = string.Empty;
        public float Gamma { get; set; }
        public bool HasTransferFunction { get; set; } = false;
    }

    public static bool AnalysisStream(Stream stream, out AnalysisResult result)
    {
        result = new AnalysisResult();
        try
        {
            var codec = SKCodec.Create(stream);
            if (codec == null)
                throw new RenderingException("Invalid image stream.");
            var info = codec.Info;
            result.Width = info.Width;
            result.Height = info.Height;
            result.Details = "Unknown Image.";
            result.ColorSpace = ColorSpaces.Unknown;
            if (info.ColorSpace == null)
            {
                result.Details = "No color info in image.";
                return true;
            }
            if (info.ColorSpace.IsSrgb)
            {
                result.ColorSpace = ColorSpaces.Srgb;
                result.Gamma = 2.2f;
                result.Details = "Standard sRGB.";
            }
            else if (info.ColorSpace.GammaIsLinear)
            {
                result.ColorSpace = ColorSpaces.Linear;
                result.Gamma = 1.0f;
                result.Details = "Linear.";
            }
            else if (info.ColorSpace.GammaIsCloseToSrgb)
            {
                result.ColorSpace = ColorSpaces.OtherSrgbLike;
                result.Gamma = 2.2f;
                result.Details = "Approximate to sRGB(Gamma ≈ 2.2).";
            }
            return true;
        }
        catch (Exception ex)
        {
            result.Details = ex.Message;
            return false;
        }
    }
}