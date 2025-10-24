// using System.Linq;
// using System.Numerics;
// using Avalonia.OpenGL;

// namespace Fantania.Models;

// [DataGroup(Group = "DrawTemplates")]
// public class UnlitCurvedSpriteTemplate : DrawTemplate
// {
//     public override RenderLayers Layer => (RenderLayers)RenderLayer;

//     private int _renderLayer = (int)RenderLayers.Platform;
//     [ConstantOption(1000, "Below Characters", -1000, "Below Platforms", -2000, "Platform", -3000, "In front of Platforms", ControlType = typeof(EditOptionControl)), DatabaseInteger, Tooltip("TooltipLayer")]
//     public int RenderLayer
//     {
//         get { return _renderLayer; }
//         set
//         {
//             if (_renderLayer != value)
//             {
//                 int oldLayer = _renderLayer;
//                 OnPropertyChanging(nameof(RenderLayer));
//                 _renderLayer = value;
//                 OnPropertyChanged(nameof(RenderLayer));
//                 RaiseRenderLayerChanged((RenderLayers)oldLayer, (RenderLayers)_renderLayer);
//             }
//         }
//     }

//     private Gradient2D _gradient = Gradient2D.Default;
//     [EditGradient2D, DatabaseGradient2D, Tooltip("TooltipGradientSprite")]
//     public Gradient2D Gradient
//     {
//         get { return _gradient; }
//         set
//         {
//             if (_gradient != value)
//             {
//                 OnPropertyChanging(nameof(Gradient));
//                 _gradient = value;
//                 OnPropertyChanged(nameof(Gradient));
//                 _gradDirty = true;
//             }
//         }
//     }

//     private CurvedEdge _edgeL = CurvedEdge.Flat;
//     [EditCurvedEdge, DatabaseCurvedEdge, Tooltip("TooltipEdgeLeft")]
//     public CurvedEdge EdgeLeft
//     {
//         get { return _edgeL; }
//         set
//         {
//             if (_edgeL != value)
//             {
//                 OnPropertyChanging(nameof(EdgeLeft));
//                 _edgeL = value;
//                 OnPropertyChanged(nameof(EdgeLeft));
//                 _edgeDirties[0] = true;
//             }
//         }
//     }

//     private CurvedEdge _edgeR = CurvedEdge.Flat;
//     [EditCurvedEdge, DatabaseCurvedEdge, Tooltip("TooltipEdgeRight")]
//     public CurvedEdge EdgeRight
//     {
//         get { return _edgeR; }
//         set
//         {
//             if (_edgeR != value)
//             {
//                 OnPropertyChanging(nameof(EdgeRight));
//                 _edgeR = value;
//                 OnPropertyChanged(nameof(EdgeRight));
//                 _edgeDirties[2] = true;
//             }
//         }
//     }

//     private CurvedEdge _edgeT = CurvedEdge.Flat;
//     [EditCurvedEdge, DatabaseCurvedEdge, Tooltip("TooltipEdgeTop")]
//     public CurvedEdge EdgeTop
//     {
//         get { return _edgeT; }
//         set
//         {
//             if (_edgeT != value)
//             {
//                 OnPropertyChanging(nameof(EdgeTop));
//                 _edgeT = value;
//                 OnPropertyChanged(nameof(EdgeTop));
//                 _edgeDirties[1] = true;
//             }
//         }
//     }

//     private CurvedEdge _edgeB = CurvedEdge.Flat;
//     [EditCurvedEdge, DatabaseCurvedEdge, Tooltip("TooltipEdgeBottom")]
//     public CurvedEdge EdgeBottom
//     {
//         get { return _edgeB; }
//         set
//         {
//             if (_edgeB != value)
//             {
//                 OnPropertyChanging(nameof(EdgeBottom));
//                 _edgeB = value;
//                 OnPropertyChanged(nameof(EdgeBottom));
//                 _edgeDirties[3] = true;
//             }
//         }
//     }

//     private int _outlineSize = 0;
//     [EditInteger(0, 8, ControlType = typeof(UpDownIntegerControl)), DatabaseInteger, Tooltip("TooltipOutlineSize")]
//     public int OutlineSize
//     {
//         get { return _outlineSize; }
//         set
//         {
//             if (_outlineSize != value)
//             {
//                 OnPropertyChanging(nameof(OutlineSize));
//                 _outlineSize = value;
//                 OnPropertyChanged(nameof(OutlineSize));
//             }
//         }
//     }

//     private Vector4 _outlineColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
//     [EditColor, DatabaseVector4, Tooltip("TooltipOutlineColor")]
//     public Vector4 OutlineColor
//     {
//         get { return _outlineColor; }
//         set
//         {
//             if (_outlineColor != value)
//             {
//                 OnPropertyChanging(nameof(OutlineColor));
//                 _outlineColor = value;
//                 OnPropertyChanged(nameof(OutlineColor));
//             }
//         }
//     }

//     public override WorldObject OnCreateWorldObject()
//     {
//         var obj = new SizeableEdgeCurvedObject(this);
//         return obj;
//     }

//     public override RenderMaterial MaterialOfPassId(int passId)
//     {
//         return BuiltinMaterials.Singleton[BuiltinMaterials.CURVED_EDGE];
//     }

