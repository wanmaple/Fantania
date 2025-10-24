using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class Preferences : ObservableObject
{
    private static Preferences _singleton = null;
    public static Preferences Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = new Preferences();
            return _singleton;
        }
    }

    public DebugSettings DebugSettings { get; private set; } = new DebugSettings();
    public LevelSettings LevelSettings { get; private set; } = new LevelSettings();

    Preferences()
    { }
}