namespace Fantania.Localization;

public static class LocalizationHelper
{
    public static string GetLocalizedString(string key)
    {
        return Resources.ResourceManager.GetString(key) ?? ("#" + key);
    }
}