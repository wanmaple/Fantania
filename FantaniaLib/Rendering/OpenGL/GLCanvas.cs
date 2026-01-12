using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;

namespace FantaniaLib;

public abstract class GLCanvas : OpenGlControlBase, ICustomHitTest
{
    public static readonly StyledProperty<int> CanvasWidthProperty = AvaloniaProperty.Register<GLCanvas, int>(nameof(CanvasWidth), defaultValue: 1920);
    public int CanvasWidth
    {
        get => GetValue(CanvasWidthProperty);
        set => SetValue(CanvasWidthProperty, value);
    }

    public static readonly StyledProperty<int> CanvasHeightProperty = AvaloniaProperty.Register<GLCanvas, int>(nameof(CanvasHeight), defaultValue: 1080);
    public int CanvasHeight
    {
        get => GetValue(CanvasHeightProperty);
        set => SetValue(CanvasHeightProperty, value);
    }

    public bool IsValid => _device != null;

    protected virtual void OnContextInitializing(GLDevice device)
    {
    }

    protected virtual void OnContextFinalizing(GLDevice device)
    {
    }

    protected virtual void OnRendering(GLDevice device, int finalFbo)
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
        _device = new GLDevice(gl);
        OnContextInitializing(_device);
        _device.CheckError();
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        if (!IsValid) return;
        OnContextFinalizing(_device!);
        _device = null;
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (!IsValid) return;
        OnRendering(_device!, fb);
        _device!.CheckError();
        RequestNextFrameRendering();
    }

    protected override void OnOpenGlLost()
    {
        if (IsValid)
        {
            OnContextFinalizing(_device!);
            _device = null;
        }
    }

    public bool HitTest(Point point)
    {
        return point.X > 0.0 && point.X < Bounds.Width && point.Y > 0.0 && point.Y < Bounds.Height;
    }

    GLDevice? _device;
}