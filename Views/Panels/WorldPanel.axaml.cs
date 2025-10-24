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

public partial class WorldPanel : UserControl
{
    private class DragData : IDataObject
    {
        public string WorldName { get; set; } = string.Empty;

        public bool Contains(string dataFormat)
        {
            return dataFormat == FORMAT;
        }

        public object? Get(string dataFormat)
        {
            if (dataFormat == FORMAT)
            {
                return WorldName;
            }
            return null;
        }

        public IEnumerable<string> GetDataFormats()
        {
            return [FORMAT,];
        }

        const string FORMAT = "dragdata.worldname";
    }

    public Workspace Workspace => DataContext as Workspace;

    public WorldPanel()
    {
        InitializeComponent();
    }

    public async Task CreateWorld()
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
            var newWorldView = new NewWorldView();
            newWorldView.DataContext = workspace;
            await newWorldView.ShowDialog<bool>(desktop.MainWindow);
            DataContext = null;
            DataContext = workspace;
        }
    }

    async void ButtonSwitch_Click(object sender, RoutedEventArgs e)
    {
        var worldName = (sender as Button).DataContext as string;
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
            await workspace.SwitchWorldAsync(worldName);
            DataContext = null;
            DataContext = workspace;
        }
    }

    async void Cell_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsMiddleButtonPressed)
        {
            string worldName = (sender as Grid).DataContext.ToString();
            var workspace = Workspace;
            var desktop = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (worldName != workspace.CurrentWorld.Name)
            {
                if (await MessageBoxHelper.PopupWarningYesNo(desktop.MainWindow, "WarningDeleteWorld", worldName))
                {
                    await workspace.DeleteWorld(worldName);
                    DataContext = null;
                    DataContext = workspace;
                }
            }
        }
        else if (e.Properties.IsLeftButtonPressed)
        {
            string worldName = (sender as Grid).DataContext.ToString();
            var data = new DragData { WorldName = worldName, };
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
        string worldName = (e.Data as DragData).WorldName;
        var tb = sender as TextBlock;
        WorldGroup group = tb.DataContext as WorldGroup;
        string oldGroupName = workspace.WorldGrouping.GetGroupName(worldName);
        string newGroupName = group.GroupName;
        if (oldGroupName != newGroupName)
        {
            if (worldName == Workspace.CurrentWorld.Name)
            {
                workspace.CurrentWorld.Group = newGroupName;
            }
            await workspace.ChangeWorldGroup(worldName, oldGroupName, newGroupName);
            DataContext = null;
            DataContext = workspace;
        }
        tb.Background = Brushes.Transparent;
    }
}