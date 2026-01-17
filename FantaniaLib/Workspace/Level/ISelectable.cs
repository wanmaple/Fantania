using System.Numerics;

namespace FantaniaLib;

public interface ISelectable
{
    bool PointTest(Vector2 pt);
}