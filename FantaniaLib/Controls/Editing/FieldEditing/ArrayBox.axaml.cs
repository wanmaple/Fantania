using Avalonia.Controls;

namespace FantaniaLib;

public partial class ArrayBox : UserControl
{
    public IEditableField? Field => DataContext as IEditableField;

    public ArrayBox()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (Field != null)
        {
        }
    }
}