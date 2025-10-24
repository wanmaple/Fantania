using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class NewBackgroundOperation : FrameBasedOperation
{
    public NewBackgroundOperation(Stylegrounds stylegrounds, Styleground bg, Styleground prev)
    {
        _sgs = stylegrounds;
        _styleground = bg;
        _prev = prev;
    }

    public override void Redo()
    {
        var workspace = WorkspaceViewModel.Current.Workspace;
        workspace.UnwatchStylegroundAddedOrRemoved(_sgs);
        _sgs.InsertBackgroundAfter(_styleground, _prev);
        workspace.WatchStylegroundAddedOrRemoved(_sgs);
    }

    public override void Undo()
    {
        var workspace = WorkspaceViewModel.Current.Workspace;
        workspace.UnwatchStylegroundAddedOrRemoved(_sgs);
        _sgs.RemoveBackground(_styleground);
        workspace.WatchStylegroundAddedOrRemoved(_sgs);
    }

    Stylegrounds _sgs;
    Styleground _styleground;
    Styleground _prev;
}