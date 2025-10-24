using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania.Views;

public partial class WorkspaceView : UserControl
{
    public static readonly StyledProperty<uint> FPSProperty =
        AvaloniaProperty.Register<MainWindow, uint>(nameof(FPS), defaultValue: 0u);

    public uint FPS
    {
        get => GetValue(FPSProperty);
        set => SetValue(FPSProperty, value);
    }

    public WorkspaceViewModel ViewModel => DataContext as WorkspaceViewModel;

    public WorkspaceView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        _bvhVisual = new BVHVisualDebugger();
        _bvhVisual.IsEnabled = Preferences.Singleton.DebugSettings.IsBVHVisualizerOn;
        Preferences.Singleton.DebugSettings.PropertyChanged += OnDebugSettingsPropertyChanged;
        _stopwatch.Start();
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        topLevel.RequestAnimationFrame(OnTick);
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _stopwatch.Stop();
        Preferences.Singleton.DebugSettings.PropertyChanged -= OnDebugSettingsPropertyChanged;
        base.OnUnloaded(e);
    }

    void OnDebugSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsBVHVisualizerOn")
        {
            bool on = (sender as DebugSettings).IsBVHVisualizerOn;
            _bvhVisual.IsEnabled = on;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.None)
        {
            if (e.Key == Key.Escape)
            {
                tvPlacements.SelectedItem = null;
                var selected = ViewModel.SelectedObjects;
                if (selected.Count == 1 && selected[0] is DatabaseObject)
                {
                    ViewModel.ClearSelections();
                    ViewModel.Workspace.CurrentWorld.CancelAdd();
                }
            }
        }
        base.OnKeyUp(e);
    }

    void ButtonAddPlacement_Click(object sender, RoutedEventArgs e)
    {
        IPlacement placement = (sender as Button).DataContext as IPlacement;
        var window = new DatabaseObjectWindow();
        DatabaseObject newObj = Activator.CreateInstance(placement.GroupType) as DatabaseObject;
        newObj._id = ViewModel.GenerateID(newObj.Group);
        var vm = new DatabaseObjectViewModel(newObj, DatabaseObjectViewModel.Functionalities.Adding);
        window.DataContext = vm;
        window.Title = $"Adding {placement.GroupType.Name}";
        var topLevel = TopLevel.GetTopLevel(this) as Window;
        window.ShowDialog(topLevel);
    }

    void ButtonWorlds_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Workspace.PanelStates.ShowWorldPanel = !ViewModel.Workspace.PanelStates.ShowWorldPanel;
    }

    void ButtonStylegrounds_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Workspace.PanelStates.ShowStylegroundPanel = !ViewModel.Workspace.PanelStates.ShowStylegroundPanel;
    }

    void ButtonLayerVisibility_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Workspace.PanelStates.ShowLayerPanel = !ViewModel.Workspace.PanelStates.ShowLayerPanel;
    }

    void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TreeView treeview = sender as TreeView;
        IPlacement placement = treeview.SelectedItem as IPlacement;
        if (placement is DatabaseObject dbObj)
        {
            ViewModel.ClearSelections();
            ViewModel.AddSelectedObject(dbObj);
        }
        else if (ViewModel.SelectedObjects.Count == 1 && ViewModel.SelectedObjects[0] is DatabaseObject)
        {
            ViewModel.ClearSelections();
        }
    }

    void CheckBoxVisibility_IsCheckedChanged(object sender, RoutedEventArgs e)
    {
        CheckBox cb = sender as CheckBox;
        if (cb.Content == null) return;
        string layerName = cb.Content.ToString();
        RenderLayers layer = Enum.Parse<RenderLayers>(layerName);
        ViewModel.Workspace.CurrentWorld.SetLayerVisible(layer, cb.IsChecked.Value);
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

    void OnTick(TimeSpan dt)
    {
        _stopwatch.Stop();
        int elapsed = Math.Max((int)_stopwatch.ElapsedMilliseconds, 1);
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            FPS = Math.Min(60u, (uint)(1000.0f / elapsed));
        });
        _stopwatch.Restart();
        if (ViewModel != null)
            ViewModel.FrameCount++;
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
            topLevel.RequestAnimationFrame(OnTick);
    }

    Stopwatch _stopwatch = new Stopwatch();
    BVHVisualDebugger _bvhVisual;
}