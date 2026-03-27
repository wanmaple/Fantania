using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Fantania.ViewModels;
using FantaniaLib;

namespace Fantania.Views;

public class DataGroup2TemplatesConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not string group) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        return workspace.DatabaseModule.GetGameDataTemplatesOfGroup(group);
    }
}

public class DataGroup2GameDataListConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not string group) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        var objs = workspace.DatabaseModule.GetObjectsOfGroup(group).Skip(1);
        return new FilterableBindingSource<DatabaseObject>((IList<DatabaseObject>)objs);
    }
}

public partial class EditGameDataView : Window
{
    public EditGameDataViewModel ViewModel => (EditGameDataViewModel)DataContext!;

    public static readonly StyledProperty<bool> NotifyChangeProperty = AvaloniaProperty.Register<EditGameDataView, bool>(nameof(NotifyChange), defaultValue: false);
    public bool NotifyChange
    {
        get => GetValue(NotifyChangeProperty);
        set => SetValue(NotifyChangeProperty, value);
    }

    public EditGameDataView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        lbGamedata.PropertyChanged += OnGameDataItemsSourceChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        lbGamedata.PropertyChanged -= OnGameDataItemsSourceChanged;
        base.OnUnloaded(e);
    }

    void OnGameDataItemsSourceChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ItemsControl.ItemsSourceProperty)
        {
            FilterableBindingSource<DatabaseObject> source = (FilterableBindingSource<DatabaseObject>)e.NewValue!;
            source.Filter(FilterGameData, null);
        }
    }

    void FilterGroupBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        string? selected = (string?)lbGroups.SelectedItem;
        string filterContent = txtFilterGroup.Text == null ? string.Empty : txtFilterGroup.Text.Trim();
        ViewModel!.FilterGroups(g => string.IsNullOrEmpty(filterContent) || g.Contains(filterContent, System.StringComparison.OrdinalIgnoreCase));
        if (selected != null && !string.IsNullOrEmpty(filterContent) && !selected.Contains(filterContent, System.StringComparison.OrdinalIgnoreCase))
            lbGroups.SelectedItem = null;
        else
            lbGroups.SelectedItem = selected;
    }

    void FilterNameBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UserGameData? selected = (UserGameData?)lbGamedata.SelectedItem;
        if (lbGamedata.ItemsSource is FilterableBindingSource<DatabaseObject> source)
        {
            source.Filter(FilterGameData, null);
        }
        if (selected != null && !string.IsNullOrEmpty(txtFilterName.Text) && !selected.Name.Contains(txtFilterName.Text, StringComparison.OrdinalIgnoreCase))
            lbGamedata.SelectedItem = null;
        else
            lbGamedata.SelectedItem = selected;
    }

    void ButtonAddGameData_Click(object? sender, RoutedEventArgs e)
    {
        GameDataTemplate template = (GameDataTemplate)((Button)sender!).DataContext!;
        IWorkspace workspace = ViewModel.Workspace;
        var newData = workspace.DatabaseModule.AddUserGameData(template.TypeName);
        newData.Name = $"{template.TypeName}_{newData.ID}";
        NotifyChange = !NotifyChange;
        lbGamedata.SelectedItem = newData;
    }

    bool FilterGameData(DatabaseObject obj)
    {
        string filterContent = txtFilterName.Text == null ? string.Empty : txtFilterName.Text.Trim();
        return string.IsNullOrEmpty(filterContent) || obj.Name.Contains(filterContent, StringComparison.OrdinalIgnoreCase);
    }
}