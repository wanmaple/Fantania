using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FantaniaLib;

public partial class RandomSeedBox : UserControl
{
    public IEditableField? Field => DataContext as IEditableField;

    public RandomSeedBox()
    {
        InitializeComponent();
    }

    public void RandomizeSeed()
    {
        var random = new Random();
        Field!.FieldValue = random.Next(int.MinValue, int.MaxValue);
    }
}