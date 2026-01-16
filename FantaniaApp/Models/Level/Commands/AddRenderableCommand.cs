using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public class AddRenderableCommand : ICanvasCommand
{
    public RenderInfo RenderInfo { get; private set; }

    public AddRenderableCommand(RenderInfo info)
    {
        RenderInfo = info;
    }

    public void Execute(LevelRenderContext context, ConfigurableRenderPipeline pipeline)
    {
        RenderMaterial? material = pipeline.MaterialSet.GetMaterial(RenderInfo.MaterialKey);
        if (material != null)
        {
            foreach (var pair in RenderInfo.Uniforms)
            {
                if (pair.Value is TextureDefinition def)
                {
                    var tex = def.ToTexture(context.Workspace.RootFolder);
                    if (tex != null)
                    {
                        RenderInfo.Size = new Vector2(tex.Width, tex.Height);
                        int texId = pipeline.TextureManager.AcquireTextureID(tex);
                        material.SetUniform(pair.Key, (0, texId));
                    }
                }
                else
                    material.SetUniformVar(pair.Key, pair.Value);
            }
            context.SpaceHierarchy.AddItem(new QuadRenderable(RenderInfo, material));
        }
    }
}