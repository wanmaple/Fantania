using Avalonia;
using Avalonia.Controls;

namespace FantaniaLib;

public partial class CustomOptionsBox : UserControl
{
    public IEditableField? Field => DataContext as IEditableField;

    public static readonly StyledProperty<IEnumerable<Option>?> OptionsProperty = AvaloniaProperty.Register<CustomOptionsBox, IEnumerable<Option>?>(nameof(Options), defaultValue: null);
    public IEnumerable<Option>? Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }
    
    public CustomOptionsBox()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (Field != null)
        {
            string args = Field.EditInfo.EditParameter;
            string[] vars = args.Split(',');
            List<Option> options = new List<Option>();
            for (int i = 0; i < vars.Length; i++)
            {
                string[] pair = vars[i].Split(':');
                int value = int.Parse(pair[0]);
                string display = pair[1];
                options.Add(new Option
                {
                    Name = display,
                    Value = value,
                });
            }
            Options = options;
        }
    }
}