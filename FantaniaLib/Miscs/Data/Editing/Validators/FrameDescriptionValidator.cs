namespace FantaniaLib;

public class FrameDescriptionValidator : FieldValidatorBase
{
    public override bool ValidateField(IWorkspace workspace, object fieldValue)
    {
        string desc = (string)fieldValue;
        if (string.IsNullOrWhiteSpace(desc))
        {
            Error = workspace.LocalizeString("ERR_FrameDescriptionEmpty");
            return false;
        }
        string[] parts = desc.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i].Trim();
            bool hasColon = part.Contains(':');
            bool hasDash = part.Contains('-');
            if (hasColon && hasDash)
            {
                Error = workspace.LocalizeString("ERR_InvalidDescriptionFormat");
                return false;
            }
            if (hasColon)
            {
                string[] subParts = part.Split(':');
                if (subParts.Length != 2)
                {
                    Error = workspace.LocalizeString("ERR_InvalidDescriptionFormat");
                    return false;
                }
                if (!int.TryParse(subParts[0].Trim(), out int frameIndex) || frameIndex < 0)
                {
                    Error = workspace.LocalizeString("ERR_InvalidDescriptionFormat");
                    return false;
                }
                if (!int.TryParse(subParts[1].Trim(), out int duration) || duration <= 0)
                {
                    Error = workspace.LocalizeString("ERR_InvalidDescriptionFormat");
                    return false;
                }
            }
            else if (hasDash)
            {
                string[] subParts = part.Split('-');
                if (subParts.Length != 2)
                {
                    Error = workspace.LocalizeString("ERR_InvalidDescriptionFormat");
                    return false;
                }
                if (!int.TryParse(subParts[0].Trim(), out int startIndex) || startIndex < 0)
                {
                    Error = workspace.LocalizeString("ERR_InvalidDescriptionFormat");
                    return false;
                }
                if (!int.TryParse(subParts[1].Trim(), out int endIndex) || endIndex < 0)
                {
                    Error = workspace.LocalizeString("ERR_InvalidDescriptionFormat");
                    return false;
                }
            }
            else if (!int.TryParse(part, out int singleIndex) || singleIndex < 0)
            {
                Error = workspace.LocalizeString("ERR_InvalidDescriptionFormat");
                return false;
            }
        }
        Error = string.Empty;
        return true;
    }
}