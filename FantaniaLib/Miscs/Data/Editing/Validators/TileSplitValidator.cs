namespace FantaniaLib;

public class TileSplitValidator : FieldValidatorBase
{
    public override bool ValidateField(IWorkspace workspace, object fieldValue)
    {
        Vector2Int size = (Vector2Int)fieldValue;
        if (size.X <= 0 || size.Y <= 0)
        {
            Error = workspace.LocalizeString("ERR_TileSplitMustBePositive");
            return false;
        }
        if (size.X * size.Y > 256)
        {
            Error = workspace.LocalizeString("ERR_TileSplitTooLarge");
            return false;
        }
        Error = string.Empty;
        return true;
    }
}