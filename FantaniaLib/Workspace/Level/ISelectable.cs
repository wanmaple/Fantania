using System.Numerics;

namespace FantaniaLib;

public interface ISelectable
{
    /// <summary>
    /// 索引如果小于0意味着无法被选中，如果是0表示Entity本身(修改则会影响整体)，如果大于0则表示额外节点，修改的是相对坐标。
    /// </summary>
    int NodeIndex { get; }

    bool PointTest(Vector2 pt);
}