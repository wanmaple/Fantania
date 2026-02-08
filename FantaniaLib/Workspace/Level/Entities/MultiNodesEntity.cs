using System.Numerics;

namespace FantaniaLib;

public class MultiNodesEntity : LevelEntity, IMultiNodeContainer
{
    public event Action<LevelEntityNode>? NodeAdded;
    public event Action<LevelEntityNode>? NodeRemoved;

    public override int NodeCount => AllNodes.Count;
    public IReadOnlyList<LevelEntityNode> AllNodes => _nodes;
    public int Depth => RealDepth;

    private EntityNodeCollection _nodes = new EntityNodeCollection();
    [SerializableField(FieldTypes.Custom)]
    public EntityNodeCollection MutableNodes
    {
        get { return _nodes; }
        set
        {
            if (_nodes != value)
            {
                OnPropertyChanging(nameof(MutableNodes));
                _nodes = value;
                OnPropertyChanged(nameof(MutableNodes));
                _nextNodeId = _nodes.Count > 0 ? _nodes.Max(n => n.NodeId) + 1 : 0;
            }
        }
    }

    internal MultiNodesEntity()
    {
    }

    private int AllocateNodeId()
    {
        return _nextNodeId++;
    }

    NodeOptions GetNodeOption(IWorkspace workspace)
    {
        UserPlacement placement = GetReferencedPlacement(workspace);
        var option = placement.Template.NodeOptions;
        return option;
    }

    internal void SetNodes(IWorkspace workspace, EntityNodesSnapshot snapshot)
    {
        foreach (var node in _nodes)
        {
            if (!snapshot.Nodes.Contains(node))
            {
                _toRm.Add(node);
                NodeRemoved?.Invoke(node);
            }
        }
        foreach (var node in snapshot.Nodes)
        {
            if (!_nodes.Contains(node))
            {
                _nodes.Add(node);
                NodeAdded?.Invoke(node);
            }
        }
        PlacementDirty = true;
        RaiseRenderingDirty();
    }

    public override void Initialize(IWorkspace workspace)
    {
        var option = GetNodeOption(workspace);
        for (int i = 0; i < Math.Max(option.Minimum, 1); i++)
        {
            var node = new LevelEntityNode(this)
            {
                NodeId = AllocateNodeId(),
            };
            _nodes.Add(node);
            _nodes[i].Position = i * option.DefaultOffset;
        }
    }

    public void AppendNode(IWorkspace workspace, Vector2? worldPos)
    {
        var option = GetNodeOption(workspace);
        if (option.Maximum < 0 || _nodes.Count < option.Maximum)
        {
            var node = new LevelEntityNode(this)
            {
                NodeId = AllocateNodeId(),
            };
            if (worldPos == null)
                node.Position = _nodes.Count > 0 ? _nodes.Last().Position + option.DefaultOffset : Vector2Int.Zero;
            else
            {
                Matrix3x3 worldMat = SelfTransform;
                Vector2 localPos = worldMat.Inverse() * worldPos.Value;
                node.Position = localPos.ToVector2i();
            }
            var snapshotBefore = new EntityNodesSnapshot(_nodes);
            _nodes.Add(node);
            NodeAdded?.Invoke(node);
            PlacementDirty = true;
            RaiseRenderingDirty();
            var snapshotAfter = new EntityNodesSnapshot(_nodes);
            var op = new BatchModifyNodesOperation(workspace, this, snapshotBefore, snapshotAfter);
            workspace.UndoStack.AddOperation(op);
        }
    }

    public bool RemoveNodes(IWorkspace workspace, EntityNodesSnapshot snapshot)
    {
        var option = GetNodeOption(workspace);
        if (_nodes.Count - snapshot.Nodes.Count >= Math.Max(option.Minimum, 1))
        {
            var snapshotBefore = new EntityNodesSnapshot(_nodes);
            foreach (var node in snapshot.Nodes)
            {
                _toRm.Add(node);
                NodeRemoved?.Invoke(node);
            }
            PlacementDirty = true;
            RaiseRenderingDirty();
            var snapshotAfter = new EntityNodesSnapshot(_nodes.Except(snapshot.Nodes).ToArray());
            var op = new BatchModifyNodesOperation(workspace, this, snapshotBefore, snapshotAfter);
            workspace.UndoStack.AddOperation(op);
            return true;
        }
        return false;
    }

