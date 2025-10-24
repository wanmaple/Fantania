using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.OpenGL;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania.Models;

public enum CreateModes
{
    Place,
    Sizeable,
    Paint,
}

public abstract class WorldObject : ObservableObject, IBVHItem, IRenderable
{
    public event Action<WorldObject> TransformChanged;
    public event Action<WorldObject, RenderLayers, RenderLayers> LayerChanged;
    public event Action<WorldObject> RenderInfoChanged;
    public event Action<WorldObject, bool> SelectStateChanged;
    public event Action<WorldObject> VisibilityChanged;

    public virtual CreateModes CreateMode => CreateModes.Place;
    public virtual bool IsScaleable => true;

    private bool _visible = true;
    public bool IsVisible
    {
        get { return _visible; }
        set
        {
            if (_visible != value)
            {
                _visible = value;
                RaiseVisibilityChanged();
            }
        }
    }

    private Matrix3x3 _transform = Matrix3x3.Identity;
    public Matrix3x3 Transform
    {
        get { return _transform; }
        set
        {
            if (_transform != value)
            {
                _transform = value;
                RaiseRenderInfoChanged();
            }
        }
    }

    public int RealDepth => (int)_template.Layer - _depth;

    private int _depth = 499;
    [EditInteger(0, 999, ControlType = typeof(UpDownIntegerControl)), Tooltip("TooltipRelativeDepth"), StandardSerialization(1)]
    public int RelativeDepth
    {
        get { return _depth; }
        set
        {
            if (_depth != value)
            {
                OnPropertyChanging(nameof(RelativeDepth));
                _depth = value;
                OnPropertyChanged(nameof(RelativeDepth));
                RaiseRenderInfoChanged();
            }
        }
    }

    private System.Numerics.Vector4 _vertColor = System.Numerics.Vector4.One;
    [EditColor, Tooltip("TooltipVertexColor"), StandardSerialization(1)]
    public System.Numerics.Vector4 VertexColor
    {
        get { return _vertColor; }
        set
        {
            if (_vertColor != value)
            {
                OnPropertyChanging(nameof(VertexColor));
                _vertColor = value;
                OnPropertyChanged(nameof(VertexColor));
                RaiseRenderInfoChanged();
            }
        }
    }

