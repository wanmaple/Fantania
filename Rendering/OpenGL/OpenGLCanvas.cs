using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using static Avalonia.OpenGL.GlConsts;

namespace Fantania;

public abstract class OpenGLCanvas : OpenGlControlBase, ICustomHitTest
{
    public static readonly StyledProperty<int> ColorBufferWidthProperty = AvaloniaProperty.Register<OpenGLCanvas, int>(nameof(ColorBufferWidth), defaultValue: 1920);
    public int ColorBufferWidth
    {
        get => GetValue(ColorBufferWidthProperty);
        set
        {
            SetValue(ColorBufferWidthProperty, value);
            _colorBufferDirty = true;
        }
    }

    public static readonly StyledProperty<int> ColorBufferHeightProperty = AvaloniaProperty.Register<OpenGLCanvas, int>(nameof(ColorBufferHeight), defaultValue: 1080);
    public int ColorBufferHeight
    {
        get => GetValue(ColorBufferHeightProperty);
        set
        {
            SetValue(ColorBufferHeightProperty, value);
            _colorBufferDirty = true;
        }
    }

    public bool IsValid { get; private set; } = false;
    public ulong FrameCount => _frames;

    protected OpenGLCanvas()
    {
        IsHitTestVisible = true;
        Focusable = true;
        _vertices = new FullScreenVertices();
        _matFinalBlit = BuiltinMaterials.Singleton[BuiltinMaterials.FINAL_BLIT];
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
    }

    void OnTick(TimeSpan dt)
    {
        foreach (var handler in _inputHandlers)
        {
            handler.Tick(dt);
        }
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        // Make sure the OpenGL is over 3.3
        if (GlVersion.Major >= 4 || (GlVersion.Major == 3 && GlVersion.Minor >= 3))
        {
            IsValid = true;
        }
        if (!IsValid)
            return;
        OnGLInitializing(gl);
        OpenGLHelper.CheckError(gl);
        _frameBuffers[0] = new FrameBuffer(gl, ColorBufferWidth, ColorBufferHeight);
        _frameBuffers[1] = new FrameBuffer(gl, ColorBufferWidth, ColorBufferHeight);
        OpenGLHelper.CheckError(gl);
        _colorBufferDirty = false;
        _vertices.Prepare(gl);
        OnGLInitialized(gl);
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        if (!IsValid)
            return;
        OnGLFinalizing(gl);
        foreach (var renderer in _renderers)
        {
            renderer.Finalize(gl);
        }
        for (int i = 0; i < _frameBuffers.Length; i++)
        {
            _frameBuffers[i].Dispose(gl);
            _frameBuffers[i] = null;
        }
        _vertices.Dispose(gl);
        OnGLFinalized(gl);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fbo)
    {
        if (!IsValid)
            return;
        RequestNextFrameRendering();
        OpenGLHelper.CheckError(gl);
        if (_colorBufferDirty)
        {
            for (int i = 0; i < _frameBuffers.Length; i++)
            {
                _frameBuffers[i].Resize(gl, ColorBufferWidth, ColorBufferHeight);
            }
            _colorBufferDirty = false;
        }
        FrameBuffer fb = _frameBuffers[_currentFrameBuffer];
        fb.Bind(gl);
        if (gl.CheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE)
        {
            // Everything is ready.
            gl.Disable(OpenGLApiEx.GL_FRAMEBUFFER_SRGB);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.DepthMask(OpenGLApiEx.GL_TRUE);
            gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            gl.Viewport(0, 0, ColorBufferWidth, ColorBufferHeight);
            SyncRenderers(gl);
            OnGLRenderColor(gl);
            // render color buffer to control buffer
            gl.BindFramebuffer(GL_FRAMEBUFFER, fbo);
            gl.Clear(GL_COLOR_BUFFER_BIT);
            gl.Disable(OpenGLApiEx.GL_FRAMEBUFFER_SRGB);
            var topLevel = TopLevel.GetTopLevel(this);
            double factor = topLevel.RenderScaling;
            int exactWidth = (int)(Bounds.Width * factor);
            int exactHeight = (int)(Bounds.Height * factor);
            gl.Viewport(0, 0, exactWidth, exactHeight);
            float designRatio = (float)ColorBufferWidth / ColorBufferHeight;
            float controlRatio = (float)Bounds.Width / (float)Bounds.Height;
            float s, t;
            if (controlRatio >= designRatio)
            {
                s = 1.0f;
                t = ColorBufferWidth / controlRatio / ColorBufferHeight;
                // gl.BlitFramebuffer(0, 0, ColorBufferWidth, (int)(ColorBufferWidth / controlRatio), 0, 0, exactWidth, exactHeight, GL_COLOR_BUFFER_BIT, GL_LINEAR);
            }
            else
            {
                s = ColorBufferHeight * controlRatio / ColorBufferWidth;
                t = 1.0f;
                // gl.BlitFramebuffer(0, 0, (int)(ColorBufferHeight * controlRatio), ColorBufferHeight, 0, 0, exactWidth, exactHeight, GL_COLOR_BUFFER_BIT, GL_LINEAR);
            }
            _vertices.SetUV(Vector2.Zero, new Vector2(s, 0.0f), new Vector2(s, t), new Vector2(0.0f, t));
            _vertices.Use(gl);
            _matFinalBlit.SetMainTexture(fb.ColorAttachment);
            _matFinalBlit.Use(gl);
            gl.DrawElements(GL_TRIANGLES, _vertices.IndiceCount, GL_UNSIGNED_SHORT, 0);
        }
        else
        {
            gl.BindFramebuffer(GL_FRAMEBUFFER, fbo);
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        }
        OpenGLHelper.CheckError(gl);
        ++_frames;
    }

