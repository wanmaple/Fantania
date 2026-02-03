namespace Fantania.Localization;

public static class LocalizationHelper
{
    public static string GetLocalizedString(string key)
    {
        return Resources.ResourceManager.GetString(key) ?? ("#" + key);
    }

    public static bool TryGetLocalizedString(string key, out string result)
    {
        string? text = Resources.ResourceManager.GetString(key);
        if (text == null)
        {
            result = "#" + key;
            return false;
        }
        result = text;
        return true;
    }
}