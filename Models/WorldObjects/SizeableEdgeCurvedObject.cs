using System;
using System.Numerics;
using Avalonia.OpenGL;

namespace Fantania.Models;

public class SizeableEdgeCurvedObject : SizeableQuadObject
{
    private Vector4 _edgeScales = Vector4.One;
    [EditVector4, Tooltip("TooltipEdgeScales"), StandardSerialization(1)]
    public Vector4 EdgeScales
    {
        get { return _edgeScales; }
        set
        {
            if (_edgeScales != value)
            {
                OnPropertyChanging(nameof(EdgeScales));
                _edgeScales = value;
                OnPropertyChanged(nameof(EdgeScales));
                CustomData2 = new Vector4(
                    _edgeScales.X,
                    _edgeScales.Y,
                    _edgeScales.Z,
                    _edgeScales.W
                    );
            }
        }
    }

    public SizeableEdgeCurvedObject()
    {
    }

    public SizeableEdgeCurvedObject(DrawTemplate template)
    : base(template)
    {
    }

    public override void OnEnterCanvas(GlInterface gl)
    {
        base.OnEnterCanvas(gl);
        CustomData2 = EdgeScales;
    }
}