    void SwapFrameBuffer()
    {
        _currentFrameBuffer = (_currentFrameBuffer + 1) % _frameBuffers.Length;
    }

    protected virtual void OnGLInitializing(GlInterface gl)
    {
    }
    protected virtual void OnGLInitialized(GlInterface gl)
    {
    }
    protected virtual void OnGLFinalizing(GlInterface gl)
    {
    }
    protected virtual void OnGLFinalized(GlInterface gl) { }
    protected virtual void OnGLRenderColor(GlInterface gl)
    {
        foreach (IRenderer renderer in _renderers)
        {
            if (renderer.IsEnabled)
                renderer.Render(gl);
        }
    }

    public void AddRenderer(IRenderer renderer)
    {
        if (!_removed.Remove(renderer))
            _added.Add(renderer);
    }

    public void RemoveRenderer(IRenderer renderer)
    {
        if (!_added.Remove(renderer))
            _removed.Add(renderer);
    }

    public void ClearRenderers()
    {
        for (int i = _renderers.Count - 1; i >= 0; i--)
        {
            RemoveRenderer(_renderers[i]);
        }
    }

    void SyncRenderers(GlInterface gl)
    {
        bool dirty = _rendererDirty;
        if (_removed.Count > 0)
        {
            foreach (var renderer in _removed)
            {
                if (_renderers.Remove(renderer))
                {
                    renderer.PriorityChanged -= OnRendererPriorityChanged;
                    renderer.Finalize(gl);
                    dirty = true;
                }
            }
            _removed.Clear();
        }
        if (_added.Count > 0)
        {
            foreach (var renderer in _added)
            {
                if (!_renderers.Contains(renderer))
                {
                    _renderers.Add(renderer);
                    renderer.Initialize(gl);
                    renderer.PriorityChanged += OnRendererPriorityChanged;
                    dirty = true;
                }
            }
            _added.Clear();
        }
        if (dirty)
        {
            _renderers.Sort(RendererComparer.Default);
        }
        _rendererDirty = false;
    }

    void OnRendererPriorityChanged()
    {
        _rendererDirty = true;
    }

    public void AddCanvasInputHandler(ICanvasInputHandler handler)
    {
        if (handler != null)
            _inputHandlers.Add(handler);
    }

    public void RemoveCanvasInputHandler(ICanvasInputHandler handler)
    {
        if (handler != null)
            _inputHandlers.Remove(handler);
    }

