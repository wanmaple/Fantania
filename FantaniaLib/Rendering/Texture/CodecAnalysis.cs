using SkiaSharp;

namespace FantaniaLib;

public enum ColorSpaceTypes
{
    Unknown,
    Linear,
    Srgb,
    Custom,
}

public static class CodecAnalysis
{
    public class AnalysisResult
    {
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public bool HasAlpha { get; set; }
        public bool IsPremultiplied { get; set; }
        public int ChannelCount { get; set; }
        public ColorSpaceTypes ColorSpace { get; set; } = ColorSpaceTypes.Unknown;
        public bool IsSrgb { get; set; }
        public string SourceFormat { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public static byte[]? DecodeBytes(Stream stream, TextureFormats format)
    {
        if (format == TextureFormats.R8) return null;
        try
        {
            var codec = SKCodec.Create(stream);
            if (codec == null)
                return null;
            var info = codec.Info;
            info.ColorType = format switch
            {
                TextureFormats.RGB8 => SKColorType.Rgb888x,
                TextureFormats.RGBA8 => SKColorType.Rgba8888,
                TextureFormats.SRGB8 => SKColorType.Rgb888x,
                TextureFormats.SRGB8_ALPHA8 => SKColorType.Rgba8888,
                _ => SKColorType.Rgba8888,
            };
            info.AlphaType = format switch
            {
                TextureFormats.RGB8 => SKAlphaType.Opaque,
                TextureFormats.RGBA8 => info.AlphaType,
                TextureFormats.SRGB8 => SKAlphaType.Opaque,
                TextureFormats.SRGB8_ALPHA8 => info.AlphaType,
                _ => info.AlphaType,
            };
            SKBitmap bitmap = SKBitmap.Decode(codec, info);
            return bitmap.Bytes;
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
            var codec = SKCodec.Create(stream);
            if (codec == null)
                return false;
            var info = codec.Info;
            result.Width = info.Width;
            result.Height = info.Height;
            AnalyzeColorSpaceForGPU(info, result);
            AnalyzePixelFormatForGpu(info, result);
            return true;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
            return false;
        }
    }

    static void AnalyzeColorSpaceForGPU(SKImageInfo info, AnalysisResult result)
    {
        if (info.ColorSpace == null)
        {
            result.ColorSpace = ColorSpaceTypes.Unknown;
            result.IsSrgb = false;
            return;
        }

        result.IsSrgb = info.ColorSpace.IsSrgb;
        result.ColorSpace = info.ColorSpace.IsSrgb ? ColorSpaceTypes.Srgb :
                           info.ColorSpace.GammaIsLinear ? ColorSpaceTypes.Linear :
                           ColorSpaceTypes.Custom;
    }

    static void AnalyzePixelFormatForGpu(SKImageInfo info, AnalysisResult result)
    {
        result.SourceFormat = $"{info.ColorType} | {info.AlphaType}";
        result.HasAlpha = info.AlphaType != SKAlphaType.Opaque;
        result.IsPremultiplied = info.AlphaType == SKAlphaType.Premul;
        result.ChannelCount = info.ColorType switch
        {
            SKColorType.Gray8 or SKColorType.Alpha8 or SKColorType.Alpha16 => 1,
            SKColorType.Rgb565 or SKColorType.Rgb888x => 3,
            _ => 4,
        };
    }
}