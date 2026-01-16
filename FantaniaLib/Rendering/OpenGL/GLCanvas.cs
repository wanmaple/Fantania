using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;

namespace FantaniaLib;

public abstract class GLCanvas : OpenGlControlBase, ICustomHitTest
{
    public bool IsValid => _pipeline != null;

    protected virtual void OnContextInitializing(ConfigurableRenderPipeline pipeline)
    {
    }

    protected virtual void OnContextFinalizing(ConfigurableRenderPipeline pipeline)
    {
    }

    protected virtual void OnRendering(ConfigurableRenderPipeline pipeline, int finalFbo)
    {
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        bool valid = false;
        // Make sure the OpenGL is over 3.3
        if (GlVersion.Major >= 4 || (GlVersion.Major == 3 && GlVersion.Minor >= 3))
        {
            valid = true;
        }
        if (!valid) return;
        var device = new GLDevice(gl);
        _pipeline = new ConfigurableRenderPipeline(device);
        OnContextInitializing(_pipeline);
        _pipeline.Device.CheckError();
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        if (!IsValid) return;
        OnContextFinalizing(_pipeline!);
        _pipeline!.Dispose();
        _pipeline = null;
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (!IsValid) return;
        OnRendering(_pipeline!, fb);
        _pipeline!.Device.CheckError();
    }

    protected override void OnOpenGlLost()
    {
        if (IsValid)
        {
            OnContextFinalizing(_pipeline!);
            _pipeline!.Dispose();
            _pipeline = null;
        }
    }

    public virtual bool HitTest(Point point)
    {
        if (!IsValid) return false;
        return point.X >= 0.0 && point.X <= Bounds.Width && point.Y >= 0.0 && point.Y <= Bounds.Height;
    }

    ConfigurableRenderPipeline? _pipeline;
}