using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FantaniaLib;

public partial class Direction3DBox : UserControl
{
    IEditableField? Field => DataContext as IEditableField;

    public Direction3DBox()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        topLevel.RequestAnimationFrame(OnTick);
    }

    void OnTick(TimeSpan dt)
    {
        if (Field != null)
        {
            float az = Convert.ToSingle(slAzimuth.Value);
            float ele = Convert.ToSingle(slElevation.Value);
            Direction3D dir = new Direction3D(az, ele);
            Field.FieldValue = dir;
        }
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }
}