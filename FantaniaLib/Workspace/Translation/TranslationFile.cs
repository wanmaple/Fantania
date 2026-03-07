namespace FantaniaLib;

public class TranslationFile
{
    public const string LANGUAGE_ZH_CN = "zh-CN";
    public const string LANGUAGE_ZH_TW = "zh-TW";
    public const string LANGUAGE_EN_US = "en-US";
    public const string LANGUAGE_JA_JP = "ja-JP";

    public string this[string key]
    {
        get
        {
            if (_translations.TryGetValue(key, out var value))
            {
                return value;
            }
            return string.Empty;
        }
    }

    public string Language { get; set; } = LANGUAGE_ZH_CN;

    public TranslationFile(string sourcePath, string language)
    {
        Language = language;
        using (var fs = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
        {
            using (var sr = new StreamReader(fs))
            {
                string? line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    {
                        continue;
                    }
                    var splits = line.Split(new char[] { '=' }, 2);
                    if (splits.Length != 2)
                    {
                        continue;
                    }
                    var key = splits[0].Trim();
                    var value = splits[1].Trim();
                    if (!string.IsNullOrEmpty(key))
                        _translations[key] = value;
                }
            }
        }
    }

    public static bool IsSupportLanguage(string language)
    {
        return SupportLanguages.Contains(language);
    }

    Dictionary<string, string> _translations = new Dictionary<string, string>(256);

    static readonly string[] SupportLanguages = new string[]
    {
        LANGUAGE_ZH_CN,
        LANGUAGE_ZH_TW,
        LANGUAGE_EN_US,
        LANGUAGE_JA_JP
    };
}
