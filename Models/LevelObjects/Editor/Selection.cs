using System;
using Avalonia.OpenGL;

namespace Fantania.Models;

public class Selection : QuadObject, IPoolable
{
    [IgnoreDatabase]
    internal class SelectionTemplate : DrawTemplate
    {
        public static readonly SelectionTemplate Default = new SelectionTemplate();

        public override RenderLayers Layer => RenderLayers.Selection;

        public SelectionTemplate()
        {
            _id = -1000;
        }

        public override RenderMaterial MaterialOfPassId(int passId)
        {
            return BuiltinMaterials.Singleton[BuiltinMaterials.SELECTION];
        }

        public override LevelObject OnCreateLevelObject()
        {
            throw new NotImplementedException();
        }
    }

    public LevelObject Target => _target;
    public override bool InSpacePartition => false;

    public Selection(LevelObject target)
    : base(SelectionTemplate.Default)
    {
        RelativeDepth = 999;
        _target = target;
        OnTargetTransformChanged(_target);
        _target.TransformChanged += OnTargetTransformChanged;
    }

    ~Selection()
    {
        _target.TransformChanged -= OnTargetTransformChanged;
    }

    public void OnPooled()
    {
        _target.TransformChanged -= OnTargetTransformChanged;
    }

    public void OnRecycled(params object[] args)
    {
        _target = args[0] as LevelObject;
        OnTargetTransformChanged(_target);
        _target.TransformChanged += OnTargetTransformChanged;
    }

    void OnTargetTransformChanged(LevelObject target)
    {
        Transform = target.Transform;
        _anchor = target.Anchor;
        _size = target.Size;
        _position = target.Position;
        _rotation = target.Rotation;
        _scale = target.Scale;
        _bounds = target.BoundingBox;
        CustomData = new System.Numerics.Vector4((float)(_target.Size.X * Math.Abs(_scale.X)), (float)(_target.Size.Y * Math.Abs(_scale.Y)), (float)_anchor.X, (float)_anchor.Y);
    }

    public override void OnEnterCanvas(GlInterface gl)
    {
        // do nothing since it tracks the target.
    }

    protected override void UpdateCustomData()
    {
    }

    LevelObject _target;
}