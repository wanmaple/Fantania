using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class DeleteForegroundOperation : FrameBasedOperation
{
    public DeleteForegroundOperation(Stylegrounds stylegrounds, Styleground fg, Styleground prev)
    {
        _sgs = stylegrounds;
        _styleground = fg;
        _prev = prev;
    }

    public override void Redo()
    {
        var workspace = WorkspaceViewModel.Current.Workspace;
        workspace.UnwatchStylegroundAddedOrRemoved(_sgs);
        _sgs.RemoveForeground(_styleground);
        workspace.WatchStylegroundAddedOrRemoved(_sgs);
    }

    public override void Undo()
    {
        var workspace = WorkspaceViewModel.Current.Workspace;
        workspace.UnwatchStylegroundAddedOrRemoved(_sgs);
        _sgs.InsertForegroundAfter(_styleground, _prev);
        workspace.WatchStylegroundAddedOrRemoved(_sgs);
    }

    Stylegrounds _sgs;
    Styleground _styleground;
    Styleground _prev;
}