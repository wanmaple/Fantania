using System;
using Avalonia;
using Avalonia.OpenGL;

namespace Fantania.Models;

public abstract class DrawTemplate : DatabaseObject
{
    public event Action<RenderLayers, RenderLayers> RenderLayerChanged;

    public override string IconPath => _texture;

    public virtual int RenderPassCount => 1;
    public virtual RenderLayers Layer => RenderLayers.Characters;

    private string _texture = string.Empty;
    [EditString(ControlType = typeof(ImagePickerControl)), DatabaseString, Tooltip("TooltipMainTexture")]
    public string MainTexture
    {
        get { return _texture; }
        set
        {
            if (_texture != value)
            {
                OnPropertyChanging(nameof(MainTexture));
                _texture = value;
                OnPropertyChanged(nameof(MainTexture));
                OnPropertyChanged(nameof(IconPath));
            }
        }
    }

    private bool _transparent = false;
    [EditBoolean, DatabaseBoolean, Tooltip("TooltipTransparent")]
    public bool Transparent
    {
        get { return _transparent; }
        set
        {
            if (_transparent != value)
            {
                OnPropertyChanging(nameof(Transparent));
                _transparent = value;
                OnPropertyChanged(nameof(Transparent));
            }
        }
    }

    protected DrawTemplate()
    {
    }

    public abstract WorldObject OnCreateWorldObject();

    public virtual IBatchBuilder BatchBuilderOfPassId(int passId)
    {
        return BuiltinBatchBuilders.Singleton[BuiltinBatchBuilders.QUAD_BATCH];
    }

    public virtual RenderMaterial MaterialOfPassId(int passId)
    {
        return BuiltinMaterials.Singleton[BuiltinMaterials.STANDARD_BATCHED];
    }

    public virtual void OnBatchAdded(RenderPass pass, IRenderable renderable)
    {
        pass.AddRenderable(renderable);
    }

    public virtual void OnBatchRemoved(RenderPass pass, IRenderable renderable)
    {
        pass.RemoveRenderable(renderable);
    }

    public virtual void OnBatchUpdated(RenderPass pass, IRenderable renderable)
    {
        pass.UpdateRenderable(renderable);
    }

    public virtual void Draw(GlInterface gl, RenderPass pass, RenderMaterial material, int indiceNum)
    {
        material.RenderState.DepthWriteEnabled = !Transparent;
        material.SetMainTexture(TextureCache.Singleton.GetTextureID(gl, MainTexture));
        material.Use(gl);
        gl.DrawElements(GlConsts.GL_TRIANGLES, indiceNum, GlConsts.GL_UNSIGNED_SHORT, 0);
    }

    public virtual Vector GetRenderSize(GlInterface gl)
    {
        return TextureCache.Singleton.GetTextureSize(gl, MainTexture);
    }

    protected void RaiseRenderLayerChanged(RenderLayers oldLayer, RenderLayers newLayer)
    {
        RenderLayerChanged?.Invoke(oldLayer, newLayer);
    }
}