    public bool CanRotate(IWorkspace workspace, int index)
    {
        return GetReferencedPlacement(workspace).Template.CanRotate(index);
    }

    public bool CanScale(IWorkspace workspace, int index)
    {
        return GetReferencedPlacement(workspace).Template.CanScale(index);
    }

    public bool CanTranslate(IWorkspace workspace, int index)
    {
        return GetReferencedPlacement(workspace).Template.CanTranslate(index);
    }

    public override int GetIndexByNodeId(int nodeId)
    {
        var node = _nodes.First(n => n.NodeId == nodeId);
        return node.LocalOrder;
    }

    public override void GetLocalNodeAt(IWorkspace workspace, int index, out IReadOnlyList<LocalRenderInfo> locals)
    {
        UserPlacement placement = GetReferencedPlacement(workspace);
        locals = placement.GetLocalNodeAt(index, NodeCount);
    }

    public override void GetBackgroundNodes(IWorkspace workspace, out IReadOnlyList<LocalRenderInfo> locals)
    {
        UserPlacement placement = GetReferencedPlacement(workspace);
        locals = placement.GetBackgroundNodes(_nodes);
    }

    public override void GetForegroundNodes(IWorkspace workspace, out IReadOnlyList<LocalRenderInfo> locals)
    {
        UserPlacement placement = GetReferencedPlacement(workspace);
        locals = placement.GetForegroundNodes(_nodes);
    }

    public override void OnAddSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index, Rectf bound)
    {
        _nodes[index].LocalBound = bound;
        var node = _nodes[index];
        node.BoundingBox = CalculateBounds(TransformAt(index), bound);
        bvh.AddItem(node);
    }

    public override void OnRemoveSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh)
    {
        foreach (var node in _nodes)
            bvh.RemoveItem(node);
        foreach (var rm in _toRm)
            _nodes.Remove(rm);
        _toRm.Clear();
    }

    public override void OnUpdateSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index)
    {
        var node = _nodes[index];
        node.BoundingBox = CalculateBounds(TransformAt(index), node.LocalBound);
        bvh.UpdateItem(node);
    }

    public override Matrix3x3 TransformAt(int index)
    {
        var node = _nodes[index];
        Matrix3x3 selfMat = MathHelper.BuildTransform(Vector2.Zero, Vector2.Zero, Position.ToVector2(), Rotation, Scale);
        Matrix3x3 nodeMat = MathHelper.BuildTransform(Vector2.Zero, Vector2.Zero, node.Position.ToVector2(), node.Rotation, node.Scale);
        return selfMat * nodeMat;
    }

    Rectf CalculateBounds(Matrix3x3 transform, Rectf bound)
    {
        Vector2 pt1 = transform * bound.TopLeft;
        Vector2 pt2 = transform * bound.TopRight;
        Vector2 pt3 = transform * bound.BottomRight;
        Vector2 pt4 = transform * bound.BottomLeft;
        float minX = MathF.Min(pt1.X, MathF.Min(pt2.X, MathF.Min(pt3.X, pt4.X)));
        float maxX = MathF.Max(pt1.X, MathF.Max(pt2.X, MathF.Max(pt3.X, pt4.X)));
        float minY = MathF.Min(pt1.Y, MathF.Min(pt2.Y, MathF.Min(pt3.Y, pt4.Y)));
        float maxY = MathF.Max(pt1.Y, MathF.Max(pt2.Y, MathF.Max(pt3.Y, pt4.Y)));
        return new Rectf(minX, minY, maxX - minX, maxY - minY);
    }

    public void MarkTransformDirty()
    {
        RaiseRenderingDirty();
    }

    List<LevelEntityNode> _toRm = new List<LevelEntityNode>(8);
    int _nextNodeId = 0;
}
