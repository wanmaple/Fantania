using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania.Views;

public partial class StylegroundEditorView : UserControl
{
    public StylegroundEditorViewModel ViewModel => DataContext as StylegroundEditorViewModel;

    public StylegroundEditorView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var workspace = ViewModel.Workspace;
        preview.AddRenderer(new BackgroundsRenderer(workspace.CurrentStylegrounds));
        preview.AddRenderer(new ForegroundsRenderer(workspace.CurrentStylegrounds));
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
    }

    public void AddBackground()
    {
        StylegroundTemplate template = tvPlacements.SelectedItem as StylegroundTemplate;
        ViewModel.Workspace.CurrentStylegrounds.AddBackground(template);
    }

    public void AddForeground()
    {
        StylegroundTemplate template = tvPlacements.SelectedItem as StylegroundTemplate;
        ViewModel.Workspace.CurrentStylegrounds.AddForeground(template);
    }

    void PreviewArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var grid = sender as Grid;
        if (grid.Bounds.Width > 0.0 && grid.Bounds.Height > 0.0)
        {
            preview.Width = grid.Bounds.Width;
            preview.Height = preview.Width * preview.ColorBufferHeight / preview.ColorBufferWidth;
        }
    }

    void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TreeView treeview = sender as TreeView;
        if (treeview.SelectedItem is Styleground styleground)
        {
            ViewModel.SelectedStyleground = styleground;
        }
        else
        {
            ViewModel.SelectedStyleground = null;
        }
    }

    void ButtonAddPlacement_Click(object sender, RoutedEventArgs e)
    {
        IPlacement placement = (sender as Button).DataContext as IPlacement;
        var window = new DatabaseObjectWindow();
        DatabaseObject newObj = Activator.CreateInstance(placement.GroupType) as DatabaseObject;
        newObj._id = WorkspaceViewModel.Current.GenerateID(newObj.Group);
        var vm = new DatabaseObjectViewModel(newObj, DatabaseObjectViewModel.Functionalities.Adding);
        window.DataContext = vm;
        window.Title = $"Adding {placement.GroupType.Name}";
        var topLevel = TopLevel.GetTopLevel(this) as Window;
        window.ShowDialog(topLevel);
    }

    async void Placement_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsMiddleButtonPressed)
        {
            TextBlock tb = sender as TextBlock;
            if (tb.DataContext is DatabaseObject obj)
            {
                if (await MessageBoxHelper.PopupWarningYesNo(this, "MessageConfirmRemoveObject", obj.Name))
                {
                    ViewModel.Workspace.RemoveObject(obj);
                }
            }
        }
    }

    async void Styleground_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsMiddleButtonPressed)
        {
            object item = (sender as TextBlock).DataContext;
            if (item is Styleground sg)
            {
                if (await MessageBoxHelper.PopupMessageYesNo(this, "MessageDeleteStyleground", sg.Name))
                {
                    ViewModel.Workspace.CurrentStylegrounds.RemoveStyleground(sg);
                }
            }
        }
        else if (e.Properties.IsLeftButtonPressed)
        {
            object item = (sender as TextBlock).DataContext;
            if (item is Styleground sg)
            {
                await DragDrop.DoDragDrop(e, sg, DragDropEffects.Move);
            }
        }
    }

    void Styleground_DragEnter(object sender, DragEventArgs e)
    {
        var tb = sender as TextBlock;
        object item = tb.DataContext;
        if (item is Styleground sg && sg != e.Data)
        {
            tb.Background = Brushes.DimGray;
        }
    }

    void Styleground_DragLeave(object sender, DragEventArgs e)
    {
        var tb = sender as TextBlock;
        object item = tb.DataContext;
        if (item is Styleground sg && sg != e.Data)
        {
            tb.Background = Brushes.Transparent;
        }
    }

    void Styleground_Drop(object sender, DragEventArgs e)
    {
        var tb = sender as TextBlock;
        object item = tb.DataContext;
        if (item is Styleground sg && sg != e.Data)
        {
            tb.Background = Brushes.Transparent;
            var sgs = ViewModel.Workspace.CurrentStylegrounds;
            var data = e.Data as Styleground;
            if (sgs.IsBackground(sg))
            {
                if (sgs.IsBackground(data))
                {
                    sgs.MoveBackgroundBefore(data, sg);
                }
                else if (sgs.IsForeground(data))
                {
                    sgs.InsertBackgroundBefore(data, sg);
                    sgs.RemoveForeground(data);
                }
            }
            else if (sgs.IsForeground(sg))
            {
                if (sgs.IsForeground(data))
                {
                    sgs.MoveForegroundBefore(data, sg);
                }
                else if (sgs.IsBackground(data))
                {
                    sgs.InsertForegroundBefore(data, sg);
                    sgs.RemoveBackground(data);
                }
            }
        }
    }
}