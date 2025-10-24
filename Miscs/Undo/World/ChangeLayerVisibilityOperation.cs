using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class ChangeLayerVisibilityOperation : FrameBasedOperation
{
    public ChangeLayerVisibilityOperation(RenderLayers layer, bool visible)
    {
        _layer = layer;
        _visible = visible;
    }

    public override void Redo()
    {
        WorkspaceViewModel.Current.Workspace.UnwatchLayerVisibilityChange();
        WorkspaceViewModel.Current.Workspace.CurrentWorld.SetLayerVisible(_layer, _visible);
        WorkspaceViewModel.Current.Workspace.WatchLayerVisibilityChange();
    }

    public override void Undo()
    {
        WorkspaceViewModel.Current.Workspace.UnwatchLayerVisibilityChange();
        WorkspaceViewModel.Current.Workspace.CurrentWorld.SetLayerVisible(_layer, !_visible);
        WorkspaceViewModel.Current.Workspace.WatchLayerVisibilityChange();
    }

    RenderLayers _layer;
    bool _visible;
}