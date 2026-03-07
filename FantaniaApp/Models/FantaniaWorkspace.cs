using Fantania.Localization;
using FantaniaLib;
using System;
using System.IO;

namespace Fantania.Models;

public class FantaniaWorkspace : Workspace
{
    public FantaniaWorkspace(string rootFolder) : base(rootFolder)
    {
        Languages lang = AppHelper.Settings.Language;
        string langStr = lang switch
        {
            Languages.SimplifiedChinese => TranslationFile.LANGUAGE_ZH_CN,
            Languages.TraditionalChinese => TranslationFile.LANGUAGE_ZH_TW,
            Languages.English => TranslationFile.LANGUAGE_EN_US,
            Languages.Japanese => TranslationFile.LANGUAGE_JA_JP,
            _ => TranslationFile.LANGUAGE_ZH_CN,
        };
        string translationPath = GetAbsolutePath("lang", $"{langStr}.txt");
        if (File.Exists(translationPath))
        {
            try
            {
                _translation = new TranslationFile(translationPath, langStr);
            }
            catch (Exception ex)
            {
                this.LogError(string.Format(LocalizationHelper.GetLocalizedString("ERR_LoadTranslationFailed"), translationPath, ex.Message));
            }
        }
        else
        {
            this.LogWarning(string.Format(LocalizationHelper.GetLocalizedString("WARN_TranslationFileNotFound"), translationPath));
        }
    }

    public override string LocalizeString(string content)
    {
        if (_translation != null)
        {
            string translated = _translation[content];
            if (!string.IsNullOrEmpty(translated))
                return translated;
        }
        if (!LocalizationHelper.TryGetLocalizedString(content, out var result))
        {
            if (!string.IsNullOrEmpty(content))
                this.LogWarning(string.Format(LocalizationHelper.GetLocalizedString("WARN_LocalizationNotFound"), content));
        }
        return result;
    }

    TranslationFile? _translation;
}