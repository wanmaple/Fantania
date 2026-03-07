using FantaniaLib;

namespace Fantania;

public class AppSettings : SyncableObject
{
    private Languages _lang = Languages.SimplifiedChinese;
    public Languages Language
    {
        get => _lang;
        set
        {
            if (_lang != value)
            {
                _lang = value;
                OnPropertyChanged(nameof(Language));
            }
        }
    }
}
