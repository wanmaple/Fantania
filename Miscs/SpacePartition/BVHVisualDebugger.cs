using System;
using Avalonia;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class BVHVisualDebugger
{
    private class BoundsNode : IPoolable
    {
        public BVHBounds Bounds => _bounds;

        public BoundsNode Left { get; set; }
        public BoundsNode Right { get; set; }

        public BoundsNode(Workspace workspace)
        {
            _world = workspace.CurrentWorld;
            _bounds = ObjectPool<BVHBounds>.Get();
            _world.AddObject(_bounds, false);
        }

        ~BoundsNode()
        {
            _world.RemoveObject(_bounds);
        }

        public void Sync(Rect bounds, int depth, float mix)
        {
            _bounds.Position = new Vector(bounds.TopLeft.X, -bounds.BottomLeft.Y);
            _bounds.Size = new Vector(bounds.Width, bounds.Height);
            _bounds.RelativeDepth = Math.Max(0, 500 - depth);
            _bounds.CustomData = new System.Numerics.Vector4((float)_bounds.Size.X, (float)_bounds.Size.Y, -1.0f, -1.0f);
            _bounds.VertexColor = System.Numerics.Vector4.Lerp(Preferences.Singleton.DebugSettings.BVHVisualMinColor, Preferences.Singleton.DebugSettings.BVHVisualMaxColor, mix);
            _bounds._bounds = bounds;
        }

        public void Delete()
        {
            DeleteInternal(this);
        }

        public void OnPooled()
        {
            _world.RemoveObject(_bounds);
            ObjectPool<BVHBounds>.Return(_bounds);
        }

        public void OnRecycled(params object[] args)
        {
            Left = Right = null;
            _world.AddObject(_bounds, false);
            _world = WorkspaceViewModel.Current.Workspace.CurrentWorld;
            _bounds = ObjectPool<BVHBounds>.Get(args);
        }

        void DeleteInternal(BoundsNode node)
        {
            if (node.Left != null)
                DeleteInternal(node.Left);
            if (node.Right != null)
                DeleteInternal(node.Right);
            ObjectPool<BoundsNode>.Return(node);
        }

        BVHBounds _bounds;
        World _world;
    }

    private bool _enabled = false;
    public bool IsEnabled
    {
        get { return _enabled; }
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                if (_enabled)
                {
                    OnBVHItemChanged(_bvh);
                    _bvh.ItemChanged += OnBVHItemChanged;
                }
                else
                {
                    if (_root != null)
                    {
                        _root.Delete();
                        _root = null;
                    }
                    _bvh.ItemChanged -= OnBVHItemChanged;
                }
            }
        }
    }

    public BVHVisualDebugger()
    {
        _workspace = WorkspaceViewModel.Current.Workspace;
        _workspace.WorldChanged += OnWorldChanged;
        _world = _workspace.CurrentWorld;
        _bvh = _world._bvh;
    }

    ~BVHVisualDebugger()
    {
        IsEnabled = false;
    }

    void OnBVHItemChanged(BoundingVolumeHierarchy<WorldObject> bvh)
    {
        _root = SyncBVH(bvh.Root, _root);
    }

    BoundsNode SyncBVH(BoundingVolumeHierarchyNode<WorldObject> bvhNode, BoundsNode node)
    {
        if (bvhNode == null)
        {
            if (node != null)
            {
                node.Delete();
                node = null;
            }
        }
        else
        {
            if (node == null)
                node = ObjectPool<BoundsNode>.Get(_workspace);
            node.Sync(bvhNode.bounds, bvhNode.height, _bvh.Root.height == 0 ? 1.0f : ((float)bvhNode.height / _bvh.Root.height));
            node.Left = SyncBVH(bvhNode.left, node.Left);
            node.Right = SyncBVH(bvhNode.right, node.Right);
        }
        return node;
    }

    void OnWorldChanged(World world)
    {
        if (IsEnabled)
        {
            Preferences.Singleton.DebugSettings.IsBVHVisualizerOn = false;
            IsEnabled = false;
        }
        _world = world;
        _bvh = _world._bvh;
    }

    World _world;
    BoundingVolumeHierarchy<WorldObject> _bvh;
    BoundsNode _root;
    Workspace _workspace;
}