namespace FantaniaLib;

public class TransparentPipelineStage : IPipelineStage
{
    public string Name => "Transparent";
    public int Order => 4000;

    public void PostRender(IRenderContext context)
    {
    }

    public void PreRender(IRenderContext context)
    {
        context.CommandBuffer.SetupState(_state);
    }

    public void Render(IRenderContext context, IEnumerable<IRenderable> renderables, Camera2D camera)
    {
        var list = renderables as List<IRenderable> ?? renderables.ToList();
        if (list.Count == 0) return;
        float cellSize = CalculateCellSize(camera);
        _ds.Reset(list);
        _buckets.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            Rectf bounds = list[i].BoundingBox;
            int minX = (int)MathF.Floor(bounds.Left / cellSize);
            int maxX = (int)MathF.Floor(bounds.Right / cellSize);
            int minY = (int)MathF.Floor(bounds.Top / cellSize);
            int maxY = (int)MathF.Floor(bounds.Bottom / cellSize);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var key = new Vector2Int(x, y);
                    if (!_buckets.TryGetValue(key, out var cellList))
                    {
                        cellList = new List<IRenderable>(4);
                        _buckets.Add(key, cellList);
                    }
                    for (int c = 0; c < cellList.Count; c++)
                    {
                        IRenderable r = cellList[c];
                        if (list[i].BoundingBox.Intersects(r.BoundingBox))
                        {
                            _ds.Union(list[i], r);
                        }
                    }
                    cellList.Add(list[i]);
                }
            }
        }
        _groupCache.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            IRenderable root = _ds.Find(list[i]);
            if (!_groupCache.TryGetValue(root, out var group))
            {
                group = new List<IRenderable>();
                _groupCache[root] = group;
            }
            group.Add(list[i]);
        }
        var groups = _groupCache.Values.ToList();
        groups.StableSort((lhs, rhs) => GetGroupDepthKey(rhs).CompareTo(GetGroupDepthKey(lhs)));
        foreach (var group in groups)
        {
            RenderSortedBatches(context, group);
        }
    }

    readonly RenderState _state = new RenderState
    {
        DepthTestEnabled = true,
        DepthWriteEnabled = false,
        BlendingEnabled = true,
        BlendFuncSrc = BlendFuncs.SrcAlpha,
        BlendFuncDst = BlendFuncs.OneMinusSrcAlpha,
    };

    float CalculateCellSize(Camera2D camera)
    {
        float zoom = MathF.Max(camera.Zoom, 0.0001f);
        float worldCellSize = TARGET_CELL_PIXELS / zoom;
        return Math.Clamp(worldCellSize, MIN_CELL_SIZE, MAX_CELL_SIZE);
    }

    int GetGroupDepthKey(IReadOnlyList<IRenderable> group)
    {
        int maxDepth = int.MinValue;
        for (int i = 0; i < group.Count; i++)
        {
            int depth = group[i].Depth;
            if (depth > maxDepth)
                maxDepth = depth;
        }
        return maxDepth;
    }

    void RenderSortedBatches(IRenderContext context, IList<IRenderable> renderables)
    {
        var list = renderables as List<IRenderable> ?? renderables;
        list.StableSort(RenderableInverseDepthComparer.Instance);
        RenderMaterial? currentMaterial = null;
        VertexDescriptor? currentVertexDesc = null;
        var meshes = new List<Mesh>(32);
        for (int i = 0; i < list.Count; i++)
        {
            var renderable = list[i];
            var vertexDesc = renderable.Mesh.Descriptor.VertexDescriptor;
            if (currentMaterial == null || currentVertexDesc == null ||
                !ReferenceEquals(currentMaterial, renderable.Material) ||
                !ReferenceEquals(currentVertexDesc, vertexDesc))
            {
                if (meshes.Count > 0 && currentMaterial != null)
                {
                    context.CommandBuffer.Draw(meshes.ToArray(), currentMaterial);
                    meshes.Clear();
                }
                currentMaterial = renderable.Material;
                currentVertexDesc = vertexDesc;
            }
            meshes.Add(renderable.Mesh);
        }
        if (meshes.Count > 0 && currentMaterial != null)
        {
            context.CommandBuffer.Draw(meshes.ToArray(), currentMaterial);
        }
    }

    DisjointSet<IRenderable> _ds = new DisjointSet<IRenderable>(Array.Empty<IRenderable>());
    Dictionary<Vector2Int, List<IRenderable>> _buckets = new Dictionary<Vector2Int, List<IRenderable>>();
    Dictionary<IRenderable, List<IRenderable>> _groupCache = new Dictionary<IRenderable, List<IRenderable>>();

    const float TARGET_CELL_PIXELS = 64.0f;
    const float MIN_CELL_SIZE = 32.0f;
    const float MAX_CELL_SIZE = 1024.0f;
}