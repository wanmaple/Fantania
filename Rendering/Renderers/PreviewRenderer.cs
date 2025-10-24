using System;
using Avalonia.OpenGL;

namespace Fantania;

public abstract class PreviewRenderer : IRenderer
{
    public event Action PriorityChanged;

    private int _priority = 0;
    public int Priority
    {
        get { return _priority; }
        set
        {
            if (_priority != value)
            {
                _priority = value;
                PriorityChanged?.Invoke();
            }
        }
    }

    public int RefreshDurationInMs { get; set; } = 500;

    public bool IsEnabled { get; set; } = true;

    protected PreviewRenderer(GPUTexture2D texture, RenderMaterial material)
    {
        _verts = new FullScreenVertices();
        _texture = texture;
        _material = material;
    }

    public virtual void Finalize(GlInterface gl)
    {
        _verts.Dispose(gl);
        _texture.Dispose(gl);
    }

    public virtual void Initialize(GlInterface gl)
    {
        _verts.Prepare(gl);
        _texture.Prepare(gl);
        _dirty = true;
    }

    public void Render(GlInterface gl)
    {
        var now = DateTime.Now;
        TimeSpan ts = now - _lastDt;
        if (ts.Milliseconds >= RefreshDurationInMs)
        {
            if (_dirty)
            {
                RefreshTexture(gl);
                _dirty = false;
            }
            _lastDt = now;
        }
        _verts.Use(gl);
        _material.SetMainTexture(_texture.ID);
        SetupMaterial(_material);
        _material.Use(gl);
        gl.DrawElements(GlConsts.GL_TRIANGLES, 6, GlConsts.GL_UNSIGNED_SHORT, 0);
    }

    protected byte ToByte(float value)
    {
        return (byte)MathHelper.RoundToInt(value * 255);
    }

    protected abstract void RefreshTexture(GlInterface gl);
    protected virtual void SetupMaterial(RenderMaterial material) { }

    protected FullScreenVertices _verts;
    protected GPUTexture2D _texture;
    protected RenderMaterial _material;
    protected bool _dirty = true;

    DateTime _lastDt = DateTime.Now;
}
