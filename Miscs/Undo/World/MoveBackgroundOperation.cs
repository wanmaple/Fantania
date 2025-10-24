using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class MoveBackgroundOperation : FrameBasedOperation
{
    public MoveBackgroundOperation(Stylegrounds stylegrounds, Styleground bg, Styleground oldPrev, Styleground newPrev)
    {
        _sgs = stylegrounds;
        _styleground = bg;
        _oldPrev = oldPrev;
        _newPrev = newPrev;
    }

    public override void Redo()
    {
        var workspace = WorkspaceViewModel.Current.Workspace;
        workspace.UnwatchStylegroundAddedOrRemoved(_sgs);
        _sgs.MoveBackgroundAfter(_styleground, _newPrev);
        workspace.WatchStylegroundAddedOrRemoved(_sgs);
    }

    public override void Undo()
    {
        var workspace = WorkspaceViewModel.Current.Workspace;
        workspace.UnwatchStylegroundAddedOrRemoved(_sgs);
        _sgs.MoveBackgroundAfter(_styleground, _oldPrev);
        workspace.WatchStylegroundAddedOrRemoved(_sgs);
    }

    Stylegrounds _sgs;
    Styleground _styleground;
    Styleground _oldPrev;
    Styleground _newPrev;
}