using System;
using Avalonia.Controls;

namespace Fantania;

public partial class RandomSeedControl : UserControl
{
    public IEditableProperty EditableProperty => DataContext as IEditableProperty;

    public RandomSeedControl()
    {
        InitializeComponent();
    }

    public void GenerateSeed()
    {
        EditIntegerAttribute editInfo = EditableProperty.EditInfo as EditIntegerAttribute;
        EditableProperty.EditValue = _rd.Next(editInfo.RangeMinimum, editInfo.RangeMaximum);
    }

    Random _rd = new Random();
}