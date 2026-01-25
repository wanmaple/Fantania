using System.Collections.Generic;
using System.Linq;
using FantaniaLib;

namespace Fantania.Views;

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

    public void UpdateSelectedObjects(IEnumerable<IRenderable> selected, bool keepOld)
    {
        var selections = _workspace.EditorModule.SelectedObjects;
        if (keepOld)
        {
            for (int i = 0; i < selections.Count;)
            {
                var s = selections[i];
                if (!_oldSelections.Contains(s) && !selected.Contains(s))
                    selections.RemoveAtFast(i);
                else
                    i++;
            }
            foreach (var r in selected)
            {
                if (!selections.Contains(r))
                    selections.Add(r);
            }
        }
        else
        {
            for (int i = 0; i < selections.Count;)
            {
                var s = selections[i];
                if (!selected.Contains(s))
                    selections.RemoveAtFast(i);
                else
                    i++;
            }
            foreach (var r in selected)
            {
                if (!selections.Contains(r))
                    selections.Add(r);
            }
        }
    }

    IWorkspace _workspace;
    HashSet<IBVHItem> _oldSelections = new HashSet<IBVHItem>(0);
}