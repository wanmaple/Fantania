using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Fantania.Localization;
using FantaniaLib;

namespace Fantania.Views;

public partial class CommonEditObjectView : Window
{
    public static readonly StyledProperty<bool> ShowButtonsProperty = AvaloniaProperty.Register<CommonEditObjectView, bool>(nameof(ShowButtons), defaultValue: true);
    public bool ShowButtons
    {
        get => GetValue(ShowButtonsProperty);
        set => SetValue(ShowButtonsProperty, value);
    }

    public CommonEditObjectView()
    {
        InitializeComponent();
    }

    public async Task Confirm()
    {
        var groups = oevObj.EditableFields!;
        foreach (var group in groups)
        {
            EditableFields fields = group.Fields;
            foreach (var field in fields.Fields)
            {
                if (!string.IsNullOrEmpty(field.FieldValidator!.Error))
                {
                    await MessageBoxHelper.PopupErrorOkay(this, LocalizationHelper.GetLocalizedString("ERR_InvalidFieldValue"), field.FieldName);
                    return;
                }
            }
        }
        Close(true);
    }

    public void Cancel()
    {
        Close(false);
    }
}