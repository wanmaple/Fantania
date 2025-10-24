using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Fantania.Models;

namespace Fantania.Views;

public partial class LevelPanel : UserControl
{
    private class DragData : IDataObject
    {
        public string LevelName { get; set; } = string.Empty;

        public bool Contains(string dataFormat)
        {
            return dataFormat == FORMAT;
        }

        public object? Get(string dataFormat)
        {
            if (dataFormat == FORMAT)
            {
                return LevelName;
            }
            return null;
        }

        public IEnumerable<string> GetDataFormats()
        {
            return [FORMAT,];
        }

        const string FORMAT = "dragdata.lvname";
    }

    public Workspace Workspace => DataContext as Workspace;

    public LevelPanel()
    {
        InitializeComponent();
    }

    public async Task CreateLevel()
    {
        Workspace workspace = Workspace;
        workspace.PanelStates.HideAll();
        var desktop = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        bool creating = true;
        if (workspace.IsModified)
        {
            await MessageBoxHelper.PopupWarningYesNoCancel(desktop.MainWindow, "WarningConfirmSavingOnExit").ContinueWith(async task =>
            {
                if (task.Result == ButtonResults.Yes)
                {
                    await workspace.Save();
                }
                else if (task.Result == ButtonResults.Cancel)
                {
                    creating = false;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        if (creating)
        {
            var newLvView = new NewLevelView();
            newLvView.DataContext = workspace;
            await newLvView.ShowDialog<bool>(desktop.MainWindow);
            DataContext = null;
            DataContext = workspace;
        }
    }

    async void ButtonSwitch_Click(object sender, RoutedEventArgs e)
    {
        var lvName = (sender as Button).DataContext as string;
        var workspace = Workspace;
        var desktop = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        bool switching = true;
        if (workspace.IsModified)
        {
            await MessageBoxHelper.PopupWarningYesNoCancel(desktop.MainWindow, "WarningConfirmSavingOnExit").ContinueWith(async task =>
            {
                if (task.Result == ButtonResults.Yes)
                {
                    await workspace.Save();
                }
                else if (task.Result == ButtonResults.Cancel)
                {
                    switching = false;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        if (switching)
        {
            await workspace.SwitchLevelAsync(lvName);
            DataContext = null;
            DataContext = workspace;
        }
    }

    async void Cell_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsMiddleButtonPressed)
        {
            string lvName = (sender as Grid).DataContext.ToString();
            var workspace = Workspace;
            var desktop = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (lvName != workspace.CurrentLevel.Name)
            {
                if (await MessageBoxHelper.PopupWarningYesNo(desktop.MainWindow, "WarningDeleteLevel", lvName))
                {
                    await workspace.DeleteLevel(lvName);
                    DataContext = null;
                    DataContext = workspace;
                }
            }
        }
        else if (e.Properties.IsLeftButtonPressed)
        {
            string lvName = (sender as Grid).DataContext.ToString();
            var data = new DragData { LevelName = lvName, };
            await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
        }
    }

    void OnDragEnter(object? sender, DragEventArgs e)
    {
        (sender as TextBlock).Background = Brushes.DimGray;
    }

    void OnDragLeave(object? sender, DragEventArgs e)
    {
        (sender as TextBlock).Background = Brushes.Transparent;
    }

    async void OnDrop(object? sender, DragEventArgs e)
    {
        var workspace = Workspace;
        string lvName = (e.Data as DragData).LevelName;
        var tb = sender as TextBlock;
        LevelGroup group = tb.DataContext as LevelGroup;
        string oldGroupName = workspace.LevelGrouping.GetGroupName(lvName);
        string newGroupName = group.GroupName;
        if (oldGroupName != newGroupName)
        {
            if (lvName == Workspace.CurrentLevel.Name)
            {
                workspace.CurrentLevel.Group = newGroupName;
            }
            await workspace.ChangeLevelGroup(lvName, oldGroupName, newGroupName);
            DataContext = null;
            DataContext = workspace;
        }
        tb.Background = Brushes.Transparent;
    }
}