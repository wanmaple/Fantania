using Avalonia;

namespace FantaniaLib;

public static class RectExtensions
{
    public static Rect ToAvaRect(this Rectf self)
    {
        return new Rect(self.X, self.Y, self.Width, self.Height);
    }
}