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
        List<LocalRenderInfo> nonNodeLocals = new List<LocalRenderInfo>();
        List<IRenderable> nonNodeRenderables = new List<IRenderable>();
        List<LocalRenderInfo> nodeLocals = new List<LocalRenderInfo>();
        List<IRenderable> nodeRenderables = new List<IRenderable>();
        int localOrderStart = 0;
        entity.GetBackgroundNodes(context.Workspace, out var bgLocals);
        nonNodeLocals.AddRange(bgLocals);
        nonNodeRenderables.AddRange(BuildRenderables(entity, bgLocals, localOrderStart, context.Workspace, pipeline, out var _));
        localOrderStart += bgLocals.Count;
        for (int i = 0; i < entity.NodeCount; i++)
        {
            entity.GetLocalNodeAt(context.Workspace, i, out var locals);
            if (entity is IMultiNodeContainer container)
            {
                int nodeId = container.AllNodes[i].NodeId;
                foreach (var local in locals)
                    local.NodeId = nodeId;
            }
            var renderables = BuildRenderables(entity, locals, localOrderStart, context.Workspace, pipeline, out var localBound);
            entity.OnAddSelectables(context.SelectableHierarchy, i, localBound);
            nodeLocals.AddRange(locals);
            nodeRenderables.AddRange(renderables);
            localOrderStart += locals.Count;
        }
        entity.GetForegroundNodes(context.Workspace, out var fgLocals);
        nonNodeLocals.AddRange(fgLocals);
        nonNodeRenderables.AddRange(BuildRenderables(entity, fgLocals, localOrderStart, context.Workspace, pipeline, out var _));
        foreach (var renderable in nodeRenderables)
        {
            context.AddRenderableToBVH(renderable);
        }
        foreach (var renderable in nonNodeRenderables)
        {
            context.AddRenderableToBVH(renderable);
        }
        context.EntityManager.Register(entity, new EntityRenderInfo
        {
            NodeLocals = nodeLocals,
            NodeRenderables = nodeRenderables,
            NonNodeLocals = nonNodeLocals,
            NonNodeRenderables = nonNodeRenderables,
            OnChange = null,
        });
    }

    protected void RemoveEntity(LevelEntity entity, LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        entity.OnRemoveSelectables(context.SelectableHierarchy);
        foreach (var renderable in context.EntityManager.GetNodeRenderables(entity))
        {
            pipeline.MaterialSet.ReleaseMaterial(renderable.Material);
            context.RemoveRenderableFromBVH(renderable);
        }
        foreach (var renderable in context.EntityManager.GetNonNodeRenderables(entity))
        {
            pipeline.MaterialSet.ReleaseMaterial(renderable.Material);
            context.RemoveRenderableFromBVH(renderable);
        }
        context.EntityManager.Unregister(entity);
    }

    protected void UpdateEntity(LevelEntity entity, LevelSpaceContext context, ConfigurableRenderPipeline pipeline)
    {
        if (entity.PlacementDirty)
        {
            RemoveEntity(entity, context, pipeline);
            AddEntity(entity, context, pipeline);
            entity.PlacementDirty = false;
        }
        else
        {
            foreach (var renderable in context.EntityManager.GetNonNodeRenderables(entity))
            {
                context.RemoveRenderableFromBVH(renderable);
            }
            List<LocalRenderInfo> nonNodeLocals = new List<LocalRenderInfo>();
            List<IRenderable> nonNodeRenderables = new List<IRenderable>();
            int localOrderStart = 0;
            entity.GetBackgroundNodes(context.Workspace, out var bgLocals);
            nonNodeLocals.AddRange(bgLocals);
            nonNodeRenderables.AddRange(BuildRenderables(entity, bgLocals, localOrderStart, context.Workspace, pipeline, out var _));
            localOrderStart += bgLocals.Count;
            var nodeRenderables = context.EntityManager.GetNodeRenderables(entity);
            var localInfo = context.EntityManager.GetLocalInfo(entity);
            for (int i = 0; i < entity.NodeCount; i++)
            {
                entity.OnUpdateSelectables(context.SelectableHierarchy, i);
            }
            for (int i = 0; i < nodeRenderables.Count; i++)
            {
                var renderable = nodeRenderables[i];
                int currentIndex = entity.GetIndexByNodeId(localInfo.NodeLocals[i].NodeId);
                Matrix3x3 worldMat = entity.TransformAt(currentIndex);
                renderable.Transform = worldMat * localInfo.NodeLocals[i].LocalTransform;
                renderable.Depth = entity.RealDepth;
                renderable.EntityOrder = entity.Order;
                renderable.LocalOrder = localOrderStart + i;
                context.UpdateRenderableInBVH(renderable);
            }
            localOrderStart += nodeRenderables.Count;
            entity.GetForegroundNodes(context.Workspace, out var fgLocals);
            nonNodeLocals.AddRange(fgLocals);
            nonNodeRenderables.AddRange(BuildRenderables(entity, fgLocals, localOrderStart, context.Workspace, pipeline, out var _));
            foreach (var renderable in nonNodeRenderables)
            {
                context.AddRenderableToBVH(renderable);
            }
            localInfo.NonNodeLocals = nonNodeLocals;
            localInfo.NonNodeRenderables = nonNodeRenderables;
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
        for (int i = 0; i < locals.Count; i++)
        {
            var local = locals[i];
            int currentIndex = entity.GetIndexByNodeId(local.NodeId);
            Matrix3x3 worldMat = currentIndex >= 0 ? entity.TransformAt(currentIndex) : entity.SelfTransform;
            RenderInfo info = new RenderInfo
            {
                Stage = local.Stage,
                Color = local.Color,
                Depth = entity.RealDepth,
                EntityOrder = entity.Order,
                LocalOrder = i + localOrderStart,
                Transform = worldMat * local.LocalTransform,
                Anchor = local.Anchor,
                Size = local.LocalSize,
                Tiling = local.Tiling,
                Tiling2 = local.Tiling2,
                MaterialKey = local.MaterialKey,
                Uniforms = local.Uniforms,
            };
            var uniforms = new UniformSet(info.Uniforms);
            RenderMaterial material = context.MaterialSet.AcquireMaterial(info.MaterialKey, uniforms);
            var renderable = (IRenderable)Activator.CreateInstance(local.RenderableType, info, material, local.CustomArgs)!;
            renderables.Add(renderable);
        }
        return renderables;
    }
}