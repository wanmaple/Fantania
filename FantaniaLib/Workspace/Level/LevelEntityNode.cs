using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public class LevelEntityNode : ISelectableItem
{
    public Color SelectionColor => Colors.Purple;
    public Vector2 Anchor => throw new NotImplementedException();
    public int Depth => throw new NotImplementedException();
    public int EntityOrder => throw new NotImplementedException();
    public int LocalOrder => throw new NotImplementedException();
    public Rectf BoundingBox => throw new NotImplementedException();

    public LevelEntityNode(IMultiNodeContainer container, int index)
    {
        _container = container;
        _index = index;
    }

    public bool CanRotate(IWorkspace workspace)
    {
        return _container.CanRotate(workspace, _index);
    }

    public bool CanScale(IWorkspace workspace)
    {
        return _container.CanScale(workspace, _index);
    }

    public bool CanTranslate(IWorkspace workspace)
    {
        return _container.CanTranslate(workspace, _index);
    }

    public void OnDelete(IWorkspace workspace)
    {
        _container.RemoveNodeAt(_index);
    }

    public void OnTranslateBegin()
    {
        throw new NotImplementedException();
    }

    public void OnTranslating(Vector2Int worldChange)
    {
        throw new NotImplementedException();
    }

    public bool PointTest(Vector2 pt)
    {
        throw new NotImplementedException();
    }

    IMultiNodeContainer _container;
    int _index;
}