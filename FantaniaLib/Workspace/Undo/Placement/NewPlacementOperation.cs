namespace FantaniaLib;

public class NewPlacementOperation : FrameBasedOperation
{
    public NewPlacementOperation(IWorkspace workspace, PlacementTemplate template, UserPlacement placement) : base(workspace.FrameCount)
    {
        _template = template;
        _placement = placement;
    }

    public override void Redo()
    {
        _template.Children.Add(_placement);
    }

    public override void Undo()
    {
        _template.Children.Remove(_placement);
    }

    PlacementTemplate _template;
    UserPlacement _placement;
}