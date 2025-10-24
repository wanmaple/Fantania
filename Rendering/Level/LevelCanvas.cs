using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class LevelCanvas : OpenGLCanvas
{
    public Level Level => _level;

    public Matrix3x3 ViewMatrix => _viewMatrix;
    public Vector2 ViewTranslation => new Vector2(_viewMatrix.m20, _viewMatrix.m21);

    public IRenderer BackgroundsRenderer => _bgsRenderer;
    public IRenderer ForegroundsRenderer => _fgsRenderer;

    public static readonly StyledProperty<float> ViewScaleProperty = AvaloniaProperty.Register<LevelCanvas, float>(nameof(ViewScale), defaultValue: 1.0f);
    public float ViewScale
    {
        get => GetValue(ViewScaleProperty);
        private set => SetValue(ViewScaleProperty, value);
    }

    public static readonly StyledProperty<Avalonia.Vector> PointerLevelPositionProperty = AvaloniaProperty.Register<LevelCanvas, Avalonia.Vector>(nameof(PointerLevelPosition), defaultValue: Avalonia.Vector.Zero);
    public Avalonia.Vector PointerLevelPosition
    {
        get => GetValue(PointerLevelPositionProperty);
        set => SetValue(PointerLevelPositionProperty, value);
    }

    public static readonly StyledProperty<double> SelectionLeftProperty = AvaloniaProperty.Register<LevelCanvas, double>(nameof(SelectionLeft), defaultValue: 0.0);
    public double SelectionLeft
    {
        get => GetValue(SelectionLeftProperty);
        set => SetValue(SelectionLeftProperty, value);
    }

    public static readonly StyledProperty<double> SelectionTopProperty = AvaloniaProperty.Register<LevelCanvas, double>(nameof(SelectionTop), defaultValue: 0.0);
    public double SelectionTop
    {
        get => GetValue(SelectionTopProperty);
        set => SetValue(SelectionTopProperty, value);
    }

    public static readonly StyledProperty<double> SelectionWidthProperty = AvaloniaProperty.Register<LevelCanvas, double>(nameof(SelectionWidth), defaultValue: 0.0);
    public double SelectionWidth
    {
        get => GetValue(SelectionWidthProperty);
        set => SetValue(SelectionWidthProperty, value);
    }

    public static readonly StyledProperty<double> SelectionHeightProperty = AvaloniaProperty.Register<LevelCanvas, double>(nameof(SelectionHeight), defaultValue: 0.0);
    public double SelectionHeight
    {
        get => GetValue(SelectionHeightProperty);
        set => SetValue(SelectionHeightProperty, value);
    }

    public LevelCanvas(Level lv)
    {
        _level = lv;
        _lvObjRenderer = new LevelObjectRenderer(this);
        AddRenderer(_lvObjRenderer);
        AddCanvasInputHandler(new LevelCanvasInputHandler(this));
    }

    public void TranslateView(Vector2 translation)
    {
        _viewMatrix = Matrix3x3.CreateTranslation(translation) * _viewMatrix;
        _viewDirty = true;
    }

    public void ScaleView(float scale)
    {
        float finalScale = ViewScale * scale;
        finalScale = Math.Clamp(finalScale, 0.25f, 4.0f);
        scale = finalScale / ViewScale;
        if (scale != 1.0f)
        {
            _viewMatrix = Matrix3x3.CreateScale(new Vector2(scale, scale)) * _viewMatrix;
            ViewScale = _viewMatrix.m00;
            _viewDirty = true;
        }
    }

    public void ResetScale()
    {
        _viewMatrix = Matrix3x3.CreateTranslation(ViewTranslation);
        ViewScale = 1.0f;
        _viewDirty = true;
    }

    protected override void OnGLInitialized(GlInterface gl)
    {
        base.OnGLInitialized(gl);
        _level.OnInitialized();
        _bgsRenderer = new BackgroundsRenderer(WorkspaceViewModel.Current.Workspace.CurrentStylegrounds);
        _fgsRenderer = new ForegroundsRenderer(WorkspaceViewModel.Current.Workspace.CurrentStylegrounds);
        AddRenderer(_bgsRenderer);
        AddRenderer(_fgsRenderer);
        WorkspaceViewModel.Current.Workspace.LevelChanged += OnLevelChanged;
    }

    protected override void OnGLFinalized(GlInterface gl)
    {
        WorkspaceViewModel.Current.Workspace.LevelChanged -= OnLevelChanged;
        base.OnGLFinalized(gl);
    }

    protected override void OnGLRenderColor(GlInterface gl)
    {
        Rect viewRect = GetViewRect();
        // check current visible objects.
        if (_viewDirty)
        {
            _lvObjRenderer.OnViewChanged(viewRect);
            _viewDirty = false;
        }
        base.OnGLRenderColor(gl);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        _viewDirty = true;
        base.OnSizeChanged(e);
    }

    public Avalonia.Vector CanvasPositionToWorldPosition(Point posToCanvas)
    {
        posToCanvas = new Point(posToCanvas.X, Bounds.Height - posToCanvas.Y);
        Avalonia.Vector movement = CanvasMovementToViewMovement(posToCanvas);
        Avalonia.Vector worldPos = new Avalonia.Vector(movement.X - ViewTranslation.X / ViewScale, movement.Y + ViewTranslation.Y / ViewScale);
        return worldPos;
    }

    public Avalonia.Vector CanvasPositionToGLPosition(Point posToCanvas)
    {
        var ret = CanvasPositionToWorldPosition(posToCanvas);
        return new Avalonia.Vector(ret.X, -ret.Y);
    }

    public Avalonia.Vector CanvasMovementToViewMovement(Point movementToCanvas)
    {
        movementToCanvas = new Point(movementToCanvas.X, -movementToCanvas.Y);
        float designRatio = (float)ColorBufferWidth / ColorBufferHeight;
        float controlRatio = (float)Bounds.Width / (float)Bounds.Height;
        double scaledWidth = ColorBufferWidth / ViewScale;
        double scaledHeight = ColorBufferHeight / ViewScale;
        if (controlRatio >= designRatio)
        {
            double xToColor = movementToCanvas.X / Bounds.Width * scaledWidth;
            double adaptHeight = Bounds.Width / designRatio;
            double yToColor = movementToCanvas.Y / adaptHeight * scaledHeight;
            return new Avalonia.Vector(xToColor, yToColor);
        }
        else
        {
            double adaptWidth = Bounds.Height * designRatio;
            double xToColor = movementToCanvas.X / adaptWidth * scaledWidth;
            double yToColor = movementToCanvas.Y / Bounds.Height * scaledHeight;
            return new Avalonia.Vector(xToColor, yToColor);
        }
    }

    public Rect GetViewRect()
    {
        Avalonia.Vector bl = CanvasPositionToGLPosition(new Point(0.0, Bounds.Height));
        Avalonia.Vector tr = CanvasPositionToGLPosition(new Point(Bounds.Width, 0.0));
        Rect rect = new Rect(new Point(bl.X, bl.Y), new Point(tr.X, tr.Y));
        return rect;
    }

    void OnLevelChanged(Level lv)
    {
        string oldGroup = lv.Group;
        RemoveRenderer(_lvObjRenderer);
        _level = lv;
        _level.OnInitialized();
        _lvObjRenderer = new LevelObjectRenderer(this);
        AddRenderer(_lvObjRenderer);
        if (_level.Group != oldGroup)
        {
            RemoveRenderer(_bgsRenderer);
            RemoveRenderer(_fgsRenderer);
            _bgsRenderer = new BackgroundsRenderer(WorkspaceViewModel.Current.Workspace.CurrentStylegrounds);
            _fgsRenderer = new ForegroundsRenderer(WorkspaceViewModel.Current.Workspace.CurrentStylegrounds);
            AddRenderer(_bgsRenderer);
            AddRenderer(_fgsRenderer);
        }
    }

    LevelObjectRenderer _lvObjRenderer;
    BackgroundsRenderer _bgsRenderer;
    ForegroundsRenderer _fgsRenderer;
    Level _level;
    Matrix3x3 _viewMatrix = Matrix3x3.Identity;
    bool _viewDirty = true;
}