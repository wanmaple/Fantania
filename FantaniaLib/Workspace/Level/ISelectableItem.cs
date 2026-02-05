using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public interface ISelectableItem : IBVHItem
{
    Color SelectionColor { get; }
    Vector2 Anchor { get; }
    Vector2 WorldPosition { get; }
    int Depth { get; }
    int EntityOrder { get; }
    int LocalOrder { get; }

    bool PointTest(Vector2 pt);
    bool CanTranslate(IWorkspace workspace);
    bool CanRotate(IWorkspace workspace);
    bool CanScale(IWorkspace workspace);
    void OnTranslateBegin();
    void OnTranslating(Vector2Int worldChange);
    void OnRotateBegin();
    void OnRotating(float rotationChange);
    void OnScaleBegin();
    void OnScaling(Vector2 scaleFactor);
}