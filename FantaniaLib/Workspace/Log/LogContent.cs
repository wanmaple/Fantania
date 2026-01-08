using Avalonia.Media;

namespace FantaniaLib;

public class LogContent : SyncableObject
{
    public string Content { get; set; } = string.Empty;
    public FontWeight FontWeight { get; set; } = FontWeight.Normal;
    public FontStyle FontStyle { get; set; } = FontStyle.Normal;
    public int FontSize { get; set; } = 14;
    public IBrush Color { get; set; } = Brushes.White;
}