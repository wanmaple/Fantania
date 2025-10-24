using System;
using System.Numerics;
using Avalonia.OpenGL;

namespace Fantania;

public class Noise2DRenderer : PreviewRenderer
{
    private Noise2DLite _noise;
    public Noise2DLite Noise
    {
        get => _noise;
        set
        {
            if (_noise != value)
            {
                _noise = value.Clone();
                _dirty = true;
            }
        }
    }

    public Noise2DRenderer(int width, int height)
    : base(new GPUTexture2D(width, height, OpenGLApiEx.GL_CLAMP_TO_EDGE), BuiltinMaterials.Singleton[BuiltinMaterials.FULLSCREEN_SAMPLE])
    {
    }

    protected unsafe override void RefreshTexture(GlInterface gl)
    {
        byte* data = stackalloc byte[_texture.Width * _texture.Height * 4];
        for (int x = 0; x < _texture.Width; x++)
        {
            for (int y = 0; y < _texture.Height; y++)
            {
                float sampleX = x + 0.5f;
                float sampleY = y + 0.5f;
                float noise = (Noise.Get(sampleX, sampleY, Math.Max(_texture.Width, _texture.Height)) + 1.0f) * 0.5f;
                Vector4 color = (Vector4.One * noise).CrtGamma();
                byte r = ToByte(color.X);
                byte g = ToByte(color.Y);
                byte b = ToByte(color.Z);
                byte a = 255;
                int offset = ((_texture.Height - y - 1) * _texture.Width + x) * 4;
                data[offset] = r;
                data[offset + 1] = g;
                data[offset + 2] = b;
                data[offset + 3] = a;
            }
        }
        _texture.SetData(gl, data);
    }
}