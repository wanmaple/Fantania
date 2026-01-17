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
        return LocalizationHelper.GetLocalizedString(content);
    }
}