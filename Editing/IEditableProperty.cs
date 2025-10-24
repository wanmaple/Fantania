using System;
using System.Reflection;

namespace Fantania;

public interface IEditableProperty
{
    event Action<object> ValueChanged;

    string PropertyName { get; }
    object Instance { get; }
    PropertyInfo PropertyInfo { get; }
    EditAttribute EditInfo { get; }
    Type EditType { get; }
    IEditableValidator Validator { get; }
    string Tooltip { get; }
    object EditValue { get; set; }
    string Error { get; set; }

    void RegisterPropertyChanged(Action<object> handler);
    void UnRegisterPropertyChanged(Action<object> handler);
}