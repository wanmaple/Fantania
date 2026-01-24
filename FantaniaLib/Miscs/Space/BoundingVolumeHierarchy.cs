using System.Numerics;

namespace FantaniaLib;

public class BoundingVolumeHierarchyNode<T> where T : IBVHItem
{
    public bool IsLeaf => item != null;
    public BoundingVolumeHierarchyNode<T>? Sibling
    {
        get
        {
            if (parent == null)
                return null;
            return parent.left == this ? parent.right : parent.left;
        }
    }

    public BoundingVolumeHierarchyNode<T>? parent;
    public BoundingVolumeHierarchyNode<T>? left;
    public BoundingVolumeHierarchyNode<T>? right;
    public Rectf bounds;
    public int height;
    public T? item;
}

public class BoundingVolumeHierarchy<T> where T : IBVHItem
{
    public Action<T>? ItemAdded;
    public Action<T>? ItemRemoved;
    public Action<T>? ItemChanged;

    public int ItemCount => _num;
    public Rectf Bounds => _root == null ? Rectf.Zero : _root.bounds;
    public BoundingVolumeHierarchyNode<T>? Root => _root;

    public BoundingVolumeHierarchy()
    {
        _item2node = new Dictionary<T, BoundingVolumeHierarchyNode<T>>(2000);
        _pool = new Stack<BoundingVolumeHierarchyNode<T>>(100);
    }

    public void AddItem(T item)
    {
        InnerAddItem(item);
        ItemAdded?.Invoke(item);
    }

    public bool RemoveItem(T item)
    {
        if (InnerRemoveItem(item))
        {
            ItemRemoved?.Invoke(item);
            return true;
        }
        return false;
    }

    public void UpdateItem(T item)
    {
        if (InnerRemoveItem(item))
        {
            InnerAddItem(item);
            ItemChanged?.Invoke(item);
        }
    }

    public void Clear()
    {
        _root = null;
        if (ItemRemoved != null)
        {
            foreach (var item in Enumerate())
            {
                ItemRemoved.Invoke(item);
            }
        }
        _item2node.Clear();
        _num = 0;
    }

    public IEnumerable<T> Enumerate()
    {
        return EnumerateTree(_root);
    }

    void InnerAddItem(T item)
    {
        var node = CreateNewNode(item.BoundingBox, item);
        _root = InsertNode(_root, node);
        _item2node.Add(item, node);
        ++_num;
    }

    bool InnerRemoveItem(T item)
    {
        if (_item2node.TryGetValue(item, out var node))
        {
            _root = RemoveNode(node);
            _item2node.Remove(item);
            --_num;
            return true;
        }
        return false;
    }

    BoundingVolumeHierarchyNode<T> InsertNode(BoundingVolumeHierarchyNode<T>? root, BoundingVolumeHierarchyNode<T> newNode)
    {
        if (root == null)
            return newNode;
        if (root.IsLeaf)
        {
            var newRoot = ReuseNode();
            newRoot.left = root;
            newRoot.right = newNode;
            root.parent = newRoot;
            newNode.parent = newRoot;
            UpdateNode(newRoot);
            return newRoot;
        }
        Rectf mergedLeft = root.left!.bounds.Merge(newNode.bounds);
        Rectf mergedRight = root.right!.bounds.Merge(newNode.bounds);
        float incLeft = mergedLeft.Area - root.left.bounds.Area;
        float incRight = mergedRight.Area - root.right.bounds.Area;
        if (incLeft < incRight)
        {
            root.left = InsertNode(root.left, newNode);
            root.left.parent = root;
        }
        else
        {
            root.right = InsertNode(root.right, newNode);
            root.right.parent = root;
        }
        UpdateNode(root);
        BalanceNode(root);
        return root;
    }

    BoundingVolumeHierarchyNode<T>? RemoveNode(BoundingVolumeHierarchyNode<T> node)
    {
        if (node == _root)
        {
            RecycleNode(node);
            return null;
        }
        var parent = node.parent;
        var sibling = node.Sibling;
        var grandParent = parent!.parent;
        sibling!.parent = grandParent;
        if (grandParent != null)
        {
            if (grandParent.left == parent)
                grandParent.left = sibling;
            else
                grandParent.right = sibling;
        }
        RecycleNode(parent);
        RecycleNode(node);
        var current = sibling;
        while (current.parent != null)
        {
            UpdateNode(current.parent);
            BalanceNode(current.parent);
            current = current.parent;
        }
        return current;
    }

