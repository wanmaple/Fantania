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
        List<LocalRenderInfo> allLocals = new List<LocalRenderInfo>();
        List<IRenderable> allRenderables = new List<IRenderable>();
        int localOrderStart = 0;
        for (int i = 0; i < entity.NodeCount; i++)
        {
            entity.GetLocalNodeAt(context.Workspace, i, out var locals);
            var renderables = BuildRenderables(entity, locals, localOrderStart, context.Workspace, pipeline, out var localBound);
            entity.OnAddSelectables(context.SelectableHierarchy, i, localBound);
            foreach (var renderable in renderables)
            {
                context.RenderableHierarchy.AddItem(renderable);
            }
            allLocals.AddRange(locals);
            allRenderables.AddRange(renderables);
            localOrderStart += locals.Count;
        }
        context.EntityManager.Register(entity, allRenderables, new EntityLocalInfo
        {
            Locals = allLocals,
            OnChange = e =>
            {
                UpdateEntity(e, context, pipeline);
                context.Workspace.EditorModule.Notify();
            },
        });
    }

    protected void RemoveEntity(LevelEntity entity, LevelSpaceContext context)
    {
        for (int i = entity.NodeCount; i >= 0; i--)
        {
            entity.OnRemoveSelectables(context.SelectableHierarchy, i);
        }
        var renderables = context.EntityManager.GetRenderables(entity);
        foreach (var renderable in renderables)
        {
            context.RenderableHierarchy.RemoveItem(renderable);
        }
        context.EntityManager.Unregister(entity);
    }

    protected void UpdateEntity(LevelEntity entity, LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        if (entity.PlacementDirty)
        {
            RemoveEntity(entity, context);
            AddEntity(entity, context, pipeline);
        }
        else
        {
            var renderables = context.EntityManager.GetRenderables(entity);
            var localInfo = context.EntityManager.GetLocalInfo(entity);
            for (int i = 0; i < entity.NodeCount; i++)
            {
                entity.OnUpdateSelectables(context.SelectableHierarchy, i);
            }
            Matrix3x3 worldMat = MathHelper.BuildTransform(Vector2.Zero, Vector2.Zero, entity.Position.ToVector2(), entity.Rotation, entity.Scale);
            for (int i = 0; i < renderables.Count; i++)
            {
                var renderable = renderables[i];
                renderable.Transform = worldMat * localInfo.Locals[i].LocalTransform;
                renderable.Depth = entity.RealDepth;
                renderable.EntityOrder = entity.Order;
                renderable.LocalOrder = i;
                context.RenderableHierarchy.UpdateItem(renderable);
            }
        }
    }

    protected IReadOnlyList<IRenderable> BuildRenderables(LevelEntity entity, IReadOnlyList<LocalRenderInfo> locals, int localOrderStart, IWorkspace workspace, IRenderContext context, out Rectf localBound)
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
        Matrix3x3 worldMat = MathHelper.BuildTransform(Vector2.Zero, Vector2.Zero, entity.Position.ToVector2(), entity.Rotation, entity.Scale);
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
                EntityOrder = entity.Order,
                LocalOrder = i + localOrderStart,
                Transform = worldMat * local.LocalTransform,
                Anchor = local.Anchor,
                Size = local.LocalSize,
                MaterialKey = local.MaterialKey,
                Uniforms = local.Uniforms,
            };
            RenderMaterial material = context.MaterialSet.GetMaterial(info.MaterialKey);
            info.Uniforms.ApplyToUniformSet(material.Uniforms);
            renderables.Add(new QuadRenderable(info, material));
        }
        return renderables;
    }
}