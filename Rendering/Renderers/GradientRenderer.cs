using System.Numerics;
using Avalonia.OpenGL;

namespace Fantania;

public class GradientRenderer : PreviewRenderer
{
    Gradient2D _gradient = Gradient2D.Default;
    public Gradient2D Gradient
    {
        get => _gradient;
        set
        {
            if (_gradient != value)
            {
                _gradient = value;
                _dirty = true;
            }
        }
    }

    bool _blur = false;
    public bool Blur
    {
        get => _blur;
        set
        {
            if (_blur != value)
            {
                _blur = value;
                _material = _blur ? BuiltinMaterials.Singleton[BuiltinMaterials.FULLSCREEN_SAMPLE_BLUR] : BuiltinMaterials.Singleton[BuiltinMaterials.FULLSCREEN_SAMPLE_BLUR];
            }
        }
    }

    public GradientRenderer(int width, int height)
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
                float sampleX = (x + 0.5f) / _texture.Width;
                float sampleY = (y + 0.5f) / _texture.Height;
                Vector4 color = Gradient.Evaluate(new Vector2(sampleX, sampleY));
                byte r = ToByte(color.X);
                byte g = ToByte(color.Y);
                byte b = ToByte(color.Z);
                byte a = ToByte(color.W);
                int offset = ((_texture.Height - y - 1) * _texture.Width + x) * 4;
                data[offset] = r;
                data[offset + 1] = g;
                data[offset + 2] = b;
                data[offset + 3] = a;
            }
        }
        _texture.SetData(gl, data);
    }

    protected override void SetupMaterial(RenderMaterial material)
    {
        if (_blur)
        {
            material.SetUniform("uTextureSize", new Vector2(_texture.Width, _texture.Height));
        }
    }
}