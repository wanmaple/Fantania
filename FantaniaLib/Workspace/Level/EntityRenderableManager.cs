namespace FantaniaLib;

public class EntityRenderInfo
{
    public required IReadOnlyList<LocalRenderInfo> NodeLocals;
    public required IReadOnlyList<IRenderable> NodeRenderables;
    public required IReadOnlyList<LocalRenderInfo> NonNodeLocals;
    public required IReadOnlyList<IRenderable> NonNodeRenderables;
    public Action<LevelEntity>? OnChange;
}

public class EntityRenderableManager
{
    public bool HasEntity(LevelEntity entity)
    {
        return _entity2renders.ContainsKey(entity);
    }

    public IReadOnlyList<IRenderable> GetNodeRenderables(LevelEntity entity)
    {
        return _entity2renders[entity].NodeRenderables;
    }

    public IReadOnlyList<IRenderable> GetNonNodeRenderables(LevelEntity entity)
    {
        return _entity2renders[entity].NonNodeRenderables;
    }

    public EntityRenderInfo GetLocalInfo(LevelEntity entity)
    {
        return _entity2renders[entity];
    }

    public LevelEntity GetEntity(IRenderable renderable)
    {
        return _renderable2entity[renderable];
    }

    public void Register(LevelEntity entity, EntityRenderInfo localInfo)
    {
        _entity2renders.Add(entity, localInfo);
        if (localInfo.OnChange != null)
            entity.RenderingDirty += localInfo.OnChange;
        foreach (IRenderable renderable in localInfo.NodeRenderables)
        {
            _renderable2entity.Add(renderable, entity);
        }
        foreach (IRenderable renderable in localInfo.NonNodeRenderables)
        {
            _renderable2entity.Add(renderable, entity);
        }
    }

    public void Unregister(LevelEntity entity)
    {
        var localInfo = _entity2renders[entity];
        foreach (IRenderable renderable in localInfo.NodeRenderables)
        {
            _renderable2entity.Remove(renderable);
        }
        foreach (IRenderable renderable in localInfo.NonNodeRenderables)
        {
            _renderable2entity.Remove(renderable);
        }
        if (localInfo.OnChange != null)
            entity.RenderingDirty -= localInfo.OnChange;
        _entity2renders.Remove(entity);
    }

    Dictionary<IRenderable, LevelEntity> _renderable2entity = new Dictionary<IRenderable, LevelEntity>(128);
    Dictionary<LevelEntity, EntityRenderInfo> _entity2renders = new Dictionary<LevelEntity, EntityRenderInfo>(64);
}