    private System.Numerics.Vector4 _tiling = new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 0.0f);
    [EditVector4, Tooltip("TooltipTiling"), StandardSerialization(1)]
    public System.Numerics.Vector4 Tiling
    {
        get { return _tiling; }
        set
        {
            if (_tiling != value)
            {
                OnPropertyChanging(nameof(Tiling));
                _tiling = value;
                OnPropertyChanged(nameof(Tiling));
                RaiseRenderInfoChanged();
            }
        }
    }

    private System.Numerics.Vector4 _customData = System.Numerics.Vector4.Zero;
    public System.Numerics.Vector4 CustomData
    {
        get { return _customData; }
        set
        {
            if (_customData != value)
            {
                _customData = value;
                OnPropertyChanged(nameof(CustomData));
                RaiseRenderInfoChanged();
            }
        }
    }

    private System.Numerics.Vector4 _customData2 = System.Numerics.Vector4.Zero;
    public System.Numerics.Vector4 CustomData2
    {
        get { return _customData2; }
        set
        {
            if (_customData2 != value)
            {
                _customData2 = value;
                OnPropertyChanged(nameof(CustomData2));
                RaiseRenderInfoChanged();
            }
        }
    }

    protected Vector _size = Vector.Zero;
    public Vector Size
    {
        get { return _size; }
        set
        {
            if (_size != value)
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
                UpdateTransform();
                UpdateCustomData();
            }
        }
    }

    public virtual bool InSpacePartition => _template.ID > 0;

    public Rect BoundingBox => _bounds;

    protected DrawTemplate _template;
    [StandardSerialization(1)]
    public DrawTemplate Template
    {
        get => _template;
        set => _template = value;
    }

    protected Vector _anchor = new Vector(0.5, 1.0);
    [EditAnchor, Tooltip("TooltipAnchor"), StandardSerialization(1)]
    public Vector Anchor
    {
        get { return _anchor; }
        set
        {
            if (_anchor != value)
            {
                OnPropertyChanging(nameof(Anchor));
                _anchor = value;
                OnPropertyChanged(nameof(Anchor));
                UpdateTransform();
            }
        }
    }

    public Vector GLPosition => new Vector(_position.X, -_position.Y);

    protected Vector _position = Vector.Zero;
    [EditVector2, Tooltip("TooltipPosition"), StandardSerialization(1)]
    public Vector Position
    {
        get { return _position; }
        set
        {
            if (_position != value)
            {
                OnPropertyChanging(nameof(Position));
                _position = value;
                OnPropertyChanged(nameof(Position));
                UpdateTransform();
            }
        }
    }

    protected double _rotation = 0.0;
    [EditAngle, Tooltip("TooltipRotation"), StandardSerialization(1)]
    public double Rotation
    {
        get { return _rotation; }
        set
        {
            if (_rotation != value)
            {
                OnPropertyChanging(nameof(Rotation));
                _rotation = value;
                OnPropertyChanged(nameof(Rotation));
                UpdateTransform();
            }
        }
    }

    protected Vector _scale = Vector.One;
    [EditVector2(Increment = 0.1f), Tooltip("TooltipScale"), StandardSerialization(1)]
    public Vector Scale
    {
        get { return _scale; }
        set
        {
            if (_scale != value)
            {
                OnPropertyChanging(nameof(Scale));
                _scale = value;
                OnPropertyChanged(nameof(Scale));
                UpdateTransform();
                UpdateCustomData();
            }
        }
    }

    internal bool _isSelected = false;
    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                SelectStateChanged?.Invoke(this, _isSelected);
            }
        }
    }

    private string _uuid;
    [Constant, Tooltip("TooltipUUID"), StandardSerialization(1)]
    public string UUID
    {
        get => _uuid;
        set
        {
            if (_uuid != null)
                _allocatedUUIDs.Add(_uuid);
            _uuid = value;
            _allocatedUUIDs.Add(_uuid);
        }
    }

    protected WorldObject()
    {
    }

    protected WorldObject(Workspace workspace)
    {
        do
            _uuid = Guid.NewGuid().ToString();
        while (!_allocatedUUIDs.Add(_uuid));
    }

    ~WorldObject()
    {
        if (_template != null)
        {
            _template.RenderLayerChanged -= OnTemplateLayerChanged;
        }
        _allocatedUUIDs.Remove(_uuid);
    }

    public override string ToString()
    {
        return $"Template: {_template.ID} Anchor: ({_anchor}) Pos: ({_position}) Rot: {double.RadiansToDegrees(_rotation)} Scale: ({_scale}) Size: ({_size})";
    }

    /// <summary>
    /// 从Placements列表中选中后鼠标进入Canvas时触发
    /// </summary>
    public virtual void OnReadyAdding(World world) { }
    /// <summary>
    /// 从Placements列表中选中后鼠标移出Canvas时触发
    /// </summary>
    public virtual void OnCancelAdding(World world) { }
    /// <summary>
    /// 从Placements列表中选中后鼠标点击放置时触发
    /// </summary>
    public virtual void OnPlaceFromAdding(World world) { }
    /// <summary>
    /// 进入BVH时触发
    /// </summary>
    public virtual void OnEnter(World world)
    {
        _template.RenderLayerChanged += OnTemplateLayerChanged;
    }
    /// <summary>
    /// 移出BVH时触发
    /// </summary>
    public virtual void OnExit(World world)
    {
        _template.RenderLayerChanged -= OnTemplateLayerChanged;
    }

    /// <summary>
    /// 添加至OpenGL Canvas时触发(绘制流程)
    /// </summary>
    public virtual void OnEnterCanvas(GlInterface gl)
    {
        if (_template != null)
        {
            Size = _template.GetRenderSize(gl);
        }
        else
        {
            Size = Vector.Zero;
        }
    }

    /// <summary>
    /// 移出OpenGL Canvas时触发(绘制流程)
    /// </summary>
    public virtual void OnExitCanvas(GlInterface gl) { }

    /// <summary>
    /// 平移快捷键触发
    /// </summary>
    public virtual void OnTranslate(Vector translation)
    {
        Position = Position + translation;
    }

    /// <summary>
    /// 旋转快捷键触发
    /// </summary>
    public virtual void OnRotate(double rotation)
    {
        Rotation += double.DegreesToRadians(rotation);
        Rotation = (Rotation + Math.PI) % (Math.PI * 2.0) - Math.PI;
    }

    /// <summary>
    /// 缩放快捷键触发
    /// </summary>
    public virtual void OnScale(Vector scale)
    {
        if (!IsScaleable) return;
        Scale = new Vector(Scale.X + scale.X, Scale.Y + scale.Y);
    }

    /// <summary>
    /// 点测试用，判断点击时是否真的命中(并非BVH的AABB框)
    /// </summary>
    public abstract bool PointTestInExactBounds(Point pt);
    /// <summary>
    /// 计算BVH读取的AABB框
    /// </summary>
    protected abstract void CalculateBounds(Matrix3x3 transform);

    /// <summary>
    /// 计算World Transform
    /// </summary>
    protected virtual void UpdateTransform()
    {
        // Transform矩阵可以分解成四部分，首先进行Anchor相关的平移，然后进行缩放，然后进行旋转，最后进行世界位置的平移。
        // 还需要注意的是这个Transform要用在OpenGL坐标系下，游戏的坐标系是以左上角为原点，而OpenGL是以左下角为原点。
        Matrix3x3 mat = Matrix3x3.Identity;
        Vector anchorGL = new Vector(_anchor.X, 1.0 - _anchor.Y);
        if (anchorGL != Vector.Zero)
        {
            mat = Matrix3x3.CreateTranslation(new System.Numerics.Vector2((float)-anchorGL.X * (float)_size.X, (float)-anchorGL.Y * (float)_size.Y));
        }
        if (_scale != Vector.One)
        {
            mat = Matrix3x3.CreateScale(_scale.ToVector2()) * mat;
        }
        if (_rotation != 0.0)
        {
            mat = Matrix3x3.CreateRotation((float)_rotation) * mat;
        }
        if (_position != Vector.Zero)
        {
            // 记得将y轴翻转
            mat = Matrix3x3.CreateTranslation(GLPosition.ToVector2()) * mat;
        }
        CalculateBounds(mat);
        Transform = mat;
        RaiseTransformChanged();
    }

    protected virtual void UpdateCustomData()
    {
        CustomData = new System.Numerics.Vector4((float)(Size.X * Scale.X), (float)(Size.Y * Scale.Y), 0.0f, 0.0f);
    }

    void OnTemplateLayerChanged(RenderLayers oldLayer, RenderLayers newLayer)
    {
        LayerChanged?.Invoke(this, oldLayer, newLayer);
    }

    protected void RaiseLayerChanged(RenderLayers oldLayer, RenderLayers newLayer)
    {
        LayerChanged?.Invoke(this, oldLayer, newLayer);
    }

    protected void RaiseTransformChanged()
    {
        TransformChanged?.Invoke(this);
    }

    protected void RaiseRenderInfoChanged()
    {
        RenderInfoChanged?.Invoke(this);
    }

    protected void RaiseVisibilityChanged()
    {
        VisibilityChanged?.Invoke(this);
    }

    internal Rect _bounds;

    static HashSet<string> _allocatedUUIDs = new HashSet<string>(1000);
}