using FantaniaLib;

namespace Fantania.Views;

public class RenderableLifePeriod
{
    public RenderableLifePeriod(IWorkspace workspace, IRenderContext context)
    {
        _workspace = workspace;
        _context = context;
    }

    public void Register(BoundingVolumeHierarchy<IRenderable> bvh)
    {
        bvh.ItemAdded += OnRenderableEnter;
        bvh.ItemRemoved += OnRenderableExit;
    }

    public void Unregister(BoundingVolumeHierarchy<IRenderable> bvh)
    {
        bvh.ItemAdded -= OnRenderableEnter;
        bvh.ItemRemoved -= OnRenderableExit;
    }

    void OnRenderableEnter(IRenderable renderable)
    {
        RenderMaterial material = renderable.Material;
        foreach (var name in material.Uniforms.Names)
        {
            var uniform = material.Uniforms[name];
            if (uniform.Type == UniformTypes.Texture)
            {
                var info = uniform.Get<UniformSet.TextureInformation>();
                TextureDefinition def = info.TextureDef;
                int texId = _context.TextureManager.FallbackTextureID;
                if (def.TextureType == TextureTypes.Gpu)
                {
                    // GpuTexture应该被它自己的上下文管理，这里我们只取它的TextureID
                    texId = def.TextureParameters.GpuParams.TextureID;
                }
                else
                {
                    ITexture2D? tex = def.ToTexture(_workspace.RootFolder);
                    if (tex != null)
                    {
                        texId = _context.TextureManager.AcquireTextureID(tex);
                    }
                }
                info.TextureID = texId;
                uniform.Set(info);
                material.Uniforms.SetUniform(name, uniform);
            }
        }
    }

    void OnRenderableExit(IRenderable renderable)
    {
        RenderMaterial material = renderable.Material;
        foreach (var name in material.Uniforms.Names)
        {
            var uniform = material.Uniforms[name];
            if (uniform.Type == UniformTypes.Texture)
            {
                var info = uniform.Get<UniformSet.TextureInformation>();
                TextureDefinition def = info.TextureDef;
                int texId = info.TextureID;
                if (def.TextureType != TextureTypes.Gpu && texId != _context.TextureManager.FallbackTextureID)
                {
                    ITexture2D tex = def.ToTexture(_workspace.RootFolder)!;
                    _context.TextureManager.ReleaseTexture(tex);
                }
                info.TextureID = texId;
                uniform.Set(info);
            }
        }
    }

    IWorkspace _workspace;
    IRenderContext _context;
}