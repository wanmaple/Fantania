using System;
using System.Collections.Generic;
using System.Numerics;
using Fantania.Views;
using FantaniaLib;

namespace Fantania.Models;

public abstract class LevelEntityCommand : ICanvasCommand
{
    public abstract void Execute(LevelSpaceContext context, ConfigurableRenderPipeline pipeline);

    protected void AddEntity(LevelEntity entity, LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        entity.GetLocalRenderInfo(context.Workspace, out var locals);
        var renderables = BuildRenderables(entity, locals, context.Workspace, pipeline, out var localBound);
        foreach (var renderable in renderables)
        {
            context.SpaceHierarchy.AddItem(renderable);
        }
        context.EntityManager.Register(entity, renderables, new EntityLocalInfo
        {
            Locals = locals,
            LocalBound = localBound,
        });
    }

    protected void RemoveEntity(LevelEntity entity, LevelSpaceContext context)
    {
        var renderables = context.EntityManager.GetRenderables(entity);
        foreach (var renderable in renderables)
        {
            context.SpaceHierarchy.RemoveItem(renderable);
        }
        context.EntityManager.Unregister(entity);
    }

    protected void UpdateEntity(LevelEntity entity, LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        if (entity.GetLocalRenderInfo(context.Workspace, out var _))
        {
            RemoveEntity(entity, context);
            AddEntity(entity, context, pipeline);
        }
        else
        {
            var renderables = context.EntityManager.GetRenderables(entity);
            var localInfo = context.EntityManager.GetLocalInfo(entity);
            Matrix3x3 worldMat = MathHelper.BuildTransform(entity.Anchor, localInfo.LocalBound.Size, entity.Position.ToVector2(), entity.Rotation, entity.Scale);
            for (int i = 0; i < renderables.Count; i++)
            {
                var renderable = renderables[i];
                renderable.Transform = worldMat * localInfo.Locals[i].LocalTransform;
                context.SpaceHierarchy.UpdateItem(renderable);
            }
        }
    }

    protected IReadOnlyList<IRenderable> BuildRenderables(LevelEntity entity, IReadOnlyList<LocalRenderInfo> locals, IWorkspace workspace, IRenderContext context, out Rectf localBound)
    {
        localBound = Rectf.Zero;
        if (locals.Count <= 0) return Array.Empty<IRenderable>();
        var renderables = new List<IRenderable>(locals.Count);
        foreach (LocalRenderInfo local in locals)
        {
            Vector2 size = local.Sizer.SizeOfRenderable(workspace).ToVector2();
            Matrix3x3 mat = MathHelper.BuildTransform(local.Anchor, size, local.Position, local.Rotation, local.Scale);
            var b = MathHelper.BuildBound(mat, size);
            localBound = localBound.Merge(b);
            local.LocalTransform = mat;
            local.LocalSize = size;
        }
        Matrix3x3 worldMat = MathHelper.BuildTransform(entity.Anchor, localBound.Size, entity.Position.ToVector2(), entity.Rotation, entity.Scale);
        for (int i = 0; i < locals.Count; i++)
        {
            var local = locals[i];
            RenderInfo info = new RenderInfo
            {
                Stage = local.Stage,
                Color = local.ColorOperator switch
                {
                    ColorOperators.Independent => local.Color,
                    ColorOperators.Multiple => local.Color * entity.Color,
                    _ => local.Color,
                },
                Depth = entity.RealDepth,
                Transform = worldMat * local.LocalTransform,
                Size = local.LocalSize,
                MaterialKey = local.MaterialKey,
                Uniforms = local.Uniforms,
                NodeIndex = local.NodeIndex,
            };
            RenderMaterial? material = context.MaterialSet.GetMaterial(info.MaterialKey);
            if (material != null)
            {
                info.Uniforms.ApplyToUniformSet(material.Uniforms);
                renderables.Add(new QuadRenderable(info, material));
            }
        }
        return renderables;
    }
}