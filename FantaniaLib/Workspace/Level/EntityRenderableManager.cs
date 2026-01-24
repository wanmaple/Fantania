namespace FantaniaLib;

public struct EntityLocalInfo
{
    public IReadOnlyList<LocalRenderInfo> Locals;
    public Rectf LocalBound;
}

public class EntityRenderableManager
{
    public IReadOnlyList<IRenderable> GetRenderables(LevelEntity entity)
    {
        return _entity2renderables[entity];
    }

    public EntityLocalInfo GetLocalInfo(LevelEntity entity)
    {
        return _entity2locals[entity];
    }

    public LevelEntity GetEntity(IRenderable renderable)
    {
        return _renderable2entity[renderable];
    }

    public void Register(LevelEntity entity, IReadOnlyList<IRenderable> renderables, EntityLocalInfo localInfo)
    {
        _entity2renderables.Add(entity, renderables);
        _entity2locals.Add(entity, localInfo);
        foreach (IRenderable renderable in renderables)
        {
            _renderable2entity.Add(renderable, entity);
        }
    }

    public void Unregister(LevelEntity entity)
    {
        foreach (IRenderable renderable in _entity2renderables[entity])
        {
            _renderable2entity.Remove(renderable);
        }
        _entity2renderables.Remove(entity);
        _entity2locals.Remove(entity);
    }

    Dictionary<LevelEntity, IReadOnlyList<IRenderable>> _entity2renderables = new Dictionary<LevelEntity, IReadOnlyList<IRenderable>>(64);
    Dictionary<IRenderable, LevelEntity> _renderable2entity = new Dictionary<IRenderable, LevelEntity>(128);
    Dictionary<LevelEntity, EntityLocalInfo> _entity2locals = new Dictionary<LevelEntity, EntityLocalInfo>(64);
}