    void BalanceNode(BoundingVolumeHierarchyNode<T> node)
    {
        if (node.IsLeaf) return;
        var left = node.left;
        var right = node.right;
        int balanceFactor = right!.height - left!.height;
        if (balanceFactor > 1)
        {
            var rl = right.left;
            var rr = right.right;
            if (rr!.height > rl!.height)
            {
                float areaBefore = left.bounds.Area + right.bounds.Area;
                Rectf boundsRightAfter = rl.bounds.Merge(left.bounds);
                float areaAfter = rr.bounds.Area + boundsRightAfter.Area;
                if (areaAfter <= areaBefore)
                {
                    right.right = left;
                    left.parent = right;
                    node.left = rr;
                    rr.parent = node;
                    right.bounds = boundsRightAfter;
                    UpdateNode(right, false);
                }
            }
            else
            {
                float areaBefore = left.bounds.Area + right.bounds.Area;
                Rectf boundsRightAfter = rr.bounds.Merge(left.bounds);
                float areaAfter = rl.bounds.Area + boundsRightAfter.Area;
                if (areaAfter <= areaBefore)
                {
                    right.left = left;
                    left.parent = right;
                    node.left = rl;
                    rl.parent = node;
                    right.bounds = boundsRightAfter;
                    UpdateNode(right, false);
                }
            }
        }
        else if (balanceFactor < -1)
        {
            var ll = left.left;
            var lr = left.right;
            if (ll!.height > lr!.height)
            {
                float areaBefore = left.bounds.Area + right.bounds.Area;
                Rectf boundsLeftAfter = lr.bounds.Merge(right.bounds);
                float areaAfter = ll.bounds.Area + boundsLeftAfter.Area;
                if (areaAfter <= areaBefore)
                {
                    left.left = right;
                    right.parent = left;
                    node.right = ll;
                    ll.parent = node;
                    left.bounds = boundsLeftAfter;
                    UpdateNode(left, false);
                }
            }
            else
            {
                float areaBefore = left.bounds.Area + right.bounds.Area;
                Rectf boundsLeftAfter = ll.bounds.Merge(right.bounds);
                float areaAfter = lr.bounds.Area + boundsLeftAfter.Area;
                if (areaAfter <= areaBefore)
                {
                    left.right = right;
                    right.parent = left;
                    node.right = lr;
                    lr.parent = node;
                    left.bounds = boundsLeftAfter;
                    UpdateNode(left, false);
                }
            }
        }
    }

    void UpdateNode(BoundingVolumeHierarchyNode<T> node, bool updateBounds = true)
    {
        if (node.IsLeaf)
        {
            node.height = 0;
            return;
        }
        node.height = Math.Max(node.left!.height, node.right!.height) + 1;
        if (updateBounds)
        {
            node.bounds = node.left.bounds.Merge(node.right.bounds);
        }
    }

    BoundingVolumeHierarchyNode<T> CreateNewNode(Rectf bounds, T item)
    {
        var node = ReuseNode();
        node.bounds = bounds;
        node.item = item;
        return node;
    }

    BoundingVolumeHierarchyNode<T> ReuseNode()
    {
        if (_pool.Count > 0)
        {
            var node = _pool.Pop();
            node.parent = node.left = node.right = null;
        }
        return new BoundingVolumeHierarchyNode<T>();
    }

    void RecycleNode(BoundingVolumeHierarchyNode<T> node)
    {
        _pool.Push(node);
    }

    public void PointTest(Vector2 point, IList<T> results, Predicate<T>? filter = null)
    {
        InnerPointTest(_root, point, filter, results);
    }

    public void RectTest(Rectf rect, IList<T> results, Predicate<T>? filter = null)
    {
        InnerRectTest(_root, rect, filter, results);
    }

    void InnerPointTest(BoundingVolumeHierarchyNode<T>? node, Vector2 point, Predicate<T>? filter, IList<T> ret)
    {
        if (node == null) return;
        if (!node.bounds.Contains(point)) return;
        if (node.IsLeaf)
        {
            if (filter == null || filter.Invoke(node.item!))
                ret.Add(node.item!);
            return;
        }
        InnerPointTest(node.left, point, filter, ret);
        InnerPointTest(node.right, point, filter, ret);
    }

    void InnerRectTest(BoundingVolumeHierarchyNode<T>? node, Rectf rect, Predicate<T>? filter, IList<T> ret)
    {
        if (node == null) return;
        if (!node.bounds.Intersects(rect)) return;
        if (node.IsLeaf)
        {
            if (filter == null || filter.Invoke(node.item!))
                ret.Add(node.item!);
            return;
        }
        InnerRectTest(node.left, rect, filter, ret);
        InnerRectTest(node.right, rect, filter, ret);
    }

    IEnumerable<T> EnumerateTree(BoundingVolumeHierarchyNode<T>? node)
    {
        if (node != null)
        {
            if (node.IsLeaf)
                yield return node.item!;
            else
            {
                foreach (T item in EnumerateTree(node.left))
                {
                    yield return item;
                }
                foreach (T item in EnumerateTree(node.right))
                {
                    yield return item;
                }
            }
        }
    }

    BoundingVolumeHierarchyNode<T>? _root = null;
    Dictionary<T, BoundingVolumeHierarchyNode<T>> _item2node;
    int _num = 0;
    Stack<BoundingVolumeHierarchyNode<T>> _pool;
}