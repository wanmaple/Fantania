namespace FantaniaLib;

public class DelPlacementOperation : FrameBasedOperation
{
    public DelPlacementOperation(IWorkspace workspace, PlacementTemplate template, UserPlacement placement) : base(workspace.FrameCount)
    {
        _template = template;
        _placement = placement;
    }

    public override void Redo()
    {
        _template.Source.Remove(_placement);
    }

    public override void Undo()
    {
        _template.Source.Add(_placement);
    }

    PlacementTemplate _template;
    UserPlacement _placement;
}