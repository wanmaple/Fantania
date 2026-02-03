using System.Collections.Generic;
using FantaniaLib;

namespace Fantania.Views;

public enum SelectionModes
{
    Replace,
    Add,
    Remove,
}

public class SelectionContext
{
    public SelectionContext(IWorkspace workspace)
    {
        _workspace = workspace;
    }

    public void Begin()
    {
        _oldSelections.Clear();
        foreach (var r in _workspace.EditorModule.SelectedObjects)
        {
            _oldSelections.Add(r);
        }
    }

    public void End()
    {
        _oldSelections.Clear();
    }

    public void UpdateSelectedObjects(IReadOnlySet<ISelectableItem> sels, SelectionModes mode)
    {
        var selections = _workspace.EditorModule.SelectedObjects;
        if (mode == SelectionModes.Add)
        {
            // 最终结果为并集
            for (int i = 0; i < selections.Count; )
            {
                var sel = selections[i];
                if (!_oldSelections.Contains(sel) && !sels.Contains(sel))
                    selections.RemoveAtFast(i);
                else
                    i++;
            }
            foreach (var sel in sels)
            {
                if (!selections.Contains(sel))
                    selections.Add(sel);
            }
        }
        else if (mode == SelectionModes.Remove)
        {
            // 最终结果为差集
            for (int i = 0; i < selections.Count; )
            {
                var sel = selections[i];
                if (sels.Contains(sel))
                    selections.RemoveAtFast(i);
                else
                    i++;
            }
            foreach (var sel in _oldSelections)
            {
                if (!sels.Contains(sel) && !selections.Contains(sel))
                    selections.Add(sel);
            }
        }
        else if (mode == SelectionModes.Replace)
        {
            // 最终结果直接替换
            for (int i = 0; i < selections.Count; )
            {
                var sel = selections[i];
                if (!sels.Contains(sel))
                    selections.RemoveAtFast(i);
                else
                    i++;
            }
            foreach (var sel in sels)
            {
                if (!selections.Contains(sel))
                    selections.Add(sel);
            }
        }
        _workspace.EditorModule.Notify();
    }

    IWorkspace _workspace;
    HashSet<ISelectableItem> _oldSelections = new HashSet<ISelectableItem>(0);
}