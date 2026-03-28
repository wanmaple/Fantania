namespace FantaniaLib;

public class AtlasValidator : FieldValidatorBase
{
    public override bool ValidateField(IWorkspace workspace, object fieldValue)
    {
        if (fieldValue is FantaniaArray<string> arr)
        {
            foreach (string atlasPath in arr)
            {
                if (!ValidateAtlas(workspace, atlasPath))
                    return false;
            }
        }
        else if (!ValidateAtlas(workspace, (string)fieldValue))
        {
            return false;
        }
        return true;
    }

    bool ValidateAtlas(IWorkspace workspace, string atlasPath)
    {
        try
        {
            if (!atlasPath.StartsWith("avares://"))
                atlasPath = workspace.GetAbsolutePath(atlasPath);
            SpriteAtlas atlas = new SpriteAtlas(atlasPath);
            if (atlas.IsValid)
            {
                Error = string.Empty;
                return true;
            }
            else
            {
                Error = workspace.LocalizeString("ERR_InvalidAtlas");
                return false;
            }
        }
        catch (Exception)
        {
            Error = workspace.LocalizeString("ERR_InvalidAtlas");
            return false;
        }
    }
}