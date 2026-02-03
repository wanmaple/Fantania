using Fantania.Localization;
using FantaniaLib;

namespace Fantania.Models;

public class FantaniaWorkspace : Workspace
{
    public FantaniaWorkspace(string rootFolder) : base(rootFolder)
    {
    }

    public override string LocalizeString(string content)
    {
        if (!LocalizationHelper.TryGetLocalizedString(content, out var result))
        {
            if (!string.IsNullOrEmpty(content))
                this.LogWarning(string.Format(LocalizationHelper.GetLocalizedString("WARN_LocalizationNotFound"), content));
        }
        return result;
    }
}