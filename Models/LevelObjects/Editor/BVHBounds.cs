using System.Numerics;
using Avalonia.OpenGL;

namespace Fantania.Models;

public class BVHBounds : QuadObject, IPoolable
{
    public override bool InSpacePartition => false;

    public BVHBounds()
    : base(Selection.SelectionTemplate.Default)
    {
        Anchor = Vector2.Zero;
    }

    ~BVHBounds()
    {
    }

    public void OnPooled()
    {
    }

    public void OnRecycled(params object[] args)
    {
        Size = Avalonia.Vector.Zero;
    }

    public override void OnEnterCanvas(GlInterface gl)
    {
    }

    protected override void UpdateCustomData()
    {
    }
}