//     public override void Draw(GlInterface gl, RenderPass pass, RenderMaterial material, int indiceNum)
//     {
//         CheckTextures(gl);
//         Vector4 value = new Vector4(OutlineColor.X, OutlineColor.Y, OutlineColor.Z, OutlineSize);
//         material.SetUniform("uOutline", value);
//         material.SetUniform("uAmplitudes", new Vector4(EdgeLeft.CurveAmplitude, EdgeTop.CurveAmplitude, EdgeRight.CurveAmplitude, EdgeBottom.CurveAmplitude));
//         material.SetAdditionalTexture("uGradient", 1, _texGradient.ID, GlConsts.GL_TEXTURE_2D);
//         material.SetAdditionalTexture("uEdges", 2, _texEdgeNoise.ID, OpenGLApiEx.GL_TEXTURE_1D);
//         base.Draw(gl, pass, material, indiceNum);
//     }

//     unsafe void CheckTextures(GlInterface gl)
//     {
//         const int SIZE2D = 512;
//         if (_gradDirty)
//         {
//             if (_texGradient == null)
//             {
//                 _texGradient = new GPUTexture2D(SIZE2D, SIZE2D, OpenGLApiEx.GL_REPEAT);
//                 _texGradient.Prepare(gl);
//             }
//             byte* data = stackalloc byte[SIZE2D * SIZE2D * 4];
//             for (int x = 0; x < SIZE2D; x++)
//             {
//                 for (int y = 0; y < SIZE2D; y++)
//                 {
//                     float sampleX = (x + 0.5f) / SIZE2D;
//                     float sampleY = (y + 0.5f) / SIZE2D;
//                     int offset = (y * SIZE2D + x) * 4;
//                     Vector4 color = Gradient.Evaluate(new Vector2(sampleX, sampleY));
//                     data[offset] = MathHelper.ToByte(color.X);
//                     data[offset + 1] = MathHelper.ToByte(color.Y);
//                     data[offset + 2] = MathHelper.ToByte(color.Z);
//                     data[offset + 3] = MathHelper.ToByte(color.W);
//                 }
//             }
//             _texGradient.SetData(gl, data);
//             _gradDirty = false;
//         }
//         const int SIZE1D = 128;
//         if (_edgeDirties.Any(v => v))
//         {
//             if (_texEdgeNoise == null)
//             {
//                 _texEdgeNoise = new GPUTexture1D(SIZE1D, OpenGLApiEx.GL_CLAMP_TO_EDGE);
//                 _texEdgeNoise.Prepare(gl);
//             }
//             if (_dataEdgeNoise == null) _dataEdgeNoise = new byte[SIZE1D * 4];
//             INoise1D noiseL = null, noiseR = null, noiseT = null, noiseB = null;
//             if (_edgeDirties[0])
//                 noiseL = new Noise1D(new BrownGradient1D(1.0f, SIZE1D, 0.5f, new Mulberry32RNG(EdgeLeft.NoiseSeed)));
//             if (_edgeDirties[1])
//                 noiseT = new Noise1D(new BrownGradient1D(1.0f, SIZE1D, 0.5f, new Mulberry32RNG(EdgeTop.NoiseSeed)));
//             if (_edgeDirties[2])
//                 noiseR = new Noise1D(new BrownGradient1D(1.0f, SIZE1D, 0.5f, new Mulberry32RNG(EdgeRight.NoiseSeed)));
//             if (_edgeDirties[3])
//                 noiseB = new Noise1D(new BrownGradient1D(1.0f, SIZE1D, 0.5f, new Mulberry32RNG(EdgeBottom.NoiseSeed)));
//             for (int i = 0; i < SIZE1D; i++)
//             {
//                 if (_edgeDirties[0])
//                 {
//                     float noise = noiseL.Get(i);
//                     float curve = EdgeLeft.Curve.Evaluate((float)i / (SIZE1D - 1));
//                     float final = MathHelper.Clamp(curve + noise * (float)EdgeLeft.NoiseAmplitude, 0.0f, 1.0f);
//                     _dataEdgeNoise[i * 4] = MathHelper.ToByte(final);
//                 }
//                 if (_edgeDirties[1])
//                 {
//                     float noise = noiseT.Get(i);
//                     float curve = EdgeTop.Curve.Evaluate((float)i / (SIZE1D - 1));
//                     float final = MathHelper.Clamp(curve + noise * (float)EdgeTop.NoiseAmplitude, 0.0f, 1.0f);
//                     _dataEdgeNoise[i * 4 + 1] = MathHelper.ToByte(final);
//                 }
//                 if (_edgeDirties[2])
//                 {
//                     float noise = noiseR.Get(i);
//                     float curve = EdgeRight.Curve.Evaluate((float)i / (SIZE1D - 1));
//                     float final = MathHelper.Clamp(curve + noise * (float)EdgeRight.NoiseAmplitude, 0.0f, 1.0f);
//                     _dataEdgeNoise[i * 4 + 2] = MathHelper.ToByte(final);
//                 }
//                 if (_edgeDirties[3])
//                 {
//                     float noise = noiseB.Get(i);
//                     float curve = EdgeBottom.Curve.Evaluate((float)i / (SIZE1D - 1));
//                     float final = MathHelper.Clamp(curve + noise * (float)EdgeBottom.NoiseAmplitude, 0.0f, 1.0f);
//                     _dataEdgeNoise[i * 4 + 3] = MathHelper.ToByte(final);
//                 }
//             }
//             fixed (byte* data = _dataEdgeNoise)
//                 _texEdgeNoise.SetData(gl, data);
//             for (int i = 0; i < _edgeDirties.Length; i++)
//             {
//                 _edgeDirties[i] = false;
//             }
//         }
//     }

//     bool[] _edgeDirties = { true, true, true, true, };  // Left, Top, Right, Bottom
//     bool _gradDirty = true;
//     GPUTexture1D _texEdgeNoise = null;
//     GPUTexture2D _texGradient = null;
//     byte[] _dataEdgeNoise = null;
// }