using ImageMagick;
using SkiaSharp;

namespace FantaniaLib;

public enum ColorSpaceTypes
{
    Unknown,
    Linear,
    Srgb,
    Custom,
}

public enum PixelFormats
{
    RGB,
    RGBA,
}

public static class ImageAnalysis
{
    public class AnalysisResult
    {
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public bool HasAlpha { get; set; }
        public PixelFormats PixelFormat { get; set; }
        public ColorSpaceTypes ColorSpace { get; set; } = ColorSpaceTypes.Unknown;
        public bool IsSrgb { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public static byte[]? DecodeBytes(Stream stream)
    {
        try
        {
            using var image = new MagickImage(stream);
            if (image.HasAlpha)
            {
                image.ColorType = ColorType.TrueColorAlpha;
                return image.GetPixels().ToByteArray(PixelMapping.RGBA);
            }
            image.ColorType = ColorType.TrueColor;
            return image.GetPixels().ToByteArray(PixelMapping.RGB);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool AnalysisStream(Stream stream, out AnalysisResult result)
    {
        result = new AnalysisResult();
        try
        {
            using var image = new MagickImage(stream);
            result.Width = (int)image.Width;
            result.Height = (int)image.Height;
            result.HasAlpha = image.HasAlpha;
            result.PixelFormat = image.HasAlpha ? PixelFormats.RGBA : PixelFormats.RGB;
            var profile = image.GetColorProfile();
            if (profile == null)
            {
                // 没有icc，按照惯例当sRGB处理。
                result.IsSrgb = true;
                result.ColorSpace = ColorSpaceTypes.Srgb;
            }
            else
            {
                var desc = profile.Description?.ToLowerInvariant() ?? string.Empty;
                if (desc.Contains("srgb"))
                {
                    result.IsSrgb = true;
                    result.ColorSpace = ColorSpaceTypes.Srgb;
                }
                else if (desc.Contains("linear"))
                {
                    result.IsSrgb = false;
                    result.ColorSpace = ColorSpaceTypes.Linear;
                }
                else
                {
                    result.IsSrgb = false;
                    result.ColorSpace = ColorSpaceTypes.Custom;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
            return false;
        }
    }
}