    public void ClearCanvasInputHandlers()
    {
        _inputHandlers.Clear();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        Point relative = e.GetPosition(this);
        foreach (var handler in _inputHandlers)
        {
            if (handler.MouseEnter(relative))
                break;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        _pressing.Clear();
        foreach (var handler in _inputHandlers)
        {
            if (handler.MouseExit())
                break;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        MouseButton button = GetMouseButton(e);
        if (button != MouseButton.None)
        {
            Point relativePos = e.GetPosition(this);
            _pressing[button] = new MouseState
            {
                Button = button,
                Moved = false,
                RelativePosition = relativePos,
            };
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        foreach (MouseButton button in _pressing.Keys)
        {
            var state = _pressing[button];
            switch (button)
            {
                case MouseButton.Left:
                    if (!e.Properties.IsLeftButtonPressed)
                    {
                        foreach (var handler in _inputHandlers)
                        {
                            if (!state.Moved)
                            {
                                if (handler.MouseClick(state.RelativePosition, state.Button, e.KeyModifiers))
                                    break;
                            }
                            else
                            {
                                if (handler.MouseDraggingEnd(state.RelativePosition, state.Button))
                                    break;
                            }
                        }
                        _pressing.Remove(button);
                    }
                    break;
                case MouseButton.Right:
                    if (!e.Properties.IsRightButtonPressed)
                    {
                        foreach (var handler in _inputHandlers)
                        {
                            if (!state.Moved)
                            {
                                if (handler.MouseClick(state.RelativePosition, state.Button, e.KeyModifiers))
                                    break;
                            }
                            else
                            {
                                if (handler.MouseDraggingEnd(state.RelativePosition, state.Button))
                                    break;
                            }
                        }
                        _pressing.Remove(button);
                    }
                    break;
                case MouseButton.Middle:
                    if (!e.Properties.IsMiddleButtonPressed)
                    {
                        foreach (var handler in _inputHandlers)
                        {
                            if (!state.Moved)
                            {
                                if (handler.MouseClick(state.RelativePosition, state.Button, e.KeyModifiers))
                                    break;
                            }
                            else
                            {
                                if (handler.MouseDraggingEnd(state.RelativePosition, state.Button))
                                    break;
                            }
                        }
                        _pressing.Remove(button);
                    }
                    break;
                case MouseButton.XButton1:
                    if (!e.Properties.IsXButton1Pressed)
                    {
                        foreach (var handler in _inputHandlers)
                        {
                            if (!state.Moved)
                            {
                                if (handler.MouseClick(state.RelativePosition, state.Button, e.KeyModifiers))
                                    break;
                            }
                            else
                            {
                                if (handler.MouseDraggingEnd(state.RelativePosition, state.Button))
                                    break;
                            }
                        }
                        _pressing.Remove(button);
                    }
                    break;
                case MouseButton.XButton2:
                    if (!e.Properties.IsXButton2Pressed)
                    {
                        foreach (var handler in _inputHandlers)
                        {
                            if (!state.Moved)
                            {
                                if (handler.MouseClick(state.RelativePosition, state.Button, e.KeyModifiers))
                                    break;
                            }
                            else
                            {
                                if (handler.MouseDraggingEnd(state.RelativePosition, state.Button))
                                    break;
                            }
                        }
                        _pressing.Remove(button);
                    }
                    break;
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        MouseButton button = GetMouseButton(e);
        Point relativePos = e.GetPosition(this);
        if (button != MouseButton.None)
        {
            if (_pressing.TryGetValue(button, out var state))
            {
                Point changed = relativePos - state.RelativePosition;
                state.Moved = true;
                state.RelativePosition = relativePos;
                _pressing[button] = state;
                foreach (var handler in _inputHandlers)
                {
                    if (handler.MouseDragging(relativePos, changed, state.Button, e.KeyModifiers))
                        break;
                }
            }
        }

        foreach (var handler in _inputHandlers)
        {
            if (handler.MouseMoving(relativePos, e.KeyModifiers))
                break;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        Point relativePos = e.GetPosition(this);
        foreach (var handler in _inputHandlers)
        {
            if (handler.MouseScrolling(relativePos, e.Delta, e.KeyModifiers))
                break;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        foreach (var handler in _inputHandlers)
        {
            handler.KeyReleased(e.Key, e.KeyModifiers);
        }
    }

    MouseButton GetMouseButton(PointerEventArgs e)
    {
        MouseButton button = MouseButton.None;
        if (e.Properties.IsLeftButtonPressed)
            button = MouseButton.Left;
        else if (e.Properties.IsRightButtonPressed)
            button = MouseButton.Right;
        else if (e.Properties.IsMiddleButtonPressed)
            button = MouseButton.Middle;
        else if (e.Properties.IsXButton1Pressed)
            button = MouseButton.XButton1;
        else if (e.Properties.IsXButton2Pressed)
            button = MouseButton.XButton2;
        return button;
    }

    public bool HitTest(Point point)
    {
        return point.X > 0.0 && point.X < Bounds.Width && point.Y > 0.0 && point.Y < Bounds.Height;
    }

    FrameBuffer[] _frameBuffers = new FrameBuffer[2];
    int _currentFrameBuffer = 0;
    bool _colorBufferDirty = false;
    HashSet<IRenderer> _added = new HashSet<IRenderer>(16);
    HashSet<IRenderer> _removed = new HashSet<IRenderer>(16);
    bool _rendererDirty = false;

    ulong _frames = 0u;
    FullScreenVertices _vertices;
    RenderMaterial _matFinalBlit;

    List<IRenderer> _renderers = new List<IRenderer>(8);
    List<ICanvasInputHandler> _inputHandlers = new List<ICanvasInputHandler>(4);
    Dictionary<MouseButton, MouseState> _pressing = new Dictionary<MouseButton, MouseState>(5);
}