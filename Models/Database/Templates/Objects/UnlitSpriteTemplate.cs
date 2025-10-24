// using System.Numerics;
// using Avalonia.OpenGL;

// namespace Fantania.Models;

// [DataGroup(Group = "DrawTemplates")]
// public class UnlitSpriteTemplate : DrawTemplate
// {
//     public override RenderLayers Layer => (RenderLayers)RenderLayer;

//     private int _renderLayer = (int)RenderLayers.BelowCharacters;
//     [ConstantOption(1000, "Below Characters", -1000, "Below Platforms", -3000, "In front of Platforms", ControlType = typeof(EditOptionControl)), DatabaseInteger, Tooltip("TooltipLayer")]
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

//     private double _cutoff = 0.1;
//     [EditDecimal(0.0, 0.99, ControlType = typeof(SliderDecimalControl)), DatabaseReal, Tooltip("TooltipCutoff")]
//     public double CutOff
//     {
//         get { return _cutoff; }
//         set
//         {
//             if (_cutoff != value)
//             {
//                 OnPropertyChanging(nameof(CutOff));
//                 _cutoff = value;
//                 OnPropertyChanged(nameof(CutOff));
//             }
//         }
//     }

//     public override WorldObject OnCreateWorldObject()
//     {
//         return new QuadObject(this);
//     }

//     public override void Draw(GlInterface gl, RenderPass pass, RenderMaterial material, int indiceNum)
//     {
//         Vector4 value = new Vector4(OutlineColor.X, OutlineColor.Y, OutlineColor.Z, OutlineSize);
//         material.SetUniform("uOutline", value);
//         material.SetUniform("uCutOff", (float)CutOff);
//         base.Draw(gl, pass, material, indiceNum);
//     }
// }