using Avalonia.Input;

namespace Fantania;

public static class KeyModifiersExtensions
{
    public static bool IsModifierPressed(this KeyModifiers self, KeyModifiers modifier)
    {
        return (int)(self & modifier) != 0;
    }
}