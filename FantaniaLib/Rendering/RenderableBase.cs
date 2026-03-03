using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public abstract class RenderableBase : ObservableObject, IRenderable
{
    public abstract Matrix3x3 Transform { get; set; }
    public abstract int Depth { get; set; }
    public abstract Mesh Mesh { get; }
    public abstract RenderMaterial Material { get; }
    public abstract Rectf BoundingBox { get; }

    public string Stage { get; protected set; } = string.Empty;
    public Vector2 Anchor { get; protected set; } = Vector2.Zero;
    public Vector2 Size { get; protected set; } = Vector2.Zero;
    public Rectf Tiling { get; protected set; } = Rectf.Zero;
    public Rectf Tiling2 { get; protected set; } = Rectf.Zero;
    public Vector4 VertexColor { get; protected set; } = Vector4.One;
    public int EntityOrder { get; set; }
    public int LocalOrder { get; set; }
    public int NodeIndex { get; set; } = -1;

    protected RenderableBase()
    {
    }

    public virtual void OnEnter(IWorkspace workspace, IRenderContext context)
    {
        RenderMaterial material = Material;
        foreach (var name in material.Uniforms.Names)
        {
            var uniform = material.Uniforms[name];
            if (uniform.Type == UniformTypes.Texture)
            {
                var info = uniform.Get<UniformSet.TextureInformation>();
                TextureDefinition def = info.TextureDef;
                int texId = context.TextureManager.FallbackTextureID;
                if (def.TextureType == TextureTypes.Gpu)
                {
                    // GpuTexture应该被它自己的上下文管理，这里我们只取它的TextureID
                    texId = def.TextureParameters.GpuParams.TextureID;
                }
                else
                {
                    ITexture2D? tex = def.ToTexture(workspace.RootFolder);
                    if (tex != null)
                    {
                        texId = context.TextureManager.AcquireTextureID(tex);
                    }
                }
                info.TextureID = texId;
                uniform.Set(info);
                material.MutableUniforms.SetUniform(name, uniform);
            }
            else if (uniform.Type == UniformTypes.TextureArray)
            {
                var info = uniform.Get<UniformSet.TextureArrayInformation>();
                int[] texIds = (int[])info.TextureIDs.Clone();
                for (int i = 0; i < info.TextureDefs.Length; i++)
                {
                    TextureDefinition def = info.TextureDefs[i];
                    int texId = context.TextureManager.FallbackTextureID;
                    if (def.TextureType == TextureTypes.Gpu)
                    {
                        texId = def.TextureParameters.GpuParams.TextureID;
                    }
                    else
                    {
                        ITexture2D? tex = def.ToTexture(workspace.RootFolder);
                        if (tex != null)
                        {
                            texId = context.TextureManager.AcquireTextureID(tex);
                        }
                    }
                    texIds[i] = texId;
                }
                info.TextureIDs = texIds;
                uniform.Set(info);
                material.MutableUniforms.SetUniform(name, uniform);
            }
        }
    }

    public virtual void OnExit(IWorkspace workspace, IRenderContext context)
    {
        RenderMaterial material = Material;
        foreach (var name in material.Uniforms.Names)
        {
            var uniform = material.Uniforms[name];
            if (uniform.Type == UniformTypes.Texture)
            {
                var info = uniform.Get<UniformSet.TextureInformation>();
                TextureDefinition def = info.TextureDef;
                int texId = info.TextureID;
                if (def.TextureType != TextureTypes.Gpu && texId != context.TextureManager.FallbackTextureID)
                {
                    ITexture2D tex = def.ToTexture(workspace.RootFolder)!;
                    context.TextureManager.ReleaseTexture(tex);
                }
                info.TextureID = texId;
                uniform.Set(info);
            }
            else if (uniform.Type == UniformTypes.TextureArray)
            {
                var info = uniform.Get<UniformSet.TextureArrayInformation>();
                for (int i = 0; i < info.TextureDefs.Length; i++)
                {
                    TextureDefinition def = info.TextureDefs[i];
                    int texId = info.TextureIDs[i];
                    if (def.TextureType != TextureTypes.Gpu && texId != context.TextureManager.FallbackTextureID)
                    {
                        ITexture2D tex = def.ToTexture(workspace.RootFolder)!;
                        context.TextureManager.ReleaseTexture(tex);
                    }
                }
            }
        }
    }
}