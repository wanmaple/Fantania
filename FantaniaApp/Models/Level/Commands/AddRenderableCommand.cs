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
            material.Uniforms.ApplyDesiredUniforms(RenderInfo.Uniforms, context.Workspace, pipeline);
            context.SpaceHierarchy.AddItem(new QuadRenderable(RenderInfo, material));
        